# 僵尸狂潮：超载重启

[下载最新 Windows 版本](https://github.com/xxxinyu0707/Unity2DGame-ZombieOverdrive/releases/latest)

这是一个使用 Tuanjie/Unity 制作的 2D 俯视角 Roguelite 射击游戏大作业。项目目标是做出一个可以完整游玩的单机生存局：玩家在无限循环的战场中移动、自动开火、收集经验升级、组合武器与被动技能，并在局外用金币强化下一局开局能力。

## 游戏预览

![主菜单](docs/image.png)
![升级界面](docs/image-1.png)
![暂停菜单](docs/image-2.png)

## 快速开始

- 直接游玩：打开上方的“下载最新 Windows 版本”链接，下载 `ZombieOverdrive-Windows.zip`，解压后运行 `ZombieOverdrive.exe`。
- 本地开发：使用 Tuanjie/Unity Hub 打开 `Game/` 工程，运行 `Assets/Scenes/Main.unity`。

## 当前完成度

- 2D 玩家移动、鼠标瞄准、相机跟随、虚线瞄准线和准心。
- 六种主动武器：哨兵手枪、爆裂霰弹枪、电磁手套、重力黑洞、光刃、裂变激光。
- 十二种被动技能、3 主动 + 3 被动槽位限制、升级三选一。
- 六种武器超进化，超进化后武器特效和效果都会增强。
- Walker、Runner、Spitter、Tanker、中期 Boss、最终 Boss。
- 经验水晶、金币、可破坏补给箱、烤鸡、磁铁、炸弹。
- 主菜单、局外养成、暂停菜单、结算面板、中文 UI。
- 程序化像素风素材、技能图标、程序化音效和 8bit 背景音乐。
- Windows 构建脚本、作业报告和设计说明。

## 操作方式

- `WASD`：移动。
- `鼠标移动`：瞄准方向。
- `ESC`：暂停并查看武器、被动、超进化搭配与音量设置。
- 主菜单中点击“开始战斗”进入局内，局外金币可以升级永久强化。

## 目录结构

```text
Unity大作业/
  Game/                     Unity/Tuanjie 工程根目录
  docs/                     设计文档、报告、任务清单
  scripts/                  环境检查和构建脚本
```

## 本地打开项目

优先使用团结引擎，当前使用的编辑器路径：

```powershell
D:\Tuanjie\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe
```

也可以通过脚本打开 Hub：

```powershell
.\scripts\open-tuanjie-hub.ps1
```

打开 `Game/` 工程后，运行 `Assets/Scenes/Main.unity` 即可测试。

## 构建 Windows 版本

先确保 Tuanjie Hub 中已安装 Windows Build Support，然后在仓库根目录运行：

```powershell
.\scripts\build-windows.ps1
```

默认输出：

```text
Builds/Windows/ZombieOverdrive.exe
```

构建后可以运行短冒烟测试：

```powershell
.\scripts\smoke-build.ps1
```

## 版本控制注意

- 提交 `Assets/`、`Packages/`、`ProjectSettings/` 中需要的工程文件。
- 保留 Unity `.meta` 文件。
- 不提交 `Library/`、`Temp/`、`Obj/`、`Logs/`、`Builds/`、导出的 `.exe`。

## 文档

- 设计说明：`docs/SUBMISSION_DESIGN_REPORT.pdf`
- 设计方案：`docs/game_design_draft.md`
- 实现评估：`docs/IMPLEMENTATION_REVIEW.md`
- 作业报告：`docs/ASSIGNMENT_REPORT.md`
- 演示录制清单：`docs/DEMO_CHECKLIST.md`
- 任务清单：`docs/TASKS.md`
