using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class PreparingForBattlePopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;
        private TextMeshProUGUI _flavorText;


        public GameObject Self { get; private set; }

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
            GameClient.Get<ICameraManager>().FadeOut(null, 1, true);

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
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PreparingForBattlePopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);
            _flavorText = Self.transform.Find("Image_Machine/Flavor_Text").GetComponent<TextMeshProUGUI>();

            ShowRandomFlavorText();
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void ShowRandomFlavorText()
        {
            IContentManager contentManger = GameClient.Get<IContentManager>();
            int randomVal = Random.Range(0, contentManger.FlavorTextInfo.Count);
            _flavorText.text = contentManger.FlavorTextInfo[randomVal].Description;
        }

    }
}
