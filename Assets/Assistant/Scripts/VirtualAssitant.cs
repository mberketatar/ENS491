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



    public Transform xrRigPosition;

    public Transform destinationBody;



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
                else{
                    DialogueManager.instance.DisableDialoguePanel();
                    currentDialogueIndex = 0;

                }

    }


    private void Update()
    {


            //set target position to the camera position

            xrRigPosition.position = Camera.main.transform.position;

            //set rig y rotation to camera y rotation

            xrRigPosition.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
      
            Vector3 targetDir = Camera.main.transform.position - transform.position;
            targetDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);

            //clamp destination y position to 0 and 2.5
            Vector3 destination = destinationBody.position;
            //lerp rotation
            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
            

            //lerp to destination
            transform.position = Vector3.Lerp(transform.position, destinationBody.position, Time.deltaTime * speed);

            //clamp y position
            transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, 2.5f), transform.position.z);
        

    }


     private void OnTriggerEnter2D(Collider2D other) {
        

        //if we enter a place of interest, we set out current dialogue data to the dialogue data of the place of interest
        if(other.gameObject.CompareTag("PlaceOfInterest")){
            PlaceOfInterest placeOfInterest = other.gameObject.GetComponent<PlaceOfInterest>();
            currentDialogueData = placeOfInterest.dialogueData;
        }
    }


    private void OnTriggerEnter(Collider other) {
            
    
            //if we enter a place of interest, we set out current dialogue data to the dialogue data of the place of interest
            if(other.gameObject.TryGetComponent(out PlaceOfInterest placeOfInterest)){
                
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





