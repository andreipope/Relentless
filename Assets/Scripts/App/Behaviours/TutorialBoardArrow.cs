using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class TutorialBoardArrow : BoardArrow
    {
        public bool IsEnabled;

        public void UpdateTargetPosition(Vector3 position)
        {
            TargetPosition = position;
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
                UpdateLength(TargetPosition);
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
