using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThermometer : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private Slider thermometer;
    [SerializeField] private RectTransform tickHolder;
    [SerializeField] private GameObject tickPrefab;
    [SerializeField] private Color activeColor;
    [SerializeField] private TMPro.TMP_Text collectTotal;
    [SerializeField] private float smoothTime;

    [Header("Transitions")]
    [SerializeField] private float positionTime;
    [SerializeField] private float rotationTime;
    [SerializeField] private float rotationIntensity;
    [SerializeField] private RectTransform start, end;

    private readonly List<Image> ticks = new();
    private LevelManager levelManager;
    private Coroutine transitionCoroutine;
    private float displayVel = 0;

    private void Start() {
        thermometer.transform.position = start.position;

        levelManager = LevelManager.Instance;
        thermometer.maxValue = levelManager.TotalPointsOfInterest; 

        for (int i = 0; i < thermometer.maxValue; ++i) {
            Image tick = Instantiate(tickPrefab, tickHolder.transform).GetComponent<Image>();
            ticks.Add(tick);
        }
    }

    private void FixedUpdate() {
        thermometer.transform.eulerAngles = new Vector3(0, 0, 90 + (Mathf.Sin(Time.time * rotationTime) * rotationIntensity));

        thermometer.value = Mathf.SmoothDamp(thermometer.value, levelManager.TotalActive, ref displayVel, smoothTime);
        collectTotal.text = (levelManager.TotalPointsOfInterest - levelManager.TotalActive).ToString() + "-" + levelManager.TotalPointsOfInterest.ToString();

        for (int i = 0; i < ticks.Count; ++i)
        {
            var tick = ticks[i];
            if (i >= levelManager.TotalActive) {
                tick.color = Color.black;
            }
            else {
                tick.color = activeColor;
            }
        }
    }

    public void OpenThermometer(bool open)
    {
        Vector3 desiredPos = open ? end.position : start.position;

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(OpenThermometerCoroutine(desiredPos));
    }

    private IEnumerator OpenThermometerCoroutine(Vector3 position)
    {
        Vector3 posVel = Vector3.zero;

        while (Vector3.Distance(thermometer.transform.position, position) >= 0.01f)
        {
            thermometer.transform.position = Vector3.SmoothDamp(thermometer.transform.position, position, ref posVel, smoothTime);
            yield return null;
        }

        thermometer.transform.position = position;
    }
}
