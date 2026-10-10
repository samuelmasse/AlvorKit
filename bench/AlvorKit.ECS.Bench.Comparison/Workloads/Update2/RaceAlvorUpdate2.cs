using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AlvorKit;

internal static class RaceAlvorUpdate2
{
    internal class AlvorKitContext : RaceAlvorKitBaseContext
    {
        private readonly EntPtr[] _ents;

        public EntPtr[] Ents => _ents;

        public AlvorKitContext(int entCount, int entPadding)
        {
            _ents = new EntPtr[entCount];

            for (int i = 0; i < entCount; ++i)
            {
                for (int j = 0; j < entPadding; ++j)
                {
                    var pad = Arena.Alloc();
                    pad.Padding1 = true;
                }
                EntPtr ent = Arena.Alloc();
                ent.Component1 = 0;
                ent.Component2 = 1;
                Ents[i] = ent;
            }
        }
    }
}
