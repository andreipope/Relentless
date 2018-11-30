using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class CardInfoPopup : IUIPopup
    {
        public Transform CardTransform;

        public CollectionCardData CardData;

        public bool BlockedClosing = false;

        private readonly bool _disableMelt = true;

        private ILoadObjectsManager _loadObjectsManager;

        private ILocalizationManager _localizationManager;

        private IUIManager _uiManager;

        private TextMeshProUGUI _description, _amountAward, _meltTextMesh;

        private Button _backButton;

        private ButtonShiftingContent _buttonMelt;

        private Card _card;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
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
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonMelt = Self.transform.Find("MeltArea/Button_Melt").GetComponent<ButtonShiftingContent>();
            _backButton = Self.transform.Find("Button_Back").GetComponent<Button>();

            _buttonMelt.onClick.AddListener(DesintegrateButtonHandler);
            _backButton.onClick.AddListener(Hide);
            Self.GetComponent<Button>().onClick.AddListener(ClosePopup);

            _description = Self.transform.Find("MeltArea/Description").GetComponent<TextMeshProUGUI>();
            _amountAward = Self.transform.Find("MeltArea/GooAward/Value").GetComponent<TextMeshProUGUI>();
            _meltTextMesh = _buttonMelt.transform.Find("Text").GetComponent<TextMeshProUGUI>();

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);

            UpdateLocalization();
        }

        public void Show(object data)
        {
            Show();

            _card = (Card) data;
            _description.text = _card.FlavorText;

            _amountAward.text = (5 * ((int) _card.CardRank + 1)).ToString();

            CardData = GameClient.Get<IDataManager>().CachedCollectionData.GetCardData(_card.Name);
            UpdateCardAmount();
        }

        public void Update()
        {
        }

        public void UpdateCardAmount()
        {
            if (CardData.Amount == 0)
            {
                _buttonMelt.GetComponent<ButtonShiftingContent>().interactable = false;
            }
            else
            {
                _buttonMelt.GetComponent<ButtonShiftingContent>().interactable = true;
            }
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
            if (Self == null)
                return;

            _meltTextMesh.text = _localizationManager.GetUITranslation(LocalizationKeys.MeltText.ToString());
            //TODO : Later when do card flavor description
            //_description.text = _localizationManager.GetUITranslation(LocalizationKeys.PressAnyKeyText.ToString());
        }

        private void ClosePopup()
        {
            if (BlockedClosing)
                return;

            Hide();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD,
                Constants.SfxSoundVolume, false, false, true);
        }

        private void DesintegrateButtonHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            int amount = CardData.Amount;

            if (!_disableMelt)
            {
                if (amount == 0)
                {
                    _buttonMelt.GetComponent<MenuButtonNoGlow>().Interactable = false;
                }
                else
                {
                    Hide();
                    _uiManager.DrawPopup<DesintigrateCardPopup>(CardData);
                }
            }
            else
            {
                string msg = _localizationManager.GetUITranslation(LocalizationKeys.MeltingDisableInVersionText.ToString());
                msg = string.Format(msg, BuildMetaInfo.Instance.DisplayVersionName);
                _uiManager.DrawPopup<WarningPopup>(msg);
            }
        }
    }
}
