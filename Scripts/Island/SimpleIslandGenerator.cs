using UnityEngine;

/// <summary>
/// SimpleIslandGenerator — Procedural island tuned for playable isometric traversal.
/// </summary>
public class SimpleIslandGenerator : MonoBehaviour
{
    [Header("Island Size")]
    public int terrainSize = 1240;
    public float terrainHeight = 20f;
    public int resolution = 420;

    [Header("Generation")]
    public bool randomizeSeedOnStart = true;
    public int seed = 42;

    [Header("Terrain Style")]
    public float baseNoiseScale = 3.8f;
    public float hillNoiseScale = 7.2f;
    public float mountainNoiseScale = 13f;
    [Range(0f, 1f)] public float flatlandStrength = 0.62f;
    [Range(0f, 1f)] public float hillStrength = 0.48f;
    [Range(0f, 1f)] public float mountainStrength = 0.58f;
    [Range(0f, 1f)] public float ridgeStrength = 0.34f;
    [Range(0f, 0.9f)] public float heightCompression = 0.55f;


    [Header("Legacy Compatibility")]
    [Tooltip("Legacy field kept for scene builder compatibility; used as ridge frequency when > 0")]
    public float ridgeNoiseScale = 0f;
    [Tooltip("Legacy field kept for compatibility; remapped to flatland strength")]
    [Range(0f, 1f)] public float flatness = 0.62f;
    [Tooltip("Optional terracing. 0 = disabled")]
    [Range(0, 20)] public int terraceSteps = 0;

    [Header("Island Shape")]
    [Range(1.4f, 5f)] public float islandFalloff = 2.8f;
    [Range(0.55f, 0.95f)] public float oceanStart = 0.84f;
    [Range(0.05f, 0.4f)] public float oceanDepth = 0.2f;

    [Header("Water")]
    [Range(0f, 0.45f)] public float waterLevel = 0.06f;
    public float waterPlaneYOffset = 0f;
    [Range(0f, 1f)] public float inlandWaterStrength = 0.2f;
    [Range(0f, 0.35f)] public float inlandLandLift = 0.1f;
    public Color waterColor = new Color(0.08f, 0.35f, 0.6f, 0.75f);

    [Header("Smoothing")]
    [Range(0f, 1f)] public float smoothingStrength = 0.48f;
    [Range(0, 6)] public int smoothingIterations = 3;

    [Header("Terrain Colors (auto-painted by slope)")]
    public Color grassColor = new Color(0.22f, 0.45f, 0.12f);
    public Color dirtColor = new Color(0.4f, 0.3f, 0.15f);
    public Color rockColor = new Color(0.35f, 0.33f, 0.3f);
    public Color sandColor = new Color(0.65f, 0.55f, 0.35f);

    private GameObject islandObj;
    private GameObject waterObj;
    private float[,] heightmap;

    void Start()
    {
        if (randomizeSeedOnStart)
            seed = Random.Range(int.MinValue, int.MaxValue);

        Debug.Log("🐚 IslandGenerator: Starting generation...");
        try
        {
            GenerateIsland();
            CreateWaterPlane();
            SpawnLandmarks();
            PositionPlayerAboveTerrain();
            Debug.Log("🐚 IslandGenerator: Complete!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"🐚 IslandGenerator: FAILED — {e.Message}\n{e.StackTrace}");
            CreateFallbackGround();
        }
    }

    public void GenerateIsland()
    {
        if (islandObj != null) Destroy(islandObj);

        heightmap = GenerateHeightmap();
        Mesh mesh = BuildMeshFromHeightmap(heightmap);

        islandObj = new GameObject("Island");
        islandObj.transform.position = Vector3.zero;

        MeshFilter mf = islandObj.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        MeshRenderer mr = islandObj.AddComponent<MeshRenderer>();
        mr.material = CreateVertexColorMaterial();

        MeshCollider mc = islandObj.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        Debug.Log($"🐚 Island: {mesh.vertexCount} verts, {mesh.triangles.Length / 3} tris, seed={seed}");
    }

    float[,] GenerateHeightmap()
    {
        int size = resolution + 1;
        float[,] heights = new float[size, size];

        Random.InitState(seed);
        Vector2 oBase = new Vector2(Random.value * 10000f, Random.value * 10000f);
        Vector2 oHill = new Vector2(Random.value * 10000f, Random.value * 10000f);
        Vector2 oMountain = new Vector2(Random.value * 10000f, Random.value * 10000f);
        Vector2 oRidge = new Vector2(Random.value * 10000f, Random.value * 10000f);
        Vector2 oRiver = new Vector2(Random.value * 10000f, Random.value * 10000f);

        Vector2[] lakeCenters = new Vector2[2];
        for (int i = 0; i < lakeCenters.Length; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.14f, 0.42f);
            lakeCenters[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        int riverCount = Random.Range(1, 3);
        Vector2[] riverStart = new Vector2[riverCount];
        Vector2[] riverEnd = new Vector2[riverCount];
        for (int i = 0; i < riverCount; i++)
        {
            float a0 = Random.Range(0f, Mathf.PI * 2f);
            float a1 = a0 + Random.Range(-0.8f, 0.8f);
            riverStart[i] = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * Random.Range(0.18f, 0.35f);
            riverEnd[i] = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * Random.Range(0.78f, 0.92f);
        }

        // Guaranteed macro landforms: one major mountain + two notable hills.
        Vector2 mainMountainCenter = Random.insideUnitCircle.normalized * Random.Range(0.26f, 0.48f);
        float mainMountainRadius = Random.Range(0.18f, 0.26f);

        Vector2[] hillCenters = new Vector2[2];
        float[] hillRadii = new float[2];
        for (int i = 0; i < hillCenters.Length; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.14f, 0.38f);
            hillCenters[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            hillRadii[i] = Random.Range(0.10f, 0.17f);
        }

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float nx = (float)x / resolution;
                float nz = (float)z / resolution;
                Vector2 p = new Vector2(nx - 0.5f, nz - 0.5f);

                float dist = p.magnitude * 2f;
                float islandMask = Mathf.Clamp01(1f - Mathf.Pow(dist, islandFalloff));
                islandMask = Mathf.SmoothStep(0f, 1f, islandMask);
                float oceanRing = Mathf.SmoothStep(oceanStart, 1.02f, dist);

                float baseShape = Fbm(nx, nz, baseNoiseScale, 4, 0.5f, oBase);
                float hills = Fbm(nx, nz, hillNoiseScale, 3, 0.5f, oHill);
                float mountains = Fbm(nx, nz, mountainNoiseScale, 3, 0.55f, oMountain);
                float ridgeFrequency = ridgeNoiseScale > 0.01f ? ridgeNoiseScale : mountainNoiseScale * 0.72f;
                float ridges = Ridged(nx, nz, ridgeFrequency, oRidge);

                float flatMask = 1f - Mathf.SmoothStep(0.28f, 0.62f, dist);
                float mountainRingMask = Mathf.SmoothStep(0.42f, 0.8f, dist) * (1f - Mathf.SmoothStep(0.86f, 1.03f, dist));

                float runtimeFlatness = Mathf.Clamp01((flatlandStrength + flatness) * 0.5f);
                float plains = Mathf.Lerp(baseShape, Mathf.Pow(baseShape, 1.65f), runtimeFlatness);
                float hillMix = hills * hillStrength;
                float mountainMix = (mountains * 0.85f + ridges * ridgeStrength) * mountainStrength * mountainRingMask;

                float bigMountain = 1f - Mathf.SmoothStep(mainMountainRadius * 0.35f, mainMountainRadius, Vector2.Distance(p, mainMountainCenter));
                bigMountain = Mathf.Pow(Mathf.Clamp01(bigMountain), 1.35f) * 0.62f;

                float hillsMacro = 0f;
                for (int i = 0; i < hillCenters.Length; i++)
                {
                    float hill = 1f - Mathf.SmoothStep(hillRadii[i] * 0.45f, hillRadii[i], Vector2.Distance(p, hillCenters[i]));
                    hillsMacro = Mathf.Max(hillsMacro, hill * 0.22f);
                }

                float height01 = plains;
                height01 = Mathf.Lerp(height01, height01 * 0.78f + hillMix * 0.55f, 1f - flatMask * 0.92f);
                height01 += mountainMix + hillsMacro + bigMountain * mountainRingMask;

                // Preserve broad flat buildable interior areas.
                float centerPlateau = Mathf.SmoothStep(0f, 0.45f, flatMask) * runtimeFlatness;
                height01 = Mathf.Lerp(height01, Mathf.Max(height01, waterLevel + inlandLandLift * 1.2f), centerPlateau * 0.85f);

                // Height compression keeps slopes traversable while retaining variation.
                height01 = Mathf.Clamp01(Mathf.Lerp(height01, Mathf.Sqrt(Mathf.Clamp01(height01)), heightCompression));

                if (terraceSteps > 0)
                {
                    float t = height01 * terraceSteps;
                    float stepped = Mathf.Floor(t) / terraceSteps;
                    height01 = Mathf.Lerp(height01, stepped, 0.32f);
                }

                height01 *= islandMask;

                // Carve 1-2 rivers that flow outward.
                float riverMask = 0f;
                for (int i = 0; i < riverCount; i++)
                {
                    float d = DistanceToSegment(p, riverStart[i], riverEnd[i]);
                    float width = Mathf.Lerp(0.012f, 0.03f, Mathf.InverseLerp(0.2f, 0.95f, dist));
                    float riverCore = 1f - Mathf.SmoothStep(width, width * 2.6f, d);
                    riverMask = Mathf.Max(riverMask, riverCore);
                }

                float riverNoise = Mathf.PerlinNoise(oRiver.x + nx * 12f, oRiver.y + nz * 12f);
                riverMask *= Mathf.SmoothStep(0.2f, 0.9f, dist) * Mathf.Lerp(0.8f, 1.2f, riverNoise);

                // Add two small lakes.
                float lakeMask = 0f;
                for (int i = 0; i < lakeCenters.Length; i++)
                {
                    float ld = Vector2.Distance(p, lakeCenters[i]);
                    lakeMask = Mathf.Max(lakeMask, 1f - Mathf.SmoothStep(0.05f, 0.16f, ld));
                }

                float inlandMask = Mathf.Clamp01((lakeMask + riverMask) * inlandWaterStrength);
                float inlandAllowed = Mathf.SmoothStep(0.16f, 0.82f, islandMask) * (1f - oceanRing);
                float inlandWaterTarget = waterLevel + 0.01f;
                height01 = Mathf.Lerp(height01, inlandWaterTarget, inlandMask * inlandAllowed);

                // Keep interior above water so the map is never "all water".
                float interiorSafety = Mathf.SmoothStep(0.2f, 0.95f, islandMask) * (1f - oceanRing);
                float interiorMin = waterLevel + inlandLandLift;
                height01 = Mathf.Max(height01, Mathf.Lerp(waterLevel, interiorMin, interiorSafety));

                // Force ocean around outside edge.
                float oceanTarget = Mathf.Max(0f, waterLevel - oceanDepth);
                height01 = Mathf.Lerp(height01, oceanTarget, oceanRing);

                heights[z, x] = height01 * terrainHeight;
            }
        }

        return SmoothHeightmap(heights);
    }

    float Fbm(float x, float z, float scale, int octaves, float persistence, Vector2 offset)
    {
        float value = 0f;
        float amp = 1f;
        float freq = scale;
        float totalAmp = 0f;

        for (int i = 0; i < octaves; i++)
        {
            value += Mathf.PerlinNoise(offset.x + x * freq, offset.y + z * freq) * amp;
            totalAmp += amp;
            amp *= persistence;
            freq *= 2f;
        }

        return totalAmp > 0f ? value / totalAmp : 0f;
    }

    float Ridged(float x, float z, float scale, Vector2 offset)
    {
        float n = Mathf.PerlinNoise(offset.x + x * scale, offset.y + z * scale);
        n = 1f - Mathf.Abs(n * 2f - 1f);
        return n * n;
    }

    float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denom = ab.sqrMagnitude;
        if (denom < 0.0001f) return Vector2.Distance(p, a);

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denom);
        Vector2 proj = a + ab * t;
        return Vector2.Distance(p, proj);
    }

    float[,] SmoothHeightmap(float[,] source)
    {
        if (smoothingStrength <= 0f || smoothingIterations <= 0) return source;

        int size = resolution + 1;
        float[,] current = source;

        for (int pass = 0; pass < smoothingIterations; pass++)
        {
            float[,] next = new float[size, size];

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sum = 0f;
                    int count = 0;

                    for (int dz = -1; dz <= 1; dz++)
                    {
                        int sz = z + dz;
                        if (sz < 0 || sz >= size) continue;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int sx = x + dx;
                            if (sx < 0 || sx >= size) continue;

                            sum += current[sz, sx];
                            count++;
                        }
                    }

                    float average = sum / count;
                    next[z, x] = Mathf.Lerp(current[z, x], average, smoothingStrength);
                }
            }

            current = next;
        }

        return current;
    }

    Mesh BuildMeshFromHeightmap(float[,] heights)
    {
        int vertCountX = resolution + 1;
        int vertCountZ = resolution + 1;
        int totalVerts = vertCountX * vertCountZ;

        Vector3[] vertices = new Vector3[totalVerts];
        Vector2[] uvs = new Vector2[totalVerts];
        int[] triangles = new int[resolution * resolution * 6];

        float halfSize = terrainSize / 2f;
        float stepX = (float)terrainSize / resolution;
        float stepZ = (float)terrainSize / resolution;

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                int i = z * vertCountX + x;
                float worldX = x * stepX - halfSize;
                float worldZ = z * stepZ - halfSize;
                vertices[i] = new Vector3(worldX, heights[z, x], worldZ);
                uvs[i] = new Vector2((float)x / resolution, (float)z / resolution);
            }
        }

        int tri = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int bl = z * vertCountX + x;
                int br = bl + 1;
                int tl = (z + 1) * vertCountX + x;
                int tr = tl + 1;

                triangles[tri++] = bl; triangles[tri++] = tl; triangles[tri++] = tr;
                triangles[tri++] = bl; triangles[tri++] = tr; triangles[tri++] = br;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "IslandMesh";
        if (totalVerts > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Vector3[] normals = mesh.normals;
        Color[] colors = new Color[totalVerts];
        float waterWorldH = waterLevel * terrainHeight + waterPlaneYOffset;

        for (int i = 0; i < totalVerts; i++)
        {
            float height = vertices[i].y;
            float slopeAngle = Vector3.Angle(normals[i], Vector3.up);

            Color vertColor;
            if (height < waterWorldH + 0.6f)
            {
                vertColor = sandColor;
            }
            else if (slopeAngle > 52f)
            {
                vertColor = rockColor;
            }
            else if (slopeAngle > 30f)
            {
                float t = (slopeAngle - 30f) / 22f;
                vertColor = Color.Lerp(dirtColor, rockColor, t);
            }
            else
            {
                float t = slopeAngle / 30f;
                vertColor = Color.Lerp(grassColor, dirtColor, t);
            }

            float heightFade = Mathf.InverseLerp(0f, terrainHeight * 0.85f, height);
            vertColor = Color.Lerp(vertColor * 0.86f, vertColor, heightFade);
            colors[i] = vertColor;
        }

        mesh.colors = colors;
        return mesh;
    }

    void CreateWaterPlane()
    {
        if (waterObj != null) Destroy(waterObj);

        waterObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObj.name = "WaterPlane";

        float waterWorldHeight = waterLevel * terrainHeight + waterPlaneYOffset;
        waterObj.transform.position = new Vector3(0f, waterWorldHeight, 0f);

        float planeScale = terrainSize / 10f * 1.6f;
        waterObj.transform.localScale = new Vector3(planeScale, 1f, planeScale);

        Renderer rend = waterObj.GetComponent<Renderer>();
        rend.material = CreateSimpleMaterial(waterColor);
        Destroy(waterObj.GetComponent<Collider>());

        Debug.Log($"🐚 Water at height {waterWorldHeight}");
    }

    void PositionPlayerAboveTerrain()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float bestX = 0f, bestZ = 0f;
            float bestHeight = SampleHeight(0f, 0f);
            float minH = waterLevel * terrainHeight + waterPlaneYOffset + 1.8f;

            for (int i = 0; i < 24; i++)
            {
                float tx = Random.Range(-24f, 24f);
                float tz = Random.Range(-24f, 24f);
                float h = SampleHeight(tx, tz);
                if (h > minH && h > bestHeight)
                {
                    bestX = tx;
                    bestZ = tz;
                    bestHeight = h;
                }
            }

            Vector3 spawnPos = new Vector3(bestX, bestHeight + 2.5f, bestZ);
            player.transform.position = spawnPos;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log($"🐚 Player at {spawnPos}");
        }
    }

    void SpawnLandmarks()
    {
        GameObject parent = new GameObject("Landmarks");
        Color[] colors = {
            new Color(0.9f, 0.3f, 0.2f), new Color(0.2f, 0.4f, 0.9f),
            new Color(0.9f, 0.8f, 0.1f), new Color(0.8f, 0.2f, 0.8f),
            new Color(1f, 0.5f, 0.1f),   new Color(0.1f, 0.8f, 0.3f),
        };

        Random.InitState(seed + 999);
        float minH = waterLevel * terrainHeight + waterPlaneYOffset + 1f;
        int placed = 0;

        for (int attempt = 0; attempt < 60 && placed < 10; attempt++)
        {
            float range = terrainSize * 0.35f;
            float x = Random.Range(-range, range);
            float z = Random.Range(-range, range);
            float y = SampleHeight(x, z);

            if (y < minH) continue;

            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = $"Landmark_{placed}";
            pillar.transform.parent = parent.transform;
            float pillarH = Random.Range(3f, 7f);
            pillar.transform.localScale = new Vector3(1.2f, pillarH, 1.2f);
            pillar.transform.position = new Vector3(x, y + pillarH / 2f, z);
            pillar.GetComponent<Renderer>().material = CreateSimpleMaterial(colors[placed % colors.Length]);
            placed++;
        }

        float cy = SampleHeight(0f, 0f);
        if (cy > minH)
        {
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            beacon.name = "CenterBeacon";
            beacon.transform.parent = parent.transform;
            beacon.transform.localScale = new Vector3(2f, 10f, 2f);
            beacon.transform.position = new Vector3(0f, cy + 5f, 0f);
            beacon.GetComponent<Renderer>().material = CreateSimpleMaterial(Color.white);
        }

        Debug.Log($"🐚 Placed {placed} landmarks");
    }

    public float SampleHeight(float worldX, float worldZ)
    {
        if (heightmap == null) return 0f;
        float halfSize = terrainSize / 2f;
        float hmX = ((worldX + halfSize) / terrainSize) * resolution;
        float hmZ = ((worldZ + halfSize) / terrainSize) * resolution;

        int x0 = Mathf.Clamp(Mathf.FloorToInt(hmX), 0, resolution - 1);
        int z0 = Mathf.Clamp(Mathf.FloorToInt(hmZ), 0, resolution - 1);
        int x1 = Mathf.Min(x0 + 1, resolution);
        int z1 = Mathf.Min(z0 + 1, resolution);

        float tx = hmX - x0;
        float tz = hmZ - z0;

        return Mathf.Lerp(
            Mathf.Lerp(heightmap[z0, x0], heightmap[z0, x1], tx),
            Mathf.Lerp(heightmap[z1, x0], heightmap[z1, x1], tx),
            tz
        );
    }

    Material CreateVertexColorMaterial()
    {
        string[] shaderNames = {
            "Particles/Standard Unlit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Lit",
            "Standard",
            "Diffuse"
        };

        Shader shader = null;
        foreach (string sn in shaderNames)
        {
            shader = Shader.Find(sn);
            if (shader != null) break;
        }

        if (shader == null) return CreateSimpleMaterial(grassColor);

        Material mat = new Material(shader);
        mat.name = "TerrainVertexColor";
        mat.color = Color.white;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        return mat;
    }

    Material CreateSimpleMaterial(Color color)
    {
        string[] shaderNames = {
            "Universal Render Pipeline/Lit", "Standard", "Diffuse", "Sprites/Default"
        };
        Shader shader = null;
        foreach (string sn in shaderNames)
        {
            shader = Shader.Find(sn);
            if (shader != null) break;
        }
        if (shader == null) return new Material(Shader.Find("Hidden/InternalErrorShader"));

        Material mat = new Material(shader);
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        return mat;
    }

    void CreateFallbackGround()
    {
        Debug.LogWarning("🐚 Using fallback flat ground.");
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Island";
        ground.transform.localScale = new Vector3(terrainSize / 10f, 1f, terrainSize / 10f);
        ground.GetComponent<Renderer>().material = CreateSimpleMaterial(grassColor);

        GameObject player = GameObject.Find("Player");
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.transform.position = new Vector3(0f, 2f, 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.up * terrainHeight * 0.5f,
            new Vector3(terrainSize, terrainHeight, terrainSize));
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(Vector3.up * (waterLevel * terrainHeight + waterPlaneYOffset),
            new Vector3(terrainSize, 0.1f, terrainSize));
    }
}
