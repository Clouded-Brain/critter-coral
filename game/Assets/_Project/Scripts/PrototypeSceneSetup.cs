using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [Header("Island")]
        [SerializeField] private int seed = 1337;
        [SerializeField] private float spawnHeightProbe = 80f;
        [SerializeField] private float spawnSurfacePadding = 0.2f;

        [ContextMenu("Create/Refresh Test Setup")]
        public void CreateOrRefreshSetup()
        {
            EnsureIslandGenerator();
            var player = EnsurePlayer();
            MovePlayerToSurface(player);

            EnsureCamera(player.transform);
        }

        private GameObject EnsurePlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
            }

            player.tag = "Player";
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

            var controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = player.AddComponent<CharacterController>();
            }

            controller.height = playerHeight;
            controller.radius = playerRadius;
            controller.center = new Vector3(0f, playerHeight * 0.5f, 0f);

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
                camera.tag = "MainCamera";
            }

            var iso = camera.GetComponent<IsometricCameraController>();
            if (iso == null)
            {
                iso = camera.gameObject.AddComponent<IsometricCameraController>();
            }

            iso.SetTarget(player);
#if UNITY_EDITOR
            EditorUtility.SetDirty(iso);
#endif

            if (camera.GetComponent<CameraOcclusionController>() == null)
            {
                camera.gameObject.AddComponent<CameraOcclusionController>();
            }

            return camera;
        }

        private IslandSeedPresetGenerator EnsureIslandGenerator()
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
#if UNITY_EDITOR
            EditorUtility.SetDirty(generator);
#endif
            return generator;
        }

        private void MovePlayerToSurface(GameObject player)
        {
            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                return;
            }

            var probeStart = new Vector3(playerSpawn.x, spawnHeightProbe, playerSpawn.z);
            if (Physics.Raycast(probeStart, Vector3.down, out var hit, spawnHeightProbe * 2f))
            {
                var groundedY = hit.point.y + characterController.height * 0.5f + spawnSurfacePadding;
                player.transform.position = new Vector3(playerSpawn.x, groundedY, playerSpawn.z);
                return;
            }

            player.transform.position = playerSpawn;
        }
    }
}
