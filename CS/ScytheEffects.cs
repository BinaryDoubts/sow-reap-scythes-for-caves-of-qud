using System;
using XRL.World.Effects;

namespace XRL.World.Effects
{
    [Serializable]
    public class SowReap_ReadyForHarvest : Effect{

        public SowReap_ReadyForHarvest(){
            DisplayName = "ready for harvest";
            Duration = 10;
        }

        private string EffectSummary()
        {
            return "Primed for additional effects from a reaping scythe attack.";
        }

        public override string GetDetails()
        {
            return EffectSummary();
        }

        public override string GetDescription()
        {
            return "{{b-B-W-g-G|ready for harvest}}";
        }

        public override bool UseStandardDurationCountdown()
        {
            return true;
        }

        public override bool Apply(GameObject Object)
        {
            if (Object.HasEffect<SowReap_ReadyForHarvest>()){
                return false;
            }

            Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalRupture");
            Object?.ParticleText("*ready for harvest*", IComponent<GameObject>.ConsequentialColorChar(null, Object));
            
            if (Object != null && Object.Equipped?.IsPlayer() == true)
            {
                IComponent<GameObject>.AddPlayerMessage("You are ready for harvest!", 'R');
            }
            
            return base.Apply(Object);
        }

        public override void Remove(GameObject Object){
            base.Remove(Object);
        }

    }

}
