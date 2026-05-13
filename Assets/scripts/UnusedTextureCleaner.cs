using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UnusedTextureCleaner : EditorWindow
{
    [MenuItem("Assets/KitBash3D/Find and Delete Unused Textures")]
    public static void FindAndCleanUnusedTextures()
    {
        // 1. Find all materials in the "Assets" folder
        string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
        HashSet<string> usedTexturePaths = new HashSet<string>();

        // 2. Collect all texture dependencies from those materials
        EditorUtility.DisplayProgressBar("Scanning", "Scanning Materials for Texture links...", 0.2f);
        foreach (string guid in matGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            
            if (mat != null)
            {
                // Look at everything this material is touching
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { mat });
                foreach (Object dep in dependencies)
                {
                    if (dep is Texture)
                    {
                        usedTexturePaths.Add(AssetDatabase.GetAssetPath(dep));
                    }
                }
            }
        }

        // 3. Find all textures in the "Assets" folder
        EditorUtility.DisplayProgressBar("Scanning", "Cross-referencing Textures...", 0.6f);
        string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets" });
        List<string> unusedTexturePaths = new List<string>();

        foreach (string guid in texGuids)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(guid);
            
            // If the texture's path isn't in our used list, flag it as unused
            if (!usedTexturePaths.Contains(texPath))
            {
                unusedTexturePaths.Add(texPath);
            }
        }
        
        EditorUtility.ClearProgressBar();

        // 4. Report and Confirm
        if (unusedTexturePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Texture Cleaner", "No unused textures found! Your project is perfectly clean.", "Awesome");
            return;
        }

        // The Safety Popup
        bool delete = EditorUtility.DisplayDialog(
            "Unused Textures Found",
            $"Found {unusedTexturePaths.Count} textures that are NOT used in any Material.\n\nDo you want to move them to the Mac Trash Bin?\n\nWARNING: If you use textures for UI Canvases or raw Particle Emitters without materials, they may be included in this list.",
            "Yes, move to Trash",
            "Cancel"
        );

        // 5. Execute the Deletion
        if (delete)
        {
            int deletedCount = 0;
            foreach (string path in unusedTexturePaths)
            {
                if (AssetDatabase.MoveAssetToTrash(path))
                {
                    deletedCount++;
                }
            }
            AssetDatabase.Refresh();
            Debug.Log($"<color=green><b>Success:</b></color> Moved {deletedCount} unused textures to the Trash.");
        }
        else
        {
            Debug.Log("Texture cleanup cancelled by user.");
        }
    }
}