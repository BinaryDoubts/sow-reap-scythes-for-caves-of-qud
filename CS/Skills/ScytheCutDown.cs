using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.World.Effects;
using XRL.UI;
using XRL.World.Anatomy;
using SowReap.Utilities;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_ScytheCutDown : BaseSkill{
        public static readonly int COOLDOWN = 50;
        public static readonly string COMMAND_NAME = "SowReap_CommandCutDown";

        public static readonly string BLEED_DAMAGE_BASE = "1d2";
        public static readonly string BLEED_DAMAGE_ENHANCED = "1d4";
        public static readonly int BLEED_SAVE_DIFFICULTY = 35; 
        
        public Guid ActivatedAbilityID = Guid.Empty;

        public SowReap_ScytheCutDown(){

        }

        public SowReap_ScytheCutDown(GameObject ParentObject) : this(){
            {
                this.ParentObject = ParentObject;
            }
        }

        public void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
        }

        public override bool AddSkill(GameObject GO){
            ActivatedAbilityID = AddMyActivatedAbility(Name: "Cut Down", 
                Command: COMMAND_NAME, 
                Class: "Skills");
            return base.AddSkill(GO);        
        }

        public override bool RemoveSkill(GameObject GO)
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            return base.RemoveSkill(GO);
        }

        // EVENT HANDLING
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(CommandEvent.ID);
            Registrar.Register(BeforeAbilityManagerOpenEvent.ID);
            base.Register(Object, Registrar);
            
        }
        
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_NAME){ 
                PerformCutDown();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeAbilityManagerOpenEvent E){
            DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
            return base.HandleEvent(E);
        }

        public bool PerformCutDown(){
            GameObject Attacker = ParentObject;
            if (!Attacker.HasPrimaryWeaponOfType("SowReap_ScytheSkill")){ //check for scythe
                Attacker.Fail("You must have a scythe equipped in your primary appendage to perform Vaulting Slash.");
                return false;
            }
            if (!ParentObject.CanMoveExtremities("Cut Down", ShowMessage: true)) //check you can attack
                return false;

            Cell targetCell = PickDirection("Cut Down");
            if (targetCell != null){
                GameObject target = targetCell.GetCombatTarget(Attacker);
                if (target != null){
                    if (target.Body == null){
                        Attacker.Fail("The target has no body for you to attack.");
                        return false;
                    }

                    GameObject scythe = Attacker.GetPrimaryWeaponOfType("SowReap_ScytheSkill");
                    List<BodyPart> legs = GetValidLegs(target);

                    if (legs.Count < 1){
                        Attacker.Fail("The target has no legs for you to cut off.");
                        return false;
                    }

                    if (target == Attacker && Attacker.IsPlayer() && Popup.ShowYesNo("Are you sure you want to cut " + Attacker.itself + " down?") != 0){
                        DislimberTarget(Attacker, target, scythe, legs); //cut your own legs off
                        SowReap_ScytheCutDown skill = ParentObject.GetPart<SowReap_ScytheCutDown>();
                        skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
                        return true;
                    }

                    MeleeAttackResult attackRes = Combat.MeleeAttackWithWeapon(
                        Attacker: Attacker,
                        Defender: target,
                        Weapon: scythe,
                        BodyPart: ParentObject.Body?.FindDefaultOrEquippedItem(scythe),
                        Properties: "SowReap_NoReadyForHarvest,SowReap_CutDownAttack",
                        Primary: true        
                    );

                    Attacker.UseEnergy(1000, "Skill Cut Down");

                    if (attackRes.Penetrations > 0){ //on hit+penetrate, also cut off a leg and make 'em bleed
                        DislimberTarget(Attacker, target, scythe, legs);
                    }

                }
                else{
                    Attacker.Fail("There's no one there for you to cut down.");
                    return false;
                }

            }

            return true;
        }
        
        public List<BodyPart> GetValidLegs(GameObject Target){
            List<BodyPart> validParts = new List<BodyPart>();
            Body targetBody = Target.Body;

            foreach (BodyPart part in targetBody.GetParts()){
                if (part.IsSeverable() && part.Mobility > 0 && !part.SeverRequiresDecapitate()){
                    validParts.Add(part);
                }
            }

            return validParts;
            
        }

        public bool DislimberTarget(GameObject Attacker, GameObject Target, GameObject Weapon, List<BodyPart> Legs, bool IgnoreReadyForHarvest = false){
            BodyPart chosenPart = Legs[SowReap_Random.Next(0, Legs.Count-1)];
            string ordinal = chosenPart.GetOrdinalName();
            chosenPart.Dismember();

            Target.ApplyEffect(new Bleeding(
                Damage: BLEED_DAMAGE_BASE, 
                SaveTarget: BLEED_SAVE_DIFFICULTY,
                Owner: Attacker
            ));

            IComponent<GameObject>.XDidYToZ( //Your scythe cut off 
                Actor: Weapon, 
                Verb: "cut", 
                Preposition: "off", 
                Object: Target,
                Extra: ordinal,
                EndMark: "!",
                ColorAsBadFor: Target,
                PossessiveObject: true,
                SubjectPossessedBy: Attacker
            );

            Legs = GetValidLegs(Target);

            if (!IgnoreReadyForHarvest && Target.HasEffect<SowReap_ReadyForHarvest>() && Legs.Count > 0){
                DislimberTarget(Attacker, Target, Weapon, Legs, IgnoreReadyForHarvest: true);
                Target.RemoveEffect<SowReap_ReadyForHarvest>();
            }
            else{

            }

            return true;
        }
    }
}