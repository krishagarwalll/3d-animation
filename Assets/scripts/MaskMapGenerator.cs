using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MaskMapGenerator : EditorWindow
{
    [MenuItem("Tools/Batch Generate Mask Maps")]
    public static void Generate()
    {
        string inputPath = EditorUtility.OpenFolderPanel("1. Select SOURCE Texture Folder", "Assets", "");
        if (string.IsNullOrEmpty(inputPath)) return;

        string outputPath = EditorUtility.OpenFolderPanel("2. Select DESTINATION Folder", inputPath, "");
        if (string.IsNullOrEmpty(outputPath)) return;

        bool deleteOriginals = EditorUtility.DisplayDialog(
            "Delete Source Files?", 
            "Do you want to PERMANENTLY DELETE the original Metallic, AO, and Roughness files after the Mask Maps are generated?\n\nThis cannot be undone!", 
            "Yes, Delete Them", 
            "No, Keep Them"
        );

        string[] allFiles = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories);

        Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();
        int validTextureCount = 0;

        foreach (string file in allFiles)
        {
            if (file.EndsWith(".meta")) continue;

            string ext = Path.GetExtension(file).ToLower();
            
            // FIX 1: Added .tga to the allowed extensions
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".tga") continue;

            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileNameLower = fileName.ToLower();
            string baseName = "";
            
            // FIX 2: Explicitly remove known suffixes to ensure perfectly matching base names
            if (fileNameLower.EndsWith("_metallic"))
                baseName = fileName.Substring(0, fileName.Length - 9);
            else if (fileNameLower.EndsWith("_mixed_ao"))
                baseName = fileName.Substring(0, fileName.Length - 9);
            else if (fileNameLower.EndsWith("_ao"))
                baseName = fileName.Substring(0, fileName.Length - 3);
            else if (fileNameLower.EndsWith("_roughness"))
                baseName = fileName.Substring(0, fileName.Length - 10);
            else
                continue; // Skip files that aren't target maps (e.g., Base_Color, Normal)

            validTextureCount++;
            
            if (!groups.ContainsKey(baseName)) groups[baseName] = new List<string>();
            groups[baseName].Add(file);
        }

        if (validTextureCount == 0)
        {
            Debug.LogError("ERROR: Found 0 matching texture files (Metallic, AO, Roughness) in the selected folder!");
            return;
        }

        int count = 0;
        foreach (var group in groups)
        {
            if (PackMaskMap(group.Key, group.Value, outputPath, deleteOriginals)) count++;
        }
        
        AssetDatabase.Refresh();
        Debug.Log($"Success! Generated {count} Mask Maps.");
        if (deleteOriginals) Debug.Log("Original source files were successfully cleaned up.");
    }

    static bool PackMaskMap(string baseName, List<string> files, string outputPath, bool deleteOriginals)
    {
        Texture2D metallic = null, ao = null, roughness = null;
        string metallicPath = "", aoPath = "", roughnessPath = "";

        foreach (string path in files)
        {
            // FIX 3: Load textures via AssetDatabase to natively support TGA formats
            string relativePath = FileUtil.GetProjectRelativePath(path);
            
            // Ensure texture is readable before trying to use GetPixel()
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            if (tex == null) continue;

            string fileNameLower = Path.GetFileNameWithoutExtension(path).ToLower();

            if (fileNameLower.EndsWith("_metallic")) { metallic = tex; metallicPath = path; }
            else if (fileNameLower.EndsWith("_mixed_ao") || fileNameLower.EndsWith("_ao")) { ao = tex; aoPath = path; }
            else if (fileNameLower.EndsWith("_roughness")) { roughness = tex; roughnessPath = path; }
        }

        if (metallic == null && ao == null && roughness == null) return false;

        int width = metallic ? metallic.width : (ao ? ao.width : roughness.width);
        int height = metallic ? metallic.height : (ao ? ao.height : roughness.height);

        Texture2D maskMap = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float r = metallic ? metallic.GetPixel(x, y).r : 0f;
                float g = ao ? ao.GetPixel(x, y).r : 1f; 
                float b = 0f; 
                float a = roughness ? 1f - roughness.GetPixel(x, y).r : 1f; 

                maskMap.SetPixel(x, y, new Color(r, g, b, a));
            }
        }

        byte[] pngData = maskMap.EncodeToPNG();
        string finalFilePath = Path.Combine(outputPath, baseName + "_MaskMap.png");
        File.WriteAllBytes(finalFilePath, pngData);

        if (deleteOriginals)
        {
            if (metallic != null) SafeDelete(metallicPath);
            if (ao != null) SafeDelete(aoPath);
            if (roughness != null) SafeDelete(roughnessPath);
        }
        
        return true;
    }

    static void SafeDelete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
        
        string metaPath = path + ".meta";
        if (File.Exists(metaPath)) File.Delete(metaPath);
    }
}