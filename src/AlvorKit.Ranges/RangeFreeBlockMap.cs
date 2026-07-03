namespace AlvorKit.Ranges;

/// <summary>Indexes free range blocks by address and size for best-fit allocation and coalescing.</summary>
internal sealed class RangeFreeBlockMap
{
    private RangeFreeBlockNode[] nodes = new RangeFreeBlockNode[16];
    private int nextNode = 1;
    private int freeNodeHead;
    private int freeNodeCount;
    private int resetFreeNode;
    private int resetFreeNodeLimit;
    private int addressRoot;
    private int sizeRoot;
    private int blockCount;
    private int distinctSizeCount;

    /// <summary>Creates a free block map with one initial block.</summary>
    internal RangeFreeBlockMap(long index, long size) => Add(index, size);

    /// <summary>Gets the number of free blocks indexed by offset.</summary>
    internal int BlockCount => blockCount;

    /// <summary>Gets the number of distinct free block sizes.</summary>
    internal int SizeCount => distinctSizeCount;

    /// <summary>Gets the number of reusable free block node slots.</summary>
    internal int PooledSetCount => freeNodeCount;

    /// <summary>Adds a free block at <paramref name="index"/> with <paramref name="size"/> bytes.</summary>
    internal void Add(long index, long size)
    {
        var node = RentNode(index, size);
        InsertAddress(node);
        if (InsertSize(node))
            distinctSizeCount++;

        blockCount++;
    }

    /// <summary>Extends the final free block when adjacent, otherwise adds a new tail block.</summary>
    internal void Extend(long oldSize, long newSize)
    {
        var diff = newSize - oldSize;
        if (blockCount == 1)
        {
            var node = addressRoot;
            if (nodes[node].Index + nodes[node].Size == oldSize)
            {
                ReplaceSoleBlock(node, nodes[node].Index, nodes[node].Size + diff);
                return;
            }
        }

        var leftNode = FindAddressPredecessor(newSize);
        if (leftNode != 0 && nodes[leftNode].Index + nodes[leftNode].Size == oldSize)
        {
            var index = nodes[leftNode].Index;
            var size = nodes[leftNode].Size + diff;
            RemoveNode(leftNode);
            Add(index, size);
        }
        else
            Add(oldSize, diff);
    }

    /// <summary>Merges a newly freed range with adjacent free blocks.</summary>
    internal void Merge(long index, long size)
    {
        if (blockCount == 1)
        {
            var node = addressRoot;
            var blockIndex = nodes[node].Index;
            var blockSize = nodes[node].Size;
            if (blockIndex + blockSize == index)
            {
                ReplaceSoleBlock(node, blockIndex, blockSize + size);
                return;
            }

            if (index + size == blockIndex)
            {
                ReplaceSoleBlock(node, index, blockSize + size);
                return;
            }
        }

        var leftNode = FindAddressPredecessor(index);
        if (leftNode != 0 && nodes[leftNode].Index + nodes[leftNode].Size == index)
        {
            index = nodes[leftNode].Index;
            size += nodes[leftNode].Size;
            RemoveNode(leftNode);
        }

        var rightNode = FindAddressSuccessor(index);
        if (rightNode != 0 && nodes[rightNode].Index == index + size)
        {
            size += nodes[rightNode].Size;
            RemoveNode(rightNode);
        }

        Add(index, size);
    }

    /// <summary>Replaces every free block with one block, usually after compaction.</summary>
    internal void Reset(long index, long size)
    {
        addressRoot = 0;
        sizeRoot = 0;
        blockCount = 0;
        distinctSizeCount = 0;
        freeNodeHead = 0;
        resetFreeNode = 1;
        resetFreeNodeLimit = nextNode;
        freeNodeCount = resetFreeNodeLimit - resetFreeNode;

        Add(index, size);
    }

    /// <summary>Removes and returns the smallest free block that can satisfy the requested reserved size.</summary>
    internal bool TryTakeBestFit(long requiredSize, out long index)
    {
        if (blockCount == 1)
            return TryTakeSoleBlock(requiredSize, out index);

        var node = FindFirstByMinSize(requiredSize);
        if (node == 0)
        {
            index = 0;
            return false;
        }

        index = nodes[node].Index;
        var size = nodes[node].Size;
        RemoveNode(node);
        AddRemainder(index, size, requiredSize);
        return true;
    }

