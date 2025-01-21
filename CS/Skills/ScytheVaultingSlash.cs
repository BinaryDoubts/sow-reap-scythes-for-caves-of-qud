using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.World.Effects;
using XRL.World.Capabilities;

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_ScytheVaultingSlash : BaseSkill{
        public static readonly int COOLDOWN = 40;
        public static readonly string COMMAND_NAME = "SowReap_CommandVaultingSlash";
        public Guid ActivatedAbilityID = Guid.Empty;

        public SowReap_ScytheVaultingSlash(){

        }

        public SowReap_ScytheVaultingSlash(GameObject ParentObject) : this(){
            {
                this.ParentObject = ParentObject;
            }
        }

        public void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
        }

        public override bool AddSkill(GameObject GO){
            ActivatedAbilityID = AddMyActivatedAbility(Name: "Vaulting Slash", 
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
                PerformVaultingSlash();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeAbilityManagerOpenEvent E){
            DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
            return base.HandleEvent(E);
        }

        public bool PerformVaultingSlash(){
            if (!ParentObject.HasPrimaryWeaponOfType("SowReap_Scythe")){ //check for scythe
                ParentObject.Fail("You must have a scythe equipped in your primary appendage to perform Vaulting Slash.");
                return false;
            }

            GameObject scythe = ParentObject.GetPrimaryWeaponOfType("SowReap_Scythe");

            if (!ParentObject.CanMoveExtremities("Vaulting Slash", ShowMessage: true)) //check you can attack
                return false;
            
            string targetDirection = PickDirectionS("Vaulting Slash");
            Cell targetCell = ParentObject.GetCurrentCell().GetCellFromDirection(targetDirection);
            GameObject target = targetCell.GetCombatTarget(
                Attacker: ParentObject,
                IgnoreFlight: true
            );
            if (target != null){ //need something to hit
                if (targetCell.IsEmptyOfSolid(IncludeCombatObjects: false)){ //check cell is passable (ie, target isn't like, kudzu on a wall)
                    Cell destinationCell = targetCell.GetCellFromDirection(targetDirection);
                    if (destinationCell.IsEmptyOfSolid(IncludeCombatObjects: true)){
                        Messaging.XDidYToZ( //"[Attacker] vault over [target]." << needs fix when the attacker isn't "You"
                            Actor: ParentObject,
                            Verb: "vault",
                            Preposition: "over",
                            Object: target
                        );
                        ParentObject.PlayWorldSound("Sounds/Abilities/sfx_ability_jump");

                        //trigger attack as part of move, note that handling special procs from ready for harvest are handled in the core scythe.cs
                        MeleeAttackResult attackRes = Combat.MeleeAttackWithWeapon(
                            Attacker: ParentObject,
                            Defender: target,
                            Weapon: scythe,
                            BodyPart: ParentObject.Body?.FindDefaultOrEquippedItem(scythe),
                            Properties: "SowReap_NoReadyForHarvest,SowReap_VaultingSlashAttack",
                            Primary: true        
                        );
                        target.DustPuff(); //
                        ParentObject.DirectMoveTo(targetCell: destinationCell, EnergyCost: 1000, Forced: false, IgnoreCombat: true, IgnoreGravity: true);
                        ParentObject.Gravitate();
                        SowReap_ScytheVaultingSlash skill = ParentObject.GetPart<SowReap_ScytheVaultingSlash>();
                        skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
                    }
                    else{
                        ParentObject.Fail("There's something blocking your vault's destination.");
                        return false;
                    }
                }
                else{
                    ParentObject.Fail("There's something blocking your vault.");
                    return false;
                }
                

            }
            else{
                ParentObject.Fail("There's no one for you to vault over.");
                return false;
            }

            return true;
        }
    }
}