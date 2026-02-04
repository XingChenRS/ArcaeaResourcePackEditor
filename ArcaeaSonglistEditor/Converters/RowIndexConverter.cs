using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcaeaSonglistEditor.Converters;

/// <summary>
/// 行索引转换器 - 用于DataGrid中显示行号
/// </summary>
public class RowIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DependencyObject depObj)
        {
            var item = depObj as DataGridRow;
            if (item != null)
            {
                var dataGrid = ItemsControl.ItemsControlFromItemContainer(item) as DataGrid;
                if (dataGrid != null)
                {
                    var index = dataGrid.ItemContainerGenerator.IndexFromContainer(item);
                    return (index + 1).ToString(); // 从1开始显示
                }
            }
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


