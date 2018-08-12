using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using LoomNetwork.CZB;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    class CardInfoPopupHandler : IUIElement
    {
        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;

        public event Action StateChanging;
        public event Action StateChanged;
        public event Action Closing;
        public event Action Opening;
        public event Action<BoardCard> PreviewCardInstantiated;

        private BoardCard _previewCard;
        private BoardCard _selectedCollectionCard;

        public bool IsStateChanging { get; private set; }
        public bool IsInteractable { get; private set; }

        public void Init() {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
        }

        public void Show() {
           
        }

        public void Hide() {

        }

        public void Update() {
            IsInteractable = false;
            if (!_uiManager.GetPopup<CardInfoPopup>().Self.activeSelf &&
                !_uiManager.GetPopup<DesintigrateCardPopup>().Self.activeSelf &&
                !_uiManager.GetPopup<WarningPopup>().Self.activeSelf)
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

        public void Dispose() {
            if (_previewCard?.gameObject != null)
            {
                Object.Destroy(_previewCard?.gameObject);
            }
        }

        private void Close()
        {
            SetIsStateChanging(true);

            var amount = _dataManager.CachedCollectionData.GetCardData(_selectedCollectionCard.libraryCard.name).amount;
            _selectedCollectionCard.UpdateAmount(amount);

            Closing?.Invoke();

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_previewCard.transform.DOScale(_selectedCollectionCard.transform.lossyScale, .3f));
            sequence.Join(_previewCard.transform.DOMove(_selectedCollectionCard.transform.position, .3f));
            sequence.Join(_previewCard.transform.DORotateQuaternion(_selectedCollectionCard.transform.rotation, .3f));
            sequence.OnComplete(() =>
            {
                ClearPreviewCard();
                SetIsStateChanging(false);
            });
        }

        private void ClearPreviewCard() {
            Object.Destroy(_previewCard?.gameObject);
            _previewCard = null;
        }

        public void SelectCard(BoardCard card)
        {
            ClearPreviewCard();
            Opening?.Invoke();

            SetIsStateChanging(true);
            _selectedCollectionCard = card;

            if (_previewCard != null && _previewCard.gameObject != null)
                MonoBehaviour.DestroyImmediate(_previewCard.gameObject);

            _previewCard = new BoardCard(MonoBehaviour.Instantiate(card.gameObject));
            _previewCard.gameObject.name = "CardPreview";
            _previewCard.gameObject.transform.position = card.gameObject.transform.position;
            _previewCard.gameObject.transform.localScale = card.gameObject.transform.lossyScale;

            PreviewCardInstantiated?.Invoke(_previewCard);

            Utilites.SetLayerRecursively(_previewCard.gameObject, 11);

            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(_previewCard.transform.DORotate(new Vector3(-20, 30, -20), .2f));
            mySequence.Append(_previewCard.transform.DORotate(new Vector3(0, 0, 0), .4f));

            Sequence mySequence2 = DOTween.Sequence();
            mySequence2.Append(_previewCard.transform.DOMove(new Vector3(-4.3f, 1.2f, 5), .4f));
            mySequence2.Append(_previewCard.transform.DOMove(new Vector3(-4.3f, .8f, 5), .2f));

            Sequence mySequence3 = DOTween.Sequence();
            mySequence3.Append(_previewCard.transform.DOScale(new Vector3(.9f, .9f, .9f), .4f));
            mySequence3.Append(_previewCard.transform.DOScale(new Vector3(.72f, .72f, .72f), .2f));
            mySequence3.OnComplete(() =>
            {
                SetIsStateChanging(false);
            });


            _uiManager.GetPopup<CardInfoPopup>().cardTransform = _previewCard.transform;
            _uiManager.DrawPopup<CardInfoPopup>(card.libraryCard);
        }

        private void SetIsStateChanging(bool isStartedStateChange) {
            if (IsStateChanging  == isStartedStateChange)
                return;

            IsStateChanging  = isStartedStateChange;
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
