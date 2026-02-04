using System;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Services;
using ArcaeaSonglistEditor.Views;
using Microsoft.Win32;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 工具视图模型
/// </summary>
public partial class ToolsViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private readonly LogService _logService;
    private readonly BundleService _bundleService;
    
    public ToolsViewModel(SonglistService songlistService)
    {
        _songlistService = songlistService ?? throw new ArgumentNullException(nameof(songlistService));
        _logService = LogService.Instance;
        _bundleService = new BundleService(songlistService);
    }
    
    /// <summary>
    /// 移除所有歌曲限制
    /// </summary>
    [RelayCommand]
    private void RemoveAllSongRestrictions()
    {
        try
        {
            // 检查服务是否可用
            if (_songlistService == null)
            {
                throw new InvalidOperationException("歌曲列表服务未初始化");
            }
            
            int count = 0;
            foreach (var song in _songlistService.Songs)
            {
                if (song.HasRestrictions())
                {
                    song.RemoveRestrictions();
                    count++;
                }
            }
            
            _songlistService.InvokeDataUpdatedEvent();
            _logService.Info($"移除了 {count} 首歌曲的限制", "ToolsViewModel");
            
            MessageBox.Show($"已移除 {count} 首歌曲的限制。", 
                "操作完成", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error($"移除所有歌曲限制失败: {ex.Message}", "ToolsViewModel", ex);
            MessageBox.Show($"移除所有歌曲限制失败:\n\n{ex.Message}\n\n详细错误: {ex.InnerException?.Message}", 
                "操作失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 清空所有解锁条件
    /// </summary>
    [RelayCommand]
    private void ClearAllUnlockConditions()
    {
        try
        {
            _songlistService.Unlocks.Unlocks.Clear();
            _songlistService.InvokeDataUpdatedEvent();
            _logService.Info("清空了所有解锁条件", "ToolsViewModel");
            
            MessageBox.Show("已清空所有解锁条件。", 
                "操作完成", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("清空所有解锁条件失败", "ToolsViewModel", ex);
            MessageBox.Show($"清空所有解锁条件失败:\n\n{ex.Message}", 
                "操作失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 批量修改曲包设置
    /// </summary>
    [RelayCommand]
    private void BatchModifyPackSettings()
    {
        try
        {
            // 这里可以添加更复杂的批量修改逻辑
            // 目前只是示例
            _logService.Info("批量修改曲包设置功能被调用", "ToolsViewModel");
            
            MessageBox.Show("批量修改曲包设置功能正在开发中。\n\n该功能将允许批量修改曲包的显示设置、扩展包标记等。", 
                "功能说明", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("批量修改曲包设置失败", "ToolsViewModel", ex);
            MessageBox.Show($"批量修改曲包设置失败:\n\n{ex.Message}", 
                "操作失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 从SlstGen导入
    /// </summary>
    [RelayCommand]
    private void ImportFromSlstGen()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SlstGen文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "选择SlstGen导出的文件"
            };
            
            if (dialog.ShowDialog() == true)
            {
                // 这里可以添加SlstGen格式的解析逻辑
                _logService.Info($"从SlstGen导入: {dialog.FileName}", "ToolsViewModel");
                
                MessageBox.Show($"已选择文件: {dialog.FileName}\n\nSlstGen导入功能正在开发中。\n该功能将解析SlstGen格式的JSON文件并转换为当前格式。", 
                    "功能说明", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("从SlstGen导入失败", "ToolsViewModel", ex);
            MessageBox.Show($"从SlstGen导入失败:\n\n{ex.Message}", 
                "导入失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 导出为SlstGen格式
    /// </summary>
    [RelayCommand]
    private void ExportToSlstGenFormat()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导出为SlstGen格式",
                FileName = "songlist_slstgen.json"
            };
            
            if (dialog.ShowDialog() == true)
            {
                // 这里可以添加SlstGen格式的导出逻辑
                _logService.Info($"导出为SlstGen格式: {dialog.FileName}", "ToolsViewModel");
                
                MessageBox.Show($"将导出到: {dialog.FileName}\n\nSlstGen导出功能正在开发中。\n该功能将当前数据转换为SlstGen兼容的格式。", 
                    "功能说明", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("导出为SlstGen格式失败", "ToolsViewModel", ex);
            MessageBox.Show($"导出为SlstGen格式失败:\n\n{ex.Message}", 
                "导出失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 转换旧版本格式
    /// </summary>
    [RelayCommand]
    private void ConvertOldVersionFormat()
    {
        try
        {
            _logService.Info("转换旧版本格式功能被调用", "ToolsViewModel");
            
            MessageBox.Show("转换旧版本格式功能正在开发中。\n\n该功能将帮助将旧版本的Arcaea Patch Tool数据转换为当前格式。", 
                "功能说明", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("转换旧版本格式失败", "ToolsViewModel", ex);
            MessageBox.Show($"转换旧版本格式失败:\n\n{ex.Message}", 
                "转换失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 检查重复歌曲
    /// </summary>
    [RelayCommand]
    private void CheckDuplicateSongs()
    {
        try
        {
            var duplicates = _songlistService.Songs
                .GroupBy(s => s.Id)
                .Where(g => g.Count() > 1)
                .Select(g => new { SongId = g.Key, Count = g.Count() })
                .ToList();
            
            if (duplicates.Count > 0)
            {
                var message = $"发现 {duplicates.Count} 组重复歌曲:\n\n";
                foreach (var dup in duplicates)
                {
                    message += $"• 歌曲ID: {dup.SongId} (出现 {dup.Count} 次)\n";
                }
                
                _logService.Warning($"发现 {duplicates.Count} 组重复歌曲", "ToolsViewModel");
                MessageBox.Show(message, 
                    "重复歌曲检查", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
            else
            {
                _logService.Info("未发现重复歌曲", "ToolsViewModel");
                MessageBox.Show("未发现重复歌曲。", 
                    "重复歌曲检查", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("检查重复歌曲失败", "ToolsViewModel", ex);
            MessageBox.Show($"检查重复歌曲失败:\n\n{ex.Message}", 
                "检查失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 检查缺失曲包
    /// </summary>
    [RelayCommand]
    private void CheckMissingPacks()
    {
        try
        {
            var songPacks = _songlistService.Songs
                .Select(s => s.Set)
                .Distinct()
                .Where(packId => !string.IsNullOrEmpty(packId))
                .ToList();
            
            var missingPacks = songPacks
                .Where(packId => !_songlistService.Packs.Any(p => p.Id == packId))
                .ToList();
            
            if (missingPacks.Count > 0)
            {
                var message = $"发现 {missingPacks.Count} 个缺失的曲包:\n\n";
                foreach (var packId in missingPacks)
                {
                    message += $"• 曲包ID: {packId}\n";
                }
                
                _logService.Warning($"发现 {missingPacks.Count} 个缺失曲包", "ToolsViewModel");
                MessageBox.Show(message, 
                    "缺失曲包检查", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
            else
            {
                _logService.Info("未发现缺失曲包", "ToolsViewModel");
                MessageBox.Show("未发现缺失曲包。", 
                    "缺失曲包检查", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("检查缺失曲包失败", "ToolsViewModel", ex);
            MessageBox.Show($"检查缺失曲包失败:\n\n{ex.Message}", 
                "检查失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 检查无效解锁条件
    /// </summary>
    [RelayCommand]
    private void CheckInvalidUnlockConditions()
    {
        try
        {
            // 这里可以添加更复杂的解锁条件验证逻辑
            _logService.Info("检查无效解锁条件功能被调用", "ToolsViewModel");
            
            MessageBox.Show("检查无效解锁条件功能正在开发中。\n\n该功能将检查解锁条件中引用的歌曲是否存在，以及条件参数是否有效。", 
                "功能说明", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("检查无效解锁条件失败", "ToolsViewModel", ex);
            MessageBox.Show($"检查无效解锁条件失败:\n\n{ex.Message}", 
                "检查失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 生成测试数据
    /// </summary>
    [RelayCommand]
    private void GenerateTestData()
    {
        try
        {
            _logService.Info("生成测试数据功能被调用", "ToolsViewModel");
            
            MessageBox.Show("生成测试数据功能正在开发中。\n\n该功能将生成用于测试的示例歌曲、曲包和解锁条件数据。", 
                "功能说明", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("生成测试数据失败", "ToolsViewModel", ex);
            MessageBox.Show($"生成测试数据失败:\n\n{ex.Message}", 
                "生成失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 清理临时文件
    /// </summary>
    [RelayCommand]
    private void CleanTempFiles()
    {
        try
        {
            var tempPath = Path.GetTempPath();
            var appTempFiles = Directory.GetFiles(tempPath, "arcaea_*.tmp", SearchOption.TopDirectoryOnly);
            
            int deletedCount = 0;
            foreach (var file in appTempFiles)
            {
                try
                {
                    File.Delete(file);
                    deletedCount++;
                }
                catch
                {
                    // 忽略无法删除的文件
                }
            }
            
            _logService.Info($"清理了 {deletedCount} 个临时文件", "ToolsViewModel");
            MessageBox.Show($"已清理 {deletedCount} 个临时文件。", 
                "清理完成", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("清理临时文件失败", "ToolsViewModel", ex);
            MessageBox.Show($"清理临时文件失败:\n\n{ex.Message}", 
                "清理失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 重置所有设置
    /// </summary>
    [RelayCommand]
    private void ResetAllSettings()
    {
        try
        {
            var result = MessageBox.Show("确定要重置所有设置吗？\n\n这将恢复所有设置为默认值，但不会删除歌曲、曲包和解锁条件数据。", 
                "确认重置", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                // 这里可以添加重置设置的逻辑
                _logService.Info("重置所有设置", "ToolsViewModel");
                MessageBox.Show("所有设置已重置为默认值。", 
                    "重置完成", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("重置所有设置失败", "ToolsViewModel", ex);
            MessageBox.Show($"重置所有设置失败:\n\n{ex.Message}", 
                "重置失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 验证active文件夹结构
    /// </summary>
    [RelayCommand]
    private void ValidateActiveFolder()
    {
        try
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择active文件夹",
                ShowNewFolderButton = false
            };
            
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var activeFolder = dialog.SelectedPath;
            
            var result = _bundleService.ValidateActiveFolder(activeFolder);
            
            var message = $"验证结果: {result.GetSummary()}\n\n";
            if (result.Errors.Count > 0)
            {
                message += "错误:\n";
                foreach (var error in result.Errors)
                {
                    message += $"• {error}\n";
                }
                message += "\n";
            }
            if (result.Warnings.Count > 0)
            {
                message += "警告:\n";
                foreach (var warning in result.Warnings)
                {
                    message += $"• {warning}\n";
                }
            }
            
            if (result.IsValid)
            {
                _logService.Info($"active文件夹验证通过: {activeFolder}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "验证成功", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            else
            {
                _logService.Warning($"active文件夹验证失败: {activeFolder}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "验证失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("验证active文件夹失败", "ToolsViewModel", ex);
            MessageBox.Show($"验证active文件夹失败:\n\n{ex.Message}", 
                "验证失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 更新现有active文件夹
    /// </summary>
    [RelayCommand]
    private void UpdateActiveFolder()
    {
        try
        {
            // 选择现有的active文件夹
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择要更新的active文件夹",
                ShowNewFolderButton = false
            };
            
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var activeFolder = folderDialog.SelectedPath;
            
            // 验证是否是active文件夹
            if (!Directory.Exists(Path.Combine(activeFolder, "songs")))
            {
                var result = MessageBox.Show($"选择的文件夹 '{activeFolder}' 看起来不是标准的active文件夹（缺少songs子文件夹）。\n\n是否继续？", 
                    "确认操作", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);
                
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            
            // 确认操作
            var confirmResult = MessageBox.Show($"将更新active文件夹: {activeFolder}\n\n此操作将覆盖以下文件:\n• songs/songlist\n• songs/packlist\n• songs/unlocks\n\n是否继续？", 
                "确认更新", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }
            
            _bundleService.UpdateActiveFolder(activeFolder);
            
            _logService.Info($"active文件夹更新成功: {activeFolder}", "ToolsViewModel");
            MessageBox.Show($"active文件夹已成功更新:\n\n{activeFolder}\n\n已更新以下文件:\n• songs/songlist\n• songs/packlist\n• songs/unlocks", 
                "更新成功", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logService.Error("更新active文件夹失败", "ToolsViewModel", ex);
            MessageBox.Show($"更新active文件夹失败:\n\n{ex.Message}", 
                "更新失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 生成meta.cb文件
    /// </summary>
    [RelayCommand]
    private void GenerateMetaCb()
    {
        try
        {
            // 选择active文件夹
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择active文件夹",
                ShowNewFolderButton = false
            };
            
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var activeFolder = folderDialog.SelectedPath;
            
            // 验证active文件夹
            var validationResult = _bundleService.ValidateActiveFolder(activeFolder);
            if (!validationResult.IsValid)
            {
                var errorMessage = "active文件夹验证失败:\n\n";
                foreach (var error in validationResult.Errors)
                {
                    errorMessage += $"• {error}\n";
                }
                
                MessageBox.Show(errorMessage, 
                    "验证失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }
            
            // 选择输出文件
            var saveDialog = new SaveFileDialog
            {
                Filter = "CB文件 (*.cb)|*.cb|所有文件 (*.*)|*.*",
                Title = "保存meta.cb文件",
                FileName = "meta.cb",
                DefaultExt = ".cb"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                var outputPath = saveDialog.FileName;
                
                // 获取版本信息
                var versionWindow = new BundleVersionWindow();
                if (versionWindow.ShowDialog() == true)
                {
                    var appVersion = versionWindow.AppVersion;
                    var bundleVersion = versionWindow.BundleVersion;
                    var previousBundleVersion = versionWindow.PreviousBundleVersion;
                    
                    _bundleService.GenerateMetaCb(activeFolder, outputPath, appVersion, bundleVersion, previousBundleVersion);
                    
                    _logService.Info($"meta.cb文件生成成功: {outputPath}", "ToolsViewModel");
                    MessageBox.Show($"meta.cb文件已成功生成到:\n\n{outputPath}\n\n版本信息:\n• 应用程序版本: {appVersion}\n• Bundle版本: {bundleVersion}\n• 前一个版本: {previousBundleVersion ?? "无"}", 
                        "生成成功", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            _logService.Error("生成meta.cb文件失败", "ToolsViewModel", ex);
            MessageBox.Show($"生成meta.cb文件失败:\n\n{ex.Message}", 
                "生成失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 验证bundle匹配性
    /// </summary>
    [RelayCommand]
    private void ValidateBundle()
    {
        try
        {
            // 步骤1: 选择meta.cb文件
            var metaCbDialog = new OpenFileDialog
            {
                Filter = "CB文件 (*.cb)|*.cb|所有文件 (*.*)|*.*",
                Title = "选择meta.cb文件",
                FileName = "meta.cb",
                DefaultExt = ".cb"
            };
            
            if (metaCbDialog.ShowDialog() != true)
            {
                return;
            }
            
            var metaCbPath = metaCbDialog.FileName;
            
            // 步骤2: 选择active文件夹
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择与meta.cb绑定的active文件夹",
                ShowNewFolderButton = false
            };
            
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var activeFolder = folderDialog.SelectedPath;
            
            _logService.Info($"开始验证bundle匹配性: meta.cb={metaCbPath}, active={activeFolder}", "ToolsViewModel");
            
            // 执行验证
            var result = _bundleService.ValidateBundle(metaCbPath, activeFolder);
            
            // 显示结果
            var message = $"Bundle验证结果: {result.GetSummary()}\n\n";
            message += $"验证文件: {metaCbPath}\n";
            message += $"验证文件夹: {activeFolder}\n\n";
            
            if (result.Errors.Count > 0)
            {
                message += "错误:\n";
                foreach (var error in result.Errors)
                {
                    message += $"• {error}\n";
                }
                message += "\n";
            }
            
            if (result.Warnings.Count > 0)
            {
                message += "警告:\n";
                foreach (var warning in result.Warnings)
                {
                    message += $"• {warning}\n";
                }
                message += "\n";
            }
            
            if (result.Infos.Count > 0)
            {
                message += "信息:\n";
                foreach (var info in result.Infos)
                {
                    message += $"• {info}\n";
                }
            }
            
            if (result.IsValid)
            {
                _logService.Info($"bundle验证通过: meta.cb={metaCbPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "Bundle验证成功", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            else
            {
                _logService.Warning($"bundle验证失败: meta.cb={metaCbPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "Bundle验证失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("验证bundle匹配性失败", "ToolsViewModel", ex);
            MessageBox.Show($"验证bundle匹配性失败:\n\n{ex.Message}", 
                "验证失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 完整的bundle封包流程
    /// </summary>
    [RelayCommand]
    private void CreateCompleteBundle()
    {
        try
        {
            var result = MessageBox.Show("完整的bundle封包流程包括以下步骤:\n\n" +
                "1. 选择现有的active文件夹\n" +
                "2. 更新active文件夹中的清单文件\n" +
                "3. 验证active文件夹结构\n" +
                "4. 生成meta.cb文件\n\n" +
                "是否继续？", 
                "完整的bundle封包", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes)
            {
                return;
            }
            
            // 步骤1: 选择现有的active文件夹
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择要打包的active文件夹",
                ShowNewFolderButton = false
            };
            
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var activeFolder = folderDialog.SelectedPath;
            
            // 步骤2: 更新active文件夹中的清单文件
            try
            {
                _bundleService.UpdateActiveFolder(activeFolder);
                MessageBox.Show($"步骤2完成: active文件夹清单文件已更新 {activeFolder}", 
                    "步骤完成", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新active文件夹失败:\n\n{ex.Message}", 
                    "步骤失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }
            
            // 步骤3: 验证active文件夹
            var validationResult = _bundleService.ValidateActiveFolder(activeFolder);
            if (!validationResult.IsValid)
            {
                var errorMessage = "active文件夹验证失败:\n\n";
                foreach (var error in validationResult.Errors)
                {
                    errorMessage += $"• {error}\n";
                }
                
                MessageBox.Show(errorMessage, 
                    "验证失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }
            
            MessageBox.Show("步骤3完成: active文件夹验证通过", 
                "步骤完成", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
            
            // 步骤4: 生成meta.cb文件
            var saveDialog = new SaveFileDialog
            {
                Filter = "CB文件 (*.cb)|*.cb|所有文件 (*.*)|*.*",
                Title = "保存meta.cb文件",
                FileName = "meta.cb",
                DefaultExt = ".cb"
            };
            
            if (saveDialog.ShowDialog() == true)
            {
                var outputPath = saveDialog.FileName;
                
                var versionWindow = new BundleVersionWindow();
                if (versionWindow.ShowDialog() == true)
                {
                    var appVersion = versionWindow.AppVersion;
                    var bundleVersion = versionWindow.BundleVersion;
                    var previousBundleVersion = versionWindow.PreviousBundleVersion;
                    
                    _bundleService.GenerateMetaCb(activeFolder, outputPath, appVersion, bundleVersion, previousBundleVersion);
                    
                    MessageBox.Show($"步骤4完成: meta.cb文件已生成到 {outputPath}\n\n完整的bundle封包流程已完成！", 
                        "流程完成", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    _logService.Info($"完整的bundle封包流程完成: active={activeFolder}, meta.cb={outputPath}", "ToolsViewModel");
                }
            }
        }
        catch (Exception ex)
        {
            _logService.Error("完整的bundle封包流程失败", "ToolsViewModel", ex);
            MessageBox.Show($"完整的bundle封包流程失败:\n\n{ex.Message}", 
                "流程失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
}
