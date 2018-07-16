
using System;
using Google.Protobuf;
using Loom.Unity3d;
using Loom.Unity3d.Zb;

public class LoomManager
{
    private static LoomManager _instance;
    private LoomManager(){}
    
    private const string CreateAccountMethod = "CreateAccount";
    private const string GetDeckDataMethod = "GetDecks";
    private const string DeleteDeckMethod = "DeleteDeck";

    public const string UserId = "f";

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
        SetMessage<UpsertAccountRequest>(CreateAccountMethod, accountTx, result);
    }

    public void GetDecks(GetDeckRequest request, Action<IMessage> result)
    {
        GetMessage<UserDecks>(GetDeckDataMethod, request, result);  
    }

    public void DeleteDeck(DeleteDeckRequest request, Action<BroadcastTxResult> result)
    {
        SetMessage<DeleteDeckRequest>(DeleteDeckMethod, request, result);
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
       