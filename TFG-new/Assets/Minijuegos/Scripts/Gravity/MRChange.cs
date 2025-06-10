using UnityEngine;
using UnityEngine.InputSystem;

public class MRChange : MonoBehaviour
{
    [Header("Cámaras")]
    [Tooltip("Cámara para Realidad Virtual.")]
    public GameObject VRCamera;

    [Tooltip("Cámara para Realidad Mixta.")]
    public GameObject MRCamera;

    [Header("GameObjects a ocultar/mostrar")]
    [Tooltip("GameObjects que se deben ocultar en modo VR.")]
    public GameObject[] VRHide;

    [Tooltip("GameObjects que se deben ocultar en modo MR.")]
    public GameObject[] MRHide;

    [Header("Input Action")]
    [Tooltip("Input Action asignado al botón A del mando derecho.")]
    public InputActionReference changeModeAction;

    // true: modo VR activo, false: modo MR activo. Se asume que el modo inicial es VR.
    private bool isVRMode = true;

    private void OnEnable()
    {
        if (changeModeAction != null && changeModeAction.action != null)
        {
            changeModeAction.action.Enable();
            changeModeAction.action.performed += OnChangeMode;
        }
    }

    private void OnDisable()
    {
        if (changeModeAction != null && changeModeAction.action != null)
        {
            changeModeAction.action.performed -= OnChangeMode;
            changeModeAction.action.Disable();
        }
    }

    private void Start()
    {
        // Configura el estado inicial de la escena.
        SetMode(isVRMode);
    }

    /// <summary>
    /// Método llamado al pulsar el botón A que alterna la modalidad entre VR y MR.
    /// </summary>
    /// <param name="context">Contexto del input.</param>
    private void OnChangeMode(InputAction.CallbackContext context)
    {
        // Alterna la modalidad
        isVRMode = !isVRMode;
        SetMode(isVRMode);
    }

    /// <summary>
    /// Configura las cámaras y los GameObjects a ocultar/mostrar según el modo actual.
    /// </summary>
    /// <param name="vrMode">Si es true se activa el modo VR, sino se activa el modo MR.</param>
    private void SetMode(bool vrMode)
    {
        if (vrMode)
        {
            Debug.Log("Cambiando a modo Realidad Virtual");

            // Activa la cámara VR y desactiva la MR
            if (VRCamera != null) VRCamera.SetActive(true);
            if (MRCamera != null) MRCamera.SetActive(false);

            // En modo VR: ocultamos los GameObjects asignados a VRHide y mostramos los de MRHide
            SetGameObjectsActive(VRHide, false);
            SetGameObjectsActive(MRHide, true);
        }
        else
        {
            Debug.Log("Cambiando a modo Realidad Mixta");

            // Activa la cámara MR y desactiva la VR
            if (MRCamera != null) MRCamera.SetActive(true);
            if (VRCamera != null) VRCamera.SetActive(false);

            // En modo MR: ocultamos los GameObjects asignados a MRHide y mostramos los de VRHide
            SetGameObjectsActive(MRHide, false);
            SetGameObjectsActive(VRHide, true);
        }
    }

    /// <summary>
    /// Configura el estado activo/inactivo de cada GameObject en la lista.
    /// </summary>
    /// <param name="objects">Lista de GameObjects.</param>
    /// <param name="state">True para activar, false para desactivar.</param>
    private void SetGameObjectsActive(GameObject[] objects, bool state)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }
}
