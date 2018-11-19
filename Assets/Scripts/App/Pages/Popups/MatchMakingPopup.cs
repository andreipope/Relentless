using System;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Object = UnityEngine.Object;
using TMPro;

namespace Loom.ZombieBattleground
{
    public class MatchMakingPopup : IUIPopup
    {
        const int WaitingTime = 5;

        const string PlayerIsAlreadyInAMatch = "Player is already in a match";

        public event Action CancelMatchmakingClicked;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Button _cancelMatchmakingButton;

        private Transform _matchMakingGroup;

        private TextMeshProUGUI _generalText;

        private MatchMakingState _state;

        private float _currentWaitingTime;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
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
            _state = MatchMakingState.RegisteringToPool;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MatchMakingPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _matchMakingGroup = Self.transform.Find("Matchmaking_Group");
            _generalText = _matchMakingGroup.Find("Text_General").GetComponent<TextMeshProUGUI>();
            _cancelMatchmakingButton = _matchMakingGroup.Find("Button_Cancel").GetComponent<Button>();

            _cancelMatchmakingButton.onClick.AddListener(PressedCancelMatchmakingHandler);

            SetUIStateAsync(MatchMakingState.WaitingPeriod);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (_state == MatchMakingState.WaitingPeriod) {
                _currentWaitingTime += Time.deltaTime;
                if (_currentWaitingTime > WaitingTime) {
                    SetUIStateAsync(MatchMakingState.FindingMatch);
                }
            } else if (_state == MatchMakingState.WaitingForOpponent)
            {
                _currentWaitingTime += Time.deltaTime;
                if (_currentWaitingTime > WaitingTime)
                {
                    SetUIStateAsync(MatchMakingState.ConfirmingWithOpponent);
                }
            }
        }

        private async void ErrorHandler (Exception exception) {
            Debug.LogWarning(exception.Message);

            if (exception.Message.Contains(PlayerIsAlreadyInAMatch)) 
            {
                try
                {
                    CancelFindMatchResponse result = await _backendFacade.CancelFindMatchRelatedToUserId(
                        _backendDataControlMediator.UserDataModel.UserId
                    );

                    await InitiateRegisterPlayerToPool(GameClient.Get<IUIManager>().GetPage<GameplayPage>().CurrentDeckId);
                }
                catch (Exception e)
                {
                    ErrorHandler(e);
                }
            }
        }

        private void PressedCancelMatchmakingHandler()
        {
            CancelMatchmakingClicked?.Invoke();
        }

        public async Task InitiateRegisterPlayerToPool (long deckId) {
            SetUIStateAsync(MatchMakingState.RegisteringToPool);
            try
            {
                RegisterPlayerPoolResponse result = await _backendFacade.RegisterPlayerPool(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deckId
                );

                SetUIStateAsync(MatchMakingState.WaitingPeriod);
            } 
            catch (Exception e) 
            {
                ErrorHandler(e);
            }
        }

        private async Task InitiateFindingMatch()
        {
            try
            {
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _backendDataControlMediator.UserDataModel.UserId
                );

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                if (!result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    mustAccept = true;
                                }
                            }
                        }

                        if (mustAccept)
                        {
                            SetUIStateAsync(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                        }
                        else
                        {
                            SetUIStateAsync(MatchMakingState.WaitingForOpponent);
                        }
                    }
                }
                else
                {
                    SetUIStateAsync(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                SetUIStateAsync(MatchMakingState.WaitingPeriod);
            }
        }

        private async Task CheckIfOpponentIsReady () {
            try
            {
                FindMatchResponse result = await _backendFacade.FindMatch(
                    _backendDataControlMediator.UserDataModel.UserId
                );

                Debug.LogWarning(result.ToString());

                if (result.Match != null)
                {
                    if (result.Match.Status == Match.Types.Status.Matching)
                    {
                        bool mustAccept = false;
                        bool opponentHasAccepted = false;
                        for (int i = 0; i < result.Match.PlayerStates.Count; i++)
                        {
                            if (result.Match.PlayerStates[i].Id == _backendDataControlMediator.UserDataModel.UserId)
                            {
                                if (!result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    mustAccept = true;
                                }
                            }
                            else
                            {
                                if (result.Match.PlayerStates[i].MatchAccepted)
                                {
                                    opponentHasAccepted = true;
                                }
                            }
                        }

                        if (mustAccept)
                        {
                            SetUIStateAsync(MatchMakingState.AcceptingMatch);
                            await InitiateAcceptingMatch(result.Match.Id);
                            return;
                        }

                        if (opponentHasAccepted && !mustAccept) 
                        {
                            Debug.Log("The Match is Starting!");
                            StartConfirmedMatch(result);
                        }
                        else
                        {
                            SetUIStateAsync(MatchMakingState.WaitingForOpponent);
                        }
                    }
                    else if (result.Match.Status == Match.Types.Status.Started) {
                        StartConfirmedMatch(result);
                    }
                    else 
                    {
                        await _backendFacade.UnsubscribeEvent();
                        await InitiateRegisterPlayerToPool(GameClient.Get<IUIManager>().GetPage<GameplayPage>().CurrentDeckId);
                    }
                }
                else
                {
                    SetUIStateAsync(MatchMakingState.WaitingForOpponent);
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                SetUIStateAsync(MatchMakingState.WaitingPeriod);
            }
        }

        private async Task InitiateAcceptingMatch (long matchId) {
            try
            {

                AcceptMatchResponse result = await _backendFacade.AcceptMatch(
                    _backendDataControlMediator.UserDataModel.UserId,
                    matchId
                );

                Debug.LogWarning(result.ToString());

                SetUIStateAsync(MatchMakingState.WaitingForOpponent);
                await _backendFacade.SubscribeEvent(result.Match.Topics.ToList());
            }
            catch (Exception e)
            {
                ErrorHandler(e);
                SetUIStateAsync(MatchMakingState.WaitingPeriod);
            }
        }

        private void StartConfirmedMatch(FindMatchResponse findMatchResponse)
        {
            GameClient.Get<IPvPManager>().MatchIsStarting(findMatchResponse);
        }

        private async void SetUIStateAsync(MatchMakingState state)
        {
            _state = state;
            switch (_state)
            {
                case MatchMakingState.RegisteringToPool:
                    _generalText.text = "Registering Player for Matchmaking...";
                    break;
                case MatchMakingState.WaitingPeriod:
                    _currentWaitingTime = 0;
                    _generalText.text = "Looking for a suitable opponent...";
                    break;
                case MatchMakingState.FindingMatch:
                    _generalText.text = "Matching with a suitable opponent...";
                    await InitiateFindingMatch();
                    break;
                case MatchMakingState.AcceptingMatch:
                    _generalText.text = "Confirming match with a suitable opponent...";
                    break;
                case MatchMakingState.WaitingForOpponent:
                    _currentWaitingTime = 0;
                    _generalText.text = "Waiting for confirmation from opponent...";
                    break;
                case MatchMakingState.ConfirmingWithOpponent:
                    _generalText.text = "Confirming opponent status...";
                    await CheckIfOpponentIsReady();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum MatchMakingState
        {
            RegisteringToPool,

            WaitingPeriod,

            FindingMatch,

            AcceptingMatch,

            WaitingForOpponent,

            ConfirmingWithOpponent
        }
    }
}
