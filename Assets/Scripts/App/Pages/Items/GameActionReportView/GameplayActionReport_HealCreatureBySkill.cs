using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportHealCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private HeroSkill _usedSkill;

        private BoardUnit _skillUsedOnUnit;

        private GameObject _healPlayerObj, _healedUnitObj;

        public GameplayActionReportHealCreatureBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;
            _usedSkill = GameAction.Parameters[1] as HeroSkill;
            _skillUsedOnUnit = GameAction.Parameters[2] as BoardUnit;

            PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" + _callerPlayer.SelfHero.HeroElement + "_EXP");

            HealPictureObject.SetActive(true);

            _healPlayerObj = CreatePlayerPreview(_callerPlayer, Vector3.zero);
            _healedUnitObj = CreateCardPreview(_skillUsedOnUnit.Card, Vector3.right * 6);

            GameObject cardView = _healedUnitObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = _usedSkill.Value.ToString();
            cardView.transform.localPosition = -Vector3.up;
        }

        public override void OnPointerEnterEventHandler(PointerEventData obj)
        {
            base.OnPointerEnterEventHandler(obj);
        }

        public override void OnPointerExitEventHandler(PointerEventData obj)
        {
            base.OnPointerExitEventHandler(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
