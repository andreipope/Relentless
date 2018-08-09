
using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using UnityEngine;
using Random = System.Random;

public partial class LoomManager
{
    private static readonly string PrivateKeyFileName = Application.persistentDataPath+"/PrivateKey.key";
    private static readonly string UserNameFileName = Application.persistentDataPath+"/UserName.txt";
    
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

    public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);
    public event ContractCreatedEventHandler ContractCreated;
    
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

    public bool IsConnected =>
        Contract != null &&
        Contract.Client.ReadClient.ConnectionState == RpcConnectionState.Connected &&
        Contract.Client.WriteClient.ConnectionState >= RpcConnectionState.Connecting;

    public static LoomManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = new LoomManager();

            return _instance;
        }
    }
    
    public async Task CreateContract(Action result = null)
    {
        var privateKey = GetPrivateKeyFromFile();
        var publicKey = CryptoUtils.PublicKeyFromPrivateKey(privateKey);
        var callerAddr = Address.FromPublicKey(publicKey);

        var writer = RpcClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithWebSocket(_writerHost)
            .Create();

        var reader = RpcClientFactory.Configure()
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
        Contract oldContract = Contract;
        Contract = new Contract(client, contractAddr, callerAddr);
        ContractCreated?.Invoke(oldContract, Contract);
        
        
        result?.Invoke();
    }

    public async Task SetUser()
    {
        if (!File.Exists(UserNameFileName))
        {
            CreateGuestUser();
            await SignUp(UserId, var => {});  
        }
        else
            UserId = File.ReadAllText(UserNameFileName);
        
        CustomDebug.Log("User = " + UserId);
    }

    private void CreateGuestUser()
    {
        var rand = new Random();
        var user = "LoomUser_" + rand.Next(1, 1000000);
        
        UserId = user;
        File.WriteAllText(UserNameFileName, UserId);
    }
    

    public static byte[] GetPrivateKeyFromFile()
    {
        byte[] privateKey;
        if (File.Exists(PrivateKeyFileName))
            privateKey = File.ReadAllBytes(PrivateKeyFileName);
        else
        {
            privateKey = CryptoUtils.GeneratePrivateKey();
            File.WriteAllBytes(PrivateKeyFileName, privateKey);
        }

        return privateKey;
    }
}
