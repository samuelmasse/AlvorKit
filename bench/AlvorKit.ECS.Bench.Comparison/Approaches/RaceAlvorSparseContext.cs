namespace AlvorKit;

/// <summary>Owns sparse components and a prepared matching handle set; all fixture work precedes timing.</summary>
internal class RaceAlvorSparseContext : RaceAlvorKitBaseContext
{
    private readonly EntPtr[] _ents;

    internal EntPtr[] Ents => _ents;

    internal RaceAlvorSparseContext(int count, int padding, int components, bool mixed)
    {
        _ents = new EntPtr[count];

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < padding; j++)
            {
                var pad = Arena.Alloc();
                pad.SparsePadding1 = true;
            }

            var ent = Arena.Alloc();
            ent.SparseComponent1 = 0;

            if (components >= 2)
                ent.SparseComponent2 = 1;

            if (components == 3)
                ent.SparseComponent3 = 1;

            if (mixed)
            {
                switch (i % 4)
                {
                    case 0: ent.SparsePadding1 = true; break;
                    case 1: ent.SparsePadding2 = true; break;
                    case 2: ent.SparsePadding3 = true; break;
                    case 3: ent.SparsePadding4 = true; break;
                }
            }

            _ents[i] = ent;
        }
    }
}
