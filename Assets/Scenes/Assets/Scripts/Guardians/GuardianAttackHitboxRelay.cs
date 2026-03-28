using DungeonPrototype.Player;
using UnityEngine;

namespace DungeonPrototype.Guardians
{
    [RequireComponent(typeof(SphereCollider))]
    public class GuardianAttackHitboxRelay : MonoBehaviour
    {
        [SerializeField] private GuardianController guardian;

        private void Reset()
        {
            SphereCollider sphere = GetComponent<SphereCollider>();
            sphere.isTrigger = true;
        }

        private void Awake()
        {
            if (guardian == null)
            {
                guardian = GetComponentInParent<GuardianController>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (guardian == null)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerHealth>() == null)
            {
                return;
            }

            guardian.NotifyPlayerInAttackRange(other.transform);
        }

        private void OnTriggerStay(Collider other)
        {
            if (guardian == null)
            {
                return;
            }

            if (other.GetComponentInParent<PlayerHealth>() == null)
            {
                return;
            }

            guardian.NotifyPlayerInAttackRange(other.transform);
        }
    }
}
