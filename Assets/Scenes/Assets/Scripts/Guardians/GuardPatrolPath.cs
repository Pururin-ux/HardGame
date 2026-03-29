using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonPrototype.Guardians
{
    public class GuardPatrolPath : MonoBehaviour
    {
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float turnSpeed = 6f;
        [SerializeField] private float pointStopDistance = 0.25f;
        [SerializeField] private float pauseAtPoint = 0.6f;

        private int _currentIndex;
        private float _pauseUntil;

        private void Awake()
        {
            if (patrolPoints != null && patrolPoints.Count > 0)
            {
                return;
            }

            patrolPoints = GetComponentsInChildren<Transform>(true)
                .Where(t => t != transform && t.name.StartsWith("PatrolPoint_"))
                .OrderBy(t => t.name)
                .ToList();
        }

        private void Update()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
            {
                return;
            }

            if (Time.time < _pauseUntil)
            {
                return;
            }

            Transform target = patrolPoints[_currentIndex];
            if (target == null)
            {
                Advance();
                return;
            }

            Vector3 current = transform.position;
            Vector3 destination = new Vector3(target.position.x, current.y, target.position.z);
            Vector3 toTarget = destination - current;
            float distance = toTarget.magnitude;

            if (distance <= pointStopDistance)
            {
                Advance();
                _pauseUntil = Time.time + Mathf.Max(0f, pauseAtPoint);
                return;
            }

            Vector3 direction = toTarget / Mathf.Max(0.0001f, distance);
            transform.position = Vector3.MoveTowards(current, destination, moveSpeed * Time.deltaTime);

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        private void Advance()
        {
            _currentIndex = (_currentIndex + 1) % patrolPoints.Count;
        }
    }
}
