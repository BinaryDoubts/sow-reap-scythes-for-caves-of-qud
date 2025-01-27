using System;
using SowReap.Utilities;
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
            return "{{b-B-W-g-G|ready for harvest}}"; //I don't think this works atm
        }

        public override bool UseStandardDurationCountdown()
        {
            return true;
        }

        public override bool Apply(GameObject Object)
        {
            if (Object.HasEffect<SowReap_ReadyForHarvest>()){ //only one at a time
                Object.RemoveEffect<SowReap_ReadyForHarvest>();
            }

            //need better sfx
            Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_physicalRupture");
            if (SowReap_Options.ShowText){
                Object?.ParticleText(Text: "*ready for harvest*", Color: IComponent<GameObject>.ConsequentialColorChar(null, Object));
            }

            
            if (Object != null && Object.Equipped?.IsPlayer() == true)
            {
                IComponent<GameObject>.AddPlayerMessage("You are ready for harvest!", 'R');
            }
            
            return base.Apply(Object);
        }

        public override void Remove(GameObject Object){
            base.Remove(Object);
        }

        public override bool Render(RenderEvent E) //shows icon on target (similar to + overwrites wading glyph)
        {
            if (Duration > 0 && SowReap_Options.ShowIcon){
                E.RenderEffectIndicator("\xf5", "Effects/ready_for_harvest.png", "&g", "g", 45);
            }
            return true;
        }

    }

}
