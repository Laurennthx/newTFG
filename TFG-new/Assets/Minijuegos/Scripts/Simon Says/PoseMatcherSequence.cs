// Assets/Scripts/PoseMatcherSequence.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.UI;

[System.Serializable]
public class HandCombination
{
    [Tooltip("Perfil de pose para la mano derecha")]
    public HandPoseProfile rightPose;

    [Tooltip("Perfil de pose para la mano izquierda")]
    public HandPoseProfile leftPose;

    [Tooltip("Renderer del modelo 3D de la mano derecha")]
    public Renderer rightRenderer;

    [Tooltip("Renderer del modelo 3D de la mano izquierda")]
    public Renderer leftRenderer;

    [Tooltip("Duración (s) de esta combinación antes de pasar a la siguiente")]
    public float duration = 5f;
}

public class PoseMatcherSequence : MonoBehaviour
{
    [Header("Combinaciones de poses")]
    [Tooltip("Lista de combinaciones: cada una con 2 perfiles, 2 renderers y un tiempo")]
    public List<HandCombination> combinations;

    [Header("Materiales")]
    [Tooltip("Material por defecto (cuando NO coincide)")]
    public Material defaultMaterial;

    [Tooltip("Material cuando la pose COINCIDE")]
    public Material matchedMaterial;

    [Header("UI Timer")]
    [Tooltip("Imagen UI de tipo Filled para mostrar el progreso del timer")]
    public Image timerFillImage;

    [Header("UI End Screen")]
    [Tooltip("Pantalla de fin de partida a activar al completar la secuencia")]
    public GameObject endGameScreen;

    // Para controlar pausa y reanudación
    bool isStarted = false;
    bool running = false;

    // Expuestos para estadísticas o UI externa
    public bool Running => running;
    public int CurrentIndex => currentIndex;
    public bool LastFrameMatched { get; private set; }

    // Estado interno de la secuencia
    XRHandSubsystem handSubsystem;
    int currentIndex = -1;
    float timer = 0f;

    void Awake()
    {
        handSubsystem = XRGeneralSettings.Instance.Manager
                          .activeLoader
                          .GetLoadedSubsystem<XRHandSubsystem>();
        if (handSubsystem == null)
            Debug.LogError("PoseMatcherSequence: no se encontró XRHandSubsystem. ¿Tienes XR Hands instalado y activado?");
    }

    void Start()
    {
        // Desactivar todos los modelos y UI al inicio
        foreach (var combo in combinations)
        {
            if (combo.rightRenderer != null) combo.rightRenderer.gameObject.SetActive(false);
            if (combo.leftRenderer != null) combo.leftRenderer.gameObject.SetActive(false);
        }

        if (timerFillImage != null)
            timerFillImage.fillAmount = 0f;

        if (endGameScreen != null)
            endGameScreen.SetActive(false);
    }

    /// <summary>
    /// Llamar al pulsar Start/Resume
    /// </summary>
    public void StartSequence()
    {
        if (!isStarted)
        {
            // Primer arranque
            if (combinations == null || combinations.Count == 0)
            {
                Debug.LogWarning("PoseMatcherSequence: No hay combinaciones configuradas.");
                return;
            }

            currentIndex = 0;
            timer = 0f;
            isStarted = true;
            running = true;

            if (endGameScreen != null)
                endGameScreen.SetActive(false);

            ActivateCombination(currentIndex);

            if (timerFillImage != null)
                timerFillImage.fillAmount = 0f;
        }
        else if (!running)
        {
            // Reanudar tras pausa
            running = true;
        }
        else
        {
            // Ya estaba corriendo
            Debug.LogWarning("PoseMatcherSequence: La secuencia ya está en marcha.");
        }
    }

    /// <summary>
    /// Llamar al pulsar Pause
    /// </summary>
    public void PauseSequence()
    {
        if (running)
            running = false;
    }

    void Update()
    {
        if (!running || handSubsystem == null || !handSubsystem.running)
            return;

        var combo = combinations[currentIndex];
        timer += Time.deltaTime;

        // Actualiza UI fill según el progreso del timer
        if (timerFillImage != null)
            timerFillImage.fillAmount = Mathf.Clamp01(timer / combo.duration);

        // Comprueba matching
        bool rightMatch = CheckMatch(handSubsystem.rightHand, combo.rightPose, combo.rightPose.tolerance);
        bool leftMatch = CheckMatch(handSubsystem.leftHand, combo.leftPose, combo.leftPose.tolerance);

        LastFrameMatched = rightMatch && leftMatch;

        ApplyMaterial(combo.rightRenderer, rightMatch);
        ApplyMaterial(combo.leftRenderer, leftMatch);

        // ¿Pasamos a la siguiente combinación?
        if (timer >= combo.duration)
        {
            DeactivateCombination(currentIndex);
            currentIndex++;

            if (currentIndex < combinations.Count)
            {
                timer = 0f;
                ActivateCombination(currentIndex);
                if (timerFillImage != null)
                    timerFillImage.fillAmount = 0f;
            }
            else
            {
                // Fin de la secuencia
                running = false;
                isStarted = false;

                if (endGameScreen != null)
                    endGameScreen.SetActive(true);
            }
        }
    }

    void ActivateCombination(int idx)
    {
        var combo = combinations[idx];
        if (combo.rightRenderer != null)
        {
            combo.rightRenderer.gameObject.SetActive(true);
            combo.rightRenderer.material = defaultMaterial;
        }
        if (combo.leftRenderer != null)
        {
            combo.leftRenderer.gameObject.SetActive(true);
            combo.leftRenderer.material = defaultMaterial;
        }
    }

    void DeactivateCombination(int idx)
    {
        var combo = combinations[idx];
        if (combo.rightRenderer != null)
            combo.rightRenderer.gameObject.SetActive(false);
        if (combo.leftRenderer != null)
            combo.leftRenderer.gameObject.SetActive(false);
    }

    bool CheckMatch(XRHand hand, HandPoseProfile profile, float tol)
    {
        if (hand == null || profile == null || !hand.isTracked)
            return false;

        if (!hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose))
            return false;

        for (int i = 0; i < profile.joints.Length; i++)
        {
            var jID = profile.joints[i];
            if (!hand.GetJoint(jID).TryGetPose(out Pose jointPose))
                return false;

            Vector3 localPos = Quaternion.Inverse(wristPose.rotation)
                               * (jointPose.position - wristPose.position);

            if (Vector3.Distance(localPos, profile.localPositions[i]) > tol)
                return false;
        }

        return true;
    }

    void ApplyMaterial(Renderer rend, bool isMatched)
    {
        if (rend == null) return;
        var mat = isMatched ? matchedMaterial : defaultMaterial;
        if (rend.material != mat)
            rend.material = mat;
    }
}
