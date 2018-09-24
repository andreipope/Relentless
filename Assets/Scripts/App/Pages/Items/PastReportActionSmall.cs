using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class PastReportActionSmall
    {
        public GameObject SelfObject;

        protected ILoadObjectsManager LoadObjectsManager;

        protected IUIManager UIManager;

        protected Image PreviewImage;

        protected PastActionsPopup.PastActionParam PastActionReport;

        protected PastActionReportPanel _mainRoot;

        public PastReportActionSmall(PastActionReportPanel root, GameObject prefab, Transform parent, PastActionsPopup.PastActionParam pastActionParam)
        {
            LoadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            UIManager = GameClient.Get<IUIManager>();

            _mainRoot = root;
            PastActionReport = pastActionParam;
            SelfObject = Object.Instantiate(prefab, parent, false);
            SelfObject.transform.SetAsFirstSibling();
            PreviewImage = SelfObject.transform.Find("Image").GetComponent<Image>();

            PreviewImage.sprite = null; // todo improve

            OnBehaviourHandler behaviour = SelfObject.transform.Find("Collider").GetComponent<OnBehaviourHandler>();
            behaviour.MouseDownTriggered += MouseDownHandler;
        }

        public void Update()
        {
            if(Input.GetMouseButtonUp(0))
            {
                if (_mainRoot.IsDrawing)
                {
                    UIManager.HidePopup<PastActionsPopup>();
                    _mainRoot.IsDrawing = false;
                }
            }
        }

        public void MouseDownHandler(GameObject obj)
        {
            if (!_mainRoot.IsDrawing)
            {
                UIManager.DrawPopup<PastActionsPopup>(PastActionReport);
                _mainRoot.IsDrawing = true;
            }
        }
      
        public void Dispose()
        {
            Object.Destroy(SelfObject);
        }
    }
}
