using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GraphRenderer : MonoBehaviour
{
    public RectTransform graphContainer;
    public Color lineColor = Color.white;
    public float lineWidth = 2f;
    public Sprite circleSprite;

    public void ShowGraph(List<SpendingEntry> spendingEntries)
    {

        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        if (spendingEntries.Count < 2)
        {
            Debug.LogWarning("Not enough data points to plot a graph.");
            return;
        }

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;


        float yMinimum = 0f;
        float yMaximum = 0f;
        foreach (var entry in spendingEntries)
        {
            if (entry.amount > yMaximum)
                yMaximum = entry.amount;
        }
        yMaximum *= 1.1f;


        DateTime minDate = spendingEntries[0].date;
        DateTime maxDate = spendingEntries[spendingEntries.Count - 1].date;
        TimeSpan dateRange = maxDate - minDate;

        GameObject lastCircle = null;

        for (int i = 0; i < spendingEntries.Count; i++)
        {
            float xPosition = (float)(spendingEntries[i].date - minDate).TotalSeconds / (float)dateRange.TotalSeconds * graphWidth;
            float yPosition = (spendingEntries[i].amount - yMinimum) / (yMaximum - yMinimum) * graphHeight;

            GameObject circle = CreateCircle(new Vector2(xPosition, yPosition));

            if (lastCircle != null)
            {
                CreateLine(lastCircle.GetComponent<RectTransform>().anchoredPosition, circle.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircle = circle;
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject("Circle", typeof(Image));
        obj.transform.SetParent(graphContainer, false);
        Image image = obj.GetComponent<Image>();
        image.sprite = circleSprite;
        image.color = lineColor;

        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(8, 8);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;

        return obj;
    }

    private void CreateLine(Vector2 pointA, Vector2 pointB)
    {
        GameObject lineObj = new GameObject("Line", typeof(Image));
        lineObj.transform.SetParent(graphContainer, false);
        Image image = lineObj.GetComponent<Image>();
        image.color = lineColor;

        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        Vector2 dir = (pointB - pointA).normalized;
        float distance = Vector2.Distance(pointA, pointB);

        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = pointA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
}

