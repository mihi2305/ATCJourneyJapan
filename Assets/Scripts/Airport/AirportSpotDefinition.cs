using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    public enum AirportSpotArea
    {
        DOM,
        INTL,
        BASE,
        OTHER
    }

    public enum AircraftSizeClass
    {
        Narrowbody,
        Widebody,
        Any
    }

    public class AirportSpotDefinition
    {
        public AirportSpotDefinition(
            string tutorialId,
            string realWorldStyleId,
            AirportSpotArea area,
            AircraftSizeClass aircraftSizeClass,
            bool hasBoardingBridge,
            Vector3 position,
            Vector3 standScale,
            string boardingBridgeObjectName)
        {
            TutorialId = tutorialId;
            RealWorldStyleId = realWorldStyleId;
            Area = area;
            AircraftSizeClass = aircraftSizeClass;
            HasBoardingBridge = hasBoardingBridge;
            Position = position;
            StandScale = standScale;
            BoardingBridgeObjectName = boardingBridgeObjectName;
        }

        public string TutorialId { get; private set; }
        public string RealWorldStyleId { get; private set; }
        public AirportSpotArea Area { get; private set; }
        public AircraftSizeClass AircraftSizeClass { get; private set; }
        public bool HasBoardingBridge { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 StandScale { get; private set; }
        public string BoardingBridgeObjectName { get; private set; }
    }
}
