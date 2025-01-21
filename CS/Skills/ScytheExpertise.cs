using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.Messages;
using ConsoleLib.Console;

namespace XRL.World.Parts.Skill{
    [Serializable]
    class SowReap_ScytheExpertise : BaseSkill
    {
        public int hitBonus = 2;
        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade))
            {
                return ID == PooledEvent<GetToHitModifierEvent>.ID;
            }
            return true;
        }
        public override bool HandleEvent(GetToHitModifierEvent E)
        {
            if (E.Actor == ParentObject && E.Checking == "Actor" && E.Skill == "SowReap_Scythe" && E.Melee)
            {
                E.Modifier += hitBonus;
                if (E.Target.HasTagOrProperty("LivePlant") || E.Target.HasTagOrProperty("LiveFungus"))
                    if (E.Actor.HasSkill("SowReap_ScytheSow"))
                        E.Modifier += hitBonus; // bonus doubled against plants and fungi
                
            }
            return base.HandleEvent(E);
        }

        public override bool AllowStaticRegistration()
        {
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("DealDamage");
            base.Register(Object, Registrar);
        }
    }
}