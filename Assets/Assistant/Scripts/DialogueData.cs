using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "DialogueData", order = 1)]
public class DialogueData : ScriptableObject
{
    
    public string[] dialogue;
    public AudioClip[] audioClips;    // Array of corresponding audio clips
}
