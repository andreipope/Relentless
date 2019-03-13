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

        public BoardUnitModel BoardUnitModel { get; }

        public OpponentHandCard(GameObject selfObject, BoardUnitModel boardUnitModel)
        {
            GameObject = selfObject;
            BoardUnitModel = boardUnitModel;

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

            if (BoardUnitModel == null)
                return;

            DebugCardInfoDrawer.Draw(Transform.position, BoardUnitModel.Card.InstanceId.Id, BoardUnitModel.Card.Prototype.Name);
        }
#endif
    }
}
