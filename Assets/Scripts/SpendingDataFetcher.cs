using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class SpendingDataFetcher : MonoBehaviour
{
    private FirebaseFirestore firestore;

    public SpendingData waterSpendingData = new SpendingData();
    public SpendingData electricitySpendingData = new SpendingData();
    public SpendingData gasSpendingData = new SpendingData();

    public GraphRenderer waterGraphRenderer;
    public GraphRenderer electricityGraphRenderer;
    public GraphRenderer gasGraphRenderer;

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
        FetchWaterSpendingData();
        FetchElectricitySpendingData();
        FetchGasSpendingData();
    }

    void FetchWaterSpendingData()
    {
        firestore.Collection("Water_Spending").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch water spending data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                ParseWaterData(snapshot, waterSpendingData);
                UpdateWaterSpendingGraph();
            }
        });
    }

    void FetchElectricitySpendingData()
    {
        firestore.Collection("Electricity_Spending").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch electricity spending data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                ParseElectricityData(snapshot, electricitySpendingData);
                UpdateElectricitySpendingGraph();
            }
        });
    }

    void FetchGasSpendingData()
    {
        firestore.Collection("Gas_Spending").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch gas spending data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                ParseGasData(snapshot, gasSpendingData);
                UpdateGasSpendingGraph();
            }
        });
    }

    void ParseWaterData(QuerySnapshot snapshot, SpendingData spendingData)
    {
        spendingData.entries.Clear();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string dateKey = doc.Id;
            float m3 = 0f;

            if (doc.TryGetValue("m3", out double m3Value))
            {
                m3 = (float)m3Value;
            }

            if (DateTime.TryParseExact(dateKey, "yy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = m3
                };

                spendingData.entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Invalid date format: " + dateKey);
            }
        }

        spendingData.entries.Sort((x, y) => x.date.CompareTo(y.date));
    }

    void ParseElectricityData(QuerySnapshot snapshot, SpendingData spendingData)
    {
        spendingData.entries.Clear();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string dateKey = doc.Id;
            float grid_kWh = 0f;
            float solar_kWh = 0f;
            float total_kWh = 0f;

            if (doc.TryGetValue("Grid_kWh", out double gridValue))
            {
                grid_kWh = (float)gridValue;
            }

            if (doc.TryGetValue("Solar_kWh", out double solarValue))
            {
                solar_kWh = (float)solarValue;
            }

            if (doc.TryGetValue("Total_kWh", out double totalValue))
            {
                total_kWh = (float)totalValue;
            }

            if (DateTime.TryParseExact(dateKey, "yy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = total_kWh,
                    additionalAmount = solar_kWh
                };

                spendingData.entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Invalid date format: " + dateKey);
            }
        }

        spendingData.entries.Sort((x, y) => x.date.CompareTo(y.date));
    }

    void ParseGasData(QuerySnapshot snapshot, SpendingData spendingData)
    {
        spendingData.entries.Clear();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string dateKey = doc.Id;
            float kWh = 0f;

            if (doc.TryGetValue("kWh", out double kWhValue))
            {
                kWh = (float)kWhValue;
            }

            if (DateTime.TryParseExact(dateKey, "yy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = kWh
                };

                spendingData.entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Invalid date format: " + dateKey);
            }
        }

        spendingData.entries.Sort((x, y) => x.date.CompareTo(y.date));
    }

    void UpdateWaterSpendingGraph()
    {
        if (waterGraphRenderer != null)
        {
            waterGraphRenderer.ShowGraph(waterSpendingData.entries);
        }
    }

    void UpdateElectricitySpendingGraph()
    {
        if (electricityGraphRenderer != null)
        {
            electricityGraphRenderer.ShowGraph(electricitySpendingData.entries);
        }
    }

    void UpdateGasSpendingGraph()
    {
        if (gasGraphRenderer != null)
        {
            gasGraphRenderer.ShowGraph(gasSpendingData.entries);
        }
    }
}
