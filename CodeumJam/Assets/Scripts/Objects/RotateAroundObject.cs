using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundObject : MonoBehaviour
{
    [SerializeField] private Transform obj;
    [SerializeField] private float yOffset;
    [SerializeField] private float dampY;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float radius;
    private float yVel;

    private void LateUpdate()
    {
        Vector3 pos = obj.transform.position + (new Vector3(Mathf.Cos(Time.time * rotateSpeed), 0, Mathf.Sin(Time.time * rotateSpeed)) * radius);
        transform.position = new Vector3(pos.x, Mathf.SmoothDamp(transform.position.y, yOffset, ref yVel, rotateSpeed), pos.z);

        transform.LookAt(obj);
    }
}
