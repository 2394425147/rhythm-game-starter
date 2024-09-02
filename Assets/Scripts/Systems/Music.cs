using RhythmGameStarter.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace RhythmGameStarter.Systems
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class Music : MonoBehaviour
    {
        public static Music Instance { get; private set; }

        public static AudioClip AudioClip      => Instance._audioPlayer.clip;
        public static float     StartTime      { get; private set; }
        public static float     TimeSinceStart { get; private set; }
        public static float     Duration       { get; private set; }
        public static bool      IsPlaying      => Instance._audioPlayer.isPlaying;

        private static NativeLinearRegression TimePrediction { get; set; }

        private AudioSource _audioPlayer;
        private float       _lastCapturedDspTime;

        private void Awake()
        {
            Instance     = this;
            _audioPlayer = GetComponent<AudioSource>();
        }

        public static void SetAudioClip(AudioClip clipToPlay)
        {
            clipToPlay.LoadAudioData();
            Instance._audioPlayer.clip = clipToPlay;

            TimePrediction ??= new NativeLinearRegression();
            TimePrediction.Clear();

            Duration = (float)clipToPlay.samples / clipToPlay.frequency;
        }

        private void Update()
        {
            // Keep updating the linear regression model even if music isn't playing.
            if (!Mathf.Approximately((float)AudioSettings.dspTime, _lastCapturedDspTime))
            {
                TimePrediction.Sample(new double2(Time.realtimeSinceStartupAsDouble, AudioSettings.dspTime));
                _lastCapturedDspTime = (float)AudioSettings.dspTime;
            }

            // Fall back to AudioSettings.dspTime if we don't have enough samples.
            var smoothDspTime = TimePrediction.SampleCount < 2
                ? AudioSettings.dspTime
                : TimePrediction.Predict(Time.realtimeSinceStartupAsDouble);

            if (IsPlaying)
            {
                TimeSinceStart = (float)smoothDspTime - StartTime;
            }
        }

        private void OnDestroy() => TimePrediction?.Dispose();

        public static void Pause()
        {
            Instance._audioPlayer.Pause();
        }

        public static void Play()
        {
            TimeSinceStart = (float)Instance._audioPlayer.timeSamples / AudioClip.frequency;
            StartTime      = (float)(AudioSettings.dspTime - TimeSinceStart);

            Instance._audioPlayer.Play();
            ClearSamples();
        }

        public static void Seek(float time)
        {
            Instance._audioPlayer.timeSamples = (int)(time * AudioClip.frequency);

            // Setting the time by timeSamples is inaccurate.
            // We're setting the value to the actual time (though it does get overridden in the next frame in Update),
            // so some functions that depend on accurate time work as expected. (like editor features)
            TimeSinceStart = time;
            var sampleAccurateStartTime = (float)Instance._audioPlayer.timeSamples / AudioClip.frequency;
            StartTime = (float)(AudioSettings.dspTime - sampleAccurateStartTime);

            ClearSamples();
        }

        private static void ClearSamples()
        {
            TimePrediction.Clear();
            Instance._lastCapturedDspTime = 0;
        }
    }
}
