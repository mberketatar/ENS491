using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SunCalcNet;

public class Light : MonoBehaviour
{

    private DateTime dayTime;
    private GameObject sun;
    // Start is called before the first frame update
    void Start()
    {
        sun = GameObject.Find("Sun");
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        DateTime dayTime = DateTime.Now;
        var sunPosition = SunCalc.GetSunPosition(DateTime.UtcNow, 41.0082, 28.9784);
        Debug.Log($"Azimuth: {sunPosition.Azimuth}, Altitude: {sunPosition.Altitude}");
        sun.transform.rotation = Quaternion.Euler(((float)sunPosition.Altitude), ((float)sunPosition.Azimuth), 0);
    }

}