    /// <summary>Consumes part or all of the sole free block without tree remove/insert work.</summary>
    private bool TryTakeSoleBlock(long requiredSize, out long index)
    {
        var node = addressRoot;
        var size = nodes[node].Size;
        if (size < requiredSize)
        {
            index = 0;
            return false;
        }

        index = nodes[node].Index;
        var newSize = size - requiredSize;
        if (newSize == 0)
        {
            addressRoot = 0;
            sizeRoot = 0;
            blockCount = 0;
            distinctSizeCount = 0;
            ReturnNode(node);
        }
        else
            ReplaceSoleBlock(node, index + requiredSize, newSize);

        return true;
    }

    /// <summary>Adds the free-block remainder left after consuming from a known block.</summary>
    private void AddRemainder(long index, long size, long reservedSize)
    {
        var newSize = size - reservedSize;
        if (newSize > 0)
            Add(index + reservedSize, newSize);
    }

    /// <summary>Updates the only indexed free block in place.</summary>
    private void ReplaceSoleBlock(int node, long index, long size)
    {
        nodes[node].Index = index;
        nodes[node].Size = size;
        nodes[node].Priority = Priority(index, size);
    }

    /// <summary>Finds the closest free block whose start index is strictly smaller than <paramref name="index"/>.</summary>
    private int FindAddressPredecessor(long index)
    {
        var current = addressRoot;
        var result = 0;
        while (current != 0)
        {
            if (nodes[current].Index < index)
            {
                result = current;
                current = nodes[current].AddressRight;
            }
            else
                current = nodes[current].AddressLeft;
        }

        return result;
    }

    /// <summary>Finds the closest free block whose start index is strictly larger than <paramref name="index"/>.</summary>
    private int FindAddressSuccessor(long index)
    {
        var current = addressRoot;
        var result = 0;
        while (current != 0)
        {
            if (nodes[current].Index > index)
            {
                result = current;
                current = nodes[current].AddressLeft;
            }
            else
                current = nodes[current].AddressRight;
        }

        return result;
    }

    /// <summary>Finds the smallest free block whose size is at least <paramref name="size"/>.</summary>
    private int FindFirstByMinSize(long size)
    {
        var current = sizeRoot;
        var result = 0;
        while (current != 0)
        {
            if (nodes[current].Size >= size)
            {
                result = current;
                current = nodes[current].SizeLeft;
            }
            else
                current = nodes[current].SizeRight;
        }

        return result;
    }

    /// <summary>Inserts <paramref name="node"/> into the address-ordered treap.</summary>
    private void InsertAddress(int node)
    {
        nodes[node].AddressLeft = 0;
        nodes[node].AddressRight = 0;
        nodes[node].AddressParent = 0;
        if (addressRoot == 0)
        {
            addressRoot = node;
            return;
        }

        var current = addressRoot;
        while (true)
        {
            if (nodes[node].Index < nodes[current].Index)
            {
                if (nodes[current].AddressLeft == 0)
                {
                    nodes[current].AddressLeft = node;
                    break;
                }

                current = nodes[current].AddressLeft;
            }
            else
            {
                if (nodes[current].AddressRight == 0)
                {
                    nodes[current].AddressRight = node;
                    break;
                }

                current = nodes[current].AddressRight;
            }
        }

        nodes[node].AddressParent = current;
        while (nodes[node].AddressParent != 0 && nodes[node].Priority < nodes[nodes[node].AddressParent].Priority)
            RotateAddressUp(node);
    }

    /// <summary>Inserts <paramref name="node"/> into the size-ordered treap and returns true for a new distinct size.</summary>
    private bool InsertSize(int node)
    {
        nodes[node].SizeLeft = 0;
        nodes[node].SizeRight = 0;
        nodes[node].SizeParent = 0;
        if (sizeRoot == 0)
        {
            sizeRoot = node;
            return true;
        }

        var current = sizeRoot;
        var isNewSize = true;
        while (true)
        {
            if (nodes[current].Size == nodes[node].Size)
                isNewSize = false;

            if (CompareSize(node, current) < 0)
            {
                if (nodes[current].SizeLeft == 0)
                {
                    nodes[current].SizeLeft = node;
                    break;
                }

                current = nodes[current].SizeLeft;
            }
            else
            {
                if (nodes[current].SizeRight == 0)
                {
                    nodes[current].SizeRight = node;
                    break;
                }

                current = nodes[current].SizeRight;
            }
        }

        nodes[node].SizeParent = current;
        while (nodes[node].SizeParent != 0 && nodes[node].Priority < nodes[nodes[node].SizeParent].Priority)
            RotateSizeUp(node);

        return isNewSize;
    }

    /// <summary>Removes a known free block from both indexes and returns its node slot to the pool.</summary>
    private void RemoveNode(int node)
    {
        if (!HasSameSizeNeighbor(node))
            distinctSizeCount--;

        RemoveAddress(node);
        RemoveSize(node);
        ReturnNode(node);
        blockCount--;
    }

    /// <summary>Removes <paramref name="node"/> from the address treap.</summary>
    private void RemoveAddress(int node)
    {
        while (nodes[node].AddressLeft != 0 || nodes[node].AddressRight != 0)
        {
            var left = nodes[node].AddressLeft;
            var right = nodes[node].AddressRight;
            if (right == 0 || left != 0 && nodes[left].Priority < nodes[right].Priority)
                RotateAddressUp(left);
            else
                RotateAddressUp(right);
        }

        var parent = nodes[node].AddressParent;
        if (parent == 0)
            addressRoot = 0;
        else if (nodes[parent].AddressLeft == node)
            nodes[parent].AddressLeft = 0;
        else
            nodes[parent].AddressRight = 0;

        nodes[node].AddressParent = 0;
    }

    /// <summary>Removes <paramref name="node"/> from the size treap.</summary>
    private void RemoveSize(int node)
    {
        while (nodes[node].SizeLeft != 0 || nodes[node].SizeRight != 0)
        {
            var left = nodes[node].SizeLeft;
            var right = nodes[node].SizeRight;
            if (right == 0 || left != 0 && nodes[left].Priority < nodes[right].Priority)
                RotateSizeUp(left);
            else
                RotateSizeUp(right);
        }

        var parent = nodes[node].SizeParent;
        if (parent == 0)
            sizeRoot = 0;
        else if (nodes[parent].SizeLeft == node)
            nodes[parent].SizeLeft = 0;
        else
            nodes[parent].SizeRight = 0;

        nodes[node].SizeParent = 0;
    }

    /// <summary>Returns true when another size-tree neighbor has the same size.</summary>
    private bool HasSameSizeNeighbor(int node)
    {
        var predecessor = SizePredecessor(node);
        if (predecessor != 0 && nodes[predecessor].Size == nodes[node].Size)
            return true;

        var successor = SizeSuccessor(node);
        return successor != 0 && nodes[successor].Size == nodes[node].Size;
    }

    /// <summary>Returns the previous node in size-index order.</summary>
    private int SizePredecessor(int node)
    {
        if (nodes[node].SizeLeft != 0)
            return SizeMaximum(nodes[node].SizeLeft);

        var current = node;
        var parent = nodes[current].SizeParent;
        while (parent != 0 && nodes[parent].SizeLeft == current)
        {
            current = parent;
            parent = nodes[current].SizeParent;
        }

        return parent;
    }

    /// <summary>Returns the next node in size-index order.</summary>
    private int SizeSuccessor(int node)
    {
        if (nodes[node].SizeRight != 0)
            return SizeMinimum(nodes[node].SizeRight);

        var current = node;
        var parent = nodes[current].SizeParent;
        while (parent != 0 && nodes[parent].SizeRight == current)
        {
            current = parent;
            parent = nodes[current].SizeParent;
        }

        return parent;
    }

    /// <summary>Returns the minimum node under <paramref name="node"/> in size-index order.</summary>
    private int SizeMinimum(int node)
    {
        while (nodes[node].SizeLeft != 0)
            node = nodes[node].SizeLeft;

        return node;
    }

    /// <summary>Returns the maximum node under <paramref name="node"/> in size-index order.</summary>
    private int SizeMaximum(int node)
    {
        while (nodes[node].SizeRight != 0)
            node = nodes[node].SizeRight;

        return node;
    }

    /// <summary>Rotates an address-tree node above its parent.</summary>
    private void RotateAddressUp(int node)
    {
        var parent = nodes[node].AddressParent;
        var grandparent = nodes[parent].AddressParent;
        if (nodes[parent].AddressLeft == node)
        {
            nodes[parent].AddressLeft = nodes[node].AddressRight;
            if (nodes[node].AddressRight != 0)
                nodes[nodes[node].AddressRight].AddressParent = parent;

            nodes[node].AddressRight = parent;
        }
        else
        {
            nodes[parent].AddressRight = nodes[node].AddressLeft;
            if (nodes[node].AddressLeft != 0)
                nodes[nodes[node].AddressLeft].AddressParent = parent;

            nodes[node].AddressLeft = parent;
        }

        nodes[parent].AddressParent = node;
        nodes[node].AddressParent = grandparent;
        ReplaceAddressChild(grandparent, parent, node);
    }

    /// <summary>Rotates a size-tree node above its parent.</summary>
    private void RotateSizeUp(int node)
    {
        var parent = nodes[node].SizeParent;
        var grandparent = nodes[parent].SizeParent;
        if (nodes[parent].SizeLeft == node)
        {
            nodes[parent].SizeLeft = nodes[node].SizeRight;
            if (nodes[node].SizeRight != 0)
                nodes[nodes[node].SizeRight].SizeParent = parent;

            nodes[node].SizeRight = parent;
        }
        else
        {
            nodes[parent].SizeRight = nodes[node].SizeLeft;
            if (nodes[node].SizeLeft != 0)
                nodes[nodes[node].SizeLeft].SizeParent = parent;

            nodes[node].SizeLeft = parent;
        }

        nodes[parent].SizeParent = node;
        nodes[node].SizeParent = grandparent;
        ReplaceSizeChild(grandparent, parent, node);
    }

    /// <summary>Replaces an address-tree child link after a rotation.</summary>
    private void ReplaceAddressChild(int parent, int oldChild, int newChild)
    {
        if (parent == 0)
            addressRoot = newChild;
        else if (nodes[parent].AddressLeft == oldChild)
            nodes[parent].AddressLeft = newChild;
        else
            nodes[parent].AddressRight = newChild;
    }

    /// <summary>Replaces a size-tree child link after a rotation.</summary>
    private void ReplaceSizeChild(int parent, int oldChild, int newChild)
    {
        if (parent == 0)
            sizeRoot = newChild;
        else if (nodes[parent].SizeLeft == oldChild)
            nodes[parent].SizeLeft = newChild;
        else
            nodes[parent].SizeRight = newChild;
    }

    /// <summary>Compares two nodes by size and then index.</summary>
    private int CompareSize(int left, int right)
    {
        var sizeCompare = nodes[left].Size.CompareTo(nodes[right].Size);
        return sizeCompare != 0 ? sizeCompare : nodes[left].Index.CompareTo(nodes[right].Index);
    }

    /// <summary>Rents a node slot for a new free block.</summary>
    private int RentNode(long index, long size)
    {
        int node;
        if (freeNodeHead != 0)
        {
            node = freeNodeHead;
            freeNodeHead = nodes[node].NextFree;
            freeNodeCount--;
        }
        else if (resetFreeNode < resetFreeNodeLimit)
        {
            node = resetFreeNode++;
            freeNodeCount--;
        }
        else
        {
            if (nextNode == nodes.Length)
                Array.Resize(ref nodes, nodes.Length * 2);

            node = nextNode++;
        }

        nodes[node] = new()
        {
            Index = index,
            Size = size,
            Priority = Priority(index, size),
        };
        return node;
    }

    /// <summary>Returns a node slot to the reusable pool.</summary>
    private void ReturnNode(int node)
    {
        nodes[node] = new() { NextFree = freeNodeHead };
        freeNodeHead = node;
        freeNodeCount++;
    }

    /// <summary>Returns a deterministic pseudo-random treap priority for a free block.</summary>
    private static uint Priority(long index, long size)
    {
        var value = (ulong)index * 0x9E3779B185EBCA87UL ^ (ulong)size * 0xC2B2AE3D27D4EB4FUL;
        value ^= value >> 33;
        value *= 0xFF51AFD7ED558CCDUL;
        value ^= value >> 33;
        return (uint)value;
    }
}
