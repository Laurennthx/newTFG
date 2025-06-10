// SessionManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  // si cargas escenas desde aquí

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
    [Tooltip("Logger para el controlador izquierdo (si se usa)")]
    public ControllerDataLogger controllerLoggerLeft;
    [Tooltip("Logger para el controlador derecho (si se usa)")]
    public ControllerDataLogger controllerLoggerRight;


    [Header("Mini Juegos")]
    public string[] miniGameCodes = new string[3] { "01", "02", "03" };
    [Range(0, 2)]
    public int selectedGameIndex = 0;

    // El ID actual (cargado de PlayerPrefs o puesto por el teclado)
    private string userID;

    private bool sessionStarted = false;

    void Awake()
    {
        // Singleton + persistir entre escenas
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar userID si estaba guardado
        if (PlayerPrefs.HasKey("UserID"))
            userID = PlayerPrefs.GetString("UserID");
    }

    /// <summary>
    /// Vinculado al botón “Start”:
    /// inicializa y arranca ambos loggers.
    /// </summary>
    /// <summary>
    /// Vinculado al botón “Start”:
    /// inicializa y arranca ambos loggers y AnalyticsManager.
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (sessionStarted)
        {
            Debug.LogWarning("[SessionManager] La sesión ya está iniciada.");
            return;
        }

        // 1) Obtener/actualizar userID
        string inputID = keyboardController.inputField.text.Trim();
        userID = string.IsNullOrEmpty(inputID) ? userID : inputID;
        if (string.IsNullOrEmpty(userID))
            userID = "NOID";

        // 2) Mostrar en pantalla
        if (outputDisplay != null)
            outputDisplay.text = $"ID jugador: {userID}";

        // 3) Validar miniGameCodes
        if (miniGameCodes == null || miniGameCodes.Length < 1)
        {
            Debug.LogError("[SessionManager] miniGameCodes mal configurado.");
            return;
        }
        string gameCode = miniGameCodes[Mathf.Clamp(selectedGameIndex, 0, miniGameCodes.Length - 1)];

        // 4) Configurar y arrancar logs de mano derecha
        if (handDataLoggerRight != null)
        {
            handDataLoggerRight.userID = userID;
            handDataLoggerRight.gameCode = gameCode;
            handDataLoggerRight.rightHand = true;
            handDataLoggerRight.Initialize();
            handDataLoggerRight.StartLogging();
        }

        // 5) Configurar y arrancar logs de mano izquierda
        if (handDataLoggerLeft != null)
        {
            handDataLoggerLeft.userID = userID;
            handDataLoggerLeft.gameCode = gameCode;
            handDataLoggerLeft.rightHand = false;
            handDataLoggerLeft.Initialize();
            handDataLoggerLeft.StartLogging();
        }

        // 6) Iniciar AnalyticsManager (solo si existe en la escena)
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.StartSession(userID);

        // Solo arrancará si en esta escena hay uno asignado.
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

            // 7) Marcar sesión iniciada
            sessionStarted = true;
        Debug.Log($"[SessionManager] Sesión iniciada → ID={userID}, Juego={gameCode}");
    }


    /// <summary>
    /// Vinculado al botón “Stop”: detiene el logging.
    /// </summary>
    public void OnStopButtonPressed()
    {
        if (!sessionStarted)
        {
            Debug.LogWarning("[SessionManager] No hay sesión iniciada.");
            return;
        }

        handDataLoggerRight?.StopLogging();
        handDataLoggerLeft?.StopLogging();
        sessionStarted = false;
        Debug.Log("[SessionManager] Sesión detenida.");
    }

    /// <summary>
    /// Permite a VRKeyboardController actualizar el ID en memoria.
    /// </summary>
    public void SetUserID(string id)
    {
        userID = id;
    }

    /// <summary>
    /// Obtener desde cualquier escena el ID actual.
    /// </summary>
    public string GetUserID()
    {
        return userID;
    }
}
