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
            string gateOrSpot,
            string origin,
            string destination)
        {
            FlightId = flightId;
            AircraftType = aircraftType;
            OperationType = operationType;
            AssignedRunwayId = assignedRunwayId;
            AssignedRunwayDisplayName = assignedRunwayDisplayName;
            ActiveRunwayDesignator = activeRunwayDesignator;
            GateOrSpot = gateOrSpot;
            Origin = origin;
            Destination = destination;
        }

        public string FlightId { get; private set; }
        public string AircraftType { get; private set; }
        public string OperationType { get; private set; }
        public string AssignedRunwayId { get; private set; }
        public string AssignedRunwayDisplayName { get; private set; }
        public string ActiveRunwayDesignator { get; private set; }
        public string GateOrSpot { get; private set; }
        public string CurrentState { get; private set; }
        public string ControllerPosition { get; private set; }
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public string RecommendedCommandId { get; private set; }
        public string RunwayBilingualDisplay => $"{AssignedRunwayDisplayName}（RWY {ActiveRunwayDesignator}）";

        public void UpdateRuntimeState(string currentState, string controllerPosition, string recommendedCommandId)
        {
            CurrentState = currentState;
            ControllerPosition = controllerPosition;
            RecommendedCommandId = recommendedCommandId;
        }
    }
}
