using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArcaeaSonglistEditor.Core.Models;

namespace ArcaeaSonglistEditor.Core.Services;

/// <summary>
/// DL文件夹导入服务
/// 用于从Arcaea的dl文件夹导入资源到songs文件夹
/// </summary>
public class DlImportService
{
    private readonly SonglistService _songlistService;
    private readonly LogService _logService;

    public DlImportService(SonglistService songlistService)
    {
        _songlistService = songlistService ?? throw new ArgumentNullException(nameof(songlistService));
        _logService = LogService.Instance;
    }

    /// <summary>
    /// 从dl文件夹导入资源到songs文件夹
    /// </summary>
    /// <param name="dlFolderPath">dl文件夹路径</param>
    /// <param name="songsFolderPath">目标songs文件夹路径</param>
    /// <param name="overwriteExisting">是否覆盖已存在的文件</param>
    /// <returns>导入结果</returns>
    public DlImportResult ImportFromDlFolder(string dlFolderPath, string songsFolderPath, bool overwriteExisting = false)
    {
        var result = new DlImportResult();
        
        try
        {
            _logService.Info($"开始从dl文件夹导入: {dlFolderPath} -> {songsFolderPath}", "DlImportService");
            
            // 验证文件夹存在性
            if (!Directory.Exists(dlFolderPath))
            {
                result.AddError($"dl文件夹不存在: {dlFolderPath}");
                return result;
            }

            if (!Directory.Exists(songsFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(songsFolderPath);
                    _logService.Info($"创建了songs文件夹: {songsFolderPath}", "DlImportService");
                }
                catch (Exception ex)
                {
                    result.AddError($"无法创建songs文件夹: {ex.Message}");
                    return result;
                }
            }

            // 获取dl文件夹中的所有文件
            var dlFiles = Directory.GetFiles(dlFolderPath);
            if (dlFiles.Length == 0)
            {
                result.AddWarning("dl文件夹为空");
                return result;
            }

            // 分析文件并分组
            var fileGroups = AnalyzeDlFiles(dlFiles);
            _logService.Info($"分析到 {fileGroups.Count} 个歌曲文件组", "DlImportService");

            // 导入每个歌曲
            foreach (var group in fileGroups)
            {
                ImportSongGroup(group, songsFolderPath, overwriteExisting, result);
            }

            // 统计信息
            result.TotalSongsProcessed = fileGroups.Count;
            result.SuccessfulImports = result.ImportedFiles.Count(f => f.Success);
            
            _logService.Info($"导入完成: 处理了 {result.TotalSongsProcessed} 个歌曲，成功导入 {result.SuccessfulImports} 个文件", "DlImportService");
        }
        catch (Exception ex)
        {
            _logService.Error($"从dl文件夹导入失败: {ex.Message}", "DlImportService", ex);
            result.AddError($"导入失败: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 分析dl文件夹中的文件并分组
    /// </summary>
    private List<DlSongFileGroup> AnalyzeDlFiles(string[] dlFiles)
    {
        var groups = new Dictionary<string, DlSongFileGroup>();
        
        foreach (var filePath in dlFiles)
        {
            var fileName = Path.GetFileName(filePath);
            
            // 解析文件名
            if (TryParseDlFileName(fileName, out var songId, out var difficulty, out var fileType))
            {
                if (!groups.ContainsKey(songId))
                {
                    groups[songId] = new DlSongFileGroup
                    {
                        SongId = songId,
                        Files = new List<DlFileInfo>()
                    };
                }
                
                groups[songId].Files.Add(new DlFileInfo
                {
                    FilePath = filePath,
                    FileName = fileName,
                    Difficulty = difficulty,
                    FileType = fileType
                });
            }
            else
            {
                // 无法解析的文件
                _logService.Warning($"无法解析dl文件: {fileName}", "DlImportService");
            }
        }

        return groups.Values.ToList();
    }

    /// <summary>
    /// 尝试解析dl文件名
    /// </summary>
    private bool TryParseDlFileName(string fileName, out string songId, out int? difficulty, out DlFileType fileType)
    {
        songId = string.Empty;
        difficulty = null;
        fileType = DlFileType.Unknown;
        
        if (string.IsNullOrEmpty(fileName))
            return false;

        // 检查是否有下划线分隔符（可能是谱面文件：歌曲名_数字）
        var lastUnderscoreIndex = fileName.LastIndexOf('_');
        if (lastUnderscoreIndex > 0)
        {
            var potentialSongId = fileName.Substring(0, lastUnderscoreIndex);
            var suffix = fileName.Substring(lastUnderscoreIndex + 1);
            
            // 检查后缀是否是数字（谱面文件）
            if (int.TryParse(suffix, out var diff))
            {
                songId = potentialSongId;
                difficulty = diff;
                fileType = DlFileType.Chart;
                return true;
            }
        }

        // 检查是否是视频音频文件（有后缀）
        if (fileName.EndsWith("_video_audio.ogg", StringComparison.OrdinalIgnoreCase))
        {
            songId = fileName.Replace("_video_audio.ogg", "", StringComparison.OrdinalIgnoreCase);
            fileType = DlFileType.VideoAudio;
            return true;
        }

        // 检查是否是视频文件（有后缀）
        if (fileName.EndsWith("_video.mp4", StringComparison.OrdinalIgnoreCase) || 
            fileName.EndsWith("_video_1080.mp4", StringComparison.OrdinalIgnoreCase))
        {
            songId = fileName.Replace("_video.mp4", "", StringComparison.OrdinalIgnoreCase)
                            .Replace("_video_1080.mp4", "", StringComparison.OrdinalIgnoreCase);
            fileType = DlFileType.Video;
            return true;
        }

        // 检查是否有扩展名
        if (fileName.Contains('.'))
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            // 根据扩展名判断文件类型
            switch (extension)
            {
                case ".aff":
                    // 有扩展名的谱面文件
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    songId = fileNameWithoutExt;
                    fileType = DlFileType.Chart;
                    // 尝试从文件名中提取难度
                    if (fileNameWithoutExt.Contains('_'))
                    {
                        var parts = fileNameWithoutExt.Split('_');
                        if (parts.Length > 1 && int.TryParse(parts[^1], out var chartDiff))
                        {
                            songId = string.Join("_", parts.Take(parts.Length - 1));
                            difficulty = chartDiff;
                        }
                    }
                    return true;
                    
                case ".ogg":
                    // 可能是音频文件或预览音频
                    songId = Path.GetFileNameWithoutExtension(fileName);
                    fileType = DlFileType.Audio;
                    return true;
                    
                case ".mp4":
                    // 视频文件
                    songId = Path.GetFileNameWithoutExtension(fileName);
                    fileType = DlFileType.Video;
                    return true;
                    
                default:
                    // 其他文件
                    songId = Path.GetFileNameWithoutExtension(fileName);
                    fileType = DlFileType.Other;
                    return true;
            }
        }

        // 无后缀的文件：可能是音频文件
        songId = fileName;
        fileType = DlFileType.Audio;
        return true;
    }

    /// <summary>
    /// 导入单个歌曲文件组
    /// </summary>
    private void ImportSongGroup(DlSongFileGroup group, string songsFolderPath, bool overwriteExisting, DlImportResult result)
    {
        try
        {
            // 检查songs文件夹中是否已存在对应的文件夹
            // 首先检查是否有dl_前缀的文件夹
            var dlFolderName = $"dl_{group.SongId}";
            var dlFolderPath = Path.Combine(songsFolderPath, dlFolderName);
            var regularFolderPath = Path.Combine(songsFolderPath, group.SongId);
            
            string targetFolderPath;
            string targetFolderName;
            
            // 优先使用已存在的文件夹
            if (Directory.Exists(dlFolderPath))
            {
                targetFolderPath = dlFolderPath;
                targetFolderName = dlFolderName;
                _logService.Info($"使用已存在的dl文件夹: {targetFolderPath}", "DlImportService");
            }
            else if (Directory.Exists(regularFolderPath))
            {
                targetFolderPath = regularFolderPath;
                targetFolderName = group.SongId;
                _logService.Info($"使用已存在的文件夹: {targetFolderPath}", "DlImportService");
            }
            else
            {
                // 创建新文件夹
                // 检查歌曲是否在songlist中，如果在则使用dl_前缀
                var songInList = _songlistService.Songs.Any(s => s.Id == group.SongId);
                targetFolderName = songInList ? $"dl_{group.SongId}" : group.SongId;
                targetFolderPath = Path.Combine(songsFolderPath, targetFolderName);
                
                Directory.CreateDirectory(targetFolderPath);
                _logService.Info($"创建了歌曲文件夹: {targetFolderPath}", "DlImportService");
            }
            
            // 记录文件夹中的现有文件（用于调试）
            var existingFiles = Directory.GetFiles(targetFolderPath);
            _logService.Info($"文件夹 '{targetFolderName}' 中原有 {existingFiles.Length} 个文件", "DlImportService");
            if (existingFiles.Length > 0)
            {
                _logService.Info($"现有文件: {string.Join(", ", existingFiles.Select(Path.GetFileName))}", "DlImportService");
            }

            // 处理每个文件
            foreach (var fileInfo in group.Files)
            {
                ImportFile(fileInfo, targetFolderPath, overwriteExisting, result);
            }
            
            // 记录导入后的文件
            var finalFiles = Directory.GetFiles(targetFolderPath);
            _logService.Info($"文件夹 '{targetFolderName}' 中现有 {finalFiles.Length} 个文件", "DlImportService");
            
            result.ProcessedSongs.Add(group.SongId);
        }
        catch (Exception ex)
        {
            _logService.Error($"导入歌曲组失败: {group.SongId} - {ex.Message}", "DlImportService", ex);
            result.AddError($"导入歌曲 {group.SongId} 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导入单个文件
    /// </summary>
    private void ImportFile(DlFileInfo fileInfo, string targetFolderPath, bool overwriteExisting, DlImportResult result)
    {
        try
        {
            string targetFileName;
            
            // 根据文件类型确定目标文件名
            switch (fileInfo.FileType)
            {
                case DlFileType.Audio:
                    targetFileName = "base.ogg";
                    break;
                    
                case DlFileType.Chart:
                    if (fileInfo.Difficulty.HasValue)
                    {
                        // 映射难度数字到文件名
                        // 0 -> 0.aff (Past)
                        // 1 -> 1.aff (Present)
                        // 2 -> 2.aff (Future)
                        // 3 -> 3.aff (Beyond)
                        // 4 -> 4.aff (特殊难度，如Beyond+)
                        targetFileName = $"{fileInfo.Difficulty.Value}.aff";
                    }
                    else
                    {
                        _logService.Warning($"谱面文件缺少难度信息: {fileInfo.FileName}", "DlImportService");
                        return;
                    }
                    break;
                    
                case DlFileType.AudioPreview:
                    targetFileName = "preview.ogg";
                    break;
                    
                case DlFileType.Video:
                    targetFileName = "video.mp4";
                    break;
                    
                case DlFileType.VideoAudio:
                    targetFileName = "video_audio.ogg";
                    break;
                    
                default:
                    // 其他文件保持原文件名
                    targetFileName = fileInfo.FileName;
                    break;
            }
            
            var targetFilePath = Path.Combine(targetFolderPath, targetFileName);
            
            // 检查目标文件是否已存在
            if (File.Exists(targetFilePath) && !overwriteExisting)
            {
                _logService.Info($"文件已存在，跳过: {targetFilePath}", "DlImportService");
                result.ImportedFiles.Add(new FileImportResult
                {
                    SourceFile = fileInfo.FileName,
                    TargetFile = targetFileName,
                    Success = false,
                    Message = "文件已存在"
                });
                return;
            }
            
            // 复制文件
            File.Copy(fileInfo.FilePath, targetFilePath, overwriteExisting);
            
            _logService.Info($"导入文件: {fileInfo.FileName} -> {targetFileName}", "DlImportService");
            result.ImportedFiles.Add(new FileImportResult
            {
                SourceFile = fileInfo.FileName,
                TargetFile = targetFileName,
                Success = true,
                Message = "导入成功"
            });
        }
        catch (Exception ex)
        {
            _logService.Error($"导入文件失败: {fileInfo.FileName} - {ex.Message}", "DlImportService", ex);
            result.ImportedFiles.Add(new FileImportResult
            {
                SourceFile = fileInfo.FileName,
                TargetFile = string.Empty,
                Success = false,
                Message = $"导入失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 验证dl文件夹结构
    /// </summary>
    public DlValidationResult ValidateDlFolder(string dlFolderPath)
    {
        var result = new DlValidationResult();
        
        try
        {
            if (!Directory.Exists(dlFolderPath))
            {
                result.AddError($"dl文件夹不存在: {dlFolderPath}");
                return result;
            }

            var dlFiles = Directory.GetFiles(dlFolderPath);
            if (dlFiles.Length == 0)
            {
                result.AddWarning("dl文件夹为空");
                return result;
            }

            // 分析文件
            var fileGroups = AnalyzeDlFiles(dlFiles);
            
            // 统计信息
            result.TotalFiles = dlFiles.Length;
            result.TotalSongs = fileGroups.Count;
            
            foreach (var group in fileGroups)
            {
                var songInfo = new DlSongInfo
                {
                    SongId = group.SongId,
                    HasAudio = group.Files.Any(f => f.FileType == DlFileType.Audio),
                    ChartDifficulties = group.Files
                        .Where(f => f.FileType == DlFileType.Chart && f.Difficulty.HasValue)
                        .Select(f => f.Difficulty.Value)
                        .OrderBy(d => d)
                        .ToList(),
                    HasPreview = group.Files.Any(f => f.FileType == DlFileType.AudioPreview),
                    HasVideo = group.Files.Any(f => f.FileType == DlFileType.Video),
                    TotalFiles = group.Files.Count
                };
                
                result.SongInfos.Add(songInfo);
                
                // 检查是否在songlist中
                var songInList = _songlistService.Songs.Any(s => s.Id == group.SongId);
                if (!songInList)
                {
                    result.AddWarning($"歌曲 '{group.SongId}' 不在songlist中");
                }
                
                // 检查是否有音频文件
                if (!songInfo.HasAudio)
                {
                    result.AddWarning($"歌曲 '{group.SongId}' 缺少音频文件");
                }
                
                // 检查是否有谱面文件
                if (songInfo.ChartDifficulties.Count == 0)
                {
                    result.AddWarning($"歌曲 '{group.SongId}' 缺少谱面文件");
                }
            }
        }
        catch (Exception ex)
        {
            result.AddError($"验证dl文件夹失败: {ex.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// 验证songs文件夹结构
    /// </summary>
    public SongsValidationResult ValidateSongsFolder(string songsFolderPath)
    {
        var result = new SongsValidationResult();
        
        try
        {
            if (!Directory.Exists(songsFolderPath))
            {
                result.AddError($"songs文件夹不存在: {songsFolderPath}");
                return result;
            }

            // 获取所有歌曲文件夹
            var songFolders = Directory.GetDirectories(songsFolderPath);
            result.TotalSongFolders = songFolders.Length;
            
            foreach (var folderPath in songFolders)
            {
                var folderName = Path.GetFileName(folderPath);
                var songInfo = new SongsFolderInfo
                {
                    FolderName = folderName,
                    FolderPath = folderPath,
                    Files = Directory.GetFiles(folderPath).Select(Path.GetFileName).ToList()
                };
                
                result.SongFolders.Add(songInfo);
                
                // 检查文件夹名称格式
                if (folderName.StartsWith("dl_"))
                {
                    var songId = folderName.Substring(3); // 移除"dl_"前缀
                    songInfo.IsDlFolder = true;
                    songInfo.SongId = songId;
                    
                    // 检查是否在songlist中
                    var songInList = _songlistService.Songs.Any(s => s.Id == songId);
                    if (!songInList)
                    {
                        result.AddWarning($"dl文件夹 '{folderName}' 对应的歌曲 '{songId}' 不在songlist中");
                    }
                }
                else
                {
                    songInfo.SongId = folderName;
                    songInfo.IsDlFolder = false;
                }
                
                // 检查必要的文件
                var hasBaseOgg = songInfo.Files.Any(f => f.Equals("base.ogg", StringComparison.OrdinalIgnoreCase));
                var hasChartFiles = songInfo.Files.Any(f => f.EndsWith(".aff", StringComparison.OrdinalIgnoreCase));
                
                if (!hasBaseOgg)
                {
                    result.AddWarning($"歌曲文件夹 '{folderName}' 缺少base.ogg文件");
                }
                
                if (!hasChartFiles)
                {
                    result.AddWarning($"歌曲文件夹 '{folderName}' 缺少谱面文件(.aff)");
                }
                
                // 统计文件类型
                songInfo.HasBaseOgg = hasBaseOgg;
                songInfo.HasChartFiles = hasChartFiles;
                songInfo.HasJacketFiles = songInfo.Files.Any(f => 
                    f.StartsWith("jacket_", StringComparison.OrdinalIgnoreCase) || 
                    f.StartsWith("jacket.", StringComparison.OrdinalIgnoreCase));
                songInfo.HasVideo = songInfo.Files.Any(f => 
                    f.Equals("video.mp4", StringComparison.OrdinalIgnoreCase) ||
                    f.Equals("video_audio.ogg", StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex)
        {
            result.AddError($"验证songs文件夹失败: {ex.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// 填充dl_前缀文件夹并从dl文件夹导入资源
    /// </summary>
    public DlImportResult FillDlFolders(string dlFolderPath, string songsFolderPath, bool overwriteExisting = false)
    {
        var result = new DlImportResult();
        
        try
        {
            _logService.Info($"开始填充dl_前缀文件夹: dl={dlFolderPath}, songs={songsFolderPath}", "DlImportService");
            
            // 验证文件夹存在性
            if (!Directory.Exists(dlFolderPath))
            {
                result.AddError($"dl文件夹不存在: {dlFolderPath}");
                return result;
            }

            if (!Directory.Exists(songsFolderPath))
            {
                result.AddError($"songs文件夹不存在: {songsFolderPath}");
                return result;
            }

            // 获取所有dl_前缀文件夹
            var songFolders = Directory.GetDirectories(songsFolderPath);
            var dlFolders = songFolders
                .Where(f => Path.GetFileName(f).StartsWith("dl_"))
                .ToList();
            
            _logService.Info($"找到 {dlFolders.Count} 个dl_前缀文件夹", "DlImportService");
            
            if (dlFolders.Count == 0)
            {
                result.AddWarning("未找到dl_前缀文件夹");
                return result;
            }

            // 获取dl文件夹中的所有文件
            var dlFiles = Directory.GetFiles(dlFolderPath);
            if (dlFiles.Length == 0)
            {
                result.AddWarning("dl文件夹为空");
                return result;
            }

            // 分析dl文件并分组
            var fileGroups = AnalyzeDlFiles(dlFiles);
            _logService.Info($"分析到 {fileGroups.Count} 个歌曲文件组", "DlImportService");

            // 处理每个dl_前缀文件夹
            foreach (var dlFolder in dlFolders)
            {
                var folderName = Path.GetFileName(dlFolder);
                var songId = folderName.Substring(3); // 移除"dl_"前缀
                
                _logService.Info($"处理dl文件夹: {folderName} (歌曲ID: {songId})", "DlImportService");
                
                // 查找对应的文件组
                var fileGroup = fileGroups.FirstOrDefault(g => g.SongId == songId);
                if (fileGroup == null)
                {
                    result.AddWarning($"dl文件夹 '{folderName}' 对应的歌曲 '{songId}' 在dl文件夹中未找到资源");
                    continue;
                }

                // 导入文件到dl_前缀文件夹
                foreach (var fileInfo in fileGroup.Files)
                {
                    ImportFile(fileInfo, dlFolder, overwriteExisting, result);
                }
                
                result.ProcessedSongs.Add(songId);
                
                // 检查文件夹是否已完整
                var hasBaseOgg = File.Exists(Path.Combine(dlFolder, "base.ogg"));
                var hasChartFiles = Directory.GetFiles(dlFolder, "*.aff", SearchOption.TopDirectoryOnly).Length > 0;
                
                if (hasBaseOgg && hasChartFiles)
                {
                    // 文件夹已完整，可以重命名（移除dl_前缀）
                    var newFolderName = songId;
                    var newFolderPath = Path.Combine(songsFolderPath, newFolderName);
                    
                    // 检查目标文件夹是否已存在
                    if (Directory.Exists(newFolderPath))
                    {
                        _logService.Warning($"目标文件夹已存在，跳过重命名: {newFolderPath}", "DlImportService");
                        result.AddWarning($"目标文件夹已存在，无法重命名 '{folderName}' 为 '{newFolderName}'");
                    }
                    else
                    {
                        try
                        {
                            Directory.Move(dlFolder, newFolderPath);
                            _logService.Info($"重命名文件夹: {folderName} -> {newFolderName}", "DlImportService");
                            result.AddWarning($"已重命名文件夹: {folderName} -> {newFolderName} (资源已完整)");
                        }
                        catch (Exception ex)
                        {
                            _logService.Error($"重命名文件夹失败: {folderName} -> {newFolderName}: {ex.Message}", "DlImportService", ex);
                            result.AddWarning($"重命名文件夹失败: {folderName} -> {newFolderName}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    _logService.Info($"文件夹 '{folderName}' 资源不完整，保持dl_前缀", "DlImportService");
                    result.AddWarning($"文件夹 '{folderName}' 资源不完整，保持dl_前缀");
                }
            }

            // 统计信息
            result.TotalSongsProcessed = dlFolders.Count;
            result.SuccessfulImports = result.ImportedFiles.Count(f => f.Success);
            
            _logService.Info($"填充完成: 处理了 {result.TotalSongsProcessed} 个dl_前缀文件夹，成功导入 {result.SuccessfulImports} 个文件", "DlImportService");
        }
        catch (Exception ex)
        {
            _logService.Error($"填充dl_前缀文件夹失败: {ex.Message}", "DlImportService", ex);
            result.AddError($"填充失败: {ex.Message}");
        }

        return result;
    }
}

#region 数据模型

/// <summary>
/// DL文件类型
/// </summary>
public enum DlFileType
{
    Unknown,
    Audio,          // 音频文件 (base.ogg)
    Chart,          // 谱面文件 (.aff)
    AudioPreview,   // 预览音频 (preview.ogg)
    Video,          // 视频文件 (.mp4)
    VideoAudio,     // 视频音频文件
    Other           // 其他文件
}

/// <summary>
/// DL文件信息
/// </summary>
public class DlFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int? Difficulty { get; set; }
    public DlFileType FileType { get; set; }
}

/// <summary>
/// DL歌曲文件组
/// </summary>
public class DlSongFileGroup
{
    public string SongId { get; set; } = string.Empty;
    public List<DlFileInfo> Files { get; set; } = new();
}

/// <summary>
/// 文件导入结果
/// </summary>
public class FileImportResult
{
    public string SourceFile { get; set; } = string.Empty;
    public string TargetFile { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DL导入结果
/// </summary>
public class DlImportResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<string> ProcessedSongs { get; } = new();
    public List<FileImportResult> ImportedFiles { get; } = new();
    
    public int TotalSongsProcessed { get; set; }
    public int SuccessfulImports { get; set; }
    
    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
    
    public string GetSummary()
    {
        var summary = new List<string>();
        
        if (HasErrors)
        {
            summary.Add($"发现 {Errors.Count} 个错误:");
            summary.AddRange(Errors.Take(5).Select((e, i) => $"{i + 1}. {e}"));
            if (Errors.Count > 5)
            {
                summary.Add($"... 还有 {Errors.Count - 5} 个错误");
            }
        }
        
        if (HasWarnings)
        {
            summary.Add($"发现 {Warnings.Count} 个警告:");
            summary.AddRange(Warnings.Take(5).Select((w, i) => $"{i + 1}. {w}"));
            if (Warnings.Count > 5)
            {
                summary.Add($"... 还有 {Warnings.Count - 5} 个警告");
            }
        }
        
        summary.Add("");
        summary.Add("导入统计:");
        summary.Add($"  处理的歌曲数: {TotalSongsProcessed}");
        summary.Add($"  成功导入的文件数: {SuccessfulImports}/{ImportedFiles.Count}");
        summary.Add($"  处理的歌曲: {string.Join(", ", ProcessedSongs.Take(10))}");
        if (ProcessedSongs.Count > 10)
        {
            summary.Add($"  ... 还有 {ProcessedSongs.Count - 10} 个歌曲");
        }
        
        return string.Join(Environment.NewLine, summary);
    }
}

/// <summary>
/// DL验证结果
/// </summary>
public class DlValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<DlSongInfo> SongInfos { get; } = new();
    
    public int TotalFiles { get; set; }
    public int TotalSongs { get; set; }
    
    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
    
    public string GetSummary()
    {
        var summary = new List<string>();
        
        if (HasErrors)
        {
            summary.Add($"发现 {Errors.Count} 个错误:");
            summary.AddRange(Errors.Take(5).Select((e, i) => $"{i + 1}. {e}"));
            if (Errors.Count > 5)
            {
                summary.Add($"... 还有 {Errors.Count - 5} 个错误");
            }
        }
        
        if (HasWarnings)
        {
            summary.Add($"发现 {Warnings.Count} 个警告:");
            summary.AddRange(Warnings.Take(5).Select((w, i) => $"{i + 1}. {w}"));
            if (Warnings.Count > 5)
            {
                summary.Add($"... 还有 {Warnings.Count - 5} 个警告");
            }
        }
        
        summary.Add("");
        summary.Add("验证统计:");
        summary.Add($"  总文件数: {TotalFiles}");
        summary.Add($"  总歌曲数: {TotalSongs}");
        summary.Add($"  有音频文件的歌曲: {SongInfos.Count(s => s.HasAudio)}");
        summary.Add($"  有谱面文件的歌曲: {SongInfos.Count(s => s.ChartDifficulties.Count > 0)}");
        summary.Add($"  有预览音频的歌曲: {SongInfos.Count(s => s.HasPreview)}");
        summary.Add($"  有视频的歌曲: {SongInfos.Count(s => s.HasVideo)}");
        
        return string.Join(Environment.NewLine, summary);
    }
}

/// <summary>
/// DL歌曲信息
/// </summary>
public class DlSongInfo
{
    public string SongId { get; set; } = string.Empty;
    public bool HasAudio { get; set; }
    public List<int> ChartDifficulties { get; set; } = new();
    public bool HasPreview { get; set; }
    public bool HasVideo { get; set; }
    public int TotalFiles { get; set; }
}

/// <summary>
/// Songs文件夹验证结果
/// </summary>
public class SongsValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public List<SongsFolderInfo> SongFolders { get; } = new();
    
    public int TotalSongFolders { get; set; }
    
    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
    
    public string GetSummary()
    {
        var summary = new List<string>();
        
        if (HasErrors)
        {
            summary.Add($"发现 {Errors.Count} 个错误:");
            summary.AddRange(Errors.Take(5).Select((e, i) => $"{i + 1}. {e}"));
            if (Errors.Count > 5)
            {
                summary.Add($"... 还有 {Errors.Count - 5} 个错误");
            }
        }
        
        if (HasWarnings)
        {
            summary.Add($"发现 {Warnings.Count} 个警告:");
            summary.AddRange(Warnings.Take(5).Select((w, i) => $"{i + 1}. {w}"));
            if (Warnings.Count > 5)
            {
                summary.Add($"... 还有 {Warnings.Count - 5} 个警告");
            }
        }
        
        summary.Add("");
        summary.Add("Songs文件夹验证统计:");
        summary.Add($"  总歌曲文件夹数: {TotalSongFolders}");
        summary.Add($"  有base.ogg的文件夹: {SongFolders.Count(s => s.HasBaseOgg)}");
        summary.Add($"  有谱面文件的文件夹: {SongFolders.Count(s => s.HasChartFiles)}");
        summary.Add($"  有曲绘文件的文件夹: {SongFolders.Count(s => s.HasJacketFiles)}");
        summary.Add($"  有视频的文件夹: {SongFolders.Count(s => s.HasVideo)}");
        summary.Add($"  dl_前缀文件夹: {SongFolders.Count(s => s.IsDlFolder)}");
        
        return string.Join(Environment.NewLine, summary);
    }
}

/// <summary>
/// Songs文件夹信息
/// </summary>
public class SongsFolderInfo
{
    public string FolderName { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string SongId { get; set; } = string.Empty;
    public bool IsDlFolder { get; set; }
    public List<string> Files { get; set; } = new();
    public bool HasBaseOgg { get; set; }
    public bool HasChartFiles { get; set; }
    public bool HasJacketFiles { get; set; }
    public bool HasVideo { get; set; }
}

#endregion
