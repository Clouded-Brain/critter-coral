using UnityEngine;

/// <summary>
/// GameManager — Central manager for game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Exploring,
        Tracking,
        ChaseCapture,
        AutoBattle,
        BaseBuilding,
        Inventory,
        Cutscene
    }

    [Header("Current State")]
    public GameState currentState = GameState.Exploring;

    [Header("Game Stats (for debugging)")]
    public int creaturesDiscovered = 0;
    public int creaturesCaptured = 0;
    public int battlesWon = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: Duplicate found — destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Safety: always restore realtime simulation on startup.
        Time.timeScale = 1f;
    }

    void Start()
    {
        ApplyTimeScaleForState(currentState);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            // Safety for editor play mode stop / object teardown.
            Time.timeScale = 1f;
        }
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        Debug.Log($"GameManager: State changed from {previousState} → {newState}");
        OnStateChanged(previousState, newState);
    }

    void OnStateChanged(GameState from, GameState to)
    {
        ApplyTimeScaleForState(to);
    }

    void ApplyTimeScaleForState(GameState state)
    {
        switch (state)
        {
            case GameState.Inventory:
                Time.timeScale = 0f;
                break;
            default:
                Time.timeScale = 1f;
                break;
        }
    }

    public bool CanPlayerMove()
    {
        return currentState == GameState.Exploring
            || currentState == GameState.Tracking;
    }

    public bool IsInCombat()
    {
        return currentState == GameState.AutoBattle;
    }

    public bool IsInChase()
    {
        return currentState == GameState.ChaseCapture;
    }
}
