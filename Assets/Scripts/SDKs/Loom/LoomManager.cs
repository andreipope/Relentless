
using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

public partial class LoomManager
{
    private static readonly string UserDataFileName = Application.persistentDataPath+"/UserData.json";

    private static LoomManager _instance;
    private LoomManager()
    {
        LoomXCommandHandlers.Initialize();
    }

    #if UNITY_EDITOR
    private string _writerHost= "ws://127.0.0.1:46657/websocket";
    private string _readerHost = "ws://127.0.0.1:9999/queryws";
    #else
    private string _writerHost= "ws://battleground-testnet-asia1.dappchains.com:46657/websocket";
    private string _readerHost = "ws://battleground-testnet-asia1.dappchains.com:9999/queryws";
    #endif

    public delegate void ContractCreatedEventHandler(Contract oldContract, Contract newContract);
    public event ContractCreatedEventHandler ContractCreated;

    public LoomUserDataModel UserDataModel { get; set; }

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
    
    public async Task CreateContract() 
    {
        LoadUserDataModel();
        var publicKey = CryptoUtils.PublicKeyFromPrivateKey(UserDataModel.PrivateKey);
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
            new SignedTxMiddleware(UserDataModel.PrivateKey)
        });

        var contractAddr = await client.ResolveContractAddressAsync("ZombieBattleground");
        Contract oldContract = Contract;
        Contract = new Contract(client, contractAddr, callerAddr);
        ContractCreated?.Invoke(oldContract, Contract);
    }

    public bool LoadUserDataModel(bool force = false) {
        if (UserDataModel != null && !force)
            return true;

        if (!File.Exists(UserDataFileName))
            return false;

        UserDataModel = JsonConvert.DeserializeObject<LoomUserDataModel>(File.ReadAllText(UserDataFileName));
        return true;
    }

    public bool SetUserDataModel(LoomUserDataModel userDataModel) {
        if (userDataModel == null)
            throw new ArgumentNullException(nameof(userDataModel));

        File.WriteAllText(UserDataFileName, JsonConvert.SerializeObject(userDataModel));
        UserDataModel = userDataModel;
        return true;
    }
}