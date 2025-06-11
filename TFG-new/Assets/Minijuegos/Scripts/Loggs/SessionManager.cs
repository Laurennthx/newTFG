// SessionManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Referencias al Teclado y Texto")]
    public VRKeyboardController keyboardController;
    public TextMeshProUGUI outputDisplay;

    [Header("Mini Juegos (códigos)")]
    public string[] miniGameCodes = new string[3] { "01", "02", "03" };
    [Range(0, 2)]
    public int selectedGameIndex = 0;

    [Header("HandDataLogger por Mano")]
    public HandDataLogger handDataLoggerRight;
    public HandDataLogger handDataLoggerLeft;

    [Header("Controller Loggers")]
    public ControllerDataLogger controllerLoggerLeft;
    public ControllerDataLogger controllerLoggerRight;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1) Detener cualquier sesión previa (manos, controladores y Analytics)
        if (sessionStarted)
        {
            handDataLoggerRight?.StopLogging();
            handDataLoggerLeft?.StopLogging();
            controllerLoggerLeft?.StopLogging();
            controllerLoggerRight?.StopLogging();

            if (AnalyticsManager.Instance != null)
                AnalyticsManager.Instance.StopSession();
        }

        // 2) Resetear flag para permitir nueva sesión
        sessionStarted = false;

        // 3) Reasignar teclado y display de esta escena
        var kb = FindObjectOfType<VRKeyboardController>();
        if (kb != null)
        {
            keyboardController = kb;
            outputDisplay = kb.outputDisplay;
        }

        // 4) Reasignar loggers de mano
        foreach (var h in FindObjectsOfType<HandDataLogger>())
        {
            if (h.rightHand) handDataLoggerRight = h;
            else handDataLoggerLeft = h;
        }

        // 5) Reasignar loggers de controlador
        foreach (var c in FindObjectsOfType<ControllerDataLogger>())
        {
            if (c.rightController) controllerLoggerRight = c;
            else controllerLoggerLeft = c;
        }

        Debug.Log($"[SessionManager] Referencias redefinidas para escena «{scene.name}»");
    }

    /// <summary>
    /// Llamado por el botón “Play”
    /// Reinicia sesión si cambió el ID y arranca nuevos logs con el código seleccionado.
    /// </summary>
    public void OnButtonPlayPressed()
    {
        // 0) Leer ID desde el input actual
        string inputID = keyboardController.inputField.text.Trim();

        // 1) Si es distinto al anterior, permitir reiniciar
        if (!string.IsNullOrEmpty(inputID) && inputID != userID)
            sessionStarted = false;

        // 2) Si ya había sesión con este ID, no hacemos nada
        if (sessionStarted)
        {
            Debug.LogWarning("[SessionManager] La sesión ya está iniciada con este ID.");
            return;
        }

        // 3) Asignar nuevo ID o mantener el anterior
        if (!string.IsNullOrEmpty(inputID))
            userID = inputID;
        if (string.IsNullOrEmpty(userID))
            userID = "NOID";

        // 4) Mostrar ID en pantalla
        if (outputDisplay != null)
            outputDisplay.text = $"ID jugador: {userID}";

        // 5) Obtener código de mini-juego
        if (miniGameCodes == null || miniGameCodes.Length == 0)
        {
            Debug.LogError("[SessionManager] miniGameCodes mal configurado.");
            return;
        }
        string gameCode = miniGameCodes[
            Mathf.Clamp(selectedGameIndex, 0, miniGameCodes.Length - 1)
        ];

        // 6) Configurar y arrancar HandDataLoggers
        if (handDataLoggerRight != null)
        {
            handDataLoggerRight.userID = userID;
            handDataLoggerRight.gameCode = gameCode;
            handDataLoggerRight.rightHand = true;
            handDataLoggerRight.Initialize();
            handDataLoggerRight.StartLogging();
        }
        if (handDataLoggerLeft != null)
        {
            handDataLoggerLeft.userID = userID;
            handDataLoggerLeft.gameCode = gameCode;
            handDataLoggerLeft.rightHand = false;
            handDataLoggerLeft.Initialize();
            handDataLoggerLeft.StartLogging();
        }

        // 7) Iniciar Analytics
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.StartSession(userID);

        // 8) Configurar y arrancar ControllerDataLoggers
        if (controllerLoggerLeft != null)
        {
            controllerLoggerLeft.userID = userID;
            controllerLoggerLeft.gameCode = gameCode;
            controllerLoggerLeft.Initialize();
            controllerLoggerLeft.StartLogging();
        }
        if (controllerLoggerRight != null)
        {
            controllerLoggerRight.userID = userID;
            controllerLoggerRight.gameCode = gameCode;
            controllerLoggerRight.Initialize();
            controllerLoggerRight.StartLogging();
        }

        // 9) Marcar sesión iniciada
        sessionStarted = true;
        Debug.Log($"[SessionManager] Sesión iniciada → ID={userID}, Juego={gameCode}");
    }

    /// <summary>
    /// Llamado por el botón “Pause”
    /// Detiene todos los loggers (manos, controladores) y la sesión de Analytics.
    /// </summary>
    public void OnButtonPausePressed()
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("[SessionManager] No hay sesión activa que detener.");
            return;
        }

        // Detener manos
        handDataLoggerRight?.StopLogging();
        handDataLoggerLeft?.StopLogging();

        // Detener controladores
        controllerLoggerLeft?.StopLogging();
        controllerLoggerRight?.StopLogging();

        // Detener Analytics
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.StopSession();

        sessionStarted = false;
        Debug.Log("[SessionManager] Sesión PAUSE → ficheros cerrados.");
    }

    /// <summary>
    /// Permite cambiar el ID de usuario desde código.
    /// </summary>
    public void SetUserID(string id) => userID = id;

    /// <summary>
    /// Devuelve el ID actual.
    /// </summary>
    public string GetUserID() => userID;

    /// <summary>
    /// Resetea internamente el flag de sesión (si necesitas llamarlo manualmente).
    /// </summary>
    public void ResetSessionState() => sessionStarted = false;
}
