using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public float water = 100f;
    public float electricity = 100f;
    public float naturalGas = 100f;
     public bool IsGameOver { get { return isGameOver; } }

    // Regeneration rate per second when no items are broken.
    public float regenRate = 5f;
    public float maxResource = 100f;

    // Game Over settings.
    public AudioClip gameOverSound;      // Assign your game over sound clip in the Inspector.
    public Text gameOverText;            // Assign your UI Text element for game over message.
    public AudioSource audioSource;      // Reference to an AudioSource component.

    private bool isGameOver = false;     // Flag to ensure GameOver logic runs only once.

    void Update()
    {
        // Clamp resource values so they stay between 0 and maxResource.
        water = Mathf.Clamp(water, 0f, maxResource);
        electricity = Mathf.Clamp(electricity, 0f, maxResource);
        naturalGas = Mathf.Clamp(naturalGas, 0f, maxResource);

        // Check for game over condition (and make sure we haven't already triggered game over).
        if (!isGameOver && (water <= 0f || electricity <= 0f || naturalGas <= 0f))
        {
            GameOver();
        }

        // Find all Repair objects (which control the broken state).
        Repair[] repairObjects = FindObjectsOfType<Repair>();
        bool anyBroken = false;
        foreach (Repair repair in repairObjects)
        {
            if (repair.IsBroken)
            {
                anyBroken = true;
                break;
            }
        }

        // If nothing is broken, regenerate resources.
        if (!anyBroken)
        {
            water = Mathf.Min(water + regenRate * Time.deltaTime, maxResource);
            electricity = Mathf.Min(electricity + regenRate * Time.deltaTime, maxResource);
            naturalGas = Mathf.Min(naturalGas + regenRate * Time.deltaTime, maxResource);
        }
    }
    public void ResetResources()
    {
        isGameOver      = false;
        water           = maxResource;
        electricity     = maxResource;
        naturalGas      = maxResource;
        if (gameOverText != null)
            gameOverText.enabled = false;
    }

    void GameOver()
    {
        isGameOver = true;

        // Display game over text if assigned.
        if (gameOverText != null)
        {
            gameOverText.text = "Game Over!";
            gameOverText.enabled = true;
        }

        // Play the game over sound if an AudioSource and clip are assigned.
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        Debug.Log("Game Over!");

        // Additional logic could include pausing the game, disabling player controls, etc.
    }
}
