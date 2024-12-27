using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private float verticalForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        if (other.transform.parent != null && other.transform.parent.TryGetComponent(out PlayerMovement movement)) {
            movement.Launch(Vector3.up * verticalForce);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.up * verticalForce);
    }
}
