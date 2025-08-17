#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayFromBoot
{
    // Mets ici le chemin exact de ta scène Boot :
    const string BootPath = "Assets/Scenes/Boot.unity";

    [MenuItem("Tools/Path of Faith/Always Start From Boot")]
    public static void AlwaysStartFromBoot()
    {
        var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootPath);
        if (boot == null)
        {
            Debug.LogError("Boot.unity introuvable à " + BootPath + " — vérifie le chemin.");
            return;
        }
        EditorSceneManager.playModeStartScene = boot;
        Debug.Log("✅ PlayModeStartScene = Boot (le Play lancera toujours Boot).");
    }

    [MenuItem("Tools/Path of Faith/Start From Current Scene")]
    public static void StartFromCurrentScene()
    {
        EditorSceneManager.playModeStartScene = null;
        Debug.Log("↪️ PlayModeStartScene désactivé (Play démarre sur la scène ouverte).");
    }
}
#endif
