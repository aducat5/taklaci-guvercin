using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TaklaciGuvercin.Shared.DTOs;

namespace TaklaciGuvercin.Api
{
    /// <summary>
    /// SignalR client for Unity using WebSocket with SignalR JSON protocol.
    /// Connects to the AirspaceHub for real-time flight and encounter events.
    /// </summary>
    public class SignalRClient : MonoBehaviour
    {
        private static SignalRClient _instance;
        public static SignalRClient Instance => _instance;

        [SerializeField] private string hubUrl = "ws://localhost:5000/hubs/airspace";
        [SerializeField] private float reconnectDelay = 5f;
        [SerializeField] private float heartbeatInterval = 15f;

        // Connection state
        private WebSocketWrapper _webSocket;
        private bool _isConnected;
        private bool _isReconnecting;
        private string _connectionId;
        private Coroutine _heartbeatCoroutine;

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;

        // Game Events
        public event Action<FlightSessionDto> OnFlightStarted;
        public event Action<string> OnFlightEnded;
        public event Action<FlightPositionUpdate> OnPositionUpdated;
        public event Action<EncounterNotification> OnEncounterDetected;
        public event Action<EncounterResultNotification> OnEncounterResult;

        public bool IsConnected => _isConnected;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public void Connect()
        {
            if (_isConnected || _isReconnecting) return;
            StartCoroutine(NegotiateAndConnect());
        }

        public void Disconnect()
        {
            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }

            _isConnected = false;
            _webSocket?.Close();
            _webSocket = null;
            OnDisconnected?.Invoke();
        }

        private IEnumerator NegotiateAndConnect()
        {
            // SignalR negotiate endpoint
            string negotiateUrl = hubUrl.Replace("ws://", "http://").Replace("wss://", "https://") + "/negotiate?negotiateVersion=1";

            using var request = UnityWebRequest.Post(negotiateUrl, "");
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SignalR negotiation failed: {request.error}");
                OnError?.Invoke($"Negotiation failed: {request.error}");
                yield break;
            }

            var negotiateResponse = JsonUtility.FromJson<NegotiateResponse>(request.downloadHandler.text);
            _connectionId = negotiateResponse.connectionId;

            // Connect via WebSocket
            string wsUrl = $"{hubUrl}?id={_connectionId}";
            yield return ConnectWebSocket(wsUrl);
        }

        private IEnumerator ConnectWebSocket(string url)
        {
            _webSocket = new WebSocketWrapper(url);

            _webSocket.OnOpen += HandleOpen;
            _webSocket.OnMessage += HandleMessage;
            _webSocket.OnClose += HandleClose;
            _webSocket.OnError += HandleError;

            yield return _webSocket.Connect();
        }

        private void HandleOpen()
        {
            Debug.Log("SignalR WebSocket connected");

            // Send handshake (SignalR JSON protocol)
            var handshake = new SignalRHandshake { protocol = "json", version = 1 };
            string handshakeJson = JsonUtility.ToJson(handshake) + "\u001e"; // Record separator
            _webSocket.Send(handshakeJson);

            _isConnected = true;
            _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
            OnConnected?.Invoke();
        }

        private void HandleMessage(string message)
        {
            // SignalR messages end with record separator \u001e
            var messages = message.Split('\u001e');

            foreach (var msg in messages)
            {
                if (string.IsNullOrEmpty(msg)) continue;

                try
                {
                    var signalRMessage = JsonUtility.FromJson<SignalRMessage>(msg);

                    if (signalRMessage.type == 1) // Invocation
                    {
                        HandleInvocation(signalRMessage);
                    }
                    else if (signalRMessage.type == 6) // Ping
                    {
                        // Respond with pong
                        _webSocket.Send("{\"type\":6}\u001e");
                    }
                    else if (signalRMessage.type == 7) // Close
                    {
                        Debug.Log("SignalR server requested close");
                        Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse SignalR message: {ex.Message}");
                }
            }
        }

        private void HandleInvocation(SignalRMessage message)
        {
            switch (message.target)
            {
                case "OnFlightStarted":
                    if (message.arguments != null && message.arguments.Length > 0)
                    {
                        var dto = JsonUtility.FromJson<FlightSessionDto>(message.arguments[0]);
                        OnFlightStarted?.Invoke(dto);
                    }
                    break;

                case "OnFlightEnded":
                    if (message.arguments != null && message.arguments.Length > 0)
                    {
                        OnFlightEnded?.Invoke(message.arguments[0]);
                    }
                    break;

                case "OnPositionUpdated":
                    if (message.arguments != null && message.arguments.Length > 0)
                    {
                        var update = JsonUtility.FromJson<FlightPositionUpdate>(message.arguments[0]);
                        OnPositionUpdated?.Invoke(update);
                    }
                    break;

                case "OnEncounterDetected":
                    if (message.arguments != null && message.arguments.Length > 0)
                    {
                        var notification = JsonUtility.FromJson<EncounterNotification>(message.arguments[0]);
                        OnEncounterDetected?.Invoke(notification);
                    }
                    break;

                case "OnEncounterResult":
                    if (message.arguments != null && message.arguments.Length > 0)
                    {
                        var result = JsonUtility.FromJson<EncounterResultNotification>(message.arguments[0]);
                        OnEncounterResult?.Invoke(result);
                    }
                    break;

                default:
                    Debug.Log($"Unknown SignalR method: {message.target}");
                    break;
            }
        }

        private void HandleClose(string reason)
        {
            Debug.Log($"SignalR WebSocket closed: {reason}");
            _isConnected = false;

            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }

            OnDisconnected?.Invoke();

            // Auto-reconnect
            if (!_isReconnecting)
            {
                StartCoroutine(Reconnect());
            }
        }

        private void HandleError(string error)
        {
            Debug.LogError($"SignalR WebSocket error: {error}");
            OnError?.Invoke(error);
        }

        private IEnumerator HeartbeatLoop()
        {
            while (_isConnected)
            {
                yield return new WaitForSeconds(heartbeatInterval);
                if (_isConnected)
                {
                    // Send ping
                    _webSocket.Send("{\"type\":6}\u001e");
                }
            }
        }

        private IEnumerator Reconnect()
        {
            _isReconnecting = true;
            Debug.Log($"Attempting to reconnect in {reconnectDelay} seconds...");
            yield return new WaitForSeconds(reconnectDelay);
            _isReconnecting = false;
            Connect();
        }

        #region Hub Methods

        public void JoinPlayerGroup(string playerId)
        {
            SendInvocation("JoinPlayerGroup", playerId);
        }

        public void LeavePlayerGroup(string playerId)
        {
            SendInvocation("LeavePlayerGroup", playerId);
        }

        public void JoinActiveFlights()
        {
            SendInvocation("JoinActiveFlights");
        }

        public void LeaveActiveFlights()
        {
            SendInvocation("LeaveActiveFlights");
        }

        public void UpdatePosition(string sessionId, double latitude, double longitude, double altitude)
        {
            var update = new FlightPositionUpdate
            {
                SessionId = sessionId,
                Latitude = latitude,
                Longitude = longitude,
                Altitude = altitude
            };
            SendInvocation("UpdatePosition", JsonUtility.ToJson(update));
        }

        private void SendInvocation(string method, params string[] args)
        {
            if (!_isConnected) return;

            var invocation = new SignalRInvocation
            {
                type = 1,
                target = method,
                arguments = args
            };

            string json = JsonUtility.ToJson(invocation) + "\u001e";
            _webSocket.Send(json);
        }

        #endregion

        #region Helper Classes

        [Serializable]
        private class NegotiateResponse
        {
            public string connectionId;
            public string connectionToken;
            public string[] availableTransports;
        }

        [Serializable]
        private class SignalRHandshake
        {
            public string protocol;
            public int version;
        }

        [Serializable]
        private class SignalRMessage
        {
            public int type;
            public string target;
            public string[] arguments;
            public string error;
        }

        [Serializable]
        private class SignalRInvocation
        {
            public int type;
            public string target;
            public string[] arguments;
        }

        #endregion
    }

    /// <summary>
    /// Simple WebSocket wrapper for Unity.
    /// Uses ClientWebSocket internally with coroutine-based async.
    /// </summary>
    public class WebSocketWrapper
    {
        private readonly string _url;
        private System.Net.WebSockets.ClientWebSocket _ws;
        private System.Threading.CancellationTokenSource _cts;
        private bool _isConnected;

        public event Action OnOpen;
        public event Action<string> OnMessage;
        public event Action<string> OnClose;
        public event Action<string> OnError;

        public WebSocketWrapper(string url)
        {
            _url = url;
        }

        public IEnumerator Connect()
        {
            _ws = new System.Net.WebSockets.ClientWebSocket();
            _cts = new System.Threading.CancellationTokenSource();

            var connectTask = _ws.ConnectAsync(new Uri(_url), _cts.Token);

            while (!connectTask.IsCompleted)
            {
                yield return null;
            }

            if (connectTask.IsFaulted)
            {
                OnError?.Invoke(connectTask.Exception?.Message ?? "Connection failed");
                yield break;
            }

            _isConnected = true;
            OnOpen?.Invoke();

            // Start receiving
            StartReceiving();
        }

        private async void StartReceiving()
        {
            var buffer = new byte[4096];

            try
            {
                while (_ws.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        OnClose?.Invoke("Server closed connection");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Handle fragmented messages
                    while (!result.EndOfMessage)
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        message += Encoding.UTF8.GetString(buffer, 0, result.Count);
                    }

                    OnMessage?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    OnError?.Invoke(ex.Message);
                    OnClose?.Invoke(ex.Message);
                }
            }
        }

        public async void Send(string message)
        {
            if (_ws.State != System.Net.WebSockets.WebSocketState.Open) return;

            var bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, _cts.Token);
        }

        public async void Close()
        {
            _isConnected = false;

            if (_ws != null && _ws.State == System.Net.WebSockets.WebSocketState.Open)
            {
                try
                {
                    await _ws.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Client closing", _cts.Token);
                }
                catch { }
            }

            _cts?.Cancel();
            _ws?.Dispose();
            _ws = null;
        }
    }
}
