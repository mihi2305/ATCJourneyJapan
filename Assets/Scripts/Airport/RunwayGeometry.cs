using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    public class RunwayGeometry
    {
        public RunwayGeometry(
            string runwayId,
            Vector3 center,
            float length,
            float width,
            float heading18L,
            float heading36R,
            Vector3 endpoint18L,
            Vector3 endpoint36R,
            Vector3 centerlineStart,
            Vector3 centerlineEnd,
            Vector3 lineupPoint18L,
            Vector3 lineupPoint36R,
            Vector3 takeoffDirection18L,
            Vector3 takeoffDirection36R,
            Vector3 landingDirection18L,
            Vector3 landingDirection36R)
        {
            RunwayId = runwayId;
            Center = center;
            Length = length;
            Width = width;
            Heading18L = heading18L;
            Heading36R = heading36R;
            Endpoint18L = endpoint18L;
            Endpoint36R = endpoint36R;
            CenterlineStart = centerlineStart;
            CenterlineEnd = centerlineEnd;
            LineupPoint18L = lineupPoint18L;
            LineupPoint36R = lineupPoint36R;
            TakeoffDirection18L = takeoffDirection18L.normalized;
            TakeoffDirection36R = takeoffDirection36R.normalized;
            LandingDirection18L = landingDirection18L.normalized;
            LandingDirection36R = landingDirection36R.normalized;
        }

        public string RunwayId { get; private set; }
        public Vector3 Center { get; private set; }
        public float Length { get; private set; }
        public float Width { get; private set; }
        public float Heading18L { get; private set; }
        public float Heading36R { get; private set; }
        public Vector3 Endpoint18L { get; private set; }
        public Vector3 Endpoint36R { get; private set; }
        public Vector3 CenterlineStart { get; private set; }
        public Vector3 CenterlineEnd { get; private set; }
        public Vector3 LineupPoint18L { get; private set; }
        public Vector3 LineupPoint36R { get; private set; }
        public Vector3 TakeoffDirection18L { get; private set; }
        public Vector3 TakeoffDirection36R { get; private set; }
        public Vector3 LandingDirection18L { get; private set; }
        public Vector3 LandingDirection36R { get; private set; }

        public Vector3 GetLineupPoint(string operationDirection)
        {
            return Is36R(operationDirection) ? LineupPoint36R : LineupPoint18L;
        }

        public Vector3 GetTakeoffDirection(string operationDirection)
        {
            return Is36R(operationDirection) ? TakeoffDirection36R : TakeoffDirection18L;
        }

        public Vector3 GetLandingDirection(string operationDirection)
        {
            return Is36R(operationDirection) ? LandingDirection36R : LandingDirection18L;
        }

        public Vector3 GetDepartureEndPoint(string operationDirection)
        {
            return Is36R(operationDirection) ? Endpoint18L : Endpoint36R;
        }

        public Vector3 GetArrivalThresholdPoint(string operationDirection)
        {
            return Is36R(operationDirection) ? Endpoint36R : Endpoint18L;
        }

        private bool Is36R(string operationDirection)
        {
            return operationDirection == "36R";
        }
    }
}
