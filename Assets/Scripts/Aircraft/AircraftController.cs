using System.Collections.Generic;
using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Core;
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

        private readonly SimpleRoute route = new SimpleRoute();
        private AirportManager airportManager;
        private GameManager gameManager;
        private TextMesh label;
        private Renderer aircraftRenderer;
        private Material normalMaterial;
        private Material selectedMaterial;

        public string FlightNumber => flightNumber;
        public bool IsArrivalAircraft => arrivalAircraft;
        public AircraftState CurrentState => currentState;
        public bool IsComplete => arrivalAircraft ? currentState == AircraftState.AtGate : currentState == AircraftState.AirborneDeparture;
        public bool IsOnRunway => currentState == AircraftState.FinalApproach
                                  || currentState == AircraftState.LandingRoll
                                  || currentState == AircraftState.LiningUp
                                  || currentState == AircraftState.TakeoffRoll;

        public void Configure(string callSign, bool isArrival, AircraftState startingState, AirportManager airport, GameManager manager, Material normal, Material selected)
        {
            flightNumber = callSign;
            arrivalAircraft = isArrival;
            currentState = startingState;
            airportManager = airport;
            gameManager = manager;
            normalMaterial = normal;
            selectedMaterial = selected;

            aircraftRenderer = GetComponentInChildren<Renderer>();
            SetSelected(false);
            CreateLabel();
            UpdateLabel();
        }

        private void Update()
        {
            route.Tick(transform, Time.deltaTime);
            UpdateLabel();
        }

        private void OnMouseDown()
        {
            SelectionManager.Instance?.SelectAircraft(this);
        }

        public void SetSelected(bool selected)
        {
            if (aircraftRenderer != null)
            {
                aircraftRenderer.sharedMaterial = selected ? selectedMaterial : normalMaterial;
            }
        }

        public bool CanExecute(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return arrivalAircraft && currentState == AircraftState.Inbound;
                case AircraftCommand.TaxiToGate:
                    return arrivalAircraft && (currentState == AircraftState.Waiting || currentState == AircraftState.VacatingRunway);
                case AircraftCommand.TaxiToHold:
                    return !arrivalAircraft && currentState == AircraftState.AtGate;
                case AircraftCommand.HoldShort:
                    return !arrivalAircraft && (currentState == AircraftState.TaxiToHold || currentState == AircraftState.HoldingShort);
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
                case AircraftCommand.TaxiToHold:
                    TaxiToHold();
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
            currentState = AircraftState.FinalApproach;
            route.StartRoute(airportManager.GetArrivalFinalRoute(), airborneSpeed, () =>
            {
                currentState = AircraftState.LandingRoll;
                route.StartRoute(airportManager.GetLandingRollRoute(), groundSpeed + 1f, () =>
                {
                    currentState = AircraftState.VacatingRunway;
                    route.StartRoute(airportManager.GetVacateRunwayRoute(), groundSpeed, () =>
                    {
                        currentState = AircraftState.Waiting;
                    });
                });
            });
        }

        private void TaxiToGate()
        {
            currentState = AircraftState.TaxiToGate;
            route.StartRoute(airportManager.GetTaxiToAvailableGateRoute(), groundSpeed, () =>
            {
                currentState = AircraftState.AtGate;
                gameManager.NotifyAircraftHandled(this);
            });
        }

        private void TaxiToHold()
        {
            currentState = AircraftState.TaxiToHold;
            route.StartRoute(airportManager.GetTaxiToHoldRoute(), groundSpeed, () =>
            {
                currentState = AircraftState.HoldingShort;
            });
        }

        private void HoldShort()
        {
            route.Stop();
            currentState = AircraftState.HoldingShort;
            transform.position = airportManager.HoldShortPosition;
        }

        private void LineUp()
        {
            currentState = AircraftState.LiningUp;
            route.StartRoute(airportManager.GetLineUpRoute(), groundSpeed, () =>
            {
                currentState = AircraftState.LiningUp;
            });
        }

        private void ClearTakeoff()
        {
            currentState = AircraftState.TakeoffRoll;
            route.StartRoute(airportManager.GetTakeoffRoute(), airborneSpeed, () =>
            {
                currentState = AircraftState.AirborneDeparture;
                gameManager.NotifyAircraftHandled(this);
            });
        }

        private void StopAircraft()
        {
            route.Stop();
            currentState = AircraftState.Waiting;
        }

        private void CreateLabel()
        {
            var labelObject = new GameObject("Flight Label");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.4f;
            label.fontSize = 42;
            label.color = Color.white;
        }

        private void UpdateLabel()
        {
            if (label == null)
            {
                return;
            }

            label.text = $"{flightNumber}\n{currentState}";
            var cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null)
            {
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - cameraTransform.position);
            }
        }
    }
}
