using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class BackgroundWorker : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundWorker> _logger;
    private readonly IOptions<SchedulerSettings> _settings;
    private Timer _timer;

    private readonly object _fileWriteLock = new object();
    private const int MAX_RETRY_ATTEMPTS = 3;
    private const int RETRY_DELAY_MS = 100;

    public BackgroundWorker(ILogger<BackgroundWorker> logger, IOptions<SchedulerSettings> settings)
    {
        _logger = logger;
        _settings = settings;

        // 初始化任務的下次執行時間
        var now = DateTime.Now;
        foreach (var task in _settings.Value.Tasks)
        {

            if (task.LastRunResult == "Running") task.LastRunResult = "";

            if (task.LastRunTime == null || task.NextRunTime == null)
            {
                if (task.NextRunTime == null) task.NextRunTime = task.StartTime;

                for (var time = task.StartTime; time <= task.EndTime; time = time.AddMinutes(task.IntervalInMinutes))
                {
                    if (time > now)
                    {

                        task.NextRunTime = time;
                        break;
                    }
                }
            }
        }

        // 寫回設定檔
        WriteSettingsToFile();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {

        #region 強制執行任務
        var ForcedExecutionTasks = GetForcedExecutionTasks();
        if (ForcedExecutionTasks.Any())
        {
            _logger.LogInformation($"Found {ForcedExecutionTasks.Count} tasks with forced execution.");
            foreach (var task in ForcedExecutionTasks)
            {
                try
                {

                    // 執行同名
                    var method = this.GetType().GetMethod(task, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if (method != null)
                    {
                        var result = method.Invoke(this, null);
                        if (result is Task taskResult)
                        {
                            Task.Run(async () =>
                            {
                                await taskResult;
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Method {task} not found.");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in task {task}");
                }
            }

            // 強制執行任務後，清除強制執行列表
            ClearForcedExecutionTasks();
        }
        #endregion

        if (!IsEnabled())
        {
            _logger.LogInformation("BackgroundWorker is disabled.");
            return;
        }

        var now = DateTime.Now;
        foreach (var task in _settings.Value.Tasks)
        {
            if (task.NextRunTime == null || now >= task.NextRunTime)
            {
                if (now >= task.StartTime && now <= task.EndTime)
                {
                    _logger.LogInformation($"Running task: {task.TaskName}");
                    task.LastRunResult = "Running";
                    WriteSettingsToFile();

                    try
                    {

                        // 執行同名
                        var method = this.GetType().GetMethod(task.TaskName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                        if (method != null)
                        {
                            var result = method.Invoke(this, null);
                            if (result is Task taskResult)
                            {
                                Task.Run(async () =>
                                {
                                    await taskResult;
                                });
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Method {task.TaskName} not found.");
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in task {task.TaskName}");
                        task.LastRunResult = "Failed: " + ex.Message;
                    }

                    task.LastRunTime = now;
                    for (var time = Convert.ToDateTime(task.NextRunTime); time <= task.EndTime; time = time.AddMinutes(task.IntervalInMinutes))
                    {
                        if (time > now)
                        {
                            task.NextRunTime = time;
                            break;
                        }
                    }

                }
            }
        }

        // 寫回設定檔
        WriteSettingsToFile();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public void RecordRes(string TaskName, string Result)
    {
        var task = _settings.Value.Tasks.FirstOrDefault(t => t.TaskName == TaskName);
        if (task != null)
        {
            task.LastRunResult = Result;
            task.LastRunTime = DateTime.Now;

            // 寫回設定檔
            WriteSettingsToFile();
        }
    }

    private void WriteSettingsToFile()
    {
        WriteSettingsToFile(_settings.Value);
    }

    private void WriteSettingsToFile(SchedulerSettings settings)
    {
        lock (_fileWriteLock)
        {
            int retryCount = 0;
            while (retryCount < MAX_RETRY_ATTEMPTS)
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(
                        settings,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                    );

                    using (var fileStream = new FileStream(
                        "BackGroundSettings.json",
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.Read,
                        4096,
                        useAsync: false))
                    {
                        using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                        {
                            writer.Write(json);
                            writer.Flush();
                        }
                    }

                    return; // 成功後退出
                }
                catch (IOException) when (retryCount < MAX_RETRY_ATTEMPTS - 1)
                {
                    retryCount++;
                    _logger.LogWarning($"WriteSettingsToFile 重試 {retryCount}/{MAX_RETRY_ATTEMPTS}，等待 {RETRY_DELAY_MS}ms");
                    System.Threading.Thread.Sleep(RETRY_DELAY_MS);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WriteSettingsToFile 最終失敗");
                    _ = DoAppRecLog("BackgroundWorker", $"WriteSettingsToFile Error: {ex}");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// 寫入Log
    /// </summary>
    /// <param name="api"></param>
    /// <param name="Json"></param>
    /// <returns></returns>
    public async Task DoAppRecLog(string api, string Json)
    {

        try
        {
            string LogPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "log",
            DateTime.Now.ToString("yyyyMMdd"),
            api
            );
            if (Directory.Exists(LogPath) == false) Directory.CreateDirectory(LogPath);
            string filename = LogPath + "/" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".log";
            using (StreamWriter sw = new StreamWriter(filename, true))
            {
                sw.WriteLine(string.Format("[{0}]   {1}"
                    , "Result"
                    , Json)
                );
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

    }

    /// <summary>
    /// 排程是否啟用
    /// </summary>
    /// <returns></returns>
    private bool IsEnabled()
    {
        try
        {
            var json = File.ReadAllText("BackGroundSettings.json");
            var settings = System.Text.Json.JsonSerializer.Deserialize<SchedulerSettings>(json);
            return settings?.Enabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Enabled status");
            return false;
        }
    }

    /// <summary>
    /// 取得強制執行的任務列表
    /// </summary>
    /// <returns></returns>
    private System.Collections.Generic.List<string> GetForcedExecutionTasks()
    {
        try
        {
            var json = File.ReadAllText("BackGroundSettings.json");
            var settings = System.Text.Json.JsonSerializer.Deserialize<SchedulerSettings>(json);
            return settings?.Forced_Execution_Task?.ToList() ?? new System.Collections.Generic.List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Forced_Execution_Task");
            return new System.Collections.Generic.List<string>();
        }

    }

    /// <summary>
    /// 清除強制執行的任務列表
    /// </summary>
    private void ClearForcedExecutionTasks()
    {
        try
        {
            var json = File.ReadAllText("BackGroundSettings.json");
            var settings = System.Text.Json.JsonSerializer.Deserialize<SchedulerSettings>(json);
            if (settings != null)
            {
                settings.Forced_Execution_Task = new string[0];
                WriteSettingsToFile(settings); // 使用重載方法
                _logger.LogInformation("Cleared forced execution tasks.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear Forced_Execution_Task");
        }
    }

}
