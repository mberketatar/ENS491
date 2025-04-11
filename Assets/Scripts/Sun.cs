using SunCalcNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    float latitude;
    float longitude;
    float fastTime;
    void Start()
    {
        // Coordinates of SUCool
        latitude = 40.8947f;
        longitude = 29.3752f;
        fastTime = 0f;

    }
    void FixedUpdate()
    {
        /*
        fastTime += 0.01f; // Increment time
        int hour = (int)Math.Floor(fastTime % 24);  // Get whole hour (integer)
        int minute = (int)Math.Floor((fastTime % 24 - hour) * 60);  // Convert fractional part of time to minutes

        DateTime customDateTime = new DateTime(2025, 6, 16, hour, minute, 0);
        
        var sunPosition = SunCalc.GetSunPosition(customDateTime, latitude, longitude);
        */
        var sunPosition = SunCalc.GetSunPosition(DateTime.UtcNow, latitude, longitude);
        transform.rotation = Quaternion.Euler(new Vector3((Mathf.Rad2Deg * (float)sunPosition.Altitude), (Mathf.Rad2Deg*(float)sunPosition.Azimuth-73), 0));
        //Debug.Log(new Vector3(0, (Mathf.Rad2Deg * (float)sunPosition.Azimuth - 73)));
    }

}
