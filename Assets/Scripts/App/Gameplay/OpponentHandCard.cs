using Loom.ZombieBattleground.View;
using UnityEngine;

#if UNITY_EDITOR
using ZombieBattleground.Editor.Runtime;
#endif

namespace Loom.ZombieBattleground
{
    public class OpponentHandCard : IView, IBoardUnitView
    {
        public Transform Transform => GameObject.transform;

        public GameObject GameObject { get; }

        public BoardUnitModel Model { get; }

        public OpponentHandCard(GameObject selfObject, BoardUnitModel model)
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
