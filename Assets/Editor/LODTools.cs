using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using UMesh = UnityEngine.Mesh;

// Utility for generating reduced-poly LOD meshes and wiring up LODGroups
// on the Environment scene's building groups. Invoked in batches via the
// Unity MCP RunCommand bridge.
public static class LODTools
{
    public const string OutputFolder = "Assets/Generated/LODMeshes";
    static readonly string[] GroupNames = { "foreGround", "middleGround", "backGround", "Props" };

    // LOD1 ~50% tris, LOD2 ~15% tris.
    const float Lod1Quality = 0.5f;
    const float Lod2Quality = 0.15f;

    static List<UMesh> CollectUniqueMeshes()
    {
        var seen = new Dictionary<UMesh, bool>();
        var ordered = new List<UMesh>();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var name in GroupNames)
        {
            GameObject root = null;
            foreach (var g in scene.GetRootGameObjects())
                if (g.name == name) { root = g; break; }
            if (root == null) continue;
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var m = mf.sharedMesh;
                if (m == null || seen.ContainsKey(m)) continue;
                seen[m] = true;
                ordered.Add(m);
            }
        }
        return ordered;
    }

    static string KeyFor(UMesh m)
    {
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localId))
            return guid + "_" + (localId < 0 ? "n" + (-localId) : localId.ToString());
        return "iid" + m.GetInstanceID();
    }

    static string PathFor(UMesh m, int lod) => $"{OutputFolder}/{KeyFor(m)}_LOD{lod}.asset";

    public static int UniqueMeshCount() => CollectUniqueMeshes().Count;

    // Simplifies meshes [start, start+count). Skips meshes whose assets already exist.
    public static void SimplifyRange(int start, int count)
    {
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);

        var meshes = CollectUniqueMeshes();
        int end = Mathf.Min(start + count, meshes.Count);
        int made = 0, skipped = 0, failed = 0;

        for (int i = start; i < end; i++)
        {
            var src = meshes[i];
            try
            {
                MakeLod(src, 1, Lod1Quality, ref made, ref skipped);
                MakeLod(src, 2, Lod2Quality, ref made, ref skipped);
            }
            catch (System.Exception e)
            {
                failed++;
                Debug.LogWarning($"LODTools: simplify failed for '{src.name}': {e.Message}");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"LODTools.SimplifyRange [{start},{end}) of {meshes.Count}: created={made} skipped={skipped} failed={failed}");
    }

    static void MakeLod(UMesh src, int lod, float quality, ref int made, ref int skipped)
    {
        string path = PathFor(src, lod);
        if (File.Exists(path)) { skipped++; return; }

        var simplifier = new MeshSimplifier();
        simplifier.Initialize(src);
        simplifier.SimplifyMesh(quality);
        var dst = simplifier.ToMesh();
        dst.name = src.name + "_LOD" + lod;
        dst.RecalculateBounds();
        AssetDatabase.CreateAsset(dst, path);
        made++;
    }

    static UMesh LoadLod(UMesh src, int lod)
    {
        return AssetDatabase.LoadAssetAtPath<UMesh>(PathFor(src, lod));
    }

    // Adds one LODGroup per building instance in the groups.
    public static void BuildLODGroups()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        int buildings = 0, skipped = 0;

        foreach (var name in GroupNames)
        {
            GameObject root = null;
            foreach (var g in scene.GetRootGameObjects())
                if (g.name == name) { root = g; break; }
            if (root == null) continue;

            var stack = new Stack<Transform>();
            foreach (Transform c in root.transform) stack.Push(c);
            while (stack.Count > 0)
            {
                var t = stack.Pop();
                if (PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))
                {
                    if (AddLODGroup(t.gameObject)) buildings++;
                    else skipped++;
                }
                else
                {
                    foreach (Transform c in t) stack.Push(c);
                }
            }
        }
        Debug.Log($"LODTools.BuildLODGroups: configured={buildings} skipped={skipped}");
    }

    static bool AddLODGroup(GameObject building)
    {
        if (building.GetComponent<LODGroup>() != null) return false;

        var filters = building.GetComponentsInChildren<MeshFilter>(true);
        var lod0 = new List<Renderer>();
        var lod1 = new List<Renderer>();
        var lod2 = new List<Renderer>();

        foreach (var mf in filters)
        {
            var src = mf.sharedMesh;
            var mr = mf.GetComponent<MeshRenderer>();
            if (src == null || mr == null) continue;
            lod0.Add(mr);

            lod1.Add(MakeLodRenderer(mf, mr, src, 1));
            lod2.Add(MakeLodRenderer(mf, mr, src, 2));
        }
        if (lod0.Count == 0) return false;

        var group = building.AddComponent<LODGroup>();
        var lods = new[]
        {
            new LOD(0.45f, lod0.ToArray()),
            new LOD(0.15f, lod1.ToArray()),
            new LOD(0.012f, lod2.ToArray()),
        };
        group.SetLODs(lods);
        group.RecalculateBounds();
        return true;
    }

    static Renderer MakeLodRenderer(MeshFilter srcFilter, MeshRenderer srcRenderer, UMesh srcMesh, int lod)
    {
        var lodMesh = LoadLod(srcMesh, lod) ?? srcMesh; // fall back to source if missing
        var go = new GameObject($"__LOD{lod}");
        go.transform.SetParent(srcFilter.transform, false);
        go.isStatic = srcFilter.gameObject.isStatic;
        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = lodMesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterials = srcRenderer.sharedMaterials;
        mr.shadowCastingMode = srcRenderer.shadowCastingMode;
        mr.receiveShadows = srcRenderer.receiveShadows;
        return mr;
    }
}
