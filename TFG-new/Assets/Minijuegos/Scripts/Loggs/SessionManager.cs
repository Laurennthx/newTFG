// SessionManager.cs
using UnityEngine;
using TMPro;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    [Header("Referencias al Teclado y Texto")]
    public VRKeyboardController keyboardController;
    [SerializeField] private TextMeshProUGUI outputDisplay;

    [Header("Mini Juegos (códigos)")]
    public string[] miniGameCodes = new string[3] { "01", "02", "03" };
    [Range(0, 2)]
    public int selectedGameIndex = 0;  // Ahora puedes elegir manualmente en el Inspector

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

    /// <summary>
    /// Llamado por el botón “Play”
    /// Siempre reinicia (si ya había) y arranca nuevos logs con el código manual seleccionado.
    /// </summary>
    public void OnButtonPlayPressed()
    {
        // Si ya había sesión activa, la reiniciamos
        if (sessionStarted)
        {
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

        // 2) Mostrar ID en pantalla (si está asignado)
        if (outputDisplay != null)
            outputDisplay.text = "ID jugador: " + userID;

        // 3) Código de juego según selección del Inspector
        int idx = Mathf.Clamp(selectedGameIndex, 0, miniGameCodes.Length - 1);
        string gameCode = miniGameCodes[idx];

        // 4) Inicializar y arrancar logs de mano derecha
        handDataLoggerRight.userID = userID;
        handDataLoggerRight.gameCode = gameCode;
        handDataLoggerRight.rightHand = true;
        handDataLoggerRight.Initialize();
        handDataLoggerRight.StartLogging();

        // 5) Inicializar y arrancar logs de mano izquierda
        handDataLoggerLeft.userID = userID;
        handDataLoggerLeft.gameCode = gameCode;
        handDataLoggerLeft.rightHand = false;
        handDataLoggerLeft.Initialize();
        handDataLoggerLeft.StartLogging();

        // 6) Iniciar sesión de Analytics
        AnalyticsManager.Instance?.StartSession(userID);

        // 7) Inicializar y arrancar logs de controlador izquierdo
        controllerLoggerLeft.userID = userID;
        controllerLoggerLeft.gameCode = gameCode;
        controllerLoggerLeft.rightController = false;
        controllerLoggerLeft.Initialize();
        controllerLoggerLeft.StartLogging();

        // 8) Inicializar y arrancar logs de controlador derecho
        controllerLoggerRight.userID = userID;
        controllerLoggerRight.gameCode = gameCode;
        controllerLoggerRight.rightController = true;
        controllerLoggerRight.Initialize();
        controllerLoggerRight.StartLogging();

        sessionStarted = true;
        Debug.Log($"[SessionManager] Sesión PLAY → ID={userID}, Juego={gameCode}");
    }

    /// <summary>
    /// Llamado por el botón “Pause”
    /// Detiene todos los loggers y cierra sus archivos.
    /// </summary>
    public void OnButtonPausePressed()
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("[SessionManager] No hay sesión activa que detener.");
            return;
        }

        handDataLoggerRight.StopLogging();
        handDataLoggerLeft.StopLogging();
        controllerLoggerLeft.StopLogging();
        controllerLoggerRight.StopLogging();

        sessionStarted = false;
        Debug.Log("[SessionManager] Sesión PAUSE → ficheros cerrados.");
    }

    /// <summary>
    /// Permite cambiar el ID de usuario desde código si es necesario.
    /// </summary>
    public void SetUserID(string id) => userID = id;

    public string GetUserID() => userID;
}
