using System.Collections.Generic;
using UnityEngine;

public class ConstraintVisualizer : MonoBehaviour
{
    public Material forbiddenZoneMaterial;
    public Material wellSafeZoneMaterial;

    private readonly List<GameObject> spawned = new List<GameObject>();

    public void Clear()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }

        spawned.Clear();
    }

    public void SetConstraints(List<ForbiddenZone> forbiddenZones, Vector3 wellPosition, float wellSafeRadius)
    {
        Clear();

        if (forbiddenZones != null)
        {
            foreach (var zone in forbiddenZones)
            {
                if (zone == null || zone.radius <= 0f)
                    continue;

                var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = $"ForbiddenZone_{zone.x}_{zone.z}";

                // adjust to ground level and width
                go.transform.position = new Vector3(zone.x, 0.01f, zone.z);
                go.transform.localScale = new Vector3(zone.radius * 2f, 0.01f, zone.radius * 2f);
                SetLocalColor(go, forbiddenZoneMaterial, new Color(1f, 0f, 0f, 0.25f));
                spawned.Add(go);
            }
        }

        if (wellSafeRadius > 0f)
        {
            var wellGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wellGo.name = "WellSafeZone";
            wellGo.transform.position = new Vector3(wellPosition.x, 0.02f, wellPosition.z);
            wellGo.transform.localScale = new Vector3(wellSafeRadius * 2f, 0.01f, wellSafeRadius * 2f);
            SetLocalColor(wellGo, wellSafeZoneMaterial, new Color(0f, 0.5f, 1f, 0.25f));
            spawned.Add(wellGo);
        }
    }

    private void SetLocalColor(GameObject go, Material overrideMaterial, Color baseColor)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null)
            return;

        if (overrideMaterial != null)
        {
            // Clone so we can safely modify color per-instance without affecting the shared asset
            var mat = new Material(overrideMaterial);
            mat.color = baseColor;
            ApplyTransparency(mat);
            renderer.material = mat;
        }
        else
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = baseColor;
            ApplyTransparency(mat);
            renderer.material = mat;
        }

        // Disable collider as it's purely visual
        var coll = go.GetComponent<Collider>();
        if (coll != null)
            Destroy(coll);
    }

    /// <summary>
    /// Configures the Standard shader for Transparent rendering mode.
    /// </summary>
    private static void ApplyTransparency(Material mat)
    {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
