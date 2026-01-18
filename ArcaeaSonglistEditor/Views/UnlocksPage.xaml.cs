using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// UnlocksPage.xaml 的交互逻辑
/// </summary>
public partial class UnlocksPage : Page
{
    private readonly UnlocksViewModel _viewModel;

    public UnlocksPage(UnlocksViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
