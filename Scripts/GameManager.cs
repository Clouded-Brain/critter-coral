using UnityEngine;

/// <summary>
/// GameManager — Central manager for game state.
/// 
/// HOW IT WORKS:
/// This is a "Singleton" — a design pattern that ensures only one GameManager 
/// exists in the entire game. Any script can access it via GameManager.Instance.
///
/// WHY:
/// Many systems (UI, creatures, combat) need to know about game state like
/// whether we're in exploration mode, a chase, or a battle. The GameManager
/// is the single source of truth for this.
///
/// SETUP:
/// 1. Create an empty GameObject called "GameManager"
/// 2. Attach this script to it
/// 3. That's it — other scripts access it via GameManager.Instance
/// </summary>
public class GameManager : MonoBehaviour
{
    // ───────────────────────────────────────────────
    // SINGLETON PATTERN
    // ───────────────────────────────────────────────

    /// <summary>
    /// The single instance of GameManager. Access from any script:
    ///   GameManager.Instance.currentState
    /// </summary>
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // If an instance already exists and it's not this one, destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: Duplicate found — destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep this object alive when loading new scenes
        DontDestroyOnLoad(gameObject);
    }

    // ───────────────────────────────────────────────
    // GAME STATE
    // ───────────────────────────────────────────────

    /// <summary>
    /// All the possible states the game can be in.
    /// This determines which systems are active at any given time.
    /// </summary>
    public enum GameState
    {
        Exploring,      // Free roam on an island
        Tracking,       // Following creature tracks
        ChaseCapture,   // Active safari chase encounter
        AutoBattle,     // Squad combat in progress
        BaseBuilding,   // Building/managing home base
        Inventory,      // Managing creatures/items (paused)
        Cutscene        // Cinematic moment
    }

    [Header("Current State")]
    public GameState currentState = GameState.Exploring;

    [Header("Game Stats (for debugging)")]
    public int creaturesDiscovered = 0;
    public int creaturesCaptured = 0;
    public int battlesWon = 0;

    // ───────────────────────────────────────────────
    // STATE TRANSITIONS
    // ───────────────────────────────────────────────

    /// <summary>
    /// Changes the game state and notifies any listening systems.
    /// Always use this method instead of setting currentState directly.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"GameManager: State changed from {previousState} → {newState}");

        // Notify all systems about the state change
        OnStateChanged(previousState, newState);
    }

    /// <summary>
    /// Handles any global logic needed when switching states.
    /// As we add more systems, we'll expand this.
    /// </summary>
    void OnStateChanged(GameState from, GameState to)
    {
        // Pause/unpause based on state
        switch (to)
        {
            case GameState.Inventory:
                Time.timeScale = 0f; // Pause the game
                break;
            default:
                Time.timeScale = 1f; // Unpause
                break;
        }

        // Future: Fire events for UI updates, music changes, etc.
        // Example: OnGameStateChanged?.Invoke(from, to);
    }

    // ───────────────────────────────────────────────
    // HELPER QUERIES
    // ───────────────────────────────────────────────

    /// <summary>
    /// Can the player move freely right now?
    /// </summary>
    public bool CanPlayerMove()
    {
        return currentState == GameState.Exploring
            || currentState == GameState.Tracking;
    }

    /// <summary>
    /// Is the game in a combat-related state?
    /// </summary>
    public bool IsInCombat()
    {
        return currentState == GameState.AutoBattle;
    }

    /// <summary>
    /// Is the game in a chase/capture state?
    /// </summary>
    public bool IsInChase()
    {
        return currentState == GameState.ChaseCapture;
    }
}
