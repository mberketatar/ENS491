using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class VideoMenu : MonoBehaviour
{
    [SerializeField] private GameObject Title;
    [SerializeField] private GameObject VideoPlayer;
    public void VideoSwitch(string url)
    {
        VideoPlayer.GetComponent<VideoPlayer>().url = url;
    }
    public void TitleSwitch(string name)
    {
        Title.GetComponent<TextMeshPro>().text = name;
    }
}
