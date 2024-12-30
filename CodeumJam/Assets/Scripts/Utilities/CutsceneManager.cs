using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public enum CutsceneType { 
        Intro,
        End,
    }

    public static CutsceneManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private List<Transform> introPositions;
    [SerializeField] private Transform endPosition;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Player player;

    [Header("Intro")]
    [SerializeField] private string levelName;
    [SerializeField] private string levelDescription;

    [Header("End")]
    [SerializeField] private float endPause;

    [Header("Interpolation")]
    [SerializeField] private float interpolationSpeed;
    [SerializeField] private float positionThreshold;

    public event System.Action IntroCutsceneTrigger;
    public event System.Action EndCutsceneTrigger;
    public event System.Action ChangeScene;

    public bool CutscenePlaying;

    private CutsceneType cutsceneType;
    private Coroutine    cutscene;

    private void OnEnable() {
        if (Instance == null) Instance = this;
    }

    private void Awake() {
        playerCamera.transform.position = introPositions[0].position;
        playerCamera.transform.forward  = introPositions[0].forward;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && cutscene != null) {
            CutscenePlaying = false;

            switch (cutsceneType) { 
                case CutsceneType.Intro:
                    IntroCutsceneTrigger?.Invoke();
                    StopCoroutine(cutscene);
                    EnablePlayer(true);

                    Transform viewCamera = Camera.main.transform;
                    viewCamera.transform.localPosition = Vector3.zero;
                    viewCamera.transform.localRotation = Quaternion.identity;
                    break;

                case CutsceneType.End:
                    EndCutsceneTrigger?.Invoke();
                    ChangeScene?.Invoke();
                    break;
            }

            cutscene = null;
        }
    }

    private void OnDrawGizmos() {
        if (introPositions != null) {
            for (int i = 0; i < introPositions.Count; ++i) {
                if (i > 0) {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(introPositions[i - 1].position, introPositions[i].position);
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(introPositions[i].position, 0.5f);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(introPositions[i].position, introPositions[i].forward * 5);
            }
        }

        if (endPosition != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPosition.position, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(endPosition.position, endPosition.forward * 5);
        }
    }

    private void EnablePlayer(bool allow) {
        player.AllowMovement(allow);
        player.CutsceneCam(allow);
    }

    public void TriggerCutscene(CutsceneType cutsceneType) {
        if (cutscene != null) StopCoroutine(cutscene);
        this.cutsceneType = cutsceneType;
        EnablePlayer(false);
        CutscenePlaying = true;

        switch (cutsceneType) {
            case CutsceneType.Intro:
                cutscene = StartCoroutine(IntroCutscene());
                break;

            case CutsceneType.End:
                cutscene = StartCoroutine(EndCutscene());
                break;
        }
    }

    private IEnumerator IntroCutscene() {
        playerCamera.transform.position = introPositions[0].position;
        playerCamera.transform.forward  = introPositions[0].forward;

        Vector3 positionVelocity = Vector3.zero;
        Vector3 rotationVelocity = Vector3.zero;

        IntroCutsceneTrigger?.Invoke();

        for (int i = 0; i < introPositions.Count; ++i) {
            while (Vector3.Distance(playerCamera.transform.position, introPositions[i].position) > positionThreshold) {
                playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, introPositions[i].position, ref positionVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
                playerCamera.transform.forward  = Vector3.SmoothDamp(playerCamera.transform.forward,  introPositions[i].forward,  ref rotationVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
                yield return null;
            }

            playerCamera.transform.position = introPositions[i].position;
            playerCamera.transform.forward  = introPositions[i].forward;
        }

        Transform camParent = player.GetCamera().Pivot;

        while (Vector3.Distance(playerCamera.transform.position, camParent.position) > 0.01f) {
            playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, camParent.position, ref positionVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            playerCamera.transform.forward  = Vector3.SmoothDamp(playerCamera.transform.forward,  camParent.forward,  ref rotationVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        CutscenePlaying = false;
        EnablePlayer(true);
    }

    private IEnumerator EndCutscene() {
        Vector3 positionVelocity = Vector3.zero;
        Vector3 rotationVelocity = Vector3.zero;

        yield return new WaitForSeconds(endPause);

        while (Vector3.Distance(playerCamera.transform.position, endPosition.position) > positionThreshold) {
            playerCamera.transform.position = Vector3.SmoothDamp(playerCamera.transform.position, endPosition.position, ref positionVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            playerCamera.transform.forward  = Vector3.SmoothDamp(playerCamera.transform.forward,  endPosition.forward,  ref rotationVelocity, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        EndCutsceneTrigger?.Invoke();
        CutscenePlaying = false;
    }
}
