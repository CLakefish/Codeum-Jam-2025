using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private float boostForce;

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        other.attachedRigidbody.velocity += other.attachedRigidbody.velocity.normalized * (boostForce * Time.deltaTime);
    }
}
