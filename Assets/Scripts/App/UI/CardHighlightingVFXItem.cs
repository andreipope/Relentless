using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CardHighlightingVFXItem
    {
        public GameObject selfObject;

        public long MouldId;

        private bool _isFirstActive;

        private const int OFFSET = 26;

        public CardHighlightingVFXItem(GameObject obj)
        {
            selfObject = obj;
            _isFirstActive = false;
            ChangeState(false);
        }

        public void ChangeState(bool isActive)
        {
            if (!_isFirstActive && isActive)
            {
                return;
            }
            selfObject.SetActive(isActive);
        }

        public void SetActiveCard(BoardCardView card)
        {
            _isFirstActive = true;
            ChangeState(true);
            MouldId = card.BoardUnitModel.Card.CardPrototype.MouldId;
            selfObject.transform.position = card.Transform.position;
            selfObject.transform.localPosition -= Vector3.up * OFFSET;
        }
    }
}
