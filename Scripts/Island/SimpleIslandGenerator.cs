using UnityEngine;

/// <summary>
/// SimpleIslandGenerator — Procedural island with Valheim-inspired terrain.
/// 
/// Features flat meadows, dramatic ridges, rocky cliffs, and coastal beaches.
/// Uses vertex colors to paint grass/dirt/rock based on slope angle.
/// </summary>
public class SimpleIslandGenerator : MonoBehaviour
{
    [Header("Island Size")]
    public int terrainSize = 240;
    public float terrainHeight = 12f;
    public int resolution = 200;

    [Header("Generation")]
    [Tooltip("Generates a different island each play run while still allowing reproducible seeds when disabled")]
    public bool randomizeSeedOnStart = true;

    [Header("Terrain Style")]
    [Tooltip("Large rolling hills scale")]
    public float baseNoiseScale = 8f;

    [Tooltip("Ridge/cliff frequency")]
    public float ridgeNoiseScale = 15f;

    [Tooltip("Fine detail bumps")]
    public float detailNoiseScale = 40f;

    [Tooltip("Controls how large flat-vs-hilly regions are")]
    public float biomeRegionScale = 2.4f;

    [Tooltip("Threshold for hilly regions (higher = more flats)")]
    [Range(0.35f, 0.75f)]
    public float hillThreshold = 0.6f;

    [Tooltip("How much of the terrain is flat meadow vs hills (0=all hills, 1=all flat)")]
    [Range(0f, 0.8f)]
    public float flatness = 0.79f;

    [Tooltip("How sharp ridges/cliffs are (higher = more dramatic)")]
    [Range(0f, 1f)]
    public float ridgeStrength = 0.06f;

    [Tooltip("Number of terrace steps (0 = smooth, 6-12 = Valheim-like stepping)")]
    [Range(0, 20)]
    public int terraceSteps = 0;

    [Tooltip("Extra smoothing pass on generated heights")]
    [Range(0f, 1f)]
    public float smoothingStrength = 0.78f;

    [Tooltip("How many smoothing passes to run")]
    [Range(0, 6)]
    public int smoothingIterations = 5;

    [Tooltip("Compresses extreme height differences while preserving shape")]
    [Range(0f, 0.8f)]
    public float heightCompression = 0.75f;


    [Tooltip("Extra low-elevation shelf near the shore (as % of terrain height)")]
    [Range(0f, 0.25f)]
    public float coastShelfHeight = 0.08f;

    [Tooltip("How wide the gentle coastal shelf is")]
    [Range(0.05f, 0.45f)]
    public float coastShelfWidth = 0.34f;


    [Tooltip("How far down terrain sinks near map edge to guarantee ocean")]
    [Range(0.02f, 0.3f)]
    public float oceanDepth = 0.12f;

    [Tooltip("Distance from center where ocean edge starts")]
    [Range(0.6f, 1f)]
    public float oceanStart = 0.84f;

    [Tooltip("Density of inland lakes/streams")]
    [Range(0f, 1f)]
    public float inlandWaterStrength = 0.08f;

    [Tooltip("Minimum interior land lift above water level")]
    [Range(0f, 0.35f)]
    public float inlandLandLift = 0.14f;

    [Header("Island Shape")]
    [Range(1f, 5f)]
    public float islandFalloff = 2.5f;

    [Tooltip("Random seed")]
    public int seed = 42;

    [Header("Water")]
    [Range(0f, 0.5f)]
    public float waterLevel = 0.04f;

    [Tooltip("Additional world-space Y offset applied to the rendered water plane")]
    public float waterPlaneYOffset = -1.4f;
    public Color waterColor = new Color(0.08f, 0.35f, 0.6f, 0.75f);

    [Header("Terrain Colors (auto-painted by slope)")]
    public Color grassColor = new Color(0.22f, 0.45f, 0.12f);
    public Color dirtColor = new Color(0.4f, 0.3f, 0.15f);
    public Color rockColor = new Color(0.35f, 0.33f, 0.3f);
    public Color sandColor = new Color(0.65f, 0.55f, 0.35f);

    // Internal
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

    // ═══════════════════════════════════════════════════════
    // ISLAND GENERATION
    // ═══════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════
    // VALHEIM-STYLE HEIGHTMAP
    // ═══════════════════════════════════════════════════════

    float[,] GenerateHeightmap()
    {
        float[,] heights = new float[resolution + 1, resolution + 1];

        Random.InitState(seed);
        float ox1 = Random.Range(0f, 10000f), oz1 = Random.Range(0f, 10000f);
        float ox2 = Random.Range(0f, 10000f), oz2 = Random.Range(0f, 10000f);
        float ox3 = Random.Range(0f, 10000f), oz3 = Random.Range(0f, 10000f);
        float ox4 = Random.Range(0f, 10000f), oz4 = Random.Range(0f, 10000f);
        float ox5 = Random.Range(0f, 10000f), oz5 = Random.Range(0f, 10000f);
        float ox6 = Random.Range(0f, 10000f), oz6 = Random.Range(0f, 10000f);

        Vector2[] lakeCenters = new Vector2[3];
        for (int i = 0; i < lakeCenters.Length; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.16f, 0.42f);
            lakeCenters[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float nx = (float)x / resolution;
                float nz = (float)z / resolution;

                // ── 1. BASE TERRAIN — broad rolling hills ──
                float baseNoise = 0f;
                baseNoise += 1.0f * Mathf.PerlinNoise(ox1 + nx * baseNoiseScale, oz1 + nz * baseNoiseScale);
                baseNoise += 0.5f * Mathf.PerlinNoise(ox1 + nx * baseNoiseScale * 2f, oz1 + nz * baseNoiseScale * 2f);
                baseNoise += 0.25f * Mathf.PerlinNoise(ox1 + nx * baseNoiseScale * 4f, oz1 + nz * baseNoiseScale * 4f);
                baseNoise /= 1.75f;

                // Create broad regions of meadow vs hill country.
                float biomeRegion = Mathf.PerlinNoise(ox4 + nx * biomeRegionScale, oz4 + nz * biomeRegionScale);
                float hillMask = Mathf.SmoothStep(hillThreshold - 0.14f, hillThreshold + 0.14f, biomeRegion);

                float distCenter = Mathf.Sqrt((nx - 0.5f) * (nx - 0.5f) + (nz - 0.5f) * (nz - 0.5f)) * 2f;
                float outerHillBias = Mathf.SmoothStep(0.4f, 0.88f, distCenter);
                hillMask = Mathf.Clamp01(Mathf.Lerp(hillMask, 1f, outerHillBias * 0.45f));

                // Encourage mountain bands near outer third while keeping center flatter/playable.
                float mountainRing = Mathf.SmoothStep(0.52f, 0.78f, distCenter) * (1f - Mathf.SmoothStep(0.9f, 1.05f, distCenter));

                // Apply flatness stronger in meadows, lighter in hilly zones.
                float meadowBase = Mathf.Pow(baseNoise, 1.9f + flatness * 3.6f);
                float hillBase = Mathf.Pow(baseNoise, 1.2f + flatness * 1.35f);
                baseNoise = Mathf.Lerp(meadowBase, hillBase, hillMask);

                // ── 2. RIDGE NOISE — creates cliff edges and ridgelines ──
                float ridgeRaw = Mathf.PerlinNoise(ox2 + nx * ridgeNoiseScale, oz2 + nz * ridgeNoiseScale);
                float ridge = 1f - Mathf.Abs(ridgeRaw - 0.5f) * 2f;
                ridge = Mathf.Pow(ridge, 3f);
                ridge *= ridgeStrength * Mathf.Lerp(0.02f, 0.38f, hillMask + mountainRing * 0.55f);

                // ── 3. DETAIL NOISE — small bumps and roughness ──
                float detail = Mathf.PerlinNoise(ox3 + nx * detailNoiseScale, oz3 + nz * detailNoiseScale);
                detail = (detail - 0.5f) * Mathf.Lerp(0.001f, 0.008f, hillMask);

                // ── 4. BIOME VARIATION — occasional raised hill groups ──
                float plateauNoise = Mathf.PerlinNoise(ox5 + nx * 3f, oz5 + nz * 3f);
                float biomeBoost = Mathf.Max(0f, plateauNoise - 0.72f) * 0.16f * (hillMask + mountainRing * 0.4f);

                // ── 5. COMBINE ──
                float combined = baseNoise + ridge + detail + biomeBoost;
                combined = Mathf.Clamp01(Mathf.InverseLerp(0.08f, 1.05f, combined));
                combined = Mathf.Lerp(combined, Mathf.SmoothStep(0f, 1f, combined), 0.88f);

                // ── 6. TERRACING — Valheim-like stepped terrain ──
                if (terraceSteps > 0)
                {
                    float t = combined * terraceSteps;
                    float stepped = Mathf.Floor(t) / terraceSteps;
                    float smooth = t / terraceSteps;
                    combined = Mathf.Lerp(smooth, stepped, 0.08f);
                }

                // ── 7. ISLAND MASK — circular falloff ──
                float dx = nx - 0.5f;
                float dz = nz - 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dz * dz) * 2f;
                float maskCore = Mathf.Clamp01(1f - Mathf.Pow(dist, islandFalloff));
                float mask = Mathf.SmoothStep(-0.05f, 1f, maskCore);

                float oceanEdge = Mathf.SmoothStep(oceanStart, 1f, dist);

                float compressed = Mathf.Lerp(combined, Mathf.Sqrt(combined), heightCompression);
                float height01 = compressed * mask;

                float coastBlend = Mathf.SmoothStep(0f, coastShelfWidth, mask);
                float coastalFloor = waterLevel + coastShelfHeight;
                float coastHeight = Mathf.Max(height01, coastalFloor);
                height01 = Mathf.Lerp(coastHeight, height01, coastBlend);

                // Additional near-shore easing to reduce steep declines into water.
                float shoreEase = Mathf.SmoothStep(0f, coastShelfWidth * 1.35f, mask);
                height01 = Mathf.Lerp(Mathf.Max(height01, waterLevel + coastShelfHeight * 0.7f), height01, shoreEase);

                // Keep interior terrain safely above sea level (prevents all-water worlds).
                float interiorMask = Mathf.SmoothStep(0.12f, 0.95f, mask) * (1f - oceanEdge);
                float interiorFloor = waterLevel + inlandLandLift;
                height01 = Mathf.Max(height01, Mathf.Lerp(waterLevel, interiorFloor, interiorMask));

                // Force true island ocean ring around edges.
                height01 = Mathf.Lerp(height01, waterLevel - oceanDepth, oceanEdge);

                // Add sparse inland lakes / stream-like depressions.
                float channel = Mathf.PerlinNoise(ox6 + nx * 9f, oz6 + nz * 9f);
                float streamBand = 1f - Mathf.SmoothStep(0f, 0.035f, Mathf.Abs(channel - 0.5f));
                float streamMask = streamBand * 0.35f;

                float lakeMask = 0f;
                for (int i = 0; i < lakeCenters.Length; i++)
                {
                    float lx = nx - 0.5f - lakeCenters[i].x;
                    float lz = nz - 0.5f - lakeCenters[i].y;
                    float lakeDist = Mathf.Sqrt(lx * lx + lz * lz);
                    lakeMask = Mathf.Max(lakeMask, 1f - Mathf.SmoothStep(0.045f, 0.11f, lakeDist));
                }

                float inlandMask = Mathf.Clamp01((streamMask + lakeMask) * inlandWaterStrength);
                float inlandAllowed = Mathf.SmoothStep(0.35f, 0.9f, mask) * (1f - oceanEdge);
                float inlandWaterLevel = waterLevel + 0.004f;
                height01 = Mathf.Lerp(height01, inlandWaterLevel, inlandMask * inlandAllowed);

                heights[z, x] = height01 * terrainHeight;
            }
        }

        float[,] smoothed = SmoothHeightmap(heights);
        return EnsureIslandBalance(smoothed);
    }

    float[,] EnsureIslandBalance(float[,] heights)
    {
        int size = resolution + 1;
        float safeInterior = (waterLevel + inlandLandLift * 0.92f) * terrainHeight;
        float oceanTarget = (waterLevel - oceanDepth) * terrainHeight;

        for (int z = 0; z < size; z++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / resolution;
                float nz = (float)z / resolution;
                float dx = nx - 0.5f;
                float dz = nz - 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dz * dz) * 2f;

                float interior = 1f - Mathf.SmoothStep(0.68f, 0.98f, dist);
                float oceanRing = Mathf.SmoothStep(oceanStart, 1.06f, dist);

                heights[z, x] = Mathf.Max(heights[z, x], safeInterior * interior);
                heights[z, x] = Mathf.Lerp(heights[z, x], oceanTarget, oceanRing);
            }
        }

        return heights;
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

    // ═══════════════════════════════════════════════════════
    // MESH BUILDING (with vertex colors by slope)
    // ═══════════════════════════════════════════════════════

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

        // ── VERTEX COLORS — paint by slope and height ──
        Vector3[] normals = mesh.normals;
        Color[] colors = new Color[totalVerts];
        float waterWorldH = waterLevel * terrainHeight + waterPlaneYOffset;

        for (int i = 0; i < totalVerts; i++)
        {
            float height = vertices[i].y;
            float slopeAngle = Vector3.Angle(normals[i], Vector3.up);

            Color vertColor;

            if (height < waterWorldH + 1.5f)
            {
                vertColor = sandColor;
            }
            else if (slopeAngle > 40f)
            {
                vertColor = rockColor;
            }
            else if (slopeAngle > 20f)
            {
                float t = (slopeAngle - 20f) / 20f;
                vertColor = Color.Lerp(dirtColor, rockColor, t);
            }
            else
            {
                float t = slopeAngle / 20f;
                vertColor = Color.Lerp(grassColor, dirtColor, t);
            }

            float heightFade = Mathf.InverseLerp(0f, terrainHeight * 0.8f, height);
            vertColor = Color.Lerp(vertColor * 0.85f, vertColor, heightFade);
            colors[i] = vertColor;
        }

        mesh.colors = colors;
        return mesh;
    }

    // ═══════════════════════════════════════════════════════
    // WATER
    // ═══════════════════════════════════════════════════════

    void CreateWaterPlane()
    {
        if (waterObj != null) Destroy(waterObj);

        waterObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObj.name = "WaterPlane";

        float waterWorldHeight = waterLevel * terrainHeight + waterPlaneYOffset;
        waterObj.transform.position = new Vector3(0f, waterWorldHeight, 0f);

        float planeScale = terrainSize / 10f * 1.5f;
        waterObj.transform.localScale = new Vector3(planeScale, 1f, planeScale);

        Renderer rend = waterObj.GetComponent<Renderer>();
        rend.material = CreateSimpleMaterial(waterColor);
        Destroy(waterObj.GetComponent<Collider>());

        Debug.Log($"🐚 Water at height {waterWorldHeight}");
    }

    // ═══════════════════════════════════════════════════════
    // PLAYER POSITIONING
    // ═══════════════════════════════════════════════════════

    void PositionPlayerAboveTerrain()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float bestX = 0f, bestZ = 0f;
            float bestHeight = SampleHeight(0f, 0f);
            float minH = waterLevel * terrainHeight + waterPlaneYOffset + 2f;

            for (int i = 0; i < 20; i++)
            {
                float tx = Random.Range(-15f, 15f);
                float tz = Random.Range(-15f, 15f);
                float h = SampleHeight(tx, tz);
                if (h > minH && h > bestHeight)
                {
                    bestX = tx; bestZ = tz; bestHeight = h;
                }
            }

            Vector3 spawnPos = new Vector3(bestX, bestHeight + 3f, bestZ);
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

    // ═══════════════════════════════════════════════════════
    // LANDMARKS
    // ═══════════════════════════════════════════════════════

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

        for (int attempt = 0; attempt < 50 && placed < 10; attempt++)
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

    // ═══════════════════════════════════════════════════════
    // HEIGHT SAMPLING
    // ═══════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════
    // MATERIALS
    // ═══════════════════════════════════════════════════════

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
        foreach (string sn in shaderNames) { shader = Shader.Find(sn); if (shader != null) break; }
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
