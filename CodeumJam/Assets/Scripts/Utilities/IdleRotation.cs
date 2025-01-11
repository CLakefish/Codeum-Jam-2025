using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleRotation : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float intensity;

    private void Update()
    {
        transform.localEulerAngles = new Vector3(0, 0, Mathf.Sin(Time.unscaledTime * speed) * intensity);
    }
}
