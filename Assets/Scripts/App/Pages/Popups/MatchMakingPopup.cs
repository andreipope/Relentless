using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;
using Loom.ZombieBattleground.BackendCommunication;
using Object = UnityEngine.Object;
using TMPro;

namespace Loom.ZombieBattleground
{
    public class MatchMakingPopup : IUIPopup
    {
        public event Action CancelMatchmakingClicked;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IPvPManager _pvpManager;

        private Button _cancelMatchmakingButton;

        private Transform _matchMakingGroup;

        private TextMeshProUGUI _generalText;

        private MatchMakingFlowController _matchMakingFlowController;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _pvpManager = GameClient.Get<IPvPManager>();
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

            _matchMakingFlowController = null;
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

            SetUIState(MatchMakingFlowController.MatchMakingState.WaitingPeriod);
        }

        public void Show(object data)
        {
            Show();
        }

        public async void Update()
        {
            Task update = _matchMakingFlowController?.Update();
            if (update != null)
            {
                await update;
            }
        }

        private void PressedCancelMatchmakingHandler()
        {
            CancelMatchmakingClicked?.Invoke();
        }

        public void SetUIState(MatchMakingFlowController.MatchMakingState state)
        {
            Debug.Log(state);
            switch (state)
            {
                case MatchMakingFlowController.MatchMakingState.RegisteringToPool:
                    _generalText.text = "Registering Player for Matchmaking...";
                    break;
                case MatchMakingFlowController.MatchMakingState.WaitingPeriod:
                    _generalText.text = "Looking for a suitable opponent...";
                    break;
                case MatchMakingFlowController.MatchMakingState.FindingMatch:
                    _generalText.text = "Matching with a suitable opponent...";
                    break;
                case MatchMakingFlowController.MatchMakingState.AcceptingMatch:
                    _generalText.text = "Confirming match with a suitable opponent...";
                    break;
                case MatchMakingFlowController.MatchMakingState.WaitingForOpponent:
                    _generalText.text = "Waiting for confirmation from opponent...";
                    break;
                case MatchMakingFlowController.MatchMakingState.ConfirmingWithOpponent:
                    _generalText.text = "Confirming opponent status...";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task InitiateRegisterPlayerToPool(int deckId)
        {
            _matchMakingFlowController = new MatchMakingFlowController(
                GameClient.Get<BackendFacade>(),
                GameClient.Get<BackendDataControlMediator>(),
                GameClient.Get<IPvPManager>()
            );

            _matchMakingFlowController.StateChanged += SetUIState;
            await _matchMakingFlowController.Start(deckId);
        }
    }

}
