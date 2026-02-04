using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcaeaSonglistEditor.Core.Converters;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Services;

    /// <summary>
    /// 歌曲列表容器类，用于解析包含"songs"属性的JSON
    /// </summary>
    public class SonglistContainer
    {
        public List<SongInfo>? Songs { get; set; }
    }
    
    /// <summary>
    /// 曲包列表容器类，用于解析包含"packs"属性的JSON
    /// </summary>
    public class PacklistContainer
    {
        public List<PackInfo>? Packs { get; set; }
    }
    
    /// <summary>
    /// 解锁条件容器类，用于解析包含"unlocks"属性的JSON
    /// </summary>
    public class UnlocksContainer
    {
        public List<UnlockEntry>? Unlocks { get; set; }
    }

/// <summary>
/// 歌曲清单服务类
/// </summary>
public class SonglistService
{
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 歌曲列表
    /// </summary>
    public List<SongInfo> Songs { get; private set; } = new();

    /// <summary>
    /// 曲包列表
    /// </summary>
    public List<PackInfo> Packs { get; private set; } = new();

    /// <summary>
    /// 解锁条件列表
    /// </summary>
    public UnlockList Unlocks { get; private set; } = new();

    /// <summary>
    /// 当数据更新时触发的事件
    /// </summary>
    public event EventHandler? OnDataUpdated;

    public SonglistService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new UnlockConditionConverter() }
        };
    }

    /// <summary>
    /// 从JSON文件加载歌曲列表
    /// </summary>
    public void LoadSonglistFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            
            // 尝试解析JSON，提供更详细的错误信息
            try
            {
                // 首先尝试解析为包含"songs"属性的对象
                var songlistContainer = JsonSerializer.Deserialize<SonglistContainer>(json, _jsonOptions);
                if (songlistContainer != null && songlistContainer.Songs != null)
                {
                    Songs = songlistContainer.Songs;
                }
                else
                {
                    // 如果失败，尝试直接解析为歌曲列表
                    Songs = JsonSerializer.Deserialize<List<SongInfo>>(json, _jsonOptions) ?? new List<SongInfo>();
                }
            }
            catch (JsonException jsonEx)
            {
                throw new InvalidOperationException($"JSON解析失败: {jsonEx.Message}\n请确保文件格式正确，包含有效的歌曲列表数据。", jsonEx);
            }
            catch (NotSupportedException notSupportedEx)
            {
                throw new InvalidOperationException($"数据类型不支持: {notSupportedEx.Message}\n可能是JSON结构不匹配或缺少必需字段。", notSupportedEx);
            }
            
            InvokeDataUpdatedEvent();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载歌曲列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从JSON文件加载曲包列表
    /// </summary>
    public void LoadPacklistFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            
            // 尝试解析JSON，提供更详细的错误信息
            try
            {
                // 首先尝试解析为包含"packs"属性的对象
                var packlistContainer = JsonSerializer.Deserialize<PacklistContainer>(json, _jsonOptions);
                if (packlistContainer != null && packlistContainer.Packs != null)
                {
                    Packs = packlistContainer.Packs;
                }
                else
                {
                    // 如果失败，尝试直接解析为曲包列表
                    Packs = JsonSerializer.Deserialize<List<PackInfo>>(json, _jsonOptions) ?? new List<PackInfo>();
                }
            }
            catch (JsonException jsonEx)
            {
                throw new InvalidOperationException($"JSON解析失败: {jsonEx.Message}\n请确保文件格式正确，包含有效的曲包列表数据。", jsonEx);
            }
            catch (NotSupportedException notSupportedEx)
            {
                throw new InvalidOperationException($"数据类型不支持: {notSupportedEx.Message}\n可能是JSON结构不匹配或缺少必需字段。", notSupportedEx);
            }
            
            InvokeDataUpdatedEvent();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载曲包列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从JSON文件加载解锁条件
    /// </summary>
    public void LoadUnlocksFromFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            
            // 尝试解析JSON，提供更详细的错误信息
            try
            {
                // 首先尝试解析为包含"unlocks"属性的对象
                var unlocksContainer = JsonSerializer.Deserialize<UnlocksContainer>(json, _jsonOptions);
                if (unlocksContainer != null && unlocksContainer.Unlocks != null)
                {
                    Unlocks = new UnlockList { Unlocks = unlocksContainer.Unlocks };
                }
                else
                {
                    // 如果失败，尝试直接解析为UnlockList
                    Unlocks = JsonSerializer.Deserialize<UnlockList>(json, _jsonOptions) ?? new UnlockList();
                }
            }
            catch (JsonException jsonEx)
            {
                throw new InvalidOperationException($"JSON解析失败: {jsonEx.Message}\n请确保文件格式正确，包含有效的解锁条件数据。", jsonEx);
            }
            catch (NotSupportedException notSupportedEx)
            {
                throw new InvalidOperationException($"数据类型不支持: {notSupportedEx.Message}\n可能是JSON结构不匹配或缺少必需字段。", notSupportedEx);
            }
            
            InvokeDataUpdatedEvent();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载解锁条件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 保存歌曲列表到JSON文件（保持容器格式）
    /// </summary>
    public void SaveSonglistToFile(string filePath)
    {
        try
        {
            var container = new SonglistContainer { Songs = Songs };
            var json = JsonSerializer.Serialize(container, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"保存歌曲列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 保存曲包列表到JSON文件（保持容器格式）
    /// </summary>
    public void SavePacklistToFile(string filePath)
    {
        try
        {
            var container = new PacklistContainer { Packs = Packs };
            var json = JsonSerializer.Serialize(container, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"保存曲包列表失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 保存解锁条件到JSON文件（保持容器格式）
    /// </summary>
    public void SaveUnlocksToFile(string filePath)
    {
        try
        {
            var container = new UnlocksContainer { Unlocks = Unlocks.Unlocks };
            var json = JsonSerializer.Serialize(container, _jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"保存解锁条件失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取歌曲列表的JSON文本
    /// </summary>
    public string GetSonglistJsonText()
    {
        return JsonSerializer.Serialize(Songs, _jsonOptions);
    }

    /// <summary>
    /// 获取曲包列表的JSON文本
    /// </summary>
    public string GetPacklistJsonText()
    {
        return JsonSerializer.Serialize(Packs, _jsonOptions);
    }

    /// <summary>
    /// 获取解锁条件的JSON文本
    /// </summary>
    public string GetUnlocksJsonText()
    {
        return JsonSerializer.Serialize(Unlocks, _jsonOptions);
    }

    /// <summary>
    /// 添加歌曲
    /// </summary>
    public void AddSong(SongInfo song)
    {
        Songs.Add(song);
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 更新歌曲
    /// </summary>
    public void UpdateSong(string songId, SongInfo updatedSong)
    {
        var index = Songs.FindIndex(s => s.Id == songId);
        if (index >= 0)
        {
            Songs[index] = updatedSong;
            InvokeDataUpdatedEvent();
        }
    }

    /// <summary>
    /// 删除歌曲
    /// </summary>
    public void DeleteSong(string songId)
    {
        Songs.RemoveAll(s => s.Id == songId);
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 获取指定ID的歌曲
    /// </summary>
    public SongInfo? GetSong(string songId)
    {
        return Songs.FirstOrDefault(s => s.Id == songId);
    }

    /// <summary>
    /// 添加曲包
    /// </summary>
    public void AddPack(PackInfo pack)
    {
        Packs.Add(pack);
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 更新曲包
    /// </summary>
    public void UpdatePack(string packId, PackInfo updatedPack)
    {
        var index = Packs.FindIndex(p => p.Id == packId);
        if (index >= 0)
        {
            Packs[index] = updatedPack;
            InvokeDataUpdatedEvent();
        }
    }

    /// <summary>
    /// 删除曲包
    /// </summary>
    public void DeletePack(string packId)
    {
        Packs.RemoveAll(p => p.Id == packId);
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 获取指定ID的曲包
    /// </summary>
    public PackInfo? GetPack(string packId)
    {
        return Packs.FirstOrDefault(p => p.Id == packId);
    }

    /// <summary>
    /// 验证所有数据
    /// </summary>
    public ValidationResult ValidateAll()
    {
        var validationService = new ValidationService(this);
        return validationService.ValidateAll(false);
    }

    /// <summary>
    /// 验证所有数据（严格模式）
    /// </summary>
    public ValidationResult ValidateAllStrict()
    {
        var validationService = new ValidationService(this);
        return validationService.ValidateAll(true);
    }

    /// <summary>
    /// 移除所有歌曲的限制
    /// </summary>
    public void RemoveAllRestrictions()
    {
        foreach (var song in Songs)
        {
            song.RemoveRestrictions();
        }
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 移除指定曲包所有歌曲的限制
    /// </summary>
    public void RemoveRestrictionsForPack(string packId)
    {
        var songsInPack = Songs.Where(s => s.Set == packId).ToList();
        foreach (var song in songsInPack)
        {
            song.RemoveRestrictions();
        }
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 清空所有解锁条件
    /// </summary>
    public void ClearAllUnlocks()
    {
        Unlocks.ClearAll();
        InvokeDataUpdatedEvent();
    }

    /// <summary>
    /// 触发数据更新事件
    /// </summary>
    public void InvokeDataUpdatedEvent()
    {
        OnDataUpdated?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// 验证结果类
/// </summary>
public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> Infos { get; } = new();

    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    public bool HasInfos => Infos.Count > 0;
    public bool IsValid => !HasErrors;

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public void AddInfo(string info)
    {
        Infos.Add(info);
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

        if (HasInfos)
        {
            summary.Add($"发现 {Infos.Count} 条信息:");
            summary.AddRange(Infos.Take(5).Select((i, idx) => $"{idx + 1}. {i}"));
            if (Infos.Count > 5)
            {
                summary.Add($"... 还有 {Infos.Count - 5} 条信息");
            }
        }

        return string.Join(Environment.NewLine, summary);
    }
    
}
