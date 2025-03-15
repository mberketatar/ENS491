using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public float water = 100f;
    public float electricity = 100f;
    public float naturalGas = 100f;

    // Regeneration rate per second when no items are broken.
    public float regenRate = 5f;
    public float maxResource = 100f;

    void Update()
    {
        // Check for game over condition.
        if (water <= 0f || electricity <= 0f || naturalGas <= 0f)
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

    void GameOver()
    {
        Debug.Log("Game Over!");
        // Additional game over logic could be added here.
    }
}
