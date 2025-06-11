// AnalyticsManager.cs
using UnityEngine;
using TMPro;
using System.IO;
using System.Globalization;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("UI – Estadísticas final")]
    public TextMeshProUGUI correctCaughtText;
    public TextMeshProUGUI incorrectCaughtText;
    public TextMeshProUGUI missedCorrectText;

    private string userID;
    private int sessionNumber;
    private string filePath;
    private int totalSpawned, correctSpawned, incorrectSpawned;
    private int correctCaught, incorrectCaught;
    private bool analyticsSessionActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Inicia nueva sesión de analytics para el usuario dado.
    /// </summary>
    public void StartSession(string rawUserID)
    {
        if (analyticsSessionActive)
        {
            Debug.LogWarning("[AnalyticsManager] Ya hay una sesión activa.");
            return;
        }

        // Normalizar ID
        string realID = string.IsNullOrWhiteSpace(rawUserID) ? "NOID" : rawUserID.Trim();
        realID = realID.Replace("/", "_").Replace("\\", "_");
        userID = realID;

        // Contador de sesiones por usuario
        sessionNumber = PlayerPrefs.GetInt($"SessionCount_{userID}", 0) + 1;
        PlayerPrefs.SetInt($"SessionCount_{userID}", sessionNumber);
        PlayerPrefs.Save();

        // Crear carpeta de usuario si no existe
        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, userID);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Fichero de estadísticas
        string fileName = $"GravityStatistics_{userID}_{sessionNumber:D2}.txt";
        filePath = Path.Combine(folderPath, fileName);

        // Cabecera inicial
        File.WriteAllText(filePath,
            $"DATA ANALYTICS - Gravity\n" +
            $"# Session {sessionNumber:D2}  User {userID}\n");

        totalSpawned = correctSpawned = incorrectSpawned = 0;
        correctCaught = incorrectCaught = 0;
        analyticsSessionActive = true;

        Debug.Log($"[AnalyticsManager] Sesión analytics iniciada: {filePath}");
    }

    public void RegisterSpawn(bool isCorrect)
    {
        if (!analyticsSessionActive) return;
        totalSpawned++;
        if (isCorrect) correctSpawned++;
        else incorrectSpawned++;
    }

    public void RegisterCaught(bool isCorrect)
    {
        if (!analyticsSessionActive) return;
        if (isCorrect) correctCaught++;
        else incorrectCaught++;
    }

    public void LogReactionTime(string itemType, float reactionTime)
    {
        if (!analyticsSessionActive) return;
        string formatted = reactionTime.ToString("F2", CultureInfo.InvariantCulture);
        string entry = $"RT | User {userID}, Obj {itemType}, {formatted}s\n";
        File.AppendAllText(filePath, entry);
        Debug.Log("[Analytics] " + entry);
    }

    /// <summary>
    /// Finaliza la sesión de analytics y escribe métricas finales.
    /// </summary>
    public void EndSession()
    {
        if (!analyticsSessionActive)
        {
            Debug.LogWarning("[AnalyticsManager] No hay sesión activa para finalizar.");
            return;
        }

        int totalCaught = correctCaught + incorrectCaught;
        int correctMissed = correctSpawned - correctCaught;
        int totalMissed = totalSpawned - totalCaught;

        float precisionPct = (totalCaught > 0) ? correctCaught / (float)totalCaught * 100f : 0f;
        float missedCorrectPct = (correctSpawned > 0) ? correctMissed / (float)correctSpawned * 100f : 0f;

        using (var sw = File.AppendText(filePath))
        {
            sw.WriteLine("\n# Precision metrics");
            sw.WriteLine($"Total spawned       = {totalSpawned}");
            sw.WriteLine($"Correct spawned     = {correctSpawned}");
            sw.WriteLine($"Incorrect spawned   = {incorrectSpawned}");
            sw.WriteLine($"Correct caught      = {correctCaught}");
            sw.WriteLine($"Incorrect caught    = {incorrectCaught}");
            sw.WriteLine($"Correct missed      = {correctMissed}");
            sw.WriteLine($"Total missed        = {totalMissed}");
            sw.WriteLine($"Precision (%)       = {precisionPct:F2}");
            sw.WriteLine($"Missed correct (%)  = {missedCorrectPct:F2}");
        }

        // Actualizar UI final
        if (correctCaughtText != null) correctCaughtText.text = correctCaught.ToString();
        if (incorrectCaughtText != null) incorrectCaughtText.text = incorrectCaught.ToString();
        if (missedCorrectText != null) missedCorrectText.text = correctMissed.ToString();

        Debug.Log("[Analytics] Sesión analytics finalizada. Fichero: " + filePath);
        analyticsSessionActive = false;
    }

    /// <summary>
    /// Detiene la sesión de analytics (alias de EndSession).
    /// </summary>
    public void StopSession()
    {
        EndSession();
    }

    void OnApplicationQuit()
    {
        if (analyticsSessionActive)
            EndSession();
    }
}