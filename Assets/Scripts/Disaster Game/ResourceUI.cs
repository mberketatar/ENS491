using UnityEngine;
using UnityEngine.UI;

public class ResourceUI : MonoBehaviour
{
    // Reference to the ResourceManager.
    public ResourceManager resourceManager;

    // Slider components for each resource.
    public Slider waterSlider;
    public Slider electricitySlider;
    public Slider naturalGasSlider;

    // Text components to display the resource values.
    public Text waterText;
    public Text electricityText;
    public Text naturalGasText;

    // Maximum resource value, should match the ResourceManager's maxResource.
    public float maxResource = 100f;

    private void Start()
    {
        // Initialize the maximum value for the sliders.
        if (waterSlider) waterSlider.maxValue = maxResource;
        if (electricitySlider) electricitySlider.maxValue = maxResource;
        if (naturalGasSlider) naturalGasSlider.maxValue = maxResource;
    }

    private void Update()
    {
        if (resourceManager != null)
        {
            // Update sliders' values.
            if (waterSlider) waterSlider.value = resourceManager.water;
            if (electricitySlider) electricitySlider.value = resourceManager.electricity;
            if (naturalGasSlider) naturalGasSlider.value = resourceManager.naturalGas;

            // Update text with formatted resource values.
            if (waterText) waterText.text = "Water: " + resourceManager.water.ToString("F0");
            if (electricityText) electricityText.text = "Electricity: " + resourceManager.electricity.ToString("F0");
            if (naturalGasText) naturalGasText.text = "Natural Gas: " + resourceManager.naturalGas.ToString("F0");

    if (resourceManager != null)
    {
        Debug.Log("Water: " + resourceManager.water);
    }

        }
    }
}
