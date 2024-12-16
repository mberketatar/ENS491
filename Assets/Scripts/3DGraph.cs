using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;

public class Spending3DGraphGenerator : MonoBehaviour
{
    private FirebaseFirestore firestore;

    public GameObject barPrefab;
    public float barWidth = 1f;
    public float barDepth = 1f;
    public float spacing = 0.5f;
    public float heightMultiplier = 1f;

    public string spendingType;

    void Start()
    {
        FirebaseInitializer initializer = FindObjectOfType<FirebaseInitializer>();
        if (initializer != null)
        {
            initializer.OnFirestoreInitialized += OnFirestoreReady;
        }
        else
        {
            Debug.LogError("FirebaseInitializer not found in the scene.");
        }
    }

    private void OnFirestoreReady(FirebaseFirestore db)
    {
        firestore = db;
        Generate3DGraph(spendingType);
    }

    void Generate3DGraph(string spendingType)
    {
        CollectionReference collection = firestore.Collection(spendingType);
        collection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                QuerySnapshot snapshot = task.Result;
                List<SpendingEntry> spendingEntries = ParseSpendingData(snapshot, spendingType);

                Create3DGraph(spendingEntries, spendingType);
            }
            else
            {
                Debug.LogError("Failed to fetch spending data: " + task.Exception);
            }
        });
    }

    List<SpendingEntry> ParseSpendingData(QuerySnapshot snapshot, string spendingType)
    {
        List<SpendingEntry> entries = new List<SpendingEntry>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string dateKey = doc.Id;
            float value = 0f;
            float additionalValue = 0f;

            if (spendingType == "Electricity_Spending")
            {
                if (doc.TryGetValue("Solar_kWh", out double solarVal))
                {
                    additionalValue = (float)solarVal;
                }
                if (doc.TryGetValue("Total_kWh", out double totalVal))
                {
                    value = (float)totalVal;
                }
            }
            else if (spendingType == "Water_Spending")
            {
                if (doc.TryGetValue("m3", out double m3Val))
                {
                    value = (float)m3Val;
                }
            }
            else if (spendingType == "Gas_Spending")
            {
                if (doc.TryGetValue("kWh", out double kWhVal))
                {
                    value = (float)kWhVal;
                }
            }

            if (DateTime.TryParseExact(dateKey, "yy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = value,
                    additionalAmount = additionalValue
                };
                entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Invalid date format: " + dateKey);
            }
        }

        entries.Sort((a, b) => a.date.CompareTo(b.date));
        return entries;
    }

    void Create3DGraph(List<SpendingEntry> entries, string spendingType)
    {
        float startX = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            float xPosition = startX + i * (barWidth + spacing);
            float barHeight = entries[i].amount * heightMultiplier;

            GameObject bar = Instantiate(barPrefab, transform);
            bar.transform.localPosition = new Vector3(xPosition, barHeight / 2f, 0f);
            bar.transform.localScale = new Vector3(barWidth, barHeight, barDepth);

            string labelText = entries[i].date.ToString("MM-yyyy");
            if (spendingType == "Electricity_Spending")
            {
                float solarPercentage = entries[i].amount > 0f 
                    ? (entries[i].additionalAmount / entries[i].amount) * 100f 
                    : 0f;
                labelText += $"\nSolar: {solarPercentage:F2}%";
            }
            else if (spendingType == "Water_Spending")
            {
                labelText += $"\n{entries[i].amount} m³";
            }
            else if (spendingType == "Gas_Spending")
            {
                labelText += $"\n{entries[i].amount} kWh";
            }

            AddLabel(bar.transform, labelText);
        }
    }

    void AddLabel(Transform barTransform, string label)
    {
        GameObject labelObj = new GameObject("BarLabel");
        labelObj.transform.SetParent(barTransform, false);

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.fontSize = 100;

        float labelHeight = 0.5f;
        labelObj.transform.localPosition = new Vector3(0f, (barTransform.localScale.y / 2f) + labelHeight, 0f);
    }
}
