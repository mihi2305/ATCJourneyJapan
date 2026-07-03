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
        private bool trainingStarted;

        public IReadOnlyList<AircraftController> Aircraft => aircraft;
        public bool StageClear => stageClear;
        public bool IsTrainingStarted => trainingStarted;
        public bool IsGameplayPaused => uiManager != null && uiManager.IsTutorialBlockingProgress;
        public bool IsDelayPaused => stageClear || (uiManager != null && uiManager.IsTutorialActive);

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
            if (!trainingStarted)
            {
                uiManager.Refresh();
                return;
            }

            if (!IsGameplayPaused)
            {
                scoreManager.Tick(Time.deltaTime, IsDelayPaused);
                RefreshRunwaySafety();
            }

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

        public void StartTraining()
        {
            trainingStarted = true;
            uiManager.ShowInstructorComment("この訓練では、到着機と出発機の基本的な順番を覚えよう");
            uiManager.StartTutorial();
        }

        public void NotifyCommandExecuted(AircraftController controller, AircraftCommand command)
        {
            uiManager.ShowCommandDescription(command);
            uiManager.ShowInstructorComment(GetInstructorComment(command));
            uiManager.NotifyCommandExecuted(command);
        }

        public bool CanAcceptCommand(AircraftCommand command)
        {
            return uiManager == null || uiManager.CanAcceptCommand(command);
        }

        private string GetInstructorComment(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "まずは到着機を安全に着陸させよう";
                case AircraftCommand.TaxiToGate:
                    return "滑走路が空いた。到着機をゲートへ進めよう";
                case AircraftCommand.TaxiToHold:
                    return "出発機を滑走路手前まで進めて、離陸の準備をしよう";
                case AircraftCommand.LineUp:
                    return "滑走路上に入れる前に、他の機体がいないことを確認しよう";
                case AircraftCommand.ClearTakeoff:
                    return "よし、安全に処理できている。離陸許可を出そう";
                case AircraftCommand.HoldShort:
                    return "滑走路に2機を同時に入れないことが基本だ";
                case AircraftCommand.Stop:
                    return "迷ったら止める判断も大切だ";
                default:
                    return "落ち着いて、次に必要な指示を選ぼう";
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
