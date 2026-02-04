using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Converters;

/// <summary>
/// UnlockConditionBase的多态JSON转换器
/// </summary>
public class UnlockConditionConverter : JsonConverter<UnlockConditionBase>
{
    public override UnlockConditionBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("type", out var typeElement))
        {
            throw new JsonException("解锁条件缺少'type'属性");
        }

        var typeValue = typeElement.GetInt32();
        var type = (UnlockConditionType)typeValue;

        var json = root.GetRawText();
        
        return type switch
        {
            UnlockConditionType.Fragment => JsonSerializer.Deserialize<FragmentUnlockCondition>(json, options),
            UnlockConditionType.ClearSong => JsonSerializer.Deserialize<ClearSongUnlockCondition>(json, options),
            UnlockConditionType.PlaySong => JsonSerializer.Deserialize<PlaySongUnlockCondition>(json, options),
            UnlockConditionType.ClearSongMultiple => JsonSerializer.Deserialize<ClearSongMultipleUnlockCondition>(json, options),
            UnlockConditionType.Choice => JsonSerializer.Deserialize<ChoiceUnlockCondition>(json, options),
            UnlockConditionType.Potential => JsonSerializer.Deserialize<PotentialUnlockCondition>(json, options),
            UnlockConditionType.ClearRatingMultiple => JsonSerializer.Deserialize<ClearRatingMultipleUnlockCondition>(json, options),
            UnlockConditionType.Special => JsonSerializer.Deserialize<SpecialUnlockCondition>(json, options),
            UnlockConditionType.Character => JsonSerializer.Deserialize<CharacterUnlockCondition>(json, options),
            UnlockConditionType.Story => JsonSerializer.Deserialize<StoryUnlockCondition>(json, options),
            UnlockConditionType.CharacterForm => JsonSerializer.Deserialize<CharacterFormUnlockCondition>(json, options),
            UnlockConditionType.DifficultyConfig => JsonSerializer.Deserialize<DifficultyConfigUnlockCondition>(json, options),
            _ => throw new JsonException($"未知的解锁条件类型: {typeValue}")
        };
    }

    public override void Write(Utf8JsonWriter writer, UnlockConditionBase value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case FragmentUnlockCondition fragment:
                JsonSerializer.Serialize(writer, fragment, options);
                break;
            case ClearSongUnlockCondition clearSong:
                JsonSerializer.Serialize(writer, clearSong, options);
                break;
            case PlaySongUnlockCondition playSong:
                JsonSerializer.Serialize(writer, playSong, options);
                break;
            case ClearSongMultipleUnlockCondition clearSongMultiple:
                JsonSerializer.Serialize(writer, clearSongMultiple, options);
                break;
            case ChoiceUnlockCondition choice:
                JsonSerializer.Serialize(writer, choice, options);
                break;
            case PotentialUnlockCondition potential:
                JsonSerializer.Serialize(writer, potential, options);
                break;
            case ClearRatingMultipleUnlockCondition clearRatingMultiple:
                JsonSerializer.Serialize(writer, clearRatingMultiple, options);
                break;
            case SpecialUnlockCondition special:
                JsonSerializer.Serialize(writer, special, options);
                break;
            case CharacterUnlockCondition character:
                JsonSerializer.Serialize(writer, character, options);
                break;
            case StoryUnlockCondition story:
                JsonSerializer.Serialize(writer, story, options);
                break;
            case CharacterFormUnlockCondition characterForm:
                JsonSerializer.Serialize(writer, characterForm, options);
                break;
            case DifficultyConfigUnlockCondition difficultyConfig:
                JsonSerializer.Serialize(writer, difficultyConfig, options);
                break;
            default:
                throw new JsonException($"未知的解锁条件类型: {value.GetType().Name}");
        }
    }
}

