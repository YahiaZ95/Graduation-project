using UnityEngine;
using TMPro;

public class WellUIController : MonoBehaviour
{
    [SerializeField] private GroundGenerator groundGenerator;
    [SerializeField] private TMP_InputField xInput;
    [SerializeField] private TMP_InputField zInput;

    private void Start()
    {
        xInput.onValueChanged.AddListener(_ => UpdateWellPosition());
        zInput.onValueChanged.AddListener(_ => UpdateWellPosition());
    }

    /// <summary>
    /// Reads X and Z from the input fields and updates the well position on GroundGenerator.
    /// </summary>
    private void UpdateWellPosition()
    {
        float x = ParseFloat(xInput, 0f);
        float z = ParseFloat(zInput, 0f);
        groundGenerator.well_pos = new Vector3(x, 0f, z);
    }

    private float ParseFloat(TMP_InputField inputField, float defaultValue)
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return defaultValue;

        return float.TryParse(inputField.text, out float result) ? result : defaultValue;
    }

    private void OnDestroy()
    {
        xInput.onValueChanged.RemoveAllListeners();
        zInput.onValueChanged.RemoveAllListeners();
    }
}
