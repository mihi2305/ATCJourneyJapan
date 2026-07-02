using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Scoring;
using ATCJourneyJapan.UI;
using UnityEngine;

namespace ATCJourneyJapan.Core
{
    // Coordinates prototype managers and owns stage completion/safety status.
    public class GameManager : MonoBehaviour
    {
        private readonly List<AircraftController> aircraft = new List<AircraftController>();
        private readonly HashSet<AircraftController> handledAircraft = new HashSet<AircraftController>();

        private AirportManager airportManager;
        private AircraftSpawner aircraftSpawner;
        private ScoreManager scoreManager;
        private UIManager uiManager;
        private bool runwayConflictActive;
        private bool stageClear;

        public IReadOnlyList<AircraftController> Aircraft => aircraft;
        public bool StageClear => stageClear;

        public void Initialize()
        {
            SetupCamera();
            SetupLight();

            airportManager = gameObject.AddComponent<AirportManager>();
            aircraftSpawner = gameObject.AddComponent<AircraftSpawner>();
            gameObject.AddComponent<SelectionManager>();
            gameObject.AddComponent<CommandSystem>();
            scoreManager = gameObject.AddComponent<ScoreManager>();
            uiManager = gameObject.AddComponent<UIManager>();

            airportManager.Initialize();
            scoreManager.Initialize();
            uiManager.Initialize(this, scoreManager);
            aircraftSpawner.Initialize(airportManager, this);
            aircraftSpawner.SpawnInitialAircraft();
        }

        private void Update()
        {
            scoreManager.Tick(Time.deltaTime, stageClear);
            RefreshRunwaySafety();
            uiManager.Refresh();

            if (!stageClear && handledAircraft.Count >= 2)
            {
                stageClear = true;
                uiManager.ShowStageClear();
            }
        }

        public void RegisterAircraft(AircraftController controller)
        {
            if (!aircraft.Contains(controller))
            {
                aircraft.Add(controller);
            }
        }

        public void NotifyAircraftHandled(AircraftController controller)
        {
            if (controller != null && handledAircraft.Add(controller))
            {
                scoreManager.SetHandledAircraftCount(handledAircraft.Count);
            }
        }

        private void RefreshRunwaySafety()
        {
            var anyConflict = false;

            foreach (var runway in airportManager.Runways)
            {
                runway.Refresh(aircraft);
                if (runway.HasConflict)
                {
                    anyConflict = true;
                    uiManager.ShowWarning($"RUNWAY CONFLICT: Runway {runway.RunwayId} has multiple aircraft.");
                    break;
                }
            }

            if (anyConflict && !runwayConflictActive)
            {
                runwayConflictActive = true;
                scoreManager.ApplySafetyPenalty(25);
            }
            else if (!anyConflict)
            {
                runwayConflictActive = false;
                uiManager.ClearWarning();
            }
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.transform.position = new Vector3(0f, 24f, -18f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            camera.backgroundColor = new Color(0.08f, 0.12f, 0.16f);
        }

        private void SetupLight()
        {
            if (FindFirstObjectByType<Light>() != null)
            {
                return;
            }

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}
