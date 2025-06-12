using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider), typeof(Renderer))]
public class ActivateButtonOnTouch : MonoBehaviour
{
    [Header("Configuraci�n del bot�n")]
    [Tooltip("Arrastra aqu� el Button cuyo onClick quieres invocar")]
    public Button targetButton;

    [Header("Detecci�n de colisi�n")]
    [Tooltip("Tag que debe tener el collider de la mano del jugador")]
    public string handTag = "PlayerHand";

    [Header("Materiales")]
    [Tooltip("Material inicial del objeto")]
    public Material defaultMaterial;
    [Tooltip("Material despu�s de tocarlo por primera vez")]
    public Material pressedMaterial;

    private Renderer _renderer;
    private bool _hasBeenPressed = false;

    void Start()
    {
        // Cache renderer y fija el material inicial
        _renderer = GetComponent<Renderer>();
        if (_renderer != null && defaultMaterial != null)
            _renderer.material = defaultMaterial;
        else
            Debug.LogWarning($"[{name}] Faltan Renderer o defaultMaterial.");

        if (targetButton == null)
            Debug.LogWarning($"[{name}] No has asignado el Button en el inspector.");

        // Recomienda usar trigger para detecci�n sin f�sica compleja
        if (!GetComponent<Collider>().isTrigger)
            Debug.Log($"[{name}] Considera marcar el Collider como 'Is Trigger' o usar OnCollisionEnter.");
    }

    void OnTriggerEnter(Collider other)
    {
        TryActivate(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryActivate(collision.gameObject);
    }

    private void TryActivate(GameObject other)
    {
        if (_hasBeenPressed || other.tag != handTag)
            return;

        // Invocar onClick del bot�n
        if (targetButton != null)
            targetButton.onClick.Invoke();

        // Cambiar al material de "presionado"
        if (_renderer != null && pressedMaterial != null)
            _renderer.material = pressedMaterial;

        _hasBeenPressed = true;
    }
}
