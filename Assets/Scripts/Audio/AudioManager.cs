using UnityEngine;

namespace SurvivorIO
{
    /// <summary>
    /// Central SFX/music hub. If no AudioClip is assigned for an event it falls
    /// back to a small procedurally-synthesised tone, so the game has audio even
    /// without imported assets. Drop real .wav/.mp3 clips into the serialized
    /// fields to override the built-in sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Optional clip overrides (empty = built-in synth)")]
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip deathClip;
        [SerializeField] private AudioClip levelUpClip;
        [SerializeField] private AudioClip hurtClip;
        [SerializeField] private AudioClip musicClip;

        [Header("Volumes")]
        [SerializeField] private float sfxVolume = 0.5f;
        [SerializeField] private float musicVolume = 0.35f;

        private AudioSource _sfx;
        private AudioSource _music;
        private AudioClip _hit, _death, _levelUp, _hurt;
        private float _lastHit, _lastDeath;

        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            PlayerPrefs.SetFloat("sfxVol", sfxVolume);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (_music != null) _music.volume = musicVolume;
            PlayerPrefs.SetFloat("musVol", musicVolume);
            PlayerPrefs.Save();
        }

        private void Awake()
        {
            Instance = this;

            sfxVolume   = PlayerPrefs.GetFloat("sfxVol", sfxVolume);
            musicVolume = PlayerPrefs.GetFloat("musVol", musicVolume);

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.volume = musicVolume;

            _hit     = hitClip     != null ? hitClip     : Synth.Blip(900f, 1500f, 0.09f, 0.15f);
            _death   = deathClip   != null ? deathClip   : Synth.Noise(0.16f);
            _levelUp = levelUpClip != null ? levelUpClip : Synth.Arp();
            _hurt    = hurtClip    != null ? hurtClip    : Synth.Blip(220f, 110f, 0.13f, 0.2f, square: true);

            if (musicClip != null)
            {
                _music.clip = musicClip;
                _music.Play();
            }
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var xp = player.GetComponent<PlayerExperience>();
                if (xp != null) xp.LeveledUp += _ => PlayLevelUp();
            }
        }

        public void PlayHit()
        {
            if (Time.unscaledTime - _lastHit < 0.04f) return;   // throttle multi-hit spam
            _lastHit = Time.unscaledTime;
            _sfx.PlayOneShot(_hit, sfxVolume);
        }

        public void PlayDeath()
        {
            if (Time.unscaledTime - _lastDeath < 0.04f) return;
            _lastDeath = Time.unscaledTime;
            _sfx.PlayOneShot(_death, sfxVolume * 0.8f);
        }

        public void PlayLevelUp() => _sfx.PlayOneShot(_levelUp, sfxVolume);
        public void PlayHurt()    => _sfx.PlayOneShot(_hurt, sfxVolume);
    }

    /// <summary>Tiny runtime audio synthesiser for placeholder SFX.</summary>
    internal static class Synth
    {
        private const int SR = 44100;

        public static AudioClip Blip(float f0, float f1, float dur, float noiseMix, bool square = false)
        {
            int n = Mathf.Max(1, (int)(SR * dur));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float freq = Mathf.Lerp(f0, f1, t);
                float phase = 2f * Mathf.PI * freq * (i / (float)SR);
                float wave = square ? Mathf.Sign(Mathf.Sin(phase)) : Mathf.Sin(phase);
                float noise = Random.value * 2f - 1f;
                float env = Mathf.Exp(-5f * t);
                data[i] = Mathf.Lerp(wave, noise, noiseMix) * env * 0.6f;
            }
            return Make("blip", data, n);
        }

        public static AudioClip Noise(float dur)
        {
            int n = Mathf.Max(1, (int)(SR * dur));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float env = Mathf.Exp(-6f * t);
                data[i] = (Random.value * 2f - 1f) * env * 0.5f;
            }
            return Make("noise", data, n);
        }

        public static AudioClip Arp()
        {
            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };  // C5 E5 G5 C6
            int per = (int)(SR * 0.09f);
            int n = per * notes.Length;
            var data = new float[n];
            for (int k = 0; k < notes.Length; k++)
                for (int i = 0; i < per; i++)
                {
                    float t = (float)i / per;
                    float env = Mathf.Exp(-3f * t);
                    data[k * per + i] = Mathf.Sin(2f * Mathf.PI * notes[k] * (i / (float)SR)) * env * 0.5f;
                }
            return Make("arp", data, n);
        }

        private static AudioClip Make(string name, float[] data, int n)
        {
            var c = AudioClip.Create(name, n, 1, SR, false);
            c.SetData(data, 0);
            return c;
        }
    }
}
