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
            string destination)
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
        public string RecommendedCommandId { get; private set; }
        public string NextTargetType { get; private set; }
        public string NextTargetId { get; private set; }
        public string NextTargetDisplayName { get; private set; }
        public string RunwayBilingualDisplay => $"{AssignedRunwayDisplayName}（RWY {ActiveRunwayDesignator}）";
        public string RunwayShortDisplay => $"RWY {ActiveRunwayDesignator}";

        public void UpdateRuntimeState(
            string currentState,
            string controllerPosition,
            string recommendedCommandId,
            string nextTargetType,
            string nextTargetId,
            string nextTargetDisplayName)
        {
            CurrentState = currentState;
            ControllerPosition = controllerPosition;
            RecommendedCommandId = recommendedCommandId;
            NextTargetType = nextTargetType;
            NextTargetId = nextTargetId;
            NextTargetDisplayName = nextTargetDisplayName;
        }
    }
}
