using System;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Localization;
using UnityEngine;
using UnityEngine.UI;
using Loom.ZombieBattleground.BackendCommunication;
using Object = UnityEngine.Object;
using TMPro;
using Loom.ZombieBattleground.Gameplay;

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

            GameClient.Get<ICameraManager>().FadeOut(null, 1, true);

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
            _generalText = _matchMakingGroup.Find("Text_General").GetComponent<TextMeshProUGUI>();
            _cancelMatchmakingButton = _matchMakingGroup.Find("Button_Cancel").GetComponent<Button>();
            _cancelMatchmakingButton.onClick.AddListener(PressedCancelMatchmakingHandler);

            _cancelMatchmakingButton.gameObject.SetActive(true);
            SetUIState(MatchMakingFlowController.MatchMakingState.RegisteringToPool);

            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {

        }

        public void ForceCancelAndHide()
        {
            CancelMatchmakingClicked?.Invoke();

            _uiManager.HidePopup<MatchMakingPopup>();
        }

        private void PressedCancelMatchmakingHandler()
        {
            CancelMatchmakingClicked?.Invoke();
        }

        public void SetUIState(MatchMakingFlowController.MatchMakingState state)
        {
            switch (state)
            {
                case MatchMakingFlowController.MatchMakingState.RegisteringToPool:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.Warning_FriendlyGames_Disabled
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.WaitingPeriod:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_WaitingPeriod
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.FindingMatch:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_FindingMatch
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.AcceptingMatch:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_AcceptingMatch
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.WaitingForOpponent:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_WaitingForOpponent
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.ConfirmingWithOpponent:
                    _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_ConfirmingWithOpponent
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.NotStarted:
                    break;
                case MatchMakingFlowController.MatchMakingState.Canceled:
                    _generalText.text = _generalText.text = LocalizationUtil.GetLocalizedString
                    (
                        LocalizationTerm.MatchMakingPopup_Label_Canceled
                    );
                    break;
                case MatchMakingFlowController.MatchMakingState.Confirmed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
