# Museum-PDRI

Museum-PDRI is a Unity-based VR project built with the Universal Render Pipeline (URP). It appears to be a museum experience/template with XR interaction support, a welcome gate flow, and a VR menu controller.

## Overview

This project is configured as a Unity 2022.3 project and uses:

- **Unity 2022.3.62f3**
- **Universal Render Pipeline (URP)**
- **XR Interaction Toolkit**
- **Unity Input System**

The repository currently contains Unity project assets, settings, and a small set of custom scripts for VR UI and locomotion control.

## Features

- **VR welcome gate**
  - Shows a welcome panel on first launch
  - Disables locomotion until the player continues
  - Remembers first-time completion using `PlayerPrefs`

- **VR menu controller**
  - Toggles a menu with an input action
  - Spawns the menu in front of the player
  - Keeps the menu facing the headset/camera

- **Unity project setup**
  - URP template assets and settings
  - XR-related project structure
  - Visual Studio game development workload configuration

## Project Structure

- `Assets/`
  - `MenuManager.cs` — Handles showing/hiding the VR menu
  - `WelcomeGate.cs` — Controls first-time welcome UI and locomotion enablement
  - `box.cs` — Placeholder MonoBehaviour
  - `Readme.asset` — Unity template readme asset
  - Various Unity asset folders such as scenes, models, materials, audio, XR, and plugins

- `ProjectSettings/`
  - Unity project version and editor settings

- `.vsconfig`
  - Visual Studio configuration for Unity game development

- `ignore.conf`
  - Ignore rules for Unity-generated files and local editor/build artifacts

## Requirements

To open and run the project, you will need:

- **Unity Hub**
- **Unity Editor 2022.3.62f3**
- **Visual Studio** or another C# IDE
- Unity packages for:
  - XR Interaction Toolkit
  - Input System
  - URP

## Getting Started

1. Clone the repository.
2. Open the project in **Unity Hub** using **Unity 2022.3.62f3**.
3. Let Unity import the project assets and packages.
4. Open the main scene from the `Assets/Scenes` folder.
5. Press Play in the Unity Editor.
6. For VR testing, connect your XR device or use your configured XR simulation workflow.

## Scripts

### `MenuManager`
Controls an in-world menu that appears in front of the player when the configured input action is pressed.

**Key fields:**
- `head` — player camera/head transform
- `spawnDistance` — distance in front of the player to place the menu
- `menu` — menu GameObject
- `showButton` — input action used to toggle the menu

### `WelcomeGate`
Controls the initial welcome experience and disables locomotion until the user continues.

**Key fields:**
- `welcomePanel` — welcome UI panel
- `continueButton` — continue button
- `moveProvider` — continuous movement provider
- `turnProvider` — continuous turn provider
- `teleportProvider` — teleport locomotion provider
- `onlyFirstTime` — whether to show the welcome gate only once
- `playerPrefsKey` — PlayerPrefs key used to store completion

## Notes

- The project currently looks like an early-stage or template-based Unity VR build.
- Some files suggest the project may still be under active setup or expansion.
- `Assets/Readme.asset` identifies the project as an **URP Empty Template** base.

## License

No license file was found in the repository.
