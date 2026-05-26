using UnityEngine;
using UnityEditor;

public class MaterialRenamer : Editor
{
    // This creates a custom button in the top menu bar
    [MenuItem("Tools/KitBash Tools/Remove '_basecolor' from Selected")]
    public static void RenameMaterials()
    {
        // Grab all the materials you currently have highlighted in the Project window
        Material[] selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets);

        if (selectedMaterials.Length == 0)
        {
            Debug.LogWarning("No materials selected! Please highlight some materials in your Project window first.");
            return;
        }

        int renameCount = 0;
        string suffix = "_basecolor";

        // Loop through everything you selected
        foreach (Material mat in selectedMaterials)
        {
            string oldName = mat.name;

            // Check if the name ends with "_basecolor" (ignoring uppercase/lowercase just in case)
            if (oldName.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                // Chop off the suffix
                string newName = oldName.Substring(0, oldName.Length - suffix.Length);
                
                // Tell Unity to rename the actual file on your hard drive
                string assetPath = AssetDatabase.GetAssetPath(mat);
                AssetDatabase.RenameAsset(assetPath, newName);
                
                renameCount++;
            }
        }

        // Save the changes so they stick
        AssetDatabase.SaveAssets();
        Debug.Log($"Success! Cleaned up {renameCount} KitBash materials.");
    }
}