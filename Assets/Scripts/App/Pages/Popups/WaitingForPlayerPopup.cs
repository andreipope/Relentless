using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace Loom.ZombieBattleground
{
    public class WaitingForPlayerPopup : IUIPopup
    {
        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private IAppStateManager _appStateManager;

        private CardsController _cardsController;

        private IUIManager _uiManager;

        private TextMeshProUGUI _text;

        private ButtonShiftingContent _gotItButton;

        public GameObject Self { get; private set; }


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _cardsController = _gameplayManager.GetController<CardsController>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            PopupHiding?.Invoke();

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
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/WarningPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _gotItButton = Self.transform.Find("Button_GotIt").GetComponent<ButtonShiftingContent>();
            _gotItButton.gameObject.SetActive(false);

            _text = Self.transform.Find("Text_Message").GetComponent<TextMeshProUGUI>();
            _text.text = "Waiting for the opponent...";

            Update();
        }

        public void Show(object data)
        {
            Show();

            _text.text = (string) data;
        }

        public void Update()
        {
            if (Self != null) 
            {
                if (_appStateManager.AppState != Enumerators.AppState.GAMEPLAY)
                {
                    Hide();
                    return;
                }
                
                if (_gameplayManager.OpponentHasDoneMulligan)
                {
                    _cardsController.EndCardDistribution();
                    Hide();

                    return;
                }
            }
        }

        private void SendMulliganEvent () 
        {
            _uiManager.GetPopup<MulliganPopup>().InvokeMulliganCardsEvent(_gameplayManager.CurrentPlayer.CardsPreparingToHand.ToList());
        }
    }
}
