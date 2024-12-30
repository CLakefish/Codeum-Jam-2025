using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewmodel : Player.PlayerComponent
{
    [Header("Viewmodels")]
    [SerializeField] private Transform viewmodel;
    [SerializeField] private GameObject snowMan;
    [SerializeField] private GameObject snowBall;
    [SerializeField] private GameObject shadow;

    [Header("Smoothing")]
    [SerializeField] private float viewmodelSmoothing;
    [SerializeField] private float snowManShadowSize;
    [SerializeField] private float snowBallShadowSize;

    public GameObject Viewmodel {
        get {
            return IsSnowman ? snowMan : snowBall;
        }
    }

    public bool IsSnowman => isSnowman;

    private const float Z_FIGHTING_PUSH = 0.015f;

    private bool isSnowman = true;
    public bool canRotate  = true;
    private float viewmodelVel = 0;

    private Quaternion prevRollRotation;

    private void Update()
    {
        if (Physics.Raycast(rb.transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, GroundLayer))
        {
            shadow.transform.position   = hit.point + (Vector3.up * Z_FIGHTING_PUSH);
            shadow.transform.forward    = hit.normal;
            shadow.transform.localScale = Vector3.one * (IsSnowman ? snowManShadowSize : snowBallShadowSize);
        }

        viewmodel.transform.position = rb.transform.position;

        if (!IsSnowman || !canRotate) return;

        float yAngleOffset = Mathf.Atan2(Camera.transform.forward.z, Camera.transform.forward.x) * Mathf.Rad2Deg - (90f - player.transform.eulerAngles.y);

        if (PlayerInput.Inputting) {
            float newAngle    = (Mathf.Atan2(PlayerInput.Inputs.normalized.x, PlayerInput.Inputs.normalized.y) * Mathf.Rad2Deg) - yAngleOffset;
            float interpAngle = Mathf.SmoothDampAngle(Viewmodel.transform.localEulerAngles.y, newAngle, ref viewmodelVel, viewmodelSmoothing);
            Viewmodel.transform.localEulerAngles = new Vector3(0, interpAngle, 0);
        }
    }

    private void FixedUpdate()
    {
        if (!IsSnowman) {
            rb.angularVelocity = Quaternion.Euler(0, 90, 0) * rb.velocity;
            snowBall.transform.rotation = rb.rotation;
        }
    }

    public void Rolling(bool active)
    {
        rb.freezeRotation = !active;
        isSnowman         = !active;

        if (!active) {
            rb.angularVelocity = Vector3.zero;
            rb.rotation        = prevRollRotation;

            Viewmodel.transform.localEulerAngles = new Vector3(0, Viewmodel.transform.localEulerAngles.y, 0);

            Physics.SyncTransforms();
        }
        else {
            prevRollRotation = rb.rotation;
        }

        snowBall.SetActive(active);
        snowMan.SetActive(!active);
    }
}
