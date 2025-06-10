using UnityEngine;
using UnityEngine.EventSystems;
using System; // Para poder usar la palabra 'Action' en caso necesario

public class ButtonHover3DMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Objeto 3D que simula el bot�n f�sico")]
    public Transform button3D;

    [Header("Materiales")]
    public Material hoverMaterial;   // Material al hacer hover
    public Material defaultMaterial; // Material original (sin hover)

    [Header("Movimiento")]
    public float pushDistance = 0.05f; // Distancia de elevaci�n para hover y de retroceso al presionar
    public float speed = 5f;

    [Header("Objetos UI para activar/desactivar")]
    [Tooltip("Se activa al hacer hover. Se desactiva al salir.")]
    public GameObject hoverObject;

    [Tooltip("Se activa al presionar este bot�n. Se desactiva si se presiona otro bot�n.")]
    public GameObject pressObject;

    // Guarda la posici�n original del bot�n
    private Vector3 originalPosition;
    // Guarda la posici�n objetivo en cada momento (hover, presionado, etc.)
    private Vector3 targetPosition;
    // MeshRenderer para cambiar materiales
    private MeshRenderer meshRenderer;

    // Variable est�tica que guarda el �ltimo objeto que se activ� al presionar un bot�n
    private static GameObject lastActivePressObject;

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

        // Posici�n inicial
        originalPosition = button3D.localPosition;
        targetPosition = originalPosition;

        // Asignar el material por defecto
        if (defaultMaterial != null)
            meshRenderer.material = defaultMaterial;

        // Asegurarnos de que los objetos hover y press est�n inactivos al inicio (si procede)
        if (hoverObject != null)
            hoverObject.SetActive(false);

        if (pressObject != null)
            pressObject.SetActive(false);
    }

    void Update()
    {
        // Movemos el bot�n suavemente hacia la posici�n objetivo
        button3D.localPosition = Vector3.Lerp(button3D.localPosition, targetPosition, Time.deltaTime * speed);
    }

    // ----------- EVENTOS DE POINTER (UI) -----------
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Subimos el bot�n en el eje Y para efecto hover
        targetPosition = originalPosition + new Vector3(0, pushDistance, 0);

        // Cambiamos el material de hover
        if (hoverMaterial != null)
            meshRenderer.material = hoverMaterial;

        // Activamos el objeto de hover (si est� asignado)
        if (hoverObject != null)
            hoverObject.SetActive(true);

        // Reproducir un sonido de hover (opcional)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonHoverSound(button3D.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Volver a posici�n original y restaurar el material
        targetPosition = originalPosition;
        if (defaultMaterial != null)
            meshRenderer.material = defaultMaterial;

        // Desactivamos el objeto de hover
        if (hoverObject != null)
            hoverObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Al presionar, movemos el bot�n de vuelta a su posici�n original
        targetPosition = originalPosition;

        // Reproducir sonido de bot�n presionado (opcional)
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonPressedSound(button3D.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Comprobamos si el cursor sigue sobre el bot�n al soltar
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            // Volvemos al estado de hover
            targetPosition = originalPosition + new Vector3(0, pushDistance, 0);

            // -----------------------------------
            // ACTIVAR pressObject y desactivar el anterior
            // -----------------------------------
            if (pressObject != null)
            {
                // Si hab�a un objeto activo de un bot�n anterior, lo desactivamos
                if (lastActivePressObject != null && lastActivePressObject != pressObject)
                {
                    lastActivePressObject.SetActive(false);
                }

                // Activamos el objeto de este bot�n
                pressObject.SetActive(true);

                // Actualizamos la referencia est�tica
                lastActivePressObject = pressObject;
            }
        }
        else
        {
            // Si el puntero ya no est� sobre el bot�n al soltar, vuelve a la posici�n original
            targetPosition = originalPosition;
        }
    }
}
