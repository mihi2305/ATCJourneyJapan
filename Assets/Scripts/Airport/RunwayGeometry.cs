using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    public class RunwayGeometry
    {
        public RunwayGeometry(
            string runwayId,
            string physicalRunwayId,
            string designatorAEnd,
            string designatorBOppositeEnd,
            Vector3 center,
            float length,
            float width,
            float headingForDesignatorA,
            float headingForDesignatorB,
            Vector3 designatorAThresholdPoint,
            Vector3 designatorBThresholdPoint,
            Vector3 centerlineStart,
            Vector3 centerlineEnd,
            Vector3 lineupPointForDesignatorA,
            Vector3 lineupPointForDesignatorB,
            Vector3 directionForDesignatorA,
            Vector3 directionForDesignatorB)
        {
            RunwayId = runwayId;
            PhysicalRunwayId = physicalRunwayId;
            DesignatorAEnd = designatorAEnd;
            DesignatorBOppositeEnd = designatorBOppositeEnd;
            Center = center;
            Length = length;
            Width = width;
            HeadingForDesignatorA = headingForDesignatorA;
            HeadingForDesignatorB = headingForDesignatorB;
            DesignatorAThresholdPoint = designatorAThresholdPoint;
            DesignatorBThresholdPoint = designatorBThresholdPoint;
            CenterlineStart = centerlineStart;
            CenterlineEnd = centerlineEnd;
            LineupPointForDesignatorA = lineupPointForDesignatorA;
            LineupPointForDesignatorB = lineupPointForDesignatorB;
            DirectionForDesignatorA = directionForDesignatorA.normalized;
            DirectionForDesignatorB = directionForDesignatorB.normalized;
        }

        public string RunwayId { get; private set; }
        public string PhysicalRunwayId { get; private set; }
        public string DesignatorAEnd { get; private set; }
        public string DesignatorBOppositeEnd { get; private set; }
        public string DesignatorNorthEnd => DesignatorAEnd;
        public string DesignatorSouthEnd => DesignatorBOppositeEnd;
        public Vector3 Center { get; private set; }
        public float Length { get; private set; }
        public float Width { get; private set; }
        public float HeadingForDesignatorA { get; private set; }
        public float HeadingForDesignatorB { get; private set; }
        public Vector3 DesignatorAThresholdPoint { get; private set; }
        public Vector3 DesignatorBThresholdPoint { get; private set; }
        public Vector3 NorthEndPoint => DesignatorAThresholdPoint;
        public Vector3 SouthEndPoint => DesignatorBThresholdPoint;
        public Vector3 CenterlineStart { get; private set; }
        public Vector3 CenterlineEnd { get; private set; }
        public Vector3 LineupPointForDesignatorA { get; private set; }
        public Vector3 LineupPointForDesignatorB { get; private set; }
        public Vector3 LineupPointForNorthEndDesignator => LineupPointForDesignatorA;
        public Vector3 LineupPointForSouthEndDesignator => LineupPointForDesignatorB;
        public Vector3 DirectionForDesignatorA { get; private set; }
        public Vector3 DirectionForDesignatorB { get; private set; }
        public Vector3 NorthToSouthDirection => DirectionForDesignatorA;
        public Vector3 SouthToNorthDirection => DirectionForDesignatorB;
        public IEnumerable<string> AvailableDesignators
        {
            get
            {
                yield return DesignatorAEnd;
                yield return DesignatorBOppositeEnd;
            }
        }

        public float Heading18L => GetHeadingForDesignator("18L");
        public float Heading36R => GetHeadingForDesignator("36R");
        public Vector3 Endpoint18L => GetThresholdPointForDesignator("18L");
        public Vector3 Endpoint36R => GetThresholdPointForDesignator("36R");
        public Vector3 LineupPoint18L => GetLineupPoint("18L");
        public Vector3 LineupPoint36R => GetLineupPoint("36R");
        public Vector3 TakeoffDirection18L => GetDirectionForDesignator("18L");
        public Vector3 TakeoffDirection36R => GetDirectionForDesignator("36R");
        public Vector3 LandingDirection18L => GetDirectionForDesignator("18L");
        public Vector3 LandingDirection36R => GetDirectionForDesignator("36R");

        public Vector3 GetLineupPoint(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? LineupPointForDesignatorB : LineupPointForDesignatorA;
        }

        public Vector3 GetTakeoffDirection(string operationDirection)
        {
            return GetDirectionForDesignator(operationDirection);
        }

        public Vector3 GetLandingDirection(string operationDirection)
        {
            return GetDirectionForDesignator(operationDirection);
        }

        public Vector3 GetDirectionForDesignator(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? DirectionForDesignatorB : DirectionForDesignatorA;
        }

        public float GetHeadingForDesignator(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? HeadingForDesignatorB : HeadingForDesignatorA;
        }

        public Vector3 GetThresholdPointForDesignator(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? DesignatorBThresholdPoint : DesignatorAThresholdPoint;
        }

        public Vector3 GetFinalApproachStart(string operationDirection)
        {
            var direction = GetDirectionForDesignator(operationDirection);
            return GetArrivalThresholdPoint(operationDirection) - direction * 12f;
        }

        public Vector3 GetFinalApproachFix(string operationDirection)
        {
            var direction = GetDirectionForDesignator(operationDirection);
            return GetArrivalThresholdPoint(operationDirection) - direction * 6f;
        }

        public Vector3 GetTouchdownPoint(string operationDirection)
        {
            var direction = GetDirectionForDesignator(operationDirection);
            return GetArrivalThresholdPoint(operationDirection) + direction * 2.25f;
        }

        public Vector3 GetRolloutEndPoint(string operationDirection)
        {
            var direction = GetDirectionForDesignator(operationDirection);
            return GetArrivalThresholdPoint(operationDirection) + direction * (Length * 0.78f);
        }

        public Vector3 GetVacateStartPoint(string operationDirection)
        {
            return GetRolloutEndPoint(operationDirection);
        }

        public IEnumerable<Vector3> GetFinalApproachRoute(string operationDirection)
        {
            return new[]
            {
                GetFinalApproachStart(operationDirection),
                GetFinalApproachFix(operationDirection),
                GetArrivalThresholdPoint(operationDirection),
                GetTouchdownPoint(operationDirection)
            };
        }

        public IEnumerable<Vector3> GetLandingRolloutRoute(string operationDirection)
        {
            return new[]
            {
                GetTouchdownPoint(operationDirection),
                GetRolloutEndPoint(operationDirection)
            };
        }

        public Vector3 GetDepartureEndPoint(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? DesignatorAThresholdPoint : DesignatorBThresholdPoint;
        }

        public Vector3 GetArrivalThresholdPoint(string operationDirection)
        {
            return IsOppositeDesignator(operationDirection) ? DesignatorBThresholdPoint : DesignatorAThresholdPoint;
        }

        private bool IsOppositeDesignator(string operationDirection)
        {
            return ResolveDesignator(operationDirection) == DesignatorBOppositeEnd;
        }

        private string ResolveDesignator(string operationDirection)
        {
            var normalized = NormalizeDesignator(operationDirection);
            if (normalized == DesignatorAEnd || normalized == DesignatorBOppositeEnd)
            {
                return normalized;
            }

            Debug.LogWarning($"Unknown runway designator '{operationDirection}' for {RunwayId}. Falling back to {DesignatorAEnd}.");
            return DesignatorAEnd;
        }

        private string NormalizeDesignator(string operationDirection)
        {
            if (string.IsNullOrEmpty(operationDirection))
            {
                return DesignatorAEnd;
            }

            var normalized = operationDirection.Trim().ToUpperInvariant();
            if (normalized.StartsWith("RUNWAY "))
            {
                normalized = normalized.Substring("RUNWAY ".Length).Trim();
            }

            if (normalized.StartsWith("RWY "))
            {
                normalized = normalized.Substring("RWY ".Length).Trim();
            }

            return normalized;
        }
    }
}
