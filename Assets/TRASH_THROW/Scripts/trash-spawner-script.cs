using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TrashCategoryPool
    {
        public TrashType trashType;
        public List<GameObject> prefabs = new List<GameObject>();
    }

    [Header("Trash Pools")]
    public List<TrashCategoryPool> trashPools = new List<TrashCategoryPool>();

    [Header("Spawn Settings")]
    public float spawnRadius = 1.0f;
    public float spawnHeight = 0.5f;
    public float minSpawnInterval = 1.0f;
    public float maxSpawnInterval = 3.0f;
    public int maxTrashItems = 10;
    public bool autoSpawn = true;

    private List<GameObject> activeTrash = new List<GameObject>();
    private bool isSpawning = false;
    private GameManager gameManager;
    private HighlightManager highlightManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        highlightManager = FindObjectOfType<HighlightManager>();
    }

    public void StartSpawning()
    {
        if (!isSpawning && gameManager != null && gameManager.IsGameRunning())
        {
            isSpawning = true;
            StartCoroutine(SpawnTrashRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void ClearAllTrash()
    {
        foreach (GameObject trash in activeTrash)
        {
            if (trash != null)
            {
                Destroy(trash);
            }
        }
        activeTrash.Clear();
    }

    private IEnumerator SpawnTrashRoutine()
    {
        while (isSpawning && gameManager != null && gameManager.IsGameRunning())
        {
            // Only spawn if we haven't reached the maximum number of items
            if (activeTrash.Count < maxTrashItems)
            {
                SpawnRandomTrash();
            }

            // Clean up null references in active trash list
            activeTrash.RemoveAll(item => item == null);

            // Wait for random interval before spawning next item
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SpawnRandomTrash()
    {
        if (trashPools.Count == 0)
        {
            Debug.LogWarning("No trash pools configured!");
            return;
        }

        // Select a random category
        int categoryIndex = Random.Range(0, trashPools.Count);
        TrashCategoryPool selectedPool = trashPools[categoryIndex];

        if (selectedPool.prefabs.Count == 0)
        {
            Debug.LogWarning($"No prefabs in the {selectedPool.trashType} pool!");
            return;
        }

        // Select a random prefab from the chosen category
        int prefabIndex = Random.Range(0, selectedPool.prefabs.Count);
        GameObject prefab = selectedPool.prefabs[prefabIndex];

        if (prefab == null)
        {
            Debug.LogWarning("Selected prefab is null!");
            return;
        }

        // Calculate a random position within spawn radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

        // Instantiate the trash item
        GameObject trashInstance = Instantiate(prefab, spawnPosition, Random.rotation);
        
        // Make sure it has a TrashItem component with the correct type
        TrashItem trashItem = trashInstance.GetComponent<TrashItem>();
        if (trashItem == null)
        {
            trashItem = trashInstance.AddComponent<TrashItem>();
        }
        trashItem.trashType = selectedPool.trashType;
        
        // Add highlight component if not already present
        if (trashItem.enableHighlight && trashInstance.GetComponent<TrashHighlight>() == null)
        {
            TrashHighlight highlight = trashInstance.AddComponent<TrashHighlight>();
            highlight.useTrashTypeColor = true;
            highlight.visibleThroughWalls = true;
            
            // Register with highlight manager if available
            if (highlightManager != null)
            {
                highlightManager.RegisterHighlight(highlight);
            }
        }

        // Add to active trash list
        activeTrash.Add(trashInstance);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize spawn area in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeight, spawnRadius);
    }
}