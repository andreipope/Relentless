using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class ReportViewBaseAttackPlayerByCreature : ReportViewBase
    {
        private BoardUnit _attackingCreature;

        private Player _attackedPlayer;

        private GameObject _attackingCreatureObj, _attackedPlayerObj;

        public ReportViewBaseAttackPlayerByCreature(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _attackingCreature = gameAction.parameters[0] as BoardUnit;
            _attackedPlayer = gameAction.parameters[1] as Player;
            previewImage.sprite = _attackingCreature.sprite;
            _attackingCreatureObj = CreateCardPreview(_attackingCreature.Card, Vector3.zero);
            _attackedPlayerObj = CreatePlayerPreview(_attackedPlayer, new Vector3(5f, 0, 0));

            attackingPictureObject.SetActive(true);

            GameObject attackViewPlayer = _attackedPlayerObj.transform.Find("AttackingHealth").gameObject;
            attackViewPlayer.SetActive(true);
            TextMeshPro damageText = attackViewPlayer.transform.Find("AttackText").GetComponent<TextMeshPro>();
            damageText.text = (-_attackingCreature.CurrentDamage).ToString();
            attackViewPlayer.transform.localPosition = -Vector3.up;
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
