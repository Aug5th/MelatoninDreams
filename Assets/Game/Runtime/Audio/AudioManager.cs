using System.Collections;
using UnityEngine;

namespace GameKit
{
    /// <summary>
    /// Crossfades music across two AudioSources and pools eight more for SFX.
    /// Volumes are read straight from SaveSystem.Settings, so Settings and Audio never drift apart.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music - leave empty to use the procedurally generated tracks")]
        public AudioClip MenuMusic;
        public AudioClip GameMusic;

        [Header("SFX - leave empty to use the procedurally generated sounds")]
        public AudioClip ClickSfx;
        public AudioClip BackSfx;
        public AudioClip StartSfx;
        public AudioClip ScoreSfx;
        public AudioClip HurtSfx;
        public AudioClip GameOverSfx;

        AudioSource _musicA;
        AudioSource _musicB;
        AudioSource _activeMusic;
        AudioSource[] _sfxPool;
        int _sfxCursor;
        Coroutine _fadeRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; }

        void Awake()
        {
            Instance = this;

            _musicA = NewSource("Music A", true);
            _musicB = NewSource("Music B", true);
            _activeMusic = _musicA;

            _sfxPool = new AudioSource[8];
            for (int i = 0; i < _sfxPool.Length; i++) _sfxPool[i] = NewSource("SFX " + i, false);

            GenerateMissingClips();
            ApplyVolumes();
        }

        AudioSource NewSource(string sourceName, bool loop)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }

        void GenerateMissingClips()
        {
            if (ClickSfx    == null) ClickSfx    = ProceduralAudio.Blip("sfx_click", 880f, 1320f, 0.07f, 0.30f, true);
            if (BackSfx     == null) BackSfx     = ProceduralAudio.Blip("sfx_back", 660f, 380f, 0.09f, 0.30f, true);
            if (StartSfx    == null) StartSfx    = ProceduralAudio.Blip("sfx_start", 520f, 1040f, 0.20f, 0.28f);
            if (ScoreSfx    == null) ScoreSfx    = ProceduralAudio.Blip("sfx_score", 740f, 1480f, 0.12f, 0.32f);
            if (HurtSfx     == null) HurtSfx     = ProceduralAudio.Noise("sfx_hurt", 0.22f, 0.28f, 11f);
            if (GameOverSfx == null) GameOverSfx = ProceduralAudio.Blip("sfx_gameover", 420f, 110f, 0.70f, 0.30f);

            if (MenuMusic == null) MenuMusic = ProceduralAudio.Music("bgm_menu", new[] { 0, -4, -5, -7 }, 84f, 0.20f);
            if (GameMusic == null) GameMusic = ProceduralAudio.Music("bgm_game", new[] { 0, 3, -2, 5 }, 124f, 0.18f);
        }

        // ---------- Music ----------

        public void PlayMusic(AudioClip clip, float fadeSeconds = 0.8f)
        {
            if (clip == null) return;
            if (_activeMusic.clip == clip && _activeMusic.isPlaying) return;

            var next = _activeMusic == _musicA ? _musicB : _musicA;
            next.clip = clip;
            next.volume = 0f;
            next.Play();

            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(Crossfade(_activeMusic, next, fadeSeconds));
            _activeMusic = next;
        }

        public void StopMusic(float fadeSeconds = 0.5f)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeOutAndStop(_activeMusic, fadeSeconds));
        }

        IEnumerator Crossfade(AudioSource from, AudioSource to, float seconds)
        {
            float target = EffectiveMusic;
            float startFrom = from.volume;
            float t = 0f;

            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / seconds);
                from.volume = Mathf.Lerp(startFrom, 0f, k);
                to.volume   = Mathf.Lerp(0f, EffectiveMusic, k);
                yield return null;
            }

            from.Stop();
            from.volume = 0f;
            to.volume = EffectiveMusic;
            _fadeRoutine = null;
        }

        IEnumerator FadeOutAndStop(AudioSource src, float seconds)
        {
            float start = src.volume;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, Mathf.Clamp01(t / seconds));
                yield return null;
            }
            src.Stop();
            _fadeRoutine = null;
        }

        // ---------- SFX ----------

        public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            var src = _sfxPool[_sfxCursor];
            _sfxCursor = (_sfxCursor + 1) % _sfxPool.Length;
            src.pitch = pitch;
            src.volume = EffectiveSfx * volumeScale;
            src.PlayOneShot(clip, 1f);
        }

        public void PlayClick() { PlaySfx(ClickSfx); }
        public void PlayBack()  { PlaySfx(BackSfx); }

        // ---------- Volume ----------

        public float MasterVolume
        {
            get { return SaveSystem.Settings.masterVolume; }
            set { SaveSystem.Settings.masterVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        public float MusicVolume
        {
            get { return SaveSystem.Settings.musicVolume; }
            set { SaveSystem.Settings.musicVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        public float SfxVolume
        {
            get { return SaveSystem.Settings.sfxVolume; }
            set { SaveSystem.Settings.sfxVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        }

        float EffectiveMusic
        {
            get { return SaveSystem.Settings.masterVolume * SaveSystem.Settings.musicVolume; }
        }

        float EffectiveSfx
        {
            get { return SaveSystem.Settings.masterVolume * SaveSystem.Settings.sfxVolume; }
        }

        public void ApplyVolumes()
        {
            if (_fadeRoutine == null && _activeMusic != null && _activeMusic.isPlaying)
                _activeMusic.volume = EffectiveMusic;
        }
    }
}
