using DungeonPrototype.Player;
using UnityEngine;

namespace DungeonPrototype.Environment
{
    [RequireComponent(typeof(Collider))]
    public class LevelExitZone : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour flow;
        [SerializeField] private bool requireObjectiveReady = true;

        private void Awake()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }

            if (flow == null)
            {
                flow = FindAnyObjectByType<MonoBehaviour>(FindObjectsInactive.Include);
                if (flow == null || flow.GetType().FullName != "DungeonPrototype.Gameplay.GameplayFlowController")
                {
                    MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    for (int i = 0; i < allBehaviours.Length; i++)
                    {
                        if (allBehaviours[i] != null && allBehaviours[i].GetType().FullName == "DungeonPrototype.Gameplay.GameplayFlowController")
                        {
                            flow = allBehaviours[i];
                            break;
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (flow == null || IsFlowFinished())
            {
                return;
            }

            if (other.GetComponentInParent<PlayerHealth>() == null)
            {
                return;
            }

            if (requireObjectiveReady && !IsExitAllowed())
            {
                return;
            }

            CompleteLevel();
        }

        private bool IsFlowFinished()
        {
            return GetFlowBoolProperty("IsFinished");
        }

        private bool IsExitAllowed()
        {
            return GetFlowBoolProperty("IsExitAllowed");
        }

        private bool GetFlowBoolProperty(string propertyName)
        {
            if (flow == null)
            {
                return false;
            }

            var property = flow.GetType().GetProperty(propertyName);
            if (property == null || property.PropertyType != typeof(bool))
            {
                return false;
            }

            object value = property.GetValue(flow, null);
            return value is bool b && b;
        }

        private void CompleteLevel()
        {
            if (flow == null)
            {
                return;
            }

            var method = flow.GetType().GetMethod("CompleteLevel", System.Type.EmptyTypes);
            if (method == null)
            {
                return;
            }

            method.Invoke(flow, null);
        }
    }
}
