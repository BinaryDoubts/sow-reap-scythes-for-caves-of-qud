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


        }

        [HarmonyPatch("AttemptHarvest")]
        [HarmonyPostfix]
        static void Postfix(GameObject who, Harvestable __instance, bool __result){
            if (__result && who.IsPlayer() && who.HasSkill("SowReap_ScytheSow")){

                    string name = GameObject.Create(__instance.OnSuccess).GetDisplayName();
                    switch (name){ //if the ingredient is common/useless, don't +1 it
                        case "vinewafer":
                        case "starapple":
                        case "spine fruit":
                        case "Eater's flesh":
                        case "witchwood bark":
                        case "plump mushroom":
                            return;
                        default:
                            break;
                    }
                    who.TakeObject(__instance.OnSuccess, NoStack: false, Silent: true, 0, 0, 0, null, null, null, null, null);
                    XRL.Messages.MessageQueue.AddPlayerMessage("You harvest an extra " + name + "!");

            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.RelicGenerator))]
    class SowReap_RelicGetTypePatch{
        [HarmonyPatch("GetType")]
        [HarmonyPostfix]
        static void Postfix(ref GameObject Object, ref string __result){
            MeleeWeapon scythe = Object.GetPart<MeleeWeapon>();
            if (scythe != null && scythe.Skill == "SowReap_Scythe"){
                __result = "SowReap_Scythe";
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.RelicGenerator))]
    class SowReap_RelicGetSubtypePatch{
        [HarmonyPatch("GetSubtype")]
        [HarmonyPostfix]
        static void Postfix(ref string type, ref string __result){
            if (type == "SowReap_Scythe"){
                __result = "weapon";
            }
            if (type == "scythe"){
                __result = "weapon";
            }
            if (type == "sickle"){
                __result = "weapon";
            }

        }
    }

    [HarmonyPatch(typeof(XRL.World.RelicGenerator))]
    class SowReap_RelicInitPatch{
        [HarmonyPatch("Init")]
        [HarmonyPostfix]
        static void Postfix(){
            if (SowReap_Options.Debug){
                foreach (string t in RelicGenerator.Types){
                        UnityEngine.Debug.LogError(t);
                }
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.Parts.ModSerrated))]
    class SowReap_SerratedPatch{
        [HarmonyPatch("ModificationApplicable")]
        static void Postfix(ref GameObject Object, ref bool __result){

            if (Object.TryGetPart<MeleeWeapon>(out var Part) && Part.Skill == "SowReap_Scythe"){
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.Parts.ModSharp))]
    class SowReap_SharpApplicablePatch{
        [HarmonyPatch("ModificationApplicable")]
        static void Postfix(ref GameObject Object, ref bool __result){

            if (Object.TryGetPart<MeleeWeapon>(out var Part) && Part.Skill == "SowReap_Scythe"){
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(XRL.World.Parts.ModSharp))]
    class SowReap_SharpApplyPatch{
        [HarmonyPatch("ApplyModification")]
        static void Postfix(ref GameObject Object, ref bool __result){

            if (Object.TryGetPart<MeleeWeapon>(out var Part) && Part.Skill == "SowReap_Scythe"){
                Part.PenBonus++;
            }
        }
    }

}