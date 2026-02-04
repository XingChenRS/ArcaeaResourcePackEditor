using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// Arcaea难度等级枚举
/// </summary>
public enum ArcaeaRatingClass
{
    /// <summary>
    /// Past难度
    /// </summary>
    [JsonPropertyName("0")]
    Past = 0,
    
    /// <summary>
    /// Present难度
    /// </summary>
    [JsonPropertyName("1")]
    Present = 1,
    
    /// <summary>
    /// Future难度
    /// </summary>
    [JsonPropertyName("2")]
    Future = 2,
    
    /// <summary>
    /// Beyond难度
    /// </summary>
    [JsonPropertyName("3")]
    Beyond = 3,
    
    /// <summary>
    /// Eternal难度（新版本）
    /// </summary>
    [JsonPropertyName("4")]
    Eternal = 4
}

/// <summary>
/// 阵营枚举
/// </summary>
public enum ArcaeaSide
{
    /// <summary>
    /// 光芒侧
    /// </summary>
    [JsonPropertyName("0")]
    Light = 0,
    
    /// <summary>
    /// 对立侧
    /// </summary>
    [JsonPropertyName("1")]
    Conflict = 1,
    
    /// <summary>
    /// 消色侧（4.0.0新增）
    /// </summary>
    [JsonPropertyName("2")]
    Colorless = 2
}

/// <summary>
/// 解锁条件类型枚举
/// </summary>
public enum UnlockConditionType
{
    /// <summary>
    /// 残片解锁
    /// </summary>
    [JsonPropertyName("0")]
    Fragment = 0,
    
    /// <summary>
    /// 通关歌曲解锁
    /// </summary>
    [JsonPropertyName("1")]
    ClearSong = 1,
    
    /// <summary>
    /// 游玩歌曲解锁（2.3.0新增）
    /// </summary>
    [JsonPropertyName("2")]
    PlaySong = 2,
    
    /// <summary>
    /// 多次通关歌曲解锁（2.5.0新增）
    /// </summary>
    [JsonPropertyName("3")]
    ClearSongMultiple = 3,
    
    /// <summary>
    /// 选择类型解锁（2.5.0新增）
    /// </summary>
    [JsonPropertyName("4")]
    Choice = 4,
    
    /// <summary>
    /// 潜力值解锁（2.5.0新增）
    /// </summary>
    [JsonPropertyName("5")]
    Potential = 5,
    
    /// <summary>
    /// 多次通过对应等级歌曲解锁（4.3.2新增）
    /// </summary>
    [JsonPropertyName("6")]
    ClearRatingMultiple = 6,
    
    /// <summary>
    /// 特殊解锁（1.5.0新增）
    /// </summary>
    [JsonPropertyName("101")]
    Special = 101,
    
    /// <summary>
    /// 搭档解锁（3.6.0新增）
    /// </summary>
    [JsonPropertyName("103")]
    Character = 103,
    
    /// <summary>
    /// 剧情解锁（4.0.255新增）
    /// </summary>
    [JsonPropertyName("104")]
    Story = 104,
    
    /// <summary>
    /// 搭档形态解锁（4.0.255新增）
    /// </summary>
    [JsonPropertyName("105")]
    CharacterForm = 105,
    
    /// <summary>
    /// 对应难度配置解锁（4.0.255新增）
    /// </summary>
    [JsonPropertyName("106")]
    DifficultyConfig = 106
}

/// <summary>
/// 隐藏直到条件枚举
/// </summary>
public enum HiddenUntilType
{
    /// <summary>
    /// 一直不解锁（不显示）
    /// </summary>
    [JsonPropertyName("always")]
    Always,
    
    /// <summary>
    /// 解锁任一难度后显示
    /// </summary>
    [JsonPropertyName("difficulty")]
    Difficulty,
    
    /// <summary>
    /// 解锁曲目后显示
    /// </summary>
    [JsonPropertyName("song")]
    Song
}

/// <summary>
/// 结算评级枚举（用于解锁条件）
/// </summary>
public enum ClearGrade
{
    /// <summary>
    /// 不限定
    /// </summary>
    [JsonPropertyName("0")]
    Any = 0,
    
    /// <summary>
    /// C级
    /// </summary>
    [JsonPropertyName("1")]
    C = 1,
    
    /// <summary>
    /// B级
    /// </summary>
    [JsonPropertyName("2")]
    B = 2,
    
    /// <summary>
    /// A级
    /// </summary>
    [JsonPropertyName("3")]
    A = 3,
    
    /// <summary>
    /// AA级
    /// </summary>
    [JsonPropertyName("4")]
    AA = 4,
    
    /// <summary>
    /// EX级
    /// </summary>
    [JsonPropertyName("5")]
    EX = 5,
    
    /// <summary>
    /// EX+级
    /// </summary>
    [JsonPropertyName("6")]
    EXPlus = 6
}


