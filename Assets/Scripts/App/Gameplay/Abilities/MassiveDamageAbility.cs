using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int Value;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action();
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();
            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            Debug.Log("CreatureOnDieEventHandler");
            Action();
        }

        protected override void CreateVfx(
            Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            int playerPos = PlayerCallerOfAbility.IsLocalPlayer ? 1 : -1;

            switch (AbilityEffectType)
            {
                case Enumerators.AbilityEffectType.MASSIVE_WATER_WAVE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_FIRE:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>(
                        "Prefabs/VFX/Spells/SpellMassiveFireVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_LIGHTNING:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/LightningVFX");
                    pos = Vector3.up * 0.5f;
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_TOXIC_ALL:
                    VfxObject = LoadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    pos = Vector3.zero;
                    break;
            }

            pos = Utilites.CastVfxPosition(pos * playerPos);

            ClearParticles();

            base.CreateVfx(pos, true, 5f);
        }

        private void Action()
        {
            object caller = AbilityUnitOwner != null ? AbilityUnitOwner : (object) BoardSpell;

            Player opponent = PlayerCallerOfAbility == GameplayManager.CurrentPlayer ?
                GameplayManager.OpponentPlayer :
                GameplayManager.CurrentPlayer;
            foreach (Enumerators.AbilityTargetType target in AbilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:
                        foreach (BoardUnit cardOpponent in opponent.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardOpponent);
                        }

                        CreateVfx(Vector3.up * 1.5f);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:
                        foreach (BoardUnit cardPlayer in PlayerCallerOfAbility.BoardCards)
                        {
                            BattleController.AttackUnitByAbility(caller, AbilityData, cardPlayer);
                            CreateVfx(cardPlayer.Transform.position);
                        }

                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, opponent);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        BattleController.AttackPlayerByAbility(caller, AbilityData, PlayerCallerOfAbility);
                        break;
                }
            }
        }
    }
}
