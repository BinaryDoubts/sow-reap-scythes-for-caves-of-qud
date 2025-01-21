using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.World.Effects;
using XRL.World.Parts;
using SowReap.Utilities;
using XRL.World;
using HarmonyLib;
using System.Linq;
using Genkit;
using Microsoft.CodeAnalysis;

/*TODO: 
- Add/improve juice
*/

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_ScytheHarvestMoon : BaseSkill
    {
        public static readonly int COOLDOWN = 60;
        public static readonly string COMMAND_NAME = "SowReap_CommandHarvestMoon";
        public Guid ActivatedAbilityID = Guid.Empty;

        public SowReap_ScytheHarvestMoon(){

        }

        public SowReap_ScytheHarvestMoon(GameObject ParentObject) : this(){
            {
                this.ParentObject = ParentObject;
            }
        }

        public void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
        }

        public override bool AddSkill(GameObject GO){
            ActivatedAbilityID = AddMyActivatedAbility(Name: "Harvest Moon", 
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
            Registrar.Register(AttackerDealingDamageEvent.ID);
            Registrar.Register(GetAttackerHitDiceEvent.ID);
            base.Register(Object, Registrar);
            
        }
        
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_NAME){ 
                PerformHarvestMoon();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeAbilityManagerOpenEvent E){
            DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
            return base.HandleEvent(E);
        }

        // MAIN
        public bool PerformHarvestMoon(){
            if (!ParentObject.HasPrimaryWeaponOfType("SowReap_Scythe")){ //check for scythe
                ParentObject.Fail("You must have a scythe equipped in your primary appendage to perform Harvest Moon.");
                return false;
            }

            if (!ParentObject.CanMoveExtremities("Harvest Moon", ShowMessage: true)) //check you can attack
                return false;
            
            string targetDirection = PickDirectionS("Harvest Moon"); //choose target cell

            Cell attackerCell = ParentObject.GetCurrentCell();
            string[] adjacentDirections = Directions.GetAdjacentDirections(targetDirection); //get two other adjacent cells (thank you Directions class we love to see it)
            Cell[] targetCells = {
                attackerCell.GetCellFromDirection(adjacentDirections[0]), //clockwise order
                attackerCell.GetCellFromDirection(targetDirection),
                attackerCell.GetCellFromDirection(adjacentDirections[1])
            };

            /*XRL.Messages.MessageQueue.AddPlayerMessage(adjacentDirections[0]);
            XRL.Messages.MessageQueue.AddPlayerMessage(targetDirection);
            XRL.Messages.MessageQueue.AddPlayerMessage(adjacentDirections[1]);*/

            List<GameObject> combatTargets = new();
            foreach (Cell c in targetCells){
                GameObject tempTarget = c.GetCombatTarget(
                    Attacker: ParentObject,
                    AllowInanimate: true,
                    InanimateSolidOnly: true
                );
                if (tempTarget != null && tempTarget.IsHostileTowards(ParentObject)){
                    //XRL.Messages.MessageQueue.AddPlayerMessage("Target found: " + tempTarget.GetDisplayName());
                    combatTargets.Add(tempTarget);
                }
            }

            if (combatTargets.Count() > 0){
                GameObject scythe = ParentObject.GetPrimaryWeaponOfType("SowReap_Scythe");
                foreach (GameObject target in combatTargets){
                    MeleeAttackResult attackRes = Combat.MeleeAttackWithWeapon(
                        Attacker: ParentObject,
                        Defender: target,
                        Weapon: scythe,
                        ParentObject.Body?.FindDefaultOrEquippedItem(scythe),
                        Properties: "SowReap_NoReadyForHarvest,SowReap_HarvestMoonAttack",
                        Primary: true        
                    );
                    if (attackRes.Hits > 0 && target.HasEffect<SowReap_ReadyForHarvest>()){
                        target.TakeDamage(
                            DamageAmount: attackRes.Damage,
                            FromAttacker: ParentObject,
                            ShowMessage: "after facing the harvester's scythe."
                        );
                        
                        Cell targetCell = target.GetCurrentCell();

                        ParentObject.GetDirectionToward(targetCell);

                        Location2D attackerLoc = new Location2D(attackerCell.X, attackerCell.Y);
                        Location2D targetLoc = new Location2D(targetCell.X, targetCell.Y);
                        float targetAngle = attackerLoc.AngleTo(targetLoc);

                        target.BloodsplatterCone(SelfSplatter: true, targetAngle, 20);
                        target.RemoveEffect<SowReap_ReadyForHarvest>();
                    }

                }
                ParentObject.UseEnergy(1000, "Skill Harvest Moon");
                SowReap_ScytheHarvestMoon skill = ParentObject.GetPart<SowReap_ScytheHarvestMoon>();
                skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
                return true;
            }
            else{
                ParentObject.Fail("There's nothing for you to attack.");
                return false;
            }

        }

    }
}