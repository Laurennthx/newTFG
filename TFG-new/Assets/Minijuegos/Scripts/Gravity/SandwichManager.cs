using UnityEngine;
using System.Collections;

/// <summary>
/// Este script se encarga de "montar" el sándwich a medida que se recogen los ingredientes.
/// Cada vez que se recoge un ingrediente (por ejemplo, "Tomato", "Bread", etc.), se activa
/// un GameObject correspondiente en la escena. Cuando se han recogido todos los ingredientes,
/// se muestra el preview del sándwich completo.
/// Al terminar una ronda (si el sandwich está completo), se instancia un prefab del sandwich en una
/// posición determinada y se reinician los previews para comenzar de nuevo.
/// Además, al desbloquear cada preview se activa un GameObject de partículas durante 1 segundo.
/// </summary>
public class SandwichManager : MonoBehaviour
{
    [Header("Ingredient Previews")]
    [Tooltip("GameObject que representa el tomate (se activa al recoger 'Tomato').")]
    [SerializeField] private GameObject tomatoPreview;
    [Tooltip("GameObject que representa la lechuga (se activa al recoger 'Lettuce').")]
    [SerializeField] private GameObject lettucePreview;
    [Tooltip("GameObject que representa el jamón (se activa al recoger 'Ham').")]
    [SerializeField] private GameObject hamPreview;
    [Tooltip("GameObject que representa el queso (se activa al recoger 'Cheese').")]
    [SerializeField] private GameObject cheesePreview;
    [Tooltip("GameObject que representa el pan (se activa al recoger 'Bread').")]
    [SerializeField] private GameObject breadPreview;

    [Header("Particle Effects")]
    [Tooltip("Partículas para el Tomato.")]
    [SerializeField] private GameObject tomatoParticles;
    [Tooltip("Partículas para la Lettuce.")]
    [SerializeField] private GameObject lettuceParticles;
    [Tooltip("Partículas para el Ham.")]
    [SerializeField] private GameObject hamParticles;
    [Tooltip("Partículas para el Cheese.")]
    [SerializeField] private GameObject cheeseParticles;
    [Tooltip("Partículas para el Bread.")]
    [SerializeField] private GameObject breadParticles;

    [Header("Final Sandwich Preview")]
    [Tooltip("GameObject que se activa cuando se han recogido todos los ingredientes.")]
    [SerializeField] private GameObject completeSandwich;

    [Header("Round Settings")]
    [Tooltip("Prefab del sandwich que se spawnea al finalizar la ronda.")]
    [SerializeField] private GameObject sandwichPrefab;
    [Tooltip("Punto de spawn donde se instanciará el sandwich final de la ronda.")]
    [SerializeField] private Transform sandwichSpawnPoint;

    // Variables internas para asegurar que cada ingrediente se active solo una vez.
    private bool tomatoAdded = false;
    private bool lettuceAdded = false;
    private bool hamAdded = false;
    private bool cheeseAdded = false;
    private bool breadAdded = false;

    // Indica si la ronda ha sido completada para evitar múltiples triggers.
    private bool roundCompleted = false;

    private void Start()
    {
        // Inicialmente, se ocultan todas las previsualizaciones, el sandwich completo y las partículas.
        if (tomatoPreview != null) tomatoPreview.SetActive(false);
        if (lettucePreview != null) lettucePreview.SetActive(false);
        if (hamPreview != null) hamPreview.SetActive(false);
        if (cheesePreview != null) cheesePreview.SetActive(false);
        if (breadPreview != null) breadPreview.SetActive(false);
        if (completeSandwich != null) completeSandwich.SetActive(false);

        if (tomatoParticles != null) tomatoParticles.SetActive(false);
        if (lettuceParticles != null) lettuceParticles.SetActive(false);
        if (hamParticles != null) hamParticles.SetActive(false);
        if (cheeseParticles != null) cheeseParticles.SetActive(false);
        if (breadParticles != null) breadParticles.SetActive(false);

        Debug.Log("SandwichManager iniciado. Todas las previews, partículas y el preview del sandwich completo están desactivados.");
    }

    private void Update()
    {
        // Consultamos los contadores en ItemCollector y los mostramos en consola.
        int tomatoCount = ItemCollector.GetItemCount("Tomato");
        int lettuceCount = ItemCollector.GetItemCount("Lettuce");
        int hamCount = ItemCollector.GetItemCount("Ham");
        int cheeseCount = ItemCollector.GetItemCount("Cheese");
        int breadCount = ItemCollector.GetItemCount("Bread");

        Debug.Log($"Contadores: Tomato={tomatoCount}, Lettuce={lettuceCount}, Ham={hamCount}, Cheese={cheeseCount}, Bread={breadCount}");

        // Activar preview y partículas para cada ingrediente si se recoge al menos uno y no se ha activado antes.
        if (!tomatoAdded && tomatoCount > 0)
        {
            if (tomatoPreview != null)
            {
                tomatoPreview.SetActive(true);
                Debug.Log("TomatoPreview activado.");
            }
            tomatoAdded = true;
            if (tomatoParticles != null)
                StartCoroutine(TriggerParticles(tomatoParticles));
        }
        if (!lettuceAdded && lettuceCount > 0)
        {
            if (lettucePreview != null)
            {
                lettucePreview.SetActive(true);
                Debug.Log("LettucePreview activado.");
            }
            lettuceAdded = true;
            if (lettuceParticles != null)
                StartCoroutine(TriggerParticles(lettuceParticles));
        }
        if (!hamAdded && hamCount > 0)
        {
            if (hamPreview != null)
            {
                hamPreview.SetActive(true);
                Debug.Log("HamPreview activado.");
            }
            hamAdded = true;
            if (hamParticles != null)
                StartCoroutine(TriggerParticles(hamParticles));
        }
        if (!cheeseAdded && cheeseCount > 0)
        {
            if (cheesePreview != null)
            {
                cheesePreview.SetActive(true);
                Debug.Log("CheesePreview activado.");
            }
            cheeseAdded = true;
            if (cheeseParticles != null)
                StartCoroutine(TriggerParticles(cheeseParticles));
        }
        if (!breadAdded && breadCount > 0)
        {
            if (breadPreview != null)
            {
                breadPreview.SetActive(true);
                Debug.Log("BreadPreview activado.");
            }
            breadAdded = true;
            if (breadParticles != null)
                StartCoroutine(TriggerParticles(breadParticles));
        }

        // Si se han recogido todos los ingredientes y la ronda aún no se ha completado.
        if (!roundCompleted && tomatoAdded && lettuceAdded && hamAdded && cheeseAdded && breadAdded)
        {
            if (completeSandwich != null && !completeSandwich.activeSelf)
            {
                completeSandwich.SetActive(true);
                Debug.Log("Preview del sandwich completo activado.");
            }
            roundCompleted = true;
            // Llamamos a la coroutine para completar la ronda.
            StartCoroutine(CompleteRoundCoroutine());
        }
    }

    /// <summary>
    /// Activa las partículas del ingrediente durante 1 segundo y luego las desactiva.
    /// </summary>
    /// <param name="particles">GameObject de partículas a activar.</param>
    private IEnumerator TriggerParticles(GameObject particles)
    {
        particles.SetActive(true);
        Debug.Log($"Partículas {particles.name} activadas.");
        yield return new WaitForSeconds(1f);
        particles.SetActive(false);
        Debug.Log($"Partículas {particles.name} desactivadas.");
    }

    /// <summary>
    /// Coroutine que se encarga de completar la ronda:
    /// espera 2 segundos, instancia el sandwich final en la posición indicada, resetea los previews y reinicia los contadores para una nueva ronda.
    /// </summary>
    private IEnumerator CompleteRoundCoroutine()
    {
        Debug.Log("Ronda completada. Esperando 2 segundos antes de reiniciar la ronda.");
        yield return new WaitForSeconds(2f);

        // Instanciar el prefab del sandwich en el punto indicado.
        if (sandwichPrefab != null && sandwichSpawnPoint != null)
        {
            Instantiate(sandwichPrefab, sandwichSpawnPoint.position, sandwichSpawnPoint.rotation);
            Debug.Log("Sandwich final instanciado en la posición indicada.");
        }
        else
        {
            Debug.LogWarning("No se ha asignado sandwichPrefab o sandwichSpawnPoint en SandwichManager.");
        }

        // Resetear la preview del sandwich completo.
        if (completeSandwich != null)
        {
            completeSandwich.SetActive(false);
        }

        // Desactivar las previews de ingredientes.
        if (tomatoPreview != null) tomatoPreview.SetActive(false);
        if (lettucePreview != null) lettucePreview.SetActive(false);
        if (hamPreview != null) hamPreview.SetActive(false);
        if (cheesePreview != null) cheesePreview.SetActive(false);
        if (breadPreview != null) breadPreview.SetActive(false);

        // Reiniciar las variables booleanas de los ingredientes.
        tomatoAdded = false;
        lettuceAdded = false;
        hamAdded = false;
        cheeseAdded = false;
        breadAdded = false;

        // Reiniciar los contadores en ItemCollector (se asume que se añadió el método ResetItemCounts).
        ResetItemCountsInCollector();

        // Reiniciar la variable de la ronda para permitir una nueva recolección.
        roundCompleted = false;
        Debug.Log("Ronda reiniciada. Se pueden recoger nuevos ingredientes.");
    }

    /// <summary>
    /// Llama al método de reset de contadores en ItemCollector.
    /// Asegúrate de haber implementado ResetItemCounts() en el script ItemCollector.
    /// </summary>
    private void ResetItemCountsInCollector()
    {
        ItemCollector.ResetItemCounts();
    }
}
