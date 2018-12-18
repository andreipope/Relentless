using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf;

namespace Loom.ZombieBattleground.BackendCommunication
{
    public interface IContractCallProxy : IDisposable
    {
        Task CallAsync(string method, IMessage args);
        Task<T> CallAsync<T>(string method, IMessage args) where T : IMessage, new();
        Task<T> StaticCallAsync<T>(string method, IMessage args) where T : IMessage, new();
    }
}
