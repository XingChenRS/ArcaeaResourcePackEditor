# Arcaea Resource Pack Editor

Arcaea Resource Pack Editor 是一个专注于 Arcaea 游戏资源包管理的 GUI 工具。此项目为Xrcxex附属。

## 项目概述

重构 Arcaea Patch Tool，创建专注于 songlist 管理的 GUI 工具，参考 SlstGen 的 UI 设计。支持完整的 Arcaea 资源包编辑、验证和打包流程。

## 主要功能

### 1. 数据管理
- **歌曲列表管理**：完整的歌曲增删改查功能，支持所有 Arcaea 官方字段
- **曲包列表管理**：曲包信息编辑和管理
- **解锁条件管理**：解锁条件编辑和验证
- **实时 JSON 预览**：右侧窗口显示选中项目的原始 JSON 格式

### 2. 数据验证
- **详细验证报告**：检查数据完整性和一致性
- **错误和警告分类**：清晰的错误提示和修复建议
- **引用关系检查**：验证歌曲、曲包、解锁条件之间的引用关系
- **Active 文件夹验证**：验证 Arcaea 资源包文件夹结构
- **Song validation**: check song IDs, required fields, pack references, and difficulty data integrity

### 3. Bundle 封包
- **更新现有 Active 文件夹**：只更新清单文件，保留原有资源
- **生成 meta.cb 文件**：符合 Arcaea 标准的校验文件
- **Bundle 匹配性验证**：验证 meta.cb 和 Active 文件夹内容是否匹配
- **完整打包流程**：一键完成从数据编辑到最终打包的全过程
- **Bundle packaging**: build/update Active and meta.cb with compatibility checks for release

### 4. 工具和批量操作
- **一键移除限制**：批量移除歌曲的各种限制条件
- **清空解锁条件**：删除所有解锁条件条目
- **检查重复歌曲**：检查歌曲列表中是否有重复的歌曲 ID
- **检查缺失曲包**：检查歌曲引用的曲包是否在曲包列表中定义
- **清理临时文件**：清理应用程序生成的临时文件

### 5. 用户体验
- **类似 SlstGen 的界面**：直观的 Modern UI 设计
- **详细的表单编辑**：支持所有字段的实时编辑
- **完整的日志系统**：多级别日志记录，便于调试和问题追踪
- **全局异常处理**：防止应用程序崩溃，提供友好的错误提示

## 技术架构

### 技术栈
- **目标框架**：.NET 8.0
- **UI 框架**：WPF
- **架构模式**：MVVM (CommunityToolkit.Mvvm)
- **序列化**：System.Text.Json
- **依赖注入**：Microsoft.Extensions.DependencyInjection
- **UI 组件库**：WPF-UI

### 数据模型特点
1. **完整性**：支持所有 rule.txt 描述的字段
2. **兼容性**：与 SlstGen 保持相似的命名和结构
3. **功能性**：包含一键移除限制、验证、克隆等方法
4. **扩展性**：易于添加新功能和新字段

### 应用程序架构特点
1. **MVVM 架构**：使用 CommunityToolkit.Mvvm 实现清晰的关注点分离
2. **依赖注入**：使用 Microsoft.Extensions.DependencyInjection 管理服务生命周期
3. **模块化设计**：每个功能模块有独立的 ViewModel 和 View
4. **数据绑定**：全面使用数据绑定，减少代码耦合
5. **验证系统**：完整的验证逻辑，支持严格模式和普通模式

## 使用方法

### 完整工作流程

1. **数据准备**
   - 导入现有的 songlist.json、packlist.json、unlocks.json
   - 或从零开始创建新的资源包数据

2. **数据编辑**
   - 使用详细的表单编辑歌曲、曲包、解锁条件
   - 实时查看 JSON 预览
   - 使用批量操作工具提高效率

3. **数据验证**
   - 使用验证报告检查数据完整性
   - 使用工具页面检查重复歌曲、缺失曲包等问题
   - 修复所有验证错误和警告
   - Song validation checks (IDs, required fields, pack references, and difficulty data)

4. **Bundle 打包**
   - 选择现有的 Active 文件夹
   - 更新清单文件到 Active 文件夹
   - 验证 Active 文件夹结构
   - 生成 meta.cb 文件（输入版本信息）
   - 验证 Bundle 匹配性确保可以通过游戏校验

5. **发布准备**
   - 将 meta.cb 文件放置到 cb/文件夹
   - 确保整个 cb 文件夹结构完整
   - 打包成 zip 或使用其他分发方式

### JSON 格式支持
- **songlist.json**：支持容器格式 (`{"songs": [...]}`) 和直接数组格式 (`[...]`)
- **packlist.json**：支持容器格式 (`{"packs": [...]}`) 和直接数组格式 (`[...]`)
- **unlocks.json**：支持容器格式 (`{"unlocks": [...]}`) 和直接数组格式 (`[...]`)
- **自定义 JSON 转换器**：支持 search_title.ja 等字段的数组格式

