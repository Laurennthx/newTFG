// PrecisionEvaluator.cs
using UnityEngine;

public class PrecisionEvaluator : MonoBehaviour
{
    [Header("Umbral alfa")]
    [Range(0, 1)] public float alphaThreshold = 0.1f;

    [Header("Penalización por pintado fuera")]
    [Range(0, 1)] public float freeThreshold = 0.05f;
    [Range(0, 1)] public float rampStart = 0.25f;
    [Range(0, 2)] public float penaltyFactorMin = 0.75f;
    [Range(0, 3)] public float penaltyFactorMax = 1.5f;

    public Canvas3DController cc;

    /// <summary>
    /// Devuelve un valor [0,1] combinando coverage y penalización de false positives.
    /// Usa el patrón activo en cc.currentPatternIndex.
    /// </summary>
    public float Evaluate()
    {
        Texture2D P = cc.GetCurrentPattern();
        Texture2D D = cc.drawingTexture;

        if (P == null || D == null || P.width != D.width || P.height != D.height)
        {
            Debug.LogError("PrecisionEvaluator: Texturas nulas o de distinto tamaño");
            return 0f;
        }
        

        Color[] pCols = P.GetPixels();
        Color[] dCols = D.GetPixels();

        int totalPattern = 0, hits = 0, falsePos = 0;
        for (int i = 0; i < pCols.Length; i++)
        {
            bool isPattern = pCols[i].a > alphaThreshold;
            bool isDrawn = dCols[i].a > alphaThreshold;

            if (isPattern)
            {
                totalPattern++;
                if (isDrawn) hits++;
            }
            else if (isDrawn)
            {
                falsePos++;
            }
        }

        float coverage = totalPattern > 0
            ? (float)hits / totalPattern
            : 0f;
        float fpRate = (float)falsePos / pCols.Length;

        // Calcula factor de penalización PF
        float PF;
        if (fpRate <= freeThreshold) PF = 0f;
        else if (fpRate <= rampStart) PF = penaltyFactorMin;
        else
        {
            float t = (fpRate - rampStart) / (1f - rampStart);
            PF = Mathf.Lerp(penaltyFactorMin, penaltyFactorMax, t);
        }

        float penalty = PF * fpRate;
        float score = Mathf.Clamp01(coverage - penalty);

        Debug.Log(
            $"Cov: {coverage * 100f:0.0}%  " +
            $"FP: {fpRate * 100f:0.0}%  " +
            $"PF: {PF:0.00}×  " +
            $"Pen: {penalty * 100f:0.0}%  → " +
            $"Score: {score * 100f:0.0}%"
        );

        return score;
    }
}
