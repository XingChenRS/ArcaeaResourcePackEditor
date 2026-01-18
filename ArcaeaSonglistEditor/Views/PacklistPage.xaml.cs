using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// PacklistPage.xaml 的交互逻辑
/// </summary>
public partial class PacklistPage : Page
{
    private readonly PacklistViewModel _viewModel;

    public PacklistPage(PacklistViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
