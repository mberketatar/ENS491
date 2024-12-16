using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class DataFetcher : MonoBehaviour
{
    private FirebaseFirestore firestore;
    public TextMeshProUGUI displayText;

    void Start()
    {
        FirebaseInitializer initializer = FindObjectOfType<FirebaseInitializer>();
        if (initializer != null)
        {
            firestore = initializer.GetFirestore();
            FetchData();
        }
        else
        {
            Debug.LogError("FirebaseInitializer not found in the scene.");
        }
    }

    void FetchData()
    {
        CollectionReference collection = firestore.Collection("databasemom");
        collection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string data = doc.Id + ": " + doc.ToDictionary();
                    Debug.Log("Data received: " + data);
                    if (displayText != null)
                    {
                        displayText.text += data + "\n";
                    }
                    else
                    {
                        Debug.LogWarning("DisplayText is not assigned in the inspector.");
                    }
                }
            }
        });
    }
}
