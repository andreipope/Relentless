using System;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using Loom.ZombieBattleground.Protobuf;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections.Generic;


using Loom.ZombieBattleground.Data;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;

using Loom.ZombieBattleground.Helpers;

namespace Loom.ZombieBattleground
{
    public class WaitingForPlayerPopup : IUIPopup
    {
        private const float TotalTimeBeforeForfeit = 180;
        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IGameplayManager _gameplayManager;

        private IAppStateManager _appStateManager;

        private IPvPManager _pvpManager;

        private CardsController _cardsController;

        private IUIManager _uiManager;

        private TextMeshProUGUI _text;

        private ButtonShiftingContent _gotItButton;

        private float _currentTimeBeforeForfeit;
        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
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

            _currentTimeBeforeForfeit = 0;

            Update();
        }

        public void Show(object data)
        {
            Show();

            _text.text = (string) data;
        }

        public void Update()
        {
            if (Self != null && Self.activeSelf) 
            {
                if (_appStateManager.AppState != Enumerators.AppState.GAMEPLAY)
                {
                    Hide();
                    return;
                }
                
                if (_gameplayManager.OpponentHasDoneMulligan != null || _pvpManager.DebugCheats.SkipMulligan)
                {
                    HandleMulliganOpponent(_gameplayManager.OpponentHasDoneMulligan);
                    _cardsController.EndCardDistribution();
                    Hide();

                    return;
                }

                _currentTimeBeforeForfeit += Time.deltaTime;

                if (_currentTimeBeforeForfeit > TotalTimeBeforeForfeit)
                {
                    _gameplayManager.OpponentPlayer.PlayerDie();
                    Hide();

                    return;
                }
            }
        }

        private void HandleMulliganOpponent(PlayerActionMulligan mulligan) 
        {
            if (Constants.MulliganEnabled && !_pvpManager.DebugCheats.SkipMulligan)
            {
                List<BoardUnitModel> cardsToRemove = new List<BoardUnitModel>();
                bool found;
                foreach (BoardUnitModel cardInHand in _gameplayManager.OpponentPlayer.CardsInHand)
                {
                    found = false;
                    foreach (Protobuf.InstanceId cardNotMulligan in mulligan.MulliganedCards)
                    {
                        if (cardNotMulligan.Id == cardInHand.InstanceId.Id)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        cardsToRemove.Add(cardInHand);
                    }
                }

                BattlegroundController battlegroundController = _gameplayManager.GetController<BattlegroundController>();

                foreach (BoardUnitModel card in cardsToRemove)
                {
                    _gameplayManager.OpponentPlayer.PlayerCardsController.RemoveCardFromHand(card);
                    OpponentHandCard opponentHandCard = battlegroundController.OpponentHandCards.FirstOrDefault(x => x.Model.InstanceId == card.InstanceId);
                    battlegroundController.OpponentHandCards.Remove(opponentHandCard);
                    opponentHandCard.Dispose();
                    _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardToDeck(card);
                }

                for (int i = 0; i < cardsToRemove.Count; i++)
                {
                    _gameplayManager.OpponentPlayer.PlayerCardsController.AddCardFromDeckToHand(_gameplayManager.OpponentPlayer.CardsInDeck[0]);
                }
            }
        }
    }
}
