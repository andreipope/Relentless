using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GrandDevs.CZB
{
    public class TestActuiosnQueue : MonoBehaviour
    {

        private

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.J))
            {
                for (int i = 0; i < 1000; i++)
                {
                    GameClient.Get<IGameplayManager>().GetController<ActionsQueueController>().AddNewActionInToQueue((x, y) =>
                  {
                      var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                      obj.name = Time.time.ToString();

                      y?.Invoke();

                  }, null);
                }
            }

        }
    }
}