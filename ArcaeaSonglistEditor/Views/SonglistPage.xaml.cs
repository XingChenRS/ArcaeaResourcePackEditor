using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// SonglistPage.xaml 的交互逻辑
/// </summary>
public partial class SonglistPage : Page
{
    private readonly SonglistViewModel _viewModel;

    public SonglistPage(SonglistViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
