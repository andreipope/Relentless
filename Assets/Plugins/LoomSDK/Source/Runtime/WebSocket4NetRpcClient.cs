#if !UNITY_WEBGL || UNITY_EDITOR

using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Loom.Client.Internal;
using Loom.Newtonsoft.Json;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using WebSocketState = WebSocket4Net.WebSocketState;

namespace Loom.Client
{
    /// <summary>
    /// WebSocket JSON-RPC client implemented with WebSocketSharp.
    /// </summary>
    public class WebSocket4NetRpcClient : BaseRpcClient
    {
        private const string LogTag = "Loom.WebSocket4NetRpcClient";

        private readonly WebSocket webSocket;
        private readonly Uri url;

        private event EventHandler<JsonRpcEventData> eventReceived;

        public override RpcConnectionState ConnectionState {
            get
            {
                switch (webSocket.State)
                {
                    case WebSocketState.None:
                        return RpcConnectionState.Invalid;
                    case WebSocketState.Connecting:
                        return RpcConnectionState.Connecting;
                    case WebSocketState.Open:
                        return RpcConnectionState.Connected;
                    case WebSocketState.Closing:
                        return RpcConnectionState.Disconnecting;
                    case WebSocketState.Closed:
                        return RpcConnectionState.Disconnected;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public WebSocket4NetRpcClient(string url)
        {
            this.url = new Uri(url);
            this.webSocket = new WebSocket(url);
            this.webSocket.Error += WebSocketOnError;
            this.webSocket.Opened += WebSocketOnOpen;
            this.webSocket.Closed += WebSocketOnClose;
        }

        public override async Task<TResult> SendAsync<TResult, TArgs>(string method, TArgs args)
        {
            var tcs = new TaskCompletionSource<TResult>();
            var msgId = Guid.NewGuid().ToString();
            EventHandler closeHandler = null;
            EventHandler<MessageReceivedEventArgs> messageHandler = null;
            closeHandler = (sender, e) =>
            {
                tcs.TrySetException(CreateExceptionForEvent(e));
            };

            messageHandler = (sender, e) =>
            {
                try
                {
                    // TODO: implement a more optimal way to handle data. Currently, each handler deserializes the payload independently,
                    // which means that if 20 simultaneous calls are made, up to 20 * 20 = 400 total deserializations can be made
                    if (!string.IsNullOrEmpty(e.Message))
                    {
                        this.Logger.Log("[Response Data] " + e.Message);
                        var partialMsg = JsonConvert.DeserializeObject<JsonRpcResponse>(e.Message);
                        if (partialMsg.Id == msgId)
                        {
                            this.webSocket.Closed -= closeHandler;
                            this.webSocket.MessageReceived -= messageHandler;
                            if (partialMsg.Error != null)
                            {
                                HandleJsonRpcResponseError(partialMsg);
                            }

                            var fullMsg = JsonConvert.DeserializeObject<JsonRpcResponse<TResult>>(e.Message);
                            tcs.TrySetResult(fullMsg.Result);
                        }
                    }
                    else
                    {
                        this.Logger.Log(LogTag, "[ignoring msg]");
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            this.webSocket.Closed += closeHandler;
            this.webSocket.MessageReceived += messageHandler;
            try
            {
                await SendAsync(method, args, msgId);
            }
            catch
            {
                this.webSocket.Closed -= closeHandler;
                this.webSocket.MessageReceived -= messageHandler;
                throw;
            }
            return await tcs.Task;
        }

        public override Task ConnectAsync()
        {
            AssertNotAlreadyConnectedOrConnecting();
            var tcs = new TaskCompletionSource<object>();
            EventHandler openHandler = null;
            EventHandler closeHandler = null;
            EventHandler<ErrorEventArgs> errorHandler = null;
            openHandler = (sender, e) =>
            {
                this.webSocket.Opened -= openHandler;
                this.webSocket.Closed -= closeHandler;
                this.webSocket.Error -= errorHandler;
                tcs.TrySetResult(null);
                this.Logger.Log(LogTag, "Connected to " + this.url.AbsoluteUri);
            };
            closeHandler = (sender, e) =>
            {
                this.webSocket.Opened -= openHandler;
                this.webSocket.Closed -= closeHandler;
                this.webSocket.Error -= errorHandler;
                if (tcs.Task.IsCompleted)
                    return;

                tcs.SetException(CreateExceptionForEvent(e));
            };
            errorHandler = (sender, e) =>
            {
                this.webSocket.Opened -= openHandler;
                this.webSocket.Closed -= closeHandler;
                this.webSocket.Error -= errorHandler;
                if (tcs.Task.IsCompleted)
                    return;

                tcs.SetException(CreateExceptionForEvent(e));
            };
            this.webSocket.Opened += openHandler;
            this.webSocket.Closed += closeHandler;
            this.webSocket.Error += errorHandler;
            this.webSocket.Open();
            return tcs.Task;
        }

        public override Task DisconnectAsync()
        {
            // TODO: should be listening for disconnection all the time
            // and auto-reconnect if there are event subscriptions
            var tcs = new TaskCompletionSource<ClosedEventArgs>();
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                ClosedEventArgs closedEventArgs = (ClosedEventArgs) e;
                this.webSocket.Closed -= handler;
                tcs.TrySetResult(closedEventArgs);
            };
            this.webSocket.Closed += handler;
            try
            {
                this.webSocket.Close("Client disconnected.");
            }
            catch
            {
                this.webSocket.Closed -= handler;
                throw;
            }
            return tcs.Task;
        }

        public override Task SubscribeAsync(EventHandler<JsonRpcEventData> handler, ICollection<string> topics)
        {
            var isFirstSub = this.eventReceived == null;
            this.eventReceived += handler;
            if (isFirstSub)
            {
                this.webSocket.MessageReceived += WebSocket4NetRpcClient_OnMessage;
            }
            // TODO: once re-sub on reconnect is implemented this should only
            // be done on first sub
            Dictionary<string, ICollection<string>> args = null;
            if (topics != null && topics.Count > 0)
            {
                args = new Dictionary<string, ICollection<string>>();
                args.Add("topics", topics);
            }

            return SendAsync<object, Dictionary<string, ICollection<string>>>("subevents", args);
        }

        public override Task UnsubscribeAsync(EventHandler<JsonRpcEventData> handler)
        {
            this.eventReceived -= handler;
            if (this.eventReceived == null)
            {
                this.webSocket.MessageReceived -= WebSocket4NetRpcClient_OnMessage;
                return SendAsync<object, object>("unsubevents", null);
            }
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                this.webSocket.Error -= WebSocketOnError;
                this.webSocket.Opened -= WebSocketOnOpen;
                this.webSocket.Closed -= WebSocketOnClose;
                this.webSocket.Dispose();
            }

            this.disposed = true;
        }

        private void WebSocketOnClose(object sender, EventArgs e)
        {
            NotifyConnectionStateChanged();
        }

        private void WebSocketOnOpen(object sender, EventArgs e)
        {
            NotifyConnectionStateChanged();
        }

        private void WebSocketOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            this.Logger.Log(LogTag, "Error: " + errorEventArgs.Exception.Message);
            NotifyConnectionStateChanged();
        }

        private async Task SendAsync<T>(string method, T args, string msgId)
        {
            AssertIsConnected();
            var tcs = new TaskCompletionSource<object>();
            var reqMsg = new JsonRpcRequest<T>(method, args, msgId);
            var reqMsgBody = JsonConvert.SerializeObject(reqMsg);
            this.Logger.Log(LogTag, "[Request Body] " + reqMsgBody);

            ErrorEventArgs errorEventArgs = null;
            EventHandler<ErrorEventArgs> errorHandler = (sender, eventArgs) =>
            {
                errorEventArgs = eventArgs;
            };
            this.webSocket.Error += errorHandler;
            try
            {
                this.webSocket.Send(reqMsgBody);
                tcs.TrySetResult(null);
            }
            catch (Exception e)
            {
                tcs.TrySetException(new RpcClientException("Send error", errorEventArgs?.Exception ?? e, 1,this));
            }
            this.webSocket.Error -= errorHandler;
            await tcs.Task;
        }

        private void WebSocket4NetRpcClient_OnMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    this.Logger.Log(LogTag, "[WebSocket4NetRpcClient_OnMessage msg body] " + e.Message);
                    var partialMsg = JsonConvert.DeserializeObject<JsonRpcResponse>(e.Message);
                    if (partialMsg.Id == "0")
                    {
                        if (partialMsg.Error != null)
                        {
                            HandleJsonRpcResponseError(partialMsg);
                        }
                        else
                        {
                            var fullMsg = JsonConvert.DeserializeObject<JsonRpcEvent>(e.Message);
                            this.eventReceived?.Invoke(this, fullMsg.Result);
                        }
                    }
                }
                else
                {
                    this.Logger.Log(LogTag, "[WebSocket4NetRpcClient_OnMessage ignoring msg]");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(LogTag, "[WebSocket4NetRpcClient_OnMessage error] " + ex);
            }
        }

        private RpcClientException CreateExceptionForEvent(EventArgs eventArgs)
        {
            if (eventArgs is ClosedEventArgs closedEventArgs)
            {
                return new RpcClientException($"WebSocket closed unexpectedly with error {closedEventArgs.Code}: {closedEventArgs.Reason}", closedEventArgs.Code, this);
            } else if (eventArgs is ErrorEventArgs errorEventArgs)
            {
                return new RpcClientException("WebSocket closed unexpectedly with error", errorEventArgs.Exception, -1,this);
            }
            else
            {
                return new RpcClientException("WebSocket closed unexpectedly with unknown error", -1, this);
            }
        }
    }
}

#endif
