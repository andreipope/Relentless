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
    public class RewardPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        
        public GameObject Self { get; private set; }

        private Transform _panelTop, _panelBottom, _imgPack;

        private GameObject _panelButton;

        private TextMeshProUGUI _message;

        private Image _rayTop, _rayBottom;
        
        private Button _buttonPlayAgain, _buttonOpenPacks;
        
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
            if (Self != null)
                return;

            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/RewardPopup"));
            Self.transform.SetParent(_uiManager.Canvas2.transform, false);
            
            _panelTop = Self.transform.Find("rewards_panel_top");
            _panelBottom = Self.transform.Find("rewards_panel_bottom");
            
            _panelButton = Self.transform.Find("rewards_panel_bottom/Panel_Buttons").gameObject;
            
            _buttonPlayAgain = Self.transform.Find("rewards_panel_bottom/Panel_Buttons/Button_PlayAgain").GetComponent<Button>();
            _buttonPlayAgain.onClick.AddListener(OnClickOpenPacksButtonEventHandler);
            _buttonPlayAgain.gameObject.SetActive(false);

            _buttonOpenPacks = Self.transform.Find("rewards_panel_bottom/Panel_Buttons/Button_OpenPacks").GetComponent<Button>();
            _buttonOpenPacks.onClick.AddListener(OnClickOpenPacksButtonEventHandler);
            _buttonOpenPacks.gameObject.SetActive(true);

            _imgPack = Self.transform.Find("Panel/Image_OpenPacks");
            _rayTop = Self.transform.Find("rewards_panel_top/rewards_light_top").GetComponent<Image>();
            _rayBottom = Self.transform.Find("rewards_panel_bottom/rewards_light_bottom").GetComponent<Image>();
            
            _message = Self.transform.Find("rewards_panel_top/Message").GetComponent<TextMeshProUGUI>();

            float offset = 5f;
            PrepareAnimation(offset);
            Self.SetActive(true);
            PlayAnimationSequence(offset);         
        }
        
        private void PrepareAnimation(float offset)
        {
            _panelButton.SetActive(false);
            _message.gameObject.SetActive(false);
        }

        private void PlayAnimationSequence(float offset)
        {
            Vector3 prevPackImageScale = _imgPack.localScale;        
            Vector3 prevPosTopPanel = _panelTop.position;   
            Vector3 prevPosBottomPanel = _panelBottom.position;       
            _panelTop.position += Vector3.up * offset;
            _panelBottom.position -= Vector3.up * offset;
            _imgPack.localScale = Vector3.one * (0.001f);
            _imgPack.DOScale
            (
                prevPackImageScale,
                1.25f
            ).SetEase(Ease.OutQuad);  
            _panelTop.DOMove
            (
                prevPosTopPanel,
                1f
            ).SetEase(Ease.OutQuad);            
            _panelBottom.DOMove
            (
                prevPosBottomPanel,
                1f
            ).SetEase(Ease.OutQuad)
            .OnComplete(
            ()=>
            {
                _panelButton.SetActive(true);
                _message.gameObject.SetActive(true);
            });

            _rayTop.color = Color.clear;
            _rayTop.DOColor(Color.white, 1f).SetEase(Ease.OutQuad);
            _rayBottom.color = Color.clear;
            _rayBottom.DOColor(Color.white, 1f).SetEase(Ease.OutQuad);
        }

        private void OnClickPlayAgainButtonEventHandler()
        {
            _uiManager.HidePopup<RewardPopup>();
        }

        private void OnClickOpenPacksButtonEventHandler()
        {
            _uiManager.HidePopup<RewardPopup>();
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.PACK_OPENER);
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
