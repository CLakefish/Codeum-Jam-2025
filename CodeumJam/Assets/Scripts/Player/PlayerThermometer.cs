using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThermometer : MonoBehaviour
{
    [SerializeField] private Slider thermometer;
    [SerializeField] private RectTransform tickHolder;
    [SerializeField] private GameObject tickPrefab;
    [SerializeField] private float smoothTime;

    private float displayVel   = 0;

    private LevelManager levelManager;

    private void Start() {
        levelManager = LevelManager.Instance;
        thermometer.maxValue = levelManager.TotalPointsOfInterest; 

        for (int i = 0; i < thermometer.maxValue; ++i) {
            Instantiate(tickPrefab, tickHolder.transform);
        }
    }

    private void Update() {
        thermometer.value = Mathf.SmoothDamp(thermometer.value, Mathf.Max(levelManager.TotalActive, 0.45f), ref displayVel, smoothTime);
    }
}
