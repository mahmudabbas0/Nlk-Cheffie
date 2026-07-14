using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nlk_Cheffie_Print.Core.Net
{
    public class LocalLoginServer
    {
        private HttpListener? _listener;
        private bool _isRunning;
        private int _port;

        public event Action<string>? TokenReceived;

        public int Start(int port = 54321)
        {
            _port = port;
            _listener = new HttpListener();

            try
            {
                _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                // Port in use – fallback to a random free port
                _port = GetFreePort();
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
                _listener.Start();
            }

            _isRunning = true;
            Task.Run(() => ListenLoop());

            return _port;
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch
            {
                // Ignore listener close errors
            }
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    // Handle each request on a background thread so the loop stays alive
                    _ = Task.Run(() => ProcessRequest(context));
                }
                catch
                {
                    // Listener was stopped or faulted
                    break;
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            var request  = context.Request;
            var response = context.Response;

            if (request.Url != null && request.Url.AbsolutePath == "/callback")
            {
                string? token = request.QueryString["token"];

                response.StatusCode  = (int)HttpStatusCode.OK;
                response.ContentType = "text/html; charset=utf-8";

                string html;
                if (!string.IsNullOrEmpty(token))
                {
                    html = @"<!DOCTYPE html>
<html lang='tr'>
<head>
  <meta charset='utf-8'>
  <title>Giriş Başarılı</title>
  <style>
    * { margin:0; padding:0; box-sizing:border-box; }
    body {
      font-family: 'Segoe UI', Tahoma, sans-serif;
      display: flex; justify-content: center; align-items: center;
      height: 100vh;
      background: #0F0F10; color: #fff;
    }
    .card {
      text-align: center;
      background: #161618;
      padding: 48px 56px;
      border-radius: 16px;
      box-shadow: 0 8px 32px rgba(0,0,0,.6);
      border: 1px solid #2C2C2E;
    }
    .icon { font-size: 3rem; margin-bottom: 16px; }
    h1   { color: #4CAF50; margin-bottom: 12px; font-size: 1.6rem; }
    p    { color: #AEAEB2; font-size: 1rem; line-height: 1.6; }
  </style>
</head>
<body>
  <div class='card'>
    <div class='icon'>✅</div>
    <h1>Giriş Başarılı!</h1>
    <p>Cheffie POS masaüstü uygulamasına<br>başarıyla giriş yaptınız.</p>
    <p style='margin-top:10px;'>Bu pencereyi kapatıp uygulamaya dönebilirsiniz.</p>
    <script>setTimeout(function(){ window.close(); }, 3000);</script>
  </div>
</body>
</html>";
                }
                else
                {
                    html = @"<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='utf-8'><title>Hata</title>
<style>body{background:#0F0F10;color:#fff;text-align:center;padding-top:80px;font-family:'Segoe UI';}</style>
</head>
<body><h1 style='color:#f44'>Yetkilendirme Hatası</h1><p>Geçerli bir token alınamadı. Lütfen tekrar deneyin.</p></body>
</html>";
                }

                byte[] buffer = Encoding.UTF8.GetBytes(html);
                response.ContentLength64 = buffer.Length;

                try
                {
                    // 1. Write the full response body
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    // 2. Flush & close the response so the browser receives the page
                    response.OutputStream.Close();
                    response.Close();
                }
                catch
                {
                    // Client disconnected early – ignore
                }

                // 3. ONLY after the response is fully sent, fire the token event & stop the server
                if (!string.IsNullOrEmpty(token))
                {
                    // Small delay to ensure the browser has received the bytes
                    await Task.Delay(200);
                    TokenReceived?.Invoke(token);
                    Stop();
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
            }
        }

        private static int GetFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
