using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    // Tracks one runway and reports safety conflicts. Multiple instances can be registered by AirportManager.
    public class RunwayController : MonoBehaviour
    {
        [SerializeField] private string runwayId = "A";
        [SerializeField] private Transform threshold;
        [SerializeField] private Transform departureEnd;

        private readonly List<AircraftController> aircraftOnRunway = new List<AircraftController>();

        public string RunwayId => runwayId;
        public IReadOnlyList<AircraftController> AircraftOnRunway => aircraftOnRunway;
        public bool HasConflict => aircraftOnRunway.Count >= 2;

        public void Configure(string id, Transform start, Transform end)
        {
            runwayId = id;
            threshold = start;
            departureEnd = end;
        }

        public void Refresh(IEnumerable<AircraftController> aircraft)
        {
            aircraftOnRunway.Clear();
            foreach (var target in aircraft)
            {
                if (target != null && target.IsOnRunway)
                {
                    aircraftOnRunway.Add(target);
                }
            }
        }
    }
}
