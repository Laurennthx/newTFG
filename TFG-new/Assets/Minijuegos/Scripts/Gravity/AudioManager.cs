using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Sound to play for a correct (food) item.")]
    public AudioClip successSound;
    [Tooltip("Sound to play for an incorrect (non-food) item.")]
    public AudioClip errorSound;

    [Header("Button Sounds")]
    [Tooltip("Sound to play when hovering over a button.")]
    public AudioClip buttonHoverSound;
    [Tooltip("Sound to play when pressing a button.")]
    public AudioClip buttonPressedSound;

    private void Awake()
    {
        // Implementa el patrón singleton
        if (Instance == null)
        {
            Instance = this;
            // Si deseas que persista entre escenas, descomenta la siguiente línea:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays the success sound at the given position.
    /// </summary>
    public void PlaySuccessSound(Vector3 position)
    {
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, position);
        }
    }

    /// <summary>
    /// Plays the error sound at the given position.
    /// </summary>
    public void PlayErrorSound(Vector3 position)
    {
        if (errorSound != null)
        {
            AudioSource.PlayClipAtPoint(errorSound, position);
        }
    }

    /// <summary>
    /// Plays the button hover sound at the given position.
    /// </summary>
    public void PlayButtonHoverSound(Vector3 position)
    {
        if (buttonHoverSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonHoverSound, position);
        }
    }

    /// <summary>
    /// Plays the button pressed sound at the given position.
    /// </summary>
    public void PlayButtonPressedSound(Vector3 position)
    {
        if (buttonPressedSound != null)
        {
            AudioSource.PlayClipAtPoint(buttonPressedSound, position);
        }
    }
}
