// Assets/Scripts/PoseRecorder.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.InputSystem;             // <-- Nuevo Input System
using UnityEngine.InputSystem.UI;         // (opcional, si usas UI)

public class PoseRecorder : MonoBehaviour
{
    public enum Handedness { Derecha, Izquierda }

    [Header("Perfil de pose")]
    [Tooltip("Profile donde se guardará la pose grabada")]
    public HandPoseProfile profile;

    [Tooltip("¿Qué mano grabamos?")]
    public Handedness mano = Handedness.Derecha;

    [Header("Input")]
    [Tooltip("Input Action que dispara la grabación de la pose")]
    public InputActionReference recordPoseAction;

    XRHandSubsystem handSubsystem;

    private void OnEnable()
    {
        // Obtengo el subsistema de Hand Tracking
        handSubsystem = XRGeneralSettings.Instance.Manager
                          .activeLoader
                          .GetLoadedSubsystem<XRHandSubsystem>();
        if (handSubsystem == null)
            Debug.LogError("XRHandSubsystem no encontrado. Revisa que XR Hands esté instalado y habilitado.");

        // Configuro el Input Action
        if (recordPoseAction != null && recordPoseAction.action != null)
        {
            recordPoseAction.action.Enable();
            recordPoseAction.action.performed += OnRecordPose;
        }
        else
        {
            Debug.LogWarning("No has asignado recordPoseAction en el Inspector.");
        }
    }

    private void OnDisable()
    {
        // Limpio el Input Action
        if (recordPoseAction != null && recordPoseAction.action != null)
        {
            recordPoseAction.action.performed -= OnRecordPose;
            recordPoseAction.action.Disable();
        }
    }

    private void OnRecordPose(InputAction.CallbackContext ctx)
    {
        RecordPose();
    }

    private void RecordPose()
    {
        if (handSubsystem == null || !handSubsystem.running)
        {
            Debug.LogWarning("HandSubsystem no está activo");
            return;
        }

        // Elegimos la mano
        XRHand hand = (mano == Handedness.Derecha)
                      ? handSubsystem.rightHand
                      : handSubsystem.leftHand;

        if (!hand.isTracked)
        {
            Debug.LogWarning($"Mano {mano} no rastreada. Acércala al campo de visión antes de grabar.");
            return;
        }

        // Leemos la pose de la muñeca como base
        if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose))
        {
            Debug.LogWarning("No se pudo leer la muñeca (Wrist).");
            return;
        }

        // Recorremos todos los joints definidos en el profile
        for (int i = 0; i < profile.joints.Length; i++)
        {
            XRHandJointID jID = profile.joints[i];
            if (hand.GetJoint(jID).TryGetPose(out Pose p))
            {
                // Convierto a espacio local de muñeca
                Vector3 localPos = Quaternion.Inverse(wristPose.rotation)
                                   * (p.position - wristPose.position);
                profile.localPositions[i] = localPos;
            }
            else
            {
                Debug.LogWarning($"No se pudo leer joint {jID} de la mano {mano}");
            }
        }

        Debug.Log($"Pose grabada en “{profile.name}” (Mano {mano})");
    }
}
