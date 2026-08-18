# Shared Vertex Buffer Copy Investigation

## Status

Investigation deferred. `SharedVertexBuffer` remains unchanged while current
consumers continue using its existing allocation, packing, and resizing
behavior.

Shroom's ASCII renderer will use the existing allocator with a maximum cache
size of 256 MiB. Redesigning or fixing the allocator is not part of the ASCII
renderer work.

## Problem

Commit [`f3050d0`](https://github.com/samuelmasse/AlvorKit/commit/f3050d0ec214fd01743a033e2a1ce28a1ae29d72)
replaced `SharedVertexBuffer`'s GPU-to-GPU scratch-buffer copies with CPU
snapshots while fixing macOS support.

The current implementation applies that workaround on every platform:

- `PackCallback` allocates native memory equal to the full VBO capacity,
  downloads the entire VBO with `GetBufferSubData`, and uploads every live
  allocation at its packed address.
- `ResizeCallback` allocates native memory equal to the old VBO capacity,
  downloads the entire VBO, reallocates its storage, and uploads the snapshot.
- Both callbacks synchronously allocate and free native memory and force
  GPU-to-CPU-to-GPU transfers.

This becomes disproportionately expensive as buffers grow and also affects
Windows and Linux even if macOS requires special handling.

The implementation is in
[`SharedVertexBuffer.cs`](https://github.com/samuelmasse/AlvorKit/blob/main/src/AlvorKit.Engine/Buffers/SharedVertexBuffer.cs).

## macOS uncertainty

The previous implementation used distinct buffers bound to
`GL_COPY_READ_BUFFER` and `GL_COPY_WRITE_BUFFER`. That operation is part of
OpenGL 3.1 and is valid for distinct source and destination buffers.

Apple's OpenGL implementation has nevertheless had reported
`glCopyBufferSubData` corruption and crashes, especially on M-series hardware:

- [Sodium maintainer report of historical Apple driver failures](https://github.com/CaffeineMC/sodium/issues/2732#issuecomment-2973108868)

The same Sodium investigation later concluded that `glCopyBufferSubData` was
probably a red herring for the reported crash and that vertex attribute layouts
were the more likely cause:

- [Later investigation result](https://github.com/CaffeineMC/sodium/issues/2732#issuecomment-4183302760)

Neither conclusion is sufficient to establish whether AlvorKit's exact copy
sequence is reliable on the macOS configurations it supports. The old path
should not be restored on macOS without exercising that sequence directly, but
the macOS workaround also should not impose CPU readback on every platform.

## Investigation scope

1. Reproduce the old two-buffer GPU copy sequence on supported Apple Silicon
   and Intel macOS configurations.
2. Exercise the exact `SharedVertexBuffer` resize and compaction behavior across
   varied sizes, offsets, alignments, fragmentation patterns, and repeated
   operations.
3. Verify resulting bytes after every operation and record the GL version,
   vendor, renderer, and reported errors.
4. Determine whether any failure originates in `glCopyBufferSubData`, buffer
   bindings or ranges, adjacent GL state, or another driver-sensitive
   operation.
5. Restore GPU-to-GPU copying on every platform where it is demonstrated to be
   correct.
6. If macOS still requires a CPU implementation, make it an explicit macOS
   backend rather than the universal path.
7. If a CPU path remains, investigate transferring only live or occupied bytes
   and reusing scratch storage instead of allocating the full VBO capacity for
   every callback.
8. Add behavioral verification that detects lost or corrupted allocation
   contents after packing and resizing.

## Completion criteria

- Supported platform behavior is documented with evidence from the exact
  AlvorKit operation.
- Windows and Linux do not perform unconditional full-VBO CPU round trips.
- macOS uses the fastest implementation demonstrated to be correct.
- Packing and resizing preservation have focused behavioral coverage.
- A broader paged or non-moving allocation design remains a separate decision;
  it is not required merely to complete this investigation.
