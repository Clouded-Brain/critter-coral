using System.Collections.Generic;
using UnityEngine;

namespace CoralCritter
{
    /// <summary>
    /// CC-003: Deterministic island generator using a fixed seed and layered perlin noise.
    /// Produces a quick prototype mesh with shoreline and elevation tiers.
    /// </summary>
    public class IslandSeedPresetGenerator : MonoBehaviour
    {
        [Header("Seed")]
        [SerializeField] private int islandSeed = 1337;

        [Header("Grid")]
        [SerializeField, Min(8)] private int resolution = 64;
        [SerializeField, Min(1f)] private float worldSize = 60f;
        [SerializeField, Min(0.1f)] private float maxHeight = 12f;

        [Header("Noise")]
        [SerializeField, Min(0.001f)] private float baseFrequency = 0.05f;
        [SerializeField, Min(1)] private int octaves = 3;
        [SerializeField, Min(0f)] private float lacunarity = 2f;
        [SerializeField, Range(0f, 1f)] private float persistence = 0.5f;

        [Header("Island Falloff")]
        [SerializeField, Range(0.05f, 2f)] private float islandRadius = 0.8f;
        [SerializeField, Min(0f)] private float shorelineHeight = 1.4f;

        [Header("Materials")]
        [SerializeField] private Material terrainMaterial;
        [SerializeField] private Material waterMaterial;

        private readonly List<GameObject> _spawnedObjects = new();

        private void Start()
        {
            Generate();
        }

        [ContextMenu("Generate")]
        public void Generate()
        {
            Clear();

            var terrainObject = new GameObject("GeneratedIsland");
            terrainObject.transform.SetParent(transform, false);
            _spawnedObjects.Add(terrainObject);

            var meshFilter = terrainObject.AddComponent<MeshFilter>();
            var meshRenderer = terrainObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = terrainMaterial;

            meshFilter.sharedMesh = BuildMesh();

            var waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterObject.name = "WaterPlane";
            waterObject.transform.SetParent(transform, false);
            waterObject.transform.localScale = Vector3.one * (worldSize / 10f);
            waterObject.transform.localPosition = new Vector3(0f, shorelineHeight, 0f);
            waterObject.GetComponent<Renderer>().sharedMaterial = waterMaterial;
            _spawnedObjects.Add(waterObject);
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            foreach (var obj in _spawnedObjects)
            {
                if (obj == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(obj);
                }
                else
#endif
                {
                    Destroy(obj);
                }
            }

            _spawnedObjects.Clear();
        }

        private Mesh BuildMesh()
        {
            var vertCount = (resolution + 1) * (resolution + 1);
            var vertices = new Vector3[vertCount];
            var uvs = new Vector2[vertCount];
            var triangles = new int[resolution * resolution * 6];

            var random = new System.Random(islandSeed);
            var offsetX = random.Next(-100_000, 100_000);
            var offsetY = random.Next(-100_000, 100_000);

            var halfSize = worldSize * 0.5f;
            var step = worldSize / resolution;

            var vertIndex = 0;
            for (var z = 0; z <= resolution; z++)
            {
                for (var x = 0; x <= resolution; x++)
                {
                    var worldX = -halfSize + x * step;
                    var worldZ = -halfSize + z * step;

                    var normalizedX = x / (float)resolution;
                    var normalizedZ = z / (float)resolution;

                    var height = SampleHeight(worldX, worldZ, offsetX, offsetY);
                    vertices[vertIndex] = new Vector3(worldX, height, worldZ);
                    uvs[vertIndex] = new Vector2(normalizedX, normalizedZ);
                    vertIndex++;
                }
            }

            var triIndex = 0;
            for (var z = 0; z < resolution; z++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var i = z * (resolution + 1) + x;

                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + resolution + 1;
                    triangles[triIndex++] = i + 1;

                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + resolution + 1;
                    triangles[triIndex++] = i + resolution + 2;
                }
            }

            var mesh = new Mesh { name = $"Island_{islandSeed}" };
            mesh.indexFormat = vertCount > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private float SampleHeight(float worldX, float worldZ, float offsetX, float offsetY)
        {
            var amplitude = 1f;
            var frequency = baseFrequency;
            var noiseValue = 0f;
            var maxPossible = 0f;

            for (var i = 0; i < octaves; i++)
            {
                var sampleX = (worldX + offsetX) * frequency;
                var sampleY = (worldZ + offsetY) * frequency;
                var perlin = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                noiseValue += perlin * amplitude;
                maxPossible += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            var normalizedNoise = maxPossible > 0f
                ? (noiseValue / maxPossible + 1f) * 0.5f
                : 0f;

            var radialDistance = new Vector2(worldX, worldZ).magnitude / (worldSize * 0.5f * islandRadius);
            var falloff = Mathf.Clamp01(1f - radialDistance * radialDistance);

            return normalizedNoise * falloff * maxHeight;
        }
    }
}
