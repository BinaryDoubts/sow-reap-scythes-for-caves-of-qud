using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using XRL.Messages;
using XRL.World;

namespace XRL.World.Parts{

    [Serializable]
    public class SowReap_HolyLight : IPart{
        public int RADIUS = 6;
        public LightSource light = new LightSource();

        public override void Initialize()
        {
            _ParentObject.AddPart(light);
            light.Radius = RADIUS;
            base.Initialize();
        }

        public override void Remove(){
            light.Remove();
            base.Remove();
        }
    }

    [Serializable]
    public class  SowReap_HolySickle : IPart{
        static int RESOURCE_MAX = 500;
        static int DRAIN_RATE = 1;
        public int Resource = RESOURCE_MAX;
        public bool HalfFlag = false;
        public bool QuarterFlag = false;
        public bool AbilityDisabled = false;

        public override void Initialize()
        {
            
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar){
            Registrar.Register(GetShortDescriptionEvent.ID);
            Registrar.Register(GetDisplayNameEvent.ID);
            Registrar.Register(The.Game, EndTurnEvent.ID);     
            Registrar.Register(The.Player, WorshipPerformedEvent.ID);
            Registrar.Register(_ParentObject, EquippedEvent.ID);
            
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(GetShortDescriptionEvent E){ 
            return base.HandleEvent(E);
        } 

        public override bool HandleEvent(GetDisplayNameEvent E){ 
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(WorshipPerformedEvent E){
            Resource = RESOURCE_MAX;
            MessageQueue.AddPlayerMessage("Your act of devotion kindles the sickle's light.");         
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EquippedEvent E){
            if (Resource > 0){
                EnableSickleAbility();
            }
            return base.HandleEvent(E);
        }

        public override bool HandleEvent(EndTurnEvent E){
            string description = "holy light fades";
            string intensifier = "to near-nothingness";
            bool intense = false;
            bool warn = false;
            
            if (Resource <= RESOURCE_MAX/4 && !QuarterFlag){
                QuarterFlag = true;
                intense = true;
                warn = true;
            }
            else if (Resource <= RESOURCE_MAX/2 && !HalfFlag){
                HalfFlag = true;
                warn = true;
            }
            else if (Resource > RESOURCE_MAX/2){
                QuarterFlag = false;
                HalfFlag = false;
            }

            if (Resource <= 0){
                //DisableSickleAbility();
                //Resource = 0;
                Resource = RESOURCE_MAX;
                    
            }
            else{
                EnableSickleAbility();
                Resource-=100;
                MessageQueue.AddPlayerMessage("Resource left: " + Resource);
            }

            if (warn){
                if (intense){
                    description = description + " " + intensifier;
                }
                MessageQueue.AddPlayerMessage("Your " + _ParentObject.ShortDisplayName + "'s " + description + ".");
            }
            return base.HandleEvent(E);
        }

        public void EnableSickleAbility(){
            SowReap_HolyLight light = _ParentObject.GetPart<SowReap_HolyLight>();
            if (light != null){
                light.light.Lit = true;
            }
            else{
                _ParentObject.AddPart<SowReap_HolyLight>();
            }
        }

        public void DisableSickleAbility(){
            SowReap_HolyLight light = _ParentObject.GetPart<SowReap_HolyLight>();
            if (light != null){
                light.light.Lit = false;
            }
        }

    }

}