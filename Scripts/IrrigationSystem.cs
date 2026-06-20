using System.Collections.Generic;
using UnityEngine;

public class IrrigationSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlantationLayout plantation;
    [SerializeField] private Transform parentContainer;

    [Header("Irrigation Settings")]
    [SerializeField] private float pipeHeight = 0.05f;
    [SerializeField] private float pipeWidth = 0.2f;
    [SerializeField] private Material pipeMaterial;

    [Header("Pipe Materials (optional overrides)")]
    [SerializeField] private Material mainPipeMaterial;
    [SerializeField] private Material branchPipeMaterial;

    public void GenerateIrrigation()
    {
        ClearPrevious();

        int countX = plantation.CountX;
        int countZ = plantation.CountZ;

        float startX = plantation.StartX;
        float startZ = plantation.StartZ;
        float spacing = plantation.Spacing;

        // === إنشاء الصفوف (Horizontal) ===
        for (int row = 0; row < countZ; row++)
        {
            float zPos = startZ + row * spacing - 1;
            Vector3 startPoint = new Vector3(startX - 1, pipeHeight, zPos);
            Vector3 endPoint = new Vector3(startX + (countX - 1) * spacing, pipeHeight, zPos);

            CreatePipe(startPoint, endPoint);
        }

        // === إنشاء الأعمدة (Vertical) ===

        float xPos = startX - 1;
        Vector3 startPointV = new Vector3(xPos, pipeHeight, startZ - 1);
        Vector3 endPointV = new Vector3(xPos, pipeHeight, (startZ + (countZ - 1) * spacing) - 1);

        CreatePipe(startPointV, endPointV);


        Debug.Log("Irrigation system generated with horizontal and vertical pipes.");
    }

    private void CreatePipe(Vector3 start, Vector3 end)
    {
        // منع AABB غير صالح
        if (start == end)
        {
            Debug.LogWarning("Skipping pipe with identical start and end points.");
            return;
        }

        GameObject pipe = new GameObject("Pipe");
        pipe.transform.SetParent(parentContainer);

        LineRenderer lr = pipe.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = pipeWidth;
        lr.endWidth = pipeWidth;
        lr.material = pipeMaterial;
        lr.useWorldSpace = true;
    }


    public void ClearPrevious()
    {
        if (parentContainer == null) return;
        for (int i = parentContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parentContainer.GetChild(i).gameObject);
        }
    }

    public void BuildFromAI(List<Pipe> pipes)
    {
        ClearPrevious();

        foreach (var p in pipes)
        {
            Vector3 start = new Vector3(p.start.x, pipeHeight, p.start.z);
            Vector3 end = new Vector3(p.end.x, pipeHeight, p.end.z);
            CreatePipeWithType(start, end, p.pipe_type);
        }
    }

    /// <summary>
    /// Creates a pipe LineRenderer with a color matching its type (main/branch/lateral).
    /// </summary>
    private void CreatePipeWithType(Vector3 start, Vector3 end, string pipeType)
    {
        if (start == end)
        {
            Debug.LogWarning("Skipping pipe with identical start and end points.");
            return;
        }

        Material material = ResolveMaterial(pipeType);
        GameObject pipe = new GameObject($"Pipe_{pipeType}");
        pipe.transform.SetParent(parentContainer);

        LineRenderer lr = pipe.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = pipeWidth;
        lr.endWidth = pipeWidth;
        lr.material = material;
        lr.useWorldSpace = true;
    }

    /// <summary>
    /// Resolves the correct material based on pipe type string.
    /// Falls back to pipeMaterial if no override is assigned.
    /// </summary>
    private Material ResolveMaterial(string pipeType)
    {
        switch (pipeType?.ToLower())
        {
            case "main":
                return mainPipeMaterial != null ? mainPipeMaterial : pipeMaterial;
            case "branch":
            case "lateral":
                return branchPipeMaterial != null ? branchPipeMaterial : pipeMaterial;
            default:
                return pipeMaterial;
        }
    }
}
