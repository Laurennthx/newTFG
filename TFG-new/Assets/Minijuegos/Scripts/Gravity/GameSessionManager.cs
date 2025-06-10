using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        // Finalizamos la sesión de Analytics si existe
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.EndSession();
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
