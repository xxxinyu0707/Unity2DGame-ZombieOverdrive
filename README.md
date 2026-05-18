# Unity Single-Player Game Assignment

这是 Unity 单机游戏大作业的仓库根目录。推荐把 Unity 项目创建在本目录下的 `Game/` 子目录中，仓库级文档和脚本保留在根目录。

## Current Setup

- Recommended Unity version: Unity 6.3 LTS, preferably latest `6000.3.x` patch
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

1. Open Unity Hub:

```powershell
.\scripts\open-unity-hub.ps1
```

2. In Unity Hub, install Unity 6.3 LTS with the latest `6000.3.x` patch available, currently `6000.3.15f1`:
   - Microsoft Visual Studio/VS Code integration
   - Windows Build Support if you need to submit a Windows `.exe`
3. Create a new Unity project at `Game/`.
4. Use a 2D template unless your assignment explicitly needs 3D.
5. In Unity, set:
   - Version Control Mode: Visible Meta Files
   - Asset Serialization Mode: Force Text
6. Run:

```powershell
.\scripts\check-env.ps1
```

See [docs/ENVIRONMENT.md](docs/ENVIRONMENT.md) and [docs/VIBE_CODING.md](docs/VIBE_CODING.md).
