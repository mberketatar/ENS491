using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

[AddComponentMenu("XR/Grab Transformers/Lever Force Grab Transformer")]
[RequireComponent(typeof(Rigidbody))]
public class LeverForceGrabTransformer : MonoBehaviour, IXRGrabTransformer
{
    [SerializeField]
    private Vector3 localRotationAxis = Vector3.up;

    [SerializeField]
    private float torqueStrength = 100f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Transform interactorTransform;
    private Vector3 lastInteractorPosition;

    public bool canProcess => interactorTransform != null;

    public void OnLink(XRGrabInteractable grabInteractable)
    {
        this.grabInteractable = grabInteractable;
        rb = grabInteractable.GetComponent<Rigidbody>();
    }

    public void OnGrab(XRGrabInteractable grabInteractable)
    {
        if (grabInteractable.interactorsSelecting.Count > 0)
        {
            interactorTransform = grabInteractable.interactorsSelecting[0].transform;
            lastInteractorPosition = interactorTransform.position;
        }
    }

    public void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
    {
        // Not needed for single-hand lever
    }

    public void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        if (interactorTransform == null || rb == null)
            return;

        Vector3 worldAxis = grabInteractable.transform.TransformDirection(localRotationAxis);
        Vector3 leverToHand = interactorTransform.position - grabInteractable.transform.position;
        Vector3 handDelta = interactorTransform.position - lastInteractorPosition;

        Vector3 torqueDir = Vector3.Cross(leverToHand, handDelta).normalized;
        float torqueAmount = Vector3.Dot(torqueDir, worldAxis);

        Vector3 torque = worldAxis * torqueAmount * torqueStrength;
        rb.AddTorque(torque, ForceMode.Force);

        lastInteractorPosition = interactorTransform.position;

        // We don’t override targetPose — we let physics handle it
        targetPose.position = grabInteractable.transform.position;
        targetPose.rotation = grabInteractable.transform.rotation;
    }

    public void OnUnlink(XRGrabInteractable grabInteractable)
    {
        interactorTransform = null;
    }
}
