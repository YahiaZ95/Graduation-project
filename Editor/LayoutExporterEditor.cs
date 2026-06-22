using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LayoutExporter))]
public class LayoutExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("exportCamera"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outputFolder"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outputFileNamePrefix"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("imageWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("imageHeight"));

        if (GUILayout.Button("Choose Output Folder"))
        {
            string currentFolder = serializedObject.FindProperty("outputFolder").stringValue;
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", string.IsNullOrEmpty(currentFolder) ? Application.dataPath : currentFolder, string.Empty);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                serializedObject.FindProperty("outputFolder").stringValue = selectedPath;
            }
        }

        serializedObject.ApplyModifiedProperties();

        LayoutExporter exporter = (LayoutExporter)target;
        if (GUILayout.Button("Export To PNG"))
        {
            exporter.ExportToPNG();
        }
    }
}
