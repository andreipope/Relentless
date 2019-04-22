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

        private IUIManager _uiManager;

        private TextMeshProUGUI _description, _amountAward;

        private Button _backButton;

        private ButtonShiftingContent _buttonMelt;

        private IReadOnlyCard _card;

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

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
        }

        public void Show(object data)
        {
            Show();

            _card = (IReadOnlyCard) data;
            _description.text = _card.FlavorText;

            _amountAward.text = (5 * ((int) _card.Rank + 1)).ToString();

            CardData = GameClient.Get<IDataManager>().CachedCollectionData.GetCardData(_card.MouldId);
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
                _uiManager.DrawPopup<WarningPopup>(
                    $"Melting is Disabled\nfor version {BuildMetaInfo.Instance.DisplayVersionName}.\n Thanks for helping us make this game Awesome\n-Loom Team");
            }
        }
    }
}