## 编译和运行

### 环境要求
- .NET 8.0 SDK
- Visual Studio 2022 或更高版本（推荐）
- 或使用命令行工具

### 编译命令
```bash
cd ArcaeaSonglistEditor
dotnet build
```

### 运行
编译成功后，可执行文件位于：
```
ArcaeaSonglistEditor\ArcaeaSonglistEditor\bin\Debug\net8.0-windows\ArcaeaSonglistEditor.exe
```

## 项目结构

```
ArcaeaSonglistEditor/
├── ArcaeaSonglistEditor.Core/          # 核心类库
│   ├── Models/                         # 数据模型
│   │   ├── Enums.cs                    # 枚举类型
│   │   ├── LocalizationInfo.cs         # 本地化信息
│   │   ├── DifficultyInfo.cs           # 难度信息
│   │   ├── SongInfo.cs                 # 歌曲信息
│   │   ├── PackInfo.cs                 # 曲包信息
│   │   └── UnlockCondition.cs          # 解锁条件
│   ├── Services/                       # 服务层
│   │   ├── SonglistService.cs          # 歌曲列表服务
│   │   ├── ValidationService.cs        # 验证服务
│   │   ├── BundleService.cs            # Bundle 封包服务
│   │   ├── LogService.cs               # 日志服务
│   │   └── EnhancedValidationService.cs # 增强验证服务
│   └── Converters/                     # JSON 转换器
│       ├── SearchLocalizationConverter.cs
│       └── UnlockConditionConverter.cs
├── ArcaeaSonglistEditor/               # WPF 应用程序
│   ├── ViewModels/                     # 视图模型
│   │   ├── MainWindowViewModel.cs      # 主窗口视图模型
│   │   ├── SonglistViewModel.cs        # 歌曲列表视图模型
│   │   ├── PacklistViewModel.cs        # 曲包列表视图模型
│   │   ├── UnlocksViewModel.cs         # 解锁条件视图模型
│   │   ├── ValidationViewModel.cs      # 验证视图模型
│   │   ├── ToolsViewModel.cs           # 工具视图模型
│   │   ├── SettingsViewModel.cs        # 设置视图模型
│   │   └── LogViewModel.cs             # 日志视图模型
│   ├── Views/                          # 视图
│   │   ├── MainWindow.xaml             # 主窗口
│   │   ├── SonglistPage.xaml           # 歌曲列表页面
│   │   ├── PacklistPage.xaml           # 曲包列表页面
│   │   ├── UnlocksPage.xaml            # 解锁条件页面
│   │   ├── ValidationPage.xaml         # 验证报告页面
│   │   ├── ToolsPage.xaml              # 工具页面
│   │   ├── SettingsPage.xaml           # 设置页面
│   │   ├── AboutPage.xaml              # 关于页面
│   │   └── LogWindow.xaml              # 日志窗口
│   ├── Converters/                     # 值转换器
│   │   ├── UnixTimestampConverter.cs   # Unix 时间戳转换
│   │   ├── BoolToVisibilityConverter.cs # 布尔值可见性转换
│   │   ├── NullToVisibilityConverter.cs # 空值可见性转换
│   │   ├── EnumToStringConverter.cs    # 枚举字符串转换
│   │   ├── NumberFormatConverter.cs    # 数字格式化
│   │   └── DateFormatConverter.cs      # 日期格式化
│   └── App.xaml                        # 应用程序入口
├── ArcaeaSonglistEditor.sln            # 解决方案文件
└── README.md                           # 项目说明文档
```

## 开源协议

本项目基于 MIT 协议开源。

Copyright © 2026 雾月星辰 XingC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

## 致谢

感谢以下项目和工具：

- **SlstGen** (https://github.com/bsdayo/SlstGen) - 提供了 UI 设计参考
- **Arcaea Bundler** (https://github.com/Lost-MSth/Arcaea-Bundler) - Bundle 封包工具
- **Arcaea Patch Tool** - 原始项目基础
- **WPF-UI** - Modern UI 组件库
- **CommunityToolkit.Mvvm** - MVVM 工具包

## 项目状态

✅ **生产就绪状态**

- **核心功能完整**：支持所有 Arcaea 数据文件的导入/导出
- **稳定性保障**：全面的异常处理和日志记录
- **可扩展架构**：清晰的 MVVM 架构，模块化设计
- **用户体验优化**：类似 SlstGen 的直观界面，详细的工具提示

项目现在可以用于实际的 Arcaea 资源包编辑和打包工作，支持从数据编辑到最终打包的完整流程。

## 联系方式

- **项目主页**：https://github.com/XingChenRS/ArcaeaSonglistEditor
- **问题反馈**：请在 GitHub Issues 中提交问题

---

**Arcaea Resource Pack Editor** - 让 Arcaea 资源包编辑更简单、更高效！
