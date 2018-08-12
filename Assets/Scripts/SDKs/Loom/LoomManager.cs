
using System;
using System.IO;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

public partial class LoomManager
{
    private const string UserDataFileName = "UserData.json";

    private static LoomManager _instance;

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

    protected string UserDataFilePath => Path.Combine(Application.persistentDataPath, UserDataFileName);

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

    public async Task LoadUserDataModelAndCreateContract()
    {
        LoadUserDataModel();
        Debug.Log("User Id: " + UserDataModel.UserId);
        await CreateContract(UserDataModel.PrivateKey);
    }
    
    public async Task CreateContract(byte[] privateKey) 
    {
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
    }

    public bool LoadUserDataModel(bool force = false) {
        if (UserDataModel != null && !force)
            return true;

        if (!File.Exists(UserDataFilePath))
            return false;

        UserDataModel = JsonConvert.DeserializeObject<LoomUserDataModel>(File.ReadAllText(UserDataFilePath));
        return true;
    }

    public bool SetUserDataModel(LoomUserDataModel userDataModel) {
        if (userDataModel == null)
            throw new ArgumentNullException(nameof(userDataModel));

        File.WriteAllText(UserDataFilePath, JsonConvert.SerializeObject(userDataModel));
        UserDataModel = userDataModel;
        return true;
    }
}