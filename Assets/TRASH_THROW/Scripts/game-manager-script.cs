using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public float gameDuration = 120f; // 2 minutes in seconds

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Game Objects")]
    public TrashSpawner[] trashSpawners;
    
    [Header("Audio")]
    public AudioClip gameStartSound;
    public AudioClip gameResetSound;
    public AudioClip timeRunningOutSound;
    public AudioClip gameOverSound;
    public float timeRunningOutThreshold = 30f; // Play warning sound when 30 seconds remain
    private bool hasPlayedTimeWarning = false;
    private AudioSource audioSource;

    private int currentScore = 0;
    private float remainingTime;
    private bool gameRunning = false;

    private void Awake()
    {
        // Find trash spawners if not set in inspector
        if (trashSpawners == null || trashSpawners.Length == 0)
        {
            trashSpawners = FindObjectsOfType<TrashSpawner>();
        }

        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        UpdateScoreDisplay();
        UpdateTimerDisplay(gameDuration);
    }

    public void StartGame()
    {
        if (gameRunning)
            return;

        // Reset values
        currentScore = 0;
        remainingTime = gameDuration;
        hasPlayedTimeWarning = false;
        
        // Update UI
        UpdateScoreDisplay();
        UpdateTimerDisplay(remainingTime);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Play start sound
        if (audioSource != null && gameStartSound != null)
        {
            audioSource.PlayOneShot(gameStartSound);
        }
        
        // Start game logic
        gameRunning = true;
        
        // Start spawners
        foreach (TrashSpawner spawner in trashSpawners)
        {
            if (spawner != null && spawner.autoSpawn)
            {
                spawner.StartSpawning();
            }
        }
        
        // Start timer
        StartCoroutine(GameTimerRoutine());
    }

    public void ResetGame()
    {
        // Stop the game
        StopAllCoroutines();
        gameRunning = false;
        
        // Stop and clear spawners
        foreach (TrashSpawner spawner in trashSpawners)
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
                spawner.ClearAllTrash();
            }
        }
        
        // Play reset sound
        if (audioSource != null && gameResetSound != null)
        {
            audioSource.PlayOneShot(gameResetSound);
        }
        
        // Reset UI
        currentScore = 0;
        hasPlayedTimeWarning = false;
        UpdateScoreDisplay();
        UpdateTimerDisplay(gameDuration);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public bool IsGameRunning()
    {
        return gameRunning;
    }

    public void AddScore(int points)
    {
        if (!gameRunning)
            return;
            
        currentScore += points;
        
        // Prevent negative score
        if (currentScore < 0)
            currentScore = 0;
            
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    private void UpdateTimerDisplay(float timeInSeconds)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private IEnumerator GameTimerRoutine()
    {
        while (remainingTime > 0 && gameRunning)
        {
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
            UpdateTimerDisplay(remainingTime);
            
            // Play warning sound when time is running out
            if (remainingTime <= timeRunningOutThreshold && !hasPlayedTimeWarning)
            {
                if (audioSource != null && timeRunningOutSound != null)
                {
                    audioSource.PlayOneShot(timeRunningOutSound);
                    hasPlayedTimeWarning = true;
                }
            }
        }
        
        // Time is up - end the game
        EndGame();
    }

    private void EndGame()
    {
        gameRunning = false;
        
        // Stop spawners
        foreach (TrashSpawner spawner in trashSpawners)
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
            }
        }
        
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {currentScore}";
        }
    }
}