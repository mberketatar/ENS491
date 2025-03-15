using UnityEngine;

public class HighlightMaterialCreator : MonoBehaviour
{
    [Header("Reference Materials")]
    [Tooltip("Drag a transparent material here to use as highlight material")]
    public Material transparentReferenceMaterial;
    
    [Header("Generated Highlight Materials")]
    public Material metalHighlightMaterial;
    public Material organicHighlightMaterial;
    public Material paperHighlightMaterial;
    public Material plasticHighlightMaterial;
    
    [Header("Colors")]
    public Color metalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f); 
    public Color organicColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    public Color paperColor = new Color(0.9f, 0.9f, 0.2f, 0.5f);
    public Color plasticColor = new Color(0.2f, 0.4f, 0.9f, 0.5f);
    
    private void Awake()
    {
        if (transparentReferenceMaterial == null)
        {
            Debug.LogWarning("No reference material assigned. Highlight materials won't be created.");
            return;
        }
        
        // Create highlight materials for each type
        metalHighlightMaterial = CreateHighlightMaterial(metalColor, "MetalHighlight");
        organicHighlightMaterial = CreateHighlightMaterial(organicColor, "OrganicHighlight");
        paperHighlightMaterial = CreateHighlightMaterial(paperColor, "PaperHighlight");
        plasticHighlightMaterial = CreateHighlightMaterial(plasticColor, "PlasticHighlight");
    }
    
    private Material CreateHighlightMaterial(Color color, string name)
    {
        // Create a new material based on the reference
        Material newMaterial = new Material(transparentReferenceMaterial);
        newMaterial.name = name;
        
        // Set the color
        newMaterial.color = color;
        
        // Enable emission if the shader supports it
        if (newMaterial.HasProperty("_EmissionColor"))
        {
            newMaterial.EnableKeyword("_EMISSION");
            newMaterial.SetColor("_EmissionColor", new Color(color.r, color.g, color.b, 1f));
        }
        
        // Make sure it's transparent
        if (newMaterial.HasProperty("_Mode"))
        {
            newMaterial.SetFloat("_Mode", 3); // Transparent mode
        }
        
        // Make sure it renders on top
        newMaterial.renderQueue = 3000;
        
        return newMaterial;
    }
    
    // Utility method for getting the material for a given trash type
    public Material GetMaterialForTrashType(TrashType trashType)
    {
        switch (trashType)
        {
            case TrashType.Metal:
                return metalHighlightMaterial;
            case TrashType.Organic:
                return organicHighlightMaterial;
            case TrashType.Paper:
                return paperHighlightMaterial;
            case TrashType.Plastic:
                return plasticHighlightMaterial;
            default:
                return metalHighlightMaterial;
        }
    }
}
