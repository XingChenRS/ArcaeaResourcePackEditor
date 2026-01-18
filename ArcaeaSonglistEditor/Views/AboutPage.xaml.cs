using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// AboutPage.xaml 的交互逻辑
/// </summary>
public partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("当前已是最新版本", "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenProjectHomepage_Click(object sender, RoutedEventArgs e)
    {
        try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/XingChenRS/ArcaeaSonglistEditor",
                    UseShellExecute = true
                });
            }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开项目主页: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        // 如果是在对话框中显示，可以关闭对话框
        // 这里只是示例，实际使用时需要根据上下文处理
        var window = Window.GetWindow(this);
        window?.Close();
    }
}

