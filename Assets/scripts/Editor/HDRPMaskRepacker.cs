using UnityEngine;
using UnityEditor;
using System.IO;

public class HDRPMaskRepacker : EditorWindow
{
    private Texture2D sourceTexture;
    
    public enum SourceChannel { R, G, B, A, Black, White }
    
    // Defaulting to the standard GLTF/Blender ORM layout mapping to HDRP
    private SourceChannel targetR_Metallic = SourceChannel.B; // Blender Metallic is Blue
    private SourceChannel targetG_AO = SourceChannel.R;       // Blender AO is Red
    private SourceChannel targetB_Detail = SourceChannel.Black; 
    private SourceChannel targetA_Smoothness = SourceChannel.G; // Blender Roughness is Green
    
    private bool invertSmoothness = true; // GLTF uses Roughness, HDRP needs Smoothness

    [MenuItem("Tools/HDRP Mask Repacker")]
    public static void ShowWindow()
    {
        GetWindow<HDRPMaskRepacker>("HDRP Mask Repacker");
    }

    void OnGUI()
    {
        GUILayout.Label("Texture Channel Swapper", EditorStyles.boldLabel);
        
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);

        if (sourceTexture != null)
        {
            if (!sourceTexture.isReadable)
            {
                EditorGUILayout.HelpBox("Texture must be marked as 'Read/Write' in its import settings!", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("Map To Unity HDRP Channels:", EditorStyles.label);
            
            targetR_Metallic = (SourceChannel)EditorGUILayout.EnumPopup("Red (Metallic) Source:", targetR_Metallic);
            targetG_AO = (SourceChannel)EditorGUILayout.EnumPopup("Green (AO) Source:", targetG_AO);
            targetB_Detail = (SourceChannel)EditorGUILayout.EnumPopup("Blue (Detail) Source:", targetB_Detail);
            
            EditorGUILayout.BeginHorizontal();
            targetA_Smoothness = (SourceChannel)EditorGUILayout.EnumPopup("Alpha (Smoothness) Source:", targetA_Smoothness);
            invertSmoothness = EditorGUILayout.ToggleLeft("Invert (Roughness to Smoothness)", invertSmoothness);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            if (GUILayout.Button("Repack and Save Texture", GUILayout.Height(40)))
            {
                Repack();
            }
        }
    }

    private void Repack()
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] sourcePixels = sourceTexture.GetPixels();
        Color[] newPixels = new Color[sourcePixels.Length];

        for (int i = 0; i < sourcePixels.Length; i++)
        {
            Color c = sourcePixels[i];
            
            float r = GetChannelValue(c, targetR_Metallic, false);
            float g = GetChannelValue(c, targetG_AO, false);
            float b = GetChannelValue(c, targetB_Detail, false);
            float a = GetChannelValue(c, targetA_Smoothness, invertSmoothness);

            newPixels[i] = new Color(r, g, b, a);
        }

        newTex.SetPixels(newPixels);
        newTex.Apply();

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        string directory = Path.GetDirectoryName(path);
        string newPath = directory + "/" + sourceTexture.name + "_HDRP_Mask.png";

        File.WriteAllBytes(newPath, newTex.EncodeToPNG());
        AssetDatabase.Refresh();

        Debug.Log("Successfully created HDRP Mask Map at: " + newPath);
    }

    private float GetChannelValue(Color c, SourceChannel channel, bool invert)
    {
        float val = 0f;
        switch (channel)
        {
            case SourceChannel.R: val = c.r; break;
            case SourceChannel.G: val = c.g; break;
            case SourceChannel.B: val = c.b; break;
            case SourceChannel.A: val = c.a; break;
            case SourceChannel.Black: val = 0f; break;
            case SourceChannel.White: val = 1f; break;
        }
        return invert ? 1f - val : val;
    }
}