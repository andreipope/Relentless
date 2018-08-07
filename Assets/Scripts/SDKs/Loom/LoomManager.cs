
using System;
using System.Threading.Tasks;
using Loom.Client;
using UnityEngine;
using Random = System.Random;

public partial class LoomManager
{
    private static LoomManager _instance;
    private LoomManager()
    {
        LoomXCommandHandlers.Initialize();
    }

    public static string UserId = "Loom";

    #if UNITY_EDITOR
    private string _writerHost= "ws://127.0.0.1:46657/websocket";
    private string _readerHost = "ws://127.0.0.1:9999/queryws";
    #else
    private string _writerHost= "ws://battleground-testnet-asia1.dappchains.com:46657/websocket";
    private string _readerHost = "ws://battleground-testnet-asia1.dappchains.com:9999/queryws";
    #endif
    
    public string WriteHost
    {
        get { return _writerHost; }
        set { _writerHost = value;  }
    }

    public string ReaderHost
    {
        get { return _readerHost; }
        set { _readerHost = value; }
    }

    public Contract Contract { get; private set; }

    public static LoomManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = new LoomManager();

            return _instance;
        }
    }
    
    public async Task Init(Action result = null)
    {
        //var privateKey = LoomX.GetPrivateKeyFromPlayerPrefs();
        var privateKey = LoomX.GetPrivateKeyFromFile();
        var publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        var callerAddr = Address.FromPublicKey(publicKey);

        var writer = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithWebSocket(_writerHost)
            .Create();

        var reader = RPCClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithWebSocket(_readerHost)
            .Create();

        var client = new DAppChainClient(writer, reader)
        {
            Logger = Debug.unityLogger
        };
        
        client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
            new NonceTxMiddleware(publicKey, client),
            new SignedTxMiddleware(privateKey)
        });

        var contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
        Contract = new Contract(client, contractAddr, callerAddr);
        
        result?.Invoke();
    }

    public async Task SetUser()
    {
        if (!PlayerPrefs.HasKey("User"))
        {
            CreateGuestUser();
            await SignUp(UserId, var => {});  
        }
        else
            UserId = PlayerPrefs.GetString("User");
        
        CustomDebug.Log("User = " + UserId);
    }

    private void CreateGuestUser()
    {
        var rand = new Random();
        var user = "LoomUser_" + rand.Next(1, 1000000);
        
        UserId = user;
        PlayerPrefs.SetString("User", UserId);
    }
}
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        