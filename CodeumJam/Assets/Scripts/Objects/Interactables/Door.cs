using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : Resettable
{
    [SerializeField] private UnityEvent onTrigger;

    [Header("Keys")]
    [SerializeField] private List<Key> keys = new();
    [SerializeField] private BoxCollider col;
    [SerializeField] private MeshRenderer rend;

    private void CheckCompletion()
    {
        foreach (var key in keys) {
            if (!key.Collected) return;
        }

        onTrigger?.Invoke();
        col.enabled  = false;
        rend.enabled = false;

        foreach (var key in keys) {
            key.OnUnlock();
        }
    }

    public override void OnReset()
    {
        col.enabled  = true;
        rend.enabled = true;

        base.OnReset();
    }

    private void OnTriggerEnter(Collider other) {
        CheckCompletion();
    }

    private void OnTriggerStay(Collider other) {
        CheckCompletion();
    }

    private void OnDrawGizmos()
    {
        if (keys.Count <= 0) return;

        foreach (var key in keys)
        {
            Gizmos.DrawLine(transform.position, key.transform.position);
        }
    }
}
