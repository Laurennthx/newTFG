using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [Tooltip("Falling speed of the object.")]
    [SerializeField] private float fallSpeed = 2.0f;

    [Tooltip("Rotation speed in degrees per second (x, y, z).")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 30, 0);

    void Update()
    {
        // Moves the object downward at a controlled speed.
       // transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // Applies a slow rotation to the object.
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);
    }
}
