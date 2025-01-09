using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Key : Resettable
{
    [SerializeField] private UnityEvent collectEvent;

    [Header("Sizing")]
    [SerializeField] private float standardSize;
    [SerializeField] private float collectedSize;
    [SerializeField] private float sizeSmoothing;

    [Header("Positions")]
    [SerializeField] private float positionSmoothing;
    [SerializeField] private float maxDistance;
    [SerializeField] private float bobbingSpeed, bobbingMagnitude;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed;

    [Header("Debugging")]
    [SerializeField] private bool collected = false;

    public bool Collected => collected;

    private Transform following;
    private Vector3 startPosition;
    private Vector3 posVel, sizeVel;

    private void Awake() {
        startPosition = transform.position;
    }

    private void Update() {
        if (following != null) {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, Vector3.one * collectedSize, ref sizeVel, sizeSmoothing);

            if (Vector3.Distance(transform.position, following.position) > 1) {
                transform.position = Vector3.SmoothDamp(transform.position, following.position, ref posVel, positionSmoothing);
            }

            Vector3 offset     = transform.position - following.position;
            offset             = Vector3.ClampMagnitude(offset, maxDistance);
            transform.position = offset + following.position;
        }
        else
        {
            transform.position = startPosition + new Vector3(0, Mathf.Sin(Time.time * bobbingSpeed) * bobbingMagnitude, 0);
        }
    }

    private void FixedUpdate() {
        transform.localEulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + (Time.fixedDeltaTime * rotationSpeed), transform.eulerAngles.z);
    }

    public void OnUnlock()
    {
        Debug.Log("unlocked");
        following = null;
        transform.position   = startPosition;
        transform.localScale = Vector3.one * standardSize;
        collectEvent?.Invoke();
        gameObject.SetActive(false);
    }

    public override void OnReset() {
        gameObject.SetActive(true);
        transform.position   = startPosition;
        transform.localScale = Vector3.one * standardSize;
        collected = false;
        following = null;

        base.OnReset();
    }

    private void OnTriggerEnter(Collider other) {
        if (collected) return;

        collectEvent?.Invoke();

        following = other.transform;
        collected = true;
    }
}
