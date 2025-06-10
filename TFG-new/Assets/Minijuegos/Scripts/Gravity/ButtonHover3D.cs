using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Objeto 3D que simula el bot�n f�sico")]
    public Transform button3D;

    [Header("Materiales")]
    public Material hoverMaterial;   // Material al hacer hover
    public Material defaultMaterial; // Material original (sin hover)

    [Header("Movimiento")]
    public float pushY = 0.05f; // Distancia de elevaci�n para hover y de retroceso al presionar
    public float pushZ = 0.00f; // Distancia de elevaci�n para hover y de retroceso al presionar
    public float pushX = 0.00f; // Distancia de elevaci�n para hover y de retroceso al presionar

    public float speed = 5f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private MeshRenderer meshRenderer;

    void Start()
    {
        if (button3D == null)
        {
            Debug.LogError("�Debes asignar el objeto 3D al script!");
            return;
        }

        meshRenderer = button3D.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("El objeto 3D necesita un componente MeshRenderer.");
            return;
        }

        // Guardar posici�n inicial
        originalPosition = button3D.localPosition;
        targetPosition = originalPosition;

        // Establecer el material por defecto
        if (defaultMaterial != null)
            meshRenderer.material = defaultMaterial;
    }

    void Update()
    {
        // Animaci�n suave del bot�n
        button3D.localPosition = Vector3.Lerp(button3D.localPosition, targetPosition, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Movimiento hacia arriba en el eje Y para efecto hover
        targetPosition = originalPosition + new Vector3(pushX, pushY, pushZ);

        // Cambiar a material de hover
        if (hoverMaterial != null)
            meshRenderer.material = hoverMaterial;

        // Reproducir sonido de hover del bot�n
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonHoverSound(button3D.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Volver a posici�n original y restaurar el material
        targetPosition = originalPosition;
        if (defaultMaterial != null)
            meshRenderer.material = defaultMaterial;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Al presionar el bot�n, se mueve de vuelta a su posici�n original para simular el efecto presionado
        targetPosition = originalPosition;

        // Reproducir sonido de pressed del bot�n
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonPressedSound(button3D.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Si al soltar el clic el puntero a�n est� sobre el bot�n, se vuelve al estado hover
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            targetPosition = originalPosition + new Vector3(pushX, pushY, pushZ);
        }
        else
        {
            targetPosition = originalPosition;
        }
    }
}
