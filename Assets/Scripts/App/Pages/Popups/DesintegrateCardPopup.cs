using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class DesintigrateCardPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _yesButton, _noButton;

        private CollectionCardData _cardData;

        public GameObject Self { get; private set; }

        private GameObject _cardPreview;
        private Vector3 _cardPreviewOriginalPos;
        private Vector3 _cardPreviewPosition = new Vector3(5.5f, 0.1f, 5f);

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;

            if (_cardPreview != null)
                _cardPreview.transform.position = _cardPreviewOriginalPos;

        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            _cardPreview = GameObject.Find("CardPreview");
            if (_cardPreview != null)
            {
                _cardPreviewOriginalPos = _cardPreview.transform.position;
                _cardPreview.transform.position = _cardPreviewPosition;
            }


            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/DesintegrateCardPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _yesButton = Self.transform.Find("QuestionArea/YesButton").GetComponent<Button>();
            _noButton = Self.transform.Find("QuestionArea/NoButton").GetComponent<Button>();


            _yesButton.onClick.AddListener(DesintegrateButtonHandler);
            _noButton.onClick.AddListener(CloseDesintegratePopup);
        }

        public void Show(object data)
        {
            Show();

            _cardData = (CollectionCardData) data;
            if (_cardData.Amount == 0)
            {
                _yesButton.interactable = false;
            }
            else
            {
                _yesButton.interactable = true;
            }
        }

        public void Update()
        {
        }

        private void CloseDesintegratePopup()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            Card prototype = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards
                .First(card => card.Name == _cardData.CardName);
            _uiManager.DrawPopup<CardInfoPopup>(prototype);

            Hide();
        }

        private void DesintegrateButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            _cardData.Amount--;
            if (_cardData.Amount == 0)
            {
                _yesButton.interactable = false;
            }

            _cardPreview.GetComponent<BoardCardView>().UpdateAmount(_cardData.Amount);

            Card prototype = GameClient.Get<IDataManager>().CachedCardsLibraryData.Cards
                .First(card => card.Name == _cardData.CardName);
            GameClient.Get<IPlayerManager>().ChangeGoo(5 * ((int) prototype.CardRank + 1));

            _uiManager.GetPage<ArmyPage>().UpdateGooValue();
        }
    }
}
