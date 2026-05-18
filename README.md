# Tuanjie/Unity Single-Player Game Assignment

这是团结引擎/Unity 单机游戏大作业的仓库根目录。国内网络环境下优先使用团结引擎。推荐把项目创建在本目录下的 `Game/` 子目录中，仓库级文档和脚本保留在根目录。

## Current Setup

- Recommended engine: Tuanjie Engine, installed through Tuanjie Hub on `D:` drive
- Fallback engine: Unity 6.3 LTS if Unity Hub downloads work
- IDE: Visual Studio Code + C# Dev Kit / Unity extension
- Version control: Git + Git LFS
- Collaboration mode: vibe coding with small playable slices

## Suggested Layout

```text
Unity大作业/
  Game/                 # Unity project root, created by Unity Hub
    Assets/
    Packages/
    ProjectSettings/
  docs/                 # Design notes, workflows, checklists
  scripts/              # Local helper scripts
```

## First Run

1. Open Tuanjie Hub:

```powershell
.\scripts\open-tuanjie-hub.ps1
```

2. In Tuanjie Hub, set install/download paths to D drive:
   - Editor path: `D:\Tuanjie\Hub\Editor`
   - Download/cache path: `D:\Tuanjie\Hub\Downloads`
3. Install the recommended Tuanjie Engine version:
   - Microsoft Visual Studio/VS Code integration
   - Windows Build Support if you need to submit a Windows `.exe`
4. Create a new 2D project at `Game/`.
5. Use a 2D template unless your assignment explicitly needs 3D.
6. In the editor, set:
   - Version Control Mode: Visible Meta Files
   - Asset Serialization Mode: Force Text
7. Run:

```powershell
.\scripts\check-env.ps1
```

See [docs/ENVIRONMENT.md](docs/ENVIRONMENT.md) and [docs/VIBE_CODING.md](docs/VIBE_CODING.md).
