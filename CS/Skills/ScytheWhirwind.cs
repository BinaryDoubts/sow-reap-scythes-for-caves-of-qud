using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;
using XRL.World.Effects;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_ScytheWhirlwind : BaseSkill{
        public static readonly int COOLDOWN = 40;
        public static readonly string COMMAND_NAME = "SowReap_CommandWhirlwind";

        public Guid ActivatedAbilityID = Guid.Empty;

        public SowReap_ScytheWhirlwind(){

        }

        public SowReap_ScytheWhirlwind(GameObject ParentObject) : this(){
            {
                this.ParentObject = ParentObject;
            }
        }

        public void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
        }

        public override bool AddSkill(GameObject GO){
            ActivatedAbilityID = AddMyActivatedAbility(Name: "Swhirlwind", 
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
                PerformWhirlwind();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(BeforeAbilityManagerOpenEvent E){
            DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
            return base.HandleEvent(E);
        }

        //MAIN
        public bool PerformWhirlwind(){
            GameObject Attacker = ParentObject;
            if (!Attacker.HasPrimaryWeaponOfType("SowReap_Scythe")){ //check for scythe
                Attacker.Fail("You must have a scythe equipped in your primary appendage to perform Swhirlwind.");
                return false;
            }
            if (!ParentObject.CanMoveExtremities("Swhirlwind", ShowMessage: true)) //check you can attack
                return false;

            string targetDirection = PickDirectionS("Swhirlwind");
            Cell attackerCell = ParentObject.GetCurrentCell();
            Cell targetCell = attackerCell.GetCellFromDirection(targetDirection, false);

            if (targetCell != null && targetCell != attackerCell && !targetCell.IsSolid()){
                List<string> orderedDirectionList = new List<string>();
                int startingIndex = Directions.DirectionList.IndexOf(targetDirection);
                int currentIndex = startingIndex+1; 

                while (orderedDirectionList.Count < 8){ //move clockwise, ending at the chosen direction's cell
                    currentIndex = currentIndex+1 >= 8 ? 0 : currentIndex+1; //wrap back to the start of the list
                    orderedDirectionList.Add(Directions.DirectionList[currentIndex]);
                }

                List<Cell> targetCells = new List<Cell>
                {
                    attackerCell //insert the attacker's cell as the first in the list to be cleared of gas
                };
                
                foreach (string direction in orderedDirectionList){                  
                    targetCells.Add(attackerCell.GetCellFromDirection(direction));
                }

                foreach (Cell currentCell in targetCells){ //check each cell for gasses, then relocate them to the target cell
                    List<GameObject> gasses = currentCell.GetObjectsWithPart("Gas");
                    if (gasses.Count > 0){
                        //MessageQueue.AddPlayerMessage("Gasses found: " + gasses.Count);
                        foreach (GameObject gas in gasses){
                            Gas g = (Gas)gas.GetPart("Gas");
                            //g.Density *= 2;  way too powerful, should maybe even be halved
                            gas.DirectMoveTo(targetCell: targetCell, Forced: true, IgnoreGravity: true, IgnoreCombat: true);
                        }
                    }
                }

                Attacker.PlayWorldSound("Sounds/Abilities/sfx_ability_longBlade_swipe");
                Attacker.DustPuff();
                GameObject target = targetCell.GetCombatTarget(Attacker, IgnoreFlight: true);
                if (target != null){ //doesn't check if target is hostile, maybe worth adding a popup?
                    if (!target.HasEffect<SowReap_ReadyForHarvest>()){
                        target.ApplyEffect(new SowReap_ReadyForHarvest());
                    }
                    Messaging.XDidYToZ(
                        Actor: Attacker, 
                        Verb: "swirled all nearby gasses", 
                        Preposition: "onto", 
                        Object: target,
                        EndMark: "!"
                    );
                }
                else{
                    Messaging.XDidY(
                        Actor: Attacker,
                        Verb: "swirled all nearby gasses",
                        Extra: "over to a nearby space"
                    );
                }
                Attacker.UseEnergy(1000, "Skill Swhirlwind");
                SowReap_ScytheWhirlwind skill = ParentObject.GetPart<SowReap_ScytheWhirlwind>();
                skill.CooldownMyActivatedAbility(skill.ActivatedAbilityID, COOLDOWN);
                return true;
            }

            return false;
        }
    }
}