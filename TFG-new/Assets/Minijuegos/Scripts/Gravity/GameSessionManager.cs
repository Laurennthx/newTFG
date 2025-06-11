// GameSessionManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.EndSession();
    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
