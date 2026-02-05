using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Services;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 验证报告ViewModel
/// </summary>
public partial class ValidationViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private readonly MainWindowViewModel _mainViewModel;

    public ValidationViewModel(SonglistService songlistService, MainWindowViewModel mainViewModel)
    {
        _songlistService = songlistService;
        _mainViewModel = mainViewModel;
    }

    /// <summary>
    /// 错误列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _errors = new();

    /// <summary>
    /// 警告列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _warnings = new();

    /// <summary>
    /// 验证摘要
    /// </summary>
    [ObservableProperty]
    private string _validationSummary = "点击验证按钮开始验证";

    /// <summary>
    /// 是否启用严格验证模式
    /// </summary>
    [ObservableProperty]
    private bool _strictValidationMode = false;

    /// <summary>
    /// 歌曲文件夹路径
    /// </summary>
    [ObservableProperty]
    private string? _songsFolderPath;

    /// <summary>
    /// 选择歌曲文件夹命令
    /// </summary>
    [RelayCommand]
    private void SelectSongsFolder()
    {
        try
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择songs文件夹（通常在active/songs下）",
                ShowNewFolderButton = false
            };
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SongsFolderPath = dialog.SelectedPath;
                LogService.Instance.Info($"选择歌曲文件夹: {SongsFolderPath}", "ValidationViewModel");
                _mainViewModel.SetStatusMessage($"已选择歌曲文件夹: {Path.GetFileName(SongsFolderPath)}");
            }
        }
        catch (Exception ex)
        {
            LogService.Instance.Error("选择歌曲文件夹失败", "ValidationViewModel", ex);
            MessageBox.Show($"选择歌曲文件夹失败:\n\n{ex.Message}", 
                "选择失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 验证命令
    /// </summary>
    [RelayCommand]
    private void Validate()
    {
        try
        {
            // 检查服务是否可用
            if (_songlistService == null)
            {
                throw new InvalidOperationException("歌曲列表服务未初始化");
            }
            
            // 创建增强验证服务
            var enhancedValidationService = new EnhancedValidationService(_songlistService);
            
            // 使用用户设置的歌曲文件夹路径，如果没有设置则尝试自动检测
            string? songsFolderPath = SongsFolderPath;
            
            if (string.IsNullOrWhiteSpace(songsFolderPath))
            {
                // 尝试自动检测歌曲文件夹路径
                var currentDir = Directory.GetCurrentDirectory();
                var possiblePaths = new[]
                {
                    Path.Combine(currentDir, "cbandroid", "cb", "active", "songs"),
                    Path.Combine(currentDir, "cbios", "cb", "active", "songs"),
                    Path.Combine(currentDir, "songs"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "arcaea", "2.9pre", "cbandroid", "cb", "active", "songs")
                };
                
                foreach (var path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        songsFolderPath = path;
                        LogService.Instance.Info($"自动找到歌曲文件夹: {path}", "ValidationViewModel");
                        break;
                    }
                }
            }
            
            if (!string.IsNullOrWhiteSpace(songsFolderPath))
            {
                enhancedValidationService.SetSongsFolderPath(songsFolderPath);
                LogService.Instance.Info($"使用歌曲文件夹: {songsFolderPath}", "ValidationViewModel");
                
                // 自动寻找并导入清单文件
                var importedFiles = new List<string>();
                var importMessages = new List<string>();
                
                // 检查并导入songlist文件
                var songlistPath = Path.Combine(songsFolderPath, "songlist");
                if (File.Exists(songlistPath))
                {
                    try
                    {
                        _songlistService.LoadSonglistFromFile(songlistPath);
                        importedFiles.Add("songlist");
                        importMessages.Add($"• songlist (已导入)");
                        LogService.Instance.Info($"已导入songlist文件: {songlistPath}", "ValidationViewModel");
                    }
                    catch (Exception ex)
                    {
                        importMessages.Add($"• songlist (导入失败: {ex.Message})");
                        LogService.Instance.Error($"导入songlist文件失败: {songlistPath}", "ValidationViewModel", ex);
                    }
                }
                else
                {
                    importMessages.Add($"• songlist (未找到)");
                }
                
                // 检查并导入packlist文件
                var packlistPath = Path.Combine(songsFolderPath, "packlist");
                if (File.Exists(packlistPath))
                {
                    try
                    {
                        _songlistService.LoadPacklistFromFile(packlistPath);
                        importedFiles.Add("packlist");
                        importMessages.Add($"• packlist (已导入)");
                        LogService.Instance.Info($"已导入packlist文件: {packlistPath}", "ValidationViewModel");
                    }
                    catch (Exception ex)
                    {
                        importMessages.Add($"• packlist (导入失败: {ex.Message})");
                        LogService.Instance.Error($"导入packlist文件失败: {packlistPath}", "ValidationViewModel", ex);
                    }
                }
                else
                {
                    importMessages.Add($"• packlist (未找到)");
                }
                
                // 检查并导入unlocks文件
                var unlocksPath = Path.Combine(songsFolderPath, "unlocks");
                if (File.Exists(unlocksPath))
                {
                    try
                    {
                        _songlistService.LoadUnlocksFromFile(unlocksPath);
                        importedFiles.Add("unlocks");
                        importMessages.Add($"• unlocks (已导入)");
                        LogService.Instance.Info($"已导入unlocks文件: {unlocksPath}", "ValidationViewModel");
                    }
                    catch (Exception ex)
                    {
                        importMessages.Add($"• unlocks (导入失败: {ex.Message})");
                        LogService.Instance.Error($"导入unlocks文件失败: {unlocksPath}", "ValidationViewModel", ex);
                    }
                }
                else
                {
                    importMessages.Add($"• unlocks (未找到)");
                }
                
                // 记录导入结果
                if (importedFiles.Count > 0)
                {
                    LogService.Instance.Info($"自动导入 {importedFiles.Count} 个清单文件: {string.Join(", ", importedFiles)}", "ValidationViewModel");
                }
            }
            else if (StrictValidationMode)
            {
                LogService.Instance.Warning("未找到歌曲文件夹，跳过文件夹验证", "ValidationViewModel");
            }
            
            EnhancedValidationResult result;
            
            try
            {
                result = enhancedValidationService.ValidateAll(StrictValidationMode);
                LogService.Instance.Info($"执行验证（严格模式: {StrictValidationMode}）", "ValidationViewModel");
            }
            catch (Exception validationEx)
            {
                throw new InvalidOperationException($"验证过程失败: {validationEx.Message}", validationEx);
            }
            
            Errors = new ObservableCollection<string>(result.Errors);
            Warnings = new ObservableCollection<string>(result.Warnings);
            ValidationSummary = result.GetSummary();
            
            _mainViewModel.SetStatusMessage($"验证完成: {result.Errors.Count} 个错误, {result.Warnings.Count} 个警告");
            
            LogService.Instance.Info($"验证完成: {result.Errors.Count} 个错误, {result.Warnings.Count} 个警告", "ValidationViewModel");
            
            if (result.Errors.Count > 0)
            {
                LogService.Instance.Warning($"发现 {result.Errors.Count} 个验证错误", "ValidationViewModel");
            }
            if (result.Warnings.Count > 0)
            {
                LogService.Instance.Warning($"发现 {result.Warnings.Count} 个验证警告", "ValidationViewModel");
            }
        }
        catch (Exception ex)
        {
            ValidationSummary = $"验证过程中发生错误: {ex.Message}";
            Errors.Clear();
            Warnings.Clear();
            
            _mainViewModel.SetStatusMessage($"验证失败: {ex.Message}");
            LogService.Instance.Error($"验证失败: {ex.Message}", "ValidationViewModel", ex);
            
            MessageBox.Show($"验证过程中发生错误:\n\n{ex.Message}\n\n详细错误: {ex.InnerException?.Message}", 
                "验证错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 导出报告命令
    /// </summary>
    [RelayCommand]
    private void ExportReport()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
            Title = "保存验证报告",
            FileName = $"validation_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var report = GenerateReport();
                File.WriteAllText(dialog.FileName, report);
                _mainViewModel.SetStatusMessage($"已导出验证报告: {Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导出失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 生成详细报告
    /// </summary>
    private string GenerateReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("Arcaea Resource Pack Editor - 验证报告");
        report.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        
        report.AppendLine("=== 错误列表 ===");
        if (Errors.Count == 0)
        {
            report.AppendLine("无错误");
        }
        else
        {
            for (int i = 0; i < Errors.Count; i++)
            {
                report.AppendLine($"{i + 1}. {Errors[i]}");
            }
        }
        
        report.AppendLine();
        report.AppendLine("=== 警告列表 ===");
        if (Warnings.Count == 0)
        {
            report.AppendLine("无警告");
        }
        else
        {
            for (int i = 0; i < Warnings.Count; i++)
            {
                report.AppendLine($"{i + 1}. {Warnings[i]}");
            }
        }
        
        report.AppendLine();
        report.AppendLine("=== 统计信息 ===");
        report.AppendLine($"歌曲数量: {_songlistService.Songs.Count}");
        report.AppendLine($"曲包数量: {_songlistService.Packs.Count}");
        report.AppendLine($"解锁条件数量: {_songlistService.Unlocks.Unlocks.Count}");
        report.AppendLine($"错误数量: {Errors.Count}");
        report.AppendLine($"警告数量: {Warnings.Count}");
        
        return report.ToString();
    }
}
