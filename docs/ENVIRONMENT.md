# Environment Setup

## Target

Use Tuanjie Engine for this assignment. It is the China-localized Unity engine route and avoids the Unity Hub CDN failures seen on this machine.

Fallback: use Unity 6.3 LTS only if Unity Hub downloads work reliably.

## Installed Tooling Detected

- Git: available
- Git LFS: available
- .NET SDK: available
- VS Code: available
- Tuanjie Hub: installed at `D:\Tuanjie\Hub\App\Tuanjie Hub.exe`
- Unity Hub: uninstalled to save C drive space
- Tuanjie/Unity Editor: install through Tuanjie Hub
- Tuanjie temp/cache/user-data route: `D:\Tuanjie\AppData` and `D:\Temp`

## Install Steps

### 1. Tuanjie Hub

Tuanjie Hub is installed. Open it with:

```powershell
.\scripts\open-tuanjie-hub.ps1
```

Use this script instead of double-clicking the Hub executable. It starts Tuanjie Hub with D-drive `APPDATA`, `LOCALAPPDATA`, `TEMP`, and `TMP`, so bulky Hub data avoids C drive.

If you need to reinstall it later:

```powershell
winget install --id Unity.TuanjieHub -e --location "D:\Tuanjie\Hub\App" --accept-source-agreements --accept-package-agreements
```

Official release page:

```text
https://unity.cn/tuanjie/releases
```

### 2. Tuanjie Engine

Open Tuanjie Hub and install the recommended stable Tuanjie Engine version.

Recommended installation path:

```powershell
D:\Tuanjie\Hub\Editor
```

Recommended download/cache path:

```powershell
D:\Tuanjie\Hub\Downloads
```

Recommended modules:

- Windows Build Support, for exporting a Windows build
- VS Code or Visual Studio integration
- Documentation, optional

Avoid unnecessary mobile platform modules unless the assignment requires them.

### 3. Create Project

Create the project in:

```text
D:\学术垃圾\大三下\Unity大作业\Game
```

Recommended template:

- 2D, for platformer, top-down, puzzle, tower defense, card, or casual games
- 3D only if the concept really needs 3D movement/camera/physics

### 4. Project Settings

In the editor:

- `Edit > Project Settings > Editor > Version Control Mode`: `Visible Meta Files`
- `Edit > Project Settings > Editor > Asset Serialization Mode`: `Force Text`
- `Edit > Preferences > External Tools`: select VS Code or your preferred C# IDE

### 5. Git

This repository uses:

- `.gitignore` to exclude Unity-generated cache folders
- `.gitattributes` to keep large art/audio/model assets in Git LFS

Run once after cloning or creating the repo:

```powershell
git lfs install
```

## Verification

Run:

```powershell
.\scripts\check-env.ps1
```

The script checks Git, Git LFS, .NET, VS Code, Tuanjie Hub, editor installation, and whether the expected project folders exist.
