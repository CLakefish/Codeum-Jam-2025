using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float standardSize;
    [SerializeField] private float hoveredSize;
    [SerializeField] private float interpolationSpeed;
    private Coroutine scaleCoroutine;

    private void OnDisable()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        transform.localScale = Vector3.one * standardSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(Scale(hoveredSize));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(Scale(standardSize));
    }

    private IEnumerator Scale(float size)
    {
        float scaleVel = 0;

        while (Mathf.Abs(transform.localScale.x - size) > 0.01f)
        {
            transform.localScale = Vector3.one * Mathf.SmoothDamp(transform.localScale.x, size, ref scaleVel, interpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);

            yield return null;
        }
    }
}
