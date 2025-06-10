using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class PoseMatcher : MonoBehaviour
{
    public enum Mano { Derecha, Izquierda }

    [Header("Perfil de referencia")]
    [Tooltip("Pose grabada en un HandPoseProfile")]
    public HandPoseProfile targetPose;

    [Tooltip("Para qué mano comprobaremos")]
    public Mano mano = Mano.Derecha;

    [Header("Ajustes de matching")]
    [Tooltip("Distancia máxima (en metros) permitida para cada joint")]
    public float tolerance = 0.03f;

    [Header("Renderer y materiales")]
    [Tooltip("Renderer del modelo 3D cuya material cambiaremos")]
    public Renderer targetRenderer;
    [Tooltip("Material por defecto (cuando NO coincide)")]
    public Material defaultMaterial;
    [Tooltip("Material cuando la pose COINCIDE")]
    public Material matchedMaterial;

    XRHandSubsystem handSubsystem;

    void Start()
    {
        // 1) Inicializamos el subsistema de hand-tracking
        handSubsystem = XRGeneralSettings.Instance.Manager
                          .activeLoader
                          .GetLoadedSubsystem<XRHandSubsystem>();
        if (handSubsystem == null)
            Debug.LogError("PoseMatcher: no se encontró XRHandSubsystem. ¿Instalaste XR Hands?");

        // 2) Si no asignaste el renderer, lo buscamos en este GameObject
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        // 3) Pon el material por defecto al inicio
        if (defaultMaterial != null && targetRenderer != null)
            targetRenderer.material = defaultMaterial;
    }

    void Update()
    {
        // Asegurarnos de que el subsistema está corriendo
        if (handSubsystem == null || !handSubsystem.running) return;

        // 1) Seleccionamos la mano adecuada
        XRHand hand = (mano == Mano.Derecha)
                      ? handSubsystem.rightHand
                      : handSubsystem.leftHand;

        // Si la mano no está rastreada, revertimos a material por defecto
        if (!hand.isTracked)
        {
            SetMaterial(defaultMaterial);
            return;
        }

        // 2) Leemos la pose de la muñeca (pivote local)
        if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose))
        {
            SetMaterial(defaultMaterial);
            return;
        }

        // 3) Comprobamos cada joint del perfil
        bool allMatch = true;
        for (int i = 0; i < targetPose.joints.Length; i++)
        {
            XRHandJointID jID = targetPose.joints[i];
            if (hand.GetJoint(jID).TryGetPose(out Pose jointPose))
            {
                // convertimos la posición world → local de muñeca
                Vector3 localPos = Quaternion.Inverse(wristPose.rotation)
                                   * (jointPose.position - wristPose.position);

                // comparamos con la posición grabada
                float dist = Vector3.Distance(localPos, targetPose.localPositions[i]);
                if (dist > tolerance)
                {
                    allMatch = false;
                    break;
                }
            }
            else
            {
                allMatch = false;
                break;
            }
        }

        // 4) Cambiamos material según el resultado
        SetMaterial(allMatch ? matchedMaterial : defaultMaterial);
    }

    void SetMaterial(Material mat)
    {
        if (targetRenderer != null && mat != null && targetRenderer.material != mat)
            targetRenderer.material = mat;
    }
}
