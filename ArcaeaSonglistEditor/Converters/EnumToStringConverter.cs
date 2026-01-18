using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 枚举到字符串转换器
/// 将枚举值转换为可读的字符串
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    /// <summary>
    /// 将枚举值转换为字符串
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;
        
        // 如果是枚举类型
        if (value.GetType().IsEnum)
        {
            return value.ToString() ?? string.Empty;
        }
        
        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 将字符串转换为枚举值
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && targetType.IsEnum)
        {
            try
            {
                return Enum.Parse(targetType, stringValue);
            }
            catch
            {
                return Activator.CreateInstance(targetType)!;
            }
        }
        
        return Activator.CreateInstance(targetType)!;
    }
}
