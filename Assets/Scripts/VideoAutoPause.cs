using System.Collections;
using System.Collections.Generic;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.Video;

public class VideoAutoPause : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    private GameObject player;
    [SerializeField] private float autopauseDistance;
    // Update is called once per frame
    private void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        player = GameObject.FindGameObjectWithTag("Player");
    }
    void Update()
    {
        if(videoPlayer.isPlaying && (Vector3.Distance(player.transform.position, this.transform.position) > autopauseDistance))
        {
            GetComponent<VideoTimeScrubControl>().VideoStop();
        }
    }
}
