using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFollow : MonoBehaviour
{
    private Transform camTransform;

    private void Start() {
        if (Camera.main != null) camTransform = Camera.main.transform;
    }

    private void LateUpdate() {
        Vector3 camPos     = camTransform.position;
        transform.position = new Vector3(camPos.x, transform.position.y, camPos.z);
    }
}
