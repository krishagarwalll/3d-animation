using UnityEngine;
using System.Collections.Generic;

public class MultiMaterialBlinker : MonoBehaviour
{
    // A small structure to hold both the material and its unique original color
    private struct BlinkerMaterialData
    {
        public Material material;
        public Color originalEmissiveColor;
    }

    [Header("Material Settings")]
    [Tooltip("Click the dropdown in the Inspector, set Size to 5, and enter your element numbers.")]
    public int[] materialIndices; 
    public float blinkSpeed = 4.0f;
    
    [Header("Blink Style")]
    [Tooltip("If true, it fades smoothly. If false, it snaps sharply on/off.")]
    public bool smoothPulse = false;

    private List<BlinkerMaterialData> blinkerDataList = new List<BlinkerMaterialData>();

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        
        if (renderer == null)
        {
            Debug.LogError("Mesh Renderer missing from this GameObject!");
            enabled = false;
            return;
        }

        Material[] runtimeMaterials = renderer.materials;

        foreach (int index in materialIndices)
        {
            if (index >= 0 && index < runtimeMaterials.Length)
            {
                Material mat = runtimeMaterials[index];
                
                // Get the unique HDR color and intensity you already configured on this material
                Color nativeColor = mat.GetColor("_EmissiveColor");

                // Save both the material instance and its color into our data list
                BlinkerMaterialData data = new BlinkerMaterialData
                {
                    material = mat,
                    originalEmissiveColor = nativeColor
                };

                blinkerDataList.Add(data);
            }
            else
            {
                Debug.LogWarning($"Material Index {index} is out of bounds for this mesh!");
            }
        }
    }

    void Update()
    {
        // 1. Calculate the blink multiplier (either 0 or 1)
        float intensityMultiplier = 0f;
        if (smoothPulse)
        {
            intensityMultiplier = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
        }
        else
        {
            intensityMultiplier = (Mathf.FloorToInt(Time.time * blinkSpeed) % 2 == 0) ? 1f : 0f;
        }

        // 2. Loop through our saved data and blink each material using its OWN color
        for (int i = 0; i < blinkerDataList.Count; i++)
        {
            BlinkerMaterialData data = blinkerDataList[i];
            
            if (data.material != null)
            {
                // Multiply the material's specific original color by 0 (off) or 1 (on)
                Color finalColor = data.originalEmissiveColor * intensityMultiplier;
                
                data.material.SetColor("_EmissiveColor", finalColor);
            }
        }
    }
    
    void OnDestroy()
    {
        for (int i = 0; i < blinkerDataList.Count; i++)
        {
            if (blinkerDataList[i].material != null)
            {
                Destroy(blinkerDataList[i].material);
            }
        }
    }
}