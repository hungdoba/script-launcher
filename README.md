# Script Launcher

Script Launcher is a lightweight WPF desktop app for storing and running your common scripts and commands from a button grid.

## Features

- Add, edit, delete, and reorder command tiles.
- Run different command types:
  - Cmd
  - Batch
  - PowerShell
  - Executable
- Optional run as administrator.
- Optional open terminal window.
- Icon picker based on Material Design icons.
- Quick command templates loaded from settings.
- Test run from the edit dialog before saving.
- Tray icon support (minimize to tray).
- Run status feedback on command tile (temporary border color).

## Project Structure

- `Views/` WPF windows and UI markup.
- `ViewModels/` UI state and command logic.
- `Services/` execution and JSON persistence services.
- `Models/` command data structures.
- `Data/commands.json` saved command list.
- `Data/common-commands.json` reusable template list.

## Requirements

- Windows
- .NET Framework 4.7.2
- Visual Studio 2019+ (or compatible MSBuild + NuGet tooling)

## Run Locally

1. Open `ScriptLauncher.sln` in Visual Studio.
2. Restore NuGet packages.
3. Build and run in `Debug` or `Release`.

## Command Data Files

### Main command file

- Path: `Data/commands.json`
- Copied to output on build.

Example item:

```json
{
  "name": "Open Downloads",
  "description": "Open Downloads folder",
  "type": "Cmd",
  "command": "explorer %USERPROFILE%\\Downloads",
  "arguments": "",
  "workingDirectory": "",
  "runAsAdministrator": false,
  "openWindow": false,
  "icon": "Download"
}
```

### Common template file

- Path: `Data/common-commands.json`
- Configured by app setting `CommonCommandsFile`.

You can edit this file to define your own frequently used command templates.

## GitHub Actions Build and Release

This project includes a workflow at `.github/workflows/build-release.yml`.

It does the following:

1. Restores packages.
2. Builds `ScriptLauncher.sln` in Release mode.
3. Packages output from `bin/Release` into a zip file.
4. Uploads the zip as a workflow artifact.
5. On version tags (`v*`), publishes the zip to GitHub Releases.

### Release steps

```bash
git tag v1.0.0
git push origin v1.0.0
```

Then download the zip from:

- Actions run artifacts, or
- GitHub Releases page (for tag builds)

## Notes

- If command execution fails, check command path, arguments, and working directory.
- For PowerShell scripts, make sure script path exists and execution policy/environment allow running it.
