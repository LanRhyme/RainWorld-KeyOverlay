# Steam Workshop 上传指南

## Rain World 创意工坊上传方法

Rain World 通过游戏内的 **Remix 菜单** 直接上传 mod 到 Steam Workshop。

### 准备工作

1. 确保你的 Steam 账号已登录且可以访问创意工坊
2. 确保 Rain World 已启动并正常运行

### 上传步骤

1. **将 mod 复制到游戏目录**
   ```
   [Rain World 安装目录]/RainWorld_Data/StreamingAssets/mods/keyoverlay/
   ```
   
   完整文件结构：
   ```
   keyoverlay/
   ├── modinfo.json        # Mod元数据
   ├── plugins/
   │   └── KeyOverlay.dll  # Mod主程序
   └── thumbnail.png       # 预览图（推荐）
   ```

2. **启动游戏**

3. **进入 Remix 菜单**
   - 在主菜单或暂停菜单中，选择 **"Remix"** 或 **"拓展"** 选项
   - 在 mod 列表中找到 **"Key Overlay"**

4. **上传到 Workshop**
   - 点击 mod 名称旁的 **上传按钮**（通常是一个向上箭头图标）
   - 填写创意工坊页面的描述信息
   - 选择预览图（thumbnail.png）
   - 点击确认上传

### 更新已上传的 Mod

如果你已经上传过此 mod，需要更新时：

1. 更新 `KeyOverlay.dll` 文件
2. 更新 `modinfo.json` 中的版本号
3. 进入 Remix 蘋单
4. 点击 mod 的上传按钮，选择 **更新现有项目**

### 预览图要求

- 格式：PNG
- 推荐尺寸：512x512 或 256x256
- 内容：展示 mod 功能的截图或标志性图像

### 创意工坊描述建议

**标题：**
```
Key Overlay - Input Display for Rain World
```

**简短描述：**
```
A pixel-art style input overlay that displays keyboard and gamepad inputs on screen. Perfect for streamers, tutorials, and gameplay videos.
```

**详细描述：**
```
Features:
- Real-time keyboard input display (WASD/Arrow keys, Jump, Throw, Grab)
- Gamepad support (A/B/X/Y buttons shown)
- Combo statistics (JMP/THR/GRB counters)
- CPS (Clicks Per Second) display
- Draggable panel position
- Customizable colors and opacity
- Key binding configuration
- F1 settings menu

Controls:
- F1: Open/Close settings menu
- F2: Reset panel position
- Mouse drag: Move panel

The mod saves your settings automatically and works seamlessly with Rain World's Remix system.
```

### 标签建议

推荐使用以下标签：
- `Utility`
- `Display`
- `Gameplay`
- `Accessibility`

---

## 发布后维护

### 版本更新流程

1. 修改代码并重新编译
2. 更新 `modinfo.json` 的 `version` 字段
3. 复制新 DLL 到游戏目录
4. 进入 Remix 菜单重新上传

### 用户反馈

关注创意工坊页面的评论，及时回应问题。

---

## 注意事项

- 确保 mod 在本地测试正常工作后再上传
- Steam Workshop 上传可能需要几分钟处理时间
- 预览图会在创意工坊页面显示，建议精心设计

---

**祝发布成功！**