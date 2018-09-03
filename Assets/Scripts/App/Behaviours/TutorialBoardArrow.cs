using UnityEngine;

namespace LoomNetwork.CZB
{
    public class TutorialBoardArrow : BoardArrow
    {
        public bool IsEnabled;

        private Vector3 _targetPosition;

        public void UpdateTargetPosition(Vector3 position)
        {
            _targetPosition = position;
            Activate();
        }

        public void Activate()
        {
            IsEnabled = true;
            UpdateVisibility();
        }

        public void Deactivate()
        {
            IsEnabled = false;
            UpdateVisibility();
        }

        protected override void Update()
        {
            if (IsEnabled)
            {
                UpdateLength(_targetPosition);
            }
        }

        private void Awake()
        {
            Init();
        }

        private void UpdateVisibility()
        {
            gameObject.SetActive(IsEnabled);
        }
    }
}
