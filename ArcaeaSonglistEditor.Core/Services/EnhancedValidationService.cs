using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Services;

/// <summary>
/// 增强的验证服务类
/// 提供完整的数据验证，包括文件夹验证
/// </summary>
public class EnhancedValidationService
{
    private readonly SonglistService _songlistService;
    private string? _songsFolderPath;

    public EnhancedValidationService(SonglistService songlistService)
    {
        _songlistService = songlistService;
    }

    /// <summary>
    /// 设置歌曲文件夹路径
    /// </summary>
    public void SetSongsFolderPath(string path)
    {
        _songsFolderPath = path;
    }

    /// <summary>
    /// 执行完整验证
    /// </summary>
    public EnhancedValidationResult ValidateAll(bool strictMode = false)
    {
        var result = new EnhancedValidationResult();

        // 验证数据完整性
        ValidateDataIntegrity(result, strictMode);
        
        // 验证文件夹存在性（如果设置了路径）
        if (!string.IsNullOrWhiteSpace(_songsFolderPath) && Directory.Exists(_songsFolderPath))
        {
            ValidateFolderExistence(result);
        }
        else if (strictMode)
        {
            result.AddWarning("未设置歌曲文件夹路径，跳过文件夹验证");
        }

        return result;
    }

    /// <summary>
    /// 验证数据完整性
    /// </summary>
    private void ValidateDataIntegrity(EnhancedValidationResult result, bool strictMode)
    {
        var songs = _songlistService.Songs;
        var packs = _songlistService.Packs;
        var unlocks = _songlistService.Unlocks.Unlocks;

        // 检查基本数据
        if (songs.Count == 0)
        {
            result.AddWarning("歌曲列表为空");
        }

        if (packs.Count == 0)
        {
            result.AddWarning("曲包列表为空");
        }

        // 验证每首歌曲
        foreach (var song in songs)
        {
            ValidateSongData(song, result, strictMode);
        }

        // 验证每个曲包
        foreach (var pack in packs)
        {
            ValidatePackData(pack, result, strictMode);
        }

        // 验证每个解锁条件
        foreach (var unlock in unlocks)
        {
            ValidateUnlockData(unlock, result, strictMode);
        }

        // 检查数据一致性
        ValidateDataConsistency(result, strictMode);
    }

    /// <summary>
    /// 验证歌曲数据
    /// </summary>
    private void ValidateSongData(SongInfo song, EnhancedValidationResult result, bool strictMode)
    {
        // 基本验证 - 只验证真正必需的字段
        if (string.IsNullOrWhiteSpace(song.Id))
        {
            result.AddError($"歌曲缺少ID");
            return;
        }

        // 验证标题本地化 - 至少需要英文标题
        if (song.TitleLocalized == null || string.IsNullOrWhiteSpace(song.TitleLocalized.English))
        {
            result.AddWarning($"歌曲 '{song.Id}' 缺少英文标题");
        }

        // 验证艺术家
        if (string.IsNullOrWhiteSpace(song.Artist))
        {
            result.AddWarning($"歌曲 '{song.Id}' 缺少艺术家信息");
        }

        // 验证BPM - 显示BPM可以为空，基础BPM必须有值
        if (song.BpmBase <= 0)
        {
            result.AddError($"歌曲 '{song.Id}' 的基础BPM必须大于0");
        }

        // 验证曲包ID
        if (string.IsNullOrWhiteSpace(song.Set))
        {
            result.AddWarning($"歌曲 '{song.Id}' 缺少曲包ID");
        }
        else
        {
            // 检查曲包是否存在
            var packExists = _songlistService.Packs.Any(p => p.Id == song.Set);
            if (!packExists)
            {
                result.AddWarning($"歌曲 '{song.Id}' 引用了不存在的曲包: {song.Set}");
            }
        }

        // 验证日期
        if (song.Date <= 0)
        {
            result.AddWarning($"歌曲 '{song.Id}' 的日期无效");
        }

        // 验证难度
        if (song.Difficulties.Count == 0)
        {
            result.AddError($"歌曲 '{song.Id}' 缺少难度信息");
        }
        else
        {
            foreach (var difficulty in song.Difficulties)
            {
                ValidateDifficultyData(song.Id, difficulty, result, strictMode);
            }
        }
    }

    /// <summary>
    /// 验证难度数据
    /// </summary>
    private void ValidateDifficultyData(string songId, DifficultyInfo difficulty, EnhancedValidationResult result, bool strictMode)
    {
        // 验证等级
        if (difficulty.RatingClass < 0)
        {
            result.AddError($"歌曲 '{songId}' 的难度等级无效");
        }

        // 验证难度数值
        if (difficulty.Rating < 0)
        {
            result.AddError($"歌曲 '{songId}' 的难度数值无效");
        }

        // 谱面设计师可以为空（有些官方歌曲就没有）
        // 只在严格模式下检查
        if (strictMode && string.IsNullOrWhiteSpace(difficulty.ChartDesigner))
        {
            result.AddWarning($"歌曲 '{songId}' 的难度缺少谱面设计师");
        }
    }

    /// <summary>
    /// 验证曲包数据
    /// </summary>
    private void ValidatePackData(PackInfo pack, EnhancedValidationResult result, bool strictMode)
    {
        if (string.IsNullOrWhiteSpace(pack.Id))
        {
            result.AddError($"曲包缺少ID");
            return;
        }

        // 验证名称本地化 - 至少需要英文名称
        if (pack.NameLocalized == null || string.IsNullOrWhiteSpace(pack.NameLocalized.English))
        {
            result.AddWarning($"曲包 '{pack.Id}' 缺少英文名称");
        }
    }

    /// <summary>
    /// 验证解锁条件数据
    /// </summary>
    private void ValidateUnlockData(UnlockEntry unlock, EnhancedValidationResult result, bool strictMode)
    {
        if (string.IsNullOrWhiteSpace(unlock.SongId))
        {
            result.AddError($"解锁条件缺少歌曲ID");
            return;
        }

        // 检查歌曲是否存在
        var songExists = _songlistService.Songs.Any(s => s.Id == unlock.SongId);
        if (!songExists)
        {
            result.AddWarning($"解锁条件引用了不存在的歌曲: {unlock.SongId}");
        }
    }

    /// <summary>
    /// 验证数据一致性
    /// </summary>
    private void ValidateDataConsistency(EnhancedValidationResult result, bool strictMode)
    {
        var songs = _songlistService.Songs;
        var packs = _songlistService.Packs;

        // 检查歌曲引用的曲包是否存在
        foreach (var song in songs)
        {
            if (!string.IsNullOrWhiteSpace(song.Set))
            {
                var packExists = packs.Any(p => p.Id == song.Set);
                if (!packExists)
                {
                    result.AddWarning($"歌曲 '{song.Id}' 引用了不存在的曲包: {song.Set}");
                }
            }
        }

        // 检查重复ID
        var duplicateSongIds = songs
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicateSongIds)
        {
            result.AddError($"发现重复的歌曲ID: {duplicateId}");
        }

        var duplicatePackIds = packs
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicatePackIds)
        {
            result.AddError($"发现重复的曲包ID: {duplicateId}");
        }
    }

    /// <summary>
    /// 验证文件夹存在性
    /// </summary>
    private void ValidateFolderExistence(EnhancedValidationResult result)
    {
        var songs = _songlistService.Songs;
        var songFolders = new HashSet<string>(Directory.GetDirectories(_songsFolderPath!)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))!);

        int missingFolders = 0;
        int dlFolders = 0;
        int matchingFolders = 0;

        foreach (var song in songs)
        {
            var songId = song.Id;
            
            // 检查普通文件夹
            if (songFolders.Contains(songId))
            {
                matchingFolders++;
                continue;
            }

            // 检查dl_前缀文件夹
            var dlFolderName = $"dl_{songId}";
            if (songFolders.Contains(dlFolderName))
            {
                dlFolders++;
                matchingFolders++;
                continue;
            }

            // 文件夹不存在
            missingFolders++;
            result.AddWarning($"歌曲 '{songId}' 对应的文件夹不存在（检查: {songId} 或 dl_{songId}）");
        }

        // 添加统计信息
        result.Statistics = new FolderStatistics
        {
            TotalSongsInList = songs.Count,
            TotalFolders = songFolders.Count,
            DlFolders = dlFolders,
            MatchingFolders = matchingFolders,
            MissingFolders = missingFolders
        };
    }
}

/// <summary>
/// 增强的验证结果类
/// </summary>
public class EnhancedValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public FolderStatistics? Statistics { get; set; }

    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    public bool IsValid => !HasErrors;

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public string GetSummary()
    {
        var summary = new List<string>();
        
        if (HasErrors)
        {
            summary.Add($"发现 {Errors.Count} 个错误:");
            summary.AddRange(Errors.Take(5).Select((e, i) => $"{i + 1}. {e}"));
            if (Errors.Count > 5)
            {
                summary.Add($"... 还有 {Errors.Count - 5} 个错误");
            }
        }
        else
        {
            summary.Add("验证通过，未发现错误");
        }

        if (HasWarnings)
        {
            summary.Add($"发现 {Warnings.Count} 个警告:");
            summary.AddRange(Warnings.Take(5).Select((w, i) => $"{i + 1}. {w}"));
            if (Warnings.Count > 5)
            {
                summary.Add($"... 还有 {Warnings.Count - 5} 个警告");
            }
        }

        // 添加文件夹统计信息
        if (Statistics != null)
        {
            summary.Add("");
            summary.Add("文件夹统计:");
            summary.Add($"  清单中的歌曲总数: {Statistics.TotalSongsInList}");
            summary.Add($"  实际文件夹总数: {Statistics.TotalFolders}");
            summary.Add($"  dl_前缀文件夹: {Statistics.DlFolders}");
            summary.Add($"  匹配的文件夹: {Statistics.MatchingFolders}");
            summary.Add($"  缺失的文件夹: {Statistics.MissingFolders}");
            
            if (Statistics.MissingFolders > 0)
            {
                summary.Add($"⚠️  警告: 有 {Statistics.MissingFolders} 首歌曲缺少对应的文件夹");
            }
        }

        return string.Join(Environment.NewLine, summary);
    }
}

/// <summary>
/// 文件夹统计信息
/// </summary>
public class FolderStatistics
{
    public int TotalSongsInList { get; set; }
    public int TotalFolders { get; set; }
    public int DlFolders { get; set; }
    public int MatchingFolders { get; set; }
    public int MissingFolders { get; set; }
}

