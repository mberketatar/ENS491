using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DialogueManager : MonoBehaviour
{

    public TextMeshProUGUI dialogueText;

    public GameObject dialoguePanel;

    public static DialogueManager instance;
    // Start is called before the first frame update 

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EnableDialoguePanel()
    {
        dialoguePanel.SetActive(true);
    }
    public void DisableDialoguePanel()
    {
        dialoguePanel.SetActive(false);
    }

    public void DisplayDialogue(string dialogue)
    {
        if (!dialoguePanel.activeInHierarchy)
        {
            EnableDialoguePanel();
        }
        dialogueText.text = dialogue;
    }
   
}
