using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script para asignar un nombre de usuario (CurrentUserName) 
/// al pulsar uno de varios botones de UI.
/// </summary>
public class UserManager : MonoBehaviour
{
    // Variable estática accesible desde cualquier script
    public static string CurrentUserName = "";

    [Header("Botones de usuario")]
    [Tooltip("Arrastra aquí los 6 botones desde el Canvas (mismo orden que userNames).")]
    public Button[] userButtons;

    [Header("Nombres de usuario")]
    [Tooltip("Nombres a asignar al pulsar cada botón. Deben coincidir en cantidad y orden con userButtons.")]
    public string[] userNames;

    void Start()
    {
        // Verificamos que el array de nombres y el de botones tengan la misma cantidad
        if (userButtons.Length != userNames.Length)
        {
            Debug.LogError("La cantidad de botones no coincide con la cantidad de nombres de usuario.");
            return;
        }

        // Asignar un listener para cada botón, de modo que cuando se pulse,
        // se setea el nombre de usuario correspondiente
        for (int i = 0; i < userButtons.Length; i++)
        {
            int index = i; // Necesario para capturar correctamente la variable dentro del lambda
            userButtons[i].onClick.AddListener(() =>
            {
                CurrentUserName = userNames[index];
                Debug.Log("Usuario seleccionado: " + CurrentUserName);
            });
        }
    }
}
