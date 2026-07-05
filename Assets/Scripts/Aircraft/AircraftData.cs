namespace ATCJourneyJapan.Aircraft
{
    public class AircraftData
    {
        public AircraftData(
            string flightId,
            string aircraftType,
            string operationType,
            string assignedRunwayId,
            string assignedRunwayDisplayName,
            string activeRunwayDesignator,
            string spotId,
            string spotDisplayName,
            string origin,
            string destination,
            string scheduledDepartureTime = "",
            string estimatedDepartureTime = "",
            string actualDepartureTime = "",
            string scheduledArrivalTime = "",
            string estimatedArrivalTime = "",
            string actualArrivalTime = "",
            int delayMinutes = 0)
        {
            FlightId = flightId;
            AircraftType = aircraftType;
            OperationType = operationType;
            AssignedRunwayId = assignedRunwayId;
            AssignedRunwayDisplayName = assignedRunwayDisplayName;
            ActiveRunwayDesignator = activeRunwayDesignator;
            SpotId = spotId;
            SpotDisplayName = spotDisplayName;
            Origin = origin;
            Destination = destination;
            ScheduledDepartureTime = scheduledDepartureTime;
            EstimatedDepartureTime = estimatedDepartureTime;
            ActualDepartureTime = actualDepartureTime;
            ScheduledArrivalTime = scheduledArrivalTime;
            EstimatedArrivalTime = estimatedArrivalTime;
            ActualArrivalTime = actualArrivalTime;
            DelayMinutes = delayMinutes;
        }

        public string FlightId { get; private set; }
        public string AircraftType { get; private set; }
        public string OperationType { get; private set; }
        public string AssignedRunwayId { get; private set; }
        public string AssignedRunwayDisplayName { get; private set; }
        public string ActiveRunwayDesignator { get; private set; }
        public string SpotId { get; private set; }
        public string SpotDisplayName { get; private set; }
        public string GateOrSpot => SpotDisplayName;
        public string CurrentState { get; private set; }
        public string ControllerPosition { get; private set; }
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public string ScheduledDepartureTime { get; private set; }
        public string EstimatedDepartureTime { get; private set; }
        public string ActualDepartureTime { get; private set; }
        public string ScheduledArrivalTime { get; private set; }
        public string EstimatedArrivalTime { get; private set; }
        public string ActualArrivalTime { get; private set; }
        public int DelayMinutes { get; private set; }
        public float HeadingDegrees { get; private set; }
        public string FacingDirection { get; private set; }
        public string RecommendedCommandId { get; private set; }
        public string NextTargetType { get; private set; }
        public string NextTargetId { get; private set; }
        public string NextTargetDisplayName { get; private set; }
        public bool HasActiveRunwayDesignator => !string.IsNullOrEmpty(ActiveRunwayDesignator);
        public string RunwayBilingualDisplay => HasActiveRunwayDesignator ? $"{AssignedRunwayDisplayName}（RWY {ActiveRunwayDesignator}）" : $"{AssignedRunwayDisplayName}（RWY --）";
        public string RunwayShortDisplay => HasActiveRunwayDesignator ? $"RWY {ActiveRunwayDesignator}" : "RWY --";

        public void AssignRunwayDirection(string runwayId, string runwayDisplayName, string activeRunwayDesignator)
        {
            if (!string.IsNullOrEmpty(runwayId))
            {
                AssignedRunwayId = runwayId;
            }

            if (!string.IsNullOrEmpty(runwayDisplayName))
            {
                AssignedRunwayDisplayName = runwayDisplayName;
            }

            if (!string.IsNullOrEmpty(activeRunwayDesignator))
            {
                ActiveRunwayDesignator = activeRunwayDesignator;
            }
        }

        public void UpdateRuntimeState(
            string currentState,
            string controllerPosition,
            string recommendedCommandId,
            string nextTargetType,
            string nextTargetId,
            string nextTargetDisplayName,
            float headingDegrees,
            string facingDirection)
        {
            CurrentState = currentState;
            ControllerPosition = controllerPosition;
            RecommendedCommandId = recommendedCommandId;
            NextTargetType = nextTargetType;
            NextTargetId = nextTargetId;
            NextTargetDisplayName = nextTargetDisplayName;
            HeadingDegrees = headingDegrees;
            FacingDirection = facingDirection;
        }
    }
}
