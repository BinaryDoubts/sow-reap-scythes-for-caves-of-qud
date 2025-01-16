//taken from the Caves of Qud wiki

using System;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Genkit;

namespace SowReap.Utilities
{
    [HasGameBasedStaticCache]
    public static class SowReap_Random
    {
        private static Random _rand;
        public static Random Rand
        {
            get
            {
                if (_rand == null)
                {
                    if (XRLCore.Core?.Game == null)
                    {
                        throw new Exception("MODNAME mod attempted to retrieve Random, but Game is not created yet.");
                    }
                    else if (XRLCore.Core.Game.IntGameState.ContainsKey("MODNAME:Random"))
                    {
                        int seed = XRLCore.Core.Game.GetIntGameState("MODNAME:Random");
                        _rand = new Random(seed);
                    }
                    else
                    {
                        _rand = Stat.GetSeededRandomGenerator("MODNAME");
                    }
                    XRLCore.Core.Game.SetIntGameState("MODNAME:Random", _rand.Next());
                }
                return _rand;
            }
        }

        [GameBasedCacheInit]
        public static void ResetRandom()
        {
            _rand = null;
        }

        public static int Next(int minInclusive, int maxInclusive)
        {
            return Rand.Next(minInclusive, maxInclusive + 1);
        }
    }
}