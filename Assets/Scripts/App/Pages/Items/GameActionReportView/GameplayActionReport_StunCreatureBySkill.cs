using UnityEngine;
using UnityEngine.EventSystems;

namespace LoomNetwork.CZB
{
    public class GameplayActionReport_StunCreatureBySkill : ReportViewBase
    {
        private Player _callerPlayer;

        private BoardUnit _skillUsedOnUnit;

        private GameObject _stunnedUnitPreviewObject, _skillOwnerObject;

        public GameplayActionReport_StunCreatureBySkill(GameObject prefab, Transform parent, GameActionReport gameAction)
            : base(prefab, parent, gameAction)
        {
        }

        public override void SetInfo()
        {
            base.SetInfo();

            _callerPlayer = gameAction.parameters[0] as Player;
            _skillUsedOnUnit = gameAction.parameters[1] as BoardUnit;

            previewImage.sprite = _skillUsedOnUnit.sprite;

            _skillOwnerObject = CreatePlayerPreview(_callerPlayer, Vector3.zero);
            _stunnedUnitPreviewObject = CreateCardPreview(_skillUsedOnUnit.Card, Vector3.right * 6);
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
