// Assets/Scripts/CanvasSessionManager.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class CanvasSessionManager : MonoBehaviour
{
    [Header("Patrones y duración")]
    [Tooltip("Texturas de los patrones a dibujar, en orden secuencial.")]
    public Texture2D[] patterns;
    [Tooltip("Duración (en segundos) de cada patrón, paralelo a `patterns`.")]
    public float[] patternDurations;

    [Header("UI y herramientas")]
    [Tooltip("Renderer donde se mostrará la textura del patrón.")]
    public Renderer patternRenderer;
    [Tooltip("Script que controla el lienzo de dibujo (limpiar, pintar).")]
    public Canvas3DController canvasController;
    [Tooltip("GameObject del pincel/herramienta de dibujo.")]
    public GameObject penTool;
    [Tooltip("Componente para evaluar la precisión del dibujo.")]
    public PrecisionEvaluator precisionEvaluator;
    [Tooltip("Array de TextMeshProUGUI para mostrar el score de cada patrón.")]
    public TextMeshProUGUI[] scoreTexts;
    [Tooltip("TMP para mostrar el temporizador restante.")]
    public TextMeshProUGUI timerText;
    [Tooltip("Transform de la manecilla del reloj.")]
    public Transform clockHand;
    [Tooltip("Pivot alrededor del cual rota la manecilla.")]
    public Transform clockPivot;
    [Tooltip("Eje de rotación de la manecilla (normalmente Vector3.back).")]
    public Vector3 rotationAxis = Vector3.back;

    [Header("Guías de patrón")]
    [Tooltip("Para cada patrón, asignar dos objetos de guía (por ejemplo, líneas o marcos).")]
    public PatternGuide[] patternGuides;

    // Estado interno
    int currentPatternIndex = -1;
    bool sessionStarted = false;
    bool sessionActive = false; // true = running, false = paused or not started
    Coroutine timerCoroutine;
    Quaternion initialHandRotation;
    float remainingTime;

    void Awake()
    {
        if (clockHand != null)
            initialHandRotation = clockHand.rotation;
    }

    /// <summary>
    /// Llamar desde el botón Start. Inicia la sesión y arranca el primer patrón.
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (sessionStarted) return;

        sessionStarted = true;
        sessionActive = true;
        currentPatternIndex = -1;

        // limpiar canvas y scores
        if (canvasController != null) canvasController.ClearDrawingTexture();
        foreach (var txt in scoreTexts) if (txt != null) txt.text = string.Empty;
        foreach (var g in patternGuides)
        {
            if (g.guide1 != null) g.guide1.SetActive(false);
            if (g.guide2 != null) g.guide2.SetActive(false);
        }
        if (penTool != null) penTool.SetActive(true);
        if (clockHand != null) clockHand.rotation = initialHandRotation;

        BeginNextPattern();
    }

    void BeginNextPattern()
    {
        // detener cualquier timer en curso
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        currentPatternIndex++;

        if (currentPatternIndex >= patterns.Length)
        {
            EndCanvasSession();
            return;
        }

        // setup del patrón
        sessionActive = true;
        if (canvasController != null) canvasController.ClearDrawingTexture();
        if (penTool != null) penTool.SetActive(true);

        if (patternRenderer != null)
            patternRenderer.material.mainTexture = patterns[currentPatternIndex];
        if (canvasController != null)
            canvasController.currentPatternIndex = currentPatternIndex;

        ActivateGuides(currentPatternIndex);

        if (currentPatternIndex < scoreTexts.Length && scoreTexts[currentPatternIndex] != null)
            scoreTexts[currentPatternIndex].text = string.Empty;

        if (clockHand != null) clockHand.rotation = initialHandRotation;

        // iniciar temporizador
        remainingTime = (patternDurations != null && currentPatternIndex < patternDurations.Length)
                        ? patternDurations[currentPatternIndex]
                        : 30f;
        UpdateTimerUI(remainingTime);
        timerCoroutine = StartCoroutine(RunTimer());
    }

    IEnumerator RunTimer()
    {
        while (remainingTime > 0f && sessionActive)
        {
            remainingTime -= Time.deltaTime;
            UpdateTimerUI(remainingTime);
            yield return null;
        }

        if (!sessionActive) yield break;

        // fin del patrón
        float scorePct = (precisionEvaluator != null)
            ? precisionEvaluator.Evaluate() * 100f
            : 0f;
        if (currentPatternIndex < scoreTexts.Length && scoreTexts[currentPatternIndex] != null)
            scoreTexts[currentPatternIndex].text = $"{scorePct:0.0}%";

        if (penTool != null) penTool.SetActive(false);

        yield return null;
        BeginNextPattern();
    }

    /// <summary>
    /// Llamar desde el botón Pause para pausar o reanudar.
    /// </summary>
    public void PauseSession()
    {
        if (!sessionStarted) return;

        if (sessionActive)
        {
            // pausar
            sessionActive = false;
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
            if (penTool != null) penTool.SetActive(false);
            Debug.Log("[CanvasSessionManager] Sesión pausada.");
        }
        else
        {
            // reanudar
            sessionActive = true;
            if (penTool != null) penTool.SetActive(true);
            timerCoroutine = StartCoroutine(RunTimer());
            Debug.Log("[CanvasSessionManager] Sesión reanudada.");
        }
    }

    void UpdateTimerUI(float remaining)
    {
        if (timerText != null)
            timerText.text = Mathf.Max(0, Mathf.CeilToInt(remaining)).ToString() + "s";

        if (clockHand != null && clockPivot != null)
        {
            float total = (patternDurations != null && currentPatternIndex < patternDurations.Length)
                ? patternDurations[currentPatternIndex]
                : 30f;
            float deltaAngle = (360f / total) * Time.deltaTime;
            clockHand.RotateAround(clockPivot.position, rotationAxis, deltaAngle);
        }
    }

    void EndCanvasSession()
    {
        sessionActive = false;
        sessionStarted = false;
        Debug.Log("[CanvasSessionManager] Sesión de dibujo finalizada.");
    }

    void ActivateGuides(int idx)
    {
        for (int i = 0; i < patternGuides.Length; i++)
        {
            bool on = (i == idx);
            if (patternGuides[i].guide1 != null) patternGuides[i].guide1.SetActive(on);
            if (patternGuides[i].guide2 != null) patternGuides[i].guide2.SetActive(on);
        }
    }

    [System.Serializable]
    public struct PatternGuide
    {
        public GameObject guide1;
        public GameObject guide2;
    }
}
