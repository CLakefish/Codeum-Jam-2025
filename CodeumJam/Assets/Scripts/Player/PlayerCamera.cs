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
    [SerializeField] private float positionSmoothing;
    [SerializeField] private float viewModelSmoothing;
    [SerializeField] private float distance, yOffset;
    [SerializeField] private Vector3 boxSize;

    private Vector2 targetRotation;
    private Vector2 currentRotation;
    private Vector2 rotationVelocity;
    private Vector3 boxPosition;
    private float viewModelVel;
    private float yVel;

    private void Start() {
        boxPosition = rb.transform.position + (Vector3.up * (boxSize.y / 2.0f)) - Vector3.up;

        PlayerInput.MouseLock = true;
    }

    private void Update()
    {
        Vector3 dir = rb.transform.position - boxPosition;

        if (Mathf.Abs(dir.x) > boxSize.x / 2.0f) boxPosition.x = rb.transform.position.x - Mathf.Sign(dir.x) * (boxSize.x / 2.0f);

        if (Mathf.Abs(dir.y) > boxSize.y / 2.0f) {
            float desiredY = rb.transform.position.y - Mathf.Sign(dir.y) * (boxSize.y / 2.0f);
            boxPosition.y = Mathf.SmoothDamp(boxPosition.y, desiredY, ref yVel, positionSmoothing);
        }

        if (Mathf.Abs(dir.z) > boxSize.z / 2.0f) boxPosition.z = rb.transform.position.z - Mathf.Sign(dir.z) * (boxSize.z / 2.0f);

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

        Vector3 endPos = boxPosition - (Vector3.up * yOffset);
        pivot.position = endPos;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) boxPosition = rb.transform.position + (Vector3.up * (boxSize.y / 2.0f)) - Vector3.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxPosition, boxSize);
    }

    public void SetBoxBoundBottom()
    {
        Vector3 pos    = new(boxPosition.x, rb.transform.position.y, boxPosition.z);
        Vector3 offset = pos + (Vector3.up * (boxSize.y / 2.0f)) - Vector3.up;
        boxPosition    = new Vector3(offset.x, Mathf.SmoothDamp(boxPosition.y, offset.y, ref yVel, positionSmoothing), offset.z);
    }
}
