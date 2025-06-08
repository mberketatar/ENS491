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

[RequireComponent(typeof(XRSimpleInteractable))]
public class Repair : MonoBehaviour
{
    [Header("Broken‐Cube Visuals")]
    [SerializeField] private ParticleSystem brokenParticles;

    [Header("Resource Consumption")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private float        consumptionRate = 1f;

    [Header("Random Break Timing")]
    [SerializeField] private float minTimeToBreak = 5f;
    [SerializeField] private float maxTimeToBreak = 15f;

    // Internals
    private XRSimpleInteractable simpleInteractable;
    private Renderer             cubeRenderer;
    private ResourceManager      resourceManager;

    private bool    isBroken;
    private Coroutine breakTimerCoroutine;
    private Coroutine consumptionCoroutine;

    private void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        cubeRenderer       = GetComponent<Renderer>();
        resourceManager    = FindObjectOfType<ResourceManager>();

        // **DON’T** call SetRepaired() here anymore.
        // That will only be invoked when the game actually starts.
    }

    private void OnEnable()
    {
        simpleInteractable.activated.AddListener(OnActivate);
    }

    private void OnDisable()
    {
        simpleInteractable.activated.RemoveListener(OnActivate);
    }

    private void OnActivate(ActivateEventArgs args)
    {
        if (isBroken)
            RepairCube();
    }

    /// <summary>
    /// Called by GameManager.StartGame():  
    /// resets to green *and* kicks off the break timer.
    /// </summary>
    public void BeginBreakCycle()
    {
        StopAllCoroutines();
        SetRepaired();    // this also starts the BreakTimer()
    }

    /// <summary>
    /// Called by GameManager.ResetGame():  
    /// immediately force‐reset to green.
    /// </summary>
    public void ResetRepair()
    {
        StopAllCoroutines();
        SetRepaired();
    }

    private IEnumerator BreakTimer()
    {
        float waitTime = Random.Range(minTimeToBreak, maxTimeToBreak);
        yield return new WaitForSeconds(waitTime);

        if (!isBroken)
            SetBroken();
    }

    private void SetBroken()
    {
        isBroken = true;
        Debug.Log($"{name} BROKE!");
        cubeRenderer.material.color = Color.red;
        brokenParticles?.Play();

        consumptionCoroutine = StartCoroutine(ConsumeResource());
    }

    private void SetRepaired()
    {
        isBroken = false;
        cubeRenderer.material.color = Color.green;
        brokenParticles?.Stop();

        // stop any draining
        if (consumptionCoroutine != null)
            StopCoroutine(consumptionCoroutine);

        // restart the break timer
        if (breakTimerCoroutine != null)
            StopCoroutine(breakTimerCoroutine);
        breakTimerCoroutine = StartCoroutine(BreakTimer());
    }

    private IEnumerator ConsumeResource()
    {
        while (isBroken)
        {
            float delta = consumptionRate * Time.deltaTime;
            switch (resourceType)
            {
                case ResourceType.Water:       resourceManager.water       -= delta; break;
                case ResourceType.Electricity: resourceManager.electricity -= delta; break;
                case ResourceType.NaturalGas:  resourceManager.naturalGas  -= delta; break;
            }
            yield return null;
        }
    }

    private void RepairCube()
    {
        SetRepaired();
    }

    /// <summary>Optional: if you need to peek at broken state elsewhere.</summary>
    public bool IsBroken => isBroken;
}
