using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ArcaeaSonglistEditor.Core.Models;
using ArcaeaSonglistEditor.Core.Services;

namespace ArcaeaSonglistEditor.ViewModels;

/// <summary>
/// 曲包列表ViewModel
/// </summary>
public partial class PacklistViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private readonly MainWindowViewModel _mainViewModel;

    public PacklistViewModel(SonglistService songlistService, MainWindowViewModel mainViewModel)
    {
        _songlistService = songlistService;
        _mainViewModel = mainViewModel;
        
        // 初始化曲包列表
        UpdatePackList();
        
        // 订阅数据更新事件
        _songlistService.OnDataUpdated += OnDataUpdated;
    }

    /// <summary>
    /// 曲包列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PackInfo> _packs = new();

    /// <summary>
    /// 选中的曲包
    /// </summary>
    [ObservableProperty]
    private PackInfo? _selectedPack;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// 数据更新时触发
    /// </summary>
    private void OnDataUpdated(object? sender, EventArgs e)
    {
        UpdatePackList();
    }

    /// <summary>
    /// 更新曲包列表
    /// </summary>
    private void UpdatePackList()
    {
        var packs = _songlistService.Packs;
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            packs = packs.Where(p => 
                p.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.GetName().Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        
        Packs = new ObservableCollection<PackInfo>(packs);
    }

    /// <summary>
    /// 搜索命令
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        UpdatePackList();
    }

    /// <summary>
    /// 添加曲包命令
    /// </summary>
    [RelayCommand]
    private void AddPack()
    {
        var newPack = new PackInfo
        {
            Id = "new_pack",
            NameLocalized = new LocalizationInfo { English = "New Pack" },
            DescriptionLocalized = new LocalizationInfo { English = "New pack description" },
            PlusCharacter = -1
        };
        
        _songlistService.AddPack(newPack);
        _mainViewModel.SetStatusMessage("已添加新曲包");
    }

    /// <summary>
    /// 编辑曲包命令 - 保存当前选中的曲包更改
    /// </summary>
    [RelayCommand]
    private void EditPack()
    {
        if (SelectedPack == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一个曲包");
            LogService.Instance.Warning("编辑曲包失败：未选择曲包", "PacklistViewModel");
            return;
        }
        
        try
        {
            // 由于SelectedPack是引用，更改已经自动应用到原始对象
            // 只需要触发数据更新事件
            _songlistService.InvokeDataUpdatedEvent();
            
            _mainViewModel.SetStatusMessage($"已保存曲包更改: {SelectedPack.Id}");
            LogService.Instance.Info($"保存曲包更改: {SelectedPack.Id}", "PacklistViewModel");
        }
        catch (Exception ex)
        {
            _mainViewModel.SetStatusMessage($"保存失败: {ex.Message}");
            LogService.Instance.Error($"保存曲包更改失败: {ex.Message}", "PacklistViewModel", ex);
            
            MessageBox.Show($"保存失败:\n\n{ex.Message}", 
                "保存错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 删除曲包命令
    /// </summary>
    [RelayCommand]
    private void DeletePack()
    {
        if (SelectedPack == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一个曲包");
            return;
        }
        
        _songlistService.DeletePack(SelectedPack.Id);
        _mainViewModel.SetStatusMessage($"已删除曲包: {SelectedPack.Id}");
    }

    /// <summary>
    /// 导入曲包列表命令
    /// </summary>
    [RelayCommand]
    private void ImportPacklist()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "选择曲包列表文件"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.LoadPacklistFromFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导入曲包列表: {Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导入失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 导出曲包列表命令
    /// </summary>
    [RelayCommand]
    private void ExportPacklist()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "保存曲包列表文件",
            FileName = "packlist.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.SavePacklistToFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导出曲包列表: {Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导出失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 移除曲包所有歌曲限制命令
    /// </summary>
    [RelayCommand]
    private void RemovePackRestrictions()
    {
        if (SelectedPack == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一个曲包");
            return;
        }
        
        _songlistService.RemoveRestrictionsForPack(SelectedPack.Id);
        _mainViewModel.SetStatusMessage($"已移除曲包 {SelectedPack.Id} 所有歌曲的限制");
    }

    /// <summary>
    /// 当搜索文本变化时
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        UpdatePackList();
    }

    /// <summary>
    /// 当选中的曲包变化时
    /// </summary>
    partial void OnSelectedPackChanged(PackInfo? value)
    {
        // 更新主窗口的JSON预览
        _mainViewModel.UpdateJsonPreview(value);
    }
}
