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
        private bool runwayOccupied;
        private string occupiedByFlightId = string.Empty;
        private string occupiedReason = string.Empty;

        public string RunwayId => runwayId;
        public IReadOnlyList<AircraftController> AircraftOnRunway => aircraftOnRunway;
        public bool HasConflict => aircraftOnRunway.Count >= 2;
        public bool RunwayOccupied => runwayOccupied;
        public string OccupiedByFlightId => occupiedByFlightId;
        public string OccupiedReason => occupiedReason;
        public RunwayGeometry Geometry { get; private set; }

        public void Configure(string id, Transform start, Transform end)
        {
            Configure(id, start, end, null);
        }

        public void Configure(string id, Transform start, Transform end, RunwayGeometry geometry)
        {
            runwayId = id;
            threshold = start;
            departureEnd = end;
            Geometry = geometry;
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

        public void Occupy(AircraftController aircraft, string reason)
        {
            if (aircraft == null)
            {
                return;
            }

            runwayOccupied = true;
            occupiedByFlightId = aircraft.FlightNumber;
            occupiedReason = reason;
        }

        public void Release(AircraftController aircraft)
        {
            if (aircraft == null || !IsOccupiedBy(aircraft))
            {
                return;
            }

            runwayOccupied = false;
            occupiedByFlightId = string.Empty;
            occupiedReason = string.Empty;
        }

        public bool IsOccupiedBy(AircraftController aircraft)
        {
            return aircraft != null && runwayOccupied && occupiedByFlightId == aircraft.FlightNumber;
        }

        public bool IsOccupiedByOther(AircraftController aircraft)
        {
            return runwayOccupied && !IsOccupiedBy(aircraft);
        }
    }
}
