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
        [SerializeField] private string flightNumber = "ATC001";
        [SerializeField] private bool arrivalAircraft;
        [SerializeField] private AircraftState currentState = AircraftState.Waiting;
        [SerializeField] private float airborneSpeed = 8f;
        [SerializeField] private float groundSpeed = 4f;
        [SerializeField] private float headingDegrees;

        private readonly SimpleRoute route = new SimpleRoute();
        private AirportManager airportManager;
        private GameManager gameManager;
        private TextMesh label;
        private Renderer[] aircraftRenderers;
        private Material normalMaterial;
        private Material selectedMaterial;
        private AircraftData flightData;
        private bool selected;
        private bool tutorialHighlighted;
        private Vector2 facingDirection = Vector2.up;
        private AircraftState heldTaxiState = AircraftState.Waiting;

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

        public void Configure(AircraftData data, bool isArrival, AircraftState startingState, AirportManager airport, GameManager manager, Material normal, Material selected)
        {
            flightData = data;
            flightNumber = data.FlightId;
            arrivalAircraft = isArrival;
            airportManager = airport;
            gameManager = manager;
            normalMaterial = normal;
            selectedMaterial = selected;

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
                route.Tick(transform, Time.deltaTime);
                UpdateHeadingFromMovement(transform.position - previousPosition);
            }

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
                    return !arrivalAircraft && currentState == AircraftState.HoldingPoint;
                case AircraftCommand.LineUp:
                    return !arrivalAircraft && currentState == AircraftState.HoldingShort;
                case AircraftCommand.ClearTakeoff:
                    return !arrivalAircraft && (currentState == AircraftState.LiningUp || currentState == AircraftState.Waiting);
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

        private void ClearLanding()
        {
            SetState(AircraftState.FinalApproach);
            gameManager.OccupyPrimaryRunway(this, "着陸中");
            StartRouteWithHeading(airportManager.GetArrivalFinalRoute(), airborneSpeed, () =>
            {
                SetState(AircraftState.LandingRoll);
                gameManager.OccupyPrimaryRunway(this, "着陸滑走中");
                StartRouteWithHeading(airportManager.GetLandingRollRoute(), groundSpeed + 1f, () =>
                {
                    SetState(AircraftState.VacatingRunway);
                    gameManager.OccupyPrimaryRunway(this, "滑走路離脱中");
                    StartRouteWithHeading(airportManager.GetVacateRunwayRoute(), groundSpeed, () =>
                    {
                        gameManager.ReleasePrimaryRunway(this);
                        SetState(AircraftState.Waiting);
                    });
                });
            });
        }

        private void TaxiToGate()
        {
            SetState(AircraftState.TaxiToGate);
            StartRouteWithHeading(airportManager.GetTaxiToAvailableGateRoute(), groundSpeed, () =>
            {
                SetState(AircraftState.AtGate);
                gameManager.NotifyAircraftHandled(this);
            });
        }

        private void Pushback()
        {
            SetState(AircraftState.Pushbacking);
            StartRouteWithHeading(airportManager.GetPushbackRoute(), groundSpeed * 0.65f, () =>
            {
                SetState(AircraftState.PushbackReady);
            });
        }

        private void TaxiToHold()
        {
            SetState(AircraftState.TaxiToHold);
            StartRouteWithHeading(airportManager.GetTaxiToHoldRoute(), groundSpeed, () =>
            {
                SetState(AircraftState.HoldingPoint);
            });
        }

        private void HoldTaxi()
        {
            heldTaxiState = currentState;
            route.Pause();
            SetState(AircraftState.TaxiHeld);
        }

        private void ResumeTaxi()
        {
            var resumeState = heldTaxiState == AircraftState.Waiting ? AircraftState.TaxiToHold : heldTaxiState;
            SetState(resumeState);
            route.Resume();
            heldTaxiState = AircraftState.Waiting;
        }

        private void HoldShort()
        {
            route.Stop();
            SetState(AircraftState.HoldingShort);
            transform.position = airportManager.HoldShortPosition;
            SetHeadingFromWorldDirection(Vector3.forward);
        }

        private void LineUp()
        {
            SetState(AircraftState.LiningUp);
            gameManager.OccupyPrimaryRunway(this, "滑走路上待機");
            StartRouteWithHeading(airportManager.GetLineUpRoute(), groundSpeed, () =>
            {
                SetState(AircraftState.LiningUp);
            });
        }

        private void ClearTakeoff()
        {
            SetState(AircraftState.TakeoffRoll);
            gameManager.OccupyPrimaryRunway(this, "離陸滑走中");
            StartRouteWithHeading(airportManager.GetTakeoffRoute(), airborneSpeed, () =>
            {
                gameManager.ReleasePrimaryRunway(this);
                SetState(AircraftState.AirborneDeparture);
                gameManager.NotifyAircraftHandled(this);
            });
        }

        private void StopAircraft()
        {
            route.Stop();
            SetState(AircraftState.Waiting);
        }

        private void SetState(AircraftState state)
        {
            currentState = state;
            ApplyDefaultHeadingForState(state);
            SyncFlightData();
        }

        private void StartRouteWithHeading(IEnumerable<Vector3> routePoints, float speed, Action completed = null)
        {
            var points = new List<Vector3>();
            foreach (var point in routePoints)
            {
                points.Add(point);
            }

            if (points.Count > 0)
            {
                SetHeadingToward(points[0]);
            }

            route.StartRoute(points, speed, completed);
        }

        private void UpdateHeadingFromMovement(Vector3 movement)
        {
            movement.y = 0f;
            if (movement.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            SetHeadingFromWorldDirection(movement);
        }

        private void SetHeadingToward(Vector3 targetPosition)
        {
            SetHeadingFromWorldDirection(targetPosition - transform.position);
        }

        private void SetHeadingFromWorldDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var normalized = worldDirection.normalized;
            facingDirection = new Vector2(normalized.x, normalized.z);
            headingDegrees = -Mathf.Atan2(facingDirection.x, facingDirection.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
            SyncFlightData();
        }

        private void ApplyDefaultHeadingForState(AircraftState state)
        {
            switch (state)
            {
                case AircraftState.Inbound:
                case AircraftState.FinalApproach:
                case AircraftState.LandingRoll:
                case AircraftState.LiningUp:
                case AircraftState.TakeoffRoll:
                case AircraftState.AirborneDeparture:
                    SetHeadingFromWorldDirection(Vector3.right);
                    break;
                case AircraftState.VacatingRunway:
                case AircraftState.TaxiToGate:
                case AircraftState.Pushbacking:
                    SetHeadingFromWorldDirection(Vector3.back);
                    break;
                case AircraftState.AtGate:
                    SetHeadingFromWorldDirection(arrivalAircraft ? Vector3.back : Vector3.forward);
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
                    return $"RWY_{flightData.ActiveRunwayDesignator}";
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
            labelObject.transform.localPosition = new Vector3(0f, 2.05f, 0.35f);
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
