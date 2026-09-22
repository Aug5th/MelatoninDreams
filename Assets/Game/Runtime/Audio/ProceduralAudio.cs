using UnityEngine;

namespace GameKit
{
    /// <summary>
    /// Generates SFX and background music in code, so the kit makes sound before any
    /// audio asset exists. Drop real .wav/.ogg files onto AudioManager in the Inspector
    /// and they replace these clips.
    /// </summary>
    public static class ProceduralAudio
    {
        const int Rate = 44100;

        /// <summary>Short blip that sweeps in frequency from fromHz to toHz.</summary>
        public static AudioClip Blip(string name, float fromHz, float toHz, float duration,
                                     float volume = 0.3f, bool square = false)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * duration));
            var data = new float[n];
            float phase = 0f;

            for (int i = 0; i < n; i++)
            {
                float t  = i / (float)n;
                float hz = Mathf.Lerp(fromHz, toHz, t);
                phase += hz / Rate;

                float s = square
                    ? (Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f)
                    : Mathf.Sin(phase * Mathf.PI * 2f);

                float env = Mathf.Min(1f, t / 0.02f) * Mathf.Exp(-4f * t);
                data[i] = s * env * volume;
            }

            return Make(name, data);
        }

        /// <summary>Decaying noise burst - for impacts, taking damage, explosions.</summary>
        public static AudioClip Noise(string name, float duration, float volume = 0.3f, float decay = 8f)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * duration));
            var data = new float[n];
            var rnd = new System.Random(20260921);

            for (int i = 0; i < n; i++)
            {
                float t = i / (float)n;
                float env = Mathf.Exp(-decay * t);
                data[i] = (float)(rnd.NextDouble() * 2.0 - 1.0) * env * volume;
            }

            return Make(name, data);
        }

        /// <summary>
        /// Looping background music: an eight-note arpeggio per chord plus a bass note
        /// on each chord change. rootSemitones holds each chord's root, in semitones from A3 (220Hz).
        /// </summary>
        public static AudioClip Music(string name, int[] rootSemitones, float bpm = 96f, float volume = 0.2f)
        {
            if (rootSemitones == null || rootSemitones.Length == 0) rootSemitones = new[] { 0 };

            float beat = 60f / bpm;
            float step = beat * 0.5f;              // eighth notes
            const int stepsPerChord = 8;
            int totalSteps = rootSemitones.Length * stepsPerChord;

            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * step * totalSteps));
            var data = new float[n];

            int[] shape = { 0, 3, 7, 12, 7, 3, 12, 7 };

            for (int s = 0; s < totalSteps; s++)
            {
                int chord = s / stepsPerChord;
                int semi  = rootSemitones[chord] + shape[s % shape.Length];
                float hz  = 220f * Mathf.Pow(2f, semi / 12f);

                int start = Mathf.RoundToInt(s * step * Rate);
                int len   = Mathf.RoundToInt(step * Rate * 1.6f);

                for (int i = 0; i < len; i++)
                {
                    int idx = start + i;
                    if (idx >= n) break;

                    float t   = i / (float)len;
                    float env = Mathf.Min(1f, t / 0.02f) * Mathf.Exp(-3.2f * t);
                    float ph  = (i / (float)Rate) * hz;

                    float tone = Mathf.Sin(ph * Mathf.PI * 2f)
                               + Mathf.Sin(ph * Mathf.PI * 2f * 2.005f) * 0.25f;

                    data[idx] += tone * env * volume;
                }

                // Bass note at the start of each chord.
                if (s % stepsPerChord != 0) continue;

                float bassHz  = 220f * Mathf.Pow(2f, (rootSemitones[chord] - 24) / 12f);
                int   bassLen = Mathf.RoundToInt(step * stepsPerChord * Rate);

                for (int i = 0; i < bassLen; i++)
                {
                    int idx = start + i;
                    if (idx >= n) break;

                    float t   = i / (float)bassLen;
                    float env = Mathf.Min(1f, t / 0.01f) * Mathf.Exp(-1.6f * t);
                    float ph  = (i / (float)Rate) * bassHz;

                    data[idx] += Mathf.Sin(ph * Mathf.PI * 2f) * env * volume * 1.1f;
                }
            }

            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(data[i], -0.95f, 0.95f);

            return Make(name, data);
        }

        static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
