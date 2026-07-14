using System;
using System.Collections.Generic;
using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Radio;
using UnityEngine;

namespace ATCJourneyJapan.Aircraft
{
    // Runtime controller for one aircraft: selection, label display, state transitions, and waypoint movement.
    [RequireComponent(typeof(Collider))]
    public class AircraftController : MonoBehaviour
    {
        private const float MinHeadingMovementSqrMagnitude = 0.0001f;

        [SerializeField] private string flightNumber = "ATC001";
        [SerializeField] private bool arrivalAircraft;
        [SerializeField] private AircraftState currentState = AircraftState.Waiting;
        [SerializeField] private float airborneSpeed = 8f;
        // Game-tuned speeds: intentionally slower and more exaggerated than real-time scale for readability.
        [SerializeField] private float pushbackSpeed = 0.35f;
        [SerializeField] private float taxiSpeed = 2.4f;
        [SerializeField] private float takeoffInitialSpeed = 2.2f;
        [SerializeField] private float takeoffMaxSpeed = 7.8f;
        [SerializeField] private float landingInitialSpeed = 8f;
        [SerializeField] private float landingRolloutEndSpeed = 2.3f;
        [SerializeField] private float takeoffAccelerationTime = 5f;
        [SerializeField] private float landingDecelerationTime = 6f;
        [SerializeField] private float landingDecelerationCompletionProgress = 0.75f;
        [SerializeField] private float routeHeadingTurnSpeed = 75f;
        [SerializeField] private float turnAngleThreshold = 22f;
        [SerializeField] private float taxiTurnSpeedMultiplier = 0.18f;
        [SerializeField] private KeyCode debugCycleDepartureRouteKey = KeyCode.R;
        [SerializeField] private KeyCode debugCycleArrivalRouteKey = KeyCode.T;
        [SerializeField] private string selectedDepartureRouteId = string.Empty;
        [SerializeField] private string selectedArrivalRouteId = string.Empty;
        [SerializeField] private float headingDegrees;

        private readonly SimpleRoute route = new SimpleRoute();
        private AirportManager airportManager;
        private GameManager gameManager;
        private TextMesh label;
        private Renderer[] aircraftRenderers;
        private Material normalMaterial;
        private Material selectedMaterial;
        private AircraftVisualSpec visualSpec;
        private AircraftData flightData;
        private bool selected;
        private bool tutorialHighlighted;
        private Vector2 facingDirection = Vector2.up;
        private AircraftState heldTaxiState = AircraftState.Waiting;
        private string lastLandingRunwayDesignator = string.Empty;
        private string confirmedDepartureRouteId = string.Empty;
        private string activeDepartureRouteId = string.Empty;
        private string activeRunwayEntryUsageId = string.Empty;
        private float takeoffSpeedProfileElapsed;
        private float landingSpeedProfileElapsed;
        private Vector3 landingRolloutStartPosition;
        private Vector3 landingRolloutEndPosition;
        private float landingRolloutDistance;
        private RunwayPerformanceProfile activeRunwayPerformanceProfile;
        private float activeTakeoffRollDistance;
        private float activeLandingRolloutDistance;

        // Provisional game-tuned runway performance values. These are not real aircraft performance data.
        private struct RunwayPerformanceProfile
        {
            public string ProfileName;
            public float TakeoffRollDistanceRatio;
            public float LandingRolloutDistanceRatio;
            public float TakeoffAccelerationTime;
            public float LandingDecelerationTime;
            public float TouchdownToTaxiSpeedRatio;
            public float RunwayOccupancyPadding;

            public RunwayPerformanceProfile(
                string profileName,
                float takeoffRollDistanceRatio,
                float landingRolloutDistanceRatio,
                float takeoffAccelerationTime,
                float landingDecelerationTime,
                float touchdownToTaxiSpeedRatio,
                float runwayOccupancyPadding)
            {
                ProfileName = profileName;
                TakeoffRollDistanceRatio = takeoffRollDistanceRatio;
                LandingRolloutDistanceRatio = landingRolloutDistanceRatio;
                TakeoffAccelerationTime = takeoffAccelerationTime;
                LandingDecelerationTime = landingDecelerationTime;
                TouchdownToTaxiSpeedRatio = touchdownToTaxiSpeedRatio;
                RunwayOccupancyPadding = runwayOccupancyPadding;
            }
        }

        public string FlightNumber => flightNumber;
        public AircraftData FlightData => flightData;
        public bool IsArrivalAircraft => arrivalAircraft;
        public AircraftState CurrentState => currentState;
        public float HeadingDegrees => headingDegrees;
        public Vector2 FacingDirection => facingDirection;
        public bool IsComplete => arrivalAircraft ? currentState == AircraftState.AtGate : currentState == AircraftState.AirborneDeparture;
        public bool IsOnRunway => currentState == AircraftState.FinalApproach
                                  || currentState == AircraftState.LandingRoll
                                  || currentState == AircraftState.LiningUp
                                  || currentState == AircraftState.TakeoffRoll;

        public void SetSelectedTaxiRoute(string purpose, string routeId)
        {
            if (purpose == "Arrival")
            {
                selectedArrivalRouteId = routeId ?? string.Empty;
            }
            else
            {
                selectedDepartureRouteId = routeId ?? string.Empty;
                confirmedDepartureRouteId = string.Empty;
                activeDepartureRouteId = string.Empty;
                activeRunwayEntryUsageId = string.Empty;
            }

            SyncSelectedRouteIds();
        }

        public void Configure(
            AircraftData data,
            bool isArrival,
            AircraftState startingState,
            AirportManager airport,
            GameManager manager,
            Material normal,
            Material selected,
            AircraftVisualSpec aircraftVisualSpec)
        {
            flightData = data;
            flightNumber = data.FlightId;
            arrivalAircraft = isArrival;
            selectedDepartureRouteId = data.SelectedDepartureRouteId;
            selectedArrivalRouteId = data.SelectedArrivalRouteId;
            confirmedDepartureRouteId = string.Empty;
            activeDepartureRouteId = string.Empty;
            activeRunwayEntryUsageId = string.Empty;
            airportManager = airport;
            gameManager = manager;
            normalMaterial = normal;
            selectedMaterial = selected;
            visualSpec = aircraftVisualSpec;

            aircraftRenderers = GetComponentsInChildren<Renderer>();
            SetState(startingState);
            SetSelected(false);
            CreateLabel();
            UpdateLabel();
        }

        private void Update()
        {
            if (gameManager == null || !gameManager.IsGameplayPaused)
            {
                var previousPosition = transform.position;
                UpdateActiveSpeedProfile(Time.deltaTime);
                var routeHeadingHandled = UpdateHeadingTowardRouteWaypoint(Time.deltaTime);
                route.Tick(transform, Time.deltaTime);
                if (ShouldLockPushbackHeading())
                {
                    SetDepartureParkingHeading();
                }
                else if (ShouldLockRunwayTakeoffHeading())
                {
                    SetRunwayTakeoffHeading();
                }
                else
                {
                    if (!routeHeadingHandled)
                    {
                        UpdateHeadingFromMovement(transform.position - previousPosition);
                    }
                }
            }

            HandleDebugRouteSwitchInput();
            UpdateLabel();
        }

        private void OnMouseDown()
        {
            SelectionManager.Instance?.SelectAircraft(this);
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            ApplyDisplayMaterial();
        }

        public void SetTutorialHighlighted(bool highlighted)
        {
            tutorialHighlighted = highlighted;
            ApplyDisplayMaterial();
        }

        public bool CanExecute(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return arrivalAircraft && currentState == AircraftState.Inbound;
                case AircraftCommand.TaxiToGate:
                    return arrivalAircraft && (currentState == AircraftState.Waiting || currentState == AircraftState.VacatingRunway);
                case AircraftCommand.Pushback:
                    return !arrivalAircraft && currentState == AircraftState.AtGate;
                case AircraftCommand.TaxiToHold:
                    return !arrivalAircraft && currentState == AircraftState.PushbackReady;
                case AircraftCommand.HoldTaxi:
                    return IsGroundTaxiState(currentState) && route.IsMoving;
                case AircraftCommand.ResumeTaxi:
                    return currentState == AircraftState.TaxiHeld;
                case AircraftCommand.HoldShort:
                    return !arrivalAircraft && currentState == AircraftState.HoldingPoint && HasBoundRunwayDirection();
                case AircraftCommand.LineUp:
                    return !arrivalAircraft && currentState == AircraftState.HoldingShort && HasBoundRunwayDirection();
                case AircraftCommand.ClearTakeoff:
                    return !arrivalAircraft && currentState == AircraftState.LiningUp && HasBoundRunwayDirection();
                case AircraftCommand.Stop:
                    return currentState != AircraftState.AtGate && currentState != AircraftState.AirborneDeparture;
                default:
                    return false;
            }
        }

