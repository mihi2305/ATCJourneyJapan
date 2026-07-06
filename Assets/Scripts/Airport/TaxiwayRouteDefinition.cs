using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    public enum TaxiwayRoutePurpose
    {
        Departure,
        Arrival,
        Both
    }

    public class TaxiwayRouteDefinition
    {
        private readonly List<Vector3> waypoints;

        public TaxiwayRouteDefinition(
            string taxiwayId,
            string routeName,
            IEnumerable<Vector3> routeWaypoints,
            string connectsFrom,
            string connectsTo,
            TaxiwayRoutePurpose purpose)
        {
            TaxiwayId = taxiwayId;
            RouteName = routeName;
            waypoints = new List<Vector3>(routeWaypoints);
            ConnectsFrom = connectsFrom;
            ConnectsTo = connectsTo;
            Purpose = purpose;
        }

        public string TaxiwayId { get; private set; }
        public string RouteName { get; private set; }
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public string ConnectsFrom { get; private set; }
        public string ConnectsTo { get; private set; }
        public TaxiwayRoutePurpose Purpose { get; private set; }
    }

    public class TaxiRouteCandidate
    {
        private readonly List<Vector3> waypoints;

        public TaxiRouteCandidate(
            string routeId,
            string displayName,
            string description,
            string spotId,
            string runwayDesignator,
            IEnumerable<Vector3> routeWaypoints,
            bool isDefault)
        {
            RouteId = routeId;
            DisplayName = displayName;
            Description = description;
            SpotId = spotId;
            RunwayDesignator = runwayDesignator;
            waypoints = new List<Vector3>(routeWaypoints);
            IsDefault = isDefault;
        }

        public string RouteId { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string SpotId { get; private set; }
        public string RunwayDesignator { get; private set; }
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public bool IsDefault { get; private set; }
    }
}
