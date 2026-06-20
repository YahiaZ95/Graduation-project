using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FarmUIController : MonoBehaviour
{
    [Header("Ground Settings")]
    public GroundGenerator groundGenerator;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;

    [Header("Well Settings")]
    public TMP_InputField wellXInput;
    public TMP_InputField wellZInput;
    public TMP_InputField wellSafeRadiusInput;

    [Header("Crop Settings")]
    public TMP_Dropdown cropTypeDropdown;

    [Header("Forbidden Zone")]
    public TMP_InputField forbiddenXInput;
    public TMP_InputField forbiddenZInput;
    public TMP_InputField forbiddenRadiusInput;

    [Header("Actions")]
    public Button generateButton;
    public Button deleteButton;
    public Button applyPresetButton;

    [Header("Managers")]
    public UnityTcpManager tcpManager;

    private void Start()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        if (applyPresetButton != null)
            applyPresetButton.onClick.AddListener(() => tcpManager?.ApplyPreset(0));

        if (groundGenerator != null)
        {
            if (widthInput != null) widthInput.text = groundGenerator.width.ToString();
            if (heightInput != null) heightInput.text = groundGenerator.length.ToString();
            if (wellXInput != null) wellXInput.text = groundGenerator.well_pos.x.ToString();
            if (wellZInput != null) wellZInput.text = groundGenerator.well_pos.z.ToString();
        }

        if (tcpManager != null && wellSafeRadiusInput != null)
            wellSafeRadiusInput.text = tcpManager.wellSafeRadius.ToString();

        if (forbiddenXInput != null) forbiddenXInput.text = "0";
        if (forbiddenZInput != null) forbiddenZInput.text = "0";
        if (forbiddenRadiusInput != null) forbiddenRadiusInput.text = "0";

        if (cropTypeDropdown != null)
        {
            cropTypeDropdown.ClearOptions();
            cropTypeDropdown.AddOptions(new List<string> { "Trees", "Palm" });
            cropTypeDropdown.value = 0;
        }
    }

    private void OnGenerateClicked()
    {
        if (groundGenerator == null || tcpManager == null)
        {
            Debug.LogWarning("[FarmUIController] GroundGenerator or UnityTcpManager not assigned!");
            return;
        }

        float width = ParseFloat(widthInput, groundGenerator.width);
        float height = ParseFloat(heightInput, groundGenerator.length);
        float wellX = ParseFloat(wellXInput, groundGenerator.well_pos.x);
        float wellZ = ParseFloat(wellZInput, groundGenerator.well_pos.z);
        float wellSafeRadius = ParseFloat(wellSafeRadiusInput, tcpManager.wellSafeRadius);
        CropType cropType = (CropType)cropTypeDropdown.value;

        float forbiddenX = ParseFloat(forbiddenXInput, 0f);
        float forbiddenZ = ParseFloat(forbiddenZInput, 0f);
        float forbiddenRadius = ParseFloat(forbiddenRadiusInput, 0f);

        var forbiddenZones = new List<ForbiddenZone>();
        if (forbiddenRadius > 0)
            forbiddenZones.Add(new ForbiddenZone { x = forbiddenX, z = forbiddenZ, radius = forbiddenRadius });

        Vector3 wellPos = new Vector3(wellX, 0f, wellZ);
        tcpManager.wellSafeRadius = wellSafeRadius;

        groundGenerator.width = width;
        groundGenerator.length = height;
        groundGenerator.well_pos = wellPos;
        groundGenerator.GenerateGround();
        groundGenerator.SetWell(wellPos);

        StartCoroutine(tcpManager.SendFarmDataCoroutine(
            width, height, wellPos, cropType, wellSafeRadius, forbiddenZones));
    }

    private void OnDeleteClicked()
    {
        if (groundGenerator == null)
        {
            Debug.LogWarning("[FarmUIController] GroundGenerator not assigned!");
            return;
        }

        groundGenerator.DeleteGround();
    }

    private static float ParseFloat(TMP_InputField field, float fallback)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return fallback;
        return float.TryParse(field.text, out float value) ? value : fallback;
    }

    private void OnDestroy()
    {
        if (generateButton != null) generateButton.onClick.RemoveAllListeners();
        if (deleteButton != null) deleteButton.onClick.RemoveAllListeners();
        if (applyPresetButton != null) applyPresetButton.onClick.RemoveAllListeners();
    }
}
