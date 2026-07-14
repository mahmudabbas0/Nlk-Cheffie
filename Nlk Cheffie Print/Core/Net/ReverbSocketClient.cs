using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Core.Net
{
    public class ReverbSocketClient
    {
        private const string AppKey = "l9mzLHx5RtNAE0TuZoQFXehWVKI4OgcS";
        private const string Host = "api.nlkmenu.com";
        private ClientWebSocket? _webSocket;
        private CancellationTokenSource? _cts;
        private bool _isConnected;
        private string _restaurantId = "";

        public event Action<JsonElement, bool>? PrintJobReceived;
        public event Action<bool>? ConnectionStatusChanged;

        public void Start(string restaurantId)
        {
            _restaurantId = restaurantId;
            if (string.IsNullOrEmpty(_restaurantId)) return;

            _cts = new CancellationTokenSource();
            _isConnected = true;
            Task.Run(() => ConnectAndListenLoop(_cts.Token));
        }

        public void Stop()
        {
            _isConnected = false;
            _cts?.Cancel();
            _webSocket?.Dispose();
            ConnectionStatusChanged?.Invoke(false);
        }

        private async Task ConnectAndListenLoop(CancellationToken cancellationToken)
        {
            string wsUri = $"wss://{Host}/app/{AppKey}?protocol=7&client=js&version=7.0.3";

            while (_isConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _webSocket = new ClientWebSocket();
                    _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

                    // Add headers matching Python User-Agent
                    _webSocket.Options.SetRequestHeader("User-Agent", "CheffiePOS-PrintBridge/1.0");

                    await _webSocket.ConnectAsync(new Uri(wsUri), cancellationToken);
                    ConnectionStatusChanged?.Invoke(true);

                    // Subscribe to the restaurant room
                    await SubscribeToChannel($"restaurant:{_restaurantId}", cancellationToken);

                    // Receive loop
                    byte[] buffer = new byte[8192];
                    while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                    {
                        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                            break;
                        }

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            ProcessWsMessage(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log connection failure
                    File.AppendAllText(GetLogFilePath(), $"[WS ERROR] {DateTime.Now}: {ex.Message}\n");
                }
                finally
                {
                    ConnectionStatusChanged?.Invoke(false);
                    _webSocket?.Dispose();
                    _webSocket = null;
                }

                // Wait before reconnecting (exponential backoff / delay)
                if (_isConnected)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        private async Task SubscribeToChannel(string channelName, CancellationToken cancellationToken)
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;

            var payload = new
            {
                @event = "pusher:subscribe",
                data = new
                {
                    channel = channelName
                }
            };

            string json = JsonSerializer.Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            File.AppendAllText(GetLogFilePath(), $"[WS SUBSCRIBE] Subscribed to {channelName}\n");
        }

        private void ProcessWsMessage(string jsonMessage)
        {
            try
            {
                using (var doc = JsonDocument.Parse(jsonMessage))
                {
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("event", out var eventProp)) return;

                    string eventName = eventProp.GetString() ?? "";

                    // Pusher Keepalive Heartbeat ping
                    if (eventName == "pusher:ping")
                    {
                        SendPong();
                        return;
                    }

                    if (eventName == "NewPrintJob" || eventName == "CancelPrintJob")
                    {
                        if (root.TryGetProperty("data", out var dataProp))
                        {
                            string dataStr = dataProp.GetString() ?? "";
                            if (!string.IsNullOrEmpty(dataStr))
                            {
                                using (var dataDoc = JsonDocument.Parse(dataStr))
                                {
                                    bool isCancel = (eventName == "CancelPrintJob");
                                    PrintJobReceived?.Invoke(dataDoc.RootElement.Clone(), isCancel);
                                    File.AppendAllText(GetLogFilePath(), $"[WS EVENT] Received {eventName} job\n");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(GetLogFilePath(), $"[WS PARSE ERROR] {ex.Message} for message: {jsonMessage}\n");
            }
        }

        private async void SendPong()
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open) return;
            try
            {
                string payload = "{\"event\":\"pusher:pong\",\"data\":{}}";
                byte[] bytes = Encoding.UTF8.GetBytes(payload);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch
            {
                // Ignore send errors
            }
        }

        private string GetLogFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = Path.Combine(appData, "NlkCheffiePrint", "logs");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "ws_client.log");
        }
    }
}
