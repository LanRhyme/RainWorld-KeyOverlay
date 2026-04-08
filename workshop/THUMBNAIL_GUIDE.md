# Thumbnail 预览图创建指南

## 如何创建 thumbnail.png

Steam Workshop mod 需要一个预览图来在创意工坊页面展示。

### 推荐规格

- **尺寸**: 512x512 像素 (推荐) 或 256x256 像素
- **格式**: PNG
- **内容**: 展示 mod 功能的截图或标志性设计

### 创建方法

### 方法 1: 直接截图 (推荐)

1. 启动 Rain World 并加载 Key Overlay mod
2. 在游戏中按 F1 调整显示效果
3. 使用截图工具 (如 ShareX, Steam截图 F12) 捕捉按键显示面板
4. 编辑截图，裁剪为正方形尺寸

### 方法 2: 设计图标

创建一个简单的像素风格图标：

```
建议内容：
- 像素风格的按键图标 (WASD + Jump/Throw/Grab)
- 简洁的边框设计
- mod 名称 "Key Overlay"
- 作者信息可选
```

### 设计工具

- **GIMP** (免费) - https://www.gimp.org/
- **Photoshop** (付费)
- **Aseprite** (像素艺术专用) - https://www.aseprite.org/
- **在线工具**: https://www.pixilart.com/

### 示例布局

```
┌─────────────────────┐
│                     │
│    KEY OVERLAY      │
│                     │
│   ┌───┬───┬───┐    │
│   │ W │ ● │ ■ │    │
│   ├───┼───┼───┤    │
│   │ A │ S │ D │    │
│   └───┴───┴───┘    │
│       ◆            │
│                     │
│   by LanRhyme      │
│                     │
└─────────────────────┘
```

### 完成后

将创建的 PNG 文件命名为 `thumbnail.png` 并放置在：

```
keyoverlay/
├── modinfo.json
├── plugins/
│   └── KeyOverlay.dll
└── thumbnail.png    <-- 放在这里
```

---

**提示**: 预览图是用户第一眼看到的内容，精心设计可以增加下载量！