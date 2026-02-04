using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 空值到可见性转换器
/// 将空值检查转换为Visibility枚举值
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 将值转换为Visibility
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 如果参数为"inverse"，则反转逻辑
        bool isInverse = parameter is string param && param.ToLower() == "inverse";
        
        bool isNullOrEmpty = value == null || 
                            (value is string str && string.IsNullOrEmpty(str)) ||
                            (value is System.Collections.ICollection collection && collection.Count == 0);
        
        if (isInverse)
        {
            return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// 将Visibility转换为值（不支持）
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("NullToVisibilityConverter不支持ConvertBack操作");
    }
}


