// Assets/Scripts/HandPoseProfile.cs
using UnityEngine;
using UnityEngine.XR.Hands;

[CreateAssetMenu(fileName = "NewHandPose", menuName = "HandTracking/HandPoseProfile")]
public class HandPoseProfile : ScriptableObject
{
    [Tooltip("Lista de joints que usaremos para esta pose")]
    public XRHandJointID[] joints;

    [Tooltip("Posiciones locales (relativas a la muñeca) para cada joint")]
    public Vector3[] localPositions;

    [Header("Matching Settings")]
    [Tooltip("Distancia máxima (en metros) permitida para considerar un joint como correcto")]
    public float tolerance = 0.03f;

    private void OnValidate()
    {
        if (joints != null && (localPositions == null || localPositions.Length != joints.Length))
            localPositions = new Vector3[joints.Length];
    }
}
