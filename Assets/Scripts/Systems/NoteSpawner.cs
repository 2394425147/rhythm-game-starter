using RhythmGameStarter.Behaviours;
using UnityEngine;
using UnityEngine.Pool;

namespace RhythmGameStarter.Systems
{
    public sealed class NoteSpawner : MonoBehaviour
    {
        public static ObjectPool<NoteBehaviour> notePool;

        [SerializeField]
        private NoteBehaviour notePrefab;

        private int _lastSpawnedIndex = -1;

        private void Start()
        {
            notePool = new ObjectPool<NoteBehaviour>(() => Instantiate(notePrefab),
                                                     n => n.gameObject.SetActive(true),
                                                     n => n.gameObject.SetActive(false));
        }

        private void Update()
        {
            for (var i = _lastSpawnedIndex + 1; i < ChartLoader.chart.notes.Count; i++)
            {
                var noteData = ChartLoader.chart.notes[i];
                var distance = (noteData.time - Music.TimeSinceStart) * NoteBehaviour.Speed;

                if (distance >= Camera.main?.orthographicSize)
                    return;

                _lastSpawnedIndex = i;
                var note = notePool.Get();
                note.Initialize(noteData);
            }
        }
    }
}
