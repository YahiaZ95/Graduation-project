using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PlantationLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GroundGenerator ground;
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject plamPrefab;
    [SerializeField] private TMP_Dropdown plants;


    [SerializeField] private Transform parentContainer;

    [Header("Layout Settings")]
    [SerializeField] private float spacing = 5f;
    [SerializeField] private float borderOffset = 2.5f;

    public int CountX { get; private set; }
    public int CountZ { get; private set; }
    public float StartX { get; private set; }
    public float StartZ { get; private set; }
    public float Spacing => spacing;

    public void GeneratePlants()
    {
        if (ground == null || treePrefab == null || plamPrefab)
        {
            Debug.LogError("Missing references.");
            return;
        }

        if (ground.width <= 0 || ground.length <= 0)
        {
            Debug.LogError("Invalid ground dimensions.");
            return;
        }

        ClearPlants();

        float width = ground.width;
        float length = ground.length;

        // الركن السفلي الأيسر داخل الهامش
        StartX = -width * 0.5f + borderOffset;
        StartZ = -length * 0.5f + borderOffset;

        float usableWidth = width - borderOffset * 2f;
        float usableLength = length - borderOffset * 2f;

        int countX = Mathf.FloorToInt(usableWidth / spacing) + 1;
        int countZ = Mathf.FloorToInt(usableLength / spacing) + 1;

        CountX = countX;
        CountZ = countZ;

        float maxX = width * 0.5f - borderOffset;
        float maxZ = length * 0.5f - borderOffset;

        for (int z = 0; z < countZ; z++)
        {
            for (int x = 0; x < countX; x++)
            {
                float posX = StartX + x * spacing;
                float posZ = StartZ + z * spacing;

                // حماية إضافية من الخروج خارج الحدود
                if (posX > maxX || posZ > maxZ)
                    continue;

                Vector3 position = new Vector3(posX, 0f, posZ);

                Instantiate(treePrefab, position, Quaternion.identity, parentContainer);
            }
        }

        Debug.Log($"Generated {CountX * CountZ} trees.");
    }

    public void ClearPlants()
    {
        if (parentContainer == null) return;

        for (int i = parentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(parentContainer.GetChild(i).gameObject);
        }
    }

    public void BuildFromAI(List<TreePoint> trees)
    {
        ClearPlants();

        string selected = plants.options[plants.value].text;

        foreach (var t in trees)
        {
            Vector3 pos = new Vector3(t.x, 0f, t.z);

            if (selected == "Trees")
            {
                Instantiate(treePrefab, pos, Quaternion.identity, parentContainer);
            }
            else if (selected == "Palm")
            {
                Instantiate(plamPrefab, pos, Quaternion.identity, parentContainer);
            }
        }
    }

}
