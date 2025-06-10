using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorCapsule : MonoBehaviour
{
    // Toma el color del material automáticamente
    public Color capsuleColor => GetComponent<Renderer>().sharedMaterial.color;
}
