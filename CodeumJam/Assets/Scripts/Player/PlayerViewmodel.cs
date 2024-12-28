using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewmodel : Player.PlayerComponent
{
    [Header("Viewmodels")]
    [SerializeField] private GameObject snowMan;
    [SerializeField] private GameObject snowBall;

    [Header("Smoothing")]
    [SerializeField] private float viewmodelSmoothing;

    public GameObject Viewmodel {
        get {
            return IsSnowman ? snowMan : snowBall;
        }
    }

    public bool IsSnowman => isSnowman;

    private bool isSnowman     = true;
    private float viewmodelVel = 0;

    private Quaternion prevRollRotation;

    private void Update()
    {
        if (!IsSnowman)
        {
            rb.angularVelocity = Quaternion.Euler(0, 90, 0) * rb.velocity;
            return;
        }

        float yAngleOffset = Mathf.Atan2(Camera.transform.forward.z, Camera.transform.forward.x) * Mathf.Rad2Deg - 90f;

        if (PlayerInput.Inputting) {
            float newAngle    = (Mathf.Atan2(PlayerInput.Inputs.normalized.x, PlayerInput.Inputs.normalized.y) * Mathf.Rad2Deg) - yAngleOffset;
            float interpAngle = Mathf.SmoothDampAngle(Viewmodel.transform.localEulerAngles.y, newAngle, ref viewmodelVel, viewmodelSmoothing);
            Viewmodel.transform.localEulerAngles = new Vector3(0, interpAngle, 0);
        }
    }

    public void Rolling(bool active)
    {
        if (!active) {
            rb.angularVelocity = Vector3.zero;
            rb.rotation        = prevRollRotation;
            rb.transform.rotation = Quaternion.identity;

            Physics.SyncTransforms();
        }
        else {
            prevRollRotation = rb.rotation;
        }

        rb.freezeRotation = !active;
        isSnowman         = !active;

        snowBall.SetActive(active);
        snowMan.SetActive(!active);
    }
}
