using System.Collections.Generic;
using UnityEngine;

public class SetRandomColor : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderers;
    [SerializeField] private float hueMin = 0f;
    [SerializeField] private float hueMax = 1f;
    [SerializeField] private float saturationMin = 0.5f;
    [SerializeField] private float saturationMax = 1f;
    [SerializeField] private float valueMin = 0.5f;
    [SerializeField] private float valueMax = 1f;

    public MarchGameVariables marchGameVariables;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        marchGameVariables.currentUnits++;
        float newHue = Random.Range(hueMin, hueMax);
        float newSaturation = Random.Range(saturationMin, saturationMax);
        newSaturation = Mathf.Round(newSaturation / 0.05f) * 0.05f;

        foreach (var spriteRenderer in spriteRenderers)
        {
            // Get current color in RGB
            Color currentColor = spriteRenderer.color;

            // Convert current RGB to HSV
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);

            // Generate a new color while keeping the original value (V)
            var newColor = Color.HSVToRGB(newHue, newSaturation, v);

            // Assign the new color
            spriteRenderer.color = newColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
