using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private readonly HashSet<Resettable> resettables           = new();
    private readonly HashSet<PointOfInterest> pointsOfInterest = new();

    [Header("Camera")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private List<Transform> positions;
    [SerializeField] private GameObject player;

    [Header("Interpolation")]
    [SerializeField] private float interpolationSpeed;
    [SerializeField] private float positionThreshold;
    private bool levelComplete = false;

    private void OnEnable() {
        if (Instance == null) Instance = this;
        GetReferences();
    }

    private void Awake()
    {
        Application.targetFrameRate = 400;
        StartCoroutine(IntroCutscene(""));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAllCoroutines();
            StartCoroutine(IntroCutscene(""));
        }
    }

    private void OnDrawGizmos()
    {
        GetReferences();

        Gizmos.color = Color.green;
        foreach (var point in pointsOfInterest) {
            Gizmos.DrawRay(point.transform.position, Vector3.up * 10);
        }

        if (positions == null || positions.Count <= 1) return;

        for (int i = 0; i < positions.Count; ++i)
        {
            if (i > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(positions[i - 1].position, positions[i].position);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(positions[i].position, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(positions[i].position, positions[i].forward * 5);
        }
    }

    private IEnumerator IntroCutscene(string displayName)
    {
        viewCamera.gameObject.SetActive(true);

        Vector3 positionVelocity = Vector3.zero;
        Vector3 rotationVelocity = Vector3.zero;

        player.SetActive(false);

        viewCamera.transform.position = positions[0].position;
        viewCamera.transform.forward  = positions[0].forward;

        for (int i = 0; i < positions.Count; ++i)
        {
            while (Vector3.Distance(viewCamera.transform.position, positions[i].position) > positionThreshold)
            {
                viewCamera.transform.position = Vector3.SmoothDamp(viewCamera.transform.position, positions[i].position, ref positionVelocity, interpolationSpeed);
                viewCamera.transform.forward  = Vector3.SmoothDamp(viewCamera.transform.forward,   positions[i].forward, ref rotationVelocity, interpolationSpeed);
                yield return null;
            }

            viewCamera.transform.position = positions[i].position;
            viewCamera.transform.forward  = positions[i].forward;
        }

        player.SetActive(true);
        PlayerCamera cam           = player.GetComponentInChildren<PlayerCamera>();
        cam.canRotate = false;
        cam.PlayerMovement.enabled = false;

        Transform desiredPos = cam.Camera.transform;

        while (Vector3.Distance(viewCamera.transform.position, desiredPos.position) > 0.01f)
        {
            viewCamera.transform.position = Vector3.SmoothDamp(viewCamera.transform.position, desiredPos.position, ref positionVelocity, interpolationSpeed);
            viewCamera.transform.forward  = Vector3.SmoothDamp(viewCamera.transform.forward, desiredPos.forward, ref rotationVelocity, interpolationSpeed);
            yield return null;
        }

        viewCamera.gameObject.SetActive(false);
        cam.PlayerMovement.enabled = true;
        cam.canRotate = true;
        player.SetActive(true);
    }

    private void GetReferences() {
        if (resettables.Count != 0 || pointsOfInterest.Count != 0) {
            resettables.Clear();
            pointsOfInterest.Clear();
        }

        var resets = FindObjectsOfType<Resettable>();
        foreach (var r in resets) {
            resettables.Add(r);
        }

        var points = FindObjectsOfType<PointOfInterest>();
        foreach (var p in points) {
            p.SetCallback(() => { CheckComplete(); });
            pointsOfInterest.Add(p);
        }
    }

    public void ResetAll() {
        if (levelComplete) return;

        foreach (var r in resettables) {
            r.gameObject.SetActive(true);
            r.OnReset();
        }
    }

    private void CheckComplete() {
        foreach (var p in pointsOfInterest) {
            if (!p.HasTriggered) return;
        }

        levelComplete = true;

        Debug.Log("Level Completed!");
    }
}
