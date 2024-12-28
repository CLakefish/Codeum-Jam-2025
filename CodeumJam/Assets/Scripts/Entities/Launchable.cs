using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launchable : MonoBehaviour
{
    [Header("Launcing Parameters")]
    [SerializeField] private float launchForce;
    [SerializeField] private float heightForce;
    [SerializeField] private float gravityForce;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private bool isLaunched = false;

    public void Launch(Vector3 position)
    {
        isLaunched = true;
        rb.freezeRotation = false;

        Vector3 dir = (transform.position - position).normalized;
        rb.velocity = dir * launchForce + (Vector3.up * heightForce);

        rb.AddTorque(dir, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (!isLaunched) return;

        rb.velocity -= Vector3.up * gravityForce * Time.fixedDeltaTime;
    }
}
