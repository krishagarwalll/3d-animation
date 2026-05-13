using UnityEngine;
using UnityEditor;

public class KitBashHeightAssigner : Editor
{
    [MenuItem("Assets/KitBash3D/Auto-Assign Height Maps")]
    public static void AssignHeightMaps()
    {
        // 1. Get only the Materials you currently have selected
        Object[] selectedObjects = Selection.GetFiltered(typeof(Material), SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one Material in the Project window.");
            return;
        }

        int processedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            Material mat = obj as Material;

            // Ensure we are only modifying HDRP Lit materials
            if (mat == null || mat.shader.name != "HDRP/Lit")
            {
                continue;
            }

            string matName = mat.name;
            string baseName = matName;

            // 2. THE FIX: If the material name ends with "_basecolor", chop it off
            if (matName.ToLower().EndsWith("_basecolor"))
            {
                // "_basecolor" is 10 characters long, so we remove the last 10 characters
                baseName = matName.Substring(0, matName.Length - 10);
            }

            // 3. Search the project for the clean name + "_height"
            string searchQuery = baseName + "_height t:Texture2D";
            string[] foundGuids = AssetDatabase.FindAssets(searchQuery);

            if (foundGuids.Length > 0)
            {
                // Grab the first texture it finds that matches the name
                string texPath = AssetDatabase.GUIDToAssetPath(foundGuids[0]);
                Texture2D heightTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

                if (heightTex != null)
                {
                    // Assign the texture to the Height Map slot
                    mat.SetTexture("_HeightMap", heightTex);

                    // Turn on Pixel Displacement automatically
                    mat.SetFloat("_DisplacementMode", 1f); 
                    mat.EnableKeyword("_PIXEL_DISPLACEMENT"); 

                    // Set a safe default height amplitude
                    mat.SetFloat("_HeightAmplitude", 0.02f); 

                    // Tell Unity this material has been modified
                    EditorUtility.SetDirty(mat);
                    processedCount++;
                }
            }
            else
            {
                Debug.Log($"Skipped {mat.name}: Could not find a texture named '{baseName}_height'.");
            }
        }

        // Save all the changes to the hard drive
        AssetDatabase.SaveAssets();
        Debug.Log($"<color=cyan><b>Finished!</b></color> Assigned height maps and enabled Pixel Displacement on {processedCount} materials.");
    }
}