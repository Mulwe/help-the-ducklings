using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindUsedShaders : EditorWindow
{
    [MenuItem("Tools/Find Used Shaders")]
    private static void FindShaders()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        HashSet<string> usedShaders = new HashSet<string>();

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null)
            {
                usedShaders.Add(mat.shader.name);
            }
        }

        Debug.Log("=== ÈÑÏÎËÜÇÓÅÌÛÅ ØÅÉÄÅĞÛ ===");
        foreach (string shader in usedShaders.OrderBy(s => s))
        {
            Debug.Log(" [+] " + shader);
        }
        Debug.Log($"Âñåãî óíèêàëüíûõ øåéäåğîâ: {usedShaders.Count}");
    }
}