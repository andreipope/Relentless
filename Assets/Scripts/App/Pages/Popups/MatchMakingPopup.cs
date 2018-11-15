using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MatchMakingPopup : IUIPopup
    {
        const int WaitingTime = 5;

        public event Action CancelMatchmakingClicked;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;

        private Button _cancelMatchmakingButton;

        private Transform _matchMakingGroup;

        private Text _generalText;

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
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MatchMakingPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _matchMakingGroup = Self.transform.Find("Matchmaking_Group");
            _generalText = _matchMakingGroup.Find("Text_General").GetComponent<Text>();
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
                await _backendFacade.RegisterPlayerPool(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deckId
                );
            } 
            catch (Exception e) 
            {
                Debug.LogWarning(e.Message);
            }
        }

        private async Task InitiateFindingMatch () {
            FindMatchResponse result = await _backendFacade.FindMatch(
                _backendDataControlMediator.UserDataModel.UserId
            );

            Debug.Log(result.Match.Status);
            Debug.Log(result.Match.PlayerStates[0].MatchAccepted);
            Debug.Log(result.Match.PlayerStates[1].MatchAccepted);
        }

        private async Task InitiateAcceptingMatch () {
            AcceptMatchResponse result = await _backendFacade.AcceptMatch(
                _backendDataControlMediator.UserDataModel.UserId
            );

            Debug.Log(result.Match.Status);
            Debug.Log(result.Match.PlayerStates[0].MatchAccepted);
            Debug.Log(result.Match.PlayerStates[1].MatchAccepted);
        }

        private async void SetUIStateAsync(MatchMakingState state)
        {
            _state = state;
            switch (_state)
            {
                case MatchMakingState.RegisteringToPool:
                    break;
                case MatchMakingState.WaitingPeriod:
                    _currentWaitingTime = 0;
                    break;
                case MatchMakingState.FindingMatch:
                    await InitiateFindingMatch();
                    break;
                case MatchMakingState.AcceptingMatch:
                    break;
                case MatchMakingState.WaitingForOpponent:
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

            WaitingForOpponent
        }
    }
}
