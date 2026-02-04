using System;
using System.Globalization;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// Unix时间戳转换器
/// 将Unix时间戳（秒）转换为DateTime，或将DateTime转换为Unix时间戳
/// </summary>
public class UnixTimestampConverter : IValueConverter
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// 将Unix时间戳（秒）转换为DateTime
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long unixTimestamp)
        {
            return UnixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }
        
        if (value is int intTimestamp)
        {
            return UnixEpoch.AddSeconds(intTimestamp).ToLocalTime();
        }
        
        return DateTime.MinValue;
    }

    /// <summary>
    /// 将DateTime转换为Unix时间戳（秒）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
        
        return 0L;
    }
}


