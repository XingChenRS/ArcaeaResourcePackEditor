using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// 本地化信息模型（用于title_localized、name_localized等）
/// </summary>
public class LocalizationInfo
{
    /// <summary>
    /// 英文文本（必需）
    /// </summary>
    [JsonPropertyName("en")]
    public string English { get; set; } = string.Empty;

    /// <summary>
    /// 日文文本
    /// </summary>
    [JsonPropertyName("ja")]
    public string? Japanese { get; set; }

    /// <summary>
    /// 韩文文本
    /// </summary>
    [JsonPropertyName("ko")]
    public string? Korean { get; set; }

    /// <summary>
    /// 简体中文文本
    /// </summary>
    [JsonPropertyName("zh-Hans")]
    public string? SimplifiedChinese { get; set; }

    /// <summary>
    /// 繁体中文文本
    /// </summary>
    [JsonPropertyName("zh-Hant")]
    public string? TraditionalChinese { get; set; }

    /// <summary>
    /// 获取指定语言的文本，如果不存在则返回英文
    /// </summary>
    public string GetText(string languageCode)
    {
        return languageCode.ToLower() switch
        {
            "en" => English,
            "ja" => Japanese ?? English,
            "ko" => Korean ?? English,
            "zh-hans" => SimplifiedChinese ?? English,
            "zh-hant" => TraditionalChinese ?? English,
            _ => English
        };
    }

    /// <summary>
    /// 设置指定语言的文本
    /// </summary>
    public void SetText(string languageCode, string text)
    {
        switch (languageCode.ToLower())
        {
            case "en":
                English = text;
                break;
            case "ja":
                Japanese = text;
                break;
            case "ko":
                Korean = text;
                break;
            case "zh-hans":
                SimplifiedChinese = text;
                break;
            case "zh-hant":
                TraditionalChinese = text;
                break;
        }
    }

    /// <summary>
    /// 检查是否所有必需语言都有文本（至少需要英文）
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(English);
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public LocalizationInfo Clone()
    {
        return new LocalizationInfo
        {
            English = English,
            Japanese = Japanese,
            Korean = Korean,
            SimplifiedChinese = SimplifiedChinese,
            TraditionalChinese = TraditionalChinese
        };
    }
}

/// <summary>
/// 搜索文本本地化信息（用于search_title、search_artist）
/// </summary>
public class SearchLocalizationInfo : LocalizationInfo
{
    /// <summary>
    /// 获取搜索词列表（按空格分割）
    /// </summary>
    public List<string> GetSearchTerms(string languageCode)
    {
        var text = GetText(languageCode);
        return text.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(term => term.Trim())
                   .Where(term => !string.IsNullOrEmpty(term))
                   .ToList();
    }
}

/// <summary>
/// 曲绘本地化信息（用于jacket_localized）
/// </summary>
public class JacketLocalizationInfo
{
    /// <summary>
    /// 英文曲绘
    /// </summary>
    [JsonPropertyName("en")]
    public bool English { get; set; }

    /// <summary>
    /// 日文曲绘
    /// </summary>
    [JsonPropertyName("ja")]
    public bool? Japanese { get; set; }

    /// <summary>
    /// 韩文曲绘
    /// </summary>
    [JsonPropertyName("ko")]
    public bool? Korean { get; set; }

    /// <summary>
    /// 简体中文曲绘
    /// </summary>
    [JsonPropertyName("zh-Hans")]
    public bool? SimplifiedChinese { get; set; }

    /// <summary>
    /// 繁体中文曲绘
    /// </summary>
    [JsonPropertyName("zh-Hant")]
    public bool? TraditionalChinese { get; set; }

    /// <summary>
    /// 检查指定语言是否需要独立曲绘
    /// </summary>
    public bool NeedsJacketForLanguage(string languageCode)
    {
        return languageCode.ToLower() switch
        {
            "en" => English,
            "ja" => Japanese ?? false,
            "ko" => Korean ?? false,
            "zh-hans" => SimplifiedChinese ?? false,
            "zh-hant" => TraditionalChinese ?? false,
            _ => false
        };
    }
}

/// <summary>
/// 日夜背景信息（用于bg_daynight）
/// </summary>
public class DayNightBackgroundInfo
{
    /// <summary>
    /// 白天背景
    /// </summary>
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    /// <summary>
    /// 夜晚背景
    /// </summary>
    [JsonPropertyName("night")]
    public string? Night { get; set; }

    /// <summary>
    /// 检查是否有效（至少有一个不为空）
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Day) || !string.IsNullOrWhiteSpace(Night);
    }
}
