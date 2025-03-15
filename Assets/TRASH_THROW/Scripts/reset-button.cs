// ResetButton.cs
using UnityEngine;
using UnityEngine.Events;

public class ResetButton : MonoBehaviour
{
    [Header("Button Settings")]
    public UnityEvent onButtonPressed;
    public GameObject startButton;
    
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    public void OnButtonPress()
    {
        if (gameManager != null)
        {
            gameManager.ResetGame();
            
            // Reactivate start button if specified
            if (startButton != null)
            {
                startButton.SetActive(true);
            }
            
            // Invoke any additional events
            onButtonPressed?.Invoke();
        }
    }
}