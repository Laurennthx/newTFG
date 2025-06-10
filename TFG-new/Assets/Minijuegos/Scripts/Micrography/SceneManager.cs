using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple manager to switch between scenes "Gravity" and "Micrography".
/// Attach to a persistent GameObject or call from UI buttons.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    /// <summary>
    /// Carga la escena llamada "Gravity".
    /// </summary>
    public void LoadGravity()
    {
        SceneManager.LoadScene("Gravity");
    }

    /// <summary>
    /// Carga la escena llamada "Micrography".
    /// </summary>
    public void LoadMicrography()
    {
        SceneManager.LoadScene("Micrography");
    }

    public void LoadLobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void LoadSimonSays()
    {
        SceneManager.LoadScene("Simon Says");
    }
}
