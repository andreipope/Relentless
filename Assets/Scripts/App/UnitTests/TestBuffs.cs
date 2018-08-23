using LoomNetwork.CZB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBuffs : MonoBehaviour
{

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            foreach(var item in GameClient.Get<IGameplayManager>().CurrentPlayer.BoardCards)
            {
                item.ClearBuffs();
                item.BuffUnit(LoomNetwork.CZB.Common.Enumerators.BuffType.RUSH);
                //item.ApplyBuffs();
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var item in GameClient.Get<IGameplayManager>().CurrentPlayer.BoardCards)
            {
                item.ClearBuffs();
                //item.ApplyBuffs();
            }
        }
    }
}