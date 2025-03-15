using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TrashHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("Enable or disable the highlight")]
    public bool highlightEnabled = true;
    
    [Tooltip("The thickness of the outline")]
    [Range(0.001f, 0.05f)]
    public float outlineWidth = 0.01f;
    
    [Tooltip("The intensity of the glow effect")]
    [Range(0.1f, 5f)]
    public float glowIntensity = 1.0f;
    
    [Tooltip("Should the highlight pulse over time?")]
    public bool pulsing = true;
    
    [Tooltip("Speed of the pulse if enabled")]
    [Range(0.1f, 5f)]
    public float pulseSpeed = 1.0f;

    [Tooltip("Minimum intensity when pulsing")]
    [Range(0.1f, 1f)]
    public float minPulseIntensity = 0.5f;

    [Tooltip("Highlight visible through walls")]
    public bool visibleThroughWalls = true;
    
    [Header("Auto Color Setting")]
    [Tooltip("If true, automatically use the color based on trash type")]
    public bool useTrashTypeColor = true;
    
    [Tooltip("Custom color if not using trash type color")]
    public Color highlightColor = Color.yellow;
    
    [Header("Type-Based Colors")]
    public Color metalColor = new Color(0.8f, 0.8f, 0.8f, 1f);    // Silver
    public Color organicColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // Green
    public Color paperColor = new Color(0.9f, 0.9f, 0.2f, 1f);    // Yellow
    public Color plasticColor = new Color(0.2f, 0.4f, 0.9f, 1f);  // Blue

    // References
    private TrashItem trashItem;
    private TrashCan trashCan;
    private MeshRenderer meshRenderer;
    private Material originalMaterial;
    private GameObject highlightObject;
    private Renderer highlightRenderer;
    private float pulseTime = 0f;

    private void Start()
    {
        // Get component references
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("TrashHighlight requires a MeshRenderer component", this);
            enabled = false;
            return;
        }

        // Store original material
        if (meshRenderer.material != null)
        {
            originalMaterial = meshRenderer.material;
        }

        // Get trash type - check for TrashItem or TrashCan
        trashItem = GetComponent<TrashItem>();
        trashCan = GetComponent<TrashCan>();

        // Create highlight effect
        CreateHighlightObject();
    }

    private void CreateHighlightObject()
    {
        // Create a child object for the highlight
        highlightObject = new GameObject("Highlight");
        highlightObject.transform.SetParent(transform);
        highlightObject.transform.localPosition = Vector3.zero;
        highlightObject.transform.localRotation = Quaternion.identity;
        highlightObject.transform.localScale = Vector3.one * 1.05f; // Slightly larger than original

        // Copy mesh from original to highlight
        MeshFilter originalMeshFilter = GetComponent<MeshFilter>();
        if (originalMeshFilter != null && originalMeshFilter.sharedMesh != null)
        {
            MeshFilter highlightMeshFilter = highlightObject.AddComponent<MeshFilter>();
            highlightMeshFilter.sharedMesh = originalMeshFilter.sharedMesh;
        }
        else
        {
            Debug.LogError("Failed to find valid mesh for highlight", this);
            Destroy(highlightObject);
            return;
        }

        // Add renderer to highlight
        highlightRenderer = highlightObject.AddComponent<MeshRenderer>();
        
        // Try to find material creator helper
        HighlightMaterialCreator materialCreator = FindObjectOfType<HighlightMaterialCreator>();
        Material highlightMaterial = null;
        
        if (materialCreator != null)
        {
            // Get appropriate material for this trash type
            TrashType currentType = TrashType.Metal;
            
            if (trashItem != null)
                currentType = trashItem.trashType;
            else if (trashCan != null)
                currentType = trashCan.acceptedTrashType;
                
            highlightMaterial = materialCreator.GetMaterialForTrashType(currentType);
        }
        
        // If we don't have a material from the creator, create a new one
        if (highlightMaterial == null)
        {
            // Try various shaders in order of preference
            Shader[] shaderOptions = new Shader[] {
                Shader.Find("Custom/XRayHighlight"),
                Shader.Find("Transparent/Diffuse"),
                Shader.Find("Transparent/VertexLit"),
                Shader.Find("Legacy Shaders/Transparent/Diffuse"),
                Shader.Find("Standard")
            };
            
            foreach (Shader shader in shaderOptions)
            {
                if (shader != null)
                {
                    highlightMaterial = new Material(shader);
                    break;
                }
            }
            
            // Last resort fallback
            if (highlightMaterial == null)
            {
                highlightMaterial = new Material(Shader.Find("Diffuse"));
                Debug.LogWarning("Could not find appropriate shader for highlights. Using opaque fallback.", this);
            }
            
            // Configure material for transparency
            if (highlightMaterial.HasProperty("_Mode"))
            {
                highlightMaterial.SetFloat("_Mode", 3); // Transparent mode
                highlightMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                highlightMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                highlightMaterial.SetInt("_ZWrite", 0);
                highlightMaterial.DisableKeyword("_ALPHATEST_ON");
                highlightMaterial.EnableKeyword("_ALPHABLEND_ON");
                highlightMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                highlightMaterial.renderQueue = 3000;
            }
            
            // Set default color
            UpdateHighlightColor();
        }
        
        // Apply material to highlight
        highlightRenderer.material = highlightMaterial;
        
        // Set render mode for see-through effect if enabled
        if (visibleThroughWalls)
        {
            highlightRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            highlightRenderer.receiveShadows = false;
            
            // Try to set ZTest mode if the shader supports it
            if (highlightRenderer.material.HasProperty("_ZTest"))
            {
                highlightRenderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
        }
    }

    private void Update()
    {
        if (!highlightEnabled || highlightRenderer == null)
        {
            if (highlightObject != null && highlightObject.activeSelf)
            {
                highlightObject.SetActive(false);
            }
            return;
        }

        // Make sure highlight is active
        if (highlightObject != null && !highlightObject.activeSelf)
        {
            highlightObject.SetActive(true);
        }

        // Update pulse effect if enabled
        if (pulsing)
        {
            pulseTime += Time.deltaTime * pulseSpeed;
            float pulseValue = Mathf.Lerp(minPulseIntensity, 1.0f, (Mathf.Sin(pulseTime) + 1f) * 0.5f);
            
            if (highlightRenderer.material != null)
            {
                Color currentColor = highlightRenderer.material.GetColor("_EmissionColor");
                Color baseColor = GetTrashTypeColor();
                Color pulseColor = baseColor * glowIntensity * pulseValue;
                highlightRenderer.material.SetColor("_EmissionColor", pulseColor);
            }
        }
    }

    public void UpdateHighlightColor()
    {
        if (highlightRenderer != null && highlightRenderer.material != null)
        {
            Color typeColor = GetTrashTypeColor();
            
            // Set material properties for glow
            highlightRenderer.material.SetColor("_Color", new Color(typeColor.r, typeColor.g, typeColor.b, 0.2f));
            highlightRenderer.material.EnableKeyword("_EMISSION");
            highlightRenderer.material.SetColor("_EmissionColor", typeColor * glowIntensity);
        }
    }

    private Color GetTrashTypeColor()
    {
        // Use automatic color based on trash type if enabled
        if (useTrashTypeColor)
        {
            // If it's a trash item
            if (trashItem != null)
            {
                switch (trashItem.trashType)
                {
                    case TrashType.Metal:
                        return metalColor;
                    case TrashType.Organic:
                        return organicColor;
                    case TrashType.Paper:
                        return paperColor;
                    case TrashType.Plastic:
                        return plasticColor;
                }
            }
            // If it's a trash can
            else if (trashCan != null)
            {
                switch (trashCan.acceptedTrashType)
                {
                    case TrashType.Metal:
                        return metalColor;
                    case TrashType.Organic:
                        return organicColor;
                    case TrashType.Paper:
                        return paperColor;
                    case TrashType.Plastic:
                        return plasticColor;
                }
            }
        }
        
        // Fall back to custom color if no trash type or auto-color is disabled
        return highlightColor;
    }

    private void OnValidate()
    {
        // Update in editor when properties change
        if (highlightRenderer != null && highlightRenderer.material != null)
        {
            UpdateHighlightColor();
            
            // Update scale based on outline width
            if (highlightObject != null)
            {
                highlightObject.transform.localScale = Vector3.one * (1f + outlineWidth * 10f);
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up materials when destroyed
        if (highlightRenderer != null && highlightRenderer.material != null)
        {
            if (Application.isPlaying)
            {
                Destroy(highlightRenderer.material);
            }
            else
            {
                DestroyImmediate(highlightRenderer.material);
            }
        }
    }
}