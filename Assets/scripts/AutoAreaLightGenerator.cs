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

            // Fetch the bounding box of the mesh to calculate its actual shape
            Vector3 size = filter.sharedMesh.bounds.size;
            Vector3 extents = filter.sharedMesh.bounds.extents;
            Vector3 center = filter.sharedMesh.bounds.center;

            // Find the thinnest side of the mesh (this represents the screen's depth)
            float minSize = Mathf.Min(size.x, Mathf.Min(size.y, size.z));

            float lightWidth = 1f;
            float lightHeight = 1f;
            Vector3 offsetDirection = Vector3.zero;
            Quaternion lightRotation = Quaternion.identity;

            // Dynamically rotate the light based on which way the screen is facing
            if (minSize == size.z)
            {
                lightWidth = size.x * obj.transform.localScale.x;
                lightHeight = size.y * obj.transform.localScale.y;
                offsetDirection = new Vector3(0, 0, extents.z + 0.05f);
                lightRotation = Quaternion.identity; // Faces Local Z
            }
            else if (minSize == size.y)
            {
                lightWidth = size.x * obj.transform.localScale.x;
                lightHeight = size.z * obj.transform.localScale.z;
                offsetDirection = new Vector3(0, extents.y + 0.05f, 0);
                lightRotation = Quaternion.Euler(90, 0, 0); // Faces Local Y
            }
            else
            {
                lightWidth = size.z * obj.transform.localScale.z;
                lightHeight = size.y * obj.transform.localScale.y;
                offsetDirection = new Vector3(extents.x + 0.05f, 0, 0);
                lightRotation = Quaternion.Euler(0, -90, 0); // Faces Local X
            }

            // Apply calculated position and rotation
            lightObj.transform.localPosition = center + offsetDirection;
            lightObj.transform.localRotation = lightRotation;

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Rectangle;
            HDAdditionalLightData hdLight = lightObj.AddComponent<HDAdditionalLightData>();

            // Apply size
            light.areaSize = new Vector2(Mathf.Max(lightWidth, 0.5f), Mathf.Max(lightHeight, 0.5f));

            // Steal color
            Material mat = renderer.sharedMaterial;
            if (mat != null && mat.HasProperty("_EmissiveColor"))
            {
                Color emColor = mat.GetColor("_EmissiveColor");
                Color.RGBToHSV(emColor, out float h, out float s, out float v);
                light.color = Color.HSVToRGB(h, s, 1f);
            }

            // Apply HDRP Lighting Settings
            light.lightUnit = UnityEngine.Rendering.LightUnit.Ev100;
            light.intensity = 10f; 
            hdLight.volumetricDimmer = 2f; 

            Undo.RegisterCreatedObjectUndo(lightObj, "Create Auto Area Light");
            processedCount++;
        }

        Debug.Log($"<color=cyan><b>Success!</b></color> Generated {processedCount} Smart Area Lights.");
    }
}