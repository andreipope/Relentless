using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoadingGameplayPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IScenesManager _sceneManager;

        private Image _progressBar;

        private Image _backgroundImage;

        private Sprite[] _backgroundSprites;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _sceneManager = GameClient.Get<IScenesManager>();

            _backgroundSprites = GameClient.Get<ILoadObjectsManager>().GetObjectsByPath<Sprite>(new string[] {
                "Images/UI/Backgrounds/Loading/loading_screen_bg_air",
                "Images/UI/Backgrounds/Loading/loading_screen_bg_earth",
                "Images/UI/Backgrounds/Loading/loading_screen_bg_fire",
                "Images/UI/Backgrounds/Loading/loading_screen_bg_life",
                "Images/UI/Backgrounds/Loading/loading_screen_bg_toxic",
                "Images/UI/Backgrounds/Loading/loading_screen_bg_water"
            });
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
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoadingGameplayPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _backgroundImage = Self.transform.Find("Image_Background").GetComponent<Image>();
            _progressBar = Self.transform.Find("ProgresBar/Fill").GetComponent<Image>();

            int randomSprite = Random.Range(0, _backgroundSprites.Length);
            //_backgroundImage.sprite = _backgroundSprites[randomSprite];

            _progressBar.fillAmount = 0f;
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (Self == null)
                return;

            _progressBar.fillAmount = Mathf.Max(_progressBar.fillAmount, _sceneManager.SceneLoadingProgress / 100f);

            if (_sceneManager.SceneLoadingProgress >= 100)
            {
                Hide();
            }
        }
    }
}
