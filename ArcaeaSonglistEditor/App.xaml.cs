using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ArcaeaSonglistEditor.Core.Services;
using ArcaeaSonglistEditor.ViewModels;
using ArcaeaSonglistEditor.Views;

namespace ArcaeaSonglistEditor;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        // 设置全局异常处理
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }
    
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        HandleUnhandledException(exception, "AppDomain");
    }
    
    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception, "Dispatcher");
        e.Handled = true; // 标记为已处理，防止应用程序崩溃
    }
    
    private void HandleUnhandledException(Exception? exception, string source)
    {
        try
        {
            var errorMessage = exception?.Message ?? "未知错误";
            var stackTrace = exception?.StackTrace ?? "无堆栈跟踪";
            
            LogService.Instance.Critical($"未处理的异常 ({source}): {errorMessage}", "App", exception);
            
            MessageBox.Show($"应用程序发生未处理的异常:\n\n{errorMessage}\n\n来源: {source}\n\n建议重启应用程序。", 
                "应用程序错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
        catch
        {
            // 如果连错误处理都失败，至少尝试显示基本错误信息
            try
            {
                MessageBox.Show("应用程序发生严重错误，建议重启应用程序。", 
                    "严重错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            catch
            {
                // 最后的手段：什么也不做
            }
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 注册服务
        services.AddSingleton<SonglistService>();
        
        // 注册ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<SonglistViewModel>();
        services.AddSingleton<PacklistViewModel>();
        services.AddSingleton<UnlocksViewModel>();
        services.AddSingleton<ValidationViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<LogViewModel>();
        services.AddSingleton<ToolsViewModel>();
        
        // 注册Views - 使用工厂方法确保页面能获取到对应的ViewModel
        services.AddSingleton<MainWindow>(provider => 
            new MainWindow(provider.GetRequiredService<MainWindowViewModel>(), provider));
        services.AddTransient<SonglistPage>(provider => 
            new SonglistPage(provider.GetRequiredService<SonglistViewModel>()));
        services.AddTransient<PacklistPage>(provider => 
            new PacklistPage(provider.GetRequiredService<PacklistViewModel>()));
        services.AddTransient<UnlocksPage>(provider => 
            new UnlocksPage(provider.GetRequiredService<UnlocksViewModel>()));
        services.AddTransient<ValidationPage>(provider => 
            new ValidationPage(provider.GetRequiredService<ValidationViewModel>()));
        services.AddTransient<ToolsPage>(provider => 
            new ToolsPage(provider.GetRequiredService<ToolsViewModel>()));
        services.AddTransient<SettingsPage>(provider => 
            new SettingsPage(provider.GetRequiredService<SettingsViewModel>()));
        services.AddTransient<AboutPage>();
        services.AddTransient<LogWindow>();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            // 记录应用程序启动
            var logService = LogService.Instance;
            logService.Info("应用程序启动", "App");
            
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            logService.Info("主窗口显示完成", "App");
        }
        catch (Exception ex)
        {
            // 即使日志服务失败，也要尝试记录
            try
            {
                LogService.Instance.Critical($"应用程序启动失败: {ex.Message}", "App", ex);
            }
            catch
            {
                // 如果日志也失败，至少显示消息框
                MessageBox.Show($"应用程序启动失败:\n\n{ex.Message}\n\n{ex.StackTrace}", 
                    "启动错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
            throw;
        }
    }
}
