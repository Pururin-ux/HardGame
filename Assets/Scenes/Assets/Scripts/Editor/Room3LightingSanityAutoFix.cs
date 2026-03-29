using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class Room3LightingSanityAutoFix
{
    private const string OnceKey = "DungeonPrototype.Room3LightingSanityAutoFix.v1";
    private const string RoomRootPath = "LevelRoot/Room3_FirstRisk";

    static Room3LightingSanityAutoFix()
    {
        EditorApplication.delayCall += ApplyOnceWhenSceneReady;
    }

    private static void ApplyOnceWhenSceneReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (EditorPrefs.GetBool(OnceKey, false))
        {
            return;
        }

        GameObject roomRoot = GameObject.Find(RoomRootPath);
        if (roomRoot == null)
        {
            return;
        }

        var settings = Lightmapping.lightingSettings;
        if (settings == null)
        {
            settings = new LightingSettings();
            Lightmapping.lightingSettings = settings;
        }

        settings.bakedGI = false;
        settings.realtimeGI = false;

        Lightmapping.Clear();
        Lightmapping.ClearLightingDataAsset();

        MeshRenderer[] renderers = roomRoot
            .GetComponentsInChildren<MeshRenderer>(true)
            .Where(r => r != null && r.transform.name.ToLowerInvariant().Contains("crystal"))
            .ToArray();

        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer mr = renderers[i];
            mr.receiveGI = ReceiveGI.LightProbes;

            var flags = GameObjectUtility.GetStaticEditorFlags(mr.gameObject);
            flags &= ~StaticEditorFlags.ContributeGI;
            GameObjectUtility.SetStaticEditorFlags(mr.gameObject, flags);

            EditorUtility.SetDirty(mr);
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        EditorPrefs.SetBool(OnceKey, true);

        Debug.Log("Room3 lighting sanity auto-fix applied: GI disabled and crystal GI contribution removed.");
    }
}
