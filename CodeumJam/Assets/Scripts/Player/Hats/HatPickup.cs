using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatPickup : MonoBehaviour
{
    [SerializeField] private Hat hat;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bobSpeed;
    [SerializeField] private float bobIntensity;

    private Vector3 startPos;
    private float yAngle = 0;

    private void Awake()
    {
        startPos = transform.position;

        GameObject obj = Instantiate(hat.HatObject, transform);
        obj.transform.localEulerAngles = new Vector3(0, 15, 15);
        obj.transform.localPosition  -= Vector3.up;
        obj.transform.localScale      = Vector3.one * 2;
    }

    private void Update()
    {
        transform.position = new Vector3(startPos.x, startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobIntensity, startPos.z);
        yAngle += Time.deltaTime * rotationSpeed;
        transform.localEulerAngles = new Vector3(0, yAngle, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerViewmodel>(out PlayerViewmodel player))
        {
            player.EquipHat(hat.ID);
        }
    }
}
