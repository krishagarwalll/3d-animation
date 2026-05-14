using UnityEngine;
using UnityEditor;
using System.IO;

public class KitBashAlphaPacker : Editor
{
    // This creates a new button in the top menu bar AND the right-click menu
    [MenuItem("Assets/KitBash3D/Pack Opacity to BaseColor Alpha")]
    public static void PackSelectedTextures()
    {
        // Get all currently selected textures in the project window
        Object[] selectedObjects = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one '_basecolor' texture in the Project window.");
            return;
        }

        int processedCount = 0;

        foreach (Object obj in selectedObjects)
        {
            Texture2D baseTex = obj as Texture2D;
            string assetPath = AssetDatabase.GetAssetPath(baseTex);

            // Ignore anything that isn't a basecolor texture
            if (!assetPath.ToLower().Contains("_basecolor"))
                continue;

            // Extract the base name (e.g., "Assets/Textures/KB3D_CPV_DecalsAtlas")
            string basePath = assetPath.Substring(0, assetPath.LastIndexOf("_basecolor", System.StringComparison.OrdinalIgnoreCase));
            
            // Find the matching mask
            Texture2D maskTex = FindMatchingMask(basePath);

            if (maskTex == null)
            {
                Debug.LogWarning($"Skipped {baseTex.name}: Could not find a matching _opacity or _refraction texture in the same folder.");
                continue;
            }

            // Unity needs permission to read the pixels of the textures to combine them
            MakeTextureReadable(baseTex);
            MakeTextureReadable(maskTex);

            // Create a new texture with an Alpha channel (RGBA32)
            Texture2D packedTex = new Texture2D(baseTex.width, baseTex.height, TextureFormat.RGBA32, true);
            Color[] basePixels = baseTex.GetPixels();
            Color[] maskPixels = maskTex.GetPixels();

            if (basePixels.Length != maskPixels.Length)
            {
                Debug.LogError($"Resolution mismatch! {baseTex.name} and {maskTex.name} must be the same size. Skipping.");
                continue;
            }

            // Loop through every pixel and combine the RGB from the basecolor and the Red channel from the mask into the Alpha
            Color[] packedPixels = new Color[basePixels.Length];
            for (int i = 0; i < basePixels.Length; i++)
            {
                packedPixels[i] = new Color(basePixels[i].r, basePixels[i].g, basePixels[i].b, maskPixels[i].r);
            }

            packedTex.SetPixels(packedPixels);
            packedTex.Apply();

            // Save the new texture as a PNG
            byte[] bytes = packedTex.EncodeToPNG();
            string newAssetPath = basePath + "_basecolor_alpha.png";
            
            // Convert Unity's relative path to an absolute system path
            string fullSystemPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), newAssetPath);

            File.WriteAllBytes(fullSystemPath, bytes);
            Debug.Log($"Successfully packed: {newAssetPath}");
            processedCount++;
        }

        // Tell Unity to refresh so the new files show up in the project window
        AssetDatabase.Refresh();
        Debug.Log($"Finished! Packed {processedCount} textures.");
    }

    private static Texture2D FindMatchingMask(string basePath)
    {
        // Prioritize opacity, then inverted refraction, then standard refraction
        string[] possibleSuffixes = { "_opacity", "_refraction.inverted", "_refraction" };
        string[] possibleExtensions = { ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff" };

        foreach (string suffix in possibleSuffixes)
        {
            foreach (string ext in possibleExtensions)
            {
                string checkPath = basePath + suffix + ext;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(checkPath);
                if (tex != null)
                {
                    return tex;
                }
            }
        }
        return null;
    }

    private static void MakeTextureReadable(Texture2D tex)
    {
        string path = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}