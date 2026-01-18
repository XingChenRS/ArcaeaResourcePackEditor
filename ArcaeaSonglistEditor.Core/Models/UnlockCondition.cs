using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// 解锁条件基类
/// </summary>
public abstract class UnlockConditionBase
{
    /// <summary>
    /// 条件类型
    /// </summary>
    [JsonPropertyName("type")]
    public abstract UnlockConditionType Type { get; }
}

/// <summary>
/// 残片解锁条件（type: 0）
/// </summary>
public class FragmentUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Fragment;

    /// <summary>
    /// 所需残片数量
    /// </summary>
    [JsonPropertyName("credit")]
    public int Credit { get; set; }
}

/// <summary>
/// 通关歌曲解锁条件（type: 1）
/// </summary>
public class ClearSongUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.ClearSong;

    /// <summary>
    /// 需要通关的歌曲ID
    /// </summary>
    [JsonPropertyName("song_id")]
    public string SongId { get; set; } = string.Empty;

    /// <summary>
    /// 需要通关的难度等级
    /// </summary>
    [JsonPropertyName("song_difficulty")]
    public ArcaeaRatingClass SongDifficulty { get; set; }

    /// <summary>
    /// 需要达到的结算评级
    /// </summary>
    [JsonPropertyName("grade")]
    public ClearGrade Grade { get; set; } = ClearGrade.Any;
}

/// <summary>
/// 游玩歌曲解锁条件（type: 2）
/// </summary>
public class PlaySongUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.PlaySong;

    /// <summary>
    /// 需要游玩的歌曲ID
    /// </summary>
    [JsonPropertyName("song_id")]
    public string SongId { get; set; } = string.Empty;

    /// <summary>
    /// 需要游玩的难度等级
    /// </summary>
    [JsonPropertyName("song_difficulty")]
    public ArcaeaRatingClass SongDifficulty { get; set; }
}

/// <summary>
/// 多次通关歌曲解锁条件（type: 3）
/// </summary>
public class ClearSongMultipleUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.ClearSongMultiple;

    /// <summary>
    /// 需要通关的歌曲ID
    /// </summary>
    [JsonPropertyName("song_id")]
    public string SongId { get; set; } = string.Empty;

    /// <summary>
    /// 需要通关的难度等级
    /// </summary>
    [JsonPropertyName("song_difficulty")]
    public ArcaeaRatingClass SongDifficulty { get; set; }

    /// <summary>
    /// 需要达到的结算评级
    /// </summary>
    [JsonPropertyName("grade")]
    public ClearGrade Grade { get; set; } = ClearGrade.Any;

    /// <summary>
    /// 需要通关的次数
    /// </summary>
    [JsonPropertyName("times")]
    public int Times { get; set; } = 1;
}

/// <summary>
/// 选择类型解锁条件（type: 4）
/// </summary>
public class ChoiceUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Choice;

    /// <summary>
    /// 可选择的解锁条件列表
    /// </summary>
    [JsonPropertyName("conditions")]
    public List<UnlockConditionBase> Conditions { get; set; } = new();
}

/// <summary>
/// 潜力值解锁条件（type: 5）
/// </summary>
public class PotentialUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Potential;

    /// <summary>
    /// 所需潜力值×100后的数字
    /// </summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; }
}

/// <summary>
/// 多次通过对应等级歌曲解锁条件（type: 6）
/// </summary>
public class ClearRatingMultipleUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.ClearRatingMultiple;

    /// <summary>
    /// 需要通关的难度数值
    /// </summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// 难度数值是否有"+"号
    /// </summary>
    [JsonPropertyName("ratingPlus")]
    public bool? RatingPlus { get; set; }

    /// <summary>
    /// 需要通关的次数
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;
}

/// <summary>
/// 特殊解锁条件（type: 101）
/// </summary>
public class SpecialUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Special;

    /// <summary>
    /// 解锁进度最小值
    /// </summary>
    [JsonPropertyName("min")]
    public int Min { get; set; }

    /// <summary>
    /// 解锁进度最大值
    /// </summary>
    [JsonPropertyName("max")]
    public int Max { get; set; }
}

/// <summary>
/// 搭档解锁条件（type: 103）
/// </summary>
public class CharacterUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Character;

    /// <summary>
    /// 需要使用的搭档ID
    /// </summary>
    [JsonPropertyName("id")]
    public int CharacterId { get; set; }
}

/// <summary>
/// 剧情解锁条件（type: 104）
/// </summary>
public class StoryUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.Story;
}

/// <summary>
/// 搭档形态解锁条件（type: 105）
/// </summary>
public class CharacterFormUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.CharacterForm;

    /// <summary>
    /// 需要使用的搭档ID
    /// </summary>
    [JsonPropertyName("char_id")]
    public int CharacterId { get; set; }

    /// <summary>
    /// 是否需要觉醒形态
    /// </summary>
    [JsonPropertyName("awakened")]
    public bool Awakened { get; set; }

    /// <summary>
    /// 是否需要未觉醒形态
    /// </summary>
    [JsonPropertyName("inverted")]
    public bool Inverted { get; set; }
}

/// <summary>
/// 对应难度配置解锁条件（type: 106）
/// </summary>
public class DifficultyConfigUnlockCondition : UnlockConditionBase
{
    [JsonPropertyName("type")]
    public override UnlockConditionType Type => UnlockConditionType.DifficultyConfig;

    /// <summary>
    /// 需要通关的歌曲ID
    /// </summary>
    [JsonPropertyName("song_id")]
    public string SongId { get; set; } = string.Empty;

    /// <summary>
    /// 需要通关的难度等级
    /// </summary>
    [JsonPropertyName("song_difficulty")]
    public ArcaeaRatingClass SongDifficulty { get; set; }

    /// <summary>
    /// 是否需要未觉醒形态
    /// </summary>
    [JsonPropertyName("inverted")]
    public bool Inverted { get; set; }
}

/// <summary>
/// 解锁条目模型
/// </summary>
public class UnlockEntry
{
    /// <summary>
    /// 歌曲ID
    /// </summary>
    [JsonPropertyName("songId")]
    public string SongId { get; set; } = string.Empty;

    /// <summary>
    /// 难度等级
    /// </summary>
    [JsonPropertyName("ratingClass")]
    public ArcaeaRatingClass RatingClass { get; set; }

    /// <summary>
    /// 解锁条件列表
    /// </summary>
    [JsonPropertyName("conditions")]
    public List<UnlockConditionBase> Conditions { get; set; } = new();

    /// <summary>
    /// 检查解锁条目是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SongId) && Conditions.Count > 0;
    }

    /// <summary>
    /// 获取解锁条件描述
    /// </summary>
    public string GetDescription()
    {
        if (Conditions.Count == 0)
            return "无解锁条件";

        var descriptions = Conditions.Select(c => GetConditionDescription(c)).ToList();
        return string.Join(" 或 ", descriptions);
    }

    private string GetConditionDescription(UnlockConditionBase condition)
    {
        return condition switch
        {
            FragmentUnlockCondition frag => $"消耗{frag.Credit}残片",
            ClearSongUnlockCondition clear => $"通关{clear.SongId}的{clear.SongDifficulty}难度",
            PlaySongUnlockCondition play => $"游玩{play.SongId}的{play.SongDifficulty}难度",
            ClearSongMultipleUnlockCondition multi => $"通关{multi.SongId}的{multi.SongDifficulty}难度{multi.Times}次",
            PotentialUnlockCondition pot => $"达到{pot.Rating / 100.0:F2}潜力值",
            _ => $"类型{condition.Type}解锁条件"
        };
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public UnlockEntry Clone()
    {
        // 注意：这里需要实现深拷贝，但为了简化先返回浅拷贝
        return new UnlockEntry
        {
            SongId = SongId,
            RatingClass = RatingClass,
            Conditions = Conditions.ToList() // 浅拷贝列表
        };
    }
}

/// <summary>
/// 解锁列表模型
/// </summary>
public class UnlockList
{
    /// <summary>
    /// 解锁条目列表
    /// </summary>
    [JsonPropertyName("unlocks")]
    public List<UnlockEntry> Unlocks { get; set; } = new();

    /// <summary>
    /// 检查是否有解锁条件
    /// </summary>
    public bool HasUnlocks()
    {
        return Unlocks.Count > 0;
    }

    /// <summary>
    /// 清空所有解锁条件
    /// </summary>
    public void ClearAll()
    {
        Unlocks.Clear();
    }

    /// <summary>
    /// 获取指定歌曲的解锁条件
    /// </summary>
    public List<UnlockEntry> GetUnlocksForSong(string songId)
    {
        return Unlocks.Where(u => u.SongId == songId).ToList();
    }

    /// <summary>
    /// 获取指定歌曲和难度的解锁条件
    /// </summary>
    public List<UnlockEntry> GetUnlocksForSongAndDifficulty(string songId, ArcaeaRatingClass ratingClass)
    {
        return Unlocks.Where(u => u.SongId == songId && u.RatingClass == ratingClass).ToList();
    }

    /// <summary>
    /// 添加解锁条件
    /// </summary>
    public void AddUnlock(UnlockEntry entry)
    {
        Unlocks.Add(entry);
    }

    /// <summary>
    /// 移除指定歌曲的所有解锁条件
    /// </summary>
    public void RemoveUnlocksForSong(string songId)
    {
        Unlocks.RemoveAll(u => u.SongId == songId);
    }

    /// <summary>
    /// 移除指定歌曲和难度的解锁条件
    /// </summary>
    public void RemoveUnlocksForSongAndDifficulty(string songId, ArcaeaRatingClass ratingClass)
    {
        Unlocks.RemoveAll(u => u.SongId == songId && u.RatingClass == ratingClass);
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public UnlockList Clone()
    {
        return new UnlockList
        {
            Unlocks = Unlocks.Select(u => u.Clone()).ToList()
        };
    }
}
