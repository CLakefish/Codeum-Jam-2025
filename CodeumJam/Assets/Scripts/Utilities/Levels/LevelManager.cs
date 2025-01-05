using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] public CutsceneManager       cutsceneManager;
    [SerializeField] public LevelScriptableObject levelScriptableObject;

    private readonly HashSet<Resettable> resettables           = new();
    private readonly HashSet<PointOfInterest> pointsOfInterest = new();
    private bool levelComplete = false;

    public int TotalPointsOfInterest {
        get {
            return pointsOfInterest.Count;
        }
    }

    private int totalActive;
    public int TotalActive => totalActive;

    private void OnEnable() {
        if (Instance == null) Instance = this;
        GetReferences();
    }

    private void Start() {
        cutsceneManager.TriggerCutscene(CutsceneManager.CutsceneType.Intro);
        cutsceneManager.ChangeScene += () => levelScriptableObject.ChangeScene();

        totalActive = TotalPointsOfInterest;
    }

    private void OnDrawGizmos()
    {
        GetReferences();

        Gizmos.color = Color.green;
        foreach (var point in pointsOfInterest) {
            Gizmos.DrawRay(point.transform.position, Vector3.up * 10);
        }

        Gizmos.color = Color.yellow;
        foreach (var resettable in resettables)
        {
            Gizmos.DrawRay(resettable.transform.position, Vector3.up * 5);
        }
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

        totalActive = TotalPointsOfInterest;

        foreach (var r in resettables) {
            r.gameObject.SetActive(true);
            r.OnReset();
        }
    }

    private void CheckComplete() {
        int total = 0;

        foreach (var p in pointsOfInterest) {
            if (!p.HasTriggered) continue;

            total++;
        }

        totalActive = TotalPointsOfInterest - total;

        if (totalActive > 0) return;

        levelComplete = true;
        Player.Instance.Explode();
        cutsceneManager.TriggerCutscene(CutsceneManager.CutsceneType.End);
    }
}
