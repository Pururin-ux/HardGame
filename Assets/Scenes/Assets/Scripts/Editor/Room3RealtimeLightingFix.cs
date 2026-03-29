using System.Collections.Generic;
using System.Linq;
using DungeonPrototype.Mana;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class Room3RealtimeLightingFix
{
    private const string RoomRootPath = "LevelRoot/Room3_FirstRisk";
    private const string VolumeAssetPath = "Assets/Scenes/Assets/Settings/Room3_RealtimePostFX.asset";

    [MenuItem("Tools/Dungeon Prototype/Fix Room3 Lighting (Realtime Contrast)")]
    [MenuItem("Tools/Dungeon Prototype/Room3 Lighting Fix Realtime")]
    public static void ApplyFix()
    {
        GameObject roomRoot = GameObject.Find(RoomRootPath);
        if (roomRoot == null)
        {
            Debug.LogWarning("Room3 root not found. Open the gameplay scene first.");
            return;
        }

        DisableBakedAndRealtimeGI();
        MakeBaseLightingDark();
        ConfigureCrystalGIAndLights(roomRoot.transform);
        ConfigureGlobalVolume();
        ConfigureMainCameraPostProcessing();

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("Room3 realtime lighting fix applied: GI disabled, localized crystal lights, and post-processing contrast restored.");
    }

    private static void DisableBakedAndRealtimeGI()
    {
        LightingSettings lightingSettings = Lightmapping.lightingSettings;
        if (lightingSettings == null)
        {
            lightingSettings = new LightingSettings();
            Lightmapping.lightingSettings = lightingSettings;
        }

        lightingSettings.bakedGI = false;
        lightingSettings.realtimeGI = false;

        Lightmapping.Clear();
        Lightmapping.ClearLightingDataAsset();
    }

    private static void MakeBaseLightingDark()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.reflectionIntensity = 0f;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;

        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < allLights.Length; i++)
        {
            Light light = allLights[i];
            if (light == null)
            {
                continue;
            }

            // Keep main gameplay room controlled by local crystal lights only.
            if (light.transform.IsChildOf(GameObject.Find(RoomRootPath).transform))
            {
                light.enabled = false;
            }
        }
    }

    private static void ConfigureCrystalGIAndLights(Transform roomRoot)
    {
        var majorCrystals = new List<GameObject>();

        ManaCrystal[] manaCrystals = roomRoot.GetComponentsInChildren<ManaCrystal>(true);
        for (int i = 0; i < manaCrystals.Length; i++)
        {
            majorCrystals.Add(manaCrystals[i].gameObject);
        }

        StunCrystal[] stunCrystals = roomRoot.GetComponentsInChildren<StunCrystal>(true);
        for (int i = 0; i < stunCrystals.Length; i++)
        {
            majorCrystals.Add(stunCrystals[i].gameObject);
        }

        majorCrystals = majorCrystals.Distinct().ToList();

        for (int i = 0; i < majorCrystals.Count; i++)
        {
            GameObject crystal = majorCrystals[i];
            bool isStun = crystal.GetComponent<StunCrystal>() != null;

            DisableCrystalGIFlags(crystal);
            RemoveOldManagedCrystalLights(crystal.transform);

            var point = new GameObject("RealtimeCrystalPoint");
            point.transform.SetParent(crystal.transform, false);
            point.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            Light l = point.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.intensity = 5f;
            l.range = 3f;
            l.color = isStun
                ? new Color(0.72f, 0.35f, 1f, 1f)
                : new Color(0.1f, 0.95f, 1f, 1f);
            l.enabled = true;
        }
    }

    private static void DisableCrystalGIFlags(GameObject crystalRoot)
    {
        MeshRenderer[] renderers = crystalRoot.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer mr = renderers[i];
            if (mr == null)
            {
                continue;
            }

            mr.receiveGI = ReceiveGI.LightProbes;

            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(mr.gameObject);
            flags &= ~StaticEditorFlags.ContributeGI;
            GameObjectUtility.SetStaticEditorFlags(mr.gameObject, flags);

            EditorUtility.SetDirty(mr);
        }
    }

    private static void RemoveOldManagedCrystalLights(Transform crystalRoot)
    {
        var toDelete = new List<GameObject>();
        for (int i = 0; i < crystalRoot.childCount; i++)
        {
            Transform child = crystalRoot.GetChild(i);
            if (child.name == "RealtimeCrystalPoint")
            {
                toDelete.Add(child.gameObject);
            }
        }

        for (int i = 0; i < toDelete.Count; i++)
        {
            Object.DestroyImmediate(toDelete[i]);
        }
    }

    private static void ConfigureGlobalVolume()
    {
        GameObject volumeGO = GameObject.Find("Room3_GlobalVolume");
        if (volumeGO == null)
        {
            volumeGO = new GameObject("Room3_GlobalVolume");
        }

        Volume volume = volumeGO.GetComponent<Volume>();
        if (volume == null)
        {
            volume = volumeGO.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 10f;
        volume.weight = 1f;

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeAssetPath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, VolumeAssetPath);
            AssetDatabase.SaveAssets();
        }

        volume.sharedProfile = profile;

        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>(true);
        }

        bloom.active = true;
        bloom.intensity.Override(2f);
        bloom.threshold.Override(1.1f);
        bloom.scatter.Override(0.7f);

        if (!profile.TryGet(out Vignette vignette))
        {
            vignette = profile.Add<Vignette>(true);
        }

        vignette.active = true;
        vignette.intensity.Override(0.4f);
        vignette.smoothness.Override(0.55f);
        vignette.rounded.Override(false);

        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>(true);
        }

        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(-0.7f);
        colorAdjustments.temperature.Override(-18f);
        colorAdjustments.saturation.Override(-8f);

        EditorUtility.SetDirty(profile);
        EditorUtility.SetDirty(volumeGO);
    }

    private static void ConfigureMainCameraPostProcessing()
    {
        Camera main = Camera.main;
        if (main == null)
        {
            main = Object.FindFirstObjectByType<Camera>();
        }

        if (main == null)
        {
            return;
        }

        UniversalAdditionalCameraData data = main.GetComponent<UniversalAdditionalCameraData>();
        if (data == null)
        {
            data = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }

        data.renderPostProcessing = true;
    }
}
