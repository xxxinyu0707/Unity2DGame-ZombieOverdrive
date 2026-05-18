# Environment Setup

## Target

Use Unity 6.3 LTS for this assignment. Install the latest `6000.3.x` patch available in Unity Hub; at setup time on 2026-05-18, Unity's latest Editor page lists `6000.3.15f1`.

LTS is a good fit for a course project because the version is stable and can stay locked for the whole development cycle.

## Installed Tooling Detected

- Git: available
- Git LFS: available
- .NET SDK: available
- VS Code: available
- Unity Hub: installed at `C:\Program Files\Unity Hub\Unity Hub.exe`
- Unity Editor: install Unity 6.3 LTS, latest `6000.3.x` patch, in Unity Hub

## Install Steps

### 1. Unity Hub

Unity Hub is installed. Open it with:

```powershell
.\scripts\open-unity-hub.ps1
```

If you need to reinstall it later, use Unity's official download page. `winget` may temporarily fail if its stored installer hash lags behind Unity's current CDN package.

If `winget` works on this machine:

```powershell
winget install --id Unity.UnityHub -e --accept-source-agreements --accept-package-agreements
```

### 2. Unity Editor

Open Unity Hub and install Unity 6.3 LTS. Prefer the newest `6000.3.x` patch shown in the Hub.

Recommended modules:

- Windows Build Support (IL2CPP), for exporting a Windows build
- Microsoft Visual Studio Community or VS Code integration
- Documentation, optional

Avoid alpha/beta versions for this assignment unless the teacher specifically asks for them.

### 3. Create Project

Create the Unity project in:

```text
D:\学术垃圾\大三下\Unity大作业\Game
```

Recommended template:

- 2D, for platformer, top-down, puzzle, tower defense, card, or casual games
- 3D only if the concept really needs 3D movement/camera/physics

### 4. Unity Project Settings

In Unity:

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

The script checks Git, Git LFS, .NET, VS Code, Unity Hub, Unity Editor, and whether the expected Unity project folders exist.
