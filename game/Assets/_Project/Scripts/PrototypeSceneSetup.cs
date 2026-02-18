using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// One-click editor/runtime setup for a playable IslandPrototype test scene.
    /// Creates player, camera rig, and seeded island generator when missing.
    /// </summary>
    public class PrototypeSceneSetup : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Vector3 playerSpawn = new(0f, 4f, 0f);
        [SerializeField] private float playerHeight = 2f;
        [SerializeField] private float playerRadius = 0.4f;

        [Header("Camera")]
        [SerializeField] private Vector3 cameraStartPosition = new(0f, 22f, -18f);

        [Header("Island")]
        [SerializeField] private int seed = 1337;

        [ContextMenu("Create/Refresh Test Setup")]
        public void CreateOrRefreshSetup()
        {
            var player = EnsurePlayer();
            var camera = EnsureCamera(player.transform);
            EnsureIslandGenerator();

            if (camera != null)
            {
                camera.transform.position = cameraStartPosition;
            }
        }

        private GameObject EnsurePlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
            }

            player.transform.position = playerSpawn;

            var collider = player.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(collider);
                }
                else
#endif
                {
                    Destroy(collider);
                }
            }

            if (player.GetComponent<CharacterController>() == null)
            {
                var controller = player.AddComponent<CharacterController>();
                controller.height = playerHeight;
                controller.radius = playerRadius;
                controller.center = new Vector3(0f, playerHeight * 0.5f, 0f);
            }

            if (player.GetComponent<PlayerMovementController>() == null)
            {
                player.AddComponent<PlayerMovementController>();
            }

            return player;
        }

        private Camera EnsureCamera(Transform player)
        {
            var existingCamera = FindFirstObjectByType<Camera>();
            Camera camera;
            if (existingCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                camera.tag = "MainCamera";
            }
            else
            {
                camera = existingCamera;
                camera.gameObject.name = "Main Camera";
            }

            var iso = camera.GetComponent<IsometricCameraController>();
            if (iso == null)
            {
                iso = camera.gameObject.AddComponent<IsometricCameraController>();
            }

            iso.SetTarget(player);

            if (camera.GetComponent<CameraOcclusionController>() == null)
            {
                camera.gameObject.AddComponent<CameraOcclusionController>();
            }

            return camera;
        }

        private void EnsureIslandGenerator()
        {
            var islandObject = GameObject.Find("IslandGenerator");
            if (islandObject == null)
            {
                islandObject = new GameObject("IslandGenerator");
                islandObject.transform.position = Vector3.zero;
            }

            var generator = islandObject.GetComponent<IslandSeedPresetGenerator>();
            if (generator == null)
            {
                generator = islandObject.AddComponent<IslandSeedPresetGenerator>();
            }

            generator.SetSeed(seed);
            generator.Generate();
        }
    }
}
