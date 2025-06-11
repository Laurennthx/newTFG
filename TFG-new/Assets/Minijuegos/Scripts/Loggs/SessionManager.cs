// SessionManager.cs
using UnityEngine;
using TMPro;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Referencias al Teclado y Texto")]
    public VRKeyboardController keyboardController;
    public TextMeshProUGUI outputDisplay;

    [Header("HandDataLogger por Mano")]
    public HandDataLogger handDataLoggerRight;
    public HandDataLogger handDataLoggerLeft;

    [Header("Controller Loggers")]
    public ControllerDataLogger controllerLoggerLeft;
    public ControllerDataLogger controllerLoggerRight;

    [Header("Mini Juegos")]
    public string[] miniGameCodes = new string[3] { "01", "02", "03" };
    [Range(0, 2)]
    public int selectedGameIndex = 0;

    private string userID;
    private bool sessionStarted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("UserID"))
            userID = PlayerPrefs.GetString("UserID");
    }

    /// <summary>
    /// Llamado por el botón “Play”
    /// Siempre reinicia los loggers para generar nuevos ficheros.
    /// </summary>
    public void OnButtonPlayPressed()
    {
        // Si ya había logging activo, lo detenemos antes de arrancar de nuevo
        if (sessionStarted)
        {
            Debug.Log("[SessionManager] Reiniciando logging para nueva sesión de juego...");
            handDataLoggerRight?.StopLogging();
            handDataLoggerLeft?.StopLogging();
            controllerLoggerLeft?.StopLogging();
            controllerLoggerRight?.StopLogging();
            sessionStarted = false;
        }

        // 1) ID jugador
        string inputID = keyboardController.inputField.text.Trim();
        userID = string.IsNullOrEmpty(inputID) ? userID : inputID;
        if (string.IsNullOrEmpty(userID))
            userID = "NOID";

        if (outputDisplay != null)
            outputDisplay.text = $"ID jugador: {userID}";

        // 2) Código de juego
        if (miniGameCodes == null || miniGameCodes.Length < 1)
        {
            Debug.LogError("[SessionManager] miniGameCodes mal configurado.");
            return;
        }
        string gameCode = miniGameCodes[Mathf.Clamp(selectedGameIndex, 0, miniGameCodes.Length - 1)];

        // 3) Mano derecha
        if (handDataLoggerRight != null)
        {
            handDataLoggerRight.userID = userID;
            handDataLoggerRight.gameCode = gameCode;
            handDataLoggerRight.rightHand = true;
            handDataLoggerRight.Initialize();
            handDataLoggerRight.StartLogging();
        }

        // 4) Mano izquierda
        if (handDataLoggerLeft != null)
        {
            handDataLoggerLeft.userID = userID;
            handDataLoggerLeft.gameCode = gameCode;
            handDataLoggerLeft.rightHand = false;
            handDataLoggerLeft.Initialize();
            handDataLoggerLeft.StartLogging();
        }

        // 5) Analytics
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.StartSession(userID);

        // 6) Controladores
        if (controllerLoggerLeft != null)
        {
            controllerLoggerLeft.userID = userID;
            controllerLoggerLeft.gameCode = gameCode;
            controllerLoggerLeft.rightController = false;
            controllerLoggerLeft.Initialize();
            controllerLoggerLeft.StartLogging();
        }
        if (controllerLoggerRight != null)
        {
            controllerLoggerRight.userID = userID;
            controllerLoggerRight.gameCode = gameCode;
            controllerLoggerRight.rightController = true;
            controllerLoggerRight.Initialize();
            controllerLoggerRight.StartLogging();
        }

        sessionStarted = true;
        Debug.Log($"[SessionManager] Sesión iniciada → ID={userID}, Juego={gameCode}");
    }

    /// <summary>
    /// Llamado por el botón “Pause”
    /// Detiene todos los loggers y cierra sus archivos.
    /// </summary>
    public void OnButtonPausePressed()
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("[SessionManager] No hay logging activo que detener.");
            return;
        }

        handDataLoggerRight?.StopLogging();
        handDataLoggerLeft?.StopLogging();
        controllerLoggerLeft?.StopLogging();
        controllerLoggerRight?.StopLogging();

        sessionStarted = false;
        Debug.Log("[SessionManager] Sesión detenida y ficheros cerrados.");
    }

    public void SetUserID(string id)
    {
        userID = id;
    }

    public string GetUserID()
    {
        return userID;
    }
}
