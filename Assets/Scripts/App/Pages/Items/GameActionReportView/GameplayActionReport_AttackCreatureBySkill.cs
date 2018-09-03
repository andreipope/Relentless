using LoomNetwork.CZB.Data;
using TMPro;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class GameplayActionReportAttackCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private HeroSkill _usedSkill;

        private int _skillValue;

        private BoardUnit _skillUsedOnUnit;

        private GameObject _attackedCreatureObj, _attackingPlayerObj;

        public GameplayActionReportAttackCreatureBySkill(
            GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = GameAction.Parameters[0] as Player;
            _usedSkill = GameAction.Parameters[1] as HeroSkill;
            _skillValue = (int) GameAction.Parameters[2];
            _skillUsedOnUnit = GameAction.Parameters[3] as BoardUnit;

            PreviewImage.sprite = LoadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/CZB_2D_Hero_Portrait_" +
                _callerPlayer.SelfHero.HeroElement + "_EXP");

            AttackingPictureObject.SetActive(true);

            _attackedCreatureObj = CreateCardPreview(_skillUsedOnUnit.Card, Vector3.right * 6);
            _attackingPlayerObj = CreatePlayerPreview(_callerPlayer, Vector3.zero);

            GameObject cardView = _attackedCreatureObj.transform.Find("AttackingHealth").gameObject;
            cardView.SetActive(true);
            TextMeshPro damageText = cardView.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_skillValue).ToString();
            cardView.transform.localPosition = -Vector3.up * 3;
        }
    }
}
