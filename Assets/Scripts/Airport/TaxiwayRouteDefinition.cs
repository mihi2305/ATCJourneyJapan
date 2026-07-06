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

    public enum TaxiwaySegmentType
    {
        Parallel,
        Connector,
        Exit,
        Apron,
        StandEntry
    }

    public enum TaxiwayAxisType
    {
        NorthSouth,
        EastWest,
        Diagonal,
        ApronFront
    }

    public class TaxiwaySegment
    {
        private readonly List<Vector3> waypoints;

        public TaxiwaySegment(
            string segmentId,
            string displayName,
            string realWorldName,
            TaxiwaySegmentType segmentType,
            TaxiwayAxisType axisType,
            IEnumerable<Vector3> segmentWaypoints,
            string description,
            bool isProvisional)
        {
            SegmentId = segmentId;
            DisplayName = displayName;
            RealWorldName = realWorldName;
            SegmentType = segmentType;
            AxisType = axisType;
            waypoints = new List<Vector3>(segmentWaypoints);
            Description = description;
            IsProvisional = isProvisional;
        }

        public string SegmentId { get; private set; }
        public string DisplayName { get; private set; }
        public string RealWorldName { get; private set; }
        public TaxiwaySegmentType SegmentType { get; private set; }
        public TaxiwayAxisType AxisType { get; private set; }
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public string Description { get; private set; }
        public bool IsProvisional { get; private set; }
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
        private readonly List<string> segmentIds;

        public TaxiRouteCandidate(
            string routeId,
            string displayName,
            string description,
            string spotId,
            string runwayDesignator,
            IEnumerable<Vector3> routeWaypoints,
            bool isDefault)
            : this(routeId, displayName, description, spotId, runwayDesignator, routeWaypoints, null, string.Empty, isDefault)
        {
        }

        public TaxiRouteCandidate(
            string routeId,
            string displayName,
            string description,
            string spotId,
            string runwayDesignator,
            IEnumerable<Vector3> routeWaypoints,
            IEnumerable<string> routeSegmentIds,
            string routeInstructionText,
            bool isDefault)
        {
            RouteId = routeId;
            DisplayName = displayName;
            Description = description;
            SpotId = spotId;
            RunwayDesignator = runwayDesignator;
            waypoints = new List<Vector3>(routeWaypoints);
            segmentIds = routeSegmentIds != null ? new List<string>(routeSegmentIds) : new List<string>();
            RouteInstructionText = routeInstructionText;
            IsDefault = isDefault;
        }

        public string RouteId { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public string SpotId { get; private set; }
        public string RunwayDesignator { get; private set; }
        public IReadOnlyList<Vector3> Waypoints => waypoints;
        public IReadOnlyList<string> SegmentIds => segmentIds;
        public string RouteInstructionText { get; private set; }
        public bool IsDefault { get; private set; }
    }
}
