using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
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

        string fileName = outputFileNamePrefix + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

        string path = "";

#if UNITY_EDITOR
        path = EditorUtility.SaveFilePanel(
            "Save Farm Layout Image",
            "",
            fileName,
            "png"
        );
#else
    path = Path.Combine(Application.persistentDataPath, fileName);
#endif

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