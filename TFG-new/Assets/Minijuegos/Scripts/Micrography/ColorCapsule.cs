using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorCapsule : MonoBehaviour
{
    // Toma el color del material autom�ticamente
    public Color capsuleColor => GetComponent<Renderer>().sharedMaterial.color;
}
