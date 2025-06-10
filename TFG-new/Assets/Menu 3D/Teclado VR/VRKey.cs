using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class VRKey : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("D�jalo vac�o para leer el texto del hijo TMP")]
    public string keyValue;

    private VRKeyboardController controller;

    private void Awake()
    {
        // busca el controlador en la escena (tambi�n podr�as arrastrarlo en Inspector)
        controller = FindObjectOfType<VRKeyboardController>();

        // si no tenemos keyValue, la leemos del TMP Text de hijo
        if (string.IsNullOrEmpty(keyValue))
        {
            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                keyValue = tmp.text;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null)
            controller.OnKeyPress(keyValue);
    }
}
