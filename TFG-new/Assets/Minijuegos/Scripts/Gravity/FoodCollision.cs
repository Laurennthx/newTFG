using UnityEngine;

/// <summary>
/// Componente encargado de destruir el objeto al colisionar con el collider del objeto especificado.
/// Para que la detección de colisiones funcione correctamente:
/// - El objeto que tenga este script debe contar con un Collider (opcionalmente marcado como "Trigger") y, si se usan triggers, un Rigidbody.
/// - El objeto con el que se detecta la colisión (definido en destroyOnCollisionWith) debe tener un Collider activo.
/// </summary>
public class FoodCollision : MonoBehaviour
{
    [Tooltip("Objeto con el collider con el que, al colisionar, se destruirá este objeto.")]
    public GameObject destroyOnCollisionWith;

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto colisionado es el destino definido
        if (other.gameObject == destroyOnCollisionWith)
        {
            Destroy(gameObject);
        }
    }

    // Si prefieres usar colisiones físicas (no triggers), puedes usar OnCollisionEnter:
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == destroyOnCollisionWith)
        {
            Destroy(gameObject);
        }
    }
    */
}
