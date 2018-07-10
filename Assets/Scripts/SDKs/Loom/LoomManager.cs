
using System;
using GrandDevs.CZB.Data;
using Loom.Unity3d;
using UnityEngine.Events;

public class LoomManager
{
    private static LoomManager _instance;

    private LoomManager(){}
    
    public static LoomManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = new LoomManager();

            return _instance;
        }
    }

    public void SignUp(CreateAccountRequest accountTx, Action<BroadcastTxResult> result)
    {
        LoomX.SignUp(accountTx, result);
    }

    public void SignIn(UnityAction<bool> result)
    {
        
    }

    public void SetMessageWithResult(MapEntry entry, Action<BroadcastTxResult> result)
    {
        LoomX.SetMessageEcho(entry, result);
    }

    public void GetMessage(MapEntry entry, Action<MapEntry> result)
    {
        LoomX.GetMessage(entry, result);
    }

    public DecksData GetDeckData()
    {
        LoomX.SetMessageEcho();
    }
}
