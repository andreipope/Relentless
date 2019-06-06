using System;
using System.Net;
using Plugins.AsyncAwaitUtil.Source;

namespace Loom.ZombieBattleground
{
    public static class HttpResponseMessageExtensions
    {
        public static void ThrowOnError(this HttpResponseMessage httpResponseMessage, WebrequestCreationInfo creationInfo)
        {
            if (!httpResponseMessage.IsSuccessStatusCode || !String.IsNullOrEmpty(httpResponseMessage.Error))
            {
                string response = httpResponseMessage.ReadToEnd();
                if (!String.IsNullOrWhiteSpace(response))
                {
                    response = ", response:\n" + response;
                }

                throw new WebException($"{creationInfo.Method} call to {creationInfo.Url} failed with error: '{httpResponseMessage.Error}' (code {(int) httpResponseMessage.StatusCode}){response}");
            }
        }
    }
}
