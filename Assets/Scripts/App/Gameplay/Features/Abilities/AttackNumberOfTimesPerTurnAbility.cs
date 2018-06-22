using GrandDevs.CZB.Common;
using GrandDevs.CZB.Data;

namespace GrandDevs.CZB
{

    public class AttackNumberOfTimesPerTurnAbility : AbilityBase
    {
        private int _numberOfAttacksWas = 0;

        public Enumerators.AttackInfoType attackInfo;
        public int value = 1;


        public AttackNumberOfTimesPerTurnAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.value = ability.value;
            this.attackInfo = ability.attackInfoType;
        }

        public override void Activate()
        {
            base.Activate();

            boardCreature.attackInfoType = this.attackInfo;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private void Action()
        {
        }

        protected override void CreatureOnAttackEventHandler(object info)
        {
            base.CreatureOnAttackEventHandler(info);

            _numberOfAttacksWas++;

            if(_numberOfAttacksWas < value)
                boardCreature.ForceSetCreaturePlayable();
        }

        protected override void OnStartTurnEventHandler()
        {
            base.OnStartTurnEventHandler();
            _numberOfAttacksWas = 0;
        }
    }
}