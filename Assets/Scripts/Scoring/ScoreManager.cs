using UnityEngine;

namespace ATCJourneyJapan.Scoring
{
    // Minimal scoring model for the prototype: safety starts at 100, delay increases over time, handled count advances completion.
    public class ScoreManager : MonoBehaviour
    {
        public int Safety { get; private set; }
        public float Delay { get; private set; }
        public int HandledAircraftCount { get; private set; }

        public void Initialize()
        {
            Safety = 100;
            Delay = 0f;
            HandledAircraftCount = 0;
        }

        public void Tick(float deltaTime, bool paused)
        {
            if (!paused)
            {
                Delay += deltaTime;
            }
        }

        public void ApplySafetyPenalty(int amount)
        {
            Safety = Mathf.Max(0, Safety - amount);
        }

        public void SetHandledAircraftCount(int count)
        {
            HandledAircraftCount = count;
        }
    }
}
