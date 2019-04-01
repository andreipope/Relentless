using UnityEngine;

#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class OpponentHandCardView : ICardView
    {
        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public CardModel Model { get; }

        public OpponentHandCardView(GameObject selfObject, CardModel model)
        {
            GameObject = selfObject;
            Model = model;

#if UNITY_EDITOR
            MainApp.Instance.OnDrawGizmosCalled += OnDrawGizmos;
#endif
        }

        public void Dispose()
        {
            Object.Destroy(GameObject);
        }

        public override string ToString()
        {
            return $"([{nameof(OpponentHandCardView)}] {nameof(Model)}: {Model})";
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (GameObject == null)
            {
                MainApp.Instance.OnDrawGizmosCalled -= OnDrawGizmos;
                return;
            }

            if (Model == null)
                return;

            DebugCardInfoDrawer.Draw(Transform.position, Model.Card.InstanceId.Id, Model.Card.Prototype.Name);
        }
#endif
    }
}
