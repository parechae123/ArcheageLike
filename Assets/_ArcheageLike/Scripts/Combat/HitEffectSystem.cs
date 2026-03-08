using UnityEngine;
using ArcheageLike.Core;
using ArcheageLike.UI;

namespace ArcheageLike.Combat
{
    /// <summary>
    /// Subscribes to damage events and spawns VFX + damage popups.
    /// All particles are created via code — no prefabs needed.
    /// </summary>
    public class HitEffectSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _showDamageNumbers = true;
        [SerializeField] private bool _showHitParticles = true;
        [SerializeField] private bool _flashOnHit = true;

        private void OnEnable()
        {
            EventBus.Subscribe<DamageEvent>(OnDamage);
            EventBus.Subscribe<EntityDeathEvent>(OnDeath);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DamageEvent>(OnDamage);
            EventBus.Unsubscribe<EntityDeathEvent>(OnDeath);
        }

        private void OnDamage(DamageEvent evt)
        {
            if (evt.Target == null) return;
            Vector3 pos = evt.Target.transform.position;

            // Damage number popup
            if (_showDamageNumbers)
            {
                bool isCrit = evt.Amount > 100f; // simplified crit detection
                RuntimeDamagePopup.Spawn(pos, evt.Amount, evt.Type, isCrit);
            }

            // Hit particles
            if (_showHitParticles)
            {
                SpawnHitParticles(pos + Vector3.up, evt.Type);
            }

            // Flash target red
            if (_flashOnHit)
            {
                var renderer = evt.Target.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    var flash = evt.Target.GetComponent<HitFlash>();
                    if (flash == null) flash = evt.Target.AddComponent<HitFlash>();
                    flash.Flash(renderer);
                }
            }
        }

        private void OnDeath(EntityDeathEvent evt)
        {
            if (evt.Entity == null) return;
            SpawnDeathParticles(evt.Entity.transform.position + Vector3.up);
        }

        /// <summary>
        /// Spawn hit spark particles at position.
        /// </summary>
        private void SpawnHitParticles(Vector3 position, DamageType type)
        {
            var go = new GameObject("HitVFX");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = 5f;
            main.startSize = 0.15f;
            main.maxParticles = 15;
            main.gravityModifier = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            if (type == DamageType.Physical)
                main.startColor = new Color(1f, 0.8f, 0.3f); // orange sparks
            else if (type == DamageType.Magical)
                main.startColor = new Color(0.5f, 0.5f, 1f); // blue sparks
            else
                main.startColor = Color.white;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 12)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            // Use default particle material
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = main.startColor.color;

            Destroy(go, 1f);
        }

        /// <summary>
        /// Larger burst for death.
        /// </summary>
        private void SpawnDeathParticles(Vector3 position)
        {
            var go = new GameObject("DeathVFX");
            go.transform.position = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = 4f;
            main.startSize = 0.3f;
            main.maxParticles = 30;
            main.gravityModifier = 0.5f;
            main.startColor = new Color(0.8f, 0.1f, 0.1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 25)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            Destroy(go, 2f);
        }
    }

    /// <summary>
    /// Flashes a MeshRenderer red briefly on hit.
    /// </summary>
    public class HitFlash : MonoBehaviour
    {
        private MeshRenderer _renderer;
        private Color _originalColor;
        private float _flashTimer;
        private static readonly float FlashDuration = 0.15f;

        public void Flash(MeshRenderer renderer)
        {
            _renderer = renderer;
            _originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            _flashTimer = FlashDuration;
            enabled = true;
        }

        private void Update()
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f && _renderer != null)
            {
                _renderer.material.color = _originalColor;
                enabled = false;
            }
        }
    }
}
