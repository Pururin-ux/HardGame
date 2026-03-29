using UnityEngine;

namespace DungeonPrototype.Mana
{
    /// <summary>
    /// A special crystal that stuns enemies when activated.
    /// Can be triggered by the player to provide tactical combat benefits.
    /// </summary>
    public class StunCrystal : MonoBehaviour
    {
        [Header("Stun Settings")]
        [SerializeField] private float stunRadius = 5f;
        [SerializeField] private float stunDuration = 2f;
        [SerializeField] private float cooldownDuration = 5f;
        [SerializeField] private LayerMask enemyMask;

        private float _cooldownTimer;
        private Collider _collider;

        public bool IsOnCooldown => _cooldownTimer > 0f;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<SphereCollider>();
                (_collider as SphereCollider).isTrigger = true;
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Attempts to activate the stun crystal.
        /// Returns true if activation was successful (not on cooldown).
        /// </summary>
        public bool TryActivate(Transform activator)
        {
            if (IsOnCooldown)
            {
                return false;
            }

            Activate();
            return true;
        }

        /// <summary>
        /// Activates the stun effect, damaging/stunning enemies in range.
        /// </summary>
        private void Activate()
        {
            // Find all enemies in range
            Collider[] hits = Physics.OverlapSphere(transform.position, stunRadius, enemyMask, QueryTriggerInteraction.Ignore);

            foreach (Collider hit in hits)
            {
                // Apply stun effect to enemy
                // This can be extended to call a Stun method on an enemy script if available
            }

            _cooldownTimer = cooldownDuration;

            // Optional: Add visual feedback here (particles, light flash, etc.)
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, stunRadius);
        }
    }
}
