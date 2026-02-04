using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ArcaeaSonglistEditor.Core.Converters;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// 歌曲信息模型
/// </summary>
public class SongInfo
{
    /// <summary>
    /// Link Play使用的ID（3.10.0新增）
    /// </summary>
    [JsonPropertyName("idx")]
    public int? Index { get; set; }

    /// <summary>
    /// 歌曲ID（文件夹名称）
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 歌曲标题本地化
    /// </summary>
    [JsonPropertyName("title_localized")]
    public LocalizationInfo TitleLocalized { get; set; } = new();

    /// <summary>
    /// 艺术家
    /// </summary>
    [JsonPropertyName("artist")]
    public string Artist { get; set; } = string.Empty;

    /// <summary>
    /// 艺术家本地化
    /// </summary>
    [JsonPropertyName("artist_localized")]
    public LocalizationInfo? ArtistLocalized { get; set; }

    /// <summary>
    /// BPM显示
    /// </summary>
    [JsonPropertyName("bpm")]
    public string Bpm { get; set; } = string.Empty;

    /// <summary>
    /// 基准BPM
    /// </summary>
    [JsonPropertyName("bpm_base")]
    public double BpmBase { get; set; }

    /// <summary>
    /// 所属曲包
    /// </summary>
    [JsonPropertyName("set")]
    public string Set { get; set; } = "base";

    /// <summary>
    /// 付费信息（为空则不需要购买）
    /// </summary>
    [JsonPropertyName("purchase")]
    public string Purchase { get; set; } = string.Empty;

    /// <summary>
    /// 音频预览开始时间（毫秒）
    /// </summary>
    [JsonPropertyName("audioPreview")]
    public int AudioPreview { get; set; }

    /// <summary>
    /// 音频预览结束时间（毫秒）
    /// </summary>
    [JsonPropertyName("audioPreviewEnd")]
    public int AudioPreviewEnd { get; set; }

    /// <summary>
    /// 阵营
    /// </summary>
    [JsonPropertyName("side")]
    public ArcaeaSide Side { get; set; } = ArcaeaSide.Light;

    /// <summary>
    /// 背景图片
    /// </summary>
    [JsonPropertyName("bg")]
    public string Background { get; set; } = string.Empty;

    /// <summary>
    /// 技能重组背景（4.0.255新增）
    /// </summary>
    [JsonPropertyName("bg_inverse")]
    public string? BackgroundInverse { get; set; }

    /// <summary>
    /// 日夜背景（2.3.0新增）
    /// </summary>
    [JsonPropertyName("bg_daynight")]
    public DayNightBackgroundInfo? BackgroundDayNight { get; set; }

    /// <summary>
    /// 发布日期时间戳
    /// </summary>
    [JsonPropertyName("date")]
    public long Date { get; set; }

    /// <summary>
    /// 版本分类
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 是否需要远程下载（2.0.0新增）
    /// </summary>
    [JsonPropertyName("remote_dl")]
    public bool? RemoteDownload { get; set; }

    /// <summary>
    /// 是否需要世界解锁（1.5.0新增）
    /// </summary>
    [JsonPropertyName("world_unlock")]
    public bool? WorldUnlock { get; set; }

    /// <summary>
    /// Beyond是否需要本地解锁（3.0.0新增）
    /// </summary>
    [JsonPropertyName("byd_local_unlock")]
    public bool? BeyondLocalUnlock { get; set; }

    /// <summary>
    /// 歌曲是否隐藏（已删除字段）
    /// </summary>
    [JsonPropertyName("songlist_hidden")]
    public bool? SonglistHidden { get; set; }

    /// <summary>
    /// 出处本地化（1.1.2新增）
    /// </summary>
    [JsonPropertyName("source_localized")]
    public LocalizationInfo? SourceLocalized { get; set; }

    /// <summary>
    /// 版权方（1.9.0新增）
    /// </summary>
    [JsonPropertyName("source_copyright")]
    public string? SourceCopyright { get; set; }

    /// <summary>
    /// 是否禁止直播（4.4.4删除直播模式）
    /// </summary>
    [JsonPropertyName("no_stream")]
    public bool? NoStream { get; set; }

    /// <summary>
    /// 曲绘本地化（1.9.3新增）
    /// </summary>
    [JsonPropertyName("jacket_localized")]
    public JacketLocalizationInfo? JacketLocalized { get; set; }

    /// <summary>
    /// 搜索标题本地化（4.4.6新增）
    /// </summary>
    [JsonPropertyName("search_title")]
    [JsonConverter(typeof(SearchLocalizationConverter))]
    public SearchLocalizationInfo? SearchTitle { get; set; }

    /// <summary>
    /// 搜索艺术家本地化（4.4.6新增）
    /// </summary>
    [JsonPropertyName("search_artist")]
    [JsonConverter(typeof(SearchLocalizationConverter))]
    public SearchLocalizationInfo? SearchArtist { get; set; }

    /// <summary>
    /// 隐藏直到条件（4.0.0新增）
    /// </summary>
    [JsonPropertyName("hidden_until")]
    public string? HiddenUntil { get; set; }

    /// <summary>
    /// 难度列表
    /// </summary>
    [JsonPropertyName("difficulties")]
    public List<DifficultyInfo> Difficulties { get; set; } = new();

    /// <summary>
    /// 检查歌曲是否有效（必需字段）
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               TitleLocalized.IsValid() &&
               !string.IsNullOrWhiteSpace(Artist) &&
               !string.IsNullOrWhiteSpace(Bpm) &&
               BpmBase > 0 &&
               !string.IsNullOrWhiteSpace(Set) &&
               !string.IsNullOrWhiteSpace(Background) &&
               Date > 0 &&
               Difficulties.Count > 0 &&
               Difficulties.All(d => d.IsValid());
    }

    /// <summary>
    /// 获取指定语言的标题
    /// </summary>
    public string GetTitle(string languageCode = "en")
    {
        return TitleLocalized.GetText(languageCode);
    }

    /// <summary>
    /// 获取指定语言的艺术家
    /// </summary>
    public string GetArtist(string languageCode = "en")
    {
        if (ArtistLocalized != null)
        {
            return ArtistLocalized.GetText(languageCode);
        }
        return Artist;
    }

    /// <summary>
    /// 获取最高难度
    /// </summary>
    public DifficultyInfo? GetHighestDifficulty()
    {
        return Difficulties.OrderByDescending(d => d.RatingClass)
                          .ThenByDescending(d => d.Rating)
                          .FirstOrDefault();
    }

    /// <summary>
    /// 获取指定难度等级的信息
    /// </summary>
    public DifficultyInfo? GetDifficulty(ArcaeaRatingClass ratingClass)
    {
        return Difficulties.FirstOrDefault(d => d.RatingClass == ratingClass);
    }

    /// <summary>
    /// 检查是否有任何限制
    /// </summary>
    public bool HasRestrictions()
    {
        return !string.IsNullOrWhiteSpace(Purchase) ||
               RemoteDownload == true ||
               WorldUnlock == true ||
               BeyondLocalUnlock == true ||
               NoStream == true ||
               !string.IsNullOrWhiteSpace(HiddenUntil) ||
               Difficulties.Any(d => d.HasRestrictions());
    }

    /// <summary>
    /// 移除所有限制
    /// </summary>
    public void RemoveRestrictions()
    {
        Purchase = string.Empty;
        RemoteDownload = false;
        WorldUnlock = false;
        BeyondLocalUnlock = false;
        NoStream = false;
        HiddenUntil = null;
        
        foreach (var difficulty in Difficulties)
        {
            difficulty.RemoveRestrictions();
        }
    }

    /// <summary>
    /// 检查是否为付费/下载歌曲
    /// </summary>
    public bool IsPaidSong()
    {
        return !string.IsNullOrWhiteSpace(Purchase) || RemoteDownload == true;
    }

    /// <summary>
    /// 检查是否为dl_前缀歌曲
    /// </summary>
    public bool IsDlSong()
    {
        return IsPaidSong() || WorldUnlock == true || BeyondLocalUnlock == true;
    }

    /// <summary>
    /// 获取歌曲文件夹名称（考虑dl_前缀）
    /// </summary>
    public string GetFolderName()
    {
        if (IsDlSong() && Id != "arcahv")
        {
            return $"dl_{Id}";
        }
        return Id;
    }

    /// <summary>
    /// 检查是否为特殊曲目
    /// </summary>
    public bool IsSpecialSong()
    {
        return Id == "tutorial" || Id == "random" || Id == "pack" || Id == "arcahv";
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public SongInfo Clone()
    {
        return new SongInfo
        {
            Index = Index,
            Id = Id,
            TitleLocalized = TitleLocalized.Clone(),
            Artist = Artist,
            ArtistLocalized = ArtistLocalized?.Clone(),
            Bpm = Bpm,
            BpmBase = BpmBase,
            Set = Set,
            Purchase = Purchase,
            AudioPreview = AudioPreview,
            AudioPreviewEnd = AudioPreviewEnd,
            Side = Side,
            Background = Background,
            BackgroundInverse = BackgroundInverse,
            BackgroundDayNight = BackgroundDayNight,
            Date = Date,
            Version = Version,
            RemoteDownload = RemoteDownload,
            WorldUnlock = WorldUnlock,
            BeyondLocalUnlock = BeyondLocalUnlock,
            SonglistHidden = SonglistHidden,
            SourceLocalized = SourceLocalized?.Clone(),
            SourceCopyright = SourceCopyright,
            NoStream = NoStream,
            JacketLocalized = JacketLocalized,
            SearchTitle = SearchTitle,
            SearchArtist = SearchArtist,
            HiddenUntil = HiddenUntil,
            Difficulties = Difficulties.Select(d => d.Clone()).ToList()
        };
    }
}
