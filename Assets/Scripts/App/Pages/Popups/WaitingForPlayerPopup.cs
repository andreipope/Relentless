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
        
        private const float _timeBeforeRetry = 5;
        private float _currentTimerCounter;

        private const int _totalAttemptsBeforeAutomaticWin = 5;
        private int _currentAttempts;


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

            _currentTimerCounter = _timeBeforeRetry;

            _currentAttempts = 0;

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
                    SendMulliganEvent();
                    _cardsController.EndCardDistribution();
                    Hide();

                    return;
                }

                _currentTimerCounter += Time.deltaTime;

                if (_currentTimerCounter >= _timeBeforeRetry)
                {
                    _currentTimerCounter = 0;
                    _currentAttempts++;

                    if (_currentAttempts > _totalAttemptsBeforeAutomaticWin)
                    {
                        _gameplayManager.OpponentPlayer.PlayerDie();
                        Hide();

                        return;
                    }
                    else
                    {
                        SendMulliganEvent ();

                        return;
                    }
                }
            }
        }

        private void SendMulliganEvent () 
        {
            _uiManager.GetPopup<MulliganPopup>().InvokeMulliganCardsEvent(_gameplayManager.CurrentPlayer.CardsPreparingToHand.ToList());
        }
    }
}