        public void Execute(AircraftCommand command)
        {
            if (!CanExecute(command))
            {
                return;
            }

            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    ClearLanding();
                    break;
                case AircraftCommand.TaxiToGate:
                    TaxiToGate();
                    break;
                case AircraftCommand.Pushback:
                    Pushback();
                    break;
                case AircraftCommand.TaxiToHold:
                    TaxiToHold();
                    break;
                case AircraftCommand.HoldTaxi:
                    HoldTaxi();
                    break;
                case AircraftCommand.ResumeTaxi:
                    ResumeTaxi();
                    break;
                case AircraftCommand.HoldShort:
                    HoldShort();
                    break;
                case AircraftCommand.LineUp:
                    LineUp();
                    break;
                case AircraftCommand.ClearTakeoff:
                    ClearTakeoff();
                    break;
                case AircraftCommand.Stop:
                    StopAircraft();
                    break;
            }
        }

        public void BindRunwayDirection(RunwayData runway, string operationDirection)
        {
            if (flightData == null || runway == null)
            {
                return;
            }

            var resolvedDirection = !string.IsNullOrEmpty(operationDirection) ? operationDirection : runway.CurrentActiveDesignator;
            flightData.AssignRunwayDirection(runway.RunwayId, runway.SimpleNameJa, resolvedDirection);
            SyncFlightData();
        }

        private void ClearLanding()
        {
            var operationDirection = GetOperationRunwayDirection();
            lastLandingRunwayDesignator = operationDirection;
            transform.position = airportManager.GetArrivalFinalApproachStart(operationDirection);
            SetState(AircraftState.FinalApproach);
            SetRunwayLandingHeading(operationDirection);
            gameManager.OccupyPrimaryRunway(this, "着陸中");
            StartRouteWithHeading(airportManager.GetArrivalFinalRoute(operationDirection), airborneSpeed, () =>
            {
                SetState(AircraftState.LandingRoll);
                SetRunwayLandingHeading(operationDirection);
                gameManager.OccupyPrimaryRunway(this, "着陸滑走中");
                var performanceProfile = ResolveRunwayPerformanceProfile();
                var landingRollRoute = BuildLandingRolloutRouteForProfile(
                    operationDirection,
                    new List<Vector3>(airportManager.GetLandingRollRoute(operationDirection)),
                    performanceProfile);
                StartLandingSpeedProfile(operationDirection, landingRollRoute, performanceProfile);
                StartRouteWithHeading(landingRollRoute, landingInitialSpeed, () =>
                {
                    Debug.Log(
                        $"Landing rollout complete: {flightNumber} aircraftType={GetAircraftTypeLabel()} "
                        + $"profile={performanceProfile.ProfileName} runway={operationDirection} "
                        + $"rolloutDistance={activeLandingRolloutDistance:0.##} position={FormatVector3(transform.position)}");
                    SetState(AircraftState.VacatingRunway);
                    gameManager.OccupyPrimaryRunway(this, "滑走路離脱中");
                    StartRouteWithHeading(airportManager.GetVacateRunwayRoute(operationDirection), taxiSpeed, () =>
                    {
                        gameManager.ReleasePrimaryRunway(this);
                        SetState(AircraftState.Waiting);
                    });
                });
            });
        }

        private void TaxiToGate()
        {
            if (currentState == AircraftState.VacatingRunway)
            {
                gameManager.ReleasePrimaryRunway(this);
            }

            var operationDirection = GetLandingRunwayDirectionForTaxiToGate();
            var spotId = flightData != null ? flightData.SpotId : string.Empty;
            var routeCandidates = airportManager.GetArrivalTaxiRouteCandidates(spotId, operationDirection);
            var routeCandidate = SelectRouteCandidate(routeCandidates, selectedArrivalRouteId);
            selectedArrivalRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            SyncSelectedRouteIds();
            var routePoints = routeCandidate != null
                ? routeCandidate.Waypoints
                : airportManager.GetTaxiToAvailableGateRoute();
            LogSelectedArrivalTaxiRoute(routeCandidate, spotId, operationDirection);

            SetState(AircraftState.TaxiToGate);
            StartRouteWithHeading(routePoints, taxiSpeed, () =>
            {
                SetState(AircraftState.AtGate);
                gameManager.NotifyAircraftHandled(this);
            });
        }

        private void Pushback()
        {
            ConfirmDepartureRouteForPushback();
            SetState(AircraftState.Pushbacking, false);
            SetDepartureParkingHeading();
            StartRouteWithHeading(airportManager.GetPushbackRoute(transform.position), pushbackSpeed, () =>
            {
                SetState(AircraftState.PushbackReady);
            }, false);
        }

        private void TaxiToHold()
        {
            var operationDirection = GetOperationRunwayDirection();
            var spotId = flightData != null ? flightData.SpotId : string.Empty;
            var routeCandidates = airportManager.GetDepartureTaxiRouteCandidates(spotId, operationDirection);
            var routeCandidate = ResolveDepartureTaxiRoute(routeCandidates, out var fallbackUsed, out var fallbackReason);
            var routePoints = routeCandidate != null
                ? routeCandidate.Waypoints
                : airportManager.GetTaxiToHoldRoute(transform.position, operationDirection);
            selectedDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            confirmedDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            activeDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            activeRunwayEntryUsageId = routeCandidate != null ? routeCandidate.RunwayEntryUsageId : string.Empty;
            SyncSelectedRouteIds();
            LogSelectedTaxiRoute(routeCandidate, spotId, operationDirection, fallbackUsed, fallbackReason);

            SetState(AircraftState.TaxiToHold);
            StartRouteWithHeading(routePoints, taxiSpeed, () =>
            {
                SetState(AircraftState.HoldingPoint);
            });
        }

        private void ConfirmDepartureRouteForPushback()
        {
            if (flightData == null || airportManager == null)
            {
                return;
            }

            var operationDirection = GetOperationRunwayDirection();
            var spotId = flightData.SpotId;
            var routeCandidates = airportManager.GetDepartureTaxiRouteCandidates(spotId, operationDirection);
            var routeCandidate = SelectDepartureRouteCandidate(routeCandidates, selectedDepartureRouteId, out var fallbackUsed, out var fallbackReason);
            selectedDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            confirmedDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            activeDepartureRouteId = routeCandidate != null ? routeCandidate.RouteId : string.Empty;
            activeRunwayEntryUsageId = routeCandidate != null ? routeCandidate.RunwayEntryUsageId : string.Empty;
            SyncSelectedRouteIds();
            LogConfirmedDepartureRoute(routeCandidate, spotId, operationDirection, fallbackUsed, fallbackReason);
        }

        private void LogConfirmedDepartureRoute(TaxiRouteCandidate routeCandidate, string spotId, string runwayDesignator, bool fallbackUsed, string fallbackReason)
        {
            Debug.Log(
                $"Departure route confirmed: {flightNumber} purpose=DepartureRoute "
                + $"selectedDepartureRouteId={(routeCandidate != null ? routeCandidate.RouteId : "none")} "
                + $"confirmedDepartureRouteId={FormatUsageId(confirmedDepartureRouteId)} "
                + $"runwayEntryUsageId={(routeCandidate != null ? FormatUsageId(routeCandidate.RunwayEntryUsageId) : "none")} "
                + $"runway={runwayDesignator} spot={spotId} fallbackUsed={fallbackUsed} fallbackReason={fallbackReason} "
                + $"segments={(routeCandidate != null ? JoinSegmentIds(routeCandidate.SegmentIds) : "none")} "
                + $"firstWaypoint={(routeCandidate != null ? FormatRouteWaypoint(routeCandidate, true) : "none")} "
                + $"lastWaypoint={(routeCandidate != null ? FormatRouteWaypoint(routeCandidate, false) : "none")}");
        }

        private void LogSelectedTaxiRoute(TaxiRouteCandidate routeCandidate, string spotId, string runwayDesignator, bool fallbackUsed, string fallbackReason)
        {
            if (routeCandidate == null)
            {
                Debug.LogWarning(
                    $"Selected taxi route fallback: {flightNumber} purpose=Departure {spotId} RWY {runwayDesignator} "
                    + $"routeId=fallback selectedDepartureRouteId=none confirmedDepartureRouteId=none fallbackUsed={fallbackUsed} fallbackReason={fallbackReason} "
                    + "displayName=Direct Waypoint | routeInstructionText=none | segments: none "
                    + "runwayEntryUsageId=none runwayExitUsageId=none");
                return;
            }

            Debug.Log(
                $"Selected taxi route: {flightNumber} purpose=Departure {spotId} RWY {runwayDesignator} "
                + $"selectedDepartureRouteId={selectedDepartureRouteId} confirmedDepartureRouteId={FormatUsageId(confirmedDepartureRouteId)} "
                + $"routeId={routeCandidate.RouteId} fallbackUsed={fallbackUsed} fallbackReason={fallbackReason} {routeCandidate.DisplayName} | "
                + $"{routeCandidate.RouteInstructionText} | segments: {JoinSegmentIds(routeCandidate.SegmentIds)} "
                + $"runwayEntryUsageId={FormatUsageId(routeCandidate.RunwayEntryUsageId)} runwayExitUsageId={FormatUsageId(routeCandidate.RunwayExitUsageId)} "
                + $"firstWaypoint={FormatRouteWaypoint(routeCandidate, true)} lastWaypoint={FormatRouteWaypoint(routeCandidate, false)} "
                + $"activeRouteId={FormatUsageId(activeDepartureRouteId)} activeEntryUsageId={FormatUsageId(activeRunwayEntryUsageId)}");
        }

        private void LogSelectedArrivalTaxiRoute(TaxiRouteCandidate routeCandidate, string spotId, string runwayDesignator)
        {
            var activeRunwayDesignator = flightData != null && !string.IsNullOrEmpty(flightData.ActiveRunwayDesignator)
                ? flightData.ActiveRunwayDesignator
                : "none";
            if (routeCandidate == null)
            {
                Debug.LogWarning(
                    $"Selected arrival taxi route fallback: {flightNumber} purpose=Arrival ActiveRunway={activeRunwayDesignator} "
                    + $"RWY {runwayDesignator} to {spotId} current={FormatVector3(transform.position)} "
                    + "routeId=fallback displayName=Direct Waypoint | routeInstructionText=none | segments: none "
                    + "runwayEntryUsageId=none runwayExitUsageId=none");
                return;
            }

            var connectorSegmentId = GetConnectorSegmentId(routeCandidate);
            var connectorSegment = !string.IsNullOrEmpty(connectorSegmentId) && airportManager != null
                ? airportManager.GetTaxiwaySegment(connectorSegmentId)
                : null;
            var connectorFirst = connectorSegment != null && connectorSegment.Waypoints.Count > 0
                ? FormatVector3(connectorSegment.Waypoints[0])
                : "none";
            var connectorLast = connectorSegment != null && connectorSegment.Waypoints.Count > 0
                ? FormatVector3(connectorSegment.Waypoints[connectorSegment.Waypoints.Count - 1])
                : "none";

            Debug.Log(
                $"Selected arrival taxi route: {flightNumber} purpose=Arrival ActiveRunway={activeRunwayDesignator} "
                + $"RWY {runwayDesignator} to {spotId} {routeCandidate.RouteId} | "
                + $"connector={connectorSegmentId} first={connectorFirst} last={connectorLast} "
                + $"current={FormatVector3(transform.position)} | {routeCandidate.RouteInstructionText} | "
                + $"segments: {JoinSegmentIds(routeCandidate.SegmentIds)} "
                + $"runwayEntryUsageId={FormatUsageId(routeCandidate.RunwayEntryUsageId)} runwayExitUsageId={FormatUsageId(routeCandidate.RunwayExitUsageId)}");
        }

        private void LogDebugRouteSelection(string purpose, TaxiRouteCandidate routeCandidate, string spotId, string runwayDesignator, int candidateCount)
        {
            if (routeCandidate == null)
            {
                Debug.LogWarning($"Debug route switch: {flightNumber} purpose={purpose} {spotId} RWY {runwayDesignator} has no route candidates.");
                return;
            }

            Debug.Log(
                $"Debug route switch: {flightNumber} purpose={purpose} {spotId} RWY {runwayDesignator} "
                + $"candidateCount={candidateCount} routeId={routeCandidate.RouteId} displayName={routeCandidate.DisplayName} | "
                + $"{routeCandidate.RouteInstructionText} | segments: {JoinSegmentIds(routeCandidate.SegmentIds)} "
                + $"runwayEntryUsageId={FormatUsageId(routeCandidate.RunwayEntryUsageId)} runwayExitUsageId={FormatUsageId(routeCandidate.RunwayExitUsageId)}");
        }

        private void LogSelectedTakeoffRoute(string runwayDesignator)
        {
            var mode = HasRunwayEntryUsage() ? "Intersection" : "FullLengthFallback";
            var direction = GetRunwayTakeoffDirection(runwayDesignator);
            Debug.Log(
                $"Selected takeoff route: {flightNumber} RWY {runwayDesignator} "
                + $"routeId={(string.IsNullOrEmpty(activeDepartureRouteId) ? "fallback" : activeDepartureRouteId)} "
                + $"runwayEntryUsageId={(string.IsNullOrEmpty(activeRunwayEntryUsageId) ? "none" : activeRunwayEntryUsageId)} "
                + $"mode={mode} start={FormatVector3(transform.position)} direction={FormatVector3(direction)}");
        }

        private void LogTakeoffSpeedProfile(string runwayDesignator)
        {
            Debug.Log(
                $"Takeoff runway performance: {flightNumber} aircraftType={GetAircraftTypeLabel()} "
                + $"profile={activeRunwayPerformanceProfile.ProfileName} RWY {runwayDesignator} "
                + $"rollDistance={activeTakeoffRollDistance:0.##} "
                + $"initial={takeoffInitialSpeed:0.##} max={takeoffMaxSpeed:0.##} "
                + $"accelerationTime={GetActiveTakeoffAccelerationTime():0.##}");
        }

        private void LogLandingSpeedProfile(string runwayDesignator)
        {
            Debug.Log(
                $"Landing runway performance: {flightNumber} aircraftType={GetAircraftTypeLabel()} "
                + $"profile={activeRunwayPerformanceProfile.ProfileName} RWY {runwayDesignator} "
                + $"rolloutDistance={activeLandingRolloutDistance:0.##} "
                + $"initial={landingInitialSpeed:0.##} rolloutEnd={landingRolloutEndSpeed:0.##} "
                + $"taxiSpeedRatio={GetActiveTouchdownToTaxiSpeedRatio():0.##} "
                + $"decelerationTime={GetActiveLandingDecelerationTime():0.##}");
        }

        private string GetConnectorSegmentId(TaxiRouteCandidate routeCandidate)
        {
            foreach (var segmentId in routeCandidate.SegmentIds)
            {
                if (segmentId.Contains("CONNECTOR"))
                {
                    return segmentId;
                }
            }

            return string.Empty;
        }

        private string FormatVector3(Vector3 value)
        {
            return $"({value.x:0.##}, {value.y:0.##}, {value.z:0.##})";
        }

        private string FormatRouteWaypoint(TaxiRouteCandidate routeCandidate, bool first)
        {
            if (routeCandidate == null || routeCandidate.Waypoints.Count == 0)
            {
                return "none";
            }

            return FormatVector3(first ? routeCandidate.Waypoints[0] : routeCandidate.Waypoints[routeCandidate.Waypoints.Count - 1]);
        }

        private string JoinSegmentIds(IReadOnlyList<string> segmentIds)
        {
            var values = new List<string>();
            foreach (var segmentId in segmentIds)
            {
                values.Add(segmentId);
            }

            return values.Count > 0 ? string.Join(", ", values.ToArray()) : "none";
        }

        private string FormatUsageId(string usageId)
        {
            return string.IsNullOrEmpty(usageId) ? "none" : usageId;
        }

        private TaxiRouteCandidate SelectRouteCandidate(IReadOnlyList<TaxiRouteCandidate> routeCandidates, string selectedRouteId)
        {
            if (!string.IsNullOrEmpty(selectedRouteId))
            {
                foreach (var routeCandidate in routeCandidates)
                {
                    if (routeCandidate.RouteId == selectedRouteId)
                    {
                        return routeCandidate;
                    }
                }
            }

            return SelectDefaultTaxiRouteCandidate(routeCandidates);
        }

        private TaxiRouteCandidate SelectDepartureRouteCandidate(IReadOnlyList<TaxiRouteCandidate> routeCandidates, string selectedRouteId, out bool fallbackUsed, out string fallbackReason)
        {
            fallbackUsed = false;
            fallbackReason = "none";
            if (!string.IsNullOrEmpty(selectedRouteId))
            {
                foreach (var routeCandidate in routeCandidates)
                {
                    if (routeCandidate.RouteId == selectedRouteId)
                    {
                        return routeCandidate;
                    }
                }

                fallbackUsed = true;
                fallbackReason = "selectedRouteIdNotFoundForSpotRunway";
            }

            var defaultCandidate = SelectDefaultTaxiRouteCandidate(routeCandidates);
            if (string.IsNullOrEmpty(selectedRouteId))
            {
                fallbackUsed = true;
                fallbackReason = "selectedRouteIdEmpty";
            }

            if (defaultCandidate == null)
            {
                fallbackReason = "noCandidate";
            }

            return defaultCandidate;
        }

        private TaxiRouteCandidate ResolveDepartureTaxiRoute(IReadOnlyList<TaxiRouteCandidate> routeCandidates, out bool fallbackUsed, out string fallbackReason)
        {
            var preferredRouteId = !string.IsNullOrEmpty(confirmedDepartureRouteId)
                ? confirmedDepartureRouteId
                : !string.IsNullOrEmpty(activeDepartureRouteId)
                    ? activeDepartureRouteId
                    : selectedDepartureRouteId;
            return SelectDepartureRouteCandidate(routeCandidates, preferredRouteId, out fallbackUsed, out fallbackReason);
        }

        private TaxiRouteCandidate SelectDefaultTaxiRouteCandidate(IReadOnlyList<TaxiRouteCandidate> routeCandidates)
        {
            TaxiRouteCandidate fallbackCandidate = null;
            foreach (var routeCandidate in routeCandidates)
            {
                if (fallbackCandidate == null)
                {
                    fallbackCandidate = routeCandidate;
                }

                if (routeCandidate.IsDefault)
                {
                    return routeCandidate;
                }
            }

            return fallbackCandidate;
        }

        private void HandleDebugRouteSwitchInput()
        {
            if (!selected || airportManager == null || flightData == null)
            {
                return;
            }

            if (Input.GetKeyDown(debugCycleDepartureRouteKey))
            {
                CycleDebugRouteCandidate("Departure");
            }

            if (Input.GetKeyDown(debugCycleArrivalRouteKey))
            {
                CycleDebugRouteCandidate("Arrival");
            }
        }

        private void CycleDebugRouteCandidate(string purpose)
        {
            var runwayDesignator = purpose == "Arrival" ? GetLandingRunwayDirectionForTaxiToGate() : GetOperationRunwayDirection();
            var spotId = flightData.SpotId;
            var routeCandidates = purpose == "Arrival"
                ? airportManager.GetArrivalTaxiRouteCandidates(spotId, runwayDesignator)
                : airportManager.GetDepartureTaxiRouteCandidates(spotId, runwayDesignator);
            var currentRouteId = purpose == "Arrival" ? selectedArrivalRouteId : selectedDepartureRouteId;
            var nextCandidate = SelectNextRouteCandidate(routeCandidates, currentRouteId);

            if (purpose == "Arrival")
            {
                selectedArrivalRouteId = nextCandidate != null ? nextCandidate.RouteId : string.Empty;
            }
            else
            {
                selectedDepartureRouteId = nextCandidate != null ? nextCandidate.RouteId : string.Empty;
            }

            SyncSelectedRouteIds();
            LogDebugRouteSelection(purpose, nextCandidate, spotId, runwayDesignator, routeCandidates.Count);
        }

        private TaxiRouteCandidate SelectNextRouteCandidate(IReadOnlyList<TaxiRouteCandidate> routeCandidates, string currentRouteId)
        {
            if (routeCandidates.Count == 0)
            {
                return null;
            }

            if (routeCandidates.Count == 1)
            {
                return SelectDefaultTaxiRouteCandidate(routeCandidates);
            }

            for (var index = 0; index < routeCandidates.Count; index++)
            {
                if (routeCandidates[index].RouteId == currentRouteId)
                {
                    return routeCandidates[(index + 1) % routeCandidates.Count];
                }
            }

            return SelectDefaultTaxiRouteCandidate(routeCandidates);
        }

        private void SyncSelectedRouteIds()
        {
            if (flightData == null)
            {
                return;
            }

            flightData.SelectDepartureRoute(selectedDepartureRouteId);
            flightData.SelectArrivalRoute(selectedArrivalRouteId);
        }

        private void HoldTaxi()
        {
            heldTaxiState = currentState;
            route.Pause();
            SetState(AircraftState.TaxiHeld, false);
        }

        private void ResumeTaxi()
        {
            var resumeState = heldTaxiState == AircraftState.Waiting ? AircraftState.TaxiToHold : heldTaxiState;
            SetState(resumeState, false);
            SetHeadingTowardNextRouteWaypoint();
            route.Resume();
            heldTaxiState = AircraftState.Waiting;
            SyncFlightData();
        }

        private void HoldShort()
        {
            var operationDirection = GetOperationRunwayDirection();
            route.Stop();
            SetState(AircraftState.HoldingShort);
            transform.position = airportManager.GetHoldShortPosition(operationDirection, activeRunwayEntryUsageId);
            Debug.Log(
                $"Hold short position set: {flightNumber} runway={operationDirection} "
                + $"selectedDepartureRouteId={FormatUsageId(selectedDepartureRouteId)} confirmedDepartureRouteId={FormatUsageId(confirmedDepartureRouteId)} "
                + $"activeRunwayEntryUsageId={FormatUsageId(activeRunwayEntryUsageId)} position={FormatVector3(transform.position)}");
            SetHeadingFromWorldDirection(Vector3.forward);
        }

        private void LineUp()
        {
            var operationDirection = GetOperationRunwayDirection();
            SetState(AircraftState.LiningUp, false);
            gameManager.OccupyPrimaryRunway(this, "滑走路上待機");
            Debug.Log(
                $"Line up cleared: {flightNumber} runway={operationDirection} "
                + $"selectedDepartureRouteId={FormatUsageId(selectedDepartureRouteId)} confirmedDepartureRouteId={FormatUsageId(confirmedDepartureRouteId)} "
                + $"entryUsage={FormatUsageId(activeRunwayEntryUsageId)} routeId={FormatUsageId(activeDepartureRouteId)} start={FormatVector3(transform.position)}");
            StartRouteWithHeading(airportManager.GetLineUpRoute(transform.position, operationDirection, activeRunwayEntryUsageId), taxiSpeed, () =>
            {
                transform.position = GetLineupCompletionPoint(operationDirection);
                SetState(AircraftState.LiningUp, false);
                SetRunwayTakeoffHeading(operationDirection);
            });
        }

        private void ClearTakeoff()
        {
            var operationDirection = GetOperationRunwayDirection();
            var baseTakeoffRoute = HasRunwayEntryUsage()
                ? airportManager.GetTakeoffRoute(operationDirection, activeRunwayEntryUsageId, transform.position)
                : airportManager.GetTakeoffRoute(operationDirection);
            var performanceProfile = ResolveRunwayPerformanceProfile();
            var takeoffRoute = BuildTakeoffRouteForProfile(operationDirection, baseTakeoffRoute, performanceProfile);
            SetState(AircraftState.TakeoffRoll, false);
            SetRunwayTakeoffHeading(operationDirection);
            LogSelectedTakeoffRoute(operationDirection);
            Debug.Log(
                $"Takeoff cleared: {flightNumber} runway={operationDirection} "
                + $"selectedDepartureRouteId={FormatUsageId(selectedDepartureRouteId)} confirmedDepartureRouteId={FormatUsageId(confirmedDepartureRouteId)} "
                + $"entryUsage={FormatUsageId(activeRunwayEntryUsageId)} routeId={FormatUsageId(activeDepartureRouteId)}");
            StartTakeoffSpeedProfile(operationDirection, performanceProfile);
            gameManager.OccupyPrimaryRunway(this, "離陸滑走中");
            StartRouteWithHeading(takeoffRoute, takeoffInitialSpeed, () =>
            {
                Debug.Log(
                    $"Takeoff airborne: {flightNumber} aircraftType={GetAircraftTypeLabel()} "
                    + $"profile={performanceProfile.ProfileName} runway={operationDirection} "
                    + $"rollDistance={activeTakeoffRollDistance:0.##} position={FormatVector3(transform.position)}");
                gameManager.ReleasePrimaryRunway(this);
                SetState(AircraftState.AirborneDeparture, false);
                SetRunwayTakeoffHeading(operationDirection);
                gameManager.NotifyAircraftHandled(this);
            }, false);
        }

        private void StopAircraft()
        {
            route.Stop();
            SetState(AircraftState.Waiting, false);
        }

        private Vector3 GetLineupCompletionPoint(string operationDirection)
        {
            return HasRunwayEntryUsage()
                ? airportManager.GetIntersectionLineupPoint(operationDirection, activeRunwayEntryUsageId)
                : airportManager.GetPrimaryRunwayLineupPoint(operationDirection);
        }

        private bool HasRunwayEntryUsage()
        {
            return !string.IsNullOrEmpty(activeRunwayEntryUsageId);
        }

        private void StartTakeoffSpeedProfile(string operationDirection, RunwayPerformanceProfile performanceProfile)
        {
            activeRunwayPerformanceProfile = performanceProfile;
            takeoffSpeedProfileElapsed = 0f;
            route.Speed = takeoffInitialSpeed;
            LogTakeoffSpeedProfile(operationDirection);
        }

        private void StartLandingSpeedProfile(string operationDirection, IReadOnlyList<Vector3> landingRollRoute, RunwayPerformanceProfile performanceProfile)
        {
            activeRunwayPerformanceProfile = performanceProfile;
            landingSpeedProfileElapsed = 0f;
            landingRolloutStartPosition = transform.position;
            landingRolloutEndPosition = landingRollRoute.Count > 0 ? landingRollRoute[landingRollRoute.Count - 1] : transform.position;
            landingRolloutDistance = Vector3.Distance(landingRolloutStartPosition, landingRolloutEndPosition);
            activeLandingRolloutDistance = landingRolloutDistance;
            route.Speed = landingInitialSpeed;
            LogLandingSpeedProfile(operationDirection);
        }

        private void UpdateActiveSpeedProfile(float deltaTime)
        {
            if (currentState == AircraftState.TakeoffRoll)
            {
                takeoffSpeedProfileElapsed += deltaTime;
                route.Speed = InterpolateSpeed(takeoffInitialSpeed, takeoffMaxSpeed, takeoffSpeedProfileElapsed, GetActiveTakeoffAccelerationTime());
            }
            else if (currentState == AircraftState.LandingRoll)
            {
                landingSpeedProfileElapsed += deltaTime;
                route.Speed = InterpolateLandingRolloutSpeed();
            }
        }

        private float InterpolateLandingRolloutSpeed()
        {
            var touchdownToTaxiSpeedRatio = GetActiveTouchdownToTaxiSpeedRatio();
            if (landingRolloutDistance > 0.1f && touchdownToTaxiSpeedRatio > 0.01f)
            {
                var traveled = Vector3.Distance(landingRolloutStartPosition, transform.position);
                var rolloutProgress = Mathf.Clamp01(traveled / landingRolloutDistance);
                return Mathf.Lerp(
                    landingInitialSpeed,
                    landingRolloutEndSpeed,
                    Mathf.Clamp01(rolloutProgress / touchdownToTaxiSpeedRatio));
            }

            return InterpolateSpeed(landingInitialSpeed, landingRolloutEndSpeed, landingSpeedProfileElapsed, GetActiveLandingDecelerationTime());
        }

        private float InterpolateSpeed(float fromSpeed, float toSpeed, float elapsed, float duration)
        {
            if (duration <= 0.01f)
            {
                return toSpeed;
            }

            return Mathf.Lerp(fromSpeed, toSpeed, Mathf.Clamp01(elapsed / duration));
        }

        private RunwayPerformanceProfile ResolveRunwayPerformanceProfile()
        {
            var aircraftType = GetAircraftTypeLabel().ToUpperInvariant();
            if (aircraftType.Contains("B787") || aircraftType.Contains("787") || aircraftType.Contains("B777") || aircraftType.Contains("777"))
            {
                return new RunwayPerformanceProfile("heavy", 0.72f, 0.7f, 6.2f, 7.2f, 0.82f, 0.04f);
            }

            if (aircraftType.Contains("A320") || aircraftType.Contains("B737") || aircraftType.Contains("737"))
            {
                return new RunwayPerformanceProfile("medium", 0.5f, 0.52f, 4.8f, 5.8f, 0.7f, 0.03f);
            }

            return new RunwayPerformanceProfile("medium-fallback", 0.55f, 0.58f, 5f, 6f, 0.75f, 0.03f);
        }

        private List<Vector3> BuildTakeoffRouteForProfile(string operationDirection, IEnumerable<Vector3> baseRoutePoints, RunwayPerformanceProfile performanceProfile)
        {
            var baseRoute = new List<Vector3>();
            foreach (var point in baseRoutePoints)
            {
                baseRoute.Add(point);
            }

            var runway = airportManager != null ? airportManager.PrimaryRunwayGeometry : null;
            if (runway == null || baseRoute.Count == 0)
            {
                activeTakeoffRollDistance = 0f;
                return baseRoute;
            }

            var takeoffDirection = runway.GetTakeoffDirection(operationDirection);
            var runwayStart = baseRoute[0];
            var departureEnd = runway.GetDepartureEndPoint(operationDirection);
            var availableDistance = Vector3.Dot(departureEnd - runwayStart, takeoffDirection);
            if (availableDistance <= 0.1f)
            {
                availableDistance = Vector3.Distance(runwayStart, departureEnd);
            }

            var minimumRollDistance = Mathf.Min(runway.Length * 0.2f, Mathf.Max(availableDistance * 0.45f, 0.5f));
            var maximumRollDistance = Mathf.Max(minimumRollDistance, availableDistance * (1f - performanceProfile.RunwayOccupancyPadding));
            var targetRollDistance = Mathf.Clamp(runway.Length * performanceProfile.TakeoffRollDistanceRatio, minimumRollDistance, maximumRollDistance);
            activeTakeoffRollDistance = targetRollDistance;

            var rotationPoint = runwayStart + takeoffDirection * targetRollDistance;
            rotationPoint.y = runwayStart.y;

            var airborneDistance = Mathf.Min(targetRollDistance + runway.Length * 0.08f, availableDistance + runway.Length * 0.04f);
            var airbornePoint = runwayStart + takeoffDirection * airborneDistance;
            airbornePoint.y = 2.4f;

            return new List<Vector3>
            {
                runwayStart,
                rotationPoint,
                airbornePoint
            };
        }

        private List<Vector3> BuildLandingRolloutRouteForProfile(string operationDirection, IReadOnlyList<Vector3> baseRoute, RunwayPerformanceProfile performanceProfile)
        {
            var runway = airportManager != null ? airportManager.PrimaryRunwayGeometry : null;
            if (runway == null || baseRoute.Count == 0)
            {
                activeLandingRolloutDistance = 0f;
                return new List<Vector3>(baseRoute);
            }

            var landingDirection = runway.GetLandingDirection(operationDirection);
            var touchdownPoint = baseRoute[0];
            var fallbackRolloutEnd = baseRoute[baseRoute.Count - 1];
            var availableDistance = Vector3.Dot(fallbackRolloutEnd - touchdownPoint, landingDirection);
            if (availableDistance <= 0.1f)
            {
                availableDistance = Vector3.Distance(touchdownPoint, fallbackRolloutEnd);
            }

            var minimumRolloutDistance = Mathf.Min(runway.Length * 0.25f, Mathf.Max(availableDistance * 0.45f, 0.5f));
            var targetRolloutDistance = Mathf.Clamp(runway.Length * performanceProfile.LandingRolloutDistanceRatio, minimumRolloutDistance, availableDistance);
            activeLandingRolloutDistance = targetRolloutDistance;

            var rolloutEndPoint = touchdownPoint + landingDirection * targetRolloutDistance;
            rolloutEndPoint.y = touchdownPoint.y;

            return new List<Vector3>
            {
                touchdownPoint,
                rolloutEndPoint
            };
        }

        private float GetActiveTakeoffAccelerationTime()
        {
            return activeRunwayPerformanceProfile.TakeoffAccelerationTime > 0.01f
                ? activeRunwayPerformanceProfile.TakeoffAccelerationTime
                : takeoffAccelerationTime;
        }

        private float GetActiveLandingDecelerationTime()
        {
            return activeRunwayPerformanceProfile.LandingDecelerationTime > 0.01f
                ? activeRunwayPerformanceProfile.LandingDecelerationTime
                : landingDecelerationTime;
        }

        private float GetActiveTouchdownToTaxiSpeedRatio()
        {
            return activeRunwayPerformanceProfile.TouchdownToTaxiSpeedRatio > 0.01f
                ? activeRunwayPerformanceProfile.TouchdownToTaxiSpeedRatio
                : landingDecelerationCompletionProgress;
        }

        private string GetAircraftTypeLabel()
        {
            return flightData != null && !string.IsNullOrEmpty(flightData.AircraftType) ? flightData.AircraftType : "Unknown";
        }

        private void SetState(AircraftState state, bool applyDefaultHeading = true)
        {
            currentState = state;
            if (applyDefaultHeading)
            {
                ApplyDefaultHeadingForState(state);
            }
            SyncFlightData();
        }

        private void StartRouteWithHeading(IEnumerable<Vector3> routePoints, float speed, Action completed = null, bool applyInitialWaypointHeading = true)
        {
            var points = new List<Vector3>();
            foreach (var point in routePoints)
            {
                points.Add(point);
            }

            if (applyInitialWaypointHeading && points.Count > 0)
            {
                SetHeadingToward(points[0]);
            }

            route.StartRoute(points, speed, completed);
        }

        private void UpdateHeadingFromMovement(Vector3 movement)
        {
            movement.y = 0f;
            if (movement.sqrMagnitude <= MinHeadingMovementSqrMagnitude)
            {
                return;
            }

            SetHeadingFromWorldDirection(movement);
        }

        private bool UpdateHeadingTowardRouteWaypoint(float deltaTime)
        {
            if (!ShouldFollowRouteHeading() || !route.TryPeekNextWaypoint(out var nextWaypoint))
            {
                return false;
            }

            var direction = nextWaypoint - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= MinHeadingMovementSqrMagnitude)
            {
                return false;
            }

            SetHeadingFromWorldDirection(direction, true, deltaTime);
            return true;
        }

        private void SetHeadingTowardNextRouteWaypoint()
        {
            if (route.TryPeekNextWaypoint(out var nextWaypoint))
            {
                SetHeadingToward(nextWaypoint);
            }
        }

        private void SetHeadingToward(Vector3 targetPosition)
        {
            SetHeadingFromWorldDirection(targetPosition - transform.position);
        }

        private bool ShouldLockRunwayTakeoffHeading()
        {
            return currentState == AircraftState.TakeoffRoll
                   || currentState == AircraftState.AirborneDeparture;
        }

        private bool ShouldLockPushbackHeading()
        {
            return currentState == AircraftState.Pushbacking;
        }

        private bool ShouldFollowRouteHeading()
        {
            return route.IsMoving
                   && !ShouldLockPushbackHeading()
                   && !ShouldLockRunwayTakeoffHeading();
        }

        private void SetRunwayTakeoffHeading()
        {
            SetHeadingFromWorldDirection(GetRunwayTakeoffDirection());
        }

        private void SetRunwayTakeoffHeading(string operationDirection)
        {
            SetHeadingFromWorldDirection(GetRunwayTakeoffDirection(operationDirection));
        }

        private void SetRunwayLandingHeading()
        {
            SetHeadingFromWorldDirection(GetRunwayLandingDirection());
        }

        private void SetRunwayLandingHeading(string operationDirection)
        {
            SetHeadingFromWorldDirection(GetRunwayLandingDirection(operationDirection));
        }

        private void SetDepartureParkingHeading()
        {
            SetHeadingFromWorldDirection(Vector3.back);
        }

        private Vector3 GetRunwayTakeoffDirection()
        {
            return GetRunwayTakeoffDirection(GetOperationRunwayDirection());
        }

        private Vector3 GetRunwayTakeoffDirection(string operationDirection)
        {
            return airportManager != null ? airportManager.GetPrimaryRunwayTakeoffDirection(operationDirection) : Vector3.right;
        }

        private Vector3 GetRunwayLandingDirection()
        {
            return GetRunwayLandingDirection(GetOperationRunwayDirection());
        }

        private Vector3 GetRunwayLandingDirection(string operationDirection)
        {
            return airportManager != null ? airportManager.GetPrimaryRunwayLandingDirection(operationDirection) : Vector3.right;
        }

        private string GetOperationRunwayDirection()
        {
            return flightData != null && !string.IsNullOrEmpty(flightData.ActiveRunwayDesignator) ? flightData.ActiveRunwayDesignator : "18L";
        }

        private string GetLandingRunwayDirectionForTaxiToGate()
        {
            return !string.IsNullOrEmpty(lastLandingRunwayDesignator) ? lastLandingRunwayDesignator : GetOperationRunwayDirection();
        }

        private bool HasBoundRunwayDirection()
        {
            return flightData != null && flightData.HasActiveRunwayDesignator;
        }

        private void SetHeadingFromWorldDirection(Vector3 worldDirection)
        {
            SetHeadingFromWorldDirection(worldDirection, false, 0f);
        }

        private void SetHeadingFromWorldDirection(Vector3 worldDirection, bool smooth, float deltaTime)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var normalized = worldDirection.normalized;
            var targetRotation = Quaternion.LookRotation(normalized, Vector3.up);
            var angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
            facingDirection = new Vector2(normalized.x, normalized.z);
            headingDegrees = -Mathf.Atan2(facingDirection.x, facingDirection.y) * Mathf.Rad2Deg;
            transform.rotation = smooth
                ? Quaternion.RotateTowards(transform.rotation, targetRotation, routeHeadingTurnSpeed * deltaTime)
                : targetRotation;
            UpdateTaxiTurnSpeed(angleToTarget);
            SyncFlightData();
        }

        private void UpdateTaxiTurnSpeed(float angleToTarget)
        {
            if (!ShouldApplyTaxiTurnSpeed())
            {
                return;
            }

            route.Speed = angleToTarget >= turnAngleThreshold ? taxiSpeed * taxiTurnSpeedMultiplier : taxiSpeed;
        }

        private bool ShouldApplyTaxiTurnSpeed()
        {
            return route.IsMoving
                   && (currentState == AircraftState.TaxiToGate
                       || currentState == AircraftState.TaxiToHold
                       || currentState == AircraftState.VacatingRunway
                       || currentState == AircraftState.LiningUp);
        }

        private void ApplyDefaultHeadingForState(AircraftState state)
        {
            switch (state)
            {
                case AircraftState.Inbound:
                case AircraftState.FinalApproach:
                case AircraftState.LandingRoll:
                    SetRunwayLandingHeading();
                    break;
                case AircraftState.LiningUp:
                case AircraftState.TakeoffRoll:
                case AircraftState.AirborneDeparture:
                    SetRunwayTakeoffHeading();
                    break;
                case AircraftState.VacatingRunway:
                case AircraftState.TaxiToGate:
                    SetHeadingFromWorldDirection(Vector3.back);
                    break;
                case AircraftState.Pushbacking:
                    SetDepartureParkingHeading();
                    break;
                case AircraftState.AtGate:
                    SetHeadingFromWorldDirection(Vector3.back);
                    break;
                case AircraftState.PushbackReady:
                case AircraftState.TaxiToHold:
                case AircraftState.TaxiHeld:
                    SetHeadingFromWorldDirection(Vector3.right);
                    break;
                case AircraftState.HoldingPoint:
                case AircraftState.HoldingShort:
                    SetHeadingFromWorldDirection(Vector3.forward);
                    break;
            }
        }

        private void SyncFlightData()
        {
            if (flightData == null)
            {
                return;
            }

            flightData.UpdateRuntimeState(
                GetDataStateLabel(),
                GetControllerPositionLabel(),
                GetRecommendedCommandId(),
                GetNextTargetType(),
                GetNextTargetId(),
                GetNextTargetDisplayName(),
                headingDegrees,
                GetFacingDirectionLabel());
        }

        private string GetFacingDirectionLabel()
        {
            if (Mathf.Abs(facingDirection.x) >= Mathf.Abs(facingDirection.y))
            {
                return facingDirection.x >= 0f ? "East" : "West";
            }

            return facingDirection.y >= 0f ? "North" : "South";
        }

        private string GetDataStateLabel()
        {
            switch (currentState)
            {
                case AircraftState.Inbound:
                    return "着陸許可待ち";
                case AircraftState.FinalApproach:
                    return "着陸中";
                case AircraftState.LandingRoll:
                    return "着陸滑走中";
                case AircraftState.VacatingRunway:
                    return "滑走路離脱中";
                case AircraftState.TaxiToGate:
                    return "スポットへ地上走行中";
                case AircraftState.AtGate:
                    return arrivalAircraft ? "スポット到着" : "出発準備";
                case AircraftState.Pushbacking:
                    return "プッシュバック中";
                case AircraftState.PushbackReady:
                    return "地上走行待ち";
                case AircraftState.TaxiToHold:
                    return "滑走路手前へ地上走行中";
                case AircraftState.TaxiHeld:
                    return "現在位置で待機中";
                case AircraftState.HoldingPoint:
                    return "滑走路手前到着";
                case AircraftState.HoldingShort:
                    return "滑走路手前待機";
                case AircraftState.LiningUp:
                    return "滑走路上待機";
                case AircraftState.TakeoffRoll:
                    return "離陸中";
                case AircraftState.AirborneDeparture:
                    return "離陸済";
                case AircraftState.Waiting:
                    return "次の指示待ち";
                default:
                    return currentState.ToString();
            }
        }

        private string GetControllerPositionLabel()
        {
            if (arrivalAircraft)
            {
                return "Tower";
            }

            switch (currentState)
            {
                case AircraftState.AtGate:
                case AircraftState.Pushbacking:
                case AircraftState.PushbackReady:
                case AircraftState.TaxiToHold:
                case AircraftState.TaxiHeld:
                case AircraftState.HoldingPoint:
                case AircraftState.HoldingShort:
                    return "Ground";
                default:
                    return "Tower";
            }
        }

        private string GetRecommendedCommandId()
        {
            var recommended = GetRecommendedCommandForData();
            if (!recommended.HasValue)
            {
                return string.Empty;
            }

            var phrase = CommandPhraseCatalog.Get(recommended.Value);
            return phrase != null ? phrase.CommandId : string.Empty;
        }

        private AircraftCommand? GetRecommendedCommandForData()
        {
            var recommendedCommands = new[]
            {
                AircraftCommand.Pushback,
                AircraftCommand.ClearLanding,
                AircraftCommand.TaxiToGate,
                AircraftCommand.TaxiToHold,
                AircraftCommand.HoldTaxi,
                AircraftCommand.ResumeTaxi,
                AircraftCommand.HoldShort,
                AircraftCommand.LineUp,
                AircraftCommand.ClearTakeoff
            };

            foreach (var command in recommendedCommands)
            {
                if (CanExecute(command))
                {
                    return command;
                }
            }

            return null;
        }

        private string GetNextTargetType()
        {
            var targetState = currentState == AircraftState.TaxiHeld ? heldTaxiState : currentState;
            if (arrivalAircraft)
            {
                switch (targetState)
                {
                    case AircraftState.Inbound:
                    case AircraftState.FinalApproach:
                    case AircraftState.LandingRoll:
                        return "Runway";
                    case AircraftState.VacatingRunway:
                    case AircraftState.Waiting:
                    case AircraftState.TaxiToGate:
                    case AircraftState.AtGate:
                        return "Spot";
                    default:
                        return string.Empty;
                }
            }

            switch (targetState)
            {
                case AircraftState.AtGate:
                case AircraftState.Pushbacking:
                    return "Pushback";
                case AircraftState.PushbackReady:
                case AircraftState.TaxiToHold:
                case AircraftState.LiningUp:
                case AircraftState.TakeoffRoll:
                    return "Runway";
                case AircraftState.HoldingPoint:
                case AircraftState.HoldingShort:
                    return "HoldingPoint";
                default:
                    return string.Empty;
            }
        }

        private bool IsGroundTaxiState(AircraftState state)
        {
            return state == AircraftState.Pushbacking
                || state == AircraftState.TaxiToHold
                || state == AircraftState.TaxiToGate;
        }

        private string GetNextTargetId()
        {
            switch (GetNextTargetType())
            {
                case "Runway":
                    return flightData.HasActiveRunwayDesignator ? $"RWY_{flightData.ActiveRunwayDesignator}" : "RWY_UNASSIGNED";
                case "Spot":
                    return flightData.SpotId;
                case "HoldingPoint":
                    return "HOLD_SHORT_A";
                case "Pushback":
                    return "PUSHBACK";
                default:
                    return string.Empty;
            }
        }

        private string GetNextTargetDisplayName()
        {
            switch (GetNextTargetType())
            {
                case "Runway":
                    return flightData.RunwayShortDisplay;
                case "Spot":
                    return flightData.SpotDisplayName;
                case "HoldingPoint":
                    return "HOLD A";
                case "Pushback":
                    return "PUSHBACK";
                default:
                    return string.Empty;
            }
        }

        private void CreateLabel()
        {
            var labelObject = new GameObject("Flight Label");
            labelObject.transform.SetParent(transform);
            var labelHeight = visualSpec != null ? visualSpec.LabelHeight : 1.35f;
            labelObject.transform.localPosition = new Vector3(0f, labelHeight, 0.35f);
            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.145f;
            label.fontSize = 28;
            label.color = Color.white;
        }

        private void ApplyDisplayMaterial()
        {
            if (aircraftRenderers == null)
            {
                return;
            }

            var displayMaterial = selected || tutorialHighlighted ? selectedMaterial : normalMaterial;
            foreach (var renderer in aircraftRenderers)
            {
                if (renderer != null)
                {
                    renderer.sharedMaterial = displayMaterial;
                }
            }
        }

        private void UpdateLabel()
        {
            if (label == null)
            {
                return;
            }

            label.gameObject.SetActive(gameManager != null && gameManager.IsTrainingStarted);
            label.text = GetLabelText();
            var cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null)
            {
                label.transform.rotation = cameraTransform.rotation;
            }
        }

        private string GetLabelText()
        {
            if (flightData == null)
            {
                return flightNumber;
            }

            var target = string.IsNullOrEmpty(flightData.NextTargetDisplayName)
                ? string.Empty
                : $"\n{flightData.NextTargetDisplayName}";
            return $"{flightData.FlightId}\n{flightData.AircraftType}{target}";
        }
    }
}
