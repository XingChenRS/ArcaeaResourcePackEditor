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
/// 歌曲列表ViewModel
/// </summary>
public partial class SonglistViewModel : ObservableObject
{
    private readonly SonglistService _songlistService;
    private readonly MainWindowViewModel _mainViewModel;

    public SonglistViewModel(SonglistService songlistService, MainWindowViewModel mainViewModel)
    {
        _songlistService = songlistService;
        _mainViewModel = mainViewModel;
        
        // 初始化歌曲列表
        UpdateSongList();
        
        // 订阅数据更新事件
        _songlistService.OnDataUpdated += OnDataUpdated;
    }

    /// <summary>
    /// 歌曲列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SongInfo> _songs = new();

    /// <summary>
    /// 选中的歌曲
    /// </summary>
    [ObservableProperty]
    private SongInfo? _selectedSong;

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
        UpdateSongList();
    }

    /// <summary>
    /// 更新歌曲列表
    /// </summary>
    private void UpdateSongList()
    {
        var songs = _songlistService.Songs;
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            songs = songs.Where(s => 
                s.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.GetTitle().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        
        Songs = new ObservableCollection<SongInfo>(songs);
    }

    /// <summary>
    /// 搜索命令
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        UpdateSongList();
    }

    /// <summary>
    /// 添加歌曲命令
    /// </summary>
    [RelayCommand]
    private void AddSong()
    {
        var newSong = new SongInfo
        {
            Id = "new_song",
            TitleLocalized = new LocalizationInfo { English = "New Song" },
            Artist = "New Artist",
            Bpm = "120",
            BpmBase = 120,
            Set = "base",
            Background = "bg.jpg",
            Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Difficulties = new()
        };
        
        _songlistService.AddSong(newSong);
        _mainViewModel.SetStatusMessage("已添加新歌曲");
    }

    /// <summary>
    /// 编辑歌曲命令 - 保存当前选中的歌曲更改
    /// </summary>
    [RelayCommand]
    private void EditSong()
    {
        if (SelectedSong == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一首歌曲");
            LogService.Instance.Warning("编辑歌曲失败：未选择歌曲", "SonglistViewModel");
            return;
        }
        
        try
        {
            // 由于SelectedSong是引用，更改已经自动应用到原始对象
            // 只需要触发数据更新事件
            _songlistService.InvokeDataUpdatedEvent();
            
            _mainViewModel.SetStatusMessage($"已保存歌曲更改: {SelectedSong.Id}");
            LogService.Instance.Info($"保存歌曲更改: {SelectedSong.Id}", "SonglistViewModel");
        }
        catch (Exception ex)
        {
            _mainViewModel.SetStatusMessage($"保存失败: {ex.Message}");
            LogService.Instance.Error($"保存歌曲更改失败: {ex.Message}", "SonglistViewModel", ex);
            
            MessageBox.Show($"保存失败:\n\n{ex.Message}", 
                "保存错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 删除歌曲命令
    /// </summary>
    [RelayCommand]
    private void DeleteSong()
    {
        if (SelectedSong == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一首歌曲");
            LogService.Instance.Warning("删除歌曲失败：未选择歌曲", "SonglistViewModel");
            return;
        }
        
        try
        {
            var songId = SelectedSong.Id;
            _songlistService.DeleteSong(songId);
            
            // 清除选中的歌曲，避免引用已删除的对象
            SelectedSong = null;
            
            _mainViewModel.SetStatusMessage($"已删除歌曲: {songId}");
            LogService.Instance.Info($"删除歌曲: {songId}", "SonglistViewModel");
        }
        catch (Exception ex)
        {
            _mainViewModel.SetStatusMessage($"删除歌曲失败: {ex.Message}");
            LogService.Instance.Error($"删除歌曲失败: {ex.Message}", "SonglistViewModel", ex);
            
            MessageBox.Show($"删除歌曲失败:\n\n{ex.Message}", 
                "删除失败", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 移除限制命令
    /// </summary>
    [RelayCommand]
    private void RemoveRestrictions()
    {
        if (SelectedSong == null)
        {
            _mainViewModel.SetStatusMessage("请先选择一首歌曲");
            return;
        }
        
        SelectedSong.RemoveRestrictions();
        _songlistService.InvokeDataUpdatedEvent();
        _mainViewModel.SetStatusMessage($"已移除歌曲限制: {SelectedSong.Id}");
    }

    /// <summary>
    /// 移除所有限制命令
    /// </summary>
    [RelayCommand]
    private void RemoveAllRestrictions()
    {
        _songlistService.RemoveAllRestrictions();
        _mainViewModel.SetStatusMessage("已移除所有歌曲的限制");
    }

    /// <summary>
    /// 导入歌曲列表命令
    /// </summary>
    [RelayCommand]
    private void ImportSonglist()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "选择歌曲列表文件"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.LoadSonglistFromFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导入歌曲列表: {Path.GetFileName(dialog.FileName)}");
                LogService.Instance.Info($"歌曲列表导入成功: {dialog.FileName}", "SonglistViewModel");
            }
            catch (Exception ex)
            {
                _mainViewModel.SetStatusMessage($"导入失败: {ex.Message}");
                LogService.Instance.Error($"歌曲列表导入失败: {ex.Message}", "SonglistViewModel", ex);
                
                // 显示更详细的错误信息
                MessageBox.Show($"导入失败:\n\n{ex.Message}\n\n请检查文件格式是否正确。", 
                    "导入错误", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 导出歌曲列表命令
    /// </summary>
    [RelayCommand]
    private void ExportSonglist()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "保存歌曲列表文件",
            FileName = "songlist.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _songlistService.SaveSonglistToFile(dialog.FileName);
                _mainViewModel.SetStatusMessage($"已导出歌曲列表: {Path.GetFileName(dialog.FileName)}");
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
        UpdateSongList();
    }

    /// <summary>
    /// 当选中的歌曲变化时
    /// </summary>
    partial void OnSelectedSongChanged(SongInfo? value)
    {
        // 更新主窗口的JSON预览
        _mainViewModel.UpdateJsonPreview(value);
    }
}
