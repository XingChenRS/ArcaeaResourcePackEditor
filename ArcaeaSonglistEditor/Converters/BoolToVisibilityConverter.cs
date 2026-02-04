using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 布尔到可见性转换器
/// 将布尔值转换为Visibility枚举值
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// 将布尔值转换为Visibility
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 如果参数为"inverse"，则反转逻辑
            if (parameter is string param && param.ToLower() == "inverse")
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    /// <summary>
    /// 将Visibility转换为布尔值
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            // 如果参数为"inverse"，则反转逻辑
            if (parameter is string param && param.ToLower() == "inverse")
            {
                return visibility != Visibility.Visible;
            }
            
            return visibility == Visibility.Visible;
        }
        
        return false;
    }
}


