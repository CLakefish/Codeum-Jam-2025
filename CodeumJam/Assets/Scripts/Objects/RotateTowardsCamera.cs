using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{
    [SerializeField] private float idleZRotateSpeed;
    private float zAngle = 0;

    private void FixedUpdate()
    {
        zAngle += Time.deltaTime * idleZRotateSpeed;
        transform.forward          = Camera.main.transform.position - transform.position;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + zAngle);
    }
}
