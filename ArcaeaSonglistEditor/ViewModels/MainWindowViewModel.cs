using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Services;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 主窗口ViewModel
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private object? _selectedNavigationItem;

    public MainWindowViewModel(SonglistService songlistService)
    {
        _songlistService = songlistService;
        
        // 订阅数据更新事件
        _songlistService.OnDataUpdated += OnDataUpdated;
        
        // 初始化数据
        UpdateCounts();
        
        // 记录初始化日志
        LogService.Instance.Info("MainWindowViewModel initialized", "MainWindowViewModel");
    }

    /// <summary>
    /// 选中的导航项
    /// </summary>
    public object? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set => SetProperty(ref _selectedNavigationItem, value);
    }

    /// <summary>
    /// 歌曲数量
    /// </summary>
    [ObservableProperty]
    private string _songCount = "0";

    /// <summary>
    /// 曲包数量
    /// </summary>
    [ObservableProperty]
    private string _packCount = "0";

    /// <summary>
    /// 验证状态
    /// </summary>
    [ObservableProperty]
    private string _validationStatus = "未验证";

    /// <summary>
    /// 状态消息
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "就绪";

    /// <summary>
    /// JSON预览文本
    /// </summary>
    [ObservableProperty]
    private string _jsonPreviewText = "选择项目以查看JSON格式";

    /// <summary>
    /// 当前选中的项目（用于JSON预览）
    /// </summary>
    private object? _currentSelectedItem;

    /// <summary>
    /// 数据更新时触发
    /// </summary>
    private void OnDataUpdated(object? sender, EventArgs e)
    {
        UpdateCounts();
        UpdateValidationStatus();
    }

    /// <summary>
    /// 更新计数
    /// </summary>
    private void UpdateCounts()
    {
        SongCount = _songlistService.Songs.Count.ToString();
        PackCount = _songlistService.Packs.Count.ToString();
    }

    /// <summary>
    /// 更新验证状态
    /// </summary>
    private void UpdateValidationStatus()
    {
        var result = _songlistService.ValidateAll();
        if (result.HasErrors)
        {
            ValidationStatus = $"错误: {result.Errors.Count}";
        }
        else if (result.HasWarnings)
        {
            ValidationStatus = $"警告: {result.Warnings.Count}";
        }
        else
        {
            ValidationStatus = "验证通过";
        }
    }

    /// <summary>
    /// 设置状态消息
    /// </summary>
    public void SetStatusMessage(string message)
    {
        StatusMessage = message;
    }

    /// <summary>
    /// 清除状态消息
    /// </summary>
    public void ClearStatusMessage()
    {
        StatusMessage = "就绪";
    }

    /// <summary>
    /// 更新JSON预览
    /// </summary>
    public void UpdateJsonPreview(object? selectedItem)
    {
        _currentSelectedItem = selectedItem;
        
        try
        {
            if (selectedItem == null)
            {
                JsonPreviewText = "选择项目以查看JSON格式";
                return;
            }

            // 根据项目类型生成JSON（忽略null值）
            var jsonOptions = new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            
            var json = selectedItem switch
            {
                Core.Models.SongInfo song => System.Text.Json.JsonSerializer.Serialize(song, jsonOptions),
                Core.Models.PackInfo pack => System.Text.Json.JsonSerializer.Serialize(pack, jsonOptions),
                Core.Models.UnlockEntry unlock => System.Text.Json.JsonSerializer.Serialize(unlock, jsonOptions),
                _ => $"不支持的类型: {selectedItem.GetType().Name}"
            };

            JsonPreviewText = json;
        }
        catch (Exception ex)
        {
            JsonPreviewText = $"生成JSON预览时出错:\n{ex.Message}";
            LogService.Instance.Error($"更新JSON预览失败: {ex.Message}", "MainWindowViewModel", ex);
        }
    }

    /// <summary>
    /// 复制JSON到剪贴板
    /// </summary>
    [RelayCommand]
    private void CopyJsonToClipboard()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(JsonPreviewText) && JsonPreviewText != "选择项目以查看JSON格式")
            {
                System.Windows.Clipboard.SetText(JsonPreviewText);
                SetStatusMessage("JSON已复制到剪贴板");
                LogService.Instance.Info("JSON复制到剪贴板", "MainWindowViewModel");
            }
            else
            {
                SetStatusMessage("没有可复制的JSON内容");
            }
        }
        catch (Exception ex)
        {
            SetStatusMessage($"复制失败: {ex.Message}");
            LogService.Instance.Error($"复制JSON到剪贴板失败: {ex.Message}", "MainWindowViewModel", ex);
        }
    }

    /// <summary>
    /// 刷新JSON预览
    /// </summary>
    [RelayCommand]
    private void RefreshJsonPreview()
    {
        UpdateJsonPreview(_currentSelectedItem);
        SetStatusMessage("JSON预览已刷新");
    }

    // 导航命令
    [RelayCommand]
    private void NavigateToSonglist()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToPacklist()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToUnlocks()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToValidation()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToTools()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
        // 导航逻辑在MainWindow.xaml.cs中处理
    }
}
