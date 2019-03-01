using System.Collections;
using System.Linq;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class TutorialRewardPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        
        public GameObject Self { get; private set; }
        
        private Button _buttonOk;
        
        private TextMeshProUGUI _message;

        private bool _isRewardClaimed;
        
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

        public async void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialRewardPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _message = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Message").GetComponent<TextMeshProUGUI>();

            _buttonOk = Self.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Panel_Buttons/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);
            _buttonOk.gameObject.SetActive(true);
            
            Self.SetActive(true);

            _isRewardClaimed = false;
            _message.text = "Claim your tutorial reward!";            
        }

        private async void OnClickOkButtonEventHandler()
        {
            if (!_isRewardClaimed)
            {
                _uiManager.DrawPopup<LoadingFiatPopup>("Request TutorialReward");
                _message.text = "Request TutorialReward";
                _buttonOk.gameObject.SetActive(false);
                TutorialRewardManager tutorialRewardManager = GameClient.Get<TutorialRewardManager>();
                try
                {
                    Protobuf.RewardTutorialCompletedResponse response = await tutorialRewardManager.CallRewardTutorialComplete();
                    await tutorialRewardManager.CallTutorialRewardContract(response);
                    _message.text = "Reward claimed!";
                }catch
                {
                    _message.text = "Reward claim fail";
                }
                _uiManager.HidePopup<LoadingFiatPopup>();
                
                _isRewardClaimed = true;
                _buttonOk.gameObject.SetActive(true);
            }
            else
            {
                _uiManager.HidePopup<TutorialRewardPopup>();
            }
        }
        
        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

    }
}