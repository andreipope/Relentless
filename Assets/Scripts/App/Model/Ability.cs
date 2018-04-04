using System.Collections;
using System.Collections.Generic;
using GrandDevs.CZB.Common;

namespace GrandDevs.CZB
{
    /// <summary>
    /// The ability types supported by the kit.
    /// </summary>
    public enum AbilityType
    {
        Triggered,
        Activated
    }

    public class Ability
    {

        public string name;

        public AbilityType type;

        //public Effect effect;

        //public Target target;
    }


    public class TriggeredAbility : Ability
    {

        //public Trigger trigger;

        public TriggeredAbility()
        {
            type = AbilityType.Triggered;
        }
    }

    public class ActivatedAbility : Ability
    {
        public ActivatedAbility()
        {
            type = AbilityType.Activated;
        }
    }
}
