using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Windows;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 设置视图模型
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    #region 常规设置
    [ObservableProperty]
    private bool _autoLoadLastFile = true;

    [ObservableProperty]
    private bool _autoBackupOnSave = true;

    [ObservableProperty]
    private bool _showConfirmationDialogs = true;

    [ObservableProperty]
    private string _defaultLanguage = "简体中文";
    #endregion

    #region 编辑器设置
    [ObservableProperty]
    private bool _autoValidateInput = true;

    [ObservableProperty]
    private bool _showAdvancedOptions = false;

    [ObservableProperty]
    private bool _enableLivePreview = true;

    [ObservableProperty]
    private string _defaultDateFormat = "Unix时间戳";
    #endregion

    #region 文件设置
    [ObservableProperty]
    private string _defaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arcaea", "songlist");

    [ObservableProperty]
    private string _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arcaea", "backup");

    [ObservableProperty]
    private string _logStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArcaeaResourcePackEditor");

    [ObservableProperty]
    private bool _autoCreateBackupFolder = true;

    [ObservableProperty]
    private bool _compressJsonOnSave = false;
    #endregion

    #region 验证设置
    [ObservableProperty]
    private bool _enableStrictValidation = false;

    [ObservableProperty]
    private bool _showWarningsAsErrors = false;

    [ObservableProperty]
    private bool _autoFixCommonErrors = true;

    [ObservableProperty]
    private int _maxErrorDisplayCount = 50;
    #endregion

    #region 界面设置
    [ObservableProperty]
    private bool _enableDarkMode = false;

    [ObservableProperty]
    private bool _showLineNumbers = true;

    [ObservableProperty]
    private bool _useCompactLayout = false;

    [ObservableProperty]
    private double _fontSize = 12.0;
    #endregion

    #region 命令
    [RelayCommand]
    private void RestoreDefaults()
    {
        // 恢复默认设置
        AutoLoadLastFile = true;
        AutoBackupOnSave = true;
        ShowConfirmationDialogs = true;
        DefaultLanguage = "简体中文";

        AutoValidateInput = true;
        ShowAdvancedOptions = false;
        EnableLivePreview = true;
        DefaultDateFormat = "Unix时间戳";

        DefaultSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arcaea", "songlist");
        BackupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Arcaea", "backup");
        LogStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArcaeaResourcePackEditor");
        AutoCreateBackupFolder = true;
        CompressJsonOnSave = false;

        EnableStrictValidation = false;
        ShowWarningsAsErrors = false;
        AutoFixCommonErrors = true;
        MaxErrorDisplayCount = 50;

        EnableDarkMode = false;
        ShowLineNumbers = true;
        UseCompactLayout = false;
        FontSize = 12.0;

        MessageBox.Show("已恢复默认设置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SaveSettings()
    {
        try
        {
            // 这里应该将设置保存到配置文件
            // 暂时只显示保存成功的消息
            ApplyLogSettings();
            MessageBox.Show("设置已保存", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ApplySettings()
    {
        // 应用设置到当前会话
        try
        {
            ApplyLogSettings();
            MessageBox.Show("设置已应用", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"应用设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 应用日志设置
    /// </summary>
    private void ApplyLogSettings()
    {
        try
        {
            var logService = Core.Services.LogService.Instance;
            logService.UpdateLogPath(LogStoragePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"更新日志路径失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    [RelayCommand]
    private void BrowseDefaultSavePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择默认保存路径",
            FileName = "选择文件夹",
            Filter = "文件夹|*.",
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            var directory = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(directory))
            {
                DefaultSavePath = directory;
            }
        }
    }

    [RelayCommand]
    private void BrowseBackupPath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择备份路径",
            FileName = "选择文件夹",
            Filter = "文件夹|*.",
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            var directory = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(directory))
            {
                BackupPath = directory;
            }
        }
    }

    [RelayCommand]
    private void BrowseLogStoragePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "选择日志存储路径",
            FileName = "选择文件夹",
            Filter = "文件夹|*.",
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            var directory = System.IO.Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(directory))
            {
                LogStoragePath = directory;
            }
        }
    }
    #endregion

    #region 构造函数
    public SettingsViewModel()
    {
        // 加载设置
        LoadSettings();
    }
    #endregion

    #region 私有方法
    private void LoadSettings()
    {
        // 这里应该从配置文件加载设置
        // 暂时使用默认值
    }
    #endregion
}
