using UnityEngine;

public class PointOfInterest : Launchable
{
    [Header("Debugging")]
    [SerializeField] private bool hasTriggered = false;
    private System.Action callback;

    public bool HasTriggered => hasTriggered;

    public void SetCallback(System.Action managerEvent) => callback = managerEvent;
    public override void OnReset() {
        hasTriggered = false;
        base.OnReset();
    }

    public override void Launch(Vector3 position) {
        hasTriggered = true;
        callback?.Invoke();
        base.Launch(position);
    }

    private void OnCollisionEnter(Collision collision) {
        if (hasTriggered) return;

        if (collision.collider.TryGetComponent<Launchable>(out Launchable interest)) {
            interest.Launch(transform.position);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (hasTriggered) return;

        if (other.transform.GetComponent<Collider>().TryGetComponent<Launchable>(out Launchable interest)) {
            interest.Launch(transform.position);
        }
    }
}
