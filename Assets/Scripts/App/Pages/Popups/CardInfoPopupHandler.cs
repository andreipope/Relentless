using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    internal class CardInfoPopupHandler : IUIElement
    {
        private IUIManager _uiManager;

        private BoardCardView _previewCard;

        private BoardCardView _selectedCollectionCard;

        private bool _blockedClosing;

        public event Action StateChanging;

        public event Action StateChanged;

        public event Action Closing;

        public event Action Opening;

        public event Action<BoardCardView> PreviewCardInstantiated;

        public bool IsStateChanging { get; private set; }

        public bool IsInteractable { get; private set; }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Show()
        {
        }

        public void Hide()
        {
        }

        public void Update()
        {
            IsInteractable = false;
            if (_uiManager.GetPopup<CardInfoPopup>().Self == null &&
                _uiManager.GetPopup<DesintigrateCardPopup>().Self == null &&
                _uiManager.GetPopup<WarningPopup>().Self == null)
            {
                if (!IsStateChanging && _previewCard != null)
                {
                    Close();
                }

                if (!IsStateChanging)
                {
                    IsInteractable = true;
                }
            }
        }

        public void Dispose()
        {
            ClearPreviewCard();
        }

        public void SelectCard(BoardCardView card)
        {
            _uiManager.GetPopup<CardInfoPopup>().Hide();
            ClearPreviewCard();

            Opening?.Invoke();

            _blockedClosing = true;

            SetIsStateChanging(true);
            _selectedCollectionCard = card;

            if (_previewCard != null && _previewCard.GameObject != null)
            {
                Object.DestroyImmediate(_previewCard.GameObject);
            }

            _previewCard = new BoardCardView(Object.Instantiate(card.GameObject));
            _previewCard.GameObject.name = "CardPreview";
            _previewCard.GameObject.transform.position = card.GameObject.transform.position;
            _previewCard.GameObject.transform.localScale = card.GameObject.transform.lossyScale;

            _previewCard.GameObject.GetComponent<SortingGroup>().sortingLayerID = SRSortingLayers.GameUI2;

            PreviewCardInstantiated?.Invoke(_previewCard);

            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(_previewCard.Transform.DORotate(new Vector3(-20, 30, -20), .2f));
            mySequence.Append(_previewCard.Transform.DORotate(new Vector3(0, 0, 0), .4f));

            Sequence mySequence2 = DOTween.Sequence();
            mySequence2.Append(_previewCard.Transform.DOMove(new Vector3(-4.3f, 1.2f, 5), .4f));
            mySequence2.Append(_previewCard.Transform.DOMove(new Vector3(-4.3f, .8f, 5), .2f));

            Sequence mySequence3 = DOTween.Sequence();
            mySequence3.Append(_previewCard.Transform.DOScale(new Vector3(.9f, .9f, .9f), .4f));
            mySequence3.Append(_previewCard.Transform.DOScale(new Vector3(.72f, .72f, .72f), .2f));
            mySequence3.OnComplete(
                () =>
                {
                    SetIsStateChanging(false);
                });

            _uiManager.GetPopup<CardInfoPopup>().BlockedClosing = true;
            _uiManager.GetPopup<CardInfoPopup>().CardTransform = _previewCard.Transform;
            _uiManager.DrawPopup<CardInfoPopup>(card.BoardUnitModel.Card);

            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    _blockedClosing = false;
                    _uiManager.GetPopup<CardInfoPopup>().BlockedClosing = false;
                });
        }

        private void Close()
        {
            if (_blockedClosing)
                return;

            SetIsStateChanging(true);

            Closing?.Invoke();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_previewCard.Transform.DOScale(_selectedCollectionCard.Transform.lossyScale, .3f));
            sequence.Join(_previewCard.Transform.DOMove(_selectedCollectionCard.Transform.position, .3f));
            sequence.Join(_previewCard.Transform.DORotateQuaternion(_selectedCollectionCard.Transform.rotation, .3f));
            sequence.OnComplete(
                () =>
                {
                    ClearPreviewCard();
                    SetIsStateChanging(false);
                });
        }

        private void ClearPreviewCard()
        {
            Object.Destroy(_previewCard?.GameObject);
            _previewCard = null;
        }

        private void SetIsStateChanging(bool isStartedStateChange)
        {
            if (IsStateChanging == isStartedStateChange)
                return;

            IsStateChanging = isStartedStateChange;
            if (isStartedStateChange)
            {
                StateChanging?.Invoke();
            }
            else
            {
                StateChanged?.Invoke();
            }
        }
    }
}
