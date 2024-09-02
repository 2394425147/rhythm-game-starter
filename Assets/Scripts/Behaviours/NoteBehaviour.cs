using RhythmGameStarter.Data;
using RhythmGameStarter.Systems;
using UnityEngine;

namespace RhythmGameStarter.Behaviours
{
    public sealed class NoteBehaviour : MonoBehaviour
    {
        public const float Speed = 10;

        private Note _data;

        public void Initialize(Note data)
        {
            _data = data;
            Update();
        }

        private void Update()
        {
            var xPosition = _data.lane * 10 - 5;
            var yPosition = _data.time      - Music.TimeSinceStart;

            transform.localPosition = new Vector3(xPosition, yPosition * Speed);

            if (Music.TimeSinceStart > _data.time)
                NoteSpawner.notePool.Release(this);
        }
    }
}
