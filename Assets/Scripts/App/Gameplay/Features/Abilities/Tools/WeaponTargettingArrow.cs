using CCGKit;
using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class WeaponTargettingArrow : TargetingArrow
    {
        public event Action<BoardCreature> OnCardSelectedEvent;
        public event Action<BoardCreature> OnCardUnselectedevent;
        public event Action<PlayerAvatar> OnPlayerSelectedEvent;
        public event Action<PlayerAvatar> OnPlayerUnselectedEvent;
        public event Action OnInputEndEvent;

        private IInputManager _inputManager;

        private int _onMouseDownInputIndex;

        public List<Enumerators.AbilityTargetType> possibleTargets = new List<Enumerators.AbilityTargetType>();

        protected void Awake()
        {
            _inputManager = GameClient.Get<IInputManager>();

            _onMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 0, OnMouseButtonUpHandler, null, null);
        }

        protected void OnDestroy()
        {
            _inputManager.UnregisterInputHandler(_onMouseDownInputIndex);
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnCardSelected(BoardCreature creature)
        {
            if ((possibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER_CARD) && creature.CompareTag(Constants.TAG_PLAYER_OWNED)) ||
                (possibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD) && creature.CompareTag(Constants.TAG_OPPONENT_OWNED)) ||
                possibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                selectedCard = creature;
                selectedPlayer = null;
                CreateTarget(creature.transform.position);

                OnCardSelectedEvent?.Invoke(creature);
            }
        }

        public override void OnCardUnselected(BoardCreature creature)
        {
            if (selectedCard == creature)
            {
                Destroy(target);
                selectedCard = null;

                OnCardUnselectedevent?.Invoke(creature);
            }
        }

        public override void OnPlayerSelected(PlayerAvatar player)
        {
            if ((possibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER) && player.CompareTag(Constants.TAG_PLAYER_OWNED)) ||
                (possibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT) && player.CompareTag(Constants.TAG_OPPONENT_OWNED)) ||
                possibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                selectedPlayer = player;
                selectedCard = null;
                CreateTarget(player.transform.position);

                OnPlayerSelectedEvent?.Invoke(player);
            }
        }

        public override void OnPlayerUnselected(PlayerAvatar player)
        {
            if (selectedPlayer == player)
            {
                Destroy(target);
                selectedPlayer = null;

                OnPlayerUnselectedEvent?.Invoke(player);
            }
        }

        protected void OnMouseButtonUpHandler()
        {
            if (startedDrag)
            {
                startedDrag = false;
                OnInputEndEvent?.Invoke();
            }
        }
    }
}