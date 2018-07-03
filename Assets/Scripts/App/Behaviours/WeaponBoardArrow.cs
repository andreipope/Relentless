// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class WeaponBoardArrow : BoardArrow
    {
        public event Action<BoardUnit> OnCardSelectedEvent;
        public event Action<BoardUnit> OnCardUnselectedevent;
        public event Action<Player> OnPlayerSelectedEvent;
        public event Action<Player> OnPlayerUnselectedEvent;
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

        public override void OnCardSelected(BoardUnit creature)
        {
            if ((possibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER_CARD) && creature.gameObject.CompareTag(Constants.TAG_PLAYER_OWNED)) ||
                (possibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD) && creature.gameObject.CompareTag(Constants.TAG_OPPONENT_OWNED)) ||
                possibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                selectedCard = creature;
                selectedPlayer = null;
                CreateTarget(creature.transform.position);

                OnCardSelectedEvent?.Invoke(creature);
            }
        }

        public override void OnCardUnselected(BoardUnit creature)
        {
            if (selectedCard == creature)
            {
                Destroy(target);
                selectedCard = null;

                OnCardUnselectedevent?.Invoke(creature);
            }
        }

        public override void OnPlayerSelected(Player player)
        {
            if ((possibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER) && player.AvatarObject.CompareTag(Constants.TAG_PLAYER_OWNED)) ||
                (possibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT) && player.AvatarObject.CompareTag(Constants.TAG_OPPONENT_OWNED)) ||
                possibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                selectedPlayer = player;
                selectedCard = null;
                CreateTarget(player.AvatarObject.transform.position);

                OnPlayerSelectedEvent?.Invoke(player);
            }
        }

        public override void OnPlayerUnselected(Player player)
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