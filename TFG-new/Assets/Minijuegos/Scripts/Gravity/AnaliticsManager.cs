// Assets/Scripts/AnalyticsManager.cs
using UnityEngine;
using TMPro;
using System.IO;
using System.Globalization;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [Header("UI – Estadísticas final")]
    [Tooltip("Número de objetos correctos atrapados")]
    public TextMeshProUGUI correctCaughtText;
    [Tooltip("Número de objetos incorrectos atrapados")]
    public TextMeshProUGUI incorrectCaughtText;
    [Tooltip("Número de objetos correctos NO atrapados")]
    public TextMeshProUGUI missedCorrectText;

    // Datos de sesión
    string userID;
    int sessionNumber;
    string filePath;

    // Contadores internos
    int totalSpawned, correctSpawned, incorrectSpawned;
    int correctCaught, incorrectCaught;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Inicia una nueva sesión de Analytics:
    /// - Sanitiza userID
    /// - Incrementa y guarda nSesión en PlayerPrefs
    /// - Crea carpeta persistentDataPath/userID/
    /// - Genera fichero GravityStatistics_{userID}_{nSesion}.txt dentro de esa carpeta
    /// - Escribe cabecera inicial
    /// </summary>
    public void StartSession(string rawUserID)
    {
        // 1) Sanitizar userID
        string realID = string.IsNullOrWhiteSpace(rawUserID) ? "NOID" : rawUserID.Trim();
        realID = realID.Replace("/", "_").Replace("\\", "_");
        userID = realID;

        // 2) Incrementar contador de sesiones (y salvar)
        sessionNumber = PlayerPrefs.GetInt($"SessionCount_{userID}", 0) + 1;
        PlayerPrefs.SetInt($"SessionCount_{userID}", sessionNumber);
        PlayerPrefs.Save();

        // 3) Carpeta del usuario: persistentDataPath/userID
        string basePath = Application.persistentDataPath;
        string folderPath = Path.Combine(basePath, userID);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // 4) Nombre de fichero y ruta completa
        string fileName = $"GravityStatistics_{userID}_{sessionNumber:D2}.txt";
        filePath = Path.Combine(folderPath, fileName);

        // 5) Crear fichero con cabecera
        File.WriteAllText(filePath,
            $"DATA ANALYTICS - Gravity\n" +
            $"# Session {sessionNumber:D2}  User {userID}\n");

        // 6) Reset de contadores
        totalSpawned = correctSpawned = incorrectSpawned = 0;
        correctCaught = incorrectCaught = 0;
    }

    public void RegisterSpawn(bool isCorrect)
    {
        totalSpawned++;
        if (isCorrect) correctSpawned++;
        else incorrectSpawned++;
    }

    public void RegisterCaught(bool isCorrect)
    {
        if (isCorrect) correctCaught++;
        else incorrectCaught++;
    }

    public void LogReactionTime(string itemType, float reactionTime)
    {
        string formatted = reactionTime.ToString("F2", CultureInfo.InvariantCulture);
        string entry = $"RT | User {userID}, Obj {itemType}, {formatted}s\n";
        File.AppendAllText(filePath, entry);
        Debug.Log("[Analytics] " + entry);
    }

    /// <summary>
    /// Finaliza la sesión: vuelca métricas al fichero y actualiza la UI.
    /// </summary>
    public void EndSession()
    {
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

        Debug.Log("[Analytics] Sesión finalizada. Fichero: " + filePath);

        // Actualizar UI
        if (correctCaughtText != null) correctCaughtText.text = correctCaught.ToString();
        if (incorrectCaughtText != null) incorrectCaughtText.text = incorrectCaught.ToString();
        if (missedCorrectText != null) missedCorrectText.text = correctMissed.ToString();
    }
}
