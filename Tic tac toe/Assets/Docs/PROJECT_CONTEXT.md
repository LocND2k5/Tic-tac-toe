# TIC TAC TOE MULTIPLAYER - PROJECT CONTEXT

> **Target Audience:** Future Developers, Team Members, and AI Coding Agents inheriting this codebase.  
> **Repository:** Unity 2D Multiplayer Tic Tac Toe  
> **Last Updated:** September 2026  

---

## 1. Project Overview & Vision

### 1.1 Executive Summary
Tic Tac Toe Multiplayer is a modern, modular, cross-platform 2D adaptation of the classic tabletop game built using **Unity 6** and **Unity Netcode for GameObjects (NGO)**. The game is inspired by Code Monkey's multiplayer architecture but has evolved to support:
- Dynamic board scaling (3x3, 5x5, 7x7 with configurable win conditions).
- Dual-mode gameplay: Local Hotseat (Offline) and Networked Multiplayer (Online).
- Strict Event-Driven Architecture (Observer Pattern) ensuring that Game Logic, Visual Presentation, User Input, and Networking remain decoupled.

### 1.2 Core Design Pillars
1. **Zero Friction Onboarding:** Immediate accessibility with zero learning curve.
2. **Deterministic Fairness:** Server-authoritative logic with mathematically rigorous win/tie evaluation.
3. **Decoupled Architecture:** Clean separation of concerns (Model-View-Controller) enabling parallel feature development without git merge conflicts.
4. **Performance & Modern Standards:** Built on Unity's New Input System (`com.unity.inputsystem`) and Universal Render Pipeline (URP 2D).

---

## 2. Technology Stack & Environment

| Component | Technology | Version / Specification |
| :--- | :--- | :--- |
| **Engine** | Unity Engine | `6000.3.15f1` (Unity 6) |
| **Render Pipeline** | Universal Render Pipeline (URP) | 2D Renderer (`UniversalRP.asset`) |
| **Networking Framework** | Netcode for GameObjects (NGO) | `com.unity.netcode.gameobjects` v2.13.2 |
| **Network Transport** | Unity Transport Package (UTP) | UDP / DTLS (`UnityTransport`) |
| **Input System** | Unity Input System (New) | `com.unity.inputsystem` v1.19.0 |
| **UI Framework** | Unity UI (uGUI) + TextMeshPro | `com.unity.ugui` v2.0.0 |
| **Language & Runtime** | C# (.NET Standard / Roslyn) | C# 12+ Features Supported |

---

## 3. Directory & File Structure Map

```
Assets/
├── Docs/                                 # Central Project Documentation
│   ├── README.md                         # Documentation Hub & Navigation
│   ├── PROJECT_CONTEXT.md                # This File: Vision, Architecture & Codebase Map
│   ├── SPECS_MULTIPLAYER.md              # Technical Specification for NGO Multiplayer
│   └── ARCHITECTURE_GUIDELINES.md        # Coding Standards, SOLID Rules & Anti-Patterns
├── Editor/                               # Custom Editor Tools & Automated Scene Setup
│   ├── MenuSceneBuilder.cs               # Generates Grid-Selection Menu Scene
│   └── TurnUIBuilder.cs                  # Generates Turn Indicator UI inside Canvas
├── Khang/                                # Contributed by Khang: Main Menu Architecture
│   └── Scripts/
│       ├── Commands/                     # Command Pattern Implementations
│       │   ├── ChangeStateCommand.cs     # Panel navigation command
│       │   ├── LoadSceneCommand.cs       # Scene transition command
│       │   ├── LogCommandDecorator.cs    # Decorator: Telemetry/logging wrapper
│       │   ├── QuitAppCommand.cs         # Application exit command
│       │   └── StartMatchCommand.cs      # Match initiation with GameMode & GridSize
│       ├── Core/                         # Core Interfaces & Scriptable Architectures
│       │   ├── GameConfig.cs             # Static Game Configuration carrier
│       │   ├── ICommand.cs               # Command Pattern interface
│       │   ├── IMenuState.cs             # State Pattern interface for UI panels
│       │   └── IUIAnimationStrategy.cs   # Strategy Pattern for menu animations
│       ├── MainMenuSystem.cs             # Main Menu Controller (State Machine host)
│       ├── States/                       # Panel States (MainPanelState, GenericPanelState)
│       └── Strategies/                   # Animation Strategies (InstantAnimationStrategy)
├── Prefabs/                              # Reusable Prefab Assets
│   ├── Circle.prefab                     # O Mark (Contains child SpriteRenderer)
│   ├── Cross.prefab                      # X Mark (Contains child SpriteRenderer)
│   ├── GridPosition.prefab               # Interactive Cell with BoxCollider2D & Script
│   └── LineComplete.prefab               # Winning Line (Green Sprite, dynamically scaled)
├── Scenes/                               # Unity Scenes
│   ├── MainMenuScene.unity               # Full Main Menu (Local, AI, Online, Settings)
│   ├── MenuScene.unity                   # Grid Selection Menu (3x3, 5x5, 7x7)
│   └── SampleScene.unity                 # Active Gameplay Arena (DynamicBoard, UI, Visuals)
├── Scripts/_Loc/                         # Core Gameplay Architecture (Contributed by Lộc & Pair)
│   ├── BoardGenerator.cs                 # Procedural Grid & Line Generation
│   ├── GameManager.cs                    # Central Model, Logic, Win Evaluator & Publisher
│   ├── GameTurnUI.cs                     # UI: Dynamic arrows & 'YOU' indicator
│   ├── GameVisualManager.cs              # View: Spawns X, O, and LineComplete visuals
│   ├── GridPosition.cs                   # Controller: New Input System IPointerClickHandler
│   ├── LevelSelectButton.cs              # UI Helper for Grid Selection Buttons
│   ├── MainMenuUI.cs                     # Static Bridge for SelectedBoardSize
│   └── RematchUI.cs                      # UI: Conditional Rematch button (Shows on Win/Tie)
└── Sprites/                              # High-Resolution Game Sprites
    ├── Background.png                    # Game Arena Background
    ├── Circle.png                        # Circle Mark Sprite
    ├── Cross.png                         # Cross Mark Sprite
    ├── Line.png                          # Grid Separator Line Sprite
    └── LineGreen.png                     # Celebration Winning Line Sprite
```

