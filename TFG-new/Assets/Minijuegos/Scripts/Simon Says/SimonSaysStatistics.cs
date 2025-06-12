// Assets/Scripts/SimonSaysStatistics.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(PoseMatcherSequence))]
public class SimonSaysStatistics : MonoBehaviour
{
    [Header("Referencia a la secuencia")]
    public PoseMatcherSequence sequence;

    [Header("UI � Precisi�n por combo (5)")]
    [Tooltip("TMP para precisi�n Combo 1")]
    public TextMeshProUGUI combo1Text;
    [Tooltip("TMP para precisi�n Combo 2")]
    public TextMeshProUGUI combo2Text;
    [Tooltip("TMP para precisi�n Combo 3")]
    public TextMeshProUGUI combo3Text;
    [Tooltip("TMP para precisi�n Combo 4")]
    public TextMeshProUGUI combo4Text;
    [Tooltip("TMP para precisi�n Combo 5")]
    public TextMeshProUGUI combo5Text;

    [Header("UI � Precisi�n total")]
    [Tooltip("TMP para precisi�n total")]
    public TextMeshProUGUI totalAccuracyText;

    // Tiempos acumulados
    float[] comboTotalTimes;
    float[] comboMatchedTimes;

    bool wasRunning = false;

    void Start()
    {
        if (sequence == null)
            sequence = GetComponent<PoseMatcherSequence>();

        int count = sequence.combinations.Count;
        comboTotalTimes = new float[count];
        comboMatchedTimes = new float[count];

        // Limpia los textos iniciales
        combo1Text.text = combo2Text.text = combo3Text.text =
        combo4Text.text = combo5Text.text = totalAccuracyText.text = "0.0%";
    }

    void Update()
    {
        // Si la secuencia no est� corriendo�
        if (!sequence.Running)
        {
            // Acab� de parar: mostramos stats s�lo una vez
            if (wasRunning)
            {
                ShowStats();
                wasRunning = false;
            }
            return;
        }

        // Sigue corriendo
        wasRunning = true;

        // Acumula tiempos para la combinaci�n actual
        float dt = Time.deltaTime;
        int idx = sequence.CurrentIndex;

        comboTotalTimes[idx] += dt;
        if (sequence.LastFrameMatched)
            comboMatchedTimes[idx] += dt;
    }

    void ShowStats()
    {
        // ��� Precisi�n por combo (igual que antes) ���
        for (int i = 0; i < comboTotalTimes.Length && i < 5; i++)
        {
            float pct = comboTotalTimes[i] > 0f
                ? comboMatchedTimes[i] / comboTotalTimes[i] * 100f
                : 0f;

            string s = $"{pct:F1}%";
            switch (i)
            {
                case 0: combo1Text.text = s; break;
                case 1: combo2Text.text = s; break;
                case 2: combo3Text.text = s; break;
                case 3: combo4Text.text = s; break;
                case 4: combo5Text.text = s; break;
            }
        }

        // ��� Precisi�n total (ignorando �ndices 2 y 7) ���
        int[] ignore = { 0, 2, 7 };
        float totalMatched = 0f, totalTime = 0f;

        for (int i = 0; i < comboMatchedTimes.Length; i++)
        {
            // Si 'i' es 2 o 7, lo ignoramos
            if (System.Array.IndexOf(ignore, i) >= 0)
                continue;

            totalMatched += comboMatchedTimes[i];
            totalTime += comboTotalTimes[i];
        }

        float totalPct = totalTime > 0f
            ? totalMatched / totalTime * 100f
            : 0f;

        totalAccuracyText.text = $"{totalPct:F1}%";
    }

}
