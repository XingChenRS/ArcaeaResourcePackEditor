using System;
using System.Windows;
using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;
        
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 默认显示歌曲列表页面
        NavigateToPage("Songlist");
    }

    private void NavigateToPage(string pageTag)
    {
        switch (pageTag)
        {
            case "Songlist":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<SonglistPage>());
                break;
            case "Packlist":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<PacklistPage>());
                break;
            case "Unlocks":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<UnlocksPage>());
                break;
            case "Validation":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<ValidationPage>());
                break;
            case "Tools":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<ToolsPage>());
                break;
            case "Settings":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<SettingsPage>());
                break;
            case "About":
                ContentFrame.Navigate(_serviceProvider.GetRequiredService<AboutPage>());
                break;
        }
    }

    // 导航按钮点击事件处理
    private void SonglistButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Songlist");
    }

    private void PacklistButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Packlist");
    }

    private void UnlocksButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Unlocks");
    }

    private void ValidationButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Validation");
    }

    private void ToolsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Tools");
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("Settings");
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToPage("About");
    }

    private void LogButton_Click(object sender, RoutedEventArgs e)
    {
        var logWindow = _serviceProvider.GetRequiredService<LogWindow>();
        logWindow.Owner = this;
        logWindow.Show();
    }

    // JSON预览按钮事件处理
    private void CopyJsonButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CopyJsonToClipboardCommand.Execute(null);
    }

    private void RefreshJsonButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshJsonPreviewCommand.Execute(null);
    }

    // 导航视图引用（如果需要使用WPF-UI的NavigationView控件，请取消注释并添加相应的using指令）
    // private NavigationView NavigationView => FindName("NavigationView") as NavigationView;
}
