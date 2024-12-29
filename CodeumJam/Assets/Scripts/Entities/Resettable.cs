using UnityEngine;
using UnityEngine.Events;

public class Resettable : MonoBehaviour
{
    [Header("On Reset Event")]
    [SerializeField] protected UnityEvent onReset;

    public virtual void OnReset() => onReset?.Invoke();
}
