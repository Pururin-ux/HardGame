using UnityEngine;
using DungeonPrototype.Core;
using DungeonPrototype.Dragon;

namespace Game.DungeonPrototype.Followers
{
    internal class DragonAnimationController : MonoBehaviour
    {
        [SerializeField] private float movementThreshold = 0.1f;

        private Animator _currentAnimator;
        private Vector3 _lastPosition;
        private bool _isMoving = false;

        private void Awake()
        {
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            GameEvents.DragonStageChanged += OnDragonStageChanged;
        }

        private void OnDisable()
        {
            GameEvents.DragonStageChanged -= OnDragonStageChanged;
        }

        private void Start()
        {
            FindActiveAnimator();
        }

        private void Update()
        {
            if (_currentAnimator == null) return;

            // Проверяем движение
            Vector3 currentPosition = transform.position;
            float speed = (currentPosition - _lastPosition).magnitude / Time.deltaTime;
            _lastPosition = currentPosition;

            bool wasMoving = _isMoving;
            _isMoving = speed > movementThreshold;

            // Меняем анимацию при изменении состояния
            if (wasMoving != _isMoving)
            {
                _currentAnimator.Play(_isMoving ? "WalkDragon" : "IdleDragon");
            }
        }

        private void OnDragonStageChanged(DragonStage stage)
        {
            FindActiveAnimator();
        }

        private void FindActiveAnimator()
        {
            // Ищем первый активный дочерний объект
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf)
                {
                    _currentAnimator = child.GetComponent<Animator>();
                    if (_currentAnimator != null) break;
                }
            }
        }
    }
}