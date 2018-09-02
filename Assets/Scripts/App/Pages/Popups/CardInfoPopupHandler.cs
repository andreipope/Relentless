using System;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    internal class CardInfoPopupHandler : IUIElement
    {
        public event Action StateChanging;

        public event Action StateChanged;

        public event Action Closing;

        public event Action Opening;

        public event Action<BoardCard> PreviewCardInstantiated;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IDataManager _dataManager;

        private BoardCard _previewCard;

        private BoardCard _selectedCollectionCard;

        private bool _blockedClosing;

        public bool IsStateChanging { get; private set; }

        public bool IsInteractable { get; private set; }

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
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
            if ((_uiManager.GetPopup<CardInfoPopup>().Self == null) && (_uiManager.GetPopup<DesintigrateCardPopup>().Self == null) && (_uiManager.GetPopup<WarningPopup>().Self == null))
            {
                if (!IsStateChanging && (_previewCard != null))
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
            if (_previewCard?.gameObject != null)
            {
                Object.Destroy(_previewCard?.gameObject);
            }
        }

        public void SelectCard(BoardCard card)
        {
            _uiManager.GetPopup<CardInfoPopup>().Hide();
            ClearPreviewCard();

            Opening?.Invoke();

            _blockedClosing = true;

            SetIsStateChanging(true);
            _selectedCollectionCard = card;

            if ((_previewCard != null) && (_previewCard.gameObject != null))
            {
                Object.DestroyImmediate(_previewCard.gameObject);
            }

            _previewCard = new BoardCard(Object.Instantiate(card.gameObject));
            _previewCard.gameObject.name = "CardPreview";
            _previewCard.gameObject.transform.position = card.gameObject.transform.position;
            _previewCard.gameObject.transform.localScale = card.gameObject.transform.lossyScale;

            _previewCard.gameObject.GetComponent<SortingGroup>().sortingLayerName = Constants.LAYER_GAME_UI2;

            PreviewCardInstantiated?.Invoke(_previewCard);

            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(_previewCard.transform.DORotate(new Vector3(-20, 30, -20), .2f));
            mySequence.Append(_previewCard.transform.DORotate(new Vector3(0, 0, 0), .4f));

            Sequence mySequence2 = DOTween.Sequence();
            mySequence2.Append(_previewCard.transform.DOMove(new Vector3(-4.3f, 1.2f, 5), .4f));
            mySequence2.Append(_previewCard.transform.DOMove(new Vector3(-4.3f, .8f, 5), .2f));

            Sequence mySequence3 = DOTween.Sequence();
            mySequence3.Append(_previewCard.transform.DOScale(new Vector3(.9f, .9f, .9f), .4f));
            mySequence3.Append(_previewCard.transform.DOScale(new Vector3(.72f, .72f, .72f), .2f));
            mySequence3.OnComplete(
                () =>
                {
                    SetIsStateChanging(false);
                });

            _uiManager.GetPopup<CardInfoPopup>().blockedClosing = true;
            _uiManager.GetPopup<CardInfoPopup>().cardTransform = _previewCard.transform;
            _uiManager.DrawPopup<CardInfoPopup>(card.libraryCard);

            GameClient.Get<ITimerManager>().AddTimer(
                x =>
                {
                    _blockedClosing = false;
                    _uiManager.GetPopup<CardInfoPopup>().blockedClosing = false;
                },
                null,
                1f);
        }

        private void Close()
        {
            if (_blockedClosing)
            
return;

            SetIsStateChanging(true);

            Closing?.Invoke();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_previewCard.transform.DOScale(_selectedCollectionCard.transform.lossyScale, .3f));
            sequence.Join(_previewCard.transform.DOMove(_selectedCollectionCard.transform.position, .3f));
            sequence.Join(_previewCard.transform.DORotateQuaternion(_selectedCollectionCard.transform.rotation, .3f));
            sequence.OnComplete(
                () =>
                {
                    ClearPreviewCard();
                    SetIsStateChanging(false);
                });
        }

        private void ClearPreviewCard()
        {
            Object.Destroy(_previewCard?.gameObject);
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
            } else
            {
                StateChanged?.Invoke();
            }
        }
    }
}
