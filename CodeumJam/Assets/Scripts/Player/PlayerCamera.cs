using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerInput PlayerInput;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform viewModel;

    [Header("Collisions")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float spherecastRadius;

    [Header("Camera References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform pivot;

    [Header("Interpolation")]
    [SerializeField] private float rotationSmoothing;
    [SerializeField] private float viewModelSmoothing;
    [SerializeField] private float distance, yOffset;

    private Vector2 targetRotation;
    private Vector2 currentRotation;
    private Vector2 rotationVelocity;
    private float viewModelVel;

    private void Start() {
        PlayerInput.MouseLock = true;
    }

    void Update()
    {
        targetRotation.y += PlayerInput.AlteredMouseDelta.x;
        targetRotation.x -= PlayerInput.AlteredMouseDelta.y;
        targetRotation.x = Mathf.Clamp(targetRotation.x, -89f, 89f);

        currentRotation = new Vector2(
            Mathf.SmoothDampAngle(currentRotation.x, targetRotation.x, ref rotationVelocity.x, rotationSmoothing),
            Mathf.SmoothDampAngle(currentRotation.y, targetRotation.y, ref rotationVelocity.y, rotationSmoothing));

        pivot.eulerAngles = new Vector3(currentRotation.x, currentRotation.y, 0f);

        float maxDist = distance;

        if (Physics.SphereCast(pivot.position, spherecastRadius, -pivot.forward, out RaycastHit hit, distance, groundLayer)) {
            maxDist = (hit.point - pivot.position).magnitude - spherecastRadius;
        }

        cam.transform.localPosition = Vector3.forward * -(maxDist - spherecastRadius);

        float yAngleOffset = Mathf.Atan2(cam.transform.forward.z, cam.transform.forward.x) * Mathf.Rad2Deg - 90f;

        if (PlayerInput.Inputting) {
            float newAngle = (Mathf.Atan2(PlayerInput.Inputs.normalized.x, PlayerInput.Inputs.normalized.y) * Mathf.Rad2Deg) - yAngleOffset;
            float interpAngle = Mathf.SmoothDampAngle(viewModel.transform.localEulerAngles.y, newAngle, ref viewModelVel, viewModelSmoothing);
            viewModel.transform.localEulerAngles = new Vector3(0, interpAngle, 0);
        }

        Vector3 endPos = Vector3.up * yOffset + rb.transform.position;
        pivot.position = endPos;
    }
}
