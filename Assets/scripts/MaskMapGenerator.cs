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

        // Ask the user if they want to destroy the original files
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
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

            validTextureCount++;
            string fileName = Path.GetFileNameWithoutExtension(file);
            
            int lastUnderscore = fileName.LastIndexOf('_');
            if (lastUnderscore == -1) continue; 
            
            string baseName = fileName.Substring(0, lastUnderscore);
            
            if (!groups.ContainsKey(baseName)) groups[baseName] = new List<string>();
            groups[baseName].Add(file);
        }

        if (validTextureCount == 0)
        {
            Debug.LogError("ERROR: Found 0 PNG or JPG files in the selected folder!");
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
        
        // Keep track of the specific paths so we know what to delete
        string metallicPath = "", aoPath = "", roughnessPath = "";

        foreach (string path in files)
        {
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            string fileNameLower = Path.GetFileNameWithoutExtension(path).ToLower();

            if (fileNameLower.EndsWith("_metallic")) { metallic = tex; metallicPath = path; }
            else if (fileNameLower.EndsWith("_ao")) { ao = tex; aoPath = path; }
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
        string finalFilePath = Path.Combine(outputPath, baseName + "_maskmap.png");
        File.WriteAllBytes(finalFilePath, pngData);

        // --- DELETION LOGIC ---
        if (deleteOriginals)
        {
            if (metallic != null) SafeDelete(metallicPath);
            if (ao != null) SafeDelete(aoPath);
            if (roughness != null) SafeDelete(roughnessPath);
        }
        
        return true;
    }

    // Helper function to delete the image AND its Unity .meta file
    static void SafeDelete(string path)
    {
        if (File.Exists(path)) 
        {
            File.Delete(path);
        }
        
        string metaPath = path + ".meta";
        if (File.Exists(metaPath)) 
        {
            File.Delete(metaPath);
        }
    }
}