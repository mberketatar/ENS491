using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase is ready!");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
}

