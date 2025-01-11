using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spring : MonoBehaviour
{
    [SerializeField] private float verticalForce;
    [SerializeField] UnityEvent onBounce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out PlayerMovement movement)) {
            movement.Launch(Vector3.up * verticalForce);
        }
        else {
            other.attachedRigidbody.velocity += Vector3.up * verticalForce;
        }

        onBounce?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, (Vector3.up * verticalForce).normalized);
    }
}
