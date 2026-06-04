using UnityEngine;

public class ScrollHologram : MonoBehaviour
{
    public float scrollSpeedX = 0.5f;
    public float scrollSpeedY = 0.0f;
    
    private Material mat;
    private int baseColorID;
    private int emissionID;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        
        // Cache the HDRP property IDs for better performance
        baseColorID = Shader.PropertyToID("_BaseColorMap");
        emissionID = Shader.PropertyToID("_EmissiveColorMap");
    }

    void Update()
    {
        float offsetX = Time.time * scrollSpeedX;
        float offsetY = Time.time * scrollSpeedY;
        Vector2 offset = new Vector2(offsetX, offsetY);
        
        // Scroll Base Color
        if (mat.HasProperty(baseColorID))
        {
            mat.SetTextureOffset(baseColorID, offset);
        }
        
        // Scroll Emission (The glowing part!)
        if (mat.HasProperty(emissionID))
        {
            mat.SetTextureOffset(emissionID, offset);
        }
    }
}