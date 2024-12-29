using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Launchable : Resettable
{
    [Header("On Launch Event")]
    [SerializeField] protected UnityEvent onLaunch;

    [Header("Launcing Parameters")]
    [SerializeField] private float launchForce;
    [SerializeField] private float heightForce;
    [SerializeField] private float gravityForce = 48.0f;
    [SerializeField] protected Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake() {
        rb.freezeRotation = true;

        startPosition = rb.position;
        startRotation = rb.rotation;
    }

    private void FixedUpdate() {
        rb.velocity -= gravityForce * Time.fixedDeltaTime * Vector3.up;
    }

    public override void OnReset() {
        rb.velocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;

        rb.freezeRotation = true;
        base.OnReset();
    }

    public virtual void Launch(Vector3 position) {
        onLaunch?.Invoke();
        rb.freezeRotation = false;

        Vector3 dir = (transform.position - position).normalized;
        rb.velocity = dir * launchForce + (Vector3.up * heightForce);

        rb.AddTorque(dir, ForceMode.VelocityChange);
    }
}
