using HarmonyLib;
using PlayFab.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.UI.ObjectFinderClassifiers;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using SowReap.Utilities;

namespace SowReap.HarmonyPatches{
    [HarmonyPatch(typeof(XRL.World.Parts.Harvestable))]
    class SowReap_HarvestPatch{
        [HarmonyPatch("AttemptHarvest")]
        [HarmonyPrefix]
        static void Prefix(GameObject who, Harvestable __instance){
            //XRL.Messages.MessageQueue.AddPlayerMessage("test");

        }

        [HarmonyPatch("AttemptHarvest")]
        [HarmonyPostfix]
        static void Postfix(GameObject who, Harvestable __instance, bool __result){
            if (__result && who.IsPlayer() && who.HasSkill("SowReap_ScytheSkill")){
                //if (Stat.Rnd(1, 20) > 10){
                    string name = GameObject.Create(__instance.OnSuccess).GetDisplayName();
                    who.TakeObject(__instance.OnSuccess, NoStack: false, Silent: true, 0, 0, 0, null, null, null, null, null);
                    XRL.Messages.MessageQueue.AddPlayerMessage("You harvest an extra " + name + "!");
                //}
            }
        }
    }
}