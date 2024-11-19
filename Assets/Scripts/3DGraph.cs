using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class Spending3DGraphGenerator : MonoBehaviour
{
    private DatabaseReference dbReference;

    public GameObject barPrefab;
    public float barWidth = 1f;
    public float barDepth = 1f;
    public float spacing = 0.5f;
    public float heightMultiplier = 1f;

    public string spendingType;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        Generate3DGraph(spendingType);
    }

    void Generate3DGraph(string spendingType)
    {
        dbReference.Child(spendingType).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<SpendingEntry> spendingEntries = ParseSpendingData(snapshot);

                Create3DGraph(spendingEntries);
            }
            else
            {
                Debug.LogError("Failed to fetch spending data: " + task.Exception);
            }
        });
    }

    List<SpendingEntry> ParseSpendingData(DataSnapshot snapshot)
    {
        List<SpendingEntry> entries = new List<SpendingEntry>();

        foreach (var childSnapshot in snapshot.Children)
        {
            string dateKey = childSnapshot.Key;
            if (float.TryParse(childSnapshot.Child("amount").Value.ToString(), out float amount))
            {
                if (DateTime.TryParse(dateKey, out DateTime date))
                {
                    entries.Add(new SpendingEntry
                    {
                        date = date,
                        amount = amount
                    });
                }
            }
        }

        entries.Sort((a, b) => a.date.CompareTo(b.date));

        return entries;
    }

    void Create3DGraph(List<SpendingEntry> entries)
    {
        float startX = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            float xPosition = startX + i * (barWidth + spacing);
            float barHeight = entries[i].amount * heightMultiplier;

            GameObject bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(xPosition, barHeight / 2f, 0f);
            bar.transform.localScale = new Vector3(barWidth, barHeight, barDepth);

            AddLabel(bar.transform, entries[i].date.ToString("MM-dd"), entries[i].amount);
        }
    }

    void AddLabel(Transform barTransform, string dateText, float amount)
    {

        GameObject labelObj = new GameObject("BarLabel");
        labelObj.transform.SetParent(barTransform, true);
        labelObj.transform.localPosition = new Vector3(0f, 0f, 0f);

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = $"Date: {dateText}\n Amount: {amount}";
        textMesh.characterSize = 0.05f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        labelObj.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        labelObj.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}

[Serializable]
public class SpendingEntry
{
    public DateTime date;
    public float amount;
}

