using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField] private float launchForce;
    [SerializeField] private float heightForce;
    [SerializeField] private Rigidbody rb;

    public void Launch(Vector3 position)
    {
        Debug.Log("hit!");
        rb.freezeRotation = false;

        Vector3 dir = (transform.position - position).normalized;
        rb.velocity = dir * launchForce + (Vector3.up * heightForce);

        rb.AddTorque(dir, ForceMode.VelocityChange);
    }
}
