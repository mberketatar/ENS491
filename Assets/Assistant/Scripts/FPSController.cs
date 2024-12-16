using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{

    [SerializeField] private InputActionAsset playerControls;
    public GameObject playerCamera;
    public CharacterController controller;

    public float lookSpeed = 2.0f;

    public float moveSpeed = 6.0f;

    public Vector3 moveDirection = Vector3.zero;

    public float lookXLimit = 45.0f;



    public Vector2 moveInput;

    public float verticalRotation = 0;




    public InputAction moveAction;
    public InputAction lookAction;
    // Start is called before the first frame update

    public InputAction interactAction;


    public Vector2 lookInput;

    public Vector2 rotation;

    

    void Awake()
    {
        controller = GetComponent<CharacterController>();


        moveAction = playerControls.FindActionMap("Player").FindAction("Move");
        lookAction = playerControls.FindActionMap("Player").FindAction("Look");
        interactAction = playerControls.FindActionMap("Player").FindAction("Interact");

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        lookAction.performed += ctx => GetLookInput(ctx);
        lookAction.canceled += ctx => lookInput = Vector2.zero;

        interactAction.started += ctx => Interact();





        if (moveAction == null)
        {
            Debug.LogError("Move action not found in player controls");
        }

    }



    public void Interact(){
        //send a raycast from the camera to see if we hit anything

        Debug.Log("Interacting");
        RaycastHit hit;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5.0f)){
            //if we hit something, check if it has an interactable component
            Debug.Log("Hit something named " + hit.collider.name);
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if(interactable != null){
                //if it does, call the interact method
                Debug.Log("Interactable found");
                interactable.OnInteraction();
            }
        }
    }


    void GetLookInput(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();



    }


    public 

    void HandleRotation()
    {

        rotation.x += lookInput.x * lookSpeed;
        rotation.y += lookInput.y * lookSpeed;

        rotation.y = Mathf.Clamp(rotation.y, -lookXLimit,lookXLimit);

        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

        transform.localRotation = xQuat;
        playerCamera.transform.localRotation = yQuat;



    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        interactAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        interactAction.Disable();
    }


    public void OnMove(InputAction.CallbackContext context)
    {

    }

    // Update is called once per frame
    void Update()
    {


        Vector3 myForward = transform.TransformDirection(Vector3.forward);
        Vector3 myRight = transform.TransformDirection(Vector3.right);

        float xSpeed = moveInput.x * moveSpeed;
        float zSpeed = moveInput.y * moveSpeed;

        moveDirection = (myRight * xSpeed) + (myForward * zSpeed);

        controller.Move(moveDirection * Time.deltaTime);

        HandleRotation();


    }



}
