using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 日期格式化转换器
/// 格式化日期显示
/// </summary>
public class DateFormatConverter : IValueConverter
{
    /// <summary>
    /// 格式化日期
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;
        
        // 获取格式参数
        string format = parameter as string ?? "yyyy-MM-dd";
        
        // 根据类型格式化
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(format, culture);
        }
        
        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(format, culture);
        }
        
        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 将格式化字符串转换回日期
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            try
            {
                // 获取格式参数
                string format = parameter as string ?? "yyyy-MM-dd";
                
                if (targetType == typeof(DateTime))
                    return DateTime.ParseExact(stringValue, format, culture);
                
                if (targetType == typeof(DateTimeOffset))
                    return DateTimeOffset.ParseExact(stringValue, format, culture);
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
