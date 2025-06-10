using UnityEngine;
using System.Collections.Generic;

public enum MovementDirection
{
    ToLeft,
    ToRight
}

public class PlatformSpawner : MonoBehaviour
{
    [Header("Objects and Timing")]
    [Tooltip("Lista de prefabs a generar, en el orden exacto en que deben aparecer.")]
    [SerializeField] private GameObject[] objectsToSpawn;

    [Tooltip("Lista de tiempos de spawn (en segundos). Debe coincidir con la cantidad de objetos.")]
    [SerializeField] private float[] spawnTimes;

    [Tooltip("Transform usado como el punto de aparición.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Movement Settings")]
    [Tooltip("Velocidad de movimiento en el eje Z.")]
    [SerializeField] private float movementSpeed = 1.0f;

    [Tooltip("Dirección de movimiento: ToLeft o ToRight.")]
    [SerializeField] private MovementDirection movementDirection = MovementDirection.ToRight;

    [Tooltip("Transform que contiene el collider con el que, al colisionar, se destruirán los objetos generados.")]
    [SerializeField] private Transform destinationPoint;

    [Header("Loop Settings")]
    [Tooltip("Número de rondas para repetir la secuencia de generación.")]
    [SerializeField] private int numberOfRounds = 1;

    [Header("Final Round Actions")]
    [Tooltip("Objeto a activar cuando se completen todas las rondas (por ejemplo, un panel final).")]
    [SerializeField] private GameObject objectToActivate;

    [Tooltip("Objeto a desactivar cuando se completen todas las rondas.")]
    [SerializeField] private GameObject objectToDeactivate;

    [Header("Hand Data Logger")]
    [Tooltip("Referencia al componente HandDataLogger para iniciar y detener la grabación de datos de mano.")]
    [SerializeField] private HandDataLogger handLogger;

    // Variables de control para la generación
    private bool spawningPaused = true;
    private int currentIndex = 0;
    private float timeElapsed = 0f;
    private int currentRound = 1;
    private bool finalActionDone = false;

    // Lista para almacenar las instancias generadas y actualizar su movimiento
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        // Inicia pausado; esperamos a que el usuario pulse Play
        spawningPaused = true;
    }

    private void Update()
    {
        if (spawningPaused)
            return;

        timeElapsed += Time.deltaTime;

        // -- Spawning --
        if (currentIndex < objectsToSpawn.Length && timeElapsed >= spawnTimes[currentIndex])
        {
            GameObject newObj = SpawnObject(objectsToSpawn[currentIndex]);
            spawnedObjects.Add(newObj);
            currentIndex++;
        }

        // Si ya se generó toda la ronda...
        if (currentIndex >= objectsToSpawn.Length)
        {
            if (currentRound < numberOfRounds)
            {
                // Siguiente ronda
                currentRound++;
                currentIndex = 0;
                timeElapsed = 0f;
            }
            else if (!finalActionDone)
            {
                // … detiene el logger de mano …
                handLogger?.StopLogging();

                // ───> Aquí llamamos a AnalyticsManager para finalizar y mostrar UI
                if (AnalyticsManager.Instance != null)
                    AnalyticsManager.Instance.EndSession();

                // Actions finales de tu escena
                if (objectToActivate != null) objectToActivate.SetActive(true);
                if (objectToDeactivate != null) objectToDeactivate.SetActive(false);

                finalActionDone = true;
                spawningPaused = true;
            }
        }

        UpdateSpawnedObjectsMovement();
    }

    private GameObject SpawnObject(GameObject prefab)
    {
        Vector3 spawnPos = spawnPoint.position;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezePositionX
                           | RigidbodyConstraints.FreezePositionY
                           | RigidbodyConstraints.FreezeRotation;
        }

        var fc = obj.GetComponent<FoodCollision>() ?? obj.AddComponent<FoodCollision>();
        fc.destroyOnCollisionWith = destinationPoint.gameObject;

        return obj;
    }

    private void UpdateSpawnedObjectsMovement()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            var obj = spawnedObjects[i];
            if (obj == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            Vector3 pos = obj.transform.position;
            float dirMul = movementDirection == MovementDirection.ToLeft ? -1f : 1f;
            pos.z += dirMul * movementSpeed * Time.deltaTime;
            pos.x = spawnPoint.position.x;
            pos.y = spawnPoint.position.y;
            obj.transform.position = pos;
        }
    }

    /// <summary>
    /// Llamar desde el botón Play (o tu UI) para comenzar la generación y el log de mano.
    /// </summary>
    public void ResumeSpawning()
    {
        spawningPaused = false;
        currentIndex = 0;
        timeElapsed = 0f;
        currentRound = 1;
        finalActionDone = false;

        handLogger?.StartLogging();
    }

    /// <summary>
    /// Llamar desde el botón Pause (o tu UI) para pausar la generación.
    /// </summary>
    public void PauseSpawning()
    {
        spawningPaused = true;
        // Si quieres, también puedes parar el logger aquí:
        // handLogger?.StopLogging();
    }
}
