using UnityEngine;

// Enum to define the different types of trash
public enum TrashType
{
    Metal,
    Organic,
    Paper,
    Plastic
}

// Component to be attached to each trash item to identify its type
public class TrashItem : MonoBehaviour
{
    public TrashType trashType;
    
    [Header("Highlighting")]
    public bool enableHighlight = true;
    
    private void Start()
    {
        if (enableHighlight)
        {
            // Add highlight component if not already present
            if (GetComponent<TrashHighlight>() == null)
            {
                TrashHighlight highlight = gameObject.AddComponent<TrashHighlight>();
                highlight.useTrashTypeColor = true;
                highlight.visibleThroughWalls = true;
            }
        }
    }
}