using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

// This script bridges XR Interactables with UI Button clicks
public class XRButtonInteractor : MonoBehaviour
{
    [Tooltip("The button component this interactor will press")]
    public Button targetButton;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        
        if (interactable == null)
        {
            Debug.LogError("XRButtonInteractor requires an XRBaseInteractable component!");
            return;
        }

        if (targetButton == null)
        {
            // Try to find button on this object or children
            targetButton = GetComponentInChildren<Button>();
            
            if (targetButton == null)
                Debug.LogWarning("No target Button assigned or found on XRButtonInteractor!");
        }
        
        // Subscribe to interaction events
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (targetButton != null && targetButton.interactable)
        {
            // Trigger the button click
            targetButton.onClick.Invoke();
            
            // Optional: play click sound or haptic feedback here
        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }
}
