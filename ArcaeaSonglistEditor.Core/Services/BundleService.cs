using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcaeaSonglistEditor.Core.Services;

/// <summary>
/// Bundle封包服务
/// 用于生成Arcaea兼容的meta.cb文件
/// </summary>
public class BundleService
{
    private readonly SonglistService _songlistService;
    private readonly LogService _logService;

    public BundleService(SonglistService songlistService)
    {
        _songlistService = songlistService ?? throw new ArgumentNullException(nameof(songlistService));
        _logService = LogService.Instance;
    }

    /// <summary>
    /// 生成meta.cb文件
    /// </summary>
    /// <param name="activeFolderPath">active文件夹路径</param>
    /// <param name="outputPath">输出meta.cb文件路径</param>
    /// <param name="appVersion">应用程序版本号</param>
    /// <param name="bundleVersion">bundle版本号</param>
    /// <param name="previousBundleVersion">前一个bundle版本号</param>
    /// <returns>是否成功</returns>
    public bool GenerateMetaCb(string activeFolderPath, string outputPath, 
        string? appVersion = null, string? bundleVersion = null, string? previousBundleVersion = null)
    {
        try
        {
            _logService.Info($"开始生成meta.cb文件: {outputPath}", "BundleService");
            
            // 验证active文件夹是否存在
            if (!Directory.Exists(activeFolderPath))
            {
                throw new DirectoryNotFoundException($"active文件夹不存在: {activeFolderPath}");
            }

            // 验证三个清单文件是否存在
            var songlistPath = Path.Combine(activeFolderPath, "songs", "songlist");
            var packlistPath = Path.Combine(activeFolderPath, "songs", "packlist");
            var unlocksPath = Path.Combine(activeFolderPath, "songs", "unlocks");

            if (!File.Exists(songlistPath))
            {
                throw new FileNotFoundException($"songlist文件不存在: {songlistPath}");
            }
            if (!File.Exists(packlistPath))
            {
                throw new FileNotFoundException($"packlist文件不存在: {packlistPath}");
            }
            if (!File.Exists(unlocksPath))
            {
                throw new FileNotFoundException($"unlocks文件不存在: {unlocksPath}");
            }

            // 收集所有文件
            var allFiles = CollectAllFiles(activeFolderPath);
            _logService.Info($"找到 {allFiles.Count} 个文件", "BundleService");

            // 生成文件哈希和偏移量
            var addedFiles = new List<BundleFileInfo>();
            var pathToHash = new Dictionary<string, string>();
            long currentOffset = 0;

            foreach (var file in allFiles)
            {
                var fileInfo = ProcessFile(file, activeFolderPath, currentOffset);
                addedFiles.Add(fileInfo);
                pathToHash[fileInfo.Path] = fileInfo.Sha256HashBase64Encoded;
                currentOffset += fileInfo.Length;
            }

            // 生成pathToDetails（三个清单文件的HMAC-SHA256哈希）
            var pathToDetails = GeneratePathToDetails(activeFolderPath);

            // 生成UUID
            var uuid = GenerateUuid();

            // 构建metadata
            var metadata = new BundleMetadata
            {
                VersionNumber = bundleVersion ?? "1.0.0",
                PreviousVersionNumber = previousBundleVersion,
                ApplicationVersionNumber = appVersion ?? "1.0.0",
                Uuid = uuid,
                Removed = new List<string>(), // 首次打包没有移除的文件
                Added = addedFiles,
                PathToHash = pathToHash,
                PathToDetails = pathToDetails
            };

            // 序列化为JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(metadata, jsonOptions);
            
            // 写入文件
            File.WriteAllText(outputPath, json);
            
            _logService.Info($"meta.cb文件生成成功: {outputPath}", "BundleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService.Error($"生成meta.cb文件失败: {ex.Message}", "BundleService", ex);
            throw;
        }
    }

    /// <summary>
    /// 验证active文件夹结构
    /// </summary>
    public ValidationResult ValidateActiveFolder(string activeFolderPath)
    {
        var result = new ValidationResult();
        
        try
        {
            // 检查文件夹是否存在
            if (!Directory.Exists(activeFolderPath))
            {
                result.AddError($"active文件夹不存在: {activeFolderPath}");
                return result;
            }

            // 检查三个清单文件
            var songlistPath = Path.Combine(activeFolderPath, "songs", "songlist");
            var packlistPath = Path.Combine(activeFolderPath, "songs", "packlist");
            var unlocksPath = Path.Combine(activeFolderPath, "songs", "unlocks");

            if (!File.Exists(songlistPath))
            {
                result.AddError($"songlist文件不存在: {songlistPath}");
            }
            else
            {
                // 验证songlist格式
                try
                {
                    var songlistContent = File.ReadAllText(songlistPath);
                    if (string.IsNullOrWhiteSpace(songlistContent))
                    {
                        result.AddError("songlist文件为空");
                    }
                }
                catch (Exception ex)
                {
                    result.AddError($"读取songlist文件失败: {ex.Message}");
                }
            }

            if (!File.Exists(packlistPath))
            {
                result.AddError($"packlist文件不存在: {packlistPath}");
            }

            if (!File.Exists(unlocksPath))
            {
                result.AddError($"unlocks文件不存在: {unlocksPath}");
            }

            // 检查songs文件夹结构
            var songsFolder = Path.Combine(activeFolderPath, "songs");
            if (Directory.Exists(songsFolder))
            {
                var songFolders = Directory.GetDirectories(songsFolder);
                foreach (var folder in songFolders)
                {
                    var folderName = Path.GetFileName(folder);
                    if (folderName.StartsWith("dl_"))
                    {
                        // 检查dl_文件夹是否包含必要的文件
                        var baseJpg = Directory.GetFiles(folder, "*_base.jpg").FirstOrDefault();
                        var previewOgg = Directory.GetFiles(folder, "preview.ogg").FirstOrDefault();
                        
                        if (baseJpg == null)
                        {
                            result.AddWarning($"歌曲文件夹 {folderName} 缺少_base.jpg文件");
                        }
                        if (previewOgg == null)
                        {
                            result.AddWarning($"歌曲文件夹 {folderName} 缺少preview.ogg文件");
                        }
                    }
                }
            }

            // 检查img文件夹
            var imgFolder = Path.Combine(activeFolderPath, "img");
            if (!Directory.Exists(imgFolder))
            {
                result.AddWarning("img文件夹不存在");
            }
        }
        catch (Exception ex)
        {
            result.AddError($"验证active文件夹时发生错误: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 更新现有active文件夹中的清单文件
    /// </summary>
    public bool UpdateActiveFolder(string activeFolderPath)
    {
        try
        {
            _logService.Info($"开始更新active文件夹: {activeFolderPath}", "BundleService");
            
            // 验证active文件夹是否存在
            if (!Directory.Exists(activeFolderPath))
            {
                throw new DirectoryNotFoundException($"active文件夹不存在: {activeFolderPath}");
            }

            // 验证songs文件夹是否存在
            var songsFolder = Path.Combine(activeFolderPath, "songs");
            if (!Directory.Exists(songsFolder))
            {
                Directory.CreateDirectory(songsFolder);
                _logService.Info($"创建了songs文件夹: {songsFolder}", "BundleService");
            }

            // 保存三个清单文件到现有active文件夹
            SaveSonglist(songsFolder);
            SavePacklist(songsFolder);
            SaveUnlocks(songsFolder);

            _logService.Info($"active文件夹更新成功: {activeFolderPath}", "BundleService");
            return true;
        }
        catch (Exception ex)
        {
            _logService.Error($"更新active文件夹失败: {ex.Message}", "BundleService", ex);
            throw;
        }
    }

    #region 私有方法

    private List<string> CollectAllFiles(string rootPath)
    {
        var files = new List<string>();
        
        // 排除metadata.oldjson文件
        var excludePatterns = new[] { "*.oldjson" };
        
        foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
        {
            var fileName = Path.GetFileName(file);
            var shouldExclude = excludePatterns.Any(pattern => 
                fileName.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase));
            
            if (!shouldExclude)
            {
                files.Add(file);
            }
        }
        
        return files;
    }

    private BundleFileInfo ProcessFile(string filePath, string rootPath, long offset)
    {
        var relativePath = GetRelativePath(filePath, rootPath);
        var fileData = File.ReadAllBytes(filePath);
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(fileData);
        var hashBase64 = Convert.ToBase64String(hash);
        
        return new BundleFileInfo
        {
            Path = relativePath.Replace("\\", "/"), // 使用Unix路径分隔符
            ByteOffset = offset,
            Length = fileData.Length,
            Sha256HashBase64Encoded = hashBase64
        };
    }

    private Dictionary<string, string> GeneratePathToDetails(string activeFolderPath)
    {
        var pathToDetails = new Dictionary<string, string>();
        var key = new byte[] { 
            0xd4, 0x1f, 0xdb, 0xe3, 0x37, 0xd0, 0x01, 0x68, 
            0x0c, 0x2a, 0x4d, 0x43, 0xaf, 0xe5, 0x70, 0xc7, 
            0x1f, 0xde, 0x85, 0xd8, 0xf3, 0xd4, 0xc4, 0x6f, 
            0x37, 0x99, 0xc1, 0x8f, 0x1f, 0x50, 0x82, 0x77, 
            0xac, 0xa7, 0xab, 0x63, 0x32, 0x83, 0x71, 0x0c, 
            0x2b, 0xb4, 0x1a, 0x07, 0x8e, 0xfb, 0xe7, 0xc1, 
            0x9c, 0xf0, 0x87, 0xa7, 0xe1, 0x37, 0x75, 0x2a, 
            0xb7, 0x58, 0x1c, 0x8d, 0x9c, 0x0e, 0x3d, 0xe9 
        };

        var detailFiles = new[] { "songs/unlocks", "songs/packlist", "songs/songlist" };
        
        foreach (var detailFile in detailFiles)
        {
            var filePath = Path.Combine(activeFolderPath, detailFile);
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                using var hmac = new HMACSHA256(key);
                var hash = hmac.ComputeHash(fileData);
                var hashBase64 = Convert.ToBase64String(hash);
                pathToDetails[detailFile] = hashBase64;
            }
        }

        return pathToDetails;
    }

    private string GenerateUuid()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[9];
        rng.GetBytes(bytes);
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant().Substring(0, 9);
    }

    private string GetRelativePath(string fullPath, string rootPath)
    {
        var rootUri = new Uri(rootPath + Path.DirectorySeparatorChar);
        var fileUri = new Uri(fullPath);
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
    }

    private void SaveSonglist(string songsFolder)
    {
        var songlistPath = Path.Combine(songsFolder, "songlist");
        var songs = _songlistService.Songs;
        
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        var json = JsonSerializer.Serialize(songs, jsonOptions);
        File.WriteAllText(songlistPath, json);
    }

    private void SavePacklist(string songsFolder)
    {
        var packlistPath = Path.Combine(songsFolder, "packlist");
        var packs = _songlistService.Packs;
        
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        var json = JsonSerializer.Serialize(packs, jsonOptions);
        File.WriteAllText(packlistPath, json);
    }

    private void SaveUnlocks(string songsFolder)
    {
        var unlocksPath = Path.Combine(songsFolder, "unlocks");
        var unlocks = _songlistService.Unlocks;
        
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        var json = JsonSerializer.Serialize(unlocks, jsonOptions);
        File.WriteAllText(unlocksPath, json);
    }

    /// <summary>
    /// 验证meta.cb文件和active文件夹的匹配性
    /// </summary>
    public ValidationResult ValidateBundle(string metaCbPath, string activeFolderPath)
    {
        var result = new ValidationResult();
        
        try
        {
            _logService.Info($"开始验证bundle匹配性: meta.cb={metaCbPath}, active={activeFolderPath}", "BundleService");
            
            // 验证文件存在性
            if (!File.Exists(metaCbPath))
            {
                result.AddError($"meta.cb文件不存在: {metaCbPath}");
                return result;
            }
            
            if (!Directory.Exists(activeFolderPath))
            {
                result.AddError($"active文件夹不存在: {activeFolderPath}");
                return result;
            }
            
            // 读取meta.cb文件
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var metadata = JsonSerializer.Deserialize<BundleMetadata>(File.ReadAllText(metaCbPath), jsonOptions);
            if (metadata == null)
            {
                result.AddError($"无法解析meta.cb文件: {metaCbPath}");
                return result;
            }
            
            // 验证版本信息
            if (string.IsNullOrEmpty(metadata.VersionNumber))
            {
                result.AddWarning("meta.cb文件中缺少versionNumber字段");
            }
            
            if (string.IsNullOrEmpty(metadata.ApplicationVersionNumber))
            {
                result.AddWarning("meta.cb文件中缺少applicationVersionNumber字段");
            }
            
            // 验证文件哈希
            int totalFiles = metadata.PathToHash?.Count ?? 0;
            int verifiedFiles = 0;
            int mismatchedFiles = 0;
            
            if (metadata.PathToHash != null)
            {
                foreach (var kvp in metadata.PathToHash)
                {
                    var relativePath = kvp.Key;
                    var expectedHashBase64 = kvp.Value;
                    
                    var filePath = Path.Combine(activeFolderPath, relativePath);
                    
                    if (!File.Exists(filePath))
                    {
                        result.AddError($"文件不存在: {relativePath} (在meta.cb中定义但active文件夹中不存在)");
                        continue;
                    }
                    
                    // 计算实际哈希
                    var fileData = File.ReadAllBytes(filePath);
                    using var sha256 = SHA256.Create();
                    var actualHash = sha256.ComputeHash(fileData);
                    var actualHashBase64 = Convert.ToBase64String(actualHash);
                    
                    if (actualHashBase64 != expectedHashBase64)
                    {
                        result.AddError($"文件哈希不匹配: {relativePath}");
                        mismatchedFiles++;
                    }
                    else
                    {
                        verifiedFiles++;
                    }
                }
            }
            
            // 验证pathToDetails（三个清单文件的HMAC-SHA256哈希）
            if (metadata.PathToDetails != null)
            {
                var key = new byte[] { 
                    0xd4, 0x1f, 0xdb, 0xe3, 0x37, 0xd0, 0x01, 0x68, 
                    0x0c, 0x2a, 0x4d, 0x43, 0xaf, 0xe5, 0x70, 0xc7, 
                    0x1f, 0xde, 0x85, 0xd8, 0xf3, 0xd4, 0xc4, 0x6f, 
                    0x37, 0x99, 0xc1, 0x8f, 0x1f, 0x50, 0x82, 0x77, 
                    0xac, 0xa7, 0xab, 0x63, 0x32, 0x83, 0x71, 0x0c, 
                    0x2b, 0xb4, 0x1a, 0x07, 0x8e, 0xfb, 0xe7, 0xc1, 
                    0x9c, 0xf0, 0x87, 0xa7, 0xe1, 0x37, 0x75, 0x2a, 
                    0xb7, 0x58, 0x1c, 0x8d, 0x9c, 0x0e, 0x3d, 0xe9 
                };
                
                var detailFiles = new[] { "songs/unlocks", "songs/packlist", "songs/songlist" };
                
                foreach (var detailFile in detailFiles)
                {
                    if (metadata.PathToDetails.TryGetValue(detailFile, out var expectedDetailHashBase64))
                    {
                        var filePath = Path.Combine(activeFolderPath, detailFile);
                        if (File.Exists(filePath))
                        {
                            var fileData = File.ReadAllBytes(filePath);
                            using var hmac = new HMACSHA256(key);
                            var actualDetailHash = hmac.ComputeHash(fileData);
                            var actualDetailHashBase64 = Convert.ToBase64String(actualDetailHash);
                            
                            if (actualDetailHashBase64 != expectedDetailHashBase64)
                            {
                                result.AddError($"清单文件HMAC哈希不匹配: {detailFile}");
                            }
                        }
                        else
                        {
                            result.AddWarning($"清单文件不存在: {detailFile}");
                        }
                    }
                    else
                    {
                        result.AddWarning($"meta.cb中缺少清单文件哈希: {detailFile}");
                    }
                }
            }
            
            // 验证added文件列表
            if (metadata.Added != null)
            {
                foreach (var addedFile in metadata.Added)
                {
                    var filePath = Path.Combine(activeFolderPath, addedFile.Path);
                    if (!File.Exists(filePath))
                    {
                        result.AddWarning($"added文件不存在: {addedFile.Path}");
                    }
                }
            }
            
            // 总结
            if (totalFiles > 0)
            {
                result.AddInfo($"文件验证: {verifiedFiles}/{totalFiles} 个文件哈希匹配，{mismatchedFiles} 个不匹配");
            }
            
            if (result.IsValid)
            {
                _logService.Info($"bundle验证通过: {verifiedFiles}/{totalFiles} 文件匹配", "BundleService");
                result.AddInfo("bundle验证通过: meta.cb文件和active文件夹内容匹配");
            }
            else
            {
                _logService.Warning($"bundle验证失败: {mismatchedFiles}/{totalFiles} 文件不匹配", "BundleService");
            }
        }
        catch (Exception ex)
        {
            _logService.Error($"验证bundle匹配性失败: {ex.Message}", "BundleService", ex);
            result.AddError($"验证bundle匹配性失败: {ex.Message}");
        }
        
        return result;
    }

    #endregion

    #region 数据模型

    public class BundleMetadata
    {
        [JsonPropertyName("versionNumber")]
        public string VersionNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("previousVersionNumber")]
        public string? PreviousVersionNumber { get; set; }
        
        [JsonPropertyName("applicationVersionNumber")]
        public string ApplicationVersionNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;
        
        [JsonPropertyName("removed")]
        public List<string> Removed { get; set; } = new();
        
        [JsonPropertyName("added")]
        public List<BundleFileInfo> Added { get; set; } = new();
        
        [JsonPropertyName("pathToHash")]
        public Dictionary<string, string> PathToHash { get; set; } = new();
        
        [JsonPropertyName("pathToDetails")]
        public Dictionary<string, string> PathToDetails { get; set; } = new();
    }

    public class BundleFileInfo
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;
        
        [JsonPropertyName("byteOffset")]
        public long ByteOffset { get; set; }
        
        [JsonPropertyName("length")]
        public long Length { get; set; }
        
        [JsonPropertyName("sha256HashBase64Encoded")]
        public string Sha256HashBase64Encoded { get; set; } = string.Empty;
    }

    #endregion
}