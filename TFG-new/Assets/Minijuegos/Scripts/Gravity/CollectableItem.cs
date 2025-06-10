using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("El tipo o categor�a de este objeto (ej.: 'Tomato', 'Cheese', etc.)")]
    [SerializeField] private string itemType = "DefaultItem";

    private float spawnTime;
    private bool isCorrect;

    void Start()
    {
        // Guardamos el momento de aparici�n
        spawnTime = Time.time;

        // Determinamos si este objeto es correcto o no
        isCorrect = IsCorrectItem();

        // Registramos spawn (s�lo el flag isCorrect)
        if (AnalyticsManager.Instance != null)
            AnalyticsManager.Instance.RegisterSpawn(isCorrect);
    }

    public void Collect()
    {
        // Tiempo de reacci�n
        float reactionTime = Time.time - spawnTime;

        // Log de reacci�n y caught
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogReactionTime(itemType, reactionTime);
            AnalyticsManager.Instance.RegisterCaught(isCorrect);
        }

        Debug.Log("Tiempo de reacci�n registrado: " + reactionTime);
        ItemCollector.AddItem(itemType);

        // Sonidos y destrucci�n
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
