# TIC TAC TOE - ARCHITECTURE & CODING GUIDELINES

> **Purpose:** Engineering standards, design patterns, and best practices for developers and AI agents working on this repository.  
> **Repository:** Unity 2D Multiplayer Tic Tac Toe  
> **Status:** Active / Enforced  
> **Last Updated:** September 2026  

---

## 1. Architectural Philosophy

### 1.1 The Golden Rule: Decoupling via Events
Our architecture prioritizes **loose coupling** above all else. 
- **Domain Logic** (Game state, grid calculation, win evaluation, turn order) must **never** reference **Visual / Presentation / Audio** code directly.
- **Presentation** (`GameVisualManager`, `GameTurnUI`, `RematchUI`, audio players) acts strictly as an **Observer**: it listens to C# `Action` events published by the Domain layer and updates the view accordingly.

```
+-----------------------------------------------------------+
|               MODEL / LOGIC LAYER (Domain)                |
|  - GameManager / NetworkGameManager                       |
|  - Evaluates rules, wins, ties, valid turns               |
|  - Publishes C# Events (OnClickedOnGridPosition, etc.)    |
+-----------------------------+-----------------------------+
                              |
                     Publishes C# Events
                              |
                              v
+-----------------------------------------------------------+
|              VIEW / PRESENTATION LAYER (View)             |
|  - GameVisualManager (Spawns X/O icons, draws line)       |
|  - GameTurnUI (Toggles active turn indicators)            |
|  - RematchUI (Displays rematch button & panel)            |
+-----------------------------------------------------------+
```

### 1.2 Multi-Paradigm Integration: Menu System
The Khang feature module showcases clean, enterprise-grade architecture:
- **Command Pattern (`ICommand`):** Encapsulates UI actions (`StartMatchCommand`, `ChangeStateCommand`, `QuitGameCommand`).
- **State Pattern (`IMenuState`):** Controls screen state transitions (`MainState`, `SettingsState`, `MatchmakingState`).
- **Strategy Pattern (`IUIAnimationStrategy`):** Decouples UI animation algorithms (Fade, Slide, Scale) from controller logic.
- **Decorator Pattern (`LogCommandDecorator`):** Dynamically injects diagnostics and logging without modifying command logic.

---

## 2. SOLID Principles in Unity Context

| Principle | Unity / Project Implementation | Good Example | Anti-Pattern to Avoid |
| :--- | :--- | :--- | :--- |
| **S - Single Responsibility** | Every class has exactly one reason to change. | `GameTurnUI` only animates turn arrows. `GameManager` only calculates board state. | `GameManager` spawning GameObjects, playing audio, and updating text directly. |
| **O - Open / Closed** | Open for extension, closed for modification. | Adding online multiplayer uses network RPCs to invoke existing events without modifying visual/UI scripts. | Modifying `GameVisualManager.cs` every time a network packet format changes. |
| **L - Liskov Substitution** | Derived classes can substitute base classes without breaking behavior. | Any `ICommand` or `IUIAnimationStrategy` can be swapped interchangeably. | Subclasses throwing `NotImplementedException` for inherited methods. |
| **I - Interface Segregation** | Fine-grained interfaces rather than monolithic ones. | Separate `ICommand`, `IMenuState`, `IUIAnimationStrategy`. | A giant `IGameManager` with 30 unrelated methods. |
| **D - Dependency Inversion** | Depend on abstractions, not concrete implementations. | Presenters depend on `Action` delegates or interfaces rather than static singletons where practical. | UI components hardcoded to search `FindObjectOfType<GameManager>()` in `Start()`. |

---

## 3. Strict Anti-Patterns (Zero Tolerance)

### 3.1 No Magic Numbers or Unnamed Literals
- **Forbidden:**
  ```csharp
  // BAD
  transform.position = new Vector3(x * 3.1f, y * 3.1f, 0);
  if (playerCount > 2) return;
  ```
