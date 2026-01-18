using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// ToolsPage.xaml 的交互逻辑
/// </summary>
public partial class ToolsPage : Page
{
    public ToolsPage(ToolsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
