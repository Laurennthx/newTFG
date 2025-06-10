using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Tooltip("Amplitude of the vertical floating motion.")]
    [SerializeField] private float amplitude = 0.5f;

    [Tooltip("Frequency of the floating motion (cycles per second).")]
    [SerializeField] private float frequency = 1f;

    // Starting position of the object
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        // Calculate vertical offset using a sine wave
        float yOffset = amplitude * Mathf.Sin(Time.time * frequency * 2 * Mathf.PI);

        // Set the new position (only modify the Y coordinate)
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
    }
}
