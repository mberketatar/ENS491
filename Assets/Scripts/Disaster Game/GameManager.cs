using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DisasterGameManager : MonoBehaviour
{
    [Header("XR Start/Reset Cubes")]
    [SerializeField] private GrabbableButton1 startCube;
    [SerializeField] private GrabbableButton1 resetCube;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI finalTimeText;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip gameStartSound;
    [SerializeField] private AudioClip gameResetSound;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Resource System")]
    [SerializeField] private ResourceManager resourceManager;

    private AudioSource audioSource;
    private bool        gameRunning;
    private float       elapsedTime;
    private Repair[]    repairCubes;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // find every Repair in the scene
        repairCubes = FindObjectsOfType<Repair>();
        Debug.Log($"[DisasterGameManager] Found {repairCubes.Length} Repair cubes.");

        // hook up your XR buttons
        if (startCube != null) startCube.onButtonActivated.AddListener(StartGame);
        if (resetCube != null) resetCube.onButtonActivated.AddListener(ResetGame);

        // initialize UI / state
        ResetGame();
    }

    private void Update()
    {
        if (!gameRunning) return;

        // tick the timer
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay(elapsedTime);

        // check for resource‐driven game over
        if (resourceManager != null && resourceManager.IsGameOver)
            EndGame();
    }

    public void StartGame()
    {   
        Debug.Log("[DisasterGameManager] ▶ StartGame()");
        if (gameRunning) return;

        gameRunning  = true;
        elapsedTime  = 0f;
        resourceManager?.ResetResources();
        UpdateTimerDisplay(0f);

        if (gameStartSound != null)
            audioSource.PlayOneShot(gameStartSound);

        // begin each repair cube’s break cycle
        foreach (var r in repairCubes)
            r.BeginBreakCycle();
    }

    public void ResetGame()
    {
        gameRunning  = false;
        elapsedTime  = 0f;
        resourceManager?.ResetResources();
        UpdateTimerDisplay(0f);

        if (gameResetSound != null)
            audioSource.PlayOneShot(gameResetSound);

        // reset each repair cube immediately
        foreach (var r in repairCubes)
            r.ResetRepair();
    }

    private void EndGame()
    {
        gameRunning = false;

        if (finalTimeText != null)
            finalTimeText.text = $"You survived: {FormatTime(elapsedTime)}";

        if (gameOverSound != null)
            audioSource.PlayOneShot(gameOverSound);

        resourceManager?.ResetResources();       // refill water/electricity/gas
        foreach (var r in repairCubes)
            r.ResetRepair();  
    }

    private void UpdateTimerDisplay(float t)
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Exposed so your GrabbableButton can check if the game is running.
    /// </summary>
    public bool IsGameRunning() => gameRunning;
}
