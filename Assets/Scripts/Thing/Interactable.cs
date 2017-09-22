using UnityEngine;

public class Interactable : MonoBehaviour {

    public Transform interactionTransform;	// The transform from where we interact in case you want to offset it
    public float radius = 3;                // How close do we need to be to interact?
    
    void Start () {
        if (interactionTransform == null) {
            interactionTransform = transform;
        }
    }

    public void InteractFrom (float distance) {
        if (distance < radius) {
            Interact ();
        }
    }

    public virtual void Interact () {
        // This method is meant to be overwritten
        Debug.Log("Interacting with " + transform.name);
    }
    
    // Draw our radius in the editor
    void OnDrawGizmosSelected () {
        if (interactionTransform == null)
            interactionTransform = transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (interactionTransform.position, radius);
    }
}
