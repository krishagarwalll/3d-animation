using UnityEngine;
using UnityEditor;

public class SceneFBXToPrefab : Editor
{
    [MenuItem("GameObject/KitBash3D/Convert Selected to Prefab In-Place", false, 0)]
    public static void ConvertSceneObjectsToPrefab()
    {
        // 1. Get objects selected in the Hierarchy (Scene)
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one object in the Hierarchy.");
            return;
        }

        // 2. Ensure the Prefabs folder exists
        string saveFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        int processedCount = 0;

        foreach (GameObject selectedObj in selectedObjects)
        {
            // Skip if the user accidentally selected a file in the Project window instead of the Hierarchy
            if (AssetDatabase.Contains(selectedObj))
            {
                Debug.LogWarning($"Skipped {selectedObj.name}: This script is for objects already in the Scene.");
                continue;
            }

            // 3. Save the current World position, rotation, and scale
            Vector3 worldPos = selectedObj.transform.position;
            Quaternion worldRot = selectedObj.transform.rotation;
            Vector3 worldScale = selectedObj.transform.localScale;

            // 4. Create the new Empty Parent object
            GameObject newParent = new GameObject(selectedObj.name + "_Prefab");
            
            // Register this creation so you can CTRL+Z (Undo) if you make a mistake
            Undo.RegisterCreatedObjectUndo(newParent, "Convert to Prefab In-Place");

            // Match the parent to the exact world space of the original object
            newParent.transform.position = worldPos;
            newParent.transform.rotation = worldRot;
            newParent.transform.localScale = worldScale;

            // 5. Parent the original FBX inside the new Empty object
            Undo.SetTransformParent(selectedObj.transform, newParent.transform, "Parent FBX to Empty");
            
            // Reset the child's local transform so the Parent dictates its exact placement
            selectedObj.transform.localPosition = Vector3.zero;
            selectedObj.transform.localRotation = Quaternion.identity;
            selectedObj.transform.localScale = Vector3.one;

            // 6. Save as a brand new Prefab file AND connect the scene object to it automatically
            string savePath = $"{saveFolder}/{newParent.name}.prefab";
            savePath = AssetDatabase.GenerateUniqueAssetPath(savePath); // Prevents overwriting
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(newParent, savePath, InteractionMode.UserAction);

            processedCount++;
        }

        Debug.Log($"<color=cyan><b>Success!</b></color> Converted {processedCount} Scene objects into Prefabs. Your scene layout was preserved.");
    }
}