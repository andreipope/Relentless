// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public class BattleBoardArrow : BoardArrow
    {
        public List<object> ignoreBoardObjectsList;

        public List<BoardUnit> BoardCards;

        public BoardUnit owner;

        public bool ignoreHeavy = false;

        public void End(BoardUnit creature)
        {
            if (!startedDrag)
                return;

            startedDrag = false;

            creature.DoCombat(selectedCard != null?selectedCard:(object)selectedPlayer);
            Dispose();
        }

        public override void OnCardSelected(BoardUnit unit)
        {
            if (_gameplayManager.IsTutorial && ((_gameplayManager.TutorialStep == 19) || (_gameplayManager.TutorialStep == 27) || (_gameplayManager.TutorialStep == 32)))
                return;

            if ((ignoreBoardObjectsList != null) && ignoreBoardObjectsList.Contains(unit))
                return;

            if (unit.CurrentHP <= 0)
                return;

            if ((elementType.Count > 0) && !elementType.Contains(unit.Card.libraryCard.cardSetType))
            
return;

            if (targetsType.Contains(Enumerators.SkillTargetType.ALL_CARDS) || (targetsType.Contains(Enumerators.SkillTargetType.PLAYER_CARD) && unit.transform.CompareTag("PlayerOwned")) || (targetsType.Contains(Enumerators.SkillTargetType.OPPONENT_CARD) && unit.transform.CompareTag("OpponentOwned")))
            {
                bool opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || (opponentHasProvoke && unit.IsHeavyUnit()) || ignoreHeavy)
                {
                    if (selectedCard != null)
                    {
                        selectedCard.SetSelectedUnit(false);
                    }

                    selectedCard = unit;
                    if (selectedPlayer != null)
                    {
                        selectedPlayer.SetGlowStatus(false);
                    }

                    selectedPlayer = null;
                    selectedCard.SetSelectedUnit(true);
                    CreateTarget(unit.transform.position);
                }
            }
        }

        public override void OnCardUnselected(BoardUnit creature)
        {
            if (selectedCard == creature)
            {
                selectedCard.SetSelectedUnit(false);

                // _targetObjectsGroup.SetActive(false);
                selectedCard = null;
            }
        }

        public override void OnPlayerSelected(Player player)
        {
            if (_gameplayManager.IsTutorial && (_gameplayManager.TutorialStep != 19) && (_gameplayManager.TutorialStep != 28) && (_gameplayManager.TutorialStep != 32))
            
return;

            if (player.HP <= 0)
            
return;

            if ((ignoreBoardObjectsList != null) && ignoreBoardObjectsList.Contains(player))
            
return;

            if ((owner != null) && !owner.hasFeral && owner.HasBuffRush)
            
return;

            if ((targetsType.Contains(Enumerators.SkillTargetType.OPPONENT) && player.AvatarObject.CompareTag("OpponentOwned")) || (targetsType.Contains(Enumerators.SkillTargetType.PLAYER) && player.AvatarObject.CompareTag("PlayerOwned")))
            {
                bool opponentHasProvoke = OpponentBoardContainsProvokingCreatures();
                if (!opponentHasProvoke || ignoreHeavy)
                {
                    selectedPlayer = player;

                    selectedPlayer.SetGlowStatus(true);
                    if (selectedCard != null)
                    {
                        selectedCard.SetSelectedUnit(false);
                    }

                    selectedCard = null;
                    CreateTarget(player.AvatarObject.transform.position);
                }
            }
        }

        public override void OnPlayerUnselected(Player player)
        {
            if (selectedPlayer == player)
            {
                if (selectedCard != null)
                {
                    selectedCard.SetSelectedUnit(false);
                }

                selectedCard = null;

                selectedPlayer.SetGlowStatus(false);

                // _targetObjectsGroup.SetActive(false);
                selectedPlayer = null;
            }
        }

        protected bool OpponentBoardContainsProvokingCreatures()
        {
            List<BoardUnit> provokeCards = BoardCards.FindAll(x => x.IsHeavyUnit());
            return provokeCards.Count > 0;
        }

        private void Awake()
        {
            Init();
        }
    }
}
