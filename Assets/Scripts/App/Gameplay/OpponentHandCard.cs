using Loom.ZombieBattleground.View;
using UnityEngine;

#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class OpponentHandCard : IView
    {
        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public WorkingCard WorkingCard { get; }

        public OpponentHandCard(GameObject selfObject, WorkingCard card)
        {
            GameObject = selfObject;
            WorkingCard = card;

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
        }

        public void Dispose()
        {
            Object.Destroy(GameObject);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (GameObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            if (WorkingCard == null)
                return;

            DebugCardInfoDrawer.Draw(Transform.position, WorkingCard.InstanceId.Id, WorkingCard.Prototype.Name);
        }
#endif
    }
}
