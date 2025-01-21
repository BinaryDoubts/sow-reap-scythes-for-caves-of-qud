using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using HistoryKit;
using SimpleJSON;
using XRL;

//This is run once at main menu
// load and patch code from acegiak's qud-kissing mod: https://github.com/acegiak/qudkissing

namespace HistoryKit{
    [HasModSensitiveStaticCache]
    public static class SowReap_Init{

        [ModSensitiveCacheInit]
        public static void Init(){

            ModManager.ForEachFileIn("json", (string filePath, ModInfo modInfo) =>
            {
                if (filePath.ToLower().Contains(".json"))
                    acegiak_HistoricSpicePatcher.Patch(filePath);
            });

            

            
        }
    }
   
}
