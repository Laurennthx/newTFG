// VRKeyboardController.cs
using UnityEngine;
using TMPro;

public class VRKeyboardController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_InputField inputField;
    public TMP_InputField inputField2;
    public TMP_InputField inputFieldES;

    public TextMeshProUGUI outputDisplay;
    public TextMeshProUGUI outputDisplay2;
    public TextMeshProUGUI outputDisplayES;

    private string currentText = "";

    void Start()
    {
        // 1) Si ya hay un UserID guardado, lo cargamos…
        if (PlayerPrefs.HasKey("UserID"))
        {
            currentText = PlayerPrefs.GetString("UserID");
            UpdateInputField();
            UpdateOutputDisplay();    // <— aquí le decimos que también rellene los outputDisplay
        }
    }

    public void OnKeyPress(string key)
    {
        switch (key)
        {
            case "Enter":
                Submit();
                break;
            case "Delete":
                if (currentText.Length > 0)
                    currentText = currentText.Substring(0, currentText.Length - 1);
                UpdateInputField();
                break;
            case "Clear":
                currentText = "";
                UpdateInputField();
                break;
            default:
                currentText += key;
                UpdateInputField();
                break;
        }
    }

    private void Submit()
    {
        // 1) Mostrar en pantalla y guardar
        UpdateOutputDisplay();

        PlayerPrefs.SetString("UserID", currentText);
        PlayerPrefs.Save();

        if (SessionManager.Instance != null)
            SessionManager.Instance.SetUserID(currentText);

        // …carga de escena, etc.
    }

    private void UpdateInputField()
    {
        inputField.text = currentText;
        inputField2.text = currentText;
        inputFieldES.text = currentText;
        inputField.caretPosition = currentText.Length;
        inputField2.caretPosition = currentText.Length;
        inputFieldES.caretPosition = currentText.Length;
    }

    private void UpdateOutputDisplay()
    {
        if (outputDisplay != null) outputDisplay.text = currentText;
        if (outputDisplay2 != null) outputDisplay2.text = currentText;
        if (outputDisplayES != null) outputDisplayES.text = currentText;
    }
}
