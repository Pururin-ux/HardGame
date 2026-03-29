using UnityEngine;
using UnityEngine.AI;
using DungeonPrototype.Player;

namespace Game.DungeonPrototype.Followers
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class SimpleNavMeshFollower : MonoBehaviour
    {
        [SerializeField] private SimpleFirstPersonController playerController;
        [SerializeField] private float followDistance = 2f;
        [SerializeField] private float stopDistance = 1.5f;
        [SerializeField] private float speed = 3.5f;

        private NavMeshAgent _agent;
        private Transform _playerTransform;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = speed;
            _agent.stoppingDistance = stopDistance;

            if (playerController == null)
                playerController = FindObjectOfType<SimpleFirstPersonController>();

            if (playerController != null)
                _playerTransform = playerController.transform;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (distance > stopDistance && distance < followDistance * 2f)
            {
                _agent.SetDestination(_playerTransform.position);
                _agent.isStopped = false;
            }
            else if (distance <= stopDistance)
            {
                _agent.isStopped = true;
            }
        }

        private void OnEnable()
        {
            if (_agent != null && !_agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
            }
        }
    }
}