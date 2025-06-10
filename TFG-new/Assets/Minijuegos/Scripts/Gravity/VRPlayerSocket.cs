using UnityEngine;

/// <summary>
/// Este script se encarga de posicionar al jugador en una ubicaci�n designada al pulsar el bot�n Play,
/// y deshabilita su movimiento (controlado por el joystick de VR). Al pulsar Pause, se reactiva el movimiento.
/// </summary>
public class VRPlayerSocket : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("El GameObject que representa al jugador en la escena.")]
    [SerializeField] private GameObject player;

    [Tooltip("El Transform que define la posici�n (y orientaci�n, opcional) a la que se teletransporta el jugador al pulsar Play.")]
    [SerializeField] private Transform targetSocket;

    [Header("Movement Control")]
    [Tooltip("El componente que controla el movimiento del jugador con el joystick de VR. Se desactivar� al pulsar Play y se activar� al pulsar Pause.")]
    [SerializeField] private MonoBehaviour movementController;

    /// <summary>
    /// Al presionar el bot�n de Play se teletransporta al jugador a la posici�n indicada en targetSocket,
    /// se le aplica una rotaci�n adicional de 90 grados en Y y se deshabilita el control del movimiento.
    /// </summary>
    public void OnPlayButtonPressed()
    {
        if (player != null && targetSocket != null)
        {
            // Actualiza la posici�n del jugador
            player.transform.position = targetSocket.position;

            // Calcula la nueva rotaci�n: se toma la rotaci�n del targetSocket y se le suma 90 grados en el eje Y
            Vector3 targetEuler = targetSocket.rotation.eulerAngles;
            targetEuler.y += 90;
            player.transform.rotation = Quaternion.Euler(targetEuler);

            Debug.Log("Jugador teletransportado a la posici�n designada con una rotaci�n de 90 grados en Y.");
        }
        else
        {
            Debug.LogWarning("Player o TargetSocket no est�n asignados en VRPlayerSocket.");
        }

        if (movementController != null)
        {
            movementController.enabled = false;
            Debug.Log("Movimiento del jugador desactivado.");
        }
    }

    /// <summary>
    /// Al presionar el bot�n de Pause se reactiva el control del movimiento, permitiendo al jugador moverse nuevamente.
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
