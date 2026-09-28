# TIC TAC TOE - DOCUMENTATION HUB

Welcome to the **Tic Tac Toe Multiplayer** engineering and design documentation repository. This directory serves as the centralized knowledge base for human developers, teammates, and AI agents collaborating on this project.

---

## 📚 Documentation Index

| Document | Primary Audience | Description |
| :--- | :--- | :--- |
| **[PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md)** | All Developers / AI Agents | Complete high-level project context, core pillars, technology stack, directory structure, design patterns catalog (MVC, Observer, Command, State, Strategy, Decorator), and feature roadmap. |
| **[SPECS_MULTIPLAYER.md](./SPECS_MULTIPLAYER.md)** | Network & Gameplay Engineers | Comprehensive technical specification for Netcode for GameObjects (NGO) online multiplayer: Listen-Server topology, RPC contracts, sequence diagrams, rematch handshake, and edge case handling. |
| **[ARCHITECTURE_GUIDELINES.md](./ARCHITECTURE_GUIDELINES.md)** | All Contributors / Code Reviewers | Engineering standards, SOLID principles applied to Unity, strict anti-patterns (no magic numbers, no Update polling), memory leak prevention, and Definition of Done. |

---

## 🚀 Quick Orientation for New Contributors & AI Agents

### 1. Where is the Code?
- **Core Gameplay & Logic:** `Assets/Scripts/_Loc/`
  - `GameManager.cs`: Turn logic, grid validation, win/tie evaluation, and C# event dispatcher.
  - `BoardGenerator.cs`: Procedural grid and line generator for dynamic board sizes (3x3, 5x5, 7x7).
  - `GameVisualManager.cs`: Presentation layer spawning visual 'X' and 'O' markers and win strike lines.
  - `GameTurnUI.cs` & `RematchUI.cs`: uGUI presenters reacting to game events.
  - `GridPosition.cs`: Clickable cell interaction via Unity's New Input System (`IPointerClickHandler`).
- **Menu & UI Architecture:** `Assets/Khang/`
  - Implements Command (`ICommand`), State (`IMenuState`), Strategy (`IUIAnimationStrategy`), and Decorator (`LogCommandDecorator`) patterns.

### 2. Core Architectural Invariant
> **NEVER break the Event-Driven Boundary.**  
> The Presentation Layer (`GameVisualManager`, `GameTurnUI`, `RematchUI`) must **never** be tightly coupled to networking or domain logic. They must **only** react to C# events published by `GameManager`. When implementing Multiplayer, trigger the existing events upon receiving server RPCs.

### 3. Current Phase
- **Active Phase:** Phase 3 — Online Multiplayer Integration (NGO v2.13.2).
- **Refer to:** `SPECS_MULTIPLAYER.md` for task breakdown and RPC signatures.

---

*Maintained by the Tic Tac Toe Engineering Team.*