- **Mandatory:**
  ```csharp
  // GOOD
  public static class GameConstants
  {
      public const float CELL_SPACING = 3.1f;
      public const int MAX_PLAYERS = 2;
  }
  transform.position = new Vector3(x * GameConstants.CELL_SPACING, y * GameConstants.CELL_SPACING, 0);
  ```

### 3.2 No Polling in `Update()`
- Do not check boolean flags or game states inside `Update()`:
  ```csharp
  // BAD: Constant wasted CPU cycles
  void Update()
  {
      if (GameManager.Instance.IsGameOver())
      {
          rematchButton.SetActive(true);
      }
  }
  ```
- Use **Event-Driven notifications**:
  ```csharp
  // GOOD: Executes exactly once upon state change
  void OnEnable()
  {
      GameManager.Instance.OnGameWin += HandleGameWin;
      GameManager.Instance.OnGameTied += HandleGameTied;
  }
  ```

### 3.3 Strict Unsubscription from Events
Every component that subscribes to an event in `Start()` or `OnEnable()` **MUST** unsubscribe in `OnDestroy()` or `OnDisable()` to prevent memory leaks and `MissingReferenceException` when scenes reload.
```csharp
private void OnEnable()
{
    GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
}

private void OnDisable()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnClickedOnGridPosition -= GameManager_OnClickedOnGridPosition;
    }
}
```

### 3.4 No Direct Scene Coupling
Never use hardcoded string scene names scattered across scripts. Use `MultiplayerConstants.SCENE_MAIN_MENU` or `SceneUtility`.

---

## 4. C# Coding Standards & Conventions

### 4.1 Naming Conventions
- **Classes, Structs, Enums, Interfaces:** PascalCase (`GameManager`, `ICommand`, `PlayerType`).
- **Interfaces:** Prefix with `I` (`ICommand`, `IMenuState`).
- **Methods, Properties, Public Events:** PascalCase (`ClickedOnGridPosition()`, `CurrentTurn`, `OnGameWin`).
- **Private Fields:** camelCase with leading underscore (`_currentTurn`, `_boardSize`, `_rematchButton`).
- **Serialized Private Fields:** `[SerializeField] private GameObject _crossPrefab;`
- **Constants:** UPPER_CASE_SNAKE or PascalCase (`MAX_PLAYERS`, `DEFAULT_PORT`).
- **Event Handler Methods:** `PublisherClass_EventName` (e.g., `GameManager_OnGameWin`).

### 4.2 Unity-Specific Guidelines
1. **Input System:** Always use the New Input System (`UnityEngine.InputSystem` or `IPointerClickHandler`). Do not introduce `Input.GetMouseButtonDown()`.
2. **Meta Files:** Do not hand-edit or manually construct `.meta` files. Let the Unity Editor or Unity MCP generate them naturally.
3. **Async / Coroutines:** Clean up active coroutines or cancellation tokens when switching scenes.

---

## 5. Network (NGO) Safety Rules

1. **Server Authority is Absolute:**
   - Clients never dictate board state, wins, or ties.
   - Clients can only send *intents* (requests) via `[ServerRpc]`.
   - The Server validates all coordinates, boundaries, turn ownership, and game status before changing state.
2. **Minimal Network Bandwidth:**
   - Send only primitive data (integers, byte coordinates, enums) over RPCs.
   - Do not serialize entire GameObjects or large strings unless strictly necessary.
3. **Idempotence & Safety:**
   - All RPCs must be safe against out-of-order or duplicate calls (e.g., rapid button spamming).

---

## 6. Definition of Done (DoD) Checklist

Before marking any feature complete:
- [ ] Code compiles without any warnings or errors.
- [ ] Conforms to Event-Driven / Observer pattern (no tight cross-coupling).
- [ ] Zero magic numbers; all constants are defined in appropriate static classes.
- [ ] All event subscriptions have symmetric unsubscriptions.
- [ ] Unit tested or validated inside Unity Editor play mode.
