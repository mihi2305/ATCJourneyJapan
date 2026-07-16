using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    // Builds and owns prototype airport data. Later this can read AirportData/StageData without changing aircraft commands.
    public class AirportManager : MonoBehaviour
    {
        [SerializeField] private string airportId = "PrototypeIsland";
        [SerializeField] private string airportDisplayName = "Prototype Island Airport";

        private readonly List<RunwayController> runways = new List<RunwayController>();
        private readonly List<RunwayData> runwayData = new List<RunwayData>();
        private readonly List<RunwayGeometry> runwayGeometries = new List<RunwayGeometry>();
        private readonly List<TaxiwaySegment> taxiwaySegments = new List<TaxiwaySegment>();
        private readonly List<RunwayAccessPoint> runwayAccessPoints = new List<RunwayAccessPoint>();
        private readonly List<RunwayAccessUsage> runwayAccessUsages = new List<RunwayAccessUsage>();
        private readonly List<TaxiwayRouteDefinition> taxiwayRoutes = new List<TaxiwayRouteDefinition>();
        private readonly List<TaxiRouteCandidate> departureTaxiRouteCandidates = new List<TaxiRouteCandidate>();
        private readonly List<TaxiRouteCandidate> arrivalTaxiRouteCandidates = new List<TaxiRouteCandidate>();
        private readonly List<Vector3> gatePositions = new List<Vector3>();
        private readonly List<AirportSpotDefinition> spotDefinitions = new List<AirportSpotDefinition>();
        private Material runwayMaterial;
        private Material futureRunwayMaterial;
        private Material runwayEdgeMaterial;
        private Material runwayMarkingMaterial;
        private Material taxiwayMaterial;
        private Material taxiwayMarkingMaterial;
        private Material gateMaterial;
        private Material apronMaterial;
        private Material secondaryApronMaterial;
        private Material militaryApronMaterial;
        private Material passengerApronMaterial;
        private Material hangarMaterial;
        private Material supportBuildingMaterial;
        private Material areaDividerMaterial;
        private Material spotMarkingMaterial;
        private Material groundMaterial;
        private Material seaMaterial;
        private Material shorelineMaterial;
        private Material terminalMaterial;
        private Material boardingBridgeMaterial;
        private Material towerMaterial;
        private Material holdShortMaterial;

        public IReadOnlyList<RunwayController> Runways => runways;
        public IReadOnlyList<RunwayData> RunwayData => runwayData;
        public IReadOnlyList<RunwayGeometry> RunwayGeometries => runwayGeometries;
        public IReadOnlyList<TaxiwaySegment> TaxiwaySegments => taxiwaySegments;
        public IReadOnlyList<RunwayAccessPoint> RunwayAccessPoints => runwayAccessPoints;
        public IReadOnlyList<RunwayAccessUsage> RunwayAccessUsages => runwayAccessUsages;
        public IReadOnlyList<TaxiwayRouteDefinition> TaxiwayRoutes => taxiwayRoutes;
        public IReadOnlyList<TaxiRouteCandidate> DepartureTaxiRouteCandidates => departureTaxiRouteCandidates;
        public IReadOnlyList<TaxiRouteCandidate> ArrivalTaxiRouteCandidates => arrivalTaxiRouteCandidates;
        public IReadOnlyList<AirportSpotDefinition> SpotDefinitions => spotDefinitions;
        public RunwayData PrimaryRunwayData => runwayData.Count > 0 ? runwayData[0] : null;
        public RunwayGeometry PrimaryRunwayGeometry => runwayGeometries.Count > 0 ? runwayGeometries[0] : null;
        public string CurrentPrimaryRunwayDesignator => GetPrimaryRunwayDirection();
        public Vector3 ArrivalSpawnPosition => new Vector3(-26f, 0.6f, 0f);
        public Vector3 DepartureSpawnPosition => gatePositions.Count > 1 ? gatePositions[1] : new Vector3(-10f, 0.6f, -9f);
        public Vector3 SecondaryArrivalSpawnPosition => new Vector3(-28f, 0.6f, 3.2f);
        public Vector3 SecondaryDepartureSpawnPosition => gatePositions.Count > 3 ? gatePositions[3] : new Vector3(-16f, 0.6f, -11.6f);
        public Vector3 PushbackReadyPosition => new Vector3(DepartureSpawnPosition.x, 0.6f, -6.45f);
        public Vector3 HoldShortPosition => new Vector3(12f, 0.55f, -3f);
        public Vector3 PrimaryRunwayLineUpPosition => GetPrimaryRunwayLineupPoint(GetPrimaryRunwayDirection());
        public Vector3 PrimaryRunwayTakeoffDirection => GetPrimaryRunwayTakeoffDirection(GetPrimaryRunwayDirection());

        public bool TryGetSpotPosition(string spotId, out Vector3 position)
        {
            var normalizedSpotId = NormalizeSpotId(spotId);
            foreach (var spotDefinition in spotDefinitions)
            {
                if (NormalizeSpotId(spotDefinition.TutorialId) == normalizedSpotId)
                {
                    position = spotDefinition.Position;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        public void Initialize()
        {
            CreateMaterials();
            CreateRunwayData();
            CreateGeometryData();
            CreateEnvironment();
        }

        public RunwayData GetRunwayData(string runwayId)
        {
            foreach (var runway in runwayData)
            {
                if (runway.RunwayId == runwayId)
                {
                    return runway;
                }
            }

            return null;
        }

        public RunwayGeometry GetRunwayGeometry(string runwayId)
        {
            foreach (var geometry in runwayGeometries)
            {
                if (geometry.RunwayId == runwayId)
                {
                    return geometry;
                }
            }

            return null;
        }

        public TaxiwayRouteDefinition GetTaxiwayRoute(string taxiwayId)
        {
            foreach (var taxiwayRoute in taxiwayRoutes)
            {
                if (taxiwayRoute.TaxiwayId == taxiwayId)
                {
                    return taxiwayRoute;
                }
            }

            return null;
        }

        public TaxiwaySegment GetTaxiwaySegment(string segmentId)
        {
            foreach (var taxiwaySegment in taxiwaySegments)
            {
                if (taxiwaySegment.SegmentId == segmentId)
                {
                    return taxiwaySegment;
                }
            }

            return null;
        }

        public RunwayAccessPoint GetRunwayAccessPoint(string accessPointId)
        {
            foreach (var accessPoint in runwayAccessPoints)
            {
                if (accessPoint.AccessPointId == accessPointId)
                {
                    return accessPoint;
                }
            }

            return null;
        }

        public RunwayAccessUsage GetRunwayAccessUsage(string usageId)
        {
            foreach (var usage in runwayAccessUsages)
            {
                if (usage.UsageId == usageId)
                {
                    return usage;
                }
            }

            return null;
        }

        public IReadOnlyList<TaxiRouteCandidate> GetDepartureTaxiRouteCandidates(string spotId, string runwayDesignator)
        {
            var normalizedSpotId = NormalizeSpotId(spotId);
            var normalizedRunwayDesignator = NormalizeRunwayDesignator(runwayDesignator);
            var matches = new List<TaxiRouteCandidate>();

            foreach (var candidate in departureTaxiRouteCandidates)
            {
                if (NormalizeSpotId(candidate.SpotId) == normalizedSpotId
                    && NormalizeRunwayDesignator(candidate.RunwayDesignator) == normalizedRunwayDesignator)
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        public TaxiRouteCandidate GetDefaultDepartureTaxiRouteCandidate(string spotId, string runwayDesignator)
        {
            var candidates = GetDepartureTaxiRouteCandidates(spotId, runwayDesignator);
            TaxiRouteCandidate fallbackCandidate = null;

            foreach (var candidate in candidates)
            {
                if (fallbackCandidate == null)
                {
                    fallbackCandidate = candidate;
                }

                if (candidate.IsDefault)
                {
                    return candidate;
                }
            }

            return fallbackCandidate;
        }

        public IReadOnlyList<TaxiRouteCandidate> GetArrivalTaxiRouteCandidates(string spotId, string runwayDesignator)
        {
            var normalizedSpotId = NormalizeSpotId(spotId);
            var normalizedRunwayDesignator = NormalizeRunwayDesignator(runwayDesignator);
            var matches = new List<TaxiRouteCandidate>();

            foreach (var candidate in arrivalTaxiRouteCandidates)
            {
                if (NormalizeSpotId(candidate.SpotId) == normalizedSpotId
                    && NormalizeRunwayDesignator(candidate.RunwayDesignator) == normalizedRunwayDesignator)
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        public TaxiRouteCandidate GetDefaultArrivalTaxiRouteCandidate(string spotId, string runwayDesignator)
        {
            var candidates = GetArrivalTaxiRouteCandidates(spotId, runwayDesignator);
            TaxiRouteCandidate fallbackCandidate = null;

            foreach (var candidate in candidates)
            {
                if (fallbackCandidate == null)
                {
                    fallbackCandidate = candidate;
                }

                if (candidate.IsDefault)
                {
                    return candidate;
                }
            }

            return fallbackCandidate;
        }

        public IEnumerable<Vector3> GetArrivalFinalRoute()
        {
            return GetArrivalFinalRoute(GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetArrivalFinalRoute(string operationDirection)
        {
            var runway = PrimaryRunwayGeometry;
            if (runway != null)
            {
                return runway.GetFinalApproachRoute(operationDirection);
            }

            var landingDirection = GetPrimaryRunwayLandingDirection(operationDirection);
            var threshold = new Vector3(-13.25f, 0.6f, 0f);
            return new[]
            {
                threshold - landingDirection * 12f,
                threshold - landingDirection * 6f,
                threshold,
                threshold + landingDirection * 2.25f
            };
        }

        public Vector3 GetArrivalFinalApproachStart(string operationDirection)
        {
            var runway = PrimaryRunwayGeometry;
            if (runway != null)
            {
                return runway.GetFinalApproachStart(operationDirection);
            }

            return new Vector3(-25.25f, 0.6f, 0f);
        }

        public Vector3 GetPrimaryRunwayTakeoffDirection(string operationDirection)
        {
            return PrimaryRunwayGeometry != null ? PrimaryRunwayGeometry.GetTakeoffDirection(operationDirection) : Vector3.right;
        }

        public Vector3 GetPrimaryRunwayLandingDirection(string operationDirection)
        {
            return PrimaryRunwayGeometry != null ? PrimaryRunwayGeometry.GetLandingDirection(operationDirection) : Vector3.right;
        }

        public Vector3 GetPrimaryRunwayLineupPoint(string operationDirection)
        {
            return PrimaryRunwayGeometry != null ? PrimaryRunwayGeometry.GetLineupPoint(operationDirection) : new Vector3(14.5f, 0.6f, 0f);
        }

        public Vector3 GetHoldShortPosition(string operationDirection)
        {
            return Is36R(operationDirection) ? new Vector3(-7f, 0.55f, -3f) : HoldShortPosition;
        }

        public Vector3 GetHoldShortPosition(string operationDirection, string runwayEntryUsageId)
        {
            if (string.IsNullOrEmpty(runwayEntryUsageId))
            {
                return GetHoldShortPosition(operationDirection);
            }

            var runwayAccessPoint = GetAccessPointForUsage(runwayEntryUsageId);
            if (runwayAccessPoint == null)
            {
                return GetHoldShortPosition(operationDirection);
            }

            return new Vector3(runwayAccessPoint.Position.x, 0.55f, -3f);
        }

        public IEnumerable<string> GetPrimaryRunwayDirectionOptions()
        {
            if (PrimaryRunwayData == null)
            {
                yield break;
            }

            yield return PrimaryRunwayData.DesignatorA;
            yield return PrimaryRunwayData.DesignatorB;
        }

        private string GetPrimaryRunwayDirection()
        {
            return PrimaryRunwayData != null ? PrimaryRunwayData.CurrentActiveDesignator : "18L";
        }

        private bool Is36R(string operationDirection)
        {
            return NormalizeRunwayDesignator(operationDirection) == "36R";
        }

        private string NormalizeRunwayDesignator(string operationDirection)
        {
            if (string.IsNullOrEmpty(operationDirection))
            {
                return GetPrimaryRunwayDirection();
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

        private string NormalizeSpotId(string spotId)
        {
            return string.IsNullOrEmpty(spotId) ? string.Empty : spotId.Trim().ToUpperInvariant().Replace(" ", "_");
        }

        public IEnumerable<Vector3> GetLandingRollRoute()
        {
            return GetLandingRollRoute(GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetLandingRollRoute(string operationDirection)
        {
            var runway = PrimaryRunwayGeometry;
            if (runway != null)
            {
                return runway.GetLandingRolloutRoute(operationDirection);
            }

            var landingDirection = GetPrimaryRunwayLandingDirection(operationDirection);
            var touchdownPoint = new Vector3(-11f, 0.6f, 0f);
            return new[]
            {
                touchdownPoint,
                touchdownPoint + landingDirection * 14f
            };
        }

        public IEnumerable<Vector3> GetVacateRunwayRoute()
        {
            return GetVacateRunwayRoute(GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetVacateRunwayRoute(string operationDirection)
        {
            var runway = PrimaryRunwayGeometry;
            var vacateStart = runway != null ? runway.GetVacateStartPoint(operationDirection) : (Is36R(operationDirection) ? new Vector3(-7f, 0.6f, 0f) : new Vector3(12f, 0.6f, 0f));
            var connectorSegment = GetTaxiwaySegment(GetConnectedSegmentIdForUsage(GetDefaultArrivalExitUsageId(operationDirection)));
            if (connectorSegment != null && connectorSegment.Waypoints.Count >= 2)
            {
                return new[]
                {
                    vacateStart,
                    connectorSegment.Waypoints[1],
                    connectorSegment.Waypoints[0]
                };
            }

            var exitX = Is36R(operationDirection) ? 12f : -7f;
            return new[]
            {
                vacateStart,
                new Vector3(exitX, 0.6f, -2.5f),
                new Vector3(exitX, 0.6f, -5f)
            };
        }

        public IEnumerable<Vector3> GetVacateRunwayRoute(string operationDirection, string runwayExitUsageId, Vector3 currentPosition)
        {
            if (string.IsNullOrEmpty(runwayExitUsageId))
            {
                return GetVacateRunwayRoute(operationDirection);
            }

            var accessPoint = GetAccessPointForUsage(runwayExitUsageId);
            var connectorSegment = accessPoint != null ? GetTaxiwaySegment(accessPoint.ConnectedSegmentId) : null;
            if (accessPoint != null && connectorSegment != null && connectorSegment.Waypoints.Count >= 2)
            {
                var runwayExitPoint = accessPoint.Position;
                runwayExitPoint.y = currentPosition.y;
                return new[]
                {
                    runwayExitPoint,
                    connectorSegment.Waypoints[1],
                    connectorSegment.Waypoints[0]
                };
            }

            return GetVacateRunwayRoute(operationDirection);
        }

        public IEnumerable<Vector3> GetTaxiToAvailableGateRoute()
        {
            var arrivalGateLink = GetTaxiwayRoute("TWY_ARRIVAL_GATE_LINK");
            if (arrivalGateLink != null && arrivalGateLink.Waypoints.Count >= 2)
            {
                return new[]
                {
                    arrivalGateLink.Waypoints[0],
                    arrivalGateLink.Waypoints[1],
                    gatePositions[0]
                };
            }

            return new[]
            {
                new Vector3(12f, 0.6f, -5f),
                new Vector3(10.5f, 0.6f, -6.8f),
                gatePositions[0]
            };
        }

        public IEnumerable<Vector3> GetPushbackRoute()
        {
            return GetPushbackRoute(DepartureSpawnPosition);
        }

        public IEnumerable<Vector3> GetPushbackRoute(Vector3 startPosition)
        {
            var pushbackLineZ = startPosition.x >= 22f ? -6.2f : -6.45f;
            return new[]
            {
                new Vector3(startPosition.x, 0.6f, pushbackLineZ)
            };
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute()
        {
            return GetTaxiToHoldRoute(PushbackReadyPosition, GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute(Vector3 startPosition)
        {
            return GetTaxiToHoldRoute(startPosition, GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute(Vector3 startPosition, string operationDirection)
        {
            var taxiwayMainEntry = new Vector3(startPosition.x, 0.6f, -5f);
            var runwayLinkX = GetTaxiwayPointForUsage(GetDefaultDepartureEntryUsageId(operationDirection)).x;
            return new[]
            {
                taxiwayMainEntry,
                new Vector3(runwayLinkX, 0.6f, -5f),
                GetHoldShortPosition(operationDirection)
            };
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute(string spotId, string operationDirection)
        {
            var defaultCandidate = GetDefaultDepartureTaxiRouteCandidate(spotId, operationDirection);
            if (defaultCandidate != null)
            {
                return defaultCandidate.Waypoints;
            }

            return GetTaxiToHoldRoute(DepartureSpawnPosition, operationDirection);
        }

        public IEnumerable<Vector3> GetLineUpRoute()
        {
            return GetLineUpRoute(HoldShortPosition, GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetLineUpRoute(Vector3 startPosition)
        {
            return GetLineUpRoute(startPosition, GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetLineUpRoute(Vector3 startPosition, string operationDirection)
        {
            var entryX = GetRunwayPointForUsage(GetDefaultDepartureEntryUsageId(operationDirection)).x;
            return new[]
            {
                new Vector3(entryX, 0.6f, -1.05f),
                new Vector3(entryX, 0.6f, 0f),
                GetPrimaryRunwayLineupPoint(operationDirection)
            };
        }

        public IEnumerable<Vector3> GetLineUpRoute(Vector3 startPosition, string operationDirection, string runwayEntryUsageId)
        {
            if (string.IsNullOrEmpty(runwayEntryUsageId))
            {
                return GetLineUpRoute(startPosition, operationDirection);
            }

            var runwayAccessPoint = GetAccessPointForUsage(runwayEntryUsageId);
            if (runwayAccessPoint == null)
            {
                return GetLineUpRoute(startPosition, operationDirection);
            }

            var entryPoint = runwayAccessPoint.Position;
            var takeoffDirection = GetPrimaryRunwayTakeoffDirection(operationDirection);
            return new[]
            {
                new Vector3(entryPoint.x, 0.6f, -1.05f),
                entryPoint,
                GetIntersectionLineupPoint(operationDirection, runwayEntryUsageId)
            };
        }

        public IEnumerable<Vector3> GetTakeoffRoute()
        {
            return GetTakeoffRoute(GetPrimaryRunwayDirection());
        }

        public IEnumerable<Vector3> GetTakeoffRoute(string operationDirection)
        {
            var runway = PrimaryRunwayGeometry;
            if (runway != null)
            {
                return runway.GetTakeoffRoute(operationDirection);
            }

            var takeoffDirection = GetPrimaryRunwayTakeoffDirection(operationDirection);
            var lineupPoint = GetPrimaryRunwayLineupPoint(operationDirection);
            var departureEnd = new Vector3(19.25f, 0.6f, 0f);
            var climbPoint = departureEnd + takeoffDirection * 6.75f;
            return new[]
            {
                lineupPoint + takeoffDirection * 2.5f,
                departureEnd - takeoffDirection * 0.25f,
                new Vector3(climbPoint.x, 2.4f, climbPoint.z)
            };
        }

        public IEnumerable<Vector3> GetTakeoffRoute(string operationDirection, string runwayEntryUsageId, Vector3 currentPosition)
        {
            if (string.IsNullOrEmpty(runwayEntryUsageId))
            {
                return GetTakeoffRoute(operationDirection);
            }

            var runway = PrimaryRunwayGeometry;
            var runwayAccessPoint = GetAccessPointForUsage(runwayEntryUsageId);
            if (runway == null || runwayAccessPoint == null)
            {
                return GetTakeoffRoute(operationDirection);
            }

            var takeoffDirection = runway.GetTakeoffDirection(operationDirection);
            var runwayStart = new Vector3(currentPosition.x, 0.6f, runwayAccessPoint.Position.z);
            var minimumStart = runwayAccessPoint.Position + takeoffDirection * 1.5f;
            if (Vector3.Dot(runwayStart - runwayAccessPoint.Position, takeoffDirection) < 1.25f)
            {
                runwayStart = minimumStart;
            }

            var departureEnd = runway.GetDepartureEndPoint(operationDirection);
            var climbPoint = departureEnd + takeoffDirection * 6.75f;
            return new[]
            {
                runwayStart,
                departureEnd - takeoffDirection * 0.25f,
                new Vector3(climbPoint.x, 2.4f, climbPoint.z)
            };
        }

        public Vector3 GetIntersectionLineupPoint(string operationDirection, string runwayEntryUsageId)
        {
            var runwayAccessPoint = GetAccessPointForUsage(runwayEntryUsageId);
            if (runwayAccessPoint == null)
            {
                return GetPrimaryRunwayLineupPoint(operationDirection);
            }

            return runwayAccessPoint.Position + GetPrimaryRunwayTakeoffDirection(operationDirection) * 1.5f;
        }

        private void CreateMaterials()
        {
            runwayMaterial = CreateMaterial("Runway Asphalt", new Color(0.13f, 0.14f, 0.145f));
            futureRunwayMaterial = CreateMaterial("Future Runway Asphalt", new Color(0.31f, 0.34f, 0.35f));
            runwayEdgeMaterial = CreateMaterial("Runway Edge Paint", new Color(0.9f, 0.92f, 0.86f));
            runwayMarkingMaterial = CreateMaterial("Runway Marking Paint", new Color(0.98f, 0.96f, 0.86f));
            taxiwayMaterial = CreateMaterial("Taxiway Asphalt", new Color(0.22f, 0.29f, 0.32f));
            taxiwayMarkingMaterial = CreateMaterial("Taxiway Centerline Paint", new Color(1f, 0.76f, 0.22f));
            gateMaterial = CreateMaterial("Spot Apron Concrete", new Color(0.22f, 0.44f, 0.42f));
            apronMaterial = CreateMaterial("Terminal Apron Concrete", new Color(0.29f, 0.37f, 0.36f));
            secondaryApronMaterial = CreateMaterial("Secondary Apron Concrete", new Color(0.25f, 0.34f, 0.34f));
            militaryApronMaterial = CreateMaterial("Base Apron Concrete", new Color(0.26f, 0.29f, 0.27f));
            passengerApronMaterial = CreateMaterial("Passenger Apron Concrete", new Color(0.35f, 0.39f, 0.38f));
            hangarMaterial = CreateMaterial("Hangar Blockout", new Color(0.38f, 0.42f, 0.4f));
            supportBuildingMaterial = CreateMaterial("Support Building Blockout", new Color(0.48f, 0.52f, 0.47f));
            areaDividerMaterial = CreateMaterial("Area Divider Paint", new Color(0.76f, 0.79f, 0.68f));
            spotMarkingMaterial = CreateMaterial("Spot Marking Paint", new Color(0.95f, 0.96f, 0.84f));
            groundMaterial = CreateMaterial("Ground Green", new Color(0.24f, 0.42f, 0.29f));
            seaMaterial = CreateMaterial("Naha Sea Blockout", new Color(0.03f, 0.48f, 0.72f));
            shorelineMaterial = CreateMaterial("Shoreline Blockout", new Color(0.52f, 0.82f, 0.84f));
            terminalMaterial = CreateMaterial("Terminal Blockout", new Color(0.68f, 0.72f, 0.7f));
            boardingBridgeMaterial = CreateMaterial("Boarding Bridge Blockout", new Color(0.82f, 0.84f, 0.8f));
            towerMaterial = CreateMaterial("Tower Blockout", new Color(0.86f, 0.88f, 0.82f));
            holdShortMaterial = CreateMaterial("Hold Yellow", new Color(0.85f, 0.68f, 0.18f));
        }

        private Material CreateMaterial(string materialName, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };
            return material;
        }

        private void CreateRunwayData()
        {
            runwayData.Clear();
            runwayData.Add(new RunwayData("RWY_A", "A滑走路", "Runway A", "18L", "36R", "18L", true, "ArrivalAndDeparture"));
            runwayData.Add(new RunwayData("RWY_B", "B滑走路", "Runway B", "18R", "36L", "18R", false, "FutureExpansion"));
        }

        private void CreateGeometryData()
        {
            runwayGeometries.Clear();
            // A runway is drawn horizontally in Unity. The current minimap convention is:
            // 36R at the left end moving right, and 18L at the right end moving left.
            runwayGeometries.Add(new RunwayGeometry(
                "RWY_A",
                "A",
                "18L",
                "36R",
                new Vector3(3f, 0.6f, 0f),
                32.5f,
                2.35f,
                180f,
                360f,
                new Vector3(19.25f, 0.6f, 0f),
                new Vector3(-13.25f, 0.6f, 0f),
                new Vector3(-13.25f, 0.6f, 0f),
                new Vector3(19.25f, 0.6f, 0f),
                new Vector3(16.75f, 0.6f, 0f),
                new Vector3(-10.75f, 0.6f, 0f),
                Vector3.left,
                Vector3.right));

            // B runway remains future-only in gameplay. It follows the same north/south mapping:
            // 18R is north to south, and 36L is south to north.
            runwayGeometries.Add(new RunwayGeometry(
                "RWY_B",
                "B",
                "18R",
                "36L",
                new Vector3(3f, 0.6f, 10.2f),
                29.2f,
                2.8f,
                180f,
                360f,
                new Vector3(-11.6f, 0.6f, 10.2f),
                new Vector3(17.6f, 0.6f, 10.2f),
                new Vector3(-11.6f, 0.6f, 10.2f),
                new Vector3(17.6f, 0.6f, 10.2f),
                new Vector3(-9.2f, 0.6f, 10.2f),
                new Vector3(15.2f, 0.6f, 10.2f),
                Vector3.right,
                Vector3.left));

            CreateTaxiwaySegmentData();
            CreateRunwayAccessData();

            taxiwayRoutes.Clear();
            taxiwayRoutes.Add(new TaxiwayRouteDefinition(
                "TWY_MAIN",
                "Taxiway Main",
                new[] { new Vector3(-13f, 0.6f, -5f), new Vector3(12f, 0.6f, -5f) },
                "Terminal spurs",
                "RWY_A east link",
                TaxiwayRoutePurpose.Both));
            taxiwayRoutes.Add(new TaxiwayRouteDefinition(
                "TWY_EAST_RUNWAY_LINK",
                "Taxiway East Runway Link",
                new[] { new Vector3(12f, 0.6f, -5f), new Vector3(12f, 0.6f, -2.5f), new Vector3(12f, 0.6f, 0f) },
                "TWY_MAIN",
                "RWY_A",
                TaxiwayRoutePurpose.Both));
            taxiwayRoutes.Add(new TaxiwayRouteDefinition(
                "TWY_WEST_RUNWAY_LINK",
                "Taxiway West Runway Link",
                new[] { new Vector3(-7f, 0.6f, -5f), new Vector3(-7f, 0.6f, -2.5f), new Vector3(-7f, 0.6f, 0f) },
                "TWY_MAIN",
                "RWY_A",
                TaxiwayRoutePurpose.Both));
            taxiwayRoutes.Add(new TaxiwayRouteDefinition(
                "TWY_TERMINAL_SPUR",
                "Taxiway T Terminal Spur",
                new[] { new Vector3(10.8f, 0.6f, -6.45f), new Vector3(10.8f, 0.6f, -5f) },
                "DOM pushback line",
                "TWY_MAIN",
                TaxiwayRoutePurpose.Departure));
            taxiwayRoutes.Add(new TaxiwayRouteDefinition(
                "TWY_ARRIVAL_GATE_LINK",
                "Arrival Gate Link",
                new[] { new Vector3(12f, 0.6f, -5f), new Vector3(10.5f, 0.6f, -6.8f) },
                "TWY_MAIN",
                "SPOT 01",
                TaxiwayRoutePurpose.Arrival));

            CreateDepartureTaxiRouteCandidateData();
            CreateArrivalTaxiRouteCandidateData();
            ValidateTaxiwaySegmentRouteData();
        }

        private void CreateTaxiwaySegmentData()
        {
            taxiwaySegments.Clear();

            AddTaxiwaySegment(
                "A_MAIN_PARALLEL_01",
                "TWY A",
                "Provisional A parallel taxiway",
                TaxiwaySegmentType.Parallel,
                TaxiwayAxisType.NorthSouth,
                "A滑走路前の主誘導路として使う暫定parallel segment",
                true,
                new Vector3(-13f, 0.6f, -5f),
                new Vector3(26.2f, 0.6f, -5f));
            AddTaxiwaySegment(
                "A_CONNECTOR_18L_END_01",
                "TWY A0",
                "Provisional RWY 18L end connector",
                TaxiwaySegmentType.Connector,
                TaxiwayAxisType.EastWest,
                "既存A滑走路datasetに沿って追加した18L側端寄りの暫定connector。segmentIdをロジックキーにする",
                true,
                new Vector3(15.2f, 0.6f, -5f),
                new Vector3(15.2f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_CONNECTOR_18L_01",
                "TWY A1",
                "Provisional RWY 18L connector",
                TaxiwaySegmentType.Connector,
                TaxiwayAxisType.EastWest,
                "RWY 18L側holding pointへ接続する暫定connector",
                true,
                new Vector3(12f, 0.6f, -5f),
                new Vector3(12f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_CONNECTOR_MID_01",
                "TWY A5",
                "Provisional mid-field connector",
                TaxiwaySegmentType.Connector,
                TaxiwayAxisType.EastWest,
                "将来の中間接続に使うための暫定connector。通常arrival routeでは使わず、rapid exit候補を優先する",
                true,
                new Vector3(4.8f, 0.6f, -5f),
                new Vector3(4.8f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_RAPID_EXIT_18L_SIDE_01",
                "Rapid Exit 18L Side",
                "Provisional rapid exit toward 18L side",
                TaxiwaySegmentType.Exit,
                TaxiwayAxisType.Diagonal,
                "RWY 36R着陸後に前方18L側へ抜けるゲーム用の暫定高速脱出誘導路",
                true,
                new Vector3(5.2f, 0.6f, -5f),
                new Vector3(8f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_RAPID_EXIT_36R_SIDE_01",
                "Rapid Exit 36R Side",
                "Provisional rapid exit toward 36R side",
                TaxiwaySegmentType.Exit,
                TaxiwayAxisType.Diagonal,
                "RWY 18L着陸後に前方36R側へ抜けるゲーム用の暫定高速脱出誘導路",
                true,
                new Vector3(0.2f, 0.6f, -5f),
                new Vector3(-2.6f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_CONNECTOR_36R_01",
                "TWY A9",
                "Provisional RWY 36R connector",
                TaxiwaySegmentType.Connector,
                TaxiwayAxisType.EastWest,
                "RWY 36R側holding pointへ接続する暫定connector",
                true,
                new Vector3(-7f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, 0f));
            AddTaxiwaySegment(
                "A_CONNECTOR_36R_END_01",
                "TWY A10",
                "Provisional RWY 36R end connector",
                TaxiwaySegmentType.Connector,
                TaxiwayAxisType.EastWest,
                "既存A滑走路datasetに沿って追加した36R側端寄りの暫定connector。segmentIdをロジックキーにする",
                true,
                new Vector3(-10.8f, 0.6f, -5f),
                new Vector3(-10.8f, 0.6f, 0f));
            AddTaxiwaySegment(
                "APRON_FRONT_01",
                "APRON LINK",
                "Provisional passenger apron frontage taxiway",
                TaxiwaySegmentType.Apron,
                TaxiwayAxisType.ApronFront,
                "Legacy provisional apron-side connection。通常routeでは使わず、主平行誘導路はA_MAIN_PARALLEL_01に集約する",
                true,
                new Vector3(4f, 0.6f, -6.45f),
                new Vector3(26.2f, 0.6f, -6.2f));
            AddTaxiwaySegment(
                "STAND_ENTRY_01",
                "SPOT 01 ENTRY",
                "Provisional SPOT 01 stand entry",
                TaxiwaySegmentType.StandEntry,
                TaxiwayAxisType.ApronFront,
                "SPOT 01からA_MAIN_PARALLEL_01へ短く接続する暫定stand entry",
                true,
                new Vector3(6.3f, 0.6f, -8.35f),
                new Vector3(6.3f, 0.6f, -5f));
            AddTaxiwaySegment(
                "STAND_ENTRY_02",
                "SPOT 02 ENTRY",
                "Provisional SPOT 02 stand entry",
                TaxiwaySegmentType.StandEntry,
                TaxiwayAxisType.ApronFront,
                "SPOT 02からA_MAIN_PARALLEL_01へ短く接続する暫定stand entry",
                true,
                new Vector3(10.8f, 0.6f, -7.95f),
                new Vector3(10.8f, 0.6f, -5f));
            AddTaxiwaySegment(
                "STAND_ENTRY_03",
                "SPOT 03 ENTRY",
                "Provisional SPOT 03 stand entry",
                TaxiwaySegmentType.StandEntry,
                TaxiwayAxisType.ApronFront,
                "SPOT 03からA_MAIN_PARALLEL_01へ短く接続する暫定stand entry",
                true,
                new Vector3(17.1f, 0.6f, -8.15f),
                new Vector3(17.1f, 0.6f, -5f));
            AddTaxiwaySegment(
                "STAND_ENTRY_04",
                "SPOT 04 ENTRY",
                "Provisional SPOT 04 stand entry",
                TaxiwaySegmentType.StandEntry,
                TaxiwayAxisType.ApronFront,
                "SPOT 04からA_MAIN_PARALLEL_01へ短く接続する暫定stand entry",
                true,
                new Vector3(26.2f, 0.6f, -8.65f),
                new Vector3(26.2f, 0.6f, -5f));
            AddTaxiwaySegment(
                "B_MAIN_PARALLEL_01",
                "TWY B",
                "Provisional future B runway parallel taxiway",
                TaxiwaySegmentType.Parallel,
                TaxiwayAxisType.NorthSouth,
                "将来B滑走路運用を検討するための暫定segment。現Phaseでは運用しない",
                true,
                new Vector3(-11.6f, 0.6f, 7.6f),
                new Vector3(17.6f, 0.6f, 7.6f));
        }

        private void AddTaxiwaySegment(
            string segmentId,
            string displayName,
            string realWorldName,
            TaxiwaySegmentType segmentType,
            TaxiwayAxisType axisType,
            string description,
            bool isProvisional,
            params Vector3[] waypoints)
        {
            taxiwaySegments.Add(new TaxiwaySegment(
                segmentId,
                displayName,
                realWorldName,
                segmentType,
                axisType,
                waypoints,
                description,
                isProvisional));
        }

        private void CreateRunwayAccessData()
        {
            runwayAccessPoints.Clear();
            runwayAccessUsages.Clear();

            AddRunwayAccessPoint(
                "A_ACCESS_18L_END_01",
                "A Access 18L End",
                "Provisional A runway end access for 18L side",
                RunwayAccessPhysicalSide.Near18L,
                "A_CONNECTOR_18L_END_01",
                GetRunwaySideConnectorPoint("A_CONNECTOR_18L_END_01", new Vector3(15.2f, 0.6f, 0f)),
                "A滑走路18L側端寄りの物理接続地点。displayNameではなくaccessPointIdをロジックキーにする",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_NEAR_18L_01",
                "A Access Near 18L",
                "Provisional A runway access near 18L end",
                RunwayAccessPhysicalSide.Near18L,
                "A_CONNECTOR_18L_01",
                GetRunwaySideConnectorPoint("A_CONNECTOR_18L_01", new Vector3(12f, 0.6f, 0f)),
                "A滑走路18L側寄りの物理接続地点。displayNameではなくaccessPointIdをロジックキーにする",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_MID_01",
                "A Access Mid",
                "Provisional A runway mid access",
                RunwayAccessPhysicalSide.MidRunway,
                "A_CONNECTOR_MID_01",
                GetRunwaySideConnectorPoint("A_CONNECTOR_MID_01", new Vector3(4.8f, 0.6f, 0f)),
                "A滑走路中間付近の暫定接続地点。将来のrapid exit / intersection departure候補",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_RAPID_18L_SIDE_01",
                "A Rapid Exit 18L Side",
                "Provisional A runway rapid exit toward 18L side",
                RunwayAccessPhysicalSide.Near18L,
                "A_RAPID_EXIT_18L_SIDE_01",
                GetRunwaySideConnectorPoint("A_RAPID_EXIT_18L_SIDE_01", new Vector3(8f, 0.6f, 0f)),
                "RWY 36R着陸後に前方18L側で使うゲーム用高速脱出誘導路。displayNameではなくaccessPointIdをロジックキーにする",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_RAPID_36R_SIDE_01",
                "A Rapid Exit 36R Side",
                "Provisional A runway rapid exit toward 36R side",
                RunwayAccessPhysicalSide.Near36R,
                "A_RAPID_EXIT_36R_SIDE_01",
                GetRunwaySideConnectorPoint("A_RAPID_EXIT_36R_SIDE_01", new Vector3(-2.6f, 0.6f, 0f)),
                "RWY 18L着陸後に前方36R側で使うゲーム用高速脱出誘導路。displayNameではなくaccessPointIdをロジックキーにする",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_NEAR_36R_01",
                "A Access Near 36R",
                "Provisional A runway access near 36R end",
                RunwayAccessPhysicalSide.Near36R,
                "A_CONNECTOR_36R_01",
                GetRunwaySideConnectorPoint("A_CONNECTOR_36R_01", new Vector3(-7f, 0.6f, 0f)),
                "A滑走路36R側寄りの物理接続地点。displayNameではなくaccessPointIdをロジックキーにする",
                true);
            AddRunwayAccessPoint(
                "A_ACCESS_36R_END_01",
                "A Access 36R End",
                "Provisional A runway end access for 36R side",
                RunwayAccessPhysicalSide.Near36R,
                "A_CONNECTOR_36R_END_01",
                GetRunwaySideConnectorPoint("A_CONNECTOR_36R_END_01", new Vector3(-10.8f, 0.6f, 0f)),
                "A滑走路36R側端寄りの物理接続地点。displayNameではなくaccessPointIdをロジックキーにする",
                true);

            AddRunwayAccessUsage(
                "RWY36R_EXIT_END",
                "36R",
                "A_ACCESS_18L_END_01",
                RunwayAccessType.Exit,
                RunwayAccessPreferredFor.Arrival,
                0.98f,
                RunwayExitSpeedLevel.Low,
                true,
                "RWY 36R着陸後、進行方向前方の18L側端寄りで離脱する将来候補");
            AddRunwayAccessUsage(
                "RWY36R_EXIT_NEAR_18L",
                "36R",
                "A_ACCESS_NEAR_18L_01",
                RunwayAccessType.Exit,
                RunwayAccessPreferredFor.Arrival,
                0.92f,
                RunwayExitSpeedLevel.Low,
                true,
                "RWY 36R着陸後、進行方向前方の18L側寄りで離脱する暫定exit");
            AddRunwayAccessUsage(
                "RWY36R_EXIT_MID",
                "36R",
                "A_ACCESS_MID_01",
                RunwayAccessType.RapidExit,
                RunwayAccessPreferredFor.Arrival,
                0.55f,
                RunwayExitSpeedLevel.Medium,
                true,
                "RWY 36R着陸後の中間離脱候補。現Phaseではdefaultにはしない");
            AddRunwayAccessUsage(
                "RWY36R_RAPID_EXIT_18L_SIDE",
                "36R",
                "A_ACCESS_RAPID_18L_SIDE_01",
                RunwayAccessType.RapidExit,
                RunwayAccessPreferredFor.Arrival,
                0.72f,
                RunwayExitSpeedLevel.High,
                true,
                "RWY 36R着陸後、前方18L側へ斜めに抜けるゲーム用高速脱出候補");
            AddRunwayAccessUsage(
                "RWY18L_EXIT_NEAR_36R",
                "18L",
                "A_ACCESS_NEAR_36R_01",
                RunwayAccessType.Exit,
                RunwayAccessPreferredFor.Arrival,
                0.92f,
                RunwayExitSpeedLevel.Low,
                true,
                "RWY 18L着陸後、進行方向前方の36R側寄りで離脱する暫定exit");
            AddRunwayAccessUsage(
                "RWY18L_EXIT_MID",
                "18L",
                "A_ACCESS_MID_01",
                RunwayAccessType.RapidExit,
                RunwayAccessPreferredFor.Arrival,
                0.55f,
                RunwayExitSpeedLevel.Medium,
                true,
                "RWY 18L着陸後の中間離脱候補。現Phaseではdefaultにはしない");
            AddRunwayAccessUsage(
                "RWY18L_RAPID_EXIT_36R_SIDE",
                "18L",
                "A_ACCESS_RAPID_36R_SIDE_01",
                RunwayAccessType.RapidExit,
                RunwayAccessPreferredFor.Arrival,
                0.72f,
                RunwayExitSpeedLevel.High,
                true,
                "RWY 18L着陸後、前方36R側へ斜めに抜けるゲーム用高速脱出候補");
            AddRunwayAccessUsage(
                "RWY18L_EXIT_END",
                "18L",
                "A_ACCESS_36R_END_01",
                RunwayAccessType.Exit,
                RunwayAccessPreferredFor.Arrival,
                0.98f,
                RunwayExitSpeedLevel.Low,
                true,
                "RWY 18L着陸後、進行方向前方の36R側端寄りで離脱する将来候補");

            AddRunwayAccessUsage(
                "RWY36R_ENTRY_END",
                "36R",
                "A_ACCESS_36R_END_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 36R出発で36R側端寄りからline upする将来候補");
            AddRunwayAccessUsage(
                "RWY36R_ENTRY_NEAR_36R",
                "36R",
                "A_ACCESS_NEAR_36R_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0.02f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 36R出発で36R側からline upする暫定entry");
            AddRunwayAccessUsage(
                "RWY36R_ENTRY_MID",
                "36R",
                "A_ACCESS_MID_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0.5f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 36R出発で中央connectorからline upする暫定entry");
            AddRunwayAccessUsage(
                "RWY18L_ENTRY_END",
                "18L",
                "A_ACCESS_18L_END_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 18L出発で18L側端寄りからline upする将来候補");
            AddRunwayAccessUsage(
                "RWY18L_ENTRY_NEAR_18L",
                "18L",
                "A_ACCESS_NEAR_18L_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0.02f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 18L出発で18L側からline upする暫定entry");
            AddRunwayAccessUsage(
                "RWY18L_ENTRY_MID",
                "18L",
                "A_ACCESS_MID_01",
                RunwayAccessType.Entry,
                RunwayAccessPreferredFor.Departure,
                0.5f,
                RunwayExitSpeedLevel.NotApplicable,
                true,
                "RWY 18L出発で中央connectorからline upする暫定entry");
        }

        private void AddRunwayAccessPoint(
            string accessPointId,
            string displayName,
            string realWorldName,
            RunwayAccessPhysicalSide physicalSide,
            string connectedSegmentId,
            Vector3 position,
            string description,
            bool isProvisional)
        {
            runwayAccessPoints.Add(new RunwayAccessPoint(
                accessPointId,
                displayName,
                realWorldName,
                physicalSide,
                connectedSegmentId,
                position,
                description,
                isProvisional));
        }

        private void AddRunwayAccessUsage(
            string usageId,
            string runwayDesignator,
            string accessPointId,
            RunwayAccessType accessType,
            RunwayAccessPreferredFor preferredFor,
            float runwayPositionRatio,
            RunwayExitSpeedLevel maxExitSpeedLevel,
            bool isProvisional,
            string description)
        {
            runwayAccessUsages.Add(new RunwayAccessUsage(
                usageId,
                runwayDesignator,
                accessPointId,
                accessType,
                preferredFor,
                runwayPositionRatio,
                maxExitSpeedLevel,
                isProvisional,
                description));
        }

        private Vector3 GetRunwaySideConnectorPoint(string connectedSegmentId, Vector3 fallback)
        {
            var segment = GetTaxiwaySegment(connectedSegmentId);
            if (segment != null && segment.Waypoints.Count > 0)
            {
                return segment.Waypoints[segment.Waypoints.Count - 1];
            }

            return fallback;
        }

        private void CreateDepartureTaxiRouteCandidateData()
        {
            departureTaxiRouteCandidates.Clear();

            // Provisional route candidates: these are not a full ROAH taxiway reproduction.
            // Current commands use the default candidate; future UI can expose Route A / Route B choices.
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT01_18L_A",
                "Route A / East Link",
                "SPOT 01から東側接続誘導路経由でRWY 18L手前へ向かう暫定経路",
                "SPOT 01",
                "18L",
                true,
                new Vector3(6.3f, 0.6f, -5f),
                new Vector3(12f, 0.6f, -5f),
                GetHoldShortPosition("18L"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT01_36R_A",
                "Route A / Main West",
                "SPOT 01から主誘導路を西側へ進みRWY 36R手前へ向かう暫定経路",
                "SPOT 01",
                "36R",
                true,
                new Vector3(6.3f, 0.6f, -5f),
                new Vector3(0f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, -5f),
                GetHoldShortPosition("36R"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT02_18L_A",
                "Route A / East Link",
                "SPOT 02から東側接続誘導路経由でRWY 18L手前へ向かう暫定経路",
                "SPOT 02",
                "18L",
                true,
                new Vector3(10.8f, 0.6f, -5f),
                new Vector3(12f, 0.6f, -5f),
                GetHoldShortPosition("18L"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT02_36R_A",
                "Route A / Main West",
                "SPOT 02から主誘導路を西側へ進みRWY 36R手前へ向かう暫定経路",
                "SPOT 02",
                "36R",
                true,
                new Vector3(10.8f, 0.6f, -5f),
                new Vector3(4.8f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, -5f),
                GetHoldShortPosition("36R"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT02_36R_B",
                "Route B / Mid Entry",
                "SPOT 02から主誘導路を中央connectorへ進みRWY 36Rへ入る将来選択用の暫定経路",
                "SPOT 02",
                "36R",
                false,
                "RWY36R_ENTRY_MID",
                new Vector3(10.8f, 0.6f, -5f),
                new Vector3(4.8f, 0.6f, -5f),
                new Vector3(0f, 0.6f, -5f),
                new Vector3(0f, 0.6f, -3f));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT02_18L_END_A",
                "Route End / 18L",
                "SPOT 02から主誘導路を東側端connectorへ進みRWY 18Lへ入る将来選択用の暫定経路",
                "SPOT 02",
                "18L",
                false,
                "RWY18L_ENTRY_END",
                new Vector3(10.8f, 0.6f, -5f),
                new Vector3(15.2f, 0.6f, -5f),
                new Vector3(15.2f, 0.6f, -3f));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT02_36R_END_A",
                "Route End / 36R",
                "SPOT 02から主誘導路を西側端connectorへ進みRWY 36Rへ入る将来選択用の暫定経路",
                "SPOT 02",
                "36R",
                false,
                "RWY36R_ENTRY_END",
                new Vector3(10.8f, 0.6f, -5f),
                new Vector3(4.8f, 0.6f, -5f),
                new Vector3(-10.8f, 0.6f, -5f),
                new Vector3(-10.8f, 0.6f, -3f));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT03_18L_A",
                "Route A / East Link",
                "SPOT 03から東側接続誘導路経由でRWY 18L手前へ向かう暫定経路",
                "SPOT 03",
                "18L",
                true,
                new Vector3(17.1f, 0.6f, -5f),
                new Vector3(12f, 0.6f, -5f),
                GetHoldShortPosition("18L"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT03_36R_A",
                "Route A / Main West",
                "SPOT 03から主誘導路を西側へ進みRWY 36R手前へ向かう暫定経路",
                "SPOT 03",
                "36R",
                true,
                new Vector3(17.1f, 0.6f, -5f),
                new Vector3(4.8f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, -5f),
                GetHoldShortPosition("36R"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT04_18L_A",
                "Route A / East Link",
                "SPOT 04から東側接続誘導路経由でRWY 18L手前へ向かう暫定経路",
                "SPOT 04",
                "18L",
                true,
                new Vector3(26.2f, 0.6f, -5f),
                new Vector3(18f, 0.6f, -5f),
                new Vector3(12f, 0.6f, -5f),
                GetHoldShortPosition("18L"));
            AddDepartureTaxiRouteCandidate(
                "DEP_SPOT04_36R_A",
                "Route A / Main West",
                "SPOT 04から主誘導路を西側へ進みRWY 36R手前へ向かう暫定経路",
                "SPOT 04",
                "36R",
                true,
                new Vector3(26.2f, 0.6f, -5f),
                new Vector3(12f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, -5f),
                GetHoldShortPosition("36R"));
        }

        private void AddDepartureTaxiRouteCandidate(
            string routeId,
            string displayName,
            string description,
            string spotId,
            string runwayDesignator,
            bool isDefault,
            params Vector3[] waypoints)
        {
            AddDepartureTaxiRouteCandidate(
                routeId,
                displayName,
                description,
                spotId,
                runwayDesignator,
                isDefault,
                GetDefaultDepartureEntryUsageId(runwayDesignator),
                waypoints);
        }

        private void AddDepartureTaxiRouteCandidate(
            string routeId,
            string displayName,
            string description,
            string spotId,
            string runwayDesignator,
            bool isDefault,
            string runwayEntryUsageId,
            params Vector3[] waypoints)
        {
            var segmentIds = BuildDepartureTaxiSegmentIds(routeId, spotId, runwayDesignator, runwayEntryUsageId);
            departureTaxiRouteCandidates.Add(new TaxiRouteCandidate(
                routeId,
                displayName,
                description,
                spotId,
                runwayDesignator,
                waypoints,
                segmentIds,
                BuildRouteInstructionText(segmentIds),
                isDefault,
                runwayEntryUsageId,
                string.Empty));
        }

        private void CreateArrivalTaxiRouteCandidateData()
        {
            arrivalTaxiRouteCandidates.Clear();

            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT01_A", "Arrival Route A / 18L to SPOT 01", "18L", "SPOT 01", true);
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT01_RAPID_A", "Arrival Rapid Exit / 18L to SPOT 01", "18L", "SPOT 01", false, "RWY18L_RAPID_EXIT_36R_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT01_A", "Arrival Route A / 36R to SPOT 01", "36R", "SPOT 01", true);
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT01_RAPID_A", "Arrival Rapid Exit / 36R to SPOT 01", "36R", "SPOT 01", false, "RWY36R_RAPID_EXIT_18L_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT02_A", "Arrival Route A / 18L to SPOT 02", "18L", "SPOT 02", true);
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT02_RAPID_A", "Arrival Rapid Exit / 18L to SPOT 02", "18L", "SPOT 02", false, "RWY18L_RAPID_EXIT_36R_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT02_A", "Arrival Route A / 36R to SPOT 02", "36R", "SPOT 02", true);
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT02_RAPID_A", "Arrival Rapid Exit / 36R to SPOT 02", "36R", "SPOT 02", false, "RWY36R_RAPID_EXIT_18L_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT03_A", "Arrival Route A / 18L to SPOT 03", "18L", "SPOT 03", true);
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT03_RAPID_A", "Arrival Rapid Exit / 18L to SPOT 03", "18L", "SPOT 03", false, "RWY18L_RAPID_EXIT_36R_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT03_A", "Arrival Route A / 36R to SPOT 03", "36R", "SPOT 03", true);
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT03_RAPID_A", "Arrival Rapid Exit / 36R to SPOT 03", "36R", "SPOT 03", false, "RWY36R_RAPID_EXIT_18L_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT04_A", "Arrival Route A / 18L to SPOT 04", "18L", "SPOT 04", true);
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT04_RAPID_A", "Arrival Rapid Exit / 18L to SPOT 04", "18L", "SPOT 04", false, "RWY18L_RAPID_EXIT_36R_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT04_A", "Arrival Route A / 36R to SPOT 04", "36R", "SPOT 04", true);
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT04_RAPID_A", "Arrival Rapid Exit / 36R to SPOT 04", "36R", "SPOT 04", false, "RWY36R_RAPID_EXIT_18L_SIDE");
            AddArrivalTaxiRouteCandidate("ARR_18L_SPOT02_END_A", "Arrival Route End / 18L to SPOT 02", "18L", "SPOT 02", false, "RWY18L_EXIT_END");
            AddArrivalTaxiRouteCandidate("ARR_36R_SPOT02_END_A", "Arrival Route End / 36R to SPOT 02", "36R", "SPOT 02", false, "RWY36R_EXIT_END");
        }

        private void AddArrivalTaxiRouteCandidate(
            string routeId,
            string displayName,
            string runwayDesignator,
            string spotId,
            bool isDefault)
        {
            AddArrivalTaxiRouteCandidate(
                routeId,
                displayName,
                runwayDesignator,
                spotId,
                isDefault,
                GetDefaultArrivalExitUsageId(runwayDesignator));
        }

        private void AddArrivalTaxiRouteCandidate(
            string routeId,
            string displayName,
            string runwayDesignator,
            string spotId,
            bool isDefault,
            string runwayExitUsageId)
        {
            var segmentIds = BuildArrivalTaxiSegmentIds(spotId, runwayDesignator, runwayExitUsageId);
            arrivalTaxiRouteCandidates.Add(new TaxiRouteCandidate(
                routeId,
                displayName,
                $"{runwayDesignator}着陸後に{spotId}へ戻る暫定arrival taxi route",
                spotId,
                runwayDesignator,
                BuildArrivalTaxiWaypoints(spotId, runwayDesignator, runwayExitUsageId),
                segmentIds,
                BuildRouteInstructionText(segmentIds),
                isDefault,
                string.Empty,
                runwayExitUsageId));
        }

        private IEnumerable<string> BuildDepartureTaxiSegmentIds(string routeId, string spotId, string runwayDesignator, string runwayEntryUsageId)
        {
            var segmentIds = new List<string>();
            var standEntrySegmentId = GetStandEntrySegmentId(spotId);
            if (!string.IsNullOrEmpty(standEntrySegmentId))
            {
                segmentIds.Add(standEntrySegmentId);
            }

            segmentIds.Add("A_MAIN_PARALLEL_01");
            segmentIds.Add(GetConnectedSegmentIdForUsage(runwayEntryUsageId));

            return segmentIds;
        }

        private IEnumerable<string> BuildArrivalTaxiSegmentIds(string spotId, string runwayDesignator, string runwayExitUsageId)
        {
            var segmentIds = new List<string>();
            segmentIds.Add(GetConnectedSegmentIdForUsage(runwayExitUsageId));
            segmentIds.Add("A_MAIN_PARALLEL_01");

            var standEntrySegmentId = GetStandEntrySegmentId(spotId);
            if (!string.IsNullOrEmpty(standEntrySegmentId))
            {
                segmentIds.Add(standEntrySegmentId);
            }

            return segmentIds;
        }

        private IEnumerable<Vector3> BuildArrivalTaxiWaypoints(string spotId, string runwayDesignator, string runwayExitUsageId)
        {
            var destination = GetSpotPosition(spotId);
            var runwayExitPoint = GetRunwayPointForUsage(runwayExitUsageId);
            var connectorTaxiwayPoint = GetTaxiwayPointForUsage(runwayExitUsageId);
            var connectorX = connectorTaxiwayPoint.x;
            var apronEntryDirection = destination.x >= connectorX ? 1f : -1f;
            var apronEntryX = Mathf.Clamp(destination.x - apronEntryDirection * 2f, 4f, 26.2f);
            return new[]
            {
                runwayExitPoint,
                connectorTaxiwayPoint,
                new Vector3(apronEntryX, 0.6f, -5f),
                new Vector3(destination.x, 0.6f, -5f),
                destination
            };
        }

        private string GetDefaultDepartureEntryUsageId(string operationDirection)
        {
            return Is36R(operationDirection) ? "RWY36R_ENTRY_NEAR_36R" : "RWY18L_ENTRY_NEAR_18L";
        }

        private string GetDefaultArrivalExitUsageId(string operationDirection)
        {
            return Is36R(operationDirection) ? "RWY36R_EXIT_NEAR_18L" : "RWY18L_EXIT_NEAR_36R";
        }

        private string GetConnectedSegmentIdForUsage(string usageId)
        {
            var usage = GetRunwayAccessUsage(usageId);
            var accessPoint = usage != null ? GetRunwayAccessPoint(usage.AccessPointId) : null;
            return accessPoint != null ? accessPoint.ConnectedSegmentId : string.Empty;
        }

        private Vector3 GetTaxiwayPointForUsage(string usageId)
        {
            var connectedSegmentId = GetConnectedSegmentIdForUsage(usageId);
            var connectorSegment = GetTaxiwaySegment(connectedSegmentId);
            if (connectorSegment != null && connectorSegment.Waypoints.Count > 0)
            {
                return connectorSegment.Waypoints[0];
            }

            return usageId.Contains("36R") ? new Vector3(-7f, 0.6f, -5f) : new Vector3(12f, 0.6f, -5f);
        }

        private Vector3 GetRunwayPointForUsage(string usageId)
        {
            var accessPoint = GetAccessPointForUsage(usageId);
            return accessPoint != null ? accessPoint.Position : (usageId.Contains("36R") ? new Vector3(-7f, 0.6f, 0f) : new Vector3(12f, 0.6f, 0f));
        }

        private RunwayAccessPoint GetAccessPointForUsage(string usageId)
        {
            var usage = GetRunwayAccessUsage(usageId);
            return usage != null ? GetRunwayAccessPoint(usage.AccessPointId) : null;
        }

        private Vector3 GetSpotPosition(string spotId)
        {
            var normalizedSpotId = NormalizeSpotId(spotId);
            foreach (var spotDefinition in spotDefinitions)
            {
                if (NormalizeSpotId(spotDefinition.TutorialId) == normalizedSpotId)
                {
                    return spotDefinition.Position;
                }
            }

            switch (normalizedSpotId)
            {
                case "SPOT_01":
                    return new Vector3(6.3f, 0.6f, -8.35f);
                case "SPOT_02":
                    return new Vector3(10.8f, 0.6f, -7.95f);
                case "SPOT_03":
                    return new Vector3(17.1f, 0.6f, -8.15f);
                case "SPOT_04":
                    return new Vector3(26.2f, 0.6f, -8.65f);
                default:
                    return gatePositions.Count > 0 ? gatePositions[0] : new Vector3(6.3f, 0.6f, -8.35f);
            }
        }

        private string GetStandEntrySegmentId(string spotId)
        {
            switch (NormalizeSpotId(spotId))
            {
                case "SPOT_01":
                    return "STAND_ENTRY_01";
                case "SPOT_02":
                    return "STAND_ENTRY_02";
                case "SPOT_03":
                    return "STAND_ENTRY_03";
                case "SPOT_04":
                    return "STAND_ENTRY_04";
                default:
                    return string.Empty;
            }
        }

        private string BuildRouteInstructionText(IEnumerable<string> segmentIds)
        {
            var displayNames = new List<string>();
            foreach (var segmentId in segmentIds)
            {
                var segment = GetTaxiwaySegment(segmentId);
                displayNames.Add(segment != null ? segment.DisplayName : segmentId);
            }

            return displayNames.Count > 0 ? $"Taxi via {string.Join(", ", displayNames.ToArray())}" : string.Empty;
        }

        private void ValidateTaxiwaySegmentRouteData()
        {
            var segmentIds = new HashSet<string>();
            foreach (var segment in taxiwaySegments)
            {
                if (string.IsNullOrEmpty(segment.SegmentId))
                {
                    Debug.LogWarning("TaxiwaySegment has an empty segmentId.");
                    continue;
                }

                if (!segmentIds.Add(segment.SegmentId))
                {
                    Debug.LogWarning($"Duplicate TaxiwaySegment segmentId detected: {segment.SegmentId}");
                }
            }

            var accessPointIds = ValidateRunwayAccessPoints(segmentIds);
            var usageIds = ValidateRunwayAccessUsages(accessPointIds);
            var defaultDepartureCandidateKeys = ValidateTaxiRouteCandidates(
                departureTaxiRouteCandidates,
                segmentIds,
                usageIds,
                "departure");
            var defaultArrivalCandidateKeys = ValidateTaxiRouteCandidates(
                arrivalTaxiRouteCandidates,
                segmentIds,
                usageIds,
                "arrival");

            ValidateExpectedDefaultTaxiCandidates(defaultDepartureCandidateKeys, "departure");
            ValidateExpectedDefaultTaxiCandidates(defaultArrivalCandidateKeys, "arrival");
        }

        private HashSet<string> ValidateTaxiRouteCandidates(
            IEnumerable<TaxiRouteCandidate> candidates,
            HashSet<string> segmentIds,
            HashSet<string> usageIds,
            string routeKind)
        {
            var defaultCandidateKeys = new HashSet<string>();
            var candidateKeys = new HashSet<string>();
            var routeIds = new HashSet<string>();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrEmpty(candidate.RouteId))
                {
                    Debug.LogWarning($"{routeKind} TaxiRouteCandidate has an empty routeId.");
                }
                else if (!routeIds.Add(candidate.RouteId))
                {
                    Debug.LogWarning($"Duplicate {routeKind} TaxiRouteCandidate routeId detected: {candidate.RouteId}");
                }

                var candidateKey = $"{NormalizeSpotId(candidate.SpotId)}_{NormalizeRunwayDesignator(candidate.RunwayDesignator)}";
                candidateKeys.Add(candidateKey);
                if (candidate.IsDefault)
                {
                    defaultCandidateKeys.Add(candidateKey);
                }

                if (string.IsNullOrEmpty(candidate.RouteInstructionText))
                {
                    Debug.LogWarning($"{routeKind} TaxiRouteCandidate {candidate.RouteId} has an empty routeInstructionText.");
                }

                var candidateSegmentIds = new HashSet<string>();
                foreach (var segmentId in candidate.SegmentIds)
                {
                    if (!candidateSegmentIds.Add(segmentId))
                    {
                        Debug.LogWarning($"{routeKind} TaxiRouteCandidate {candidate.RouteId} contains duplicate segmentId: {segmentId}");
                    }

                    if (!segmentIds.Contains(segmentId))
                    {
                        Debug.LogWarning($"{routeKind} TaxiRouteCandidate {candidate.RouteId} references missing segmentId: {segmentId}");
                    }
                }

                if (routeKind == "departure")
                {
                    ValidateCandidateUsageReference(candidate.RouteId, "runwayEntryUsageId", candidate.RunwayEntryUsageId, usageIds);
                }
                else if (routeKind == "arrival")
                {
                    ValidateCandidateUsageReference(candidate.RouteId, "runwayExitUsageId", candidate.RunwayExitUsageId, usageIds);
                    if (candidate.IsDefault)
                    {
                        ValidateArrivalDefaultExit(candidate);
                    }
                }
            }

            foreach (var candidateKey in candidateKeys)
            {
                if (!defaultCandidateKeys.Contains(candidateKey))
                {
                    Debug.LogWarning($"No default {routeKind} TaxiRouteCandidate found for {candidateKey}.");
                }
            }

            return defaultCandidateKeys;
        }

        private HashSet<string> ValidateRunwayAccessPoints(HashSet<string> segmentIds)
        {
            var accessPointIds = new HashSet<string>();
            foreach (var accessPoint in runwayAccessPoints)
            {
                if (string.IsNullOrEmpty(accessPoint.AccessPointId))
                {
                    Debug.LogWarning("RunwayAccessPoint has an empty accessPointId.");
                    continue;
                }

                if (!accessPointIds.Add(accessPoint.AccessPointId))
                {
                    Debug.LogWarning($"Duplicate RunwayAccessPoint accessPointId detected: {accessPoint.AccessPointId}");
                }

                if (!segmentIds.Contains(accessPoint.ConnectedSegmentId))
                {
                    Debug.LogWarning($"RunwayAccessPoint {accessPoint.AccessPointId} references missing connectedSegmentId: {accessPoint.ConnectedSegmentId}");
                }
            }

            return accessPointIds;
        }

        private HashSet<string> ValidateRunwayAccessUsages(HashSet<string> accessPointIds)
        {
            var usageIds = new HashSet<string>();
            foreach (var usage in runwayAccessUsages)
            {
                if (string.IsNullOrEmpty(usage.UsageId))
                {
                    Debug.LogWarning("RunwayAccessUsage has an empty usageId.");
                    continue;
                }

                if (!usageIds.Add(usage.UsageId))
                {
                    Debug.LogWarning($"Duplicate RunwayAccessUsage usageId detected: {usage.UsageId}");
                }

                if (!accessPointIds.Contains(usage.AccessPointId))
                {
                    Debug.LogWarning($"RunwayAccessUsage {usage.UsageId} references missing accessPointId: {usage.AccessPointId}");
                }

                if (usage.RunwayPositionRatio < 0f || usage.RunwayPositionRatio > 1f)
                {
                    Debug.LogWarning($"RunwayAccessUsage {usage.UsageId} has invalid runwayPositionRatio: {usage.RunwayPositionRatio}");
                }
            }

            return usageIds;
        }

        private void ValidateCandidateUsageReference(string routeId, string fieldName, string usageId, HashSet<string> usageIds)
        {
            if (string.IsNullOrEmpty(usageId))
            {
                Debug.LogWarning($"TaxiRouteCandidate {routeId} has an empty {fieldName}.");
                return;
            }

            if (!usageIds.Contains(usageId))
            {
                Debug.LogWarning($"TaxiRouteCandidate {routeId} references missing {fieldName}: {usageId}");
            }
        }

        private void ValidateArrivalDefaultExit(TaxiRouteCandidate candidate)
        {
            var usage = GetRunwayAccessUsage(candidate.RunwayExitUsageId);
            if (usage == null)
            {
                return;
            }

            if (usage.PreferredFor != RunwayAccessPreferredFor.Arrival && usage.PreferredFor != RunwayAccessPreferredFor.Both)
            {
                Debug.LogWarning($"Arrival TaxiRouteCandidate {candidate.RouteId} uses non-arrival runwayExitUsageId: {candidate.RunwayExitUsageId}");
            }

            if (usage.AccessType != RunwayAccessType.Exit
                && usage.AccessType != RunwayAccessType.EntryExit
                && usage.AccessType != RunwayAccessType.RapidExit)
            {
                Debug.LogWarning($"Arrival TaxiRouteCandidate {candidate.RouteId} uses non-exit runwayExitUsageId: {candidate.RunwayExitUsageId}");
            }

            if (usage.RunwayPositionRatio < 0.4f)
            {
                Debug.LogWarning($"Arrival TaxiRouteCandidate {candidate.RouteId} default exit is behind the arrival direction: {candidate.RunwayExitUsageId} ratio {usage.RunwayPositionRatio}");
            }
        }

        private void ValidateExpectedDefaultTaxiCandidates(HashSet<string> defaultCandidateKeys, string routeKind)
        {
            var expectedSpotIds = new[] { "SPOT 01", "SPOT 02", "SPOT 03", "SPOT 04" };
            foreach (var spotId in expectedSpotIds)
            {
                foreach (var runwayDesignator in GetPrimaryRunwayDirectionOptions())
                {
                    var candidateKey = $"{NormalizeSpotId(spotId)}_{NormalizeRunwayDesignator(runwayDesignator)}";
                    if (!defaultCandidateKeys.Contains(candidateKey))
                    {
                        Debug.LogWarning($"Expected default {routeKind} TaxiRouteCandidate is missing for {spotId} RWY {runwayDesignator}.");
                    }
                }
            }
        }

        private void CreateEnvironment()
        {
            var root = new GameObject($"{airportDisplayName} Layout");
            root.transform.SetParent(transform);

            CreateBox("Main Airport Island Ground", new Vector3(3f, -0.1f, -8.2f), new Vector3(68f, 0.1f, 18.8f), groundMaterial, root.transform);
            CreateNahaStyleBlockout(root.transform);
            CreateRunway(root.transform);
            CreateTaxiways(root.transform);

            CreateSpotDefinitions();
            CreateSpotStands(root.transform);
            CreateTerminalBlockout(root.transform);
            CreateBox("Hold Short A", HoldShortPosition + Vector3.down * 0.5f, new Vector3(2.4f, 0.16f, 1.6f), holdShortMaterial, root.transform);
            CreateBox("Hold Short A Stop Bar", HoldShortPosition + new Vector3(0f, -0.39f, 0.62f), new Vector3(2.35f, 0.045f, 0.08f), runwayMarkingMaterial, root.transform);
            CreateBox("Hold Short A 36R", GetHoldShortPosition("36R") + Vector3.down * 0.5f, new Vector3(2.4f, 0.16f, 1.6f), holdShortMaterial, root.transform);
            CreateBox("Hold Short A 36R Stop Bar", GetHoldShortPosition("36R") + new Vector3(0f, -0.39f, 0.62f), new Vector3(2.35f, 0.045f, 0.08f), runwayMarkingMaterial, root.transform);

            var runwayObject = new GameObject("RunwayController A");
            runwayObject.transform.SetParent(transform);
            var runway = runwayObject.AddComponent<RunwayController>();
            var runwayGeometry = PrimaryRunwayGeometry;
            runway.Configure(
                "A",
                CreateMarker("Runway A 18L Threshold", ToGroundPoint(runwayGeometry.Endpoint18L), runwayObject.transform),
                CreateMarker("Runway A 36R Threshold", ToGroundPoint(runwayGeometry.Endpoint36R), runwayObject.transform),
                runwayGeometry);
            runways.Add(runway);
        }

        private void CreateSpotDefinitions()
        {
            spotDefinitions.Clear();
            gatePositions.Clear();

            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 01",
                "DOM 21-25 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(6.3f, 0.6f, -8.35f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 01"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 02",
                "DOM 23-27 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(10.8f, 0.6f, -7.95f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 02"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 03",
                "DOM 31-37 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(17.1f, 0.6f, -8.15f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 03"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 04",
                "INTL 41-46/51 candidate",
                AirportSpotArea.INTL,
                AircraftSizeClass.Widebody,
                true,
                new Vector3(26.2f, 0.6f, -8.65f),
                new Vector3(3.7f, 0.16f, 4.85f),
                "Boarding Bridge SPOT 04"));

            foreach (var spotDefinition in spotDefinitions)
            {
                gatePositions.Add(spotDefinition.Position);
            }
        }

        private void CreateSpotStands(Transform parent)
        {
            for (var index = 0; index < spotDefinitions.Count; index++)
            {
                var spotDefinition = spotDefinitions[index];
                CreateSpotStand(
                    $"Gate {index + 1} Stand",
                    spotDefinition.TutorialId,
                    spotDefinition.Position,
                    spotDefinition.StandScale,
                    parent);
            }
        }

        private void CreateNahaStyleBlockout(Transform parent)
        {
            CreateBox("East China Sea Inner Water West", new Vector3(-13f, -0.035f, 5.4f), new Vector3(31f, 0.04f, 7.2f), seaMaterial, parent);
            CreateBox("East China Sea Inner Water Center", new Vector3(7f, -0.035f, 5.9f), new Vector3(15f, 0.04f, 4.2f), seaMaterial, parent);
            CreateBox("East China Sea Inner Water East", new Vector3(25f, -0.035f, 5.4f), new Vector3(16f, 0.04f, 7.2f), seaMaterial, parent);
            CreateBox("East China Sea Offshore", new Vector3(3f, -0.035f, 17.2f), new Vector3(76f, 0.04f, 12.8f), seaMaterial, parent);

            CreateFlatPolygon(
                "Central Facility Wedge Shoreline",
                0.006f,
                shorelineMaterial,
                parent,
                new Vector2(-5.5f, 1.28f),
                new Vector2(12.6f, 1.28f),
                new Vector2(14.4f, 3.2f),
                new Vector2(11.5f, 6.9f),
                new Vector2(4.4f, 7.35f),
                new Vector2(-1.7f, 3.6f));
            CreateFlatPolygon(
                "Central Facility Wedge Island",
                0.035f,
                groundMaterial,
                parent,
                new Vector2(-4.6f, 1.5f),
                new Vector2(11.4f, 1.5f),
                new Vector2(13.2f, 3.2f),
                new Vector2(10.5f, 6.25f),
                new Vector2(4.6f, 6.75f),
                new Vector2(-0.8f, 3.35f));
            CreateBox("Right Runway Connector Island", new Vector3(17.1f, -0.03f, 5.1f), new Vector3(4.6f, 0.12f, 10.6f), groundMaterial, parent);
            CreateBox("Future Runway B Island", new Vector3(3f, -0.03f, 10.2f), new Vector3(36.5f, 0.12f, 5.4f), groundMaterial, parent);
            CreateBox("Left Service Causeway", new Vector3(-12.5f, -0.025f, 4.6f), new Vector3(0.65f, 0.08f, 5.4f), shorelineMaterial, parent);

            CreateBox("Shoreline Main Island North", new Vector3(3f, 0.005f, 1.35f), new Vector3(68f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Future Runway Island South", new Vector3(3f, 0.005f, 7.5f), new Vector3(36f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Future Runway Island North", new Vector3(3f, 0.005f, 12.9f), new Vector3(36f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Offshore", new Vector3(3f, 0.005f, 23.55f), new Vector3(76f, 0.025f, 0.12f), shorelineMaterial, parent);

            CreateBox("Future Runway B 18R 36L", new Vector3(3f, -0.02f, 10.2f), new Vector3(29.2f, 0.12f, 2.8f), futureRunwayMaterial, parent);
            CreateBox("Future Runway B Centerline", new Vector3(3f, 0.065f, 10.2f), new Vector3(24.2f, 0.035f, 0.06f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge North", new Vector3(3f, 0.06f, 11.45f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge South", new Vector3(3f, 0.06f, 8.95f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);

            CreateBox("Taxiway E Right Primary Link", new Vector3(17.1f, 0.025f, 5.1f), new Vector3(1.25f, 0.13f, 10.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway E Right Centerline", new Vector3(17.1f, 0.12f, 5.1f), new Vector3(0.055f, 0.035f, 9.4f), taxiwayMarkingMaterial, parent);
            CreateBox("Left Service Track", new Vector3(-12.5f, 0.01f, 4.6f), new Vector3(0.18f, 0.08f, 4.6f), secondaryApronMaterial, parent);
        }

        private void CreateTerminalBlockout(Transform parent)
        {
            CreateBox("Fighter Alert Base Apron", new Vector3(-13.5f, -0.02f, -9.7f), new Vector3(12.5f, 0.12f, 5.2f), militaryApronMaterial, parent);
            CreateBox("JASDF Support Transport Apron", new Vector3(-1.6f, -0.018f, -10f), new Vector3(11.2f, 0.12f, 5.9f), secondaryApronMaterial, parent);
            CreateBox("Passenger Apron Blockout", new Vector3(17.1f, -0.01f, -10.1f), new Vector3(29.2f, 0.12f, 9.35f), passengerApronMaterial, parent);
            CreateBox("DOM Apron Depth Reserve", new Vector3(12.2f, 0.035f, -8.2f), new Vector3(18.5f, 0.035f, 5.7f), secondaryApronMaterial, parent);
            CreateBox("INTL Widebody Apron Depth Reserve", new Vector3(26.2f, 0.04f, -8.5f), new Vector3(7.6f, 0.04f, 6.95f), secondaryApronMaterial, parent);
            CreateBox("Base Passenger Area Divider", new Vector3(2.7f, 0.04f, -10.2f), new Vector3(0.16f, 0.08f, 9.4f), areaDividerMaterial, parent);

            CreateBox("Fighter Shelter 01", new Vector3(-17.5f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Fighter Shelter 02", new Vector3(-14.6f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Fighter Shelter 03", new Vector3(-11.7f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Alert Support Building", new Vector3(-17.8f, 0.45f, -6.7f), new Vector3(2.6f, 0.9f, 1.4f), supportBuildingMaterial, parent);

            CreateBox("JASDF Transport Hangar West", new Vector3(-3.6f, 0.9f, -14.4f), new Vector3(4.4f, 1.8f, 2.5f), hangarMaterial, parent);
            CreateBox("JASDF Transport Hangar East", new Vector3(1.6f, 0.85f, -14.5f), new Vector3(4f, 1.7f, 2.4f), hangarMaterial, parent);
            CreateBox("Base Operations Blocks", new Vector3(-0.9f, 0.45f, -6.7f), new Vector3(5.8f, 0.9f, 1.4f), supportBuildingMaterial, parent);

            CreateBox("DOM TERMINAL Main Blockout", new Vector3(12.5f, 0.85f, -15.35f), new Vector3(18.2f, 1.7f, 2.35f), terminalMaterial, parent);
            CreateBox("INTL TERMINAL Main Blockout", new Vector3(25.7f, 0.92f, -15.35f), new Vector3(8.4f, 1.85f, 2.55f), terminalMaterial, parent);
            CreateBox("Passenger Terminal Concourse Joint", new Vector3(21.3f, 0.72f, -14.05f), new Vector3(1.9f, 1.45f, 1.35f), terminalMaterial, parent);
            CreateBox("DOM Finger Pier West Blockout", new Vector3(8.6f, 0.68f, -12.35f), new Vector3(1.55f, 1.35f, 5.25f), terminalMaterial, parent);
            CreateBox("DOM Finger Pier East Blockout", new Vector3(16.3f, 0.68f, -12.35f), new Vector3(1.55f, 1.35f, 5.25f), terminalMaterial, parent);
            CreateBox("INTL Widebody Pier Blockout", new Vector3(25.7f, 0.72f, -12.55f), new Vector3(5.9f, 1.45f, 2.05f), terminalMaterial, parent);
            CreateBox("DOM West Future Stand Reserve", new Vector3(4.4f, 0.045f, -9.25f), new Vector3(2.4f, 0.045f, 5.15f), secondaryApronMaterial, parent);
            CreateBox("DOM Center Pushback Reserve", new Vector3(12.5f, 0.047f, -9f), new Vector3(4.6f, 0.045f, 5.45f), secondaryApronMaterial, parent);
            CreateBox("DOM East Future Stand Reserve", new Vector3(19.4f, 0.045f, -9.15f), new Vector3(2.8f, 0.045f, 5.25f), secondaryApronMaterial, parent);
            CreateBox("INTL Widebody Future Stand Reserve", new Vector3(30.4f, 0.047f, -8.8f), new Vector3(3.6f, 0.045f, 6.5f), secondaryApronMaterial, parent);
            CreateBoardingBridge("Boarding Bridge SPOT 01", new Vector3(7.05f, 0.78f, -10.55f), new Vector3(0.22f, 0.2f, 1.95f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 02", new Vector3(10f, 0.78f, -10.45f), new Vector3(0.22f, 0.2f, 2.15f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 03", new Vector3(17.05f, 0.78f, -10.45f), new Vector3(0.22f, 0.2f, 2.15f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 04", new Vector3(26.2f, 0.82f, -10.35f), new Vector3(0.28f, 0.22f, 2.8f), parent);

            CreateBox("TWR Shaft Blockout", new Vector3(8.4f, 1.65f, 4.95f), new Vector3(0.85f, 3.3f, 0.85f), towerMaterial, parent);
            CreateBox("TWR Cab Blockout", new Vector3(8.4f, 3.55f, 4.95f), new Vector3(2f, 0.85f, 2f), towerMaterial, parent);
        }

        private void CreateRunway(Transform parent)
        {
            var runway = PrimaryRunwayGeometry;
            var runwayCenter = ToGroundPoint(runway.Center);
            var edgeOffset = runway.Width * 0.5f - 0.145f;
            var markingDirection = runway.DirectionForDesignatorA;
            CreateBox("Runway A", runwayCenter, new Vector3(runway.Length, 0.2f, runway.Width), runwayMaterial, parent);
            CreateBox("Runway A Edge North", runwayCenter + new Vector3(0f, 0.13f, edgeOffset), new Vector3(runway.Length - 0.6f, 0.04f, 0.08f), runwayEdgeMaterial, parent);
            CreateBox("Runway A Edge South", runwayCenter + new Vector3(0f, 0.13f, -edgeOffset), new Vector3(runway.Length - 0.6f, 0.04f, 0.08f), runwayEdgeMaterial, parent);
            CreateBox($"Runway A Threshold {runway.DesignatorAEnd}", ToGroundPoint(runway.DesignatorAThresholdPoint + markingDirection * 0.45f) + Vector3.up * 0.14f, new Vector3(0.28f, 0.05f, runway.Width - 0.63f), runwayMarkingMaterial, parent);
            CreateBox($"Runway A Threshold {runway.DesignatorBOppositeEnd}", ToGroundPoint(runway.DesignatorBThresholdPoint - markingDirection * 0.45f) + Vector3.up * 0.14f, new Vector3(0.28f, 0.05f, runway.Width - 0.63f), runwayMarkingMaterial, parent);

            for (var index = 0; index < 9; index++)
            {
                var centerlinePosition = runway.DesignatorAThresholdPoint + markingDirection * (3.85f + index * 3.1f);
                CreateBox($"Runway A Centerline {index + 1}", ToGroundPoint(centerlinePosition) + Vector3.up * 0.15f, new Vector3(1.15f, 0.045f, 0.08f), runwayMarkingMaterial, parent);
            }
        }

        private void CreateTaxiways(Transform parent)
        {
            CreateBox("Taxiway Main", new Vector3(6.6f, 0.02f, -5f), new Vector3(39.2f, 0.14f, 1.24f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link 18L End", new Vector3(15.2f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link 36R End", new Vector3(-10.8f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link West", new Vector3(-7f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link East", new Vector3(12f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("SPOT 01 Stand Link", new Vector3(6.3f, 0.025f, -6.68f), new Vector3(0.76f, 0.13f, 3.35f), taxiwayMaterial, parent);
            CreateBox("SPOT 02 Stand Link", new Vector3(10.8f, 0.025f, -6.48f), new Vector3(0.76f, 0.13f, 2.95f), taxiwayMaterial, parent);
            CreateBox("SPOT 03 Stand Link", new Vector3(17.1f, 0.025f, -6.58f), new Vector3(0.76f, 0.13f, 3.15f), taxiwayMaterial, parent);
            CreateBox("SPOT 04 Stand Link", new Vector3(26.2f, 0.025f, -6.83f), new Vector3(0.76f, 0.13f, 3.65f), taxiwayMaterial, parent);
            CreateBox("Rapid Exit 18L Side", new Vector3(6.6f, 0.028f, -2.5f), new Vector3(1.05f, 0.13f, 5.7f), taxiwayMaterial, Quaternion.Euler(0f, -29f, 0f), parent);
            CreateBox("Rapid Exit 36R Side", new Vector3(-1.2f, 0.028f, -2.5f), new Vector3(1.05f, 0.13f, 5.7f), taxiwayMaterial, Quaternion.Euler(0f, 29f, 0f), parent);
            CreateBox("Taxiway Main Centerline", new Vector3(6.6f, 0.12f, -5f), new Vector3(38.4f, 0.035f, 0.055f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway 18L End Centerline", new Vector3(15.2f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway 36R End Centerline", new Vector3(-10.8f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway West Centerline", new Vector3(-7f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway East Centerline", new Vector3(12f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("SPOT 01 Stand Link Centerline", new Vector3(6.3f, 0.12f, -6.68f), new Vector3(0.055f, 0.035f, 3.05f), taxiwayMarkingMaterial, parent);
            CreateBox("SPOT 02 Stand Link Centerline", new Vector3(10.8f, 0.12f, -6.48f), new Vector3(0.055f, 0.035f, 2.65f), taxiwayMarkingMaterial, parent);
            CreateBox("SPOT 03 Stand Link Centerline", new Vector3(17.1f, 0.12f, -6.58f), new Vector3(0.055f, 0.035f, 2.85f), taxiwayMarkingMaterial, parent);
            CreateBox("SPOT 04 Stand Link Centerline", new Vector3(26.2f, 0.12f, -6.83f), new Vector3(0.055f, 0.035f, 3.35f), taxiwayMarkingMaterial, parent);
            CreateBox("Rapid Exit 18L Side Centerline", new Vector3(6.6f, 0.12f, -2.5f), new Vector3(0.055f, 0.035f, 5.35f), taxiwayMarkingMaterial, Quaternion.Euler(0f, -29f, 0f), parent);
            CreateBox("Rapid Exit 36R Side Centerline", new Vector3(-1.2f, 0.12f, -2.5f), new Vector3(0.055f, 0.035f, 5.35f), taxiwayMarkingMaterial, Quaternion.Euler(0f, 29f, 0f), parent);
        }

        private void CreateSpotStand(string standName, string spotLabel, Vector3 spotPosition, Transform parent)
        {
            CreateSpotStand(standName, spotLabel, spotPosition, new Vector3(3.2f, 0.16f, 2.55f), parent);
        }

        private void CreateSpotStand(string standName, string spotLabel, Vector3 spotPosition, Vector3 standScale, Transform parent)
        {
            var basePosition = spotPosition + Vector3.down * 0.5f;
            var halfWidth = standScale.x * 0.5f;
            var halfDepth = standScale.z * 0.5f;
            CreateBox(standName, basePosition, standScale, gateMaterial, parent);
            CreateBox($"{spotLabel} Front Line", basePosition + new Vector3(0f, 0.12f, halfDepth - 0.22f), new Vector3(standScale.x - 0.45f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Rear Line", basePosition + new Vector3(0f, 0.12f, -halfDepth + 0.22f), new Vector3(standScale.x - 0.45f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Left Line", basePosition + new Vector3(-halfWidth + 0.2f, 0.12f, 0f), new Vector3(0.07f, 0.04f, standScale.z - 0.55f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Right Line", basePosition + new Vector3(halfWidth - 0.2f, 0.12f, 0f), new Vector3(0.07f, 0.04f, standScale.z - 0.55f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Stop Bar", basePosition + new Vector3(0f, 0.13f, halfDepth * 0.36f), new Vector3(standScale.x * 0.42f, 0.045f, 0.08f), runwayMarkingMaterial, parent);
        }

        private void CreateBoardingBridge(string bridgeName, Vector3 position, Vector3 scale, Transform parent)
        {
            CreateBox(bridgeName, position, scale, boardingBridgeMaterial, parent);
            CreateBox($"{bridgeName} Head", position + new Vector3(0f, -0.04f, scale.z * 0.5f), new Vector3(0.58f, 0.28f, 0.44f), boardingBridgeMaterial, parent);
        }

        private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            return CreateBox(objectName, position, scale, material, Quaternion.identity, parent);
        }

        private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material, Quaternion rotation, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.rotation = rotation;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private GameObject CreateFlatPolygon(string objectName, float y, Material material, Transform parent, params Vector2[] points)
        {
            var polygon = new GameObject(objectName);
            polygon.transform.SetParent(parent);

            var vertices = new Vector3[points.Length];
            for (var index = 0; index < points.Length; index++)
            {
                vertices[index] = new Vector3(points[index].x, y, points[index].y);
            }

            var triangles = new int[(points.Length - 2) * 3];
            var triangleIndex = 0;
            for (var index = 1; index < points.Length - 1; index++)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index;
            }

            var mesh = new Mesh
            {
                name = objectName,
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            polygon.AddComponent<MeshFilter>().sharedMesh = mesh;
            polygon.AddComponent<MeshRenderer>().sharedMaterial = material;
            return polygon;
        }

        private Transform CreateMarker(string markerName, Vector3 position, Transform parent)
        {
            var marker = new GameObject(markerName);
            marker.transform.SetParent(parent);
            marker.transform.position = position;
            return marker.transform;
        }

        private Vector3 ToGroundPoint(Vector3 movementPoint)
        {
            return new Vector3(movementPoint.x, 0f, movementPoint.z);
        }
    }
}
