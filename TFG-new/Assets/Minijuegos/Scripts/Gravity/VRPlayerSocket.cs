using UnityEngine;

/// <summary>
/// Este script se encarga de posicionar al jugador en una ubicación designada al pulsar el botón Play,
/// y deshabilita su movimiento (controlado por el joystick de VR). Al pulsar Pause, se reactiva el movimiento.
/// </summary>
public class VRPlayerSocket : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("El GameObject que representa al jugador en la escena.")]
    [SerializeField] private GameObject player;

    [Tooltip("El Transform que define la posición (y orientación, opcional) a la que se teletransporta el jugador al pulsar Play.")]
    [SerializeField] private Transform targetSocket;

    [Header("Movement Control")]
    [Tooltip("El componente que controla el movimiento del jugador con el joystick de VR. Se desactivará al pulsar Play y se activará al pulsar Pause.")]
    [SerializeField] private MonoBehaviour movementController;

    /// <summary>
    /// Al presionar el botón de Play se teletransporta al jugador a la posición indicada en targetSocket,
    /// se le aplica una rotación adicional de 90 grados en Y y se deshabilita el control del movimiento.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (player != null && targetSocket != null)
        {
            // Actualiza la posición del jugador
            player.transform.position = targetSocket.position;

            // Calcula la nueva rotación: se toma la rotación del targetSocket y se le suma 90 grados en el eje Y
            Vector3 targetEuler = targetSocket.rotation.eulerAngles;
            targetEuler.y += 90;
            player.transform.rotation = Quaternion.Euler(targetEuler);

            Debug.Log("Jugador teletransportado a la posición designada con una rotación de 90 grados en Y.");
        }
        else
        {
            Debug.LogWarning("Player o TargetSocket no están asignados en VRPlayerSocket.");
        }

        if (movementController != null)
        {
            movementController.enabled = false;
            Debug.Log("Movimiento del jugador desactivado.");
        }
    }

    /// <summary>
    /// Al presionar el botón de Pause se reactiva el control del movimiento, permitiendo al jugador moverse nuevamente.
    /// </summary>
    public void OnPauseButtonPressed()
    {
        if (movementController != null)
        {
            movementController.enabled = true;
            Debug.Log("Movimiento del jugador activado.");
        }
    }
}
