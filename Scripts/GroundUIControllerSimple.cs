using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GroundUIController : MonoBehaviour
{
    public GroundGenerator groundGenerator;

    public TMP_InputField widthInput;
    public TMP_InputField lengthInput;
    public Button generateButton;
    public Button deleteButton;



    private void Start()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateClicked);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClicked);

        if (groundGenerator != null)
        {
            if (widthInput != null)
                widthInput.text = groundGenerator.width.ToString();

            if (lengthInput != null)
                lengthInput.text = groundGenerator.length.ToString();
        }
    }

    private void OnGenerateClicked()
    {
        if (groundGenerator == null)
        {
            Debug.LogWarning("لم يتم تعيين GroundGenerator!");
            return;
        }

        float width = ParseFloat(widthInput, groundGenerator.width);
        float length = ParseFloat(lengthInput, groundGenerator.length);

        width = Mathf.Max(0.1f, width);
        length = Mathf.Max(0.1f, length);

        groundGenerator.width = width;
        groundGenerator.length = length;

        groundGenerator.GenerateGround();
    }


    private void OnDeleteClicked()
    {
        if (groundGenerator == null)
        {
            Debug.LogWarning("لم يتم تعيين GroundGenerator!");
            return;
        }

        groundGenerator.DeleteGround();
    }


    private float ParseFloat(TMP_InputField inputField, float defaultValue)
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
            return defaultValue;

        if (float.TryParse(inputField.text, out float result))
            return result;

        return defaultValue;
    }

    private void OnDestroy()
    {
        if (generateButton != null)
            generateButton.onClick.RemoveAllListeners();

        if (deleteButton != null)
            deleteButton.onClick.RemoveAllListeners();
    }
}

