using System.Collections.Generic;

namespace LoomNetwork.CZB.BackendCommunication
{
    public class ActionLogModel
    {
        // ReSharper disable once CollectionNeverQueried.Global
        public Dictionary<string, object> LogData { get; } = new Dictionary<string, object>();

        public ActionLogModel Add(string key, object value)
        {
            LogData.Add(key, value);
            return this;
        }
    }
}
