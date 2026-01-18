using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Services;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 日志视图模型
/// </summary>
public partial class LogViewModel : ObservableObject
{
    private readonly LogService _logService;
    
    public LogViewModel()
    {
        _logService = LogService.Instance;
        
        // 初始化日志条目
        RefreshLogEntries();
        
        // 订阅日志添加事件
        _logService.LogAdded += OnLogAdded;
        
        // 订阅剪贴板请求事件
        _logService.OnCopyToClipboardRequested += OnCopyToClipboardRequested;
    }
    
    /// <summary>
    /// 日志条目集合
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<LogService.LogEntry> _logEntries = new();
    
    /// <summary>
    /// 选中的日志条目
    /// </summary>
    [ObservableProperty]
    private LogService.LogEntry? _selectedLogEntry;
    
    /// <summary>
    /// 选中的日志级别
    /// </summary>
    [ObservableProperty]
    private LogLevel _selectedLogLevel = LogLevel.Debug;
    
    /// <summary>
    /// 搜索文本
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;
    
    /// <summary>
    /// 是否自动滚动
    /// </summary>
    [ObservableProperty]
    private bool _autoScroll = true;
    
    /// <summary>
    /// 日志文件路径
    /// </summary>
    [ObservableProperty]
    private string _logFilePath = string.Empty;
    
    /// <summary>
    /// 日志添加事件处理
    /// </summary>
    private void OnLogAdded(object? sender, LogService.LogEntry entry)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogEntries.Add(entry);
            
            // 如果启用了自动滚动，确保显示最新日志
            if (AutoScroll && LogEntries.Count > 0)
            {
                SelectedLogEntry = LogEntries.Last();
            }
            
            UpdateLogFilePath();
        });
    }
    
    /// <summary>
    /// 刷新日志条目
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        RefreshLogEntries();
    }
    
    /// <summary>
    /// 刷新日志条目
    /// </summary>
    private void RefreshLogEntries()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogEntries.Clear();
            foreach (var entry in _logService.LogEntries)
            {
                LogEntries.Add(entry);
            }
            
            UpdateLogFilePath();
            
            if (AutoScroll && LogEntries.Count > 0)
            {
                SelectedLogEntry = LogEntries.Last();
            }
        });
    }
    
    /// <summary>
    /// 清空日志
    /// </summary>
    [RelayCommand]
    private void Clear()
    {
        _logService.Clear();
        LogEntries.Clear();
    }
    
    /// <summary>
    /// 复制选中的日志
    /// </summary>
    [RelayCommand]
    private void CopySelected()
    {
        if (SelectedLogEntry != null)
        {
            try
            {
                Clipboard.SetText(SelectedLogEntry.ToString());
                _logService.Info("Selected log entry copied to clipboard", "LogViewModel");
            }
            catch (Exception ex)
            {
                _logService.Error("Failed to copy log entry to clipboard", "LogViewModel", ex);
            }
        }
    }
    
    /// <summary>
    /// 复制所有日志
    /// </summary>
    [RelayCommand]
    private void CopyAll()
    {
        try
        {
            _logService.CopyToClipboard();
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to copy all logs to clipboard", "LogViewModel", ex);
        }
    }
    
    /// <summary>
    /// 剪贴板请求事件处理
    /// </summary>
    private void OnCopyToClipboardRequested(object? sender, string logs)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                Clipboard.SetText(logs);
                _logService.Info("Logs copied to clipboard", "LogViewModel");
            }
            catch (Exception ex)
            {
                _logService.Error("Failed to copy logs to clipboard", "LogViewModel", ex);
            }
        });
    }
    
    /// <summary>
    /// 保存日志到文件
    /// </summary>
    [RelayCommand]
    private void SaveToFile()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = $"arcaea_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            
            if (dialog.ShowDialog() == true)
            {
                _logService.SaveToFile(dialog.FileName);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to save logs to file", "LogViewModel", ex);
        }
    }
    
    /// <summary>
    /// 打开日志文件所在文件夹
    /// </summary>
    [RelayCommand]
    private void OpenLogFolder()
    {
        try
        {
            if (!string.IsNullOrEmpty(_logService.LogFilePath) && File.Exists(_logService.LogFilePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_logService.LogFilePath}\"");
            }
            else
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appDataPath, "ArcaeaResourcePackEditor");
                
                if (Directory.Exists(appFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", appFolder);
                }
                else
                {
                    _logService.Warning("Log folder does not exist", "LogViewModel");
                }
            }
        }
        catch (Exception ex)
        {
            _logService.Error("Failed to open log folder", "LogViewModel", ex);
        }
    }
    
    /// <summary>
    /// 更新日志文件路径显示
    /// </summary>
    private void UpdateLogFilePath()
    {
        LogFilePath = _logService.LogFilePath;
    }
    
    /// <summary>
    /// 过滤日志条目
    /// </summary>
    public ObservableCollection<LogService.LogEntry> FilteredLogEntries
    {
        get
        {
            var filtered = LogEntries.Where(entry => 
                (int)entry.Level >= (int)SelectedLogLevel &&
                (string.IsNullOrEmpty(SearchText) || 
                 entry.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                 (entry.Source?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (entry.Exception?.Message?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
            );
            
            return new ObservableCollection<LogService.LogEntry>(filtered);
        }
    }
    
    /// <summary>
    /// 当属性变化时更新过滤
    /// </summary>
    partial void OnSelectedLogLevelChanged(LogLevel value)
    {
        OnPropertyChanged(nameof(FilteredLogEntries));
    }
    
    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredLogEntries));
    }
    
    /// <summary>
    /// 析构函数，取消事件订阅
    /// </summary>
    ~LogViewModel()
    {
        _logService.LogAdded -= OnLogAdded;
        _logService.OnCopyToClipboardRequested -= OnCopyToClipboardRequested;
    }
}
