using TMPro;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ReportViewBaseAttackPlayerByCreature : ReportViewBase
    {
        private BoardUnitModel _attackingCreature;

        private Player _attackedPlayer;

        private GameObject _attackingCreatureObj, _attackedPlayerObj;

        public ReportViewBaseAttackPlayerByCreature(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _attackingCreature = GameAction.Parameters[0] as BoardUnitModel;
            _attackedPlayer = GameAction.Parameters[1] as Player;
            _attackingCreatureObj = CreateCardPreview(_attackingCreature.Card, Vector3.zero, out BoardCard attackingBoardCard);
            _attackedPlayerObj = CreatePlayerPreview(_attackedPlayer, new Vector3(5f, 0, 0));
            PreviewImage.sprite = attackingBoardCard.PictureSprite.sprite;

            AttackingPictureObject.SetActive(true);

            GameObject attackViewPlayer = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_attackingCreature.CurrentDamage).ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up;
        }
    }
}
