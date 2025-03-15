using System.Collections;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("Trash Can Settings")]
    public TrashType acceptedTrashType;
    public int correctScoreValue = 10;
    public int incorrectScorePenalty = 5;
    public bool hasPenalty = true;
    
    [Header("Feedback")]
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public GameObject correctEffectPrefab;
    public GameObject incorrectEffectPrefab;
    public float effectDuration = 1f;
    
    [Header("Highlighting")]
    public bool enableHighlight = true;
    
    private GameManager gameManager;
    private AudioSource audioSource;
    private HighlightManager highlightManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        highlightManager = FindObjectOfType<HighlightManager>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (correctSound != null || incorrectSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (enableHighlight)
        {
            // Add highlight component if not already present
            if (GetComponent<TrashHighlight>() == null)
            {
                TrashHighlight highlight = gameObject.AddComponent<TrashHighlight>();
                highlight.useTrashTypeColor = true;
                highlight.visibleThroughWalls = true;
                
                // Register with highlight manager if available
                if (highlightManager != null)
                {
                    highlightManager.RegisterHighlight(highlight);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the game is running
        if (gameManager == null || !gameManager.IsGameRunning())
            return;

        // Check if the object has a TrashItem component
        TrashItem trashItem = other.GetComponent<TrashItem>();
        if (trashItem == null)
        {
            trashItem = other.GetComponentInParent<TrashItem>();
            if (trashItem == null)
                return;
        }

        // Check if the trash type matches the accepted type
        bool isCorrectType = (trashItem.trashType == acceptedTrashType);

        // Update score and play feedback
        if (isCorrectType)
        {
            // Award points for correct sorting
            gameManager.AddScore(correctScoreValue);
            PlayFeedback(true);
        }
        else if (hasPenalty)
        {
            // Subtract points for incorrect sorting if penalties are enabled
            gameManager.AddScore(-incorrectScorePenalty);
            PlayFeedback(false);
        }

        // Destroy the trash item
        Destroy(other.gameObject);
    }

    private void PlayFeedback(bool isCorrect)
    {
        // Play sound
        if (audioSource != null)
        {
            AudioClip clipToPlay = isCorrect ? correctSound : incorrectSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }

        // Show visual effect
        GameObject effectPrefab = isCorrect ? correctEffectPrefab : incorrectEffectPrefab;
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
    }
}