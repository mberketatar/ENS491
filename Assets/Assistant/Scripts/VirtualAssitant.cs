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

    private AudioSource audioSource; // Reference to the AudioSource

    private void Awake()
    {
        // Ensure there's an AudioSource component on the GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnInteraction()
    {
        if (currentDialogueData == null)
        {
            Debug.Log("No dialogue data");
            return;
        }

        if (currentDialogueIndex < currentDialogueData.dialogue.Length)
        {
            if (currentDialogueIndex == 0)
            {
                DialogueManager.instance.EnableDialoguePanel();
                // Do a little jump
            }
            transform.DOSmoothRewind();
            transform.DOJump(transform.position, 0.5f, 1, 0.5f);
            AdvanceDialogue();
        }
        else
        {
            DialogueManager.instance.DisableDialoguePanel();
            currentDialogueIndex = 0;
        }
    }

    private void Update()
    {
        xrRigPosition.position = Camera.main.transform.position;

        if (Mathf.Abs(Camera.main.transform.rotation.eulerAngles.y - xrRigPosition.rotation.eulerAngles.y) > 15)
        {
            xrRigPosition.rotation = Quaternion.Lerp(
                xrRigPosition.rotation, 
                Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0), 
                Time.deltaTime * 5.0f
            );
        }

        Vector3 targetDir = Camera.main.transform.position - transform.position;
        targetDir.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir);

        Vector3 destination = destinationBody.position;

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5.0f);
        transform.position = Vector3.Lerp(transform.position, destinationBody.position, Time.deltaTime * speed);
        transform.position = new Vector3(
            transform.position.x,
            Mathf.Clamp(transform.position.y, 0, 2.5f),
            transform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlaceOfInterest"))
        {
            PlaceOfInterest placeOfInterest = other.gameObject.GetComponent<PlaceOfInterest>();
            currentDialogueData = placeOfInterest.dialogueData;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlaceOfInterest placeOfInterest))
        {
            currentDialogueData = placeOfInterest.dialogueData;
        }
    }

    public void AdvanceDialogue()
    {
        if (currentDialogueIndex < currentDialogueData.dialogue.Length)
        {
            string dialogueText = currentDialogueData.dialogue[currentDialogueIndex];
            Debug.Log(dialogueText);
            DialogueManager.instance.DisplayDialogue(dialogueText);

            // Play the corresponding audio clip
            if (currentDialogueData.audioClips != null && 
                currentDialogueIndex < currentDialogueData.audioClips.Length &&
                currentDialogueData.audioClips[currentDialogueIndex] != null)
            {
                audioSource.clip = currentDialogueData.audioClips[currentDialogueIndex];
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("No audio clip assigned for this dialogue.");
            }

            currentDialogueIndex++;
        }
        else
        {
            DialogueManager.instance.DisableDialoguePanel();
            currentDialogueIndex = 0;
        }
    }
}
