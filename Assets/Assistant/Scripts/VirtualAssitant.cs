using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using DG.Tweening;

public class VirtualAssitant : MonoBehaviour, IInteractable
{
    public VirtualAssitantPatrolData patrolData;


    public int currentPatrolIndex = 0;

    public DialogueData currentDialogueData;

    public int currentDialogueIndex = 0;

    public bool dialogueActive = false;


    public float speed;



    public void OnInteraction()
    {
      
        if(currentDialogueData == null){
            Debug.Log("No dialogue data");
            return;
        }
            
                if (currentDialogueIndex < currentDialogueData.dialogue.Length)
                {
                    if (currentDialogueIndex == 0)
                    {
                        DialogueManager.instance.EnableDialoguePanel();
                        //do a little jump
                    }
                    transform.DOJump(transform.position, 0.5f, 1, 0.5f);
                    AdvanceDialogue();


                }

    }


    private void Update()
    {

      
            Vector3 targetDir = Camera.main.transform.position - transform.position;
            targetDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            //lerp rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
        

    }


     private void OnTriggerEnter2D(Collider2D other) {
        

        //if we enter a place of interest, we set out current dialogue data to the dialogue data of the place of interest
        if(other.gameObject.CompareTag("PlaceOfInterest")){
            PlaceOfInterest placeOfInterest = other.gameObject.GetComponent<PlaceOfInterest>();
            currentDialogueData = placeOfInterest.dialogueData;
        }
    }

    public void AdvanceDialogue()
    {

        string dialogueText = currentDialogueData.dialogue[currentDialogueIndex];
        Debug.Log(dialogueText);
        DialogueManager.instance.DisplayDialogue(dialogueText);

        currentDialogueIndex++;
    }

    
}





