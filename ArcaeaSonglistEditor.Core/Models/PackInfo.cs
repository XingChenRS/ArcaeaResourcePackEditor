using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Models;

/// <summary>
/// 曲包信息模型
/// </summary>
public class PackInfo
{
    /// <summary>
    /// 曲包ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 是否为extend类型曲包（4.2.0新增）
    /// </summary>
    [JsonPropertyName("is_extend_pack")]
    public bool? IsExtendPack { get; set; }

    /// <summary>
    /// 是否为活动的extend曲包（4.2.0新增）
    /// </summary>
    [JsonPropertyName("is_active_extend_pack")]
    public bool? IsActiveExtendPack { get; set; }

    /// <summary>
    /// 扩展包的原曲包设定（3.3.1新增）
    /// </summary>
    [JsonPropertyName("pack_parent")]
    public string? PackParent { get; set; }

    /// <summary>
    /// 是否显示自带曲包标题框（1.1.2新增）
    /// </summary>
    [JsonPropertyName("custom_banner")]
    public bool? CustomBanner { get; set; }

    /// <summary>
    /// 是否启用菱角形曲包图片特殊显示（4.2.0新增）
    /// </summary>
    [JsonPropertyName("cutout_pack_image")]
    public bool? CutoutPackImage { get; set; }

    /// <summary>
    /// 是否启用小型曲包特殊显示（4.2.0新增）
    /// </summary>
    [JsonPropertyName("small_pack_image")]
    public bool? SmallPackImage { get; set; }

    /// <summary>
    /// 本曲包附带的搭档（-1表示无）
    /// </summary>
    [JsonPropertyName("plus_character")]
    public int PlusCharacter { get; set; } = -1;

    /// <summary>
    /// 曲包名称本地化
    /// </summary>
    [JsonPropertyName("name_localized")]
    public LocalizationInfo NameLocalized { get; set; } = new();

    /// <summary>
    /// 曲包描述本地化
    /// </summary>
    [JsonPropertyName("description_localized")]
    public LocalizationInfo DescriptionLocalized { get; set; } = new();

    /// <summary>
    /// 检查曲包是否有效（必需字段）
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               NameLocalized.IsValid() &&
               DescriptionLocalized.IsValid();
    }

    /// <summary>
    /// 获取指定语言的曲包名称
    /// </summary>
    public string GetName(string languageCode = "en")
    {
        return NameLocalized.GetText(languageCode);
    }

    /// <summary>
    /// 获取指定语言的曲包描述
    /// </summary>
    public string GetDescription(string languageCode = "en")
    {
        return DescriptionLocalized.GetText(languageCode);
    }

    /// <summary>
    /// 检查是否为扩展包
    /// </summary>
    public bool IsExtend()
    {
        return IsExtendPack == true;
    }

    /// <summary>
    /// 检查是否为活动扩展包
    /// </summary>
    public bool IsActiveExtend()
    {
        return IsActiveExtendPack == true;
    }

    /// <summary>
    /// 检查是否有父曲包
    /// </summary>
    public bool HasParent()
    {
        return !string.IsNullOrWhiteSpace(PackParent);
    }

    /// <summary>
    /// 设置曲包为extend类型
    /// </summary>
    public void SetAsExtendPack(bool isActive = false, string? parent = null)
    {
        IsExtendPack = true;
        IsActiveExtendPack = isActive;
        PackParent = parent;
    }

    /// <summary>
    /// 设置曲包为普通类型
    /// </summary>
    public void SetAsNormalPack()
    {
        IsExtendPack = false;
        IsActiveExtendPack = false;
        PackParent = null;
    }

    /// <summary>
    /// 检查是否有特殊显示设置
    /// </summary>
    public bool HasSpecialDisplay()
    {
        return CustomBanner == true || 
               CutoutPackImage == true || 
               SmallPackImage == true;
    }

    /// <summary>
    /// 获取搭档信息
    /// </summary>
    public string GetCharacterInfo()
    {
        return PlusCharacter >= 0 ? $"搭档: {PlusCharacter}" : "无搭档";
    }

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    public PackInfo Clone()
    {
        return new PackInfo
        {
            Id = Id,
            IsExtendPack = IsExtendPack,
            IsActiveExtendPack = IsActiveExtendPack,
            PackParent = PackParent,
            CustomBanner = CustomBanner,
            CutoutPackImage = CutoutPackImage,
            SmallPackImage = SmallPackImage,
            PlusCharacter = PlusCharacter,
            NameLocalized = NameLocalized.Clone(),
            DescriptionLocalized = DescriptionLocalized.Clone()
        };
    }
}


