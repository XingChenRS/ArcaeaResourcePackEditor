using System.Windows.Controls;
using ArcaeaSonglistEditor.ViewModels;

namespace ArcaeaSonglistEditor.Views;

/// <summary>
/// ValidationPage.xaml 的交互逻辑
/// </summary>
public partial class ValidationPage : Page
{
    private readonly ValidationViewModel _viewModel;

    public ValidationPage(ValidationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
