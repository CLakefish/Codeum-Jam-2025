using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Launchable : Resettable
{
    [SerializeField] protected UnityEvent onLaunch;

    [Header("Physics")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] private bool isKinematic = false;

    [Header("Launcing Parameters")]
    [SerializeField] private float launchForce;
    [SerializeField] private float heightForce;
    [SerializeField] private float gravityForce = 48.0f;

    private Vector3    startPosition;
    private Quaternion startRotation;

    private void Awake() {
        rb.freezeRotation = true;
        rb.isKinematic    = isKinematic;

        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void FixedUpdate() {
        if (rb.isKinematic) return;
        rb.velocity -= gravityForce * Time.fixedDeltaTime * Vector3.up;
    }

    public override void OnReset() {
        rb.isKinematic = isKinematic;

        if (!rb.isKinematic)
        {
            rb.velocity       = Vector3.zero;
            rb.freezeRotation = true;
        }

        rb.position = startPosition;
        rb.rotation = startRotation;

        base.OnReset();
    }

    public virtual void Launch(Vector3 position) {
        onLaunch?.Invoke();
        rb.freezeRotation = false;
        rb.isKinematic    = false;

        Vector3 dir = (transform.position - position).normalized;
        rb.velocity = dir * launchForce + (Vector3.up * heightForce);

        rb.AddTorque(dir, ForceMode.VelocityChange);
    }
}
