using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    [System.Serializable]
    public class TypeHighlightSettings
    {
        public TrashType trashType;
        public Color highlightColor = Color.white;
        [Range(0.1f, 5f)]
        public float glowIntensity = 1.0f;
        public bool pulsing = true;
        [Range(0.1f, 5f)]
        public float pulseSpeed = 1.0f;
    }
    
    [Header("Global Settings")]
    public bool highlightsEnabled = true;
    [Range(0.001f, 0.05f)]
    public float globalOutlineWidth = 0.01f;
    public bool visibleThroughWalls = true;
    
    [Header("Type-Specific Settings")]
    public List<TypeHighlightSettings> typeSettings = new List<TypeHighlightSettings>();
    
    // Keep track of all active highlights
    private List<TrashHighlight> activeHighlights = new List<TrashHighlight>();
    
    private void Awake()
    {
        // Initialize default settings if none exist
        if (typeSettings.Count == 0)
        {
            typeSettings.Add(new TypeHighlightSettings { 
                trashType = TrashType.Metal, 
                highlightColor = new Color(0.8f, 0.8f, 0.8f) 
            });
            
            typeSettings.Add(new TypeHighlightSettings { 
                trashType = TrashType.Organic, 
                highlightColor = new Color(0.2f, 0.8f, 0.2f) 
            });
            
            typeSettings.Add(new TypeHighlightSettings { 
                trashType = TrashType.Paper, 
                highlightColor = new Color(0.9f, 0.9f, 0.2f) 
            });
            
            typeSettings.Add(new TypeHighlightSettings { 
                trashType = TrashType.Plastic, 
                highlightColor = new Color(0.2f, 0.4f, 0.9f) 
            });
        }
    }
    
    private void Start()
    {
        // Wait for all objects to initialize and then refresh highlights
        StartCoroutine(InitialHighlightRefresh());
    }
    
    private IEnumerator InitialHighlightRefresh()
    {
        yield return new WaitForSeconds(0.5f);
        RefreshAllHighlights();
    }
    
    public void RefreshAllHighlights()
    {
        // Find all highlights in the scene
        TrashHighlight[] highlights = FindObjectsOfType<TrashHighlight>();
        
        // Clear and rebuild the active list
        activeHighlights.Clear();
        
        foreach (TrashHighlight highlight in highlights)
        {
            // Apply global settings
            highlight.highlightEnabled = highlightsEnabled;
            highlight.outlineWidth = globalOutlineWidth;
            highlight.visibleThroughWalls = visibleThroughWalls;
            
            // Apply type-specific settings
            ApplyTypeSettings(highlight);
            
            // Add to active list
            activeHighlights.Add(highlight);
        }
    }
    
    private void ApplyTypeSettings(TrashHighlight highlight)
    {
        TrashType currentType = TrashType.Metal; // Default
        
        // Determine the type
        TrashItem trashItem = highlight.GetComponent<TrashItem>();
        TrashCan trashCan = highlight.GetComponent<TrashCan>();
        
        if (trashItem != null)
        {
            currentType = trashItem.trashType;
        }
        else if (trashCan != null)
        {
            currentType = trashCan.acceptedTrashType;
        }
        
        // Find matching settings
        foreach (TypeHighlightSettings setting in typeSettings)
        {
            if (setting.trashType == currentType)
            {
                // Apply type-specific settings
                if (currentType == TrashType.Metal)
                    highlight.metalColor = setting.highlightColor;
                else if (currentType == TrashType.Organic)
                    highlight.organicColor = setting.highlightColor;
                else if (currentType == TrashType.Paper)
                    highlight.paperColor = setting.highlightColor;
                else if (currentType == TrashType.Plastic)
                    highlight.plasticColor = setting.highlightColor;
                
                highlight.glowIntensity = setting.glowIntensity;
                highlight.pulsing = setting.pulsing;
                highlight.pulseSpeed = setting.pulseSpeed;
                
                highlight.UpdateHighlightColor();
                break;
            }
        }
    }
    
    // Call this when a new trash item is created at runtime
    public void RegisterHighlight(TrashHighlight highlight)
    {
        if (highlight != null && !activeHighlights.Contains(highlight))
        {
            // Apply global settings
            highlight.highlightEnabled = highlightsEnabled;
            highlight.outlineWidth = globalOutlineWidth;
            highlight.visibleThroughWalls = visibleThroughWalls;
            
            // Apply type-specific settings
            ApplyTypeSettings(highlight);
            
            // Add to active list
            activeHighlights.Add(highlight);
        }
    }
    
    // Global control functions
    public void SetHighlightsEnabled(bool enabled)
    {
        highlightsEnabled = enabled;
        
        foreach (TrashHighlight highlight in activeHighlights)
        {
            if (highlight != null)
            {
                highlight.highlightEnabled = enabled;
            }
        }
    }
    
    public void SetOutlineWidth(float width)
    {
        globalOutlineWidth = Mathf.Clamp(width, 0.001f, 0.05f);
        
        foreach (TrashHighlight highlight in activeHighlights)
        {
            if (highlight != null)
            {
                highlight.outlineWidth = globalOutlineWidth;
            }
        }
    }
    
    // This method refreshes all highlights when settings change in the editor
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            RefreshAllHighlights();
        }
    }
}
