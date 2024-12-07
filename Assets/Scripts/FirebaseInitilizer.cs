using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class FirebaseInitializer : MonoBehaviour
{
    [SerializeField] private TMP_Text logText;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                LogMessage("Firebase is ready!");
            }
            else
            {
                LogMessage($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void LogMessage(string message)
    {
        Debug.Log(message);
        if (logText != null)
        {
            logText.text += message + "\n"; 
        }
        else
        {
            Debug.LogWarning("LogText is not assigned in the inspector.");
        }
    }
}


