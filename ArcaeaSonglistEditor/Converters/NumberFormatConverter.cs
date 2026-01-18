using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 数字格式化转换器
/// 格式化数字显示
/// </summary>
public class NumberFormatConverter : IValueConverter
{
    /// <summary>
    /// 格式化数字
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;
        
        // 获取格式参数
        string format = parameter as string ?? "N0";
        
        // 根据类型格式化
        if (value is int intValue)
        {
            return intValue.ToString(format, culture);
        }
        
        if (value is long longValue)
        {
            return longValue.ToString(format, culture);
        }
        
        if (value is double doubleValue)
        {
            return doubleValue.ToString(format, culture);
        }
        
        if (value is float floatValue)
        {
            return floatValue.ToString(format, culture);
        }
        
        if (value is decimal decimalValue)
        {
            return decimalValue.ToString(format, culture);
        }
        
        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 将格式化字符串转换回数字
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            try
            {
                if (targetType == typeof(int))
                    return int.Parse(stringValue, culture);
                
                if (targetType == typeof(long))
                    return long.Parse(stringValue, culture);
                
                if (targetType == typeof(double))
                    return double.Parse(stringValue, culture);
                
                if (targetType == typeof(float))
                    return float.Parse(stringValue, culture);
                
                if (targetType == typeof(decimal))
                    return decimal.Parse(stringValue, culture);
            }
            catch
            {
                // 解析失败时返回默认值
                return Activator.CreateInstance(targetType)!;
            }
        }
        
        return Activator.CreateInstance(targetType)!;
    }
}
