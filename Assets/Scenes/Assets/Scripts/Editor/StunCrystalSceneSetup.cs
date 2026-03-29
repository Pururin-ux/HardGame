using DungeonPrototype.Mana;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StunCrystalSceneSetup
{
    private const string AutoSetupKey = "DungeonPrototype.StunCrystalSetup.Done";

    [InitializeOnLoadMethod]
    private static void AutoSetupOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (EditorPrefs.GetBool(AutoSetupKey, false))
            {
                return;
            }

            if (SetupRoom3StunCrystalInternal())
            {
                EditorPrefs.SetBool(AutoSetupKey, true);
            }
        };
    }

    [MenuItem("Tools/Dungeon Prototype/Setup Room3 Stun Crystal")]
    public static void SetupRoom3StunCrystalMenu()
    {
        bool ok = SetupRoom3StunCrystalInternal();
        if (!ok)
        {
            Debug.LogWarning("Room3 setup not found. Open the gameplay scene with LevelRoot/Room3_FirstRisk and try again.");
        }
    }

    private static bool SetupRoom3StunCrystalInternal()
    {
        GameObject room = GameObject.Find("LevelRoot/Room3_FirstRisk");
        if (room == null)
        {
            return false;
        }

        GameObject guardian = GameObject.Find("LevelRoot/Room3_FirstRisk/Guardian_Sleeper");
        GameObject stunCrystal = GameObject.Find("LevelRoot/Room3_FirstRisk/Room3_StunCrystal");

        if (stunCrystal == null)
        {
            stunCrystal = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            stunCrystal.name = "Room3_StunCrystal";
            stunCrystal.transform.SetParent(room.transform, true);
        }

        Vector3 spawnPosition = guardian != null
            ? guardian.transform.position + new Vector3(-1.7f, 0f, -1.5f)
            : new Vector3(32.4f, 1f, 1.8f);

        stunCrystal.transform.position = spawnPosition;
        stunCrystal.transform.localScale = new Vector3(0.55f, 0.8f, 0.55f);

        Collider col = stunCrystal.GetComponent<Collider>();
        if (col == null)
        {
            col = stunCrystal.AddComponent<CapsuleCollider>();
        }

        col.isTrigger = false;

        Light light = stunCrystal.GetComponent<Light>();
        if (light == null)
        {
            light = stunCrystal.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = new Color(1f, 0.15f, 0.15f, 1f);
        light.intensity = 3.2f;
        light.range = 7.5f;

        StunCrystal stun = stunCrystal.GetComponent<StunCrystal>();
        if (stun == null)
        {
            stun = stunCrystal.AddComponent<StunCrystal>();
        }

        SerializedObject so = new SerializedObject(stun);
        SetFloat(so, "stunDuration", 3f);
        SetFloat(so, "cooldownDuration", 5f);
        SetFloat(so, "targetSearchRadius", 14f);
        so.ApplyModifiedPropertiesWithoutUndo();

        ManaCrystal accidentalMana = stunCrystal.GetComponent<ManaCrystal>();
        if (accidentalMana != null)
        {
            Object.DestroyImmediate(accidentalMana);
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Room3_StunCrystal is configured and saved.");
        return true;
    }

    private static void SetFloat(SerializedObject so, string propertyName, float value)
    {
        SerializedProperty p = so.FindProperty(propertyName);
        if (p != null)
        {
            p.floatValue = value;
        }
    }
}
