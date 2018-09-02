using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class CardInfoPopup : IUIPopup
    {
        private readonly bool disableMelt = true;

        public Transform cardTransform;

        public CollectionCardData _cardData;

        public bool blockedClosing = false;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private TextMeshProUGUI _description, _amountAward;

        private Button _backButton;

        private ButtonShiftingContent _buttonMelt;

        private TextMeshProUGUI _buttonText;

        private Card _card;

        public GameObject Self { get; private set; }

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
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/CardInfoPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _buttonMelt = Self.transform.Find("MeltArea/Button_Melt").GetComponent<ButtonShiftingContent>();
            _backButton = Self.transform.Find("Button_Back").GetComponent<Button>();

            _buttonMelt.onClick.AddListener(DesintegrateButtonHandler);
            _backButton.onClick.AddListener(Hide);
            Self.GetComponent<Button>().onClick.AddListener(ClosePopup);

            _description = Self.transform.Find("MeltArea/Description").GetComponent<TextMeshProUGUI>();
            _amountAward = Self.transform.Find("MeltArea/GooAward/Value").GetComponent<TextMeshProUGUI>();

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        public void Show(object data)
        {
            Show();

            _card = data as Card;
            _description.text = _card.flavorText;

            _amountAward.text = (5 * ((int)_card.cardRank + 1)).ToString();

            _cardData = GameClient.Get<IDataManager>().CachedCollectionData.GetCardData(_card.name);
            UpdateCardAmount();
        }

        public void Update()
        {
        }

        public void UpdateCardAmount()
        {
            if (_cardData.amount == 0)
            {
                _buttonMelt.GetComponent<ButtonShiftingContent>().interactable = false;
            } else
            {
                _buttonMelt.GetComponent<ButtonShiftingContent>().interactable = true;
            }
        }

        private void ClosePopup()
        {
            if (blockedClosing)

                return;

            Hide();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.DECKEDITING_REMOVE_CARD, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        private void DesintegrateButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            int amount = _cardData.amount;

            if (!disableMelt)
            {
                if (amount == 0)
                {
                    _buttonMelt.GetComponent<MenuButtonNoGlow>().interactable = false;
                }

                // _uiManager.DrawPopup<WarningPopup>("Sorry you don't have cards to desintegrate");
                else
                {
                    /*cardTransform.DOKill();
                    cardTransform.DOScale(new Vector3(.3f, .3f, .3f), 0.2f);*/
                    Hide();
                    _uiManager.DrawPopup<DesintigrateCardPopup>(_cardData);
                    _uiManager.GetPopup<DesintigrateCardPopup>().cardTransform = cardTransform;
                }
            } else
            {
                _uiManager.DrawPopup<WarningPopup>($"Melting is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}.\n Thanks for helping us make this game Awesome\n-Loom Team");
            }
        }
    }
}
