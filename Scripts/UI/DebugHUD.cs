using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// DebugHUD — Shows useful debug info on screen during development.
/// 
/// This is a quick-and-dirty debug overlay using Unity's immediate-mode GUI (OnGUI).
/// It's not meant for the final game UI — we'll build proper UI with Unity's 
/// Canvas system later. This just helps us see what's happening while prototyping.
///
/// SETUP:
/// 1. Attach to the GameManager object (or any always-active object)
/// 2. Assign the Player reference in the Inspector
/// 3. Press Play — debug info appears in the top-left corner
/// </summary>
public class DebugHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;

    [Header("Display Options")]
    public bool showHUD = true;
    public KeyCode toggleKey = KeyCode.F1;

    // Styling
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;
    private bool stylesInitialized = false;

    void Update()
    {
        // Toggle HUD visibility with F1
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f1Key.wasPressedThisFrame)
        {
            showHUD = !showHUD;
        }
    }

    void InitStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.7f));

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = new Color(0.4f, 0.9f, 0.7f);

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (!showHUD) return;
        if (!stylesInitialized) InitStyles();

        float boxWidth = 280;
        float boxHeight = 240;
        float padding = 10;

        // Background box
        GUI.Box(new Rect(padding, padding, boxWidth, boxHeight), "", boxStyle);

        float x = padding + 10;
        float y = padding + 5;
        float lineHeight = 22;

        // Title
        GUI.Label(new Rect(x, y, boxWidth, 25), "🐚 CORAL CRITTER DEBUG", headerStyle);
        y += lineHeight + 5;

        // FPS
        float fps = 1f / Time.unscaledDeltaTime;
        GUI.Label(new Rect(x, y, boxWidth, 25), $"FPS: {fps:F0}", labelStyle);
        y += lineHeight;

        // Game State
        string state = GameManager.Instance != null ? GameManager.Instance.currentState.ToString() : "No GameManager";
        GUI.Label(new Rect(x, y, boxWidth, 25), $"State: {state}", labelStyle);
        y += lineHeight;

        // Player info
        if (player != null)
        {
            GUI.Label(new Rect(x, y, boxWidth, 25), $"Speed: {player.GetCurrentSpeed():F1}", labelStyle);
            y += lineHeight;

            string status = player.IsGrounded() ? "Grounded" : (player.IsJumping() ? "Jumping" : "Falling");
            GUI.Label(new Rect(x, y, boxWidth, 25), $"{status}  Sprint: {player.IsSprinting()}", labelStyle);
            y += lineHeight;

            Vector3 pos = player.transform.position;
            GUI.Label(new Rect(x, y, boxWidth, 25), $"Pos: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})", labelStyle);
            y += lineHeight;
        }
        else
        {
            GUI.Label(new Rect(x, y, boxWidth, 25), "Player: Not assigned", labelStyle);
            y += lineHeight;
        }

        // Controls reminder
        y += 5;
        GUI.Label(new Rect(x, y, boxWidth, 25), "WASD=Move Shift=Sprint Space=Jump F1=Toggle", labelStyle);
    }

    /// <summary>
    /// Creates a simple colored texture for the GUI background.
    /// This is a standard Unity trick for custom GUI styling.
    /// </summary>
    Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
