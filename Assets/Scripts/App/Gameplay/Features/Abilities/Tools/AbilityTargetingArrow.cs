using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.CZB
{
    public class AbilityTargetingArrow : TargetingArrow
    {
        public event Action<BoardCreature> OnCardSelectedEvent;
        public event Action<BoardCreature> OnCardUnselectedevent;
        public event Action<Player> OnPlayerSelectedEvent;
        public event Action<Player> OnPlayerUnselectedEvent;
        public event Action OnInputEndEvent;
        public event Action OnInputCancelEvent;

        private IInputManager _inputManager;

        private int _onMouseDownInputIndex;
        private int _onRightMouseDownInputIndex;
        private int _onEscapeInputIndex;

        public List<Enumerators.AbilityTargetType> possibleTargets = new List<Enumerators.AbilityTargetType>();
        public BoardCreature selfBoardCreature;


        protected void Awake()
        {
            _inputManager = GameClient.Get<IInputManager>();

            _onMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 0, null, OnMouseButtonDownHandler, null);
            _onRightMouseDownInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.MOUSE, 1, null, OnRightMouseButtonDownHandler, null);
            _onEscapeInputIndex = _inputManager.RegisterInputHandler(Enumerators.InputType.KEYBOARD, (int)KeyCode.Escape, null, OnRightMouseButtonDownHandler, null);
        }

        protected void OnDestroy()
        {
            _inputManager.UnregisterInputHandler(_onMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onRightMouseDownInputIndex);
            _inputManager.UnregisterInputHandler(_onEscapeInputIndex);
        }

        protected override void Update()
        {
            base.Update();
        }

        public override void OnCardSelected(BoardCreature creature)
        {
            if ((possibleTargets.Contains(Enumerators.AbilityTargetType.PLAYER_CARD) && creature.gameObject.CompareTag(Constants.TAG_PLAYER_OWNED)) ||
                (possibleTargets.Contains(Enumerators.AbilityTargetType.OPPONENT_CARD) && creature.gameObject.CompareTag(Constants.TAG_OPPONENT_OWNED)) ||
                possibleTargets.Contains(Enumerators.AbilityTargetType.ALL))
            {
                if (selfBoardCreature != creature)
                {
                    selectedCard = creature;
                    selectedPlayer = null;
                    CreateTarget(creature.transform.position);

                    OnCardSelectedEvent?.Invoke(creature);
                }
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

        protected void OnMouseButtonDownHandler()
        {
            if (startedDrag)
            {
                startedDrag = false;
                OnInputEndEvent?.Invoke();
            }
        }

        protected void OnRightMouseButtonDownHandler()
        {
            if (startedDrag)
            {
                startedDrag = false;
                OnInputCancelEvent?.Invoke();
            }
        }  
    }
}