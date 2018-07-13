
using System;
using Google.Protobuf;
using Loom.Unity3d;
using Loom.Unity3d.Zb;

public class LoomManager
{
    private static LoomManager _instance;
    private LoomManager(){}
    
    private const string Createaccount = "CreateAccount";
    private const string GetDeckData = "GetDecks"; 
    
    public static LoomManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = new LoomManager();

            return _instance;
        }
    }

    public void SignUp(UpsertAccountRequest accountTx, Action<BroadcastTxResult> result)
    {
        SetMessage<UpsertAccountRequest>(Createaccount, accountTx, result);
    }

    public void GetDecks(GetDeckRequest deckRequest, Action<IMessage> result)
    {
        GetMessage<UserDecks>(GetDeckData, deckRequest, result);  
    }
    
    
    
    private void SetMessage<T>(string methodName, IMessage msg, Action<BroadcastTxResult> result) where T : IMessage, new()
    {
        LoomX.SetMessageEcho<T>(methodName, msg, result);
    }
    

    private void GetMessage<T>(string methodName, IMessage entry, Action<IMessage> result) where T : IMessage, new()
    {
        LoomX.GetMessage<T>(methodName, entry, result);
    }
}
       