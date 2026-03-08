using UnityEngine;
using System.Collections.Generic;

namespace ArcheageLike.Core
{
    /// <summary>
    /// Simple sound manager using procedural AudioClips for prototype.
    /// Generates basic SFX at runtime — no audio files needed.
    /// </summary>
    public class SoundManager : Singleton<SoundManager>
    {
        [Header("Volume")]
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _sfxVolume = 0.7f;
        [SerializeField] private float _musicVolume = 0.3f;

        private AudioSource _musicSource;
        private List<AudioSource> _sfxSources = new List<AudioSource>();
        private Dictionary<string, AudioClip> _cachedClips = new Dictionary<string, AudioClip>();
        private const int MaxSFXSources = 8;

        protected override void Awake()
        {
            base.Awake();

            // Music source
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = _musicVolume;

            // SFX pool
            for (int i = 0; i < MaxSFXSources; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                _sfxSources.Add(src);
            }

            // Pre-generate common clips
            GenerateClips();

            // Subscribe to events
            EventBus.Subscribe<DamageEvent>(OnDamage);
            EventBus.Subscribe<EntityDeathEvent>(OnDeath);
            EventBus.Subscribe<SkillUsedEvent>(OnSkillUsed);
            EventBus.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
        }

        private void GenerateClips()
        {
            _cachedClips["hit"] = GenerateTone(0.1f, 200f, 100f);       // short low thud
            _cachedClips["crit"] = GenerateTone(0.15f, 400f, 150f);     // higher impact
            _cachedClips["death"] = GenerateTone(0.3f, 150f, 50f);      // low rumble
            _cachedClips["skill"] = GenerateTone(0.2f, 600f, 400f);     // whoosh
            _cachedClips["heal"] = GenerateTone(0.2f, 800f, 1000f);     // high chime
            _cachedClips["build"] = GenerateTone(0.15f, 300f, 200f);    // thunk
            _cachedClips["pickup"] = GenerateTone(0.1f, 1000f, 1200f);  // ding
            _cachedClips["error"] = GenerateTone(0.15f, 200f, 180f);    // buzz
            _cachedClips["splash"] = GenerateNoise(0.3f, 0.3f);         // water
            _cachedClips["sail"] = GenerateNoise(0.5f, 0.1f);           // wind
        }

        /// <summary>
        /// Generate a simple sine wave tone with frequency sweep.
        /// </summary>
        private AudioClip GenerateTone(float duration, float startFreq, float endFreq)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = 1f - t; // linear fade
                envelope *= envelope; // exponential fade
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t * duration) * envelope * 0.5f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Generate filtered noise (for wind, water, etc.)
        /// </summary>
        private AudioClip GenerateNoise(float duration, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
            float[] data = new float[samples];

            float prev = 0;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float raw = Random.Range(-1f, 1f) * volume;
                // Simple low-pass filter
                data[i] = prev * 0.9f + raw * 0.1f;
                data[i] *= (1f - t); // fade out
                prev = data[i];
            }

            clip.SetData(data, 0);
            return clip;
        }

        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            if (!_cachedClips.ContainsKey(clipName)) return;

            var source = GetAvailableSFXSource();
            if (source != null)
            {
                source.clip = _cachedClips[clipName];
                source.volume = _sfxVolume * _masterVolume * volumeScale;
                source.pitch = Random.Range(0.9f, 1.1f); // slight variation
                source.Play();
            }
        }

        public void PlaySFXAtPosition(string clipName, Vector3 position, float volumeScale = 1f)
        {
            if (!_cachedClips.ContainsKey(clipName)) return;

            AudioSource.PlayClipAtPoint(_cachedClips[clipName], position,
                _sfxVolume * _masterVolume * volumeScale);
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var src in _sfxSources)
            {
                if (!src.isPlaying) return src;
            }
            return _sfxSources[0]; // override oldest
        }

        // ===== Event Handlers =====
        private void OnDamage(DamageEvent evt)
        {
            if (evt.Target != null)
            {
                string clip = evt.Amount > 100f ? "crit" : "hit";
                PlaySFXAtPosition(clip, evt.Target.transform.position);
            }
        }

        private void OnDeath(EntityDeathEvent evt)
        {
            if (evt.Entity != null)
                PlaySFXAtPosition("death", evt.Entity.transform.position);
        }

        private void OnSkillUsed(SkillUsedEvent evt)
        {
            if (evt.Caster != null)
                PlaySFXAtPosition("skill", evt.Caster.transform.position, 0.6f);
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            PlaySFXAtPosition("build", evt.Position);
        }
    }
}
