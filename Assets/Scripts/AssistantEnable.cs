using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AssistantEnable : MonoBehaviour
{
    private GameObject assistant;
    public InputActionReference toggleAction;
    // Start is called before the first frame update
    void Awake()
    {
        toggleAction.action.performed += enableAssistant;
    }


    private void enableAssistant(InputAction.CallbackContext context)
    {
        assistant = transform.GetChild(0).gameObject;
        assistant.SetActive(!assistant.activeSelf);
        Debug.Log("Assistant toggled: " + assistant.activeSelf);
    }
}
