using System.Runtime.InteropServices;

namespace AlvorKit;

internal static class RaceAlvorMixed
{
    internal class AlvorKitContext : RaceAlvorKitBaseContext
    {
        private readonly EntPtr[] _ents;

        public EntPtr[] Ents => _ents;

        public AlvorKitContext(int entCount)
        {
            _ents = new EntPtr[entCount];

            for (int i = 0; i < entCount; ++i)
            {
                EntPtr ent = Arena.Alloc();
                ent.Component1 = 0;
                ent.Component2 = 1;

                switch (i % 4)
                {
                    case 0:
                        ent.Padding1 = true;
                        break;
                    case 1:
                        ent.Padding2 = true;
                        break;
                    case 2:
                        ent.Padding3 = true;
                        break;
                    case 3:
                        ent.Padding4 = true;
                        break;
                }
                Ents[i] = ent;
            }
        }
    }
}
