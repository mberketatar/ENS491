using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VRTemplate;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoMenu : MonoBehaviour
{
    [SerializeField] private VideoClip[] Videos;
    [SerializeField] private string[] VideoNames;
    [SerializeField] private GameObject Title;
    //[SerializeField] private VideoPlayer VideoPlayer;
    [SerializeField] private GameObject Button;
    [SerializeField] private GameObject Menu;
    [SerializeField] private GameObject Content;
    [SerializeField] private GameObject ScreenPlayer;
    [SerializeField] private GameObject ContentAnchor;
    private GameObject inst;
    private GameObject inst2;
    private List<GameObject> contentInstances = new List<GameObject>();
    Vector3 hiddenPos = new Vector3(0, -9999, 0);
    Vector3 activePos;
    private void Start()
    {
        activePos = ContentAnchor.transform.position;
        for (int i = 0; i < Videos.Length; i++)
        {
            //Debug.Log(i);
            inst = Instantiate(Button, Menu.transform);
            inst2 = Instantiate(Content, ScreenPlayer.transform);
            contentInstances.Add(inst2);
            // Capture the current value of 'i' by using a local variable
            int index = i;
            inst.GetComponent<Button>().onClick.AddListener(delegate { VideoSwitch(index); });
            inst.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = VideoNames[i];
            inst2.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = VideoNames[i];
            inst2.transform.GetChild(1).GetComponentInChildren<VideoPlayer>().clip = Videos[i];
            inst2.transform.GetChild(1).GetComponentInChildren<VideoPlayer>().SetTargetAudioSource(0, inst2.transform.GetChild(1).GetComponentInChildren<AudioSource>());
            VideoPlayer vp = inst2.transform.GetChild(1).GetComponentInChildren<VideoPlayer>();
            vp.clip = Videos[i];
            //vp.Prepare();
            if (i == 0)
            {
                activePos = inst2.transform.localPosition; // Set the position of the first instance to original
            }
            else
            {
                inst2.transform.localPosition = hiddenPos; // Hide the others
                //StartCoroutine(PlayForThreeSeconds(vp));
            }
            //inst2.SetActive(false);
        }
    }
    /*
    private IEnumerator PlayForThreeSeconds(VideoPlayer vp)
    {
        // Start playing the video
        vp.Play();

        // Wait for 3 seconds
        yield return new WaitForSeconds(0.1f);

        // Stop the video after 3 seconds
        vp.Stop();
    }
    
    public void VideoSwitch(int no)
    {
        Debug.Log(no);
        VideoPlayer.Stop();
        VideoPlayer.clip = Videos[no];
        VideoPlayer.time = 0;
        Title.GetComponent<TextMeshProUGUI>().text = VideoNames[no];
    }
    IEnumerator HideAfterPrepared(VideoPlayer vp, GameObject obj)
    {
        Debug.Log("Preparing video...");
        vp.Prepare();

        while (!vp.isPrepared)
        {
            yield return null;
        }

        Debug.Log("Video prepared!");
        yield return null;

        obj.SetActive(false);
    }*/
    public void VideoSwitch(int no)
    {
        // Disable all content instances
        for (int i = 0; i < contentInstances.Count; i++)
        {
            if (i == no)
            {
                contentInstances[i].transform.localPosition = activePos; // Set the position of the selected instance to original
            }
            else
            {
                contentInstances[i].transform.GetChild(1).GetComponentInChildren<VideoTimeScrubControl>().VideoStop();
                contentInstances[i].transform.localPosition = hiddenPos; // Hide the others
            }
        }
    }
}
