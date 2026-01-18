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
/// 解锁条件ViewModel
/// </summary>
public partial class UnlocksViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private readonly MainWindowViewModel _mainViewModel;

    public UnlocksViewModel(SonglistService songlistService, MainWindowViewModel mainViewModel)
    {
        _songlistService = songlistService;
        _mainViewModel = mainViewModel;
        
        // 初始化解锁条件列表
        UpdateUnlockList();
        
        // 订阅数据更新事件
        _songlistService.OnDataUpdated += OnDataUpdated;
    }

    /// <summary>
    /// 解锁条件列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<UnlockEntry> _unlocks = new();

    /// <summary>
    /// 选中的解锁条件
    /// </summary>
    [ObservableProperty]
    private UnlockEntry? _selectedUnlock;

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
        UpdateUnlockList();
    }

    /// <summary>
    /// 更新解锁条件列表
    /// </summary>
    private void UpdateUnlockList()
    {
        var unlocks = _songlistService.Unlocks.Unlocks;
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            unlocks = unlocks.Where(u => 
                u.SongId.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        
        Unlocks = new ObservableCollection<UnlockEntry>(unlocks);
    }

    /// <summary>
    /// 搜索命令
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        UpdateUnlockList();
    }

    /// <summary>
    /// 添加解锁条件命令
    /// </summary>
    [RelayCommand]
    private void AddUnlock()
    {
        var newUnlock = new UnlockEntry
        {
            SongId = "new_song",
            RatingClass = ArcaeaRatingClass.Past,
            Conditions = new()
        };
        
        _songlistService.Unlocks.AddUnlock(newUnlock);
        _songlistService.InvokeDataUpdatedEvent();
        _mainViewModel.SetStatusMessage("已添加新解锁条件");
    }

    /// <summary>
    /// 编辑解锁条件命令 - 保存当前选中的解锁条件更改
    /// </summary>
    [RelayCommand]
    private void EditUnlock()
    {
        if (SelectedUnlock == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一个解锁条件");
            LogService.Instance.Warning("编辑解锁条件失败：未选择解锁条件", "UnlocksViewModel");
            return;
        }
        
        try
        {
            // 由于SelectedUnlock是引用，更改已经自动应用到原始对象
            // 只需要触发数据更新事件
            _songlistService.InvokeDataUpdatedEvent();
            
            _mainViewModel.SetStatusMessage($"已保存解锁条件更改: {SelectedUnlock.SongId}");
            LogService.Instance.Info($"保存解锁条件更改: {SelectedUnlock.SongId}", "UnlocksViewModel");
        }
        catch (Exception ex)
        {
            _mainViewModel.SetStatusMessage($"保存失败: {ex.Message}");
            LogService.Instance.Error($"保存解锁条件更改失败: {ex.Message}", "UnlocksViewModel", ex);
            
            MessageBox.Show($"保存失败:\n\n{ex.Message}", 
                "保存错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 删除解锁条件命令
    /// </summary>
    [RelayCommand]
    private void DeleteUnlock()
    {
        if (SelectedUnlock == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一个解锁条件");
            return;
        }
        
        _songlistService.Unlocks.Unlocks.Remove(SelectedUnlock);
        _songlistService.InvokeDataUpdatedEvent();
        _mainViewModel.SetStatusMessage($"已删除解锁条件: {SelectedUnlock.SongId}");
    }

    /// <summary>
    /// 清空所有解锁条件命令
    /// </summary>
    [RelayCommand]
    private void ClearAllUnlocks()
    {
        _songlistService.ClearAllUnlocks();
        _mainViewModel.SetStatusMessage("已清空所有解锁条件");
    }

    /// <summary>
    /// 导入解锁条件命令
    /// </summary>
    [RelayCommand]
    private void ImportUnlocks()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "选择解锁条件文件"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.LoadUnlocksFromFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导入解锁条件: {Path.GetFileName(dialog.FileName)}");
                LogService.Instance.Info($"解锁条件导入成功: {dialog.FileName}", "UnlocksViewModel");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导入失败: {ex.Message}");
                LogService.Instance.Error($"解锁条件导入失败: {ex.Message}", "UnlocksViewModel", ex);
                
                // 显示更详细的错误信息
                MessageBox.Show($"导入失败:\n\n{ex.Message}\n\n请检查文件格式是否正确。", 
                    "导入错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 导出解锁条件命令
    /// </summary>
    [RelayCommand]
    private void ExportUnlocks()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "保存解锁条件文件",
            FileName = "unlocks.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.SaveUnlocksToFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导出解锁条件: {Path.GetFileName(dialog.FileName)}");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导出失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 当搜索文本变化时
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        UpdateUnlockList();
    }

    /// <summary>
    /// 当选中的解锁条件变化时
    /// </summary>
    partial void OnSelectedUnlockChanged(UnlockEntry? value)
    {
        // 更新主窗口的JSON预览
        _mainViewModel.UpdateJsonPreview(value);
    }
}
