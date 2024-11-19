using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class DataFetcher : MonoBehaviour
{
    private DatabaseReference dbReference;
    public TextMeshProUGUI displayText;

    void Start()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        FetchData();
    }

    void FetchData()
    {
        dbReference.Child("databasemom").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string data = snapshot.Value.ToString();
                Debug.Log("Data received: " + data);
                displayText.text = data;
            }
        });
    }
}

