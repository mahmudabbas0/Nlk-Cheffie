using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Printer;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Core.Workers
{
    public class PrintQueueWorker
    {
        private static readonly ConcurrentQueue<PrintJob> Queue = new ConcurrentQueue<PrintJob>();
        private static readonly SemaphoreSlim Signal = new SemaphoreSlim(0);
        private static HashSet<string> _printedJobIds = new HashSet<string>();
        private static readonly ConcurrentDictionary<string, byte> CancelledJobIds = new ConcurrentDictionary<string, byte>();
        private static bool _isRunning;
        private static CancellationTokenSource? _cts;

        public static event Action<string, bool>? JobProcessed; // (message, isError)

        public static void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = new CancellationTokenSource();
            
            LoadDeduplicationHistory();
            Task.Run(() => ProcessQueueLoop(_cts.Token));
            AppendLog("PrintQueueWorker background consumer started.");
        }



        public static void Stop()
        {
            _isRunning = false;
            _cts?.Cancel();
        }

        public static void Enqueue(PrintJob job)
        {
            Queue.Enqueue(job);
            Signal.Release();
            AppendLog($"[QUEUED] JobId={job.JobId} Role={job.Role}");
        }

        public static void Cancel(string jobId)
        {
            if (string.IsNullOrWhiteSpace(jobId)) return;
            CancelledJobIds.TryAdd(jobId, 0);
            AppendLog($"[CANCELLED] JobId={jobId}");
        }

        private static async Task ProcessQueueLoop(CancellationToken token)
        {
            while (_isRunning && !token.IsCancellationRequested)
            {
                try
                {
                    await Signal.WaitAsync(token);

                    if (Queue.TryDequeue(out var job))
                    {
                        await Task.Run(() => ProcessJob(job), token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AppendLog($"[QUEUE WORKER ERROR] {ex.Message}");
                }
            }
        }

        private static void ProcessJob(PrintJob job)
        {
            try
            {
                if (!string.IsNullOrEmpty(job.JobId) && CancelledJobIds.TryRemove(job.JobId, out _))
                {
                    AppendLog($"[CANCELLED] JobId={job.JobId} was removed before printing.");
                    return;
                }

                // 1. Check duplicate shield
                if (!string.IsNullOrEmpty(job.JobId) && _printedJobIds.Contains(job.JobId))
                {
                    AppendLog($"[DUPLICATE SHIELD] Job {job.JobId} already printed. Skipped.");
                    return;
                }

                // 2. Fetch active configs
                var app = ConfigManager.Current.App;
                bool dryRun = app.DryRun && !job.ForcePrint;
                bool graphicMode = app.GraphicMode;

                // Find mapped printer for role
                PrinterInfo? targetPrinter = null;
                string printerId = job.Role.ToLower() switch
                {
                    "kitchen" => ConfigManager.Current.Printers.Kitchen ?? "",
                    "cashier" => ConfigManager.Current.Printers.Cashier ?? "",
                    "courier" => ConfigManager.Current.Printers.Courier ?? "",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(printerId))
                {
                    targetPrinter = ConfigManager.Current.Printers.Available.Find(p => p.Id == printerId);
                }

                if (targetPrinter == null && !dryRun)
                {
                    string roleLabel = job.Role.ToLower() switch
                    {
                        "kitchen" => LocalizationService.T("settings.kitchen", "Mutfak"),
                        "cashier" => LocalizationService.T("settings.cashier", "Kasa"),
                        "courier" => LocalizationService.T("settings.courier", "Kurye"),
                        _ => job.Role
                    };
                    string err = LocalizationService.T("printers.not_selected", "{label}: Henüz seçilmedi. Yazıcılar sekmesinden bir yazıcı atayın.").Replace("{label}", roleLabel);
                    AppendLog($"[ERROR] {err}");
                    JobProcessed?.Invoke(err, true);
                    return;
                }

                PrinterProfile profile = PrinterProfileResolver.Resolve(targetPrinter);

                AppendLog($"[RENDERING] Role={job.Role.ToUpper()} JobId={job.JobId}");

                // Load templates from disk or fallback
                SlipTemplate template = LoadSlipTemplate(job.Role);

                if (dryRun)
                {
                    // Dry run representation
                    AppendLog($"[DRY RUN] ({job.Role.ToUpper()}) Printing Job {job.JobId}...");
                    using (var bmp = ReceiptRenderer.RenderToBitmap(template, job.Data, profile.RasterWidthDots, profile.BarcodeWidthDots))
                    {
                        AppendLog($"Generated bitmap size: {bmp.Width}x{bmp.Height}");
                    }
                    AppendLog("-------------------------");
                    
                    string dryRunLabel = LocalizationService.T("printers.dry_run_suffix", " - Deneme modunda (gerçek yazdırma kapalı).").TrimStart(' ', '-');
                    JobProcessed?.Invoke($"({job.Role.ToUpper()}) {dryRunLabel}", false);
                }
                else
                {
                    AppendLog($"[SPOOLER SUBMITTED] Spooling to {targetPrinter!.Name} ({targetPrinter.Type}, {profile.DisplayName})");

                    if (graphicMode && !profile.UsesWindowsGdi)
                    {
                        using var bitmap = ReceiptRenderer.RenderToBitmap(template, job.Data, profile.RasterWidthDots, profile.BarcodeWidthDots);
                        byte[] rasterBytes = ReceiptRenderer.RenderBitmapToEscPos(bitmap);

                        if (targetPrinter.Type.ToLower() == "usb" || targetPrinter.Type.ToLower() == "win32")
                        {
                            EscPosDriver.SendRawToWin32(targetPrinter.Name, rasterBytes);
                        }
                        else if (targetPrinter.Type.ToLower() == "ip" || targetPrinter.Type.ToLower() == "network")
                        {
                            EscPosDriver.SendRawToIP(targetPrinter.Id, targetPrinter.Port, rasterBytes);
                        }
                    }
                    else if (graphicMode || profile.UsesWindowsGdi)
                    {
                        // Render to bitmap and draw via GDI
                        using (var bmp = ReceiptRenderer.RenderToBitmap(template, job.Data, profile.RasterWidthDots, profile.BarcodeWidthDots))
                        {
                            EscPosDriver.SendBitmapToWin32GDI(targetPrinter.Name, bmp);
                        }
                    }
                    else
                    {
                        // ESC/POS raw bytes
                        byte[] rawBytes = ReceiptRenderer.RenderToEscPos(template, job.Data, profile.TextColumns, profile.BarcodeWidthDots);

                        if (targetPrinter.Type.ToLower() == "usb" || targetPrinter.Type.ToLower() == "win32")
                        {
                            EscPosDriver.SendRawToWin32(targetPrinter.Name, rawBytes);
                        }
                        else if (targetPrinter.Type.ToLower() == "ip" || targetPrinter.Type.ToLower() == "network")
                        {
                            string ip = targetPrinter.Id;
                            int port = 9100;
                            if (ip.Contains(":"))
                            {
                                var parts = ip.Split(':');
                                ip = parts[0];
                                int.TryParse(parts[1], out port);
                            }
                            EscPosDriver.SendRawToIP(ip, port, rawBytes);
                        }
                    }

                    AppendLog($"[SUCCESS] JobId={job.JobId}");
                    JobProcessed?.Invoke($"({job.Role.ToUpper()}) {LocalizationService.T("printers.print_success", "Yazdırma başarılı.")}", false);
                }

                // 3. Mark duplicate shield as processed only on successful print
                if (!dryRun && !string.IsNullOrEmpty(job.JobId))
                {
                    _printedJobIds.Add(job.JobId);
                    SaveDeduplicationHistory();
                }

                // 4. Log to daily orders log file (OrdersControl lists this log)
                if (!job.SkipOrderLog)
                {
                    AppendOrderToStreamLog(job, targetPrinter?.Name ?? "Dry Run");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"[ERROR] Role={job.Role}, JobId={job.JobId}: {ex.Message}");
                string errText = LocalizationService.T("printers.error_title", "Yazıcı Hatası");
                JobProcessed?.Invoke($"{errText} ({job.Role.ToUpper()}): {ex.Message}", true);
            }
        }

        private static SlipTemplate LoadSlipTemplate(string role)
        {
            return TemplateStore.Load(role);
        }

        private static void AppendOrderToStreamLog(PrintJob job, string printerName)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string logDir = Path.Combine(appData, "nlkCheffiePrint", "logs");
                Directory.CreateDirectory(logDir);

                string slug = ConfigManager.Current.App.RestaurantSlug;
                string day = DateTime.Now.ToString("yyyy-MM-dd");

                string logPath = string.IsNullOrEmpty(slug)
                    ? Path.Combine(logDir, $"orders_stream_{day}.log")
                    : Path.Combine(logDir, $"orders_stream_{slug}_{day}.log");

                var entry = new Dictionary<string, object>();
                entry["ts"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                entry["role"] = job.Role;
                entry["printer"] = printerName;
                entry["job_id"] = job.JobId;

                var data = job.Data;
                var orderInfo = data.TryGetProperty("order_info", out var oi) ? oi : default;
                var payInfo = data.TryGetProperty("payment_info", out var pay) ? pay : default;
                var restInfo = data.TryGetProperty("restaurant_info", out var rest) ? rest : default;

                entry["order_number"] = GetStr(orderInfo, "order_number", "");
                entry["table"] = GetStr(orderInfo, "table_name", "");
                entry["waiter"] = GetStr(orderInfo, "waiter_name", "");
                entry["total"] = GetStr(payInfo, "total", "0.00");
                entry["restaurant"] = GetStr(restInfo, "name", "");
                entry["status"] = GetStr(orderInfo, "status", "pending");
                entry["payment_method"] = GetStr(orderInfo, "payment_method", "");

                // Convert items
                var itemsLog = new List<object>();
                if (data.TryGetProperty("items", out var itemsProp) && itemsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemsProp.EnumerateArray())
                    {
                        var it = new Dictionary<string, object>();
                        it["quantity"] = item.TryGetProperty("quantity", out var q) ? (q.ValueKind == JsonValueKind.Number ? q.GetInt32() : 1) : 1;
                        it["name"] = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                        it["notes"] = item.TryGetProperty("notes", out var no) ? no.GetString() ?? "" : "";
                        itemsLog.Add(it);
                    }
                }
                entry["items"] = itemsLog;

                string line = JsonSerializer.Serialize(entry) + "\n";
                LogMaintenance.Append(logPath, line, 10 * 1024 * 1024);
            }
            catch (Exception ex)
            {
                AppendLog($"[LOGGER ERROR] Failed writing order stream log: {ex.Message}");
            }
        }

        private static string GetStr(JsonElement parent, string propName, string fallback)
        {
            if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(propName, out var p))
            {
                if (p.ValueKind == JsonValueKind.Number) return p.GetDouble().ToString();
                return p.GetString() ?? fallback;
            }
            return fallback;
        }

        private static void LoadDeduplicationHistory()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                string path = Path.Combine(appData, "nlkCheffiePrint", "logs", $"printed_jobs_{day}.json");

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var list = JsonSerializer.Deserialize<List<string>>(json);
                    if (list != null)
                    {
                        _printedJobIds = new HashSet<string>(list);
                    }
                }
            }
            catch
            {
                _printedJobIds = new HashSet<string>();
            }
        }

        private static void SaveDeduplicationHistory()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string day = DateTime.Now.ToString("yyyy-MM-dd");
                string dir = Path.Combine(appData, "nlkCheffiePrint", "logs");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"printed_jobs_{day}.json");

                var list = new List<string>(_printedJobIds);
                if (list.Count > 2000)
                {
                    list = list.GetRange(list.Count - 2000, 2000);
                }

                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = false });
                File.WriteAllText(path, json);
            }
            catch
            {
                // Ignore history write errors
            }
        }

        private static void AppendLog(string message)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string logDir = Path.Combine(appData, "nlkCheffiePrint", "logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "print_out.txt");
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                LogMaintenance.Append(logFile, line, 2 * 1024 * 1024);
            }
            catch
            {
                // Ignore log errors
            }
        }
    }
}
