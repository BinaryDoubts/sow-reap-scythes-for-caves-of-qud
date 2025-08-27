using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.World.Effects;
using XRL.World.Parts;
using SowReap.Utilities;

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_Scythe : BaseSkill
    {
        public override int Priority => int.MinValue;
        public override string GetWeaponCriticalDescription()
        {
            return "Scythe (trips on critical hit)";
        }

        //scythe base on-crit effect
        public override void WeaponMadeCriticalHit(GameObject Attacker, GameObject Defender, GameObject Weapon, string Properties)
        {
            bool proneAttempt = Defender.ApplyEffect(new Prone());
            base.WeaponMadeCriticalHit(Attacker, Defender, Weapon, Properties);
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar){
            Registrar.Register("AttackerAfterAttack");
            Registrar.Register("DefenderAfterAttack");
            Registrar.Register("DealDamage");
            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E){
            string eventParams = E.GetStringParameter("Properties");

            if (E.ID == "AttackerAfterAttack"){
                GameObject Attacker = E.GetGameObjectParameter("Attacker");
                GameObject Defender = E.GetGameObjectParameter("Defender");
                GameObject Weapon = E.GetGameObjectParameter("Weapon");

                if (GameObject.Validate(ref Attacker) && GameObject.Validate(ref Defender)){
                    MeleeWeapon wep = Weapon.GetPart<MeleeWeapon>();

                    if (wep != null && wep.Skill == "SowReap_Scythe" && Attacker.HasSkill("SowReap_Scythe")){

                        if (!eventParams.HasDelimitedSubstring(",", "SowReap_NoReadyForHarvest")){ //scythe special skills can't apply Ready for Harvest as part of their action
                            int successThreshold = Attacker.HasSkill("SowReap_ScytheSow") ? 8 : 4; 

                            if (SowReap_Random.Next(1,20) <= successThreshold) //roll-under d20, applies status 20% of the time (on 1-4) with basic skill, applies 40% of the time (on 1-8) with upgraded skill
                                Defender.ApplyEffect(new SowReap_ReadyForHarvest());
                        }
                    }

                    if (eventParams.HasDelimitedSubstring(",", "SowReap_VaultingSlashAttack")){ //handle the vaulting slash activated skill
                        if (Defender.HasEffect<SowReap_ReadyForHarvest>()){
                            int attackerStrMod = Attacker.StatMod("Strength");
                            int attackerAgiMod = Attacker.StatMod("Agility");
                            int difficultyAdded = attackerStrMod > attackerAgiMod ? attackerStrMod : attackerAgiMod;
                            difficultyAdded = difficultyAdded > 0 ? difficultyAdded : 0; //no negatives from stat penalties because im nice like that

                            bool defSave = Defender.MakeSave( //save vs confusion
                                Stat: "Willpower",
                                Difficulty: 20+difficultyAdded
                            );

                            string effectString = Defender.ShortDisplayName + " became ";
                            string effectStringMid = "{{polarized|bamboozled}}.";
                            int confusionLevel = Attacker.Level / 2;
                            if (!defSave){
                                confusionLevel = confusionLevel <= 3 ? 3 : confusionLevel;
                                confusionLevel = confusionLevel >= 10 ? 10 : confusionLevel;
                                effectStringMid = "fully " + effectStringMid;
                            }
                            else{
                                effectStringMid = "partially" + effectStringMid;
                            }
                            
                            Messages.MessageQueue.AddPlayerMessage(effectString + effectStringMid);

                            Defender.ApplyEffect(new Prone());
                            if (!defSave){
                                Defender.ApplyEffect(new Confused(
                                    Duration: SowReap_Random.Next(8,12),
                                    Level: confusionLevel,
                                    MentalPenalty: confusionLevel
                                ));
                            }
                        }
                    }
                }
            }

            return base.FireEvent(E);
        }

    }

}