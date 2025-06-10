using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("El tipo o categoría de este objeto (ej.: 'Tomato', 'Cheese', etc.)")]
    [SerializeField] private string itemType = "DefaultItem";

    private float spawnTime;
    private bool isCorrect;

    void Start()
    {
        // Guardamos el momento de aparición
        spawnTime = Time.time;

        // Determinamos si este objeto es correcto o no
        isCorrect = IsCorrectItem();

        // Registramos spawn (sólo el flag isCorrect)
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.RegisterSpawn(isCorrect);
    }

    public void Collect()
    {
        // Tiempo de reacción
        float reactionTime = Time.time - spawnTime;

        // Log de reacción y caught
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogReactionTime(itemType, reactionTime);
            AnalyticsManager.Instance.RegisterCaught(isCorrect);
        }

        Debug.Log("Tiempo de reacción registrado: " + reactionTime);
        ItemCollector.AddItem(itemType);

        // Sonidos y destrucción
        if (isCorrect)
            AudioManager.Instance.PlaySuccessSound(transform.position);
        else
            AudioManager.Instance.PlayErrorSound(transform.position);

        var dwp = GetComponent<DestroyWithParticles>();
        if (dwp != null)
            dwp.PlayEffectAndDestroy();
        else
            Destroy(gameObject);
    }

    private bool IsCorrectItem()
    {
        return itemType == "Tomato" ||
               itemType == "Lettuce" ||
               itemType == "Ham" ||
               itemType == "Cheese" ||
               itemType == "Bread";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("playerHand"))
            Collect();
    }
}
