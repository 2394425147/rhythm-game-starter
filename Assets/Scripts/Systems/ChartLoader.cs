using Newtonsoft.Json;
using RhythmGameStarter.Data;
using UnityEngine;

namespace RhythmGameStarter.Systems
{
    public sealed class ChartLoader : MonoBehaviour
    {
        [SerializeField]
        private TextAsset chartAsset;

        [SerializeField]
        private AudioClip clip;

        public static Chart chart;

        private void Awake() => chart = JsonConvert.DeserializeObject<Chart>(chartAsset.text);
        private void Start()
        {
            Music.SetAudioClip(clip);
            Music.Play();
        }
    }
}
