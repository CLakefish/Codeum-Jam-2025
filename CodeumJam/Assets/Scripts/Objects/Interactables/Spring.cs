using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private float verticalForce;

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        other.attachedRigidbody.velocity += Vector3.up * verticalForce;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.up * verticalForce);
    }
}
