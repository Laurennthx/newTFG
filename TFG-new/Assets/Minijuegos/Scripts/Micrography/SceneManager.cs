using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void OnEnable()
    {
        // Suscribirse al evento para cada carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var sm = SessionManager.Instance;
        if (sm == null) return;

        // 1) Detener sesión si estaba activa
        sm.OnButtonPausePressed();  // pone sessionStarted = false y detiene loggers :contentReference[oaicite:0]{index=0}

        // 2) (Re)inicializar estado interno para poder arrancar de nuevo
        sm.ResetSessionState();    // método a añadir en SessionManager, que haga sessionStarted = false

        // 3) Reasignar referencias de esta escena:

        // Teclado / campo de texto
        var kb = FindObjectOfType<VRKeyboardController>();
        if (kb != null)
        {
            sm.keyboardController = kb;                         // :contentReference[oaicite:1]{index=1}
            sm.outputDisplay = kb.outputDisplay;            // si usas el display del teclado
        }

        // Loggers de mano
        foreach (var h in FindObjectsOfType<HandDataLogger>())
        {
            if (h.rightHand) sm.handDataLoggerRight = h;
            else sm.handDataLoggerLeft = h;
        }

        // Loggers de controlador
        foreach (var c in FindObjectsOfType<ControllerDataLogger>())
        {
            if (c.rightController) sm.controllerLoggerRight = c;
            else sm.controllerLoggerLeft = c;
        }
    }

    // Métodos originales para cambiar de escena…
    public void LoadGravity() => SceneManager.LoadScene("Gravity");
    public void LoadMicrography() => SceneManager.LoadScene("Micrography");
    public void LoadLobby() => SceneManager.LoadScene("Lobby");
    public void LoadSimonSays() => SceneManager.LoadScene("Simon Says");
}
