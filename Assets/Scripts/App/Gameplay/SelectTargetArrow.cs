using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SelectTargetArrow : MonoBehaviour
    {
        private const float DefaultArrowScale = 6.25f;

        private GameObject _selfObject;

        private GameObject _targetObjectsGroup, _rootObjectsGroup, _arrowObject;

        private Vector3 _fromPosition;

        public bool IsSelectingTarget { get; protected set; }

        public void Update()
        {
            if (IsSelectingTarget)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                UpdateArrow(mousePosition);
            }
        }

        public void UpdateArrow(Vector3 target)
        {
            target.z = 0;

            _targetObjectsGroup.transform.position = target;

            float angle = Mathf.Atan2(target.y - _fromPosition.y, target.x - _fromPosition.x) * Mathf.Rad2Deg - 90;

            _arrowObject.transform.eulerAngles = new Vector3(0, 180, -angle);
            _rootObjectsGroup.transform.eulerAngles = new Vector3(0, 180, -angle + 21f);

            float scale = Vector3.Distance(_fromPosition, target) / DefaultArrowScale;

            _arrowObject.transform.localScale = new Vector3(1, scale, 1);
        }

        public virtual void OnUnitSelected(BoardUnit unit)
        {
        }

        public virtual void OnUnitUnselected(BoardUnit unit)
        {
        }

        public virtual void OnPlayerSelected(Player player)
        {
        }

        public virtual void OnPlayerUnselected(Player player)
        {
        }

        private void Awake()
        {
            Init(new Vector3(0, -5f, 0));
        }

        private void Init(Vector3 from)
        {
            _fromPosition = from;
            _selfObject = gameObject;

            _targetObjectsGroup = _selfObject.transform.Find("Group_TargetObjects").gameObject;
            _rootObjectsGroup = _selfObject.transform.Find("Group_RootObjects").gameObject;
            _arrowObject = _selfObject.transform.Find("Arrow").gameObject;

            _rootObjectsGroup.transform.position = _fromPosition;
            _arrowObject.transform.position = _fromPosition;
        }
    }
}
