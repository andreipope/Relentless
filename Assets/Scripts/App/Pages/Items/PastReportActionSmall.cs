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

            PreviewImage.sprite = GetPreviewImage();

            OnBehaviourHandler behaviour = SelfObject.transform.Find("Collider").GetComponent<OnBehaviourHandler>();
            behaviour.MouseDownTriggered += MouseDownHandler;
        }

        public void Update()
        {
            if (Input.GetMouseButtonUp(0))
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

        private Sprite GetPreviewImage()
        {
            Sprite sprite = null;

            if (PastActionReport.Caller is Player player)
            {
                sprite = LoadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" +
                                                                    player.SelfHero.HeroElement + "_EXP");
            }
            else if (PastActionReport.Caller is BoardUnit unit)
            {
                sprite = unit.Sprite;
            }
            else if (PastActionReport.Caller is BoardCard card)
            {
                sprite = card.PictureSprite.sprite;
            }
            else if (PastActionReport.Caller is BoardSkill skill)
            {
                sprite = LoadObjectsManager.GetObjectByPath<Sprite>("Images/HeroesIcons/heroability_" +
                                                                    skill.OwnerPlayer.SelfHero.HeroElement + "_" +
                                                                    skill.Skill.Skill.ToLower());
            }

            return sprite;
        }
    }
}