---

## 4. Architectural Patterns Applied

The project strictly avoids monolithic scripts by applying well-established Software Engineering and Game Design patterns:

### 4.1 Decoupled Model-View-Controller (MVC / Separation of Concerns)
- **Model (`GameManager.cs`):** Maintains the board array (`PlayerType[,]`), tracks active turn, executes win/tie algorithms, and controls game state. It contains **zero visual or UI dependencies**.
- **View (`GameVisualManager.cs` & `RematchUI.cs` & `GameTurnUI.cs`):** Subscribes to Model events to spawn prefabs, adjust visual scales, move UI arrows, and reveal buttons.
- **Controller (`GridPosition.cs`):** Attaches to individual cells, listens for hardware clicks via Unity's EventSystem, and relays sanitized `(x, y)` coordinates to the Model.

### 4.2 Observer Pattern (C# Events)
`GameManager` serves as the event publisher, exposing standard `EventHandler` delegates:
- `OnClickedOnGridPosition(x, y, playerType)`
- `OnGameWin(line, winPlayerType)`
- `OnGameTied`
- `OnCurrentPlayablePlayerTypeChanged`
- `OnRematch`
**Key Benefit:** Visuals, UI, and future Audio systems hook into game logic without the Model ever needing references to them. Team members can modify visuals without risking game logic regressions.

### 4.3 Singleton Pattern (Controlled Coordinator)
`GameManager.Instance` provides a single global access point for input controllers (`GridPosition`) and UI buttons (`RematchUI`) to dispatch actions without requiring tedious Inspector cross-referencing.

### 4.4 Command & State Patterns (Menu System)
Contributed in `Assets/Khang/`:
- **Command Pattern (`ICommand`):** Encapsulates UI actions (Scene loading, mode selection, panel switching) into standalone command objects.
- **Decorator Pattern (`LogCommandDecorator`):** Intercepts commands to log analytics/debugging information transparently.
- **State Pattern (`IMenuState`):** Treats each menu view as an isolated state with dedicated enter/exit behaviors.

### 4.5 Event-Driven Hardware Input
`GridPosition.cs` implements `IPointerClickHandler` instead of polling `Update()` raycasts:
- Eliminates CPU overhead across inactive frames.
- Automatically respects UI raycast blockers (`GraphicRaycaster`), preventing clicks on cells underneath popup menus.

---

## 5. Current State & Completed Features

1. **Dynamic Board Generation:** Supports arbitrary N x N dimensions (3x3 with 3 in a row; 5x5 with 4 in a row; 7x7 with 5 in a row). Grid lines and collider hitboxes are procedurally calculated to fit screen space.
2. **Full Gameplay Loop:** Alternating turns between Cross and Circle, cell-occupancy checks, 8-directional win evaluation, tie evaluation, and single-click rematch board clears.
3. **Dynamic Visual Scaling:** Visual pieces and celebration win lines automatically compute scale multipliers to fit any board size cleanly.
4. **Context-Aware Rematch UI:** The rematch button remains hidden during active play and dynamically displays only upon a Win or Tie condition.
5. **Turn UI Indicator:** Real-time indicator displaying whose turn it is with dedicated visual arrows and active text badges.

---

## 6. Future Project Roadmap

- [x] **Sprint 1 (Local Foundation):** Core 3x3 Logic, Observer Events, Visual Spawning, Win Line.
- [x] **Sprint 2 (Polish & Extensions):** New Input System migration, Dynamic Board Generator, Turn UI, Rematch System, Menu System.
- [x] **Sprint 3 (NGO Multiplayer - Completed):**
  - [x] Integrate `NetworkManager` and `UnityTransport`.
  - [x] Convert `GameManager` to authoritative `NetworkBehaviour`.
  - [x] Authorize moves via `ServerRpc` and broadcast updates via `ClientRpc` / `NetworkVariable`.
  - [x] Local/Online Dual-Mode Adapter, Rematch Handshake & Disconnect Handling.
- [ ] **Sprint 4 (Lobby & Online Connectivity):**
  - Unity Relay service integration (Join Code connection).
  - Unity Lobby service for room discovery.
- [ ] **Sprint 5 (Audio & Juiciness):**
  - `SoundManager` listening to Observer events for piece placement, victory, defeat, and button clicks.
  - Tweening / particle effects on piece placement.
