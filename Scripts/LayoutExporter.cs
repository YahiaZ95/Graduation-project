using UnityEngine;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_STANDALONE
using SFB;
#endif

public class LayoutExporter : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera exportCamera;

    [Header("Output")]
    public string outputFolder = "";
    public string outputFileNamePrefix = "FarmLayout_";

    public int imageWidth = 1920;
    public int imageHeight = 1080;

    public void ExportToPNG()
    {
        Debug.Log("Starting PNG export...");
        StartCoroutine(CaptureAndSave());
    }

    private System.Collections.IEnumerator CaptureAndSave()
    {
        RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
        exportCamera.targetTexture = rt;

        exportCamera.Render();

        RenderTexture.active = rt;

        Texture2D image = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        image.Apply();

        exportCamera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(rt);

        byte[] bytes = image.EncodeToPNG();

        string fileName = outputFileNamePrefix +
                          DateTime.Now.ToString("yyyyMMdd_HHmmss") +
                          ".png";

        string path = "";

        // =========================
        // UNITY EDITOR
        // =========================
#if UNITY_EDITOR
        path = EditorUtility.SaveFilePanel(
            "Save Farm Layout Image",
            "",
            fileName,
            "png"
        );

        // =========================
        // BUILD (Standalone)
        // =========================
#elif UNITY_STANDALONE
        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "png")
        };

        string result = StandaloneFileBrowser.SaveFilePanel(
            "Save Farm Layout Image",
            "",
            fileName,
            extensions
        );

        if (!string.IsNullOrEmpty(result))
        {
            path = result;
        }
#endif

        // =========================
        // SAVE FILE
        // =========================
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("Export complete: " + path);
        }
        else
        {
            Debug.Log("Export cancelled");
        }

        yield return null;
    }
}