using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneLoaderButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load")]
    public string targetSceneName;
    
    [Tooltip("Optional fade transition time (set to 0 for instant change)")]
    public float fadeTransitionTime = 1.0f;
    
    [Header("Button Settings")]
    public float returnSpeed = 5f;
    public float activationDistance = 0.1f;
    public float cooldownTime = 1f;
    
    [Header("Visual Feedback")]
    public Color defaultColor = Color.white;
    public Color grabbedColor = Color.cyan;
    public Color activatedColor = Color.blue;
    public float glowDuration = 0.5f;
    
    [Header("Audio Feedback")]
    public AudioClip grabSound;
    public AudioClip activationSound;
    public AudioClip returnSound;
    
    [Header("Events")]
    [Tooltip("Called right before scene transition begins")]
    public UnityEvent onBeforeSceneLoad;
    
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
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private CanvasGroup fadeCanvasGroup;
    
    void Start()
    {
        // Store original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Get required components
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
        
        // Set up XR Grab Interactable if not already present
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
            grabInteractable.throwOnDetach = false;
        }
        
        // Subscribe to events
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        // Check if scene name is valid
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"SceneLoaderButton on {gameObject.name} has no target scene name set!", this);
        }
        else
        {
            // Verify the scene exists in build settings
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == targetSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (!sceneExists)
            {
                Debug.LogWarning($"Scene '{targetSceneName}' not found in build settings! Make sure to add it.", this);
            }
        }
        
        // Create fade canvas if using transitions
        if (fadeTransitionTime > 0)
        {
            CreateFadeCanvas();
        }
    }
    
    private void CreateFadeCanvas()
    {
        // Only create if we don't already have one in the scene
        if (FindObjectOfType<CanvasGroup>() == null)
        {
            // Create canvas for fade transition
            GameObject fadeCanvas = new GameObject("FadeCanvas");
            Canvas canvas = fadeCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Make sure it renders on top of everything
            
            fadeCanvasGroup = fadeCanvas.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
            
            // Add a black image
            GameObject imageObj = new GameObject("BlackImage");
            imageObj.transform.SetParent(fadeCanvas.transform, false);
            UnityEngine.UI.Image image = imageObj.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            
            // Set it to fill the screen
            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            // Make the canvas persistent across scene loads
            DontDestroyOnLoad(fadeCanvas);
        }
        else
        {
            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
        }
    }
    
    private void OnGrab(SelectEnterEventArgs args)
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
        {
            audioSource.PlayOneShot(grabSound);
        }
    }
    
    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        
        // Check if button was moved far enough to trigger
        float distanceMoved = Vector3.Distance(transform.position, originalPosition);
        
        if (distanceMoved > activationDistance && !isCoolingDown)
        {
            ActivateButton();
        }
        
        // Start returning to original position
        StartCoroutine(ReturnToOriginalPosition());
    }
    
    private void ActivateButton()
    {
        if (isCoolingDown || string.IsNullOrEmpty(targetSceneName))
            return;
            
        isActivated = true;
        isCoolingDown = true;
        
        // Change color to activated state
        if (buttonMaterial != null)
        {
            StartCoroutine(FlashColor(activatedColor));
        }
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Call events before scene change
        onBeforeSceneLoad?.Invoke();
        
        // Load the scene with optional fade transition
        StartCoroutine(LoadSceneWithTransition());
    }
    
    private IEnumerator LoadSceneWithTransition()
    {
        // If we have a fade canvas, use it for transition
        if (fadeCanvasGroup != null && fadeTransitionTime > 0)
        {
            // Fade to black
            float elapsedTime = 0;
            while (elapsedTime < fadeTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeTransitionTime);
                yield return null;
            }
            
            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            
            // Wait until scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Fade back in
            elapsedTime = 0;
            while (elapsedTime < fadeTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeTransitionTime));
                yield return null;
            }
            
            // Ensure alpha is 0
            fadeCanvasGroup.alpha = 0;
        }
        else
        {
            // Just load the scene directly without transition
            SceneManager.LoadScene(targetSceneName);
        }
    }
    
    private IEnumerator ReturnToOriginalPosition()
    {
        // Wait one frame to ensure the interactor is fully detached
        yield return null;
        
        // Play return sound
        if (audioSource != null && returnSound != null)
        {
            audioSource.PlayOneShot(returnSound);
        }
        
        // Smoothly return to original position
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (elapsedTime < 1f)
        {
            if (isGrabbed)
            {
                // If grabbed again during return, exit the coroutine
                yield break;
            }
            
            elapsedTime += Time.deltaTime * returnSpeed;
            float t = Mathf.SmoothStep(0, 1, elapsedTime);
            
            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, t);
            
            yield return null;
        }
        
        // Ensure we're exactly at the original position
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
        Color originalColor = currentColor;
        float elapsedTime = 0f;
        
        // Fade to flash color
        while (elapsedTime < glowDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (glowDuration / 2);
            
            if (buttonMaterial != null)
            {
                buttonMaterial.color = Color.Lerp(originalColor, flashColor, t);
            }
            
            yield return null;
        }
        
        // Fade back to original color
        elapsedTime = 0f;
        while (elapsedTime < glowDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (glowDuration / 2);
            
            if (buttonMaterial != null)
            {
                buttonMaterial.color = Color.Lerp(flashColor, defaultColor, t);
            }
            
            yield return null;
        }
        
        if (buttonMaterial != null)
        {
            buttonMaterial.color = defaultColor;
            currentColor = defaultColor;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}
