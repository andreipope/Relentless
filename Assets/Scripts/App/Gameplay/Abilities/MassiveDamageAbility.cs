using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class MassiveDamageAbility : AbilityBase
    {
        public int value = 1;

        public MassiveDamageAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            value = ability.value;
        }

        public override void Activate()
        {
            base.Activate();
            if (abilityCallType != Enumerators.AbilityCallType.ENTRY)

                return;

            Action();

            // _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/healVFX");
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnDieEventHandler()
        {
            base.UnitOnDieEventHandler();
            if (abilityCallType != Enumerators.AbilityCallType.DEATH)

                return;

            Debug.Log("CreatureOnDieEventHandler");
            Action();
        }

        protected override void CreateVFX(Vector3 pos, bool autoDestroy = false, float duration = 3f, bool justPosition = false)
        {
            int playerPos = playerCallerOfAbility.IsLocalPlayer?1:-1;

            switch (abilityEffectType)
            {
                case Enumerators.AbilityEffectType.MASSIVE_WATER_WAVE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_FIRE:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/SpellMassiveFireVFX");
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_LIGHTNING:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/LightningVFX");
                    pos = Vector3.up * 0.5f;
                    break;
                case Enumerators.AbilityEffectType.MASSIVE_TOXIC_ALL:
                    _vfxObject = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/VFX/Spells/ToxicMassiveAllVFX");
                    pos = Vector3.zero;
                    break;
            }

            pos = Utilites.CastVFXPosition(pos * playerPos);

            ClearParticles();

            base.CreateVFX(pos, true, 5f);
        }

        private void Action()
        {
            object caller = abilityUnitOwner != null?abilityUnitOwner:(object)boardSpell;

            Player opponent = playerCallerOfAbility == _gameplayManager.CurrentPlayer?_gameplayManager.OpponentPlayer:_gameplayManager.CurrentPlayer;
            foreach (Enumerators.AbilityTargetType target in abilityTargetTypes)
            {
                switch (target)
                {
                    case Enumerators.AbilityTargetType.OPPONENT_ALL_CARDS:

                        // BoardCreature[] creatures = new BoardCreature[playerCallerOfAbility.opponentBoardCardsList.Count];
                        // player.BoardCards.CopyTo(creatures);
                        foreach (BoardUnit cardOpponent in opponent.BoardCards)
                        {
                            _battleController.AttackUnitByAbility(caller, abilityData, cardOpponent);
                        }

                        CreateVFX(Vector3.up * 1.5f);

                        // Array.Clear(creatures, 0, creatures.Length);
                        // creatures = null;
                        break;
                    case Enumerators.AbilityTargetType.PLAYER_ALL_CARDS:

                        // RuntimeCard[] cards = new RuntimeCard[playerCallerOfAbility.boardZone.cards.Count];
                        // playerCallerOfAbility.boardZone.cards.CopyTo(cards);
                        // foreach (var cardPlayer in cards)
                        // {
                        // playerCallerOfAbility.FightCreatureBySkill(value, cardPlayer);
                        // CreateVFX(cardPlayer.transform.position);
                        // }
                        // Array.Clear(cards, 0, cards.Length);
                        // cards = null;
                        foreach (BoardUnit cardPlayer in playerCallerOfAbility.BoardCards)
                        {
                            _battleController.AttackUnitByAbility(caller, abilityData, cardPlayer);
                            CreateVFX(cardPlayer.transform.position);
                        }

                        break;
                    case Enumerators.AbilityTargetType.OPPONENT:
                        _battleController.AttackPlayerByAbility(caller, abilityData, opponent);

                        // CreateVFX(targetCreature.transform.position);
                        break;
                    case Enumerators.AbilityTargetType.PLAYER:
                        _battleController.AttackPlayerByAbility(caller, abilityData, playerCallerOfAbility);

                        // CreateVFX(targetCreature.transform.position);
                        break;
                }
            }
        }
    }
}
