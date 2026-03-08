using UnityEngine;
using UnityEngine.AI;
using ArcheageLike.Character;
using ArcheageLike.Core;

namespace ArcheageLike.Combat
{
    /// <summary>
    /// Basic enemy AI with patrol, chase, and attack states.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(Targetable))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Idle, Patrol, Chase, Attack, Return, Dead }

        [Header("AI Settings")]
        [SerializeField] private AIState _currentState = AIState.Idle;
        [SerializeField] private float _detectionRange = 15f;
        [SerializeField] private float _attackRange = 2.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private float _attackDamage = 30f;
        [SerializeField] private float _leashRange = 30f;

        [Header("Patrol")]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private float _patrolWaitTime = 3f;

        private NavMeshAgent _agent;
        private CharacterStats _stats;
        private Transform _target;
        private Vector3 _spawnPosition;
        private int _patrolIndex;
        private float _attackTimer;
        private float _patrolWaitTimer;

        public AIState CurrentState => _currentState;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _stats = GetComponent<CharacterStats>();
            _spawnPosition = transform.position;

            _stats.OnDeath.AddListener(OnDeath);
        }

        private void Update()
        {
            if (_stats.IsDead) return;
            if (_agent == null || !_agent.isOnNavMesh) return;

            _attackTimer -= Time.deltaTime;

            switch (_currentState)
            {
                case AIState.Idle:
                    UpdateIdle();
                    break;
                case AIState.Patrol:
                    UpdatePatrol();
                    break;
                case AIState.Chase:
                    UpdateChase();
                    break;
                case AIState.Attack:
                    UpdateAttack();
                    break;
                case AIState.Return:
                    UpdateReturn();
                    break;
            }
        }

        private void UpdateIdle()
        {
            // Look for player
            if (TryDetectPlayer())
            {
                _currentState = AIState.Chase;
                return;
            }

            // Start patrol if has patrol points
            if (_patrolPoints != null && _patrolPoints.Length > 0)
            {
                _patrolWaitTimer -= Time.deltaTime;
                if (_patrolWaitTimer <= 0)
                {
                    _currentState = AIState.Patrol;
                }
            }
        }

        private void UpdatePatrol()
        {
            if (TryDetectPlayer())
            {
                _currentState = AIState.Chase;
                return;
            }

            if (_patrolPoints == null || _patrolPoints.Length == 0) return;

            if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
            {
                _patrolWaitTimer = _patrolWaitTime;
                _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
                _agent.SetDestination(_patrolPoints[_patrolIndex].position);
                _currentState = AIState.Idle;
            }
        }

        private void UpdateChase()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                _currentState = AIState.Return;
                return;
            }

            float distToTarget = Vector3.Distance(transform.position, _target.position);
            float distToSpawn = Vector3.Distance(transform.position, _spawnPosition);

            // Leash check
            if (distToSpawn > _leashRange)
            {
                _target = null;
                _currentState = AIState.Return;
                return;
            }

            if (distToTarget <= _attackRange)
            {
                _agent.ResetPath();
                _currentState = AIState.Attack;
                return;
            }

            _agent.SetDestination(_target.position);
        }

        private void UpdateAttack()
        {
            if (_target == null)
            {
                _currentState = AIState.Return;
                return;
            }

            float distToTarget = Vector3.Distance(transform.position, _target.position);

            if (distToTarget > _attackRange * 1.2f)
            {
                _currentState = AIState.Chase;
                return;
            }

            // Face target
            Vector3 dir = (_target.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);

            // Attack
            if (_attackTimer <= 0f)
            {
                PerformAttack();
                _attackTimer = _attackCooldown;
            }
        }

        private void UpdateReturn()
        {
            _agent.SetDestination(_spawnPosition);

            if (_agent.remainingDistance < 1f)
            {
                // Heal back to full when returning
                _stats.Heal(_stats.MaxHealth);
                _currentState = AIState.Idle;
                _patrolWaitTimer = _patrolWaitTime;
            }
        }

        private bool TryDetectPlayer()
        {
            var colliders = Physics.OverlapSphere(transform.position, _detectionRange);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    var playerStats = col.GetComponent<CharacterStats>();
                    if (playerStats != null && !playerStats.IsDead)
                    {
                        _target = col.transform;
                        return true;
                    }
                }
            }
            return false;
        }

        private void PerformAttack()
        {
            if (_target == null) return;

            var targetStats = _target.GetComponent<CharacterStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(_attackDamage, DamageType.Physical);

                EventBus.Publish(new DamageEvent
                {
                    Source = gameObject,
                    Target = _target.gameObject,
                    Amount = _attackDamage,
                    Type = DamageType.Physical
                });
            }
        }

        private void OnDeath()
        {
            _currentState = AIState.Dead;
            _agent.ResetPath();
            _agent.enabled = false;

            EventBus.Publish(new EntityDeathEvent
            {
                Entity = gameObject,
                Killer = _target?.gameObject
            });

            // Despawn after delay
            Destroy(gameObject, 5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_spawnPosition != Vector3.zero ? _spawnPosition : transform.position, _leashRange);
        }
    }
}
