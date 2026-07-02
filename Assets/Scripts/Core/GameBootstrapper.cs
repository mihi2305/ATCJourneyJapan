using UnityEngine;

namespace ATCJourneyJapan.Core
{
    // Ensures SampleScene becomes playable without manual scene wiring in the first prototype.
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null)
            {
                return;
            }

            var gameObject = new GameObject("GameManager");
            var manager = gameObject.AddComponent<GameManager>();
            manager.Initialize();
        }
    }
}
