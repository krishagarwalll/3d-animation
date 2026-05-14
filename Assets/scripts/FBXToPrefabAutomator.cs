using UnityEngine;
using UnityEditor;
using System.IO;

public class FBXToPrefabAutomator : Editor
{
    [MenuItem("Assets/KitBash3D/Convert FBX to Editable Prefab")]
    public static void ConvertToPrefab()
    {
        // 1. Get all selected objects in the Project window
        GameObject[] selectedObjects = Selection.GetFiltered<GameObject>(SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one FBX model in the Project window.");
            return;
        }

        // 2. Define where the new prefabs will be saved. 
        // If the folder doesn't exist, the script creates it automatically.
        string saveFolder = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        int processedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            
            // Only process actual model files (ignores folders, materials, etc.)
            if (!assetPath.ToLower().EndsWith(".fbx") && !assetPath.ToLower().EndsWith(".obj"))
            {
                continue;
            }

            // 3. Temporarily instantiate the FBX into the hidden scene space
            GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            if (fbxInstance == null) continue;

            // 4. Create the new Empty GameObject to act as the parent
            GameObject newParent = new GameObject(obj.name + "_Prefab");

            // 5. Parent the FBX to the Empty Object and reset its position to 0,0,0
            fbxInstance.transform.SetParent(newParent.transform);
            fbxInstance.transform.localPosition = Vector3.zero;
            fbxInstance.transform.localRotation = Quaternion.identity;
            fbxInstance.transform.localScale = Vector3.one;

            // 6. Generate a safe save path (prevents overwriting if two objects have the same name)
            string savePath = $"{saveFolder}/{newParent.name}.prefab";
            savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

            // 7. Save the new hierarchy as a true Unity Prefab file
            PrefabUtility.SaveAsPrefabAsset(newParent, savePath);

            // 8. Delete the temporary objects from the scene memory so we don't cause a mess
            DestroyImmediate(newParent);
            
            processedCount++;
        }

        // Tell Unity to refresh the project window so the new folder and files appear immediately
        AssetDatabase.Refresh();
        Debug.Log($"<color=green><b>Success!</b></color> Converted {processedCount} models into fully editable Prefabs. You can find them in {saveFolder}");
    }
}