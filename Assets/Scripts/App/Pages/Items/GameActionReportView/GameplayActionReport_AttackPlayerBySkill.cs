using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_AttackPlayerBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private HeroSkill _usedSkill;

        private Player _skillUsedOnPlayer;

        private GameObject _attackedPlayerObj, _attackingPlayerObj;

        public GameplayActionReport_AttackPlayerBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = gameAction.parameters[0] as Player;
            _usedSkill = gameAction.parameters[1] as HeroSkill;
            _skillUsedOnPlayer = gameAction.parameters[2] as Player;

            previewImage.sprite = loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" + _callerPlayer.SelfHero.heroElement + "_EXP");

            attackingPictureObject.SetActive(true);

            _attackingPlayerObj = CreatePlayerPreview(_callerPlayer, Vector3.zero);
            _attackedPlayerObj = CreatePlayerPreview(_skillUsedOnPlayer, Vector3.right * 6);

            GameObject cardView = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_usedSkill.value).ToString();
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
