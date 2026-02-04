using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// 难度信息模型
/// </summary>
public class DifficultyInfo
{
    /// <summary>
    /// 难度等级（0=PST, 1=PRS, 2=FTR, 3=BYD, 4=ETR）
    /// </summary>
    [JsonPropertyName("ratingClass")]
    public ArcaeaRatingClass RatingClass { get; set; } = ArcaeaRatingClass.Past;

    /// <summary>
    /// 谱师
    /// </summary>
    [JsonPropertyName("chartDesigner")]
    public string ChartDesigner { get; set; } = string.Empty;

    /// <summary>
    /// 曲绘画师
    /// </summary>
    [JsonPropertyName("jacketDesigner")]
    public string JacketDesigner { get; set; } = string.Empty;

    /// <summary>
    /// 难度数值（1-20）
    /// </summary>
    [JsonPropertyName("rating")]
    public int Rating { get; set; } = 1;

    /// <summary>
    /// 难度数值是否有"+"号（3.0.0新增）
    /// </summary>
    [JsonPropertyName("ratingPlus")]
    public bool? RatingPlus { get; set; }

    /// <summary>
    /// 难度标题本地化（3.10.0新增）
    /// </summary>
    [JsonPropertyName("title_localized")]
    public LocalizationInfo? TitleLocalized { get; set; }

    /// <summary>
    /// 难度艺术家（3.12.2新增）
    /// </summary>
    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    /// <summary>
    /// 难度BPM显示（3.12.2新增）
    /// </summary>
    [JsonPropertyName("bpm")]
    public string? Bpm { get; set; }

    /// <summary>
    /// 难度基准BPM（3.12.2新增）
    /// </summary>
    [JsonPropertyName("bpm_base")]
    public double? BpmBase { get; set; }

    /// <summary>
    /// 难度背景（1.1.2新增）
    /// </summary>
    [JsonPropertyName("bg")]
    public string? Background { get; set; }

    /// <summary>
    /// 难度技能重组背景（4.0.255新增）
    /// </summary>
    [JsonPropertyName("bg_inverse")]
    public string? BackgroundInverse { get; set; }

    /// <summary>
    /// 难度是否需要世界解锁（3.0.0新增）
    /// </summary>
    [JsonPropertyName("world_unlock")]
    public bool? WorldUnlock { get; set; }

    /// <summary>
    /// 是否使用独立曲绘（3.0.0新增）
    /// </summary>
    [JsonPropertyName("jacketOverride")]
    public bool? JacketOverride { get; set; }

    /// <summary>
    /// 是否使用独立音频（3.10.0新增）
    /// </summary>
    [JsonPropertyName("audioOverride")]
    public bool? AudioOverride { get; set; }

    /// <summary>
    /// 夜晚曲绘（2.3.0新增）
    /// </summary>
    [JsonPropertyName("jacket_night")]
    public string? JacketNight { get; set; }

    /// <summary>
    /// 隐藏直到条件（4.0.0新增）
    /// </summary>
    [JsonPropertyName("hidden_until")]
    public string? HiddenUntil { get; set; }

    /// <summary>
    /// 难度发布日期时间戳
    /// </summary>
    [JsonPropertyName("date")]
    public long? Date { get; set; }

    /// <summary>
    /// 难度版本分类
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// 检查难度是否有效（必需字段）
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ChartDesigner) &&
               !string.IsNullOrWhiteSpace(JacketDesigner) &&
               Rating >= 1 && Rating <= 20;
    }

    /// <summary>
    /// 获取难度显示名称
    /// </summary>
    public string GetDisplayName()
    {
        return RatingClass switch
        {
            ArcaeaRatingClass.Past => "Past",
            ArcaeaRatingClass.Present => "Present",
            ArcaeaRatingClass.Future => "Future",
            ArcaeaRatingClass.Beyond => "Beyond",
            ArcaeaRatingClass.Eternal => "Eternal",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// 获取完整难度显示（包含等级）
    /// </summary>
    public string GetFullDisplayName()
    {
        var ratingText = Rating.ToString();
        if (RatingPlus == true)
        {
            ratingText += "+";
        }
        
        return $"{GetDisplayName()} {ratingText}";
    }

    /// <summary>
    /// 检查是否为Beyond难度
    /// </summary>
    public bool IsBeyondDifficulty()
    {
        return RatingClass == ArcaeaRatingClass.Beyond;
    }

    /// <summary>
    /// 检查是否为Eternal难度
    /// </summary>
    public bool IsEternalDifficulty()
    {
        return RatingClass == ArcaeaRatingClass.Eternal;
    }

    /// <summary>
    /// 检查是否有任何限制
    /// </summary>
    public bool HasRestrictions()
    {
        return WorldUnlock == true || 
               !string.IsNullOrWhiteSpace(HiddenUntil) ||
               JacketOverride == true ||
               AudioOverride == true;
    }

    /// <summary>
    /// 移除所有限制
    /// </summary>
    public void RemoveRestrictions()
    {
        WorldUnlock = false;
        HiddenUntil = null;
        // JacketOverride和AudioOverride保留，因为它们是功能性的不是限制
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public DifficultyInfo Clone()
    {
        return new DifficultyInfo
        {
            RatingClass = RatingClass,
            ChartDesigner = ChartDesigner,
            JacketDesigner = JacketDesigner,
            Rating = Rating,
            RatingPlus = RatingPlus,
            TitleLocalized = TitleLocalized?.Clone(),
            Artist = Artist,
            Bpm = Bpm,
            BpmBase = BpmBase,
            Background = Background,
            BackgroundInverse = BackgroundInverse,
            WorldUnlock = WorldUnlock,
            JacketOverride = JacketOverride,
            AudioOverride = AudioOverride,
            JacketNight = JacketNight,
            HiddenUntil = HiddenUntil,
            Date = Date,
            Version = Version
        };
    }
}


