using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class SpendingDataFetcher : MonoBehaviour
{
    private DatabaseReference dbReference;

    public SpendingData waterSpendingData = new SpendingData();
    public SpendingData electricitySpendingData = new SpendingData();
        public GraphRenderer waterGraphRenderer;
    public GraphRenderer electricityGraphRenderer;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        FetchWaterSpendingData();
        FetchElectricitySpendingData();
    }

    void FetchWaterSpendingData()
    {
        dbReference.Child("Water_Spending").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch water spending data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                ParseSpendingData(snapshot, waterSpendingData);
                UpdateWaterSpendingGraph();
            }
        });
    }

    void FetchElectricitySpendingData()
    {
        dbReference.Child("Electricity_Spending").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Failed to fetch electricity spending data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                ParseSpendingData(snapshot, electricitySpendingData);
                UpdateElectricitySpendingGraph();
            }
        });
    }

    void ParseSpendingData(DataSnapshot snapshot, SpendingData spendingData)
    {
        spendingData.entries.Clear();

        foreach (var childSnapshot in snapshot.Children)
        {
            if (childSnapshot == null || childSnapshot.Value == null)
                continue;

            string dateKey = childSnapshot.Key;
            float amount = 0f;

            if (childSnapshot.Child("amount").Value != null)
            {
                amount = float.Parse(childSnapshot.Child("amount").Value.ToString());
            }

            DateTime date;
            if (DateTime.TryParse(dateKey, out date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = amount
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
}

