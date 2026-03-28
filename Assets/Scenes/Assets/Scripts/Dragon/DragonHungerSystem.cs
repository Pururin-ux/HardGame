using UnityEngine;
using DungeonPrototype.Core;
using DungeonPrototype.Player;

namespace DungeonPrototype.Dragon
{
    public class DragonHungerSystem : MonoBehaviour
    {
        [SerializeField] private DragonCompanion dragon;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private float starvingAfterSeconds = 90f;
        [SerializeField] private float damagePerTick = 4f;
        [SerializeField] private float tickInterval = 2f;

        private float _lastFedTime;
        private float _damageTickTimer;

        //private void Awake()
        //{
        //    _lastFedTime = Time.time;
        //}

        private void OnEnable()
        {
            GameEvents.DragonHPIsZero += OnDragonManaChanged;
        }

        private void OnDisable()
        {
            GameEvents.DragonHPIsZero -= OnDragonManaChanged;
        }

        private void Update()
        {
            //if (dragon == null || playerHealth == null)
            //{
            //    return;
            //}

            //if (dragon.CurrentStage != DragonStage.Hatchling)
            //{
            //    return;
            //}

            //if (Time.time - _lastFedTime < starvingAfterSeconds)
            //{
            //    return;
            //}

            //_damageTickTimer += Time.deltaTime;
            //if (_damageTickTimer >= tickInterval)
            //{
            //    _damageTickTimer = 0f;
            //    playerHealth.TakeDamage(damagePerTick);
            //}
        }

        private void OnDragonManaChanged()
        {
            playerHealth.TakeDamage(damagePerTick);
            Debug.Log(playerHealth.CurrentHealth);
           

            //if (delta > 0f)
            //{
            //    _lastFedTime = Time.time;
            //    _damageTickTimer = 0f;
            //}
        }
    }
}
