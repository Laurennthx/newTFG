using UnityEngine;

public class SpawnRotationFixer : MonoBehaviour
{
    void Start()
    {
        // Get the current rotation angles
        Vector3 currentEuler = transform.rotation.eulerAngles;
        // Set the X angle to -90 while keeping Y and Z unchanged
        transform.rotation = Quaternion.Euler(-90f, currentEuler.y, currentEuler.z);
    }
}
