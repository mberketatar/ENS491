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

    public bool isMovingToPatrolPoint = false;

    public bool readyAtPatrolPoint = false;

    public float speed;


    public SerializedDictionary<string, Transform> patrolPoints;



    public void OnInteraction()
    {
        if (dialogueActive)
            return;
        if (isMovingToPatrolPoint)
            return;

        if (patrolData.patrolData[currentPatrolIndex].dialogueData.dialogue.Length > 0)
        {
            if (readyAtPatrolPoint)
            {
                if(currentDialogueIndex < currentDialogueData.dialogue.Length)
                {
                    if(currentDialogueIndex == 0)
                    {
                        DialogueManager.instance.EnableDialoguePanel();
                    }
                    AdvanceDialogue();
                }
                else
                {
                    DialogueManager.instance.DisableDialoguePanel();
                    currentPatrolIndex++;
                    if (currentPatrolIndex >= patrolData.patrolData.Length)
                    {
                        currentPatrolIndex = 0;
                    }
                    StartNewAssistAction(currentPatrolIndex);
                }
            }
            else
            {
                StartNewAssistAction(currentPatrolIndex);
            }
        }

    }


    private void Update() {

        //if not moving to patrol point, face player 
        if (!isMovingToPatrolPoint)
        {
            Vector3 targetDir = Camera.main.transform.position - transform.position;
            targetDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            //lerp rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
        }
        
    }

    public void AdvanceDialogue(){
        
       string dialogueText = currentDialogueData.dialogue[currentDialogueIndex];
       Debug.Log(dialogueText);
         DialogueManager.instance.DisplayDialogue(dialogueText);

         currentDialogueIndex++;
    }

    public void StartNewAssistAction(int index)
    {

        PatrolData data = patrolData.patrolData[index];

        Transform patrolPoint = patrolPoints[data.patrolPointName];


        //if already close to patrol point, dont move

        float distance = Vector3.Distance(transform.position, patrolPoint.position);

        if (distance < 1.0f)
        {
            readyAtPatrolPoint = true;

        }
        else
        {
            MoveToPatrolPoint(patrolPoint);

        }


        currentDialogueData = data.dialogueData;
        currentDialogueIndex = 0;
        

    }


    public void MoveToPatrolPoint(Transform target)
    {

        StartCoroutine(MoveToPatrolPointCoroutine());
        IEnumerator MoveToPatrolPointCoroutine()
        {
            readyAtPatrolPoint = false;
            isMovingToPatrolPoint = true;


            //only rotate y axis
            Vector3 targetDir = target.position - transform.position;
            targetDir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.DORotateQuaternion(targetRotation, 0.5f).SetEase(Ease.Linear);
            yield return new WaitForSeconds(0.5f);

            float distance = Vector3.Distance(transform.position, target.position);
            float duration = distance / speed;

            transform.DOMove(target.position, duration).SetEase(Ease.Linear);

            yield return new WaitForSeconds(duration);

            isMovingToPatrolPoint = false;

            readyAtPatrolPoint = true;

        }
    }
}





