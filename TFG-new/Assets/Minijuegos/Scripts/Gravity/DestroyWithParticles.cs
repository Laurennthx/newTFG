using UnityEngine;

public class DestroyWithParticles : MonoBehaviour
{
    [Header("Particle Effect")]
    [Tooltip("GameObject que contiene el sistema de part�culas. Debe estar inactivo en la escena.")]
    public GameObject destructionParticlesObject;

    // Flag para evitar ejecuciones m�ltiples del efecto
    private bool hasTriggered = false;

    /// <summary>
    /// Activa el GameObject de part�culas, reproduce los efectos y luego destruye el objeto original.
    /// Se asegura de ejecutar la acci�n solo una vez.
    /// </summary>
    public void PlayEffectAndDestroy()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;  // Marcar que ya se activ� el efecto

        if (destructionParticlesObject != null)
        {
            // Desvincular el objeto de part�culas del objeto actual para que no se destruya junto a �l.
            destructionParticlesObject.transform.parent = null;
            // Activar el GameObject de part�culas (debe estar inactivo en el Inspector)
            destructionParticlesObject.SetActive(true);        
        }

        // Destruir el objeto original de inmediato.
        Destroy(gameObject);
        Destroy(destructionParticlesObject,1f);

    }
}
