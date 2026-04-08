# Rain World Key Overlay Mod

**Author:** LanRhyme  
**Version:** 1.0.0  
**Game:** Rain World (Latest Version)

## 功能说明 / Features

这个mod为雨世界游戏添加了一个像素风格的按键显示覆盖层，支持键盘和手柄输入显示。

- **基础按键显示** - 显示移动(WASD)、跳跃、抓取、投掷等核心操作
- **多按键支持** - 同时支持小键盘、箭头键和WASD移动
- **手柄支持** - 自动检测并显示手柄按钮(A/B/X/Y等)
- **详细面板** - 按键图标 + 按下状态 + 连击统计
- **位置调整** - 可拖拽面板位置
- **缩放/透明度** - 自定义显示大小(1.0-3.0)和透明度(0.1-1.0)
- **颜色自定义** - 可配置按键颜色、边框颜色、透明度
- **按键绑定配置** - 可自定义按键绑定(F1菜单)
- **配置保存** - 使用BepInEx配置系统自动保存/加载配置

## 安装方法 / Installation

### 前置要求
- Rain World 最新版本
- BepInEx 已安装 (通常随游戏一起安装)

### 安装步骤
1. 确保游戏目录中已有 `BepInEx` 文件夹
2. 将 `keyoverlay` 文件夹复制到:
   ```
   [Rain World安装目录]/RainWorld_Data/StreamingAssets/mods/
   ```
3. 启动游戏，在"拓展"菜单中启用 "Key Overlay" mod
4. 重启游戏

## 使用说明 / Usage

### 快捷键
| 按键 | 功能 |
|------|------|
| **F1** | 打开/关闭设置菜单 |
| **F2** | 重置面板位置到默认 |
| **Esc** | 关闭设置菜单 |

### 设置菜单选项
- **Scale (1-3)** - 调整显示大小
- **Opacity (0.1-1.0)** - 调整透明度
- **Show Keyboard** - 显示/隐藏键盘输入
- **Show Gamepad** - 显示/隐藏手柄输入
- **Show Combo Stats** - 显示/隐藏连击统计
- **Show Key Names** - 显示/隐藏按键名称
- **Movement Keys** - 显示/隐藏移动键
- **Action Keys** - 显示/隐藏动作键
- **Reset Position** - 重置面板位置
- **Reset Statistics** - 重置统计计数

### 拖拽面板
- 鼠标左键点击面板区域并拖动即可移动位置
- 位置会自动保存

## 文件结构 / File Structure

```
keyoverlay/
├── modinfo.json        # Mod元数据
└── plugins/
    └── KeyOverlay.dll  # Mod主程序
```

## 配置文件 / Configuration

配置文件位于BepInEx配置目录:
```
BepInEx/config/keyoverlay.cfg
```

### 默认按键绑定 (WASD 模式)

| 动作 | 默认按键 | 备用按键 |
|------|----------|----------|
| 上 | W | ↑箭头 |
| 下 | S | ↓箭头 |
| 左 | A | ←箭头 |
| 右 | D | →箭头 |
| 跳跃 | Space | - |
| 投掷 | K | C |
| 抓取 | L | LeftShift |

### 默认显示配置

| 设置项 | 默认值 | 说明 |
|--------|--------|------|
| 位置 X | 136 | 面板水平位置 |
| 位置 Y | 666 | 面板垂直位置 |
| 缩放 | 1.0 | 显示大小 |
| 透明度 | 0.8 | 整体透明度 |
| 按键颜色 | 白色 | 未按下状态 |
| 按下颜色 | 白色 | 按下状态 |
| 边框颜色 | 白色 | 边框颜色 |
| 边框透明度 | 0.45 | 边框透明度 |
| 填充透明度 | 0.1 | 内部填充透明度 |
| 按下效果 | 0.75 | 按下状态效果 |
| 字体大小 | 11px | 文字大小 |
| 边框宽度 | 1.5px | 边框粗细 |

配置会在以下情况自动保存:
- 拖拽面板后
- 修改设置后
- 退出游戏时

## 开发说明 / Development

### 项目结构
```
RainWorld-KeyOverlay/
├── KeyOverlayPlugin.cs      # BepInEx插件入口
├── InputMonitor.cs          # 输入监听系统
├── KeyOverlayUI.cs          # 像素风格UI渲染
├── PauseMenuIntegration.cs  # 设置菜单集成
├── KeyOverlay.csproj        # 项目配置
├── modinfo.json             # Mod信息
└── build.bat                # Windows编译脚本
```

### 编译要求
- .NET Framework 4.8
- Visual Studio 2019+ 或 dotnet SDK

### 编译命令
```bash
dotnet build KeyOverlay.csproj -c Release
```

编译后的DLL会自动复制到游戏mod目录。

## 已知问题 / Known Issues

- 首次安装时配置使用默认值
- 手柄检测依赖Unity Input系统，某些特殊手柄可能不被识别

## 更新日志 / Changelog

### v1.0.0
- 初始发布
- 支持键盘和手柄输入显示
- 像素艺术风格UI
- 可拖拽面板
- 设置菜单(F1)
- 连击统计
- BepInEx配置集成

## 致谢 / Credits

- Rain World by Videocult
- BepInEx Framework
- 0Harmony for method patching

## 许可证 / License

此项目为个人学习项目，代码可自由使用和修改。

---

**雨世界** 是一款由 Videocult 开发的独特生存平台游戏。本mod旨在增强游戏体验，不修改任何游戏核心机制。