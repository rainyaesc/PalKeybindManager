# PalKeybindManager — 帕鲁中文键位编辑器

为 Palworld 提供独立中文键位面板，修改游戏设置中未显示的隐藏操作，并在下次启动时重新应用。适用于 Windows Steam 版和已有 UE4SS 环境。

## 功能

- 中文操作名与英文内部名双列显示，支持搜索。
- 编辑键盘、鼠标按键和滚轮映射，以及 WASD 等键盘方向轴。
- 检查同键冲突，支持禁用、单条恢复和全部恢复。
- 配置保存在 Mod 自己的 `config.tsv`，无需每次启动重新设置。
- 游戏退出后编辑，标题界面启动阶段应用；不修改游戏本体、存档或系统键位。

## 安装与使用

1. 先准备与游戏版本兼容的 UE4SS。本仓库不附带或安装 UE4SS。
2. 从 Releases 下载安装 ZIP，将其中 `PalKeybindManager` 文件夹放到 `Palworld/Mods/NativeMods/UE4SS/Mods/`。
3. 确认目录中有 `enabled.txt`、`Scripts/main.lua` 和 `Panel/PalKeybindPanel.exe`。若 UE4SS 安装在其他位置，请放到它实际的 `Mods` 目录。
4. 从 Steam 启动游戏，到标题界面等待约 5 秒，再正常退出，以生成 `mappings.tsv`。
5. 双击 `编辑键位.cmd`，选择操作、点击更改键位并输入新按键。配置自动保存。
6. 关闭面板，再从 Steam 启动游戏。

**编辑时必须退出游戏。** 本版采用离线面板，不支持游戏运行期间实时改键。

从源码安装时，本仓库根目录就是 Mod 目录；复制整个目录并命名为 `PalKeybindManager`。

## 已验证环境与限制

- 历史实测：Steam Palworld v1.0.1，Build `24181527`。
- 历史实测加载器：UE4SS Experimental (Palworld) `experimental-palworld-6`，核心 v3.0.1 Beta #0，SHA `c838a8ac`。
- 当前版本：`0.4.0-personal-cn-panel`。后续游戏与 UE4SS 版本未重新验证。
- 手柄、VR 和 MouseX/MouseY 视角轴只读。
- 当前版本中 Jump 无法可靠响应滚轮；跳跃请保留空格或使用其他受支持按键。
- 本 Mod 可独立使用；不需要 MouseMiddleSprint，也不会自动添加中键疾跑。

更新游戏后请先确认兼容性。项目按现状提供，不承诺持续维护。

## 源码与构建

Lua 后端：`Scripts/main.lua`；Windows Forms 面板源码：`Panel/PalKeybindPanel.cs`。仓库附带历史已使用的面板 EXE。

在安装了 .NET Framework 4.x 编译器的 Windows PowerShell 中，从仓库根目录运行：

```powershell
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /target:winexe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /out:Panel\PalKeybindPanel.exe Panel\PalKeybindPanel.cs
```

## 配置与卸载

运行时生成 `config.tsv`、`mappings.tsv` 和日志，不包含在发布包中。升级前自行备份 `config.tsv`。

退出游戏后移除 `PalKeybindManager` 目录即可卸载；下次启动恢复游戏和其他 Mod 提供的键位。

发布账户：[rainyaesc](https://github.com/rainyaesc)。这是非官方社区 Mod，与 Pocketpair 无隶属关系。
