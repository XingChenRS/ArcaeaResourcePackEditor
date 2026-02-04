using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Converters;

/// <summary>
/// SearchLocalizationInfo JSON转换器
/// 处理search_title和search_artist字段，其中ja/ko等属性可能是数组或字符串
/// </summary>
public class SearchLocalizationConverter : JsonConverter<SearchLocalizationInfo>
{
    public override SearchLocalizationInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null!;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject, found {reader.TokenType}");

        var searchLocalization = new SearchLocalizationInfo();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Expected PropertyName, found {reader.TokenType}");

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "en":
                    if (reader.TokenType == JsonTokenType.String)
                        searchLocalization.English = reader.GetString() ?? string.Empty;
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        // 处理数组格式
                        var list = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                list.Add(reader.GetString() ?? string.Empty);
                        }
                        searchLocalization.English = string.Join(" ", list);
                    }
                    break;
                    
                case "ja":
                    if (reader.TokenType == JsonTokenType.String)
                        searchLocalization.Japanese = reader.GetString();
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        // 处理数组格式
                        var list = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                list.Add(reader.GetString() ?? string.Empty);
                        }
                        searchLocalization.Japanese = string.Join(" ", list);
                    }
                    break;
                    
                case "ko":
                    if (reader.TokenType == JsonTokenType.String)
                        searchLocalization.Korean = reader.GetString();
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        // 处理数组格式
                        var list = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                list.Add(reader.GetString() ?? string.Empty);
                        }
                        searchLocalization.Korean = string.Join(" ", list);
                    }
                    break;
                    
                case "zh-Hans":
                    if (reader.TokenType == JsonTokenType.String)
                        searchLocalization.SimplifiedChinese = reader.GetString();
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        // 处理数组格式
                        var list = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                list.Add(reader.GetString() ?? string.Empty);
                        }
                        searchLocalization.SimplifiedChinese = string.Join(" ", list);
                    }
                    break;
                    
                case "zh-Hant":
                    if (reader.TokenType == JsonTokenType.String)
                        searchLocalization.TraditionalChinese = reader.GetString();
                    else if (reader.TokenType == JsonTokenType.StartArray)
                    {
                        // 处理数组格式
                        var list = new List<string>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            if (reader.TokenType == JsonTokenType.String)
                                list.Add(reader.GetString() ?? string.Empty);
                        }
                        searchLocalization.TraditionalChinese = string.Join(" ", list);
                    }
                    break;
                    
                default:
                    // 跳过未知属性
                    if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
                        reader.Skip();
                    break;
            }
        }

        return searchLocalization;
    }

    public override void Write(Utf8JsonWriter writer, SearchLocalizationInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        if (!string.IsNullOrEmpty(value.English))
            writer.WriteString("en", value.English);
            
        if (!string.IsNullOrEmpty(value.Japanese))
            writer.WriteString("ja", value.Japanese);
            
        if (!string.IsNullOrEmpty(value.Korean))
            writer.WriteString("ko", value.Korean);
            
        if (!string.IsNullOrEmpty(value.SimplifiedChinese))
            writer.WriteString("zh-Hans", value.SimplifiedChinese);
            
        if (!string.IsNullOrEmpty(value.TraditionalChinese))
            writer.WriteString("zh-Hant", value.TraditionalChinese);
            
        writer.WriteEndObject();
    }
}


