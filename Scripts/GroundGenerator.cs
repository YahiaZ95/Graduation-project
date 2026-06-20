using UnityEngine;

public class GroundGenerator : MonoBehaviour
{
    public float width;
    public float length;
    public PlantationLayout plantationLayout;
    public IrrigationSystem irrigationSystem;
    public ConstraintVisualizer constraintVisualizer;

    [Header("Visual")]
    [SerializeField] private Material groundMaterial;

    private const int widthSegments = 10;
    private const int lengthSegments = 10;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public GameObject Well;
    public Vector3 well_pos;

    private GameObject _wellInstance; // تتبع نسخة البئر الحالية




    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        // إخفاء الـ Renderer لمنع Invalid AABB قبل توليد الـ Mesh
        if (meshRenderer != null)
            meshRenderer.enabled = false;

        if (constraintVisualizer == null)
            constraintVisualizer = FindObjectOfType<ConstraintVisualizer>();

    }


    public void GenerateGround()
    {
        if (width <= 0 || length <= 0)
        {
            Debug.LogError("Invalid ground size.");
            return;
        }

        int vertsX = widthSegments + 1;
        int vertsZ = lengthSegments + 1;
        int totalVerts = vertsX * vertsZ;

        Vector3[] vertices = new Vector3[totalVerts];
        Vector2[] uvs = new Vector2[totalVerts];

        float halfWidth = width * 0.5f;
        float halfLength = length * 0.5f;
        float stepX = width / widthSegments;
        float stepZ = length / lengthSegments;

        for (int z = 0; z < vertsZ; z++)
        {
            for (int x = 0; x < vertsX; x++)
            {
                int index = z * vertsX + x;
                vertices[index] = new Vector3(
                    -halfWidth + x * stepX,
                    0f,
                    -halfLength + z * stepZ
                );
                uvs[index] = new Vector2(
                    (float)x / widthSegments,
                    (float)z / lengthSegments
                );
            }
        }

        int totalTriangles = widthSegments * lengthSegments * 2;
        int[] triangles = new int[totalTriangles * 3];
        int triIndex = 0;

        for (int z = 0; z < lengthSegments; z++)
        {
            for (int x = 0; x < widthSegments; x++)
            {
                int bottomLeft = z * vertsX + x;
                int topLeft = bottomLeft + vertsX;
                int bottomRight = bottomLeft + 1;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = bottomRight;

                triangles[triIndex++] = bottomRight;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;
            }
        }

        Mesh mesh = new Mesh
        {
            name = "GroundMesh",
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>() ?? gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        // Apply ground material if assigned
        if (groundMaterial != null && meshRenderer != null)
            meshRenderer.sharedMaterial = groundMaterial;

        meshRenderer.enabled = true;

        SetWell(well_pos); // ← تم تفعيلها
    }

    /// <summary>
    /// Places or moves the well to the given position. Replaces any existing well instance.
    /// </summary>
    public void SetWell(Vector3 positionWell)
    {
        if (Well == null)
        {
            Debug.LogWarning("Well prefab is not assigned.");
            return;
        }

        if (_wellInstance != null)
            Destroy(_wellInstance);

        _wellInstance = Instantiate(Well, positionWell, Quaternion.identity, transform.parent);
    }

    public void DeleteGround()
    {
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Destroy(meshFilter.mesh);
            meshFilter.sharedMesh = null;
            if (meshCollider != null)
                meshCollider.sharedMesh = null;
            meshRenderer.enabled = false;
            plantationLayout.ClearPlants();
            irrigationSystem.ClearPrevious();
        }

        if (_wellInstance != null)
        {
            Destroy(_wellInstance);
            _wellInstance = null;
        }

        if (constraintVisualizer != null)
            constraintVisualizer.Clear();
    }
}
