using UnityEngine;
using UnityEditor;

/// <summary>
/// CoralCritterSceneBuilder — One-click full scene setup.
/// 
/// HOW TO USE:
/// 1. Copy ALL the .cs scripts into your Assets/Scripts/ folder (subfolders are fine)
/// 2. Put THIS script inside Assets/Editor/ (create that folder if it doesn't exist)
/// 3. In Unity's top menu bar, click: Coral Critter → Build Prototype Scene
/// 4. Everything is created and wired up automatically!
///
/// WHAT IT DOES:
/// - Creates the scene hierarchy (Player, CameraRig, GameManager, Island, Lighting)
/// - Attaches all scripts to the correct objects
/// - Wires all Inspector references (Target, Camera, Player, etc.)
/// - Configures Rigidbody, Camera, and all component settings
/// - Sets up a directional light for good isometric visibility
/// </summary>
public class CoralCritterSceneBuilder : EditorWindow
{
    [MenuItem("Coral Critter/Build Prototype Scene")]
    public static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Coral Critter — Scene Builder",
            "This will set up the full prototype scene in your current scene.\n\n" +
            "It will create:\n" +
            "• Player (with movement controller)\n" +
            "• Isometric Camera Rig\n" +
            "• Game Manager + Debug HUD\n" +
            "• Island Generator + Water\n" +
            "• Directional Light\n\n" +
            "Existing objects with the same names will be replaced.\n\n" +
            "Continue?",
            "Build It!", "Cancel"))
        {
            return;
        }

        Debug.Log("🐚 Coral Critter: Building prototype scene...");

        // Clean up any existing objects from a previous build
        CleanupExisting();

        // ─── 1. PLAYER ─────────────────────────────────────
        GameObject player = CreatePlayer();

        // ─── 2. CAMERA RIG ─────────────────────────────────
        GameObject cameraRig = CreateCameraRig(player);

        // ─── 3. GAME MANAGER ───────────────────────────────
        GameObject gameManager = CreateGameManager(player);

        // ─── 4. ISLAND GENERATOR ───────────────────────────
        GameObject islandGen = CreateIslandGenerator();

        // ─── 5. LIGHTING ───────────────────────────────────
        SetupLighting();

        // ─── 6. DELETE DEFAULT OBJECTS ──────────────────────
        CleanupDefaults();

        // ─── 7. SELECT PLAYER SO USER CAN SEE IT ──────────
        Selection.activeGameObject = player;

        // Mark scene as dirty so Unity knows to save changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("🐚 Coral Critter: Scene build complete! Press Play to test.");
        Debug.Log("   Controls: WASD = Move, Shift = Sprint, Scroll = Zoom, F1 = Debug HUD");

        EditorUtility.DisplayDialog(
            "✅ Scene Built!",
            "Your Coral Critter prototype scene is ready!\n\n" +
            "Press PLAY to test.\n\n" +
            "Controls:\n" +
            "• WASD / Arrows = Move\n" +
            "• Left Shift = Sprint\n" +
            "• Space = Jump (hold for higher)\n" +
            "• Mouse Scroll = Zoom\n" +
            "• F1 = Toggle Debug HUD",
            "Let's Go!");
    }

    // ═══════════════════════════════════════════════════════
    // CREATION METHODS
    // ═══════════════════════════════════════════════════════

    static GameObject CreatePlayer()
    {
        Debug.Log("   Creating Player...");

        // Create capsule as placeholder player model
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 30f, 0f); // Start high — will fall to terrain
        player.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Slightly bigger so we can see it

        // ── Rigidbody (physics) ──
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.linearDamping = 2f;                  // Higher drag = less sliding around
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        // ── Capsule Collider — use the existing one, just adjust ──
        CapsuleCollider col = player.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            PhysicsMaterial noSlide = new PhysicsMaterial("NoSlide");
            noSlide.dynamicFriction = 1f;
            noSlide.staticFriction = 1f;
            noSlide.frictionCombine = PhysicsMaterialCombine.Maximum;
            col.material = noSlide;
        }

        // ── Player Controller script ──
        PlayerController controller = player.AddComponent<PlayerController>();
        controller.moveSpeed = 12f;
        controller.rotationSpeed = 10f;
        controller.acceleration = 60f;
        controller.isoAngle = 45f;
        controller.sprintMultiplier = 1.8f;
        controller.jumpForce = 10f;
        controller.holdJumpForce = 18f;
        controller.maxJumpHoldTime = 0.25f;
        controller.gravityMultiplier = 3f;
        controller.fallMultiplier = 4f;
        controller.maxSlopeAngle = 55f;
        controller.slopeAssistForce = 15f;

        // ── Player Material (bright teal so it stands out) ──
        Renderer renderer = player.GetComponent<Renderer>();
        Material playerMat = CreateSafeMaterial(new Color(0f, 1f, 0.8f), "PlayerMaterial");
        if (playerMat != null)
        {
            renderer.material = playerMat;
        }

        return player;
    }

    static GameObject CreateCameraRig(GameObject player)
    {
        Debug.Log("   Creating Camera Rig...");

        // ── Camera Rig (parent object) ──
        GameObject cameraRig = new GameObject("CameraRig");
        cameraRig.transform.position = player.transform.position;

        // ── Main Camera (child of rig) ──
        // Find existing main camera or create one
        Camera existingCam = Camera.main;
        GameObject camObj;

        if (existingCam != null)
        {
            camObj = existingCam.gameObject;
            camObj.name = "Main Camera";
        }
        else
        {
            camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        // Parent camera to rig
        camObj.transform.SetParent(cameraRig.transform);
        camObj.transform.localPosition = Vector3.zero;
        camObj.transform.localRotation = Quaternion.identity;

        // Configure camera for isometric
        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 12f;     // Zoomed in enough to see the player clearly
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 500f;        // Far enough to see whole terrain
        cam.backgroundColor = new Color(0.05f, 0.08f, 0.15f); // Dark ocean blue

        // ── Isometric Camera Controller ──
        IsometricCameraController isoController = cameraRig.AddComponent<IsometricCameraController>();
        isoController.target = player.transform;
        isoController.cameraTransform = camObj.transform;
        isoController.pitch = 35f;
        isoController.yaw = 45f;
        isoController.distance = 40f;       // Far enough to not clip terrain hills
        isoController.followSmoothness = 10f;
        isoController.minZoom = 5f;
        isoController.maxZoom = 30f;
        isoController.zoomSpeed = 3f;
        isoController.zoomSmoothness = 8f;

        return cameraRig;
    }

    static GameObject CreateGameManager(GameObject player)
    {
        Debug.Log("   Creating Game Manager...");

        GameObject gmObj = new GameObject("GameManager");

        // ── Game Manager ──
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.currentState = GameManager.GameState.Exploring;

        // ── Debug HUD ──
        DebugHUD hud = gmObj.AddComponent<DebugHUD>();
        hud.player = player.GetComponent<PlayerController>();
        hud.showHUD = true;

        return gmObj;
    }

    static GameObject CreateIslandGenerator()
    {
        Debug.Log("   Creating Island Generator...");

        GameObject islandGenObj = new GameObject("IslandGenerator");

        SimpleIslandGenerator gen = islandGenObj.AddComponent<SimpleIslandGenerator>();
        gen.terrainSize = 200;
        gen.terrainHeight = 25f;
        gen.resolution = 200;
        gen.islandFalloff = 2.5f;
        gen.baseNoiseScale = 8f;
        gen.ridgeNoiseScale = 15f;
        gen.ridgeStrength = 0.4f;
        gen.flatness = 0.35f;
        gen.terraceSteps = 8;
        gen.seed = 42;
        gen.waterLevel = 0.06f;
        gen.waterColor = new Color(0.08f, 0.35f, 0.6f, 0.75f);

        return islandGenObj;
    }

    static void SetupLighting()
    {
        Debug.Log("   Setting up lighting...");

        // Find existing directional light or create one
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        GameObject lightObj = null;

        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                lightObj = light.gameObject;
                break;
            }
        }

        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.AddComponent<Light>();
        }

        lightObj.name = "Directional Light";

        // Angle the light to complement the isometric view
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        Light dirLight = lightObj.GetComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.color = new Color(1f, 0.95f, 0.85f); // Warm sunlight
        dirLight.intensity = 1.2f;
        dirLight.shadows = LightShadows.Soft;
    }

    // ═══════════════════════════════════════════════════════
    // SAFE MATERIAL CREATION
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Creates a material with shader fallback chain that works in any pipeline.
    /// Shader.Find can return null in editor for URP shaders, so we try multiple.
    /// </summary>
    static Material CreateSafeMaterial(Color color, string name = "Material")
    {
        // Try shaders in order of preference
        string[] shaderNames = {
            "Universal Render Pipeline/Lit",
            "Standard",
            "Diffuse",
            "Sprites/Default"
        };

        Shader shader = null;
        foreach (string sn in shaderNames)
        {
            shader = Shader.Find(sn);
            if (shader != null) break;
        }

        if (shader == null)
        {
            Debug.LogWarning($"Could not find any shader for material '{name}'. Using default.");
            return null;
        }

        Material mat = new Material(shader);
        mat.name = name;
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        return mat;
    }

    // ═══════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Removes objects from a previous scene build so we don't get duplicates.
    /// </summary>
    static void CleanupExisting()
    {
        string[] objectNames = {
            "Player", "CameraRig", "GameManager", "IslandGenerator",
            "Island", "WaterPlane", "DeepWater", "Ground", "Props",
            "Landmarks", "CenterBeacon"
        };

        foreach (string name in objectNames)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        // Also clean up any Unity Terrain objects from previous generation
        Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
        foreach (Terrain t in terrains)
        {
            Object.DestroyImmediate(t.gameObject);
        }

        // Clean up any leftover mesh islands
        MeshFilter[] meshes = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
        foreach (MeshFilter mf in meshes)
        {
            if (mf.gameObject.name == "Island")
            {
                Object.DestroyImmediate(mf.gameObject);
            }
        }
    }

    /// <summary>
    /// Removes default scene objects that we don't need (like the default camera
    /// if we've already created our own).
    /// </summary>
    static void CleanupDefaults()
    {
        // Remove any extra cameras (we keep only the one under CameraRig)
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in cameras)
        {
            if (cam.transform.parent == null || cam.transform.parent.name != "CameraRig")
            {
                // Check if it's an orphan main camera
                if (cam.gameObject.name == "Main Camera" && cam.transform.parent == null)
                {
                    Object.DestroyImmediate(cam.gameObject);
                }
            }
        }
    }

    // ═══════════════════════════════════════════════════════
    // BONUS UTILITIES IN THE MENU
    // ═══════════════════════════════════════════════════════

    [MenuItem("Coral Critter/Regenerate Island (New Seed)")]
    public static void RegenerateIsland()
    {
        SimpleIslandGenerator gen = Object.FindFirstObjectByType<SimpleIslandGenerator>();
        if (gen == null)
        {
            Debug.LogWarning("No IslandGenerator found in scene. Run Build Prototype Scene first.");
            return;
        }

        // Clean up old island and water
        GameObject oldIsland = GameObject.Find("Island");
        if (oldIsland != null) Object.DestroyImmediate(oldIsland);

        GameObject oldWater = GameObject.Find("WaterPlane");
        if (oldWater != null) Object.DestroyImmediate(oldWater);

        // Also clean up any Unity Terrain objects
        Terrain[] terrains = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None);
        foreach (Terrain t in terrains) Object.DestroyImmediate(t.gameObject);

        // New random seed
        gen.seed = Random.Range(0, 99999);

        Debug.Log($"🐚 Island seed set to {gen.seed} — press Play to regenerate.");
    }

    [MenuItem("Coral Critter/Reset Player Position")]
    public static void ResetPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 20f, 0f);
            Debug.Log("🐚 Player reset to (0, 20, 0) — will fall to terrain on Play.");
        }
        else
        {
            Debug.LogWarning("No Player found. Run Build Prototype Scene first.");
        }
    }
}
