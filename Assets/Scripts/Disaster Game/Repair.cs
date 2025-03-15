using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public enum ResourceType
{
    Water,
    Electricity,
    NaturalGas
}

[RequireComponent(typeof(XRBaseInteractable))]
public class Repair : MonoBehaviour
{
    private XRBaseInteractable interactable;
    private Renderer cubeRenderer;

    // Particle effect for the broken state (assign in the Inspector)
    public ParticleSystem brokenParticles;

    // Choose which resource this object consumes when broken.
    public ResourceType resourceType;
    // How fast the resource is consumed (units per second)
    public float consumptionRate = 1f;

    // Track whether the cube is broken. Starts as not broken (false).
    private bool isBroken = false;

    // Time range (in seconds) before the cube automatically breaks.
    public float minTimeToBreak = 5f;
    public float maxTimeToBreak = 15f;

    private Coroutine breakTimerCoroutine;
    private Coroutine consumptionCoroutine;

    // Reference to the ResourceManager in your scene.
    private ResourceManager resourceManager;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        cubeRenderer = GetComponent<Renderer>();
        resourceManager = FindObjectOfType<ResourceManager>();

        // Start with the cube repaired (green) and start the break timer.
        SetRepaired();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Only allow a click to repair the cube if it is broken.
        if (isBroken)
        {
            RepairCube();
        }
    }

    private IEnumerator BreakTimer()
    {
        // Wait for a random time between minTimeToBreak and maxTimeToBreak seconds.
        float waitTime = Random.Range(minTimeToBreak, maxTimeToBreak);
        yield return new WaitForSeconds(waitTime);

        // Only break the cube if it is still repaired (green).
        if (!isBroken)
        {
            SetBroken();
        }
    }

    private void SetBroken()
    {
        isBroken = true;
        cubeRenderer.material.color = Color.red;
        if (brokenParticles != null)
        {
            brokenParticles.Play();
        }

        // Start consuming the assigned resource.
        consumptionCoroutine = StartCoroutine(ConsumeResource());
    }

    private void SetRepaired()
    {
        isBroken = false;
        cubeRenderer.material.color = Color.green;
        if (brokenParticles != null)
        {
            brokenParticles.Stop();
        }

        // Stop resource consumption if it was running.
        if (consumptionCoroutine != null)
        {
            StopCoroutine(consumptionCoroutine);
        }
        
        // Restart the break timer.
        if (breakTimerCoroutine != null)
        {
            StopCoroutine(breakTimerCoroutine);
        }
        breakTimerCoroutine = StartCoroutine(BreakTimer());
    }

    // Consumes the selected resource over time while the cube is broken.
    private IEnumerator ConsumeResource()
    {
        while (isBroken)
        {
            float consumptionThisFrame = consumptionRate * Time.deltaTime;
            switch (resourceType)
            {
                case ResourceType.Water:
                    resourceManager.water -= consumptionThisFrame;
                    break;
                case ResourceType.Electricity:
                    resourceManager.electricity -= consumptionThisFrame;
                    break;
                case ResourceType.NaturalGas:
                    resourceManager.naturalGas -= consumptionThisFrame;
                    break;
            }
            yield return null;
        }
    }

    private void RepairCube()
    {
        SetRepaired();
    }

    // Expose the broken state to other scripts
    public bool IsBroken
    {
        get { return isBroken; }
    }
}
