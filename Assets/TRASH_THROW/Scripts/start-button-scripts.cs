// StartButton.cs
using UnityEngine;
using UnityEngine.Events;

public class StartButton : MonoBehaviour
{
    [Header("Button Settings")]
    public UnityEvent onButtonPressed;
    public bool disableAfterPress = true;
    
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }
    
    public void OnButtonPress()
    {
        if (gameManager != null && !gameManager.IsGameRunning())
        {
            gameManager.StartGame();
            
            // Invoke any additional events
            onButtonPressed?.Invoke();
            
            // Disable if configured
            if (disableAfterPress)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
