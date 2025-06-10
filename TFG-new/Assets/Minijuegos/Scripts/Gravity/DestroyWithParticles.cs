using UnityEngine;

public class DestroyWithParticles : MonoBehaviour
{
    [Header("Particle Effect")]
    [Tooltip("GameObject que contiene el sistema de partículas. Debe estar inactivo en la escena.")]
    public GameObject destructionParticlesObject;

    // Flag para evitar ejecuciones múltiples del efecto
    private bool hasTriggered = false;

    /// <summary>
    /// Activa el GameObject de partículas, reproduce los efectos y luego destruye el objeto original.
    /// Se asegura de ejecutar la acción solo una vez.
    /// </summary>
    public void PlayEffectAndDestroy()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;  // Marcar que ya se activó el efecto

        if (destructionParticlesObject != null)
        {
            // Desvincular el objeto de partículas del objeto actual para que no se destruya junto a él.
            destructionParticlesObject.transform.parent = null;
            // Activar el GameObject de partículas (debe estar inactivo en el Inspector)
            destructionParticlesObject.SetActive(true);        
        }

        // Destruir el objeto original de inmediato.
        Destroy(gameObject);
        Destroy(destructionParticlesObject,1f);

    }
}
