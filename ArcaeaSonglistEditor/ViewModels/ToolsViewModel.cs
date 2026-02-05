using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Services;
using ArcaeaSonglistEditor.Core.Models;
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
            var errors = new List<string>();
            var warnings = new List<string>();
            
            // 检查所有解锁条件
            foreach (var unlock in _songlistService.Unlocks.Unlocks)
            {
                // 检查歌曲是否存在
                var song = _songlistService.GetSong(unlock.SongId);
                if (song == null)
                {
                    errors.Add($"解锁条件引用了不存在的歌曲: {unlock.SongId} (难度: {unlock.RatingClass})");
                    continue;
                }
                
                // 检查难度是否存在
                var difficultyExists = song.Difficulties.Any(d => d.RatingClass == unlock.RatingClass);
                if (!difficultyExists)
                {
                    errors.Add($"解锁条件引用了不存在的难度: 歌曲 {unlock.SongId} 没有 {unlock.RatingClass} 难度");
                }
                
                // 检查每个解锁条件
                foreach (var condition in unlock.Conditions)
                {
                    ValidateUnlockCondition(condition, unlock.SongId, unlock.RatingClass, errors, warnings);
                }
            }
            
            // 显示结果
            if (errors.Count == 0 && warnings.Count == 0)
            {
                _logService.Info("未发现无效解锁条件", "ToolsViewModel");
                MessageBox.Show("未发现无效解锁条件。所有解锁条件验证通过。", 
                    "检查完成", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
            else
            {
                var message = "";
                
                if (errors.Count > 0)
                {
                    message += $"发现 {errors.Count} 个错误:\n\n";
                    for (int i = 0; i < errors.Count; i++)
                    {
                        message += $"{i + 1}. {errors[i]}\n";
                    }
                    message += "\n";
                }
                
                if (warnings.Count > 0)
                {
                    message += $"发现 {warnings.Count} 个警告:\n\n";
                    for (int i = 0; i < warnings.Count; i++)
                    {
                        message += $"{i + 1}. {warnings[i]}\n";
                    }
                }
                
                _logService.Warning($"发现 {errors.Count} 个错误和 {warnings.Count} 个警告的解锁条件", "ToolsViewModel");
                MessageBox.Show(message, 
                    "检查结果", 
                    MessageBoxButton.OK, 
                    errors.Count > 0 ? MessageBoxImage.Error : MessageBoxImage.Warning);
            }
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
    /// 验证单个解锁条件
    /// </summary>
    private void ValidateUnlockCondition(UnlockConditionBase condition, string songId, ArcaeaRatingClass ratingClass, 
        List<string> errors, List<string> warnings)
    {
        switch (condition)
        {
            case ClearSongUnlockCondition clearCondition:
                ValidateClearSongCondition(clearCondition, songId, ratingClass, errors, warnings);
                break;
                
            case PlaySongUnlockCondition playCondition:
                ValidatePlaySongCondition(playCondition, songId, ratingClass, errors, warnings);
                break;
                
            case ClearSongMultipleUnlockCondition multiCondition:
                ValidateClearSongMultipleCondition(multiCondition, songId, ratingClass, errors, warnings);
                break;
                
            case FragmentUnlockCondition fragCondition:
                ValidateFragmentCondition(fragCondition, errors, warnings);
                break;
                
            case PotentialUnlockCondition potCondition:
                ValidatePotentialCondition(potCondition, errors, warnings);
                break;
                
            case ClearRatingMultipleUnlockCondition ratingCondition:
                ValidateClearRatingMultipleCondition(ratingCondition, errors, warnings);
                break;
                
            case ChoiceUnlockCondition choiceCondition:
                ValidateChoiceCondition(choiceCondition, songId, ratingClass, errors, warnings);
                break;
                
            case SpecialUnlockCondition specialCondition:
                ValidateSpecialCondition(specialCondition, errors, warnings);
                break;
                
            case CharacterUnlockCondition charCondition:
                ValidateCharacterCondition(charCondition, errors, warnings);
                break;
                
            case CharacterFormUnlockCondition formCondition:
                ValidateCharacterFormCondition(formCondition, errors, warnings);
                break;
                
            case DifficultyConfigUnlockCondition configCondition:
                ValidateDifficultyConfigCondition(configCondition, songId, ratingClass, errors, warnings);
                break;
                
            default:
                warnings.Add($"未知的解锁条件类型: {condition.Type} (歌曲: {songId}, 难度: {ratingClass})");
                break;
        }
    }
    
    /// <summary>
    /// 验证通关歌曲解锁条件
    /// </summary>
    private void ValidateClearSongCondition(ClearSongUnlockCondition condition, string currentSongId, ArcaeaRatingClass currentRatingClass,
        List<string> errors, List<string> warnings)
    {
        // 检查引用的歌曲是否存在
        var referencedSong = _songlistService.GetSong(condition.SongId);
        if (referencedSong == null)
        {
            errors.Add($"通关歌曲解锁条件引用了不存在的歌曲: {condition.SongId} (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
            return;
        }
        
        // 检查引用的难度是否存在
        var difficultyExists = referencedSong.Difficulties.Any(d => d.RatingClass == condition.SongDifficulty);
        if (!difficultyExists)
        {
            errors.Add($"通关歌曲解锁条件引用了不存在的难度: 歌曲 {condition.SongId} 没有 {condition.SongDifficulty} 难度 (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
        }
        
        // 检查评级是否有效
        if (condition.Grade < ClearGrade.Any || condition.Grade > ClearGrade.EXPlus)
        {
            warnings.Add($"通关歌曲解锁条件使用了无效的评级: {condition.Grade} (歌曲: {currentSongId}, 难度: {currentRatingClass})");
        }
    }
    
    /// <summary>
    /// 验证游玩歌曲解锁条件
    /// </summary>
    private void ValidatePlaySongCondition(PlaySongUnlockCondition condition, string currentSongId, ArcaeaRatingClass currentRatingClass,
        List<string> errors, List<string> warnings)
    {
        // 检查引用的歌曲是否存在
        var referencedSong = _songlistService.GetSong(condition.SongId);
        if (referencedSong == null)
        {
            errors.Add($"游玩歌曲解锁条件引用了不存在的歌曲: {condition.SongId} (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
            return;
        }
        
        // 检查引用的难度是否存在
        var difficultyExists = referencedSong.Difficulties.Any(d => d.RatingClass == condition.SongDifficulty);
        if (!difficultyExists)
        {
            errors.Add($"游玩歌曲解锁条件引用了不存在的难度: 歌曲 {condition.SongId} 没有 {condition.SongDifficulty} 难度 (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
        }
    }
    
    /// <summary>
    /// 验证多次通关歌曲解锁条件
    /// </summary>
    private void ValidateClearSongMultipleCondition(ClearSongMultipleUnlockCondition condition, string currentSongId, ArcaeaRatingClass currentRatingClass,
        List<string> errors, List<string> warnings)
    {
        // 检查引用的歌曲是否存在
        var referencedSong = _songlistService.GetSong(condition.SongId);
        if (referencedSong == null)
        {
            errors.Add($"多次通关歌曲解锁条件引用了不存在的歌曲: {condition.SongId} (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
            return;
        }
        
        // 检查引用的难度是否存在
        var difficultyExists = referencedSong.Difficulties.Any(d => d.RatingClass == condition.SongDifficulty);
        if (!difficultyExists)
        {
            errors.Add($"多次通关歌曲解锁条件引用了不存在的难度: 歌曲 {condition.SongId} 没有 {condition.SongDifficulty} 难度 (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
        }
        
        // 检查通关次数是否有效
        if (condition.Times <= 0)
        {
            errors.Add($"多次通关歌曲解锁条件的通关次数必须大于0: {condition.Times} (歌曲: {currentSongId}, 难度: {currentRatingClass})");
        }
        else if (condition.Times > 100)
        {
            warnings.Add($"多次通关歌曲解锁条件的通关次数可能过大: {condition.Times} (歌曲: {currentSongId}, 难度: {currentRatingClass})");
        }
        
        // 检查评级是否有效
        if (condition.Grade < ClearGrade.Any || condition.Grade > ClearGrade.EXPlus)
        {
            warnings.Add($"多次通关歌曲解锁条件使用了无效的评级: {condition.Grade} (歌曲: {currentSongId}, 难度: {currentRatingClass})");
        }
    }
    
    /// <summary>
    /// 验证残片解锁条件
    /// </summary>
    private void ValidateFragmentCondition(FragmentUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        if (condition.Credit <= 0)
        {
            errors.Add($"残片解锁条件的残片数量必须大于0: {condition.Credit}");
        }
        else if (condition.Credit > 100000)
        {
            warnings.Add($"残片解锁条件的残片数量可能过大: {condition.Credit}");
        }
    }
    
    /// <summary>
    /// 验证潜力值解锁条件
    /// </summary>
    private void ValidatePotentialCondition(PotentialUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        // 潜力值×100后的数字，所以实际潜力值 = Rating / 100
        if (condition.Rating <= 0)
        {
            errors.Add($"潜力值解锁条件的潜力值必须大于0: {condition.Rating / 100.0:F2}");
        }
        else if (condition.Rating > 13000) // 130.00潜力值
        {
            warnings.Add($"潜力值解锁条件的潜力值可能过大: {condition.Rating / 100.0:F2}");
        }
    }
    
    /// <summary>
    /// 验证多次通过对应等级歌曲解锁条件
    /// </summary>
    private void ValidateClearRatingMultipleCondition(ClearRatingMultipleUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        // 检查难度数值是否有效
        if (condition.Rating < 1 || condition.Rating > 12)
        {
            errors.Add($"多次通过对应等级歌曲解锁条件的难度数值无效: {condition.Rating} (有效范围: 1-12)");
        }
        
        // 检查通关次数是否有效
        if (condition.Count <= 0)
        {
            errors.Add($"多次通过对应等级歌曲解锁条件的通关次数必须大于0: {condition.Count}");
        }
        else if (condition.Count > 100)
        {
            warnings.Add($"多次通过对应等级歌曲解锁条件的通关次数可能过大: {condition.Count}");
        }
    }
    
    /// <summary>
    /// 验证选择类型解锁条件
    /// </summary>
    private void ValidateChoiceCondition(ChoiceUnlockCondition condition, string songId, ArcaeaRatingClass ratingClass,
        List<string> errors, List<string> warnings)
    {
        if (condition.Conditions.Count == 0)
        {
            errors.Add($"选择类型解锁条件没有子条件 (歌曲: {songId}, 难度: {ratingClass})");
            return;
        }
        
        // 验证所有子条件
        foreach (var subCondition in condition.Conditions)
        {
            ValidateUnlockCondition(subCondition, songId, ratingClass, errors, warnings);
        }
    }
    
    /// <summary>
    /// 验证特殊解锁条件
    /// </summary>
    private void ValidateSpecialCondition(SpecialUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        if (condition.Min < 0 || condition.Max < 0)
        {
            errors.Add($"特殊解锁条件的解锁进度不能为负数: min={condition.Min}, max={condition.Max}");
        }
        
        if (condition.Min > condition.Max)
        {
            errors.Add($"特殊解锁条件的解锁进度最小值不能大于最大值: min={condition.Min}, max={condition.Max}");
        }
    }
    
    /// <summary>
    /// 验证搭档解锁条件
    /// </summary>
    private void ValidateCharacterCondition(CharacterUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        if (condition.CharacterId <= 0)
        {
            errors.Add($"搭档解锁条件的搭档ID必须大于0: {condition.CharacterId}");
        }
        else if (condition.CharacterId > 100)
        {
            warnings.Add($"搭档解锁条件的搭档ID可能过大: {condition.CharacterId}");
        }
    }
    
    /// <summary>
    /// 验证搭档形态解锁条件
    /// </summary>
    private void ValidateCharacterFormCondition(CharacterFormUnlockCondition condition, List<string> errors, List<string> warnings)
    {
        if (condition.CharacterId <= 0)
        {
            errors.Add($"搭档形态解锁条件的搭档ID必须大于0: {condition.CharacterId}");
        }
        else if (condition.CharacterId > 100)
        {
            warnings.Add($"搭档形态解锁条件的搭档ID可能过大: {condition.CharacterId}");
        }
        
        if (!condition.Awakened && !condition.Inverted)
        {
            warnings.Add($"搭档形态解锁条件既不需要觉醒形态也不需要未觉醒形态，可能无效");
        }
    }
    
    /// <summary>
    /// 验证对应难度配置解锁条件
    /// </summary>
    private void ValidateDifficultyConfigCondition(DifficultyConfigUnlockCondition condition, string currentSongId, ArcaeaRatingClass currentRatingClass,
        List<string> errors, List<string> warnings)
    {
        // 检查引用的歌曲是否存在
        var referencedSong = _songlistService.GetSong(condition.SongId);
        if (referencedSong == null)
        {
            errors.Add($"对应难度配置解锁条件引用了不存在的歌曲: {condition.SongId} (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
            return;
        }
        
        // 检查引用的难度是否存在
        var difficultyExists = referencedSong.Difficulties.Any(d => d.RatingClass == condition.SongDifficulty);
        if (!difficultyExists)
        {
            errors.Add($"对应难度配置解锁条件引用了不存在的难度: 歌曲 {condition.SongId} 没有 {condition.SongDifficulty} 难度 (在歌曲 {currentSongId} 的 {currentRatingClass} 难度中)");
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
    
    /// <summary>
    /// 验证dl文件夹结构
    /// </summary>
    [RelayCommand]
    private void ValidateDlFolder()
    {
        try
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择dl文件夹",
                ShowNewFolderButton = false
            };
            
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var dlFolderPath = dialog.SelectedPath;
            
            // 创建DlImportService实例
            var dlImportService = new DlImportService(_songlistService);
            
            // 执行验证
            var result = dlImportService.ValidateDlFolder(dlFolderPath);
            
            // 显示结果
            var message = $"DL文件夹验证结果:\n\n{result.GetSummary()}\n\n";
            
            if (result.SongInfos.Count > 0)
            {
                message += "歌曲详情:\n";
                foreach (var songInfo in result.SongInfos.Take(10))
                {
                    message += $"• {songInfo.SongId}: ";
                    var parts = new List<string>();
                    if (songInfo.HasAudio) parts.Add("音频");
                    if (songInfo.ChartDifficulties.Count > 0) parts.Add($"谱面({string.Join(",", songInfo.ChartDifficulties)})");
                    if (songInfo.HasPreview) parts.Add("预览");
                    if (songInfo.HasVideo) parts.Add("视频");
                    message += string.Join(", ", parts);
                    message += $" ({songInfo.TotalFiles}个文件)\n";
                }
                
                if (result.SongInfos.Count > 10)
                {
                    message += $"  ... 还有 {result.SongInfos.Count - 10} 个歌曲\n";
                }
            }
            
            if (result.HasErrors || result.HasWarnings)
            {
                _logService.Warning($"DL文件夹验证发现 {result.Errors.Count} 个错误和 {result.Warnings.Count} 个警告: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "DL文件夹验证结果", 
                    MessageBoxButton.OK, 
                    result.HasErrors ? MessageBoxImage.Error : MessageBoxImage.Warning);
            }
            else
            {
                _logService.Info($"DL文件夹验证通过: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "DL文件夹验证结果", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("验证DL文件夹失败", "ToolsViewModel", ex);
            MessageBox.Show($"验证DL文件夹失败:\n\n{ex.Message}", 
                "验证失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// 从dl文件夹导入资源到songs文件夹
    /// </summary>
    [RelayCommand]
    private void ImportFromDlFolder()
    {
        try
        {
            // 步骤1: 选择dl文件夹
            var dlDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择dl文件夹",
                ShowNewFolderButton = false
            };
            
            if (dlDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var dlFolderPath = dlDialog.SelectedPath;
            
            // 步骤2: 选择目标songs文件夹
            var songsDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择目标songs文件夹（通常在active/songs下）",
                ShowNewFolderButton = true
            };
            
            if (songsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var songsFolderPath = songsDialog.SelectedPath;
            
            // 步骤3: 询问是否覆盖已存在的文件
            var overwriteResult = MessageBox.Show("是否覆盖已存在的文件？\n\n" +
                "• 选择'是'：覆盖已存在的文件\n" +
                "• 选择'否'：跳过已存在的文件\n" +
                "• 选择'取消'：中止操作", 
                "覆盖选项", 
                MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question);
            
            if (overwriteResult == MessageBoxResult.Cancel)
            {
                return;
            }
            
            var overwriteExisting = overwriteResult == MessageBoxResult.Yes;
            
            // 步骤4: 确认操作
            var confirmResult = MessageBox.Show($"将从以下位置导入资源:\n\n" +
                $"DL文件夹: {dlFolderPath}\n" +
                $"目标文件夹: {songsFolderPath}\n" +
                $"覆盖模式: {(overwriteExisting ? "覆盖已存在文件" : "跳过已存在文件")}\n\n" +
                $"是否继续？", 
                "确认导入", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }
            
            // 步骤5: 执行导入
            var dlImportService = new DlImportService(_songlistService);
            var result = dlImportService.ImportFromDlFolder(dlFolderPath, songsFolderPath, overwriteExisting);
            
            // 步骤6: 显示结果
            var message = $"DL文件夹导入结果:\n\n{result.GetSummary()}\n\n";
            
            if (result.ImportedFiles.Count > 0)
            {
                message += "导入的文件:\n";
                var successCount = 0;
                var skipCount = 0;
                var failCount = 0;
                
                foreach (var fileResult in result.ImportedFiles.Take(20))
                {
                    var status = fileResult.Success ? "✓" : (fileResult.Message == "文件已存在" ? "○" : "✗");
                    message += $"{status} {fileResult.SourceFile} -> {fileResult.TargetFile}";
                    if (!string.IsNullOrEmpty(fileResult.Message))
                    {
                        message += $" ({fileResult.Message})";
                    }
                    message += "\n";
                    
                    if (fileResult.Success) successCount++;
                    else if (fileResult.Message == "文件已存在") skipCount++;
                    else failCount++;
                }
                
                if (result.ImportedFiles.Count > 20)
                {
                    message += $"  ... 还有 {result.ImportedFiles.Count - 20} 个文件\n";
                }
                
                message += $"\n统计: 成功 {successCount} 个, 跳过 {skipCount} 个, 失败 {failCount} 个\n";
            }
            
            if (result.HasErrors)
            {
                _logService.Error($"DL文件夹导入失败: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "导入失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if (result.HasWarnings)
            {
                _logService.Warning($"DL文件夹导入完成但有警告: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "导入完成（有警告）", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
            else
            {
                _logService.Info($"DL文件夹导入成功: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "导入成功", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("从DL文件夹导入失败", "ToolsViewModel", ex);
            MessageBox.Show($"从DL文件夹导入失败:\n\n{ex.Message}", 
                "导入失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
    
    /// <summary>
    /// 填充dl_前缀文件夹
    /// </summary>
    [RelayCommand]
    private void FillDlFolders()
    {
        try
        {
            // 步骤1: 选择dl文件夹
            var dlDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择dl文件夹",
                ShowNewFolderButton = false
            };
            
            if (dlDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var dlFolderPath = dlDialog.SelectedPath;
            
            // 步骤2: 选择songs文件夹
            var songsDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择songs文件夹（通常在active/songs下）",
                ShowNewFolderButton = false
            };
            
            if (songsDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            var songsFolderPath = songsDialog.SelectedPath;
            
            // 步骤3: 询问是否覆盖已存在的文件
            var overwriteResult = MessageBox.Show("是否覆盖已存在的文件？\n\n" +
                "• 选择'是'：覆盖已存在的文件\n" +
                "• 选择'否'：跳过已存在的文件\n" +
                "• 选择'取消'：中止操作", 
                "覆盖选项", 
                MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question);
            
            if (overwriteResult == MessageBoxResult.Cancel)
            {
                return;
            }
            
            var overwriteExisting = overwriteResult == MessageBoxResult.Yes;
            
            // 步骤4: 确认操作
            var confirmResult = MessageBox.Show($"将填充以下位置的dl_前缀文件夹:\n\n" +
                $"DL文件夹: {dlFolderPath}\n" +
                $"Songs文件夹: {songsFolderPath}\n" +
                $"覆盖模式: {(overwriteExisting ? "覆盖已存在文件" : "跳过已存在文件")}\n\n" +
                $"此操作将:\n" +
                $"1. 查找songs文件夹中的所有dl_前缀文件夹\n" +
                $"2. 从dl文件夹中查找对应的资源文件\n" +
                $"3. 将资源文件导入到dl_前缀文件夹中\n" +
                $"4. 如果文件夹资源完整，将移除dl_前缀\n\n" +
                $"是否继续？", 
                "确认填充", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (confirmResult != MessageBoxResult.Yes)
            {
                return;
            }
            
            // 步骤5: 执行填充
            var dlImportService = new DlImportService(_songlistService);
            var result = dlImportService.FillDlFolders(dlFolderPath, songsFolderPath, overwriteExisting);
            
            // 步骤6: 显示结果
            var message = $"DL文件夹填充结果:\n\n{result.GetSummary()}\n\n";
            
            if (result.ImportedFiles.Count > 0)
            {
                message += "导入的文件:\n";
                var successCount = 0;
                var skipCount = 0;
                var failCount = 0;
                
                foreach (var fileResult in result.ImportedFiles.Take(20))
                {
                    var status = fileResult.Success ? "✓" : (fileResult.Message == "文件已存在" ? "○" : "✗");
                    message += $"{status} {fileResult.SourceFile} -> {fileResult.TargetFile}";
                    if (!string.IsNullOrEmpty(fileResult.Message))
                    {
                        message += $" ({fileResult.Message})";
                    }
                    message += "\n";
                    
                    if (fileResult.Success) successCount++;
                    else if (fileResult.Message == "文件已存在") skipCount++;
                    else failCount++;
                }
                
                if (result.ImportedFiles.Count > 20)
                {
                    message += $"  ... 还有 {result.ImportedFiles.Count - 20} 个文件\n";
                }
                
                message += $"\n统计: 成功 {successCount} 个, 跳过 {skipCount} 个, 失败 {failCount} 个\n";
            }
            
            // 添加重命名信息
            var renameWarnings = result.Warnings.Where(w => w.Contains("重命名文件夹")).ToList();
            if (renameWarnings.Count > 0)
            {
                message += "\n文件夹重命名:\n";
                foreach (var warning in renameWarnings.Take(10))
                {
                    message += $"• {warning}\n";
                }
                if (renameWarnings.Count > 10)
                {
                    message += $"  ... 还有 {renameWarnings.Count - 10} 个重命名操作\n";
                }
            }
            
            if (result.HasErrors)
            {
                _logService.Error($"DL文件夹填充失败: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "填充失败", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            else if (result.HasWarnings)
            {
                _logService.Warning($"DL文件夹填充完成但有警告: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "填充完成（有警告）", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
            }
            else
            {
                _logService.Info($"DL文件夹填充成功: {dlFolderPath}", "ToolsViewModel");
                MessageBox.Show(message, 
                    "填充成功", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            _logService.Error("填充DL文件夹失败", "ToolsViewModel", ex);
            MessageBox.Show($"填充DL文件夹失败:\n\n{ex.Message}", 
                "填充失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }
    
}
