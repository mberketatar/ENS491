using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoMenu : MonoBehaviour
{
    [SerializeField] private VideoClip[] Videos;
    [SerializeField] private string[] VideoNames;
    [SerializeField] private GameObject Title;
    [SerializeField] private VideoPlayer VideoPlayer;
    [SerializeField] private GameObject Button;
    [SerializeField] private GameObject Menu;
    private GameObject inst;
    private void Start()
    {
        for (int i = 0; i < Videos.Length; i++)
        {
            Debug.Log(i);
            inst = Instantiate(Button, Menu.transform);
            // Capture the current value of 'i' by using a local variable
            int index = i;
            inst.GetComponent<Button>().onClick.AddListener(delegate { VideoSwitch(index); });
            inst.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = VideoNames[i];
        }
    }
    /*
    public void VideoSwitch(int no)
    {
        Debug.Log(no);
        VideoPlayer.Stop();
        VideoPlayer.clip = Videos[no];
        VideoPlayer.time = 0;
        Title.GetComponent<TextMeshProUGUI>().text = VideoNames[no];
    }*/
    public void VideoSwitch(int no)
    {
        Debug.Log(no);
        VideoPlayer.Stop();
        VideoPlayer.clip = Videos[no];
        VideoPlayer.time = 0;
        Title.GetComponent<TextMeshProUGUI>().text = VideoNames[no];
    }
}
