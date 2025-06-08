using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GrabbableButton1 : MonoBehaviour
{
    public enum ButtonType { Start, Reset }
    
    [Header("Button Settings")]
    public ButtonType buttonType = ButtonType.Start;
    public float returnSpeed = 5f;
    public float activationDistance = 0.1f;
    public float cooldownTime = 1f;
    public UnityEvent onButtonActivated;
    
    [Header("Visual Feedback")]
    public Color defaultColor = Color.white;
    public Color grabbedColor = Color.yellow;
    public Color activatedColor = Color.green;
    public float glowDuration = 0.5f;
    
    [Header("Audio Feedback")]
    public AudioClip grabSound;
    public AudioClip activationSound;
    public AudioClip returnSound;
    
    // Private variables
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isGrabbed = false;
    private bool isActivated = false;
    private bool isCoolingDown = false;
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;
    private Material buttonMaterial;
    private Color currentColor;

    [SerializeField] private DisasterGameManager gameManager;

    // fully-qualified grab-interactable type
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    
    void Start()
    {
        // Store original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Get components
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.material != null)
        {
            buttonMaterial = meshRenderer.material;
            currentColor = defaultColor;
            buttonMaterial.color = defaultColor;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (grabSound != null || activationSound != null || returnSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.volume = 0.7f;
        }
        
        // Find the GameManager
        gameManager = FindObjectOfType<DisasterGameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GrabbableButton: No GameManager found in scene!");
        }
        
        // Set up XR Grab Interactable (fully-qualified)
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.movementType  = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
            grabInteractable.throwOnDetach = false;
        }
        
        // Subscribe to events (fully-qualified event args)
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited .AddListener(OnRelease);
    }
    
    private void OnGrab(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isGrabbed = true;
        
        // Change color to grabbed state
        if (buttonMaterial != null)
        {
            currentColor = grabbedColor;
            buttonMaterial.color = grabbedColor;
        }
        
        // Play grab sound
        if (audioSource != null && grabSound != null)
            audioSource.PlayOneShot(grabSound);
    }
    
    private void OnRelease(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // Check if button was moved far enough to trigger
        float distanceMoved = Vector3.Distance(transform.position, originalPosition);
        if (distanceMoved > activationDistance && !isCoolingDown)
{
    Debug.Log("[Button] Activating!");
    ActivateButton();
}
        
        // Start returning to original position
        StartCoroutine(ReturnToOriginalPosition());
    }
    
    private void ActivateButton()
    {
        if (isCoolingDown) return;
        
        isActivated    = true;
        isCoolingDown  = true;
        
        // Change color to activated state
        if (buttonMaterial != null)
            StartCoroutine(FlashColor(activatedColor));
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
            audioSource.PlayOneShot(activationSound);
        
        // Trigger the appropriate game action
        if (gameManager != null)
        {
            if (buttonType == ButtonType.Start && !gameManager.IsGameRunning())
                gameManager.StartGame();
            else if (buttonType == ButtonType.Reset)
                gameManager.ResetGame();
        }
        
        // Invoke any additional events
        onButtonActivated?.Invoke();
        
        // Start cooldown
        StartCoroutine(ButtonCooldown());
    }
    
    private IEnumerator ReturnToOriginalPosition()
    {
        // Wait one frame to ensure the interactor is fully detached
        yield return null;
        
        // Play return sound
        if (audioSource != null && returnSound != null)
            audioSource.PlayOneShot(returnSound);
        
        // Smoothly return to original position
        float t = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        while (t < 1f)
        {
            if (isGrabbed) yield break;
            t += Time.deltaTime * returnSpeed;
            float sm = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, originalPosition, sm);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, sm);
            yield return null;
        }
        
        // Ensure exact reset
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // Reset color after returning if not activated
        if (!isActivated && buttonMaterial != null)
        {
            buttonMaterial.color = defaultColor;
            currentColor = defaultColor;
        }
        
        isActivated = false;
    }
    
    private IEnumerator ButtonCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        isCoolingDown = false;
    }
    
    private IEnumerator FlashColor(Color flashColor)
    {
        Color orig = currentColor;
        float half = glowDuration * 0.5f;
        float t = 0f;
        
        // Fade to flash
        while (t < half)
        {
            t += Time.deltaTime;
            buttonMaterial.color = Color.Lerp(orig, flashColor, t / half);
            yield return null;
        }
        // Fade back
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            buttonMaterial.color = Color.Lerp(flashColor, defaultColor, t / half);
            yield return null;
        }
        buttonMaterial.color = defaultColor;
        currentColor = defaultColor;
    }
    
    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited .RemoveListener(OnRelease);
        }
    }
}
