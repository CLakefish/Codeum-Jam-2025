using UnityEngine;
using UnityEngine.Events;

public class Resettable : MonoBehaviour
{
    [SerializeField] protected UnityEvent onReset;

    public virtual void OnReset() => onReset?.Invoke();
}
