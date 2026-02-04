using System.Windows;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// BundleVersionWindow.xaml 的交互逻辑
/// </summary>
public partial class BundleVersionWindow : Window
{
    public string AppVersion { get; private set; } = string.Empty;
    public string BundleVersion { get; private set; } = string.Empty;
    public string? PreviousBundleVersion { get; private set; }
    
    public BundleVersionWindow()
    {
        InitializeComponent();
        
        // 设置默认值
        AppVersionTextBox.Text = "1.0.0";
        BundleVersionTextBox.Text = "1.0.0";
        PreviousVersionTextBox.Text = "";
        
        // 设置焦点
        Loaded += (s, e) => AppVersionTextBox.Focus();
    }
    
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // 验证输入
        if (string.IsNullOrWhiteSpace(AppVersionTextBox.Text))
        {
            MessageBox.Show("请输入应用程序版本", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            AppVersionTextBox.Focus();
            return;
        }
        
        if (string.IsNullOrWhiteSpace(BundleVersionTextBox.Text))
        {
            MessageBox.Show("请输入Bundle版本", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            BundleVersionTextBox.Focus();
            return;
        }
        
        // 保存值
        AppVersion = AppVersionTextBox.Text.Trim();
        BundleVersion = BundleVersionTextBox.Text.Trim();
        PreviousBundleVersion = string.IsNullOrWhiteSpace(PreviousVersionTextBox.Text) 
            ? null 
            : PreviousVersionTextBox.Text.Trim();
        
        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
