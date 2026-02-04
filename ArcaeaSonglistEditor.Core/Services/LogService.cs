using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArcaeaSonglistEditor.Core.Services;

/// <summary>
/// 日志级别
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// 日志服务
/// </summary>
public class LogService
{
    private static LogService? _instance;
    private readonly List<LogEntry> _logEntries = new();
    private readonly object _lock = new();
    private string _logFilePath = string.Empty;
    
    /// <summary>
    /// 日志条目
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Source { get; set; }
        public Exception? Exception { get; set; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
            sb.Append($"{Level.ToString().ToUpper(),-8} ");
            
            if (!string.IsNullOrEmpty(Source))
            {
                sb.Append($"[{Source}] ");
            }
            
            sb.Append(Message);
            
            if (Exception != null)
            {
                sb.Append($" | Exception: {Exception.GetType().Name}: {Exception.Message}");
                if (Exception.InnerException != null)
                {
                    sb.Append($" | Inner: {Exception.InnerException.Message}");
                }
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// 日志条目列表
    /// </summary>
    public IReadOnlyList<LogEntry> LogEntries => _logEntries;
    
    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string LogFilePath => _logFilePath;
    
    /// <summary>
    /// 日志事件
    /// </summary>
    public event EventHandler<LogEntry>? LogAdded;
    
    /// <summary>
    /// 单例实例
    /// </summary>
    public static LogService Instance => _instance ??= new LogService();
    
    private LogService()
    {
        InitializeLogFile();
    }
    
    /// <summary>
    /// 初始化日志文件
    /// </summary>
    private void InitializeLogFile()
    {
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var defaultLogFolder = Path.Combine(appDataPath, "ArcaeaResourcePackEditor");
            
            // 使用默认路径初始化
            UpdateLogPath(defaultLogFolder);
            
            Info("Log service initialized", "LogService");
        }
        catch (Exception ex)
        {
            // 如果文件日志失败，至少记录到内存
            _logFilePath = string.Empty;
            Error("Failed to initialize log file", "LogService", ex);
        }
    }
    
    /// <summary>
    /// 更新日志路径
    /// </summary>
    /// <param name="logFolderPath">日志文件夹路径</param>
    public void UpdateLogPath(string logFolderPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(logFolderPath))
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                logFolderPath = Path.Combine(appDataPath, "ArcaeaResourcePackEditor");
            }
            
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logFilePath = Path.Combine(logFolderPath, $"log_{timestamp}.txt");
            
            // 如果这是第一次设置路径，写入初始日志
            if (!File.Exists(_logFilePath))
            {
                WriteToFile($"=== Arcaea Resource Pack Editor Log ===\n");
                WriteToFile($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
                WriteToFile($"Version: 1.0.0\n");
                WriteToFile($"OS: {Environment.OSVersion}\n");
                WriteToFile($"Framework: {Environment.Version}\n");
                WriteToFile($"Machine: {Environment.MachineName}\n");
                WriteToFile($"User: {Environment.UserName}\n");
                WriteToFile($"Working Directory: {Environment.CurrentDirectory}\n");
                WriteToFile($"Log Path: {_logFilePath}\n");
                WriteToFile($"=======================================\n\n");
            }
            
            Info($"Log path updated to: {logFolderPath}", "LogService");
        }
        catch (Exception ex)
        {
            Error($"Failed to update log path to: {logFolderPath}", "LogService", ex);
            throw;
        }
    }
    
    /// <summary>
    /// 写入日志文件
    /// </summary>
    private void WriteToFile(string message)
    {
        if (string.IsNullOrEmpty(_logFilePath))
            return;
            
        try
        {
            File.AppendAllText(_logFilePath, message);
        }
        catch
        {
            // 忽略文件写入错误
        }
    }
    
    /// <summary>
    /// 添加日志条目
    /// </summary>
    private void AddLogEntry(LogLevel level, string message, string? source, Exception? exception = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Source = source,
            Exception = exception
        };
        
        lock (_lock)
        {
            _logEntries.Add(entry);
            
            // 限制日志条目数量，避免内存占用过大
            if (_logEntries.Count > 10000)
            {
                _logEntries.RemoveRange(0, 1000);
            }
        }
        
        // 写入文件
        WriteToFile(entry.ToString() + "\n");
        
        // 触发事件
        LogAdded?.Invoke(this, entry);
    }
    
    /// <summary>
    /// 调试日志
    /// </summary>
    public void Debug(string message, string? source = null)
    {
        AddLogEntry(LogLevel.Debug, message, source);
    }
    
    /// <summary>
    /// 信息日志
    /// </summary>
    public void Info(string message, string? source = null)
    {
        AddLogEntry(LogLevel.Info, message, source);
    }
    
    /// <summary>
    /// 警告日志
    /// </summary>
    public void Warning(string message, string? source = null)
    {
        AddLogEntry(LogLevel.Warning, message, source);
    }
    
    /// <summary>
    /// 错误日志
    /// </summary>
    public void Error(string message, string? source = null, Exception? exception = null)
    {
        AddLogEntry(LogLevel.Error, message, source, exception);
    }
    
    /// <summary>
    /// 严重错误日志
    /// </summary>
    public void Critical(string message, string? source = null, Exception? exception = null)
    {
        AddLogEntry(LogLevel.Critical, message, source, exception);
    }
    
    /// <summary>
    /// 获取所有日志文本
    /// </summary>
    public string GetAllLogs()
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            foreach (var entry in _logEntries)
            {
                sb.AppendLine(entry.ToString());
            }
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// 获取最近N条日志
    /// </summary>
    public string GetRecentLogs(int count)
    {
        lock (_lock)
        {
            var sb = new StringBuilder();
            var startIndex = Math.Max(0, _logEntries.Count - count);
            
            for (int i = startIndex; i < _logEntries.Count; i++)
            {
                sb.AppendLine(_logEntries[i].ToString());
            }
            
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// 清空日志
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _logEntries.Clear();
        }
        
        Info("Log cleared", "LogService");
    }
    
    /// <summary>
    /// 保存日志到文件
    /// </summary>
    public void SaveToFile(string? filePath = null)
    {
        try
        {
            var path = filePath ?? _logFilePath;
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Path.GetTempPath(), $"arcaea_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }
            
            var logs = GetAllLogs();
            File.WriteAllText(path, logs);
            
            Info($"Logs saved to: {path}", "LogService");
        }
        catch (Exception ex)
        {
            Error("Failed to save logs to file", "LogService", ex);
        }
    }
    
    /// <summary>
    /// 复制日志到剪贴板
    /// </summary>
    public void CopyToClipboard()
    {
        try
        {
            var logs = GetRecentLogs(100); // 复制最近100条日志
            
            // 剪贴板操作需要在UI线程中执行
            // 这里只记录日志，实际复制操作在ViewModel中处理
            Info("Copy to clipboard requested", "LogService");
            
            // 返回日志文本，让调用者处理剪贴板操作
            OnCopyToClipboardRequested?.Invoke(this, logs);
        }
        catch (Exception ex)
        {
            Error("Failed to copy logs to clipboard", "LogService", ex);
        }
    }
    
    /// <summary>
    /// 复制到剪贴板请求事件
    /// </summary>
    public event EventHandler<string>? OnCopyToClipboardRequested;
}
