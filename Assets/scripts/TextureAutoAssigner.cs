using UnityEngine;
using UnityEditor;

public class TextureAutoAssigner : Editor
{
    // Creates the button right next to your renaming tool
    [MenuItem("Tools/KitBash Tools/Auto-Assign Textures to Selected")]
    public static void AssignTextures()
    {
        // Get highlighted materials
        Material[] selectedMaterials = Selection.GetFiltered<Material>(SelectionMode.Assets);

        if (selectedMaterials.Length == 0)
        {
            Debug.LogWarning("No materials selected! Please highlight materials in the Project window.");
            return;
        }

        int assignCount = 0;

        foreach (Material mat in selectedMaterials)
        {
            string matName = mat.name;
            bool updated = false;

            // 1. Find and assign the Base Color
            Texture2D baseColor = FindTexture(matName + "_basecolor");
            if (baseColor != null)
            {
                mat.SetTexture("_BaseColorMap", baseColor);
                updated = true;
            }

            // 2. Find and assign the Mask Map
            Texture2D maskMap = FindTexture(matName + "_maskmap");
            if (maskMap != null)
            {
                mat.SetTexture("_MaskMap", maskMap);
                updated = true;
            }

            // 3. Find and assign the Normal Map
            Texture2D normalMap = FindTexture(matName + "_normal");
            if (normalMap != null)
            {
                mat.SetTexture("_NormalMap", normalMap);
                updated = true;
            }

            if (updated) assignCount++;
        }

        // Save everything to the hard drive
        AssetDatabase.SaveAssets();
        Debug.Log($"Success! Auto-assigned textures to {assignCount} materials.");
    }

    // Helper function that searches your entire Unity project for the file
    private static Texture2D FindTexture(string textureName)
    {
        // Ask Unity's database to find a Texture2D with this exact string
        string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");

        if (guids.Length > 0)
        {
            // Convert the search result ID into a usable file path, then load it
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        
        // Return nothing if the file isn't found
        return null;
    }
}