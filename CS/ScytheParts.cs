using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using XRL.Messages;
using XRL.World;

namespace XRL.World.Parts{
    [Serializable]
    public class SowReap_SplittingScythe : IActivePart{
        
    }

    [Serializable]
    public class SowReap_HolyScythe : ActiveLightSource{
        static int HOLY_RADIUS = 6;
        static int RESOURCE_MAX = 500;
        static int RESOURCE_DRAIN_RATE = 1;
        public int Resource = RESOURCE_MAX;


        public SowReap_HolyScythe(){
            WorksOnHolder = true;
            Radius = 0;
        }

        public override void Register (GameObject Object, IEventRegistrar Registrar){
            Registrar.Register(The.Game, EndTurnEvent.ID);     
            Registrar.Register(The.Player, WorshipPerformedEvent.ID);
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(EndTurnEvent E){
            if (IsReady()){
                if (Resource > 0){ //even if drain rate > 1, give them one grace turn
                    if (Radius != HOLY_RADIUS){
                        Radius = HOLY_RADIUS;
                    }
                    
                    if (Resource > (RESOURCE_MAX/2) && Resource - RESOURCE_DRAIN_RATE <= (RESOURCE_MAX/2)){
                        MessageQueue.AddPlayerMessage("Your " + _ParentObject.ShortDisplayName + "'s holy light dims.");
                    } 
                    else if (Resource > (RESOURCE_MAX/4) && Resource - RESOURCE_DRAIN_RATE <= (RESOURCE_MAX/4)){
                        MessageQueue.AddPlayerMessage("Your " + _ParentObject.ShortDisplayName + "'s holy light fades to near-nothingness.");
                    }
                    Resource -= RESOURCE_DRAIN_RATE;
                }
                else{
                    if (Radius != 0){
                        Radius = 0;
                    }
                }
            }

            return base.HandleEvent(E);
        }

        public override bool HandleEvent(WorshipPerformedEvent E){
            Resource = RESOURCE_MAX;
            MessageQueue.AddPlayerMessage("The act of devotion kindles your " + _ParentObject.ShortDisplayName + "'s blade-light.");         
            return base.HandleEvent(E);
        }

    }

}