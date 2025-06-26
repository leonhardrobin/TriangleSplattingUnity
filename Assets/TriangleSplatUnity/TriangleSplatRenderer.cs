using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class TriangleSplatRenderer : MonoBehaviour
{
    [Header("Fichier COFF")]
    public string coffFilePath = "Assets/Models/your_model.off";
    public Material triangleMaterial;

    [Header("Paramètres de chunking")]
    public float chunkSize = 5f;
    public int maxTrianglesPerChunk = 10000;

    [Header("Culling")]
    public float cullingDistance = 50f;
    public float updateInterval = 0.5f;

    private List<GameObject> chunks = new List<GameObject>();
    private Camera mainCamera;
    public bool GenerateColliders = false;
    public Vector3 InitRotation = new Vector3(151, 0, 0);
    void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(LoadCOFFInChunks());
        StartCoroutine(UpdateChunkVisibility());
    }

    IEnumerator LoadCOFFInChunks()
    {
        if (!File.Exists(coffFilePath))
        {
            Debug.LogError("Fichier COFF introuvable : " + coffFilePath);
            yield break;
        }

        string[] lines = File.ReadAllLines(coffFilePath);
        if (!lines[0].Trim().StartsWith("COFF"))
        {
            Debug.LogError("Format COFF invalide.");
            yield break;
        }

        string[] counts = lines[1].Split();
        int vertexCount = int.Parse(counts[0]);
        int faceCount = int.Parse(counts[1]);

        List<Vector3> originalVertices = new List<Vector3>();
        for (int i = 0; i < vertexCount; i++)
        {
            string[] parts = lines[2 + i].Split();
            float x = float.Parse(parts[0], CultureInfo.InvariantCulture);
            float y = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float z = float.Parse(parts[2], CultureInfo.InvariantCulture);
            originalVertices.Add(new Vector3(x, y, z));
        }

        Dictionary<Vector3Int, List<(Vector3, Vector3, Vector3, Color)>> chunkMap = new();

        for (int i = 0; i < faceCount; i++)
        {
            string[] parts = lines[2 + vertexCount + i].Split();
            if (int.Parse(parts[0]) != 3) continue;

            int i0 = int.Parse(parts[1]);
            int i1 = int.Parse(parts[2]);
            int i2 = int.Parse(parts[3]);

            float r = float.Parse(parts[4]) / 255f;
            float g = float.Parse(parts[5]) / 255f;
            float b = float.Parse(parts[6]) / 255f;
            float a = float.Parse(parts[7]) / 255f;
            Color faceColor = new Color(r, g, b, a);

            Vector3 v0 = originalVertices[i0];
            Vector3 v1 = originalVertices[i1];
            Vector3 v2 = originalVertices[i2];

            Vector3 center = (v0 + v1 + v2) / 3f;
            Vector3Int chunkCoord = new Vector3Int(
                Mathf.FloorToInt(center.x / chunkSize),
                Mathf.FloorToInt(center.y / chunkSize),
                Mathf.FloorToInt(center.z / chunkSize)
            );

            if (!chunkMap.ContainsKey(chunkCoord))
                chunkMap[chunkCoord] = new List<(Vector3, Vector3, Vector3, Color)>();

            chunkMap[chunkCoord].Add((v0, v1, v2, faceColor));

            if (i % 10000 == 0)
            {
                float progress = (float)i / faceCount * 100f;
                Debug.Log($"Loading COFF : {progress:F1}% ({i}/{faceCount} faces)");
                yield return null;
            }
        }

        foreach (var kvp in chunkMap)
        {
            GameObject chunk = new GameObject($"Chunk_{kvp.Key}");
            chunk.transform.parent = this.transform;

            List<Vector3> verts = new();
            List<Color> colors = new();
            List<int> tris = new();

            foreach (var (v0, v1, v2, col) in kvp.Value)
            {
                int baseIndex = verts.Count;
                verts.Add(v0); verts.Add(v1); verts.Add(v2);
                colors.Add(col); colors.Add(col); colors.Add(col);
                tris.Add(baseIndex); tris.Add(baseIndex + 1); tris.Add(baseIndex + 2);
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetColors(colors);
            mesh.RecalculateNormals();

            var mf = chunk.AddComponent<MeshFilter>();
            var mr = chunk.AddComponent<MeshRenderer>();
            mf.mesh = mesh;
            mr.material = triangleMaterial;

            chunks.Add(chunk);
            if (GenerateColliders)
                chunk.AddComponent<MeshCollider>();
        }

        Debug.Log($"Chargement terminé : {chunks.Count} chunks créés.");
        this.transform.localEulerAngles = InitRotation;
    }

    IEnumerator UpdateChunkVisibility()
    {
        while (true)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            foreach (var chunk in chunks)
            {

                Renderer renderer = chunk.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    Bounds worldBounds = renderer.bounds;
                    bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, worldBounds);
                    float distance = Vector3.Distance(mainCamera.transform.position, worldBounds.center);
                    chunk.SetActive(isVisible && distance < cullingDistance);
                }

            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

}
