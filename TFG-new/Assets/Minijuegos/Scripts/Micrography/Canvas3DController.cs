// Canvas3DController.cs
using UnityEngine;

public class Canvas3DController : MonoBehaviour
{
    [Header("Resolución de la textura")]
    public int texWidth = 1024;
    public int texHeight = 1024;

    [Header("Referencias")]
    public MeshCollider canvasCollider;
    public Renderer drawingRenderer;

    [Header("Patrones")]
    [Tooltip("Asigna aquí los 3 (o N) patrones en el mismo orden que en SessionManager")]
    public Texture2D[] patternTextures;

    [HideInInspector] public int currentPatternIndex = 0;
    [HideInInspector] public Texture2D drawingTexture;

    void Awake()
    {
        // 1) Crea la textura de dibujo (inicialmente transparente)
        drawingTexture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        drawingTexture.filterMode = FilterMode.Bilinear;

        var cols = new Color32[texWidth * texHeight];
        for (int i = 0; i < cols.Length; i++)
            cols[i] = new Color32(0, 0, 0, 0);
        drawingTexture.SetPixels32(cols);
        drawingTexture.Apply();

        // 2) Asigna la textura de dibujo al material
        drawingRenderer.material.SetTexture("_BaseMap", drawingTexture);
    }

    /// <summary>
    /// Patrón actualmente activo para la evaluación
    /// </summary>
    public Texture2D GetCurrentPattern()
    {
        if (patternTextures != null &&
            currentPatternIndex >= 0 &&
            currentPatternIndex < patternTextures.Length)
            return patternTextures[currentPatternIndex];
        return null;
    }

    public void ClearDrawingTexture()
    {
        var cols = new Color32[texWidth * texHeight];
        for (int i = 0; i < cols.Length; i++)
            cols[i] = new Color32(0, 0, 0, 0);

        drawingTexture.SetPixels32(cols);
        drawingTexture.Apply();
    }
}
