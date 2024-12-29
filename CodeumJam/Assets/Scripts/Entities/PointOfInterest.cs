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
}
