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
        // 0) Leemos el ID nuevo que haya tecleado el jugador
        string inputID = keyboardController.inputField.text.Trim();

        // 1) Si el jugador ha puesto un ID distinto al anterior,
        //    reiniciamos el flag para poder arrancar de nuevo
        if (!string.IsNullOrEmpty(inputID) && inputID != userID)
        {
            sessionStarted = false;
        }

        // 2) Si ya está iniciada la sesión para este mismo ID, no hacemos nada
        if (sessionStarted)
        {
            Debug.LogWarning("[SessionManager] La sesión ya está iniciada con este ID.");
            return;
        }

        // 3) Asignamos el ID actualizado (o mantenemos el anterior si no escribe nada)
        if (!string.IsNullOrEmpty(inputID))
            userID = inputID;
        if (string.IsNullOrEmpty(userID))
            userID = "NOID";

        // 4) Mostramos el ID en pantalla
        if (outputDisplay != null)
            outputDisplay.text = $"ID jugador: {userID}";

        // 5) Obtenemos el código de mini-juego actual
        if (miniGameCodes == null || miniGameCodes.Length == 0)
        {
            Debug.LogError("[SessionManager] miniGameCodes mal configurado.");
            return;
        }
        string gameCode = miniGameCodes[
            Mathf.Clamp(selectedGameIndex, 0, miniGameCodes.Length - 1)
        ];

        // 6) Configuramos y arrancamos el log de la mano derecha (si existe)
        if (handDataLoggerRight != null)
        {
            handDataLoggerRight.userID = userID;
            handDataLoggerRight.gameCode = gameCode;
            handDataLoggerRight.rightHand = true;
            handDataLoggerRight.Initialize();
            handDataLoggerRight.StartLogging();
        }

        // 7) Configuramos y arrancamos el log de la mano izquierda (si existe)
        if (handDataLoggerLeft != null)
        {
            handDataLoggerLeft.userID = userID;
            handDataLoggerLeft.gameCode = gameCode;
            handDataLoggerLeft.rightHand = false;
            handDataLoggerLeft.Initialize();
            handDataLoggerLeft.StartLogging();
        }

        // 8) Arrancamos AnalyticsManager con este nuevo ID (si existe)
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.StartSession(userID);

        // 9) Finalmente, marcamos que la sesión está en marcha
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
