using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public enum CutsceneType { 
        Intro,
        End,
    }

    [Header("Camera")]
    [SerializeField] private List<Transform> introPositions;
    [SerializeField] private Transform endPosition;
    [SerializeField] private GameObject player;

    [Header("Intro")]
    [SerializeField] private string levelName;
    [SerializeField] private string levelDescription;

    [Header("End")]
    [SerializeField] private float endPause;

    [Header("Interpolation")]
    [SerializeField] private float interpolationSpeed;
    [SerializeField] private float positionThreshold;

    private CutsceneType cutsceneType;
    private Coroutine cutscene;

    public System.Action IntroCutsceneTrigger;
    public System.Action EndCutsceneTrigger;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cutscene != null) {
            StopCoroutine(cutscene);
            EnablePlayer(true);

            switch (cutsceneType) { 
                case CutsceneType.Intro:
                    IntroCutsceneTrigger?.Invoke();
                    break;

                case CutsceneType.End:
                    IntroCutsceneTrigger?.Invoke();
                    break;
            }

            Transform viewCamera = Camera.main.transform;
            viewCamera.transform.localPosition = Vector3.zero;
            viewCamera.transform.localRotation = Quaternion.identity;

            cutscene = null;
        }
    }

    private void OnDrawGizmos()
    {
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
        Player p = player.GetComponent<Player>();
        p.AllowMovement(allow);
    }

    public void TriggerCutscene(CutsceneType cutsceneType)
    {
        if (cutscene != null) StopCoroutine(cutscene);
        this.cutsceneType = cutsceneType;

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
        Transform viewCamera = Camera.main.transform;
        Transform camParent  = Camera.main.transform.parent;

        viewCamera.transform.position = introPositions[0].position;
        viewCamera.transform.forward  = introPositions[0].forward;

        Vector3 positionVelocity = Vector3.zero;
        Vector3 rotationVelocity = Vector3.zero;

        EnablePlayer(false);
        IntroCutsceneTrigger?.Invoke();

        for (int i = 0; i < introPositions.Count; ++i) {
            while (Vector3.Distance(viewCamera.transform.position, introPositions[i].position) > positionThreshold) {
                viewCamera.transform.position = Vector3.SmoothDamp(viewCamera.transform.position, introPositions[i].position, ref positionVelocity, interpolationSpeed);
                viewCamera.transform.forward  = Vector3.SmoothDamp(viewCamera.transform.forward,  introPositions[i].forward,  ref rotationVelocity, interpolationSpeed);
                yield return null;
            }

            viewCamera.transform.position = introPositions[i].position;
            viewCamera.transform.forward  = introPositions[i].forward;
        }

        while (Vector3.Distance(viewCamera.transform.position, camParent.position) > 0.01f) {
            viewCamera.transform.position = Vector3.SmoothDamp(viewCamera.transform.position, camParent.position, ref positionVelocity, interpolationSpeed);
            viewCamera.transform.forward  = Vector3.SmoothDamp(viewCamera.transform.forward,  camParent.forward,  ref rotationVelocity, interpolationSpeed);
            yield return null;
        }

        EnablePlayer(true);
    }

    private IEnumerator EndCutscene() {
        Transform viewCamera = Camera.main.transform;
        EnablePlayer(false);

        Vector3 positionVelocity = Vector3.zero;
        Vector3 rotationVelocity = Vector3.zero;

        yield return new WaitForSeconds(endPause);

        while (Vector3.Distance(viewCamera.transform.position, endPosition.position) > positionThreshold) {
            viewCamera.transform.position = Vector3.SmoothDamp(viewCamera.transform.position, endPosition.position, ref positionVelocity, interpolationSpeed);
            viewCamera.transform.forward  = Vector3.SmoothDamp(viewCamera.transform.forward,  endPosition.forward,  ref rotationVelocity, interpolationSpeed);
            yield return null;
        }

        EndCutsceneTrigger?.Invoke();
    }
}
