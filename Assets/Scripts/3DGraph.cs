using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class Spending3DGraphGenerator : MonoBehaviour
{
    private FirebaseFirestore firestore;

    [Header("Bar Settings")]
    public GameObject barPrefab;
    public float barWidth = 0.5f;
    public float barDepth = 0.5f;
    public float spacing = 0.5f;
    public float maxHeight = 4f; // Maximum bar height in meters

    [Header("Electricity Bar Colors")]
    public Color gridBarColor = Color.blue;
    public Color solarBarColor = Color.yellow;

    [Header("Labels")]
    public float labelCharacterSize = 0.05f;
    public int labelFontSize = 100;
    public float labelVerticalOffset = 0.2f;

    [Header("Data")]
    public string spendingType;
    private List<GameObject> spawnedBars = new List<GameObject>();
    private List<GameObject> spawnedLabels = new List<GameObject>();
    private List<SpendingEntry> currentEntries = new List<SpendingEntry>();

    // The "Reset" button can be a simple UI button in a world-space canvas.
    // Hook this function up to the button's OnClick event.
    public void OnResetButtonPressed()
    {
        ResetGraph();
    }

    void Start()
    {
        FirebaseInitializer initializer = FindObjectOfType<FirebaseInitializer>();
        if (initializer != null)
        {
            initializer.OnFirestoreInitialized += OnFirestoreReady;
        }
        else
        {
            Debug.LogError("FirebaseInitializer not found in the scene.");
        }
    }

    private void OnFirestoreReady(FirebaseFirestore db)
    {
        firestore = db;
        Generate3DGraph(spendingType);
    }

    public void ResetGraph()
    {
        // Destroy old bars and labels
        foreach (var bar in spawnedBars)
        {
            if (bar != null) Destroy(bar);
        }
        spawnedBars.Clear();

        foreach (var label in spawnedLabels)
        {
            if (label != null) Destroy(label);
        }
        spawnedLabels.Clear();

        // Re-generate the graph with the stored entries
        Create3DGraph(currentEntries, spendingType);
    }

    void Generate3DGraph(string spendingType)
    {
        firestore.Collection(spendingType).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                QuerySnapshot snapshot = task.Result;
                List<SpendingEntry> spendingEntries = ParseSpendingData(snapshot, spendingType);

                // Store entries for re-creation on reset
                currentEntries = spendingEntries;
                Create3DGraph(spendingEntries, spendingType);
            }
            else
            {
                Debug.LogError("Failed to fetch spending data: " + task.Exception);
            }
        });
    }

    List<SpendingEntry> ParseSpendingData(QuerySnapshot snapshot, string spendingType)
    {
        List<SpendingEntry> entries = new List<SpendingEntry>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string dateKey = doc.Id;
            float value = 0f;
            float additionalValue = 0f; // For Electricity: solar_kWh

            if (spendingType == "Electricity_Spending")
            {
                if (doc.TryGetValue("Solar_kWh", out double solarVal))
                {
                    additionalValue = (float)solarVal;
                }
                if (doc.TryGetValue("Total_kWh", out double totalVal))
                {
                    value = (float)totalVal;
                }
            }
            else if (spendingType == "Water_Spending")
            {
                if (doc.TryGetValue("m3", out double m3Val))
                {
                    value = (float)m3Val;
                }
            }
            else if (spendingType == "Gas_Spending")
            {
                if (doc.TryGetValue("kWh", out double kWhVal))
                {
                    value = (float)kWhVal;
                }
            }

            if (DateTime.TryParseExact(dateKey, "yy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime date))
            {
                SpendingEntry entry = new SpendingEntry
                {
                    date = date,
                    amount = value,
                    additionalAmount = additionalValue
                };
                entries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Invalid date format: " + dateKey);
            }
        }

        entries.Sort((a, b) => a.date.CompareTo(b.date));
        return entries;
    }

    void Create3DGraph(List<SpendingEntry> entries, string spendingType)
    {
        // Determine the largest value for normalization
        float maxValue = 0f;
        if (spendingType == "Electricity_Spending")
        {
            // Based on total_kWh (amount)
            foreach (var e in entries)
            {
                if (e.amount > maxValue) maxValue = e.amount;
            }
        }
        else
        {
            foreach (var e in entries)
            {
                if (e.amount > maxValue) maxValue = e.amount;
            }
        }

        if (maxValue <= 0f) maxValue = 1f;

        float heightMultiplier = maxHeight / maxValue;
        float startX = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            float xPosition = startX + i * (barWidth + spacing);
            string labelText = entries[i].date.ToString("MM-yyyy");

            if (spendingType == "Electricity_Spending")
            {
                // We'll stack two bars: Grid (bottom) and Solar (top)
                float total_kWh = entries[i].amount;
                float solar_kWh = entries[i].additionalAmount;
                float grid_kWh = total_kWh - solar_kWh;

                float gridHeight = grid_kWh * heightMultiplier;
                float solarHeight = solar_kWh * heightMultiplier;

                // Create grid bar
                GameObject gridBar = Instantiate(barPrefab, transform);
                Rigidbody gridRb = gridBar.GetComponent<Rigidbody>();
                if (gridRb != null)
                {
                    // Make it stable (no tipping)
                    //gridRb.isKinematic = true;
                }
                gridBar.transform.localPosition = new Vector3(xPosition, gridHeight / 2f, 0f);
                gridBar.transform.localScale = new Vector3(barWidth, gridHeight, barDepth);
                AssignBarColor(gridBar, gridBarColor);
                Debug.LogWarning("Hurr");
                spawnedBars.Add(gridBar);

                // Create solar bar on top of the grid bar
                float solarBarYPos = gridHeight + (solarHeight / 2f);
                GameObject solarBar = Instantiate(barPrefab, transform);
                Rigidbody solarRb = solarBar.GetComponent<Rigidbody>();
                if (solarRb != null)
                {
                    //solarRb.isKinematic = true;
                }
                solarBar.transform.localPosition = new Vector3(xPosition, solarBarYPos, 0f);
                solarBar.transform.localScale = new Vector3(barWidth, solarHeight, barDepth);
                AssignBarColor(solarBar, solarBarColor);
                spawnedBars.Add(solarBar);

                // Update label text: show grid and solar values separately
                labelText += $"\nGrid: {grid_kWh} kWh\nSolar: {solar_kWh} kWh";

                // Label above top bar
                float labelYPos = ((gridHeight + solarHeight) / 2f) + labelVerticalOffset;
                AddLabel(new Vector3(xPosition, labelYPos, 0f), labelText);
            }
            else
            {
                // Single bar scenario (Water or Gas)
                float value = entries[i].amount;
                float barHeight = value * heightMultiplier;

                GameObject bar = Instantiate(barPrefab, transform);
                Rigidbody rb = bar.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    //rb.isKinematic = true;
                }
                bar.transform.localPosition = new Vector3(xPosition, barHeight / 2f, 0f);
                bar.transform.localScale = new Vector3(barWidth, barHeight, barDepth);

                // Assign a default color if desired
                AssignBarColor(bar, Color.green);
                spawnedBars.Add(bar);

                // Add unit labels
                if (spendingType == "Water_Spending")
                {
                    labelText += $"\n{value} m³";
                }
                else if (spendingType == "Gas_Spending")
                {
                    labelText += $"\n{value} kWh";
                }

                float labelYPos = (barHeight / 2f) + labelVerticalOffset;
                AddLabel(new Vector3(xPosition, labelYPos, 0f), labelText);
            }
        }
    }

    void AssignBarColor(GameObject bar, Color color)
    {
        Renderer rend = bar.GetComponent<Renderer>();
        if (rend != null)
        {
            // Assign a simple material color
            rend.material.color = color;
            Debug.LogWarning(bar.name + " color set to " + color);

        }
        else
        {
            Debug.LogWarning("Renderer not found on bar prefab.");
        }
    }

    void AddLabel(Vector3 worldPos, string label)
    {
        GameObject labelObj = new GameObject("BarLabel");
        labelObj.transform.SetParent(this.transform, false);

        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.characterSize = labelCharacterSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        textMesh.fontSize = labelFontSize;

        // Place label at the calculated world position
        labelObj.transform.localPosition = worldPos;
        spawnedLabels.Add(labelObj);
    }
}
