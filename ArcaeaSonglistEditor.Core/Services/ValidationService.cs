using System;
using System.Collections.Generic;
using System.Linq;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Services;

/// <summary>
/// 验证服务类
/// 提供详细的数据验证功能
/// </summary>
public class ValidationService
{
    private readonly SonglistService _songlistService;

    public ValidationService(SonglistService songlistService)
    {
        _songlistService = songlistService;
    }

    /// <summary>
    /// 执行完整验证
    /// </summary>
    public ValidationResult ValidateAll(bool strictMode = false)
    {
        var result = new ValidationResult();

        ValidateSongs(result, strictMode);
        ValidatePacks(result, strictMode);
        ValidateUnlocks(result, strictMode);
        ValidateConsistency(result, strictMode);

        return result;
    }

    /// <summary>
    /// 验证歌曲数据
    /// </summary>
    private void ValidateSongs(ValidationResult result, bool strictMode)
    {
        var songs = _songlistService.Songs;
        
        // 检查歌曲数量
        if (songs.Count == 0)
        {
            result.AddWarning("歌曲列表为空");
        }

        // 验证每首歌曲
        foreach (var song in songs)
        {
            ValidateSong(song, result, strictMode);
        }

        // 检查重复ID
        var duplicateIds = songs
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicateIds)
        {
            result.AddError($"发现重复的歌曲ID: {duplicateId}");
        }
    }

    /// <summary>
    /// 验证单个歌曲
    /// </summary>
    private void ValidateSong(SongInfo song, ValidationResult result, bool strictMode)
    {
        // 基本验证
        if (!song.IsValid())
        {
            result.AddError($"歌曲 '{song.Id}' 缺少必需字段");
            return;
        }

        // 验证本地化信息
        if (!song.TitleLocalized.IsValid())
        {
            result.AddError($"歌曲 '{song.Id}' 缺少标题本地化信息");
        }

        // 验证艺术家
        if (string.IsNullOrWhiteSpace(song.Artist))
        {
            result.AddError($"歌曲 '{song.Id}' 缺少艺术家信息");
        }

        // 验证BPM
        if (string.IsNullOrWhiteSpace(song.Bpm))
        {
            result.AddError($"歌曲 '{song.Id}' 缺少BPM信息");
        }

        // 验证基础BPM
        if (song.BpmBase <= 0)
        {
            result.AddError($"歌曲 '{song.Id}' 的基础BPM必须大于0");
        }

        // 验证曲包ID
        if (string.IsNullOrWhiteSpace(song.Set))
        {
            result.AddError($"歌曲 '{song.Id}' 缺少曲包ID");
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

        // 验证背景
        if (string.IsNullOrWhiteSpace(song.Background))
        {
            result.AddError($"歌曲 '{song.Id}' 缺少背景信息");
        }

        // 验证日期
        if (song.Date <= 0)
        {
            result.AddError($"歌曲 '{song.Id}' 的日期必须大于0");
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
                ValidateDifficulty(song.Id, difficulty, result, strictMode);
            }
        }
    }

    /// <summary>
    /// 验证难度
    /// </summary>
    private void ValidateDifficulty(string songId, DifficultyInfo difficulty, ValidationResult result, bool strictMode)
    {
        if (!difficulty.IsValid())
        {
            result.AddError($"歌曲 '{songId}' 的难度 '{difficulty.GetDisplayName()}' 无效");
            return;
        }

        // 验证等级
        if (difficulty.RatingClass < 0)
        {
            result.AddError($"歌曲 '{songId}' 的难度 '{difficulty.GetDisplayName()}' 等级无效");
        }

        // 验证谱面设计师
        if (string.IsNullOrWhiteSpace(difficulty.ChartDesigner))
        {
            result.AddWarning($"歌曲 '{songId}' 的难度 '{difficulty.GetDisplayName()}' 缺少谱面设计师");
        }

        // 验证曲绘设计师
        if (string.IsNullOrWhiteSpace(difficulty.JacketDesigner))
        {
            result.AddWarning($"歌曲 '{songId}' 的难度 '{difficulty.GetDisplayName()}' 缺少曲绘设计师");
        }
    }

    /// <summary>
    /// 验证曲包数据
    /// </summary>
    private void ValidatePacks(ValidationResult result, bool strictMode)
    {
        var packs = _songlistService.Packs;
        
        // 检查曲包数量
        if (packs.Count == 0)
        {
            result.AddWarning("曲包列表为空");
        }

        // 验证每个曲包
        foreach (var pack in packs)
        {
            ValidatePack(pack, result, strictMode);
        }

        // 检查重复ID
        var duplicateIds = packs
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicateIds)
        {
            result.AddError($"发现重复的曲包ID: {duplicateId}");
        }
    }

    /// <summary>
    /// 验证单个曲包
    /// </summary>
    private void ValidatePack(PackInfo pack, ValidationResult result, bool strictMode)
    {
        // 基本验证
        if (!pack.IsValid())
        {
            result.AddError($"曲包 '{pack.Id}' 缺少必需字段");
            return;
        }

        // 验证本地化信息
        if (!pack.NameLocalized.IsValid())
        {
            result.AddError($"曲包 '{pack.Id}' 缺少名称本地化信息");
        }

        if (!pack.DescriptionLocalized.IsValid())
        {
            result.AddError($"曲包 '{pack.Id}' 缺少描述本地化信息");
        }
    }

    /// <summary>
    /// 验证解锁条件
    /// </summary>
    private void ValidateUnlocks(ValidationResult result, bool strictMode)
    {
        var unlocks = _songlistService.Unlocks.Unlocks;
        
        // 检查解锁条件数量
        if (unlocks.Count == 0)
        {
            result.AddWarning("解锁条件列表为空");
        }

        // 验证每个解锁条件
        foreach (var unlock in unlocks)
        {
            ValidateUnlock(unlock, result, strictMode);
        }
    }

    /// <summary>
    /// 验证单个解锁条件
    /// </summary>
    private void ValidateUnlock(UnlockEntry unlock, ValidationResult result, bool strictMode)
    {
        // 基本验证
        if (!unlock.IsValid())
        {
            result.AddError($"解锁条件 '{unlock.SongId}' 无效");
            return;
        }

        // 验证歌曲ID
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
    private void ValidateConsistency(ValidationResult result, bool strictMode)
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

        // 检查曲包是否有歌曲
        foreach (var pack in packs)
        {
            var songsInPack = songs.Count(s => s.Set == pack.Id);
            if (songsInPack == 0)
            {
                result.AddWarning($"曲包 '{pack.Id}' 中没有歌曲");
            }
        }

        // 检查解锁条件引用的歌曲是否存在
        foreach (var unlock in _songlistService.Unlocks.Unlocks)
        {
            if (!string.IsNullOrWhiteSpace(unlock.SongId))
            {
                var songExists = songs.Any(s => s.Id == unlock.SongId);
                if (!songExists)
                {
                    result.AddWarning($"解锁条件引用了不存在的歌曲: {unlock.SongId}");
                }
            }
        }
    }
}