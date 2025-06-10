using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PaletteAttachPoint : MonoBehaviour
{
    [Tooltip("Empty GameObject que marca el punto real de agarre.")]
    public Transform attachPoint;

    XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (attachPoint != null)
        {
            // Le decimos al GrabInteractable que use este Transform como punto de sujeción
            grabInteractable.attachTransform = attachPoint;
        }
        else
        {
            Debug.LogWarning($"[{name}] No has asignado ningún attachPoint.", this);
        }
    }

#if UNITY_EDITOR
    // Para que en el Editor ya se actualice al cambiar el campo
    void OnValidate()
    {
        if (grabInteractable == null)
            grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null && attachPoint != null)
            grabInteractable.attachTransform = attachPoint;
    }
#endif
}
