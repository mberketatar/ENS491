using System;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class FirebaseInitializer : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;
    private FirebaseFirestore firestore;

    public event Action<FirebaseFirestore> OnFirestoreInitialized;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                LogMessage("Firestore is ready!");
                OnFirestoreInitialized?.Invoke(firestore);
            }
            else
            {
                LogMessage($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public FirebaseFirestore GetFirestore()
    {
        return firestore;
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        if (logText != null)
        {
            logText.text += message + "\n"; 
        }
    }
}
