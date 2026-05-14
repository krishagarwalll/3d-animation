using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;

public class AutoAreaLightGenerator : Editor
{
    [MenuItem("GameObject/KitBash3D/Generate Area Lights for Screens", false, 11)]
    public static void GenerateAreaLights()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one screen/billboard in the Hierarchy.");
            return;
        }

        int processedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            MeshFilter filter = obj.GetComponent<MeshFilter>();

            if (renderer == null || filter == null) continue;

            GameObject lightObj = new GameObject(obj.name + "_AreaLight");
            lightObj.transform.SetParent(obj.transform);
            
            lightObj.transform.localPosition = new Vector3(0, 0, 0.1f); 
            lightObj.transform.localRotation = Quaternion.identity;
            lightObj.transform.localScale = Vector3.one;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Rectangle;

            HDAdditionalLightData hdLight = lightObj.AddComponent<HDAdditionalLightData>();

            // Calculate the size based on the mesh bounding box
            Vector3 meshSize = filter.sharedMesh.bounds.size;
            float width = Mathf.Max(meshSize.x * obj.transform.localScale.x, 0.5f);
            float height = Mathf.Max(meshSize.y * obj.transform.localScale.y, 0.5f);

            // THE FIX 1 & 2: Set the Area Size directly on the standard Light component
            light.areaSize = new Vector2(width, height);

            Material mat = renderer.sharedMaterial;
            if (mat != null && mat.HasProperty("_EmissiveColor"))
            {
                Color emColor = mat.GetColor("_EmissiveColor");
                Color.RGBToHSV(emColor, out float h, out float s, out float v);
                light.color = Color.HSVToRGB(h, s, 1f);
            }

            // THE FIX 3: Set the Light Unit directly on the standard Light component
            light.lightUnit = UnityEngine.Rendering.LightUnit.Ev100;
            light.intensity = 10f; 
            
            // Volumetric dimmer remains on the HDAdditionalLightData for now
            hdLight.volumetricDimmer = 2f; 

            Undo.RegisterCreatedObjectUndo(lightObj, "Create Auto Area Light");
            processedCount++;
        }

        Debug.Log($"<color=cyan><b>Success!</b></color> Generated {processedCount} HDRP Area Lights.");
    }
}