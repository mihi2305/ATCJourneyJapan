using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Scoring;
using ATCJourneyJapan.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace ATCJourneyJapan.Core
{
    // Coordinates prototype managers and owns stage completion/safety status.
    public class GameManager : MonoBehaviour
    {
        private struct CameraPreset
        {
            public readonly string PresetName;
            public readonly Vector3 TargetCenter;
            public readonly Vector3 PositionOffset;
            public readonly Quaternion FixedRotation;
            public readonly bool UseLookAt;
            public readonly bool IsOrthographic;
            public readonly float OrthographicSize;
            public readonly float FieldOfView;

            public CameraPreset(
                string presetName,
                Vector3 targetCenter,
                Vector3 positionOffset,
                Quaternion fixedRotation,
                bool useLookAt,
                bool isOrthographic,
                float orthographicSize,
                float fieldOfView)
            {
                PresetName = presetName;
                TargetCenter = targetCenter;
                PositionOffset = positionOffset;
                FixedRotation = fixedRotation;
                UseLookAt = useLookAt;
                IsOrthographic = isOrthographic;
                OrthographicSize = orthographicSize;
                FieldOfView = fieldOfView;
            }
        }

        private static readonly Vector3 AirportViewCenter = new Vector3(3f, 0.2f, 3f);

        private readonly List<AircraftController> aircraft = new List<AircraftController>();
        private readonly HashSet<AircraftController> handledAircraft = new HashSet<AircraftController>();

        private AirportManager airportManager;
        private AircraftSpawner aircraftSpawner;
        private ScoreManager scoreManager;
        private UIManager uiManager;
        private Camera mainCamera;
        private CameraPreset[] cameraPresets;
        private int currentCameraPresetIndex;
        private bool runwayConflictActive;
        private bool stageClear;
        private bool trainingStarted;

        public IReadOnlyList<AircraftController> Aircraft => aircraft;
        public bool StageClear => stageClear;
        public bool IsTrainingStarted => trainingStarted;
        public bool IsGameplayPaused => uiManager != null && uiManager.IsTutorialBlockingProgress;
        public bool IsDelayPaused => trainingStarted || stageClear;
        public bool IsPrimaryRunwayOccupied => GetPrimaryRunwayController()?.RunwayOccupied ?? false;
        public string PrimaryRunwayOccupiedByFlightId => GetPrimaryRunwayController()?.OccupiedByFlightId ?? string.Empty;
        public string PrimaryRunwayOccupiedReason => GetPrimaryRunwayController()?.OccupiedReason ?? string.Empty;
        public AirportManager Airport => airportManager;

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
            HandleCameraViewInput();

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
            uiManager.AddControllerCommandLog(controller, command);
            uiManager.NotifyCommandExecuted(command);
        }

        public void BindRunwayDirectionForCommand(AircraftController controller, AircraftCommand command, string operationDirection = "")
        {
            if (controller == null || airportManager == null || !RequiresRunwayDirectionBinding(command, operationDirection))
            {
                return;
            }

            var resolvedDirection = !string.IsNullOrEmpty(operationDirection)
                ? operationDirection
                : airportManager.CurrentPrimaryRunwayDesignator;
            controller.BindRunwayDirection(airportManager.PrimaryRunwayData, resolvedDirection);
        }

        public bool CanAcceptCommand(AircraftCommand command)
        {
            return uiManager == null || uiManager.CanAcceptCommand(command);
        }

        public bool CanAcceptCommand(AircraftController controller, AircraftCommand command)
        {
            return uiManager == null || uiManager.CanAcceptCommand(controller, command);
        }

        public bool CanExecuteRunwaySafetyCommand(AircraftController controller, AircraftCommand command)
        {
            if (!RequiresRunwaySafetyCheck(command))
            {
                return true;
            }

            var runway = GetPrimaryRunwayController();
            return runway == null || !runway.IsOccupiedByOther(controller);
        }

        public void RejectUnsafeRunwayCommand(AircraftController controller, AircraftCommand command)
        {
            scoreManager.ApplySafetyPenalty(10);

            var runway = GetPrimaryRunwayController();
            var occupiedBy = runway != null && !string.IsNullOrEmpty(runway.OccupiedByFlightId)
                ? runway.OccupiedByFlightId
                : "他機";
            uiManager.ShowWarning($"RWY 18Lは使用中です。{occupiedBy}が滑走路を使用中です。");
        }

        public void OccupyPrimaryRunway(AircraftController controller, string reason)
        {
            GetPrimaryRunwayController()?.Occupy(controller, reason);
        }

        public void ReleasePrimaryRunway(AircraftController controller)
        {
            GetPrimaryRunwayController()?.Release(controller);
        }

        private string GetInstructorComment(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "まずは到着機を安全に着陸させよう";
                case AircraftCommand.TaxiToGate:
                    return "滑走路が空いた。到着機をSPOT 01へ進めよう";
                case AircraftCommand.Pushback:
                    return "スポットから後退させて、地上走行の準備をしよう";
                case AircraftCommand.TaxiToHold:
                    return "Groundでは滑走路手前まで安全に進める";
                case AircraftCommand.HoldTaxi:
                    return "必要な時は地上走行中の機体を現在位置で止められる";
                case AircraftCommand.ResumeTaxi:
                    return "安全を確認したら地上走行を再開しよう";
                case AircraftCommand.LineUp:
                    return "滑走路上に入れる前に、他の機体がいないことを確認しよう";
                case AircraftCommand.ClearTakeoff:
                    return "よし、安全に処理できている。離陸許可を出そう";
                case AircraftCommand.HoldShort:
                    return "Hold Shortは滑走路の手前で止める指示だ";
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
                if (!IsPrimaryRunwayOccupied)
                {
                    uiManager.ClearWarning();
                }
            }
        }

        private RunwayController GetPrimaryRunwayController()
        {
            return airportManager != null && airportManager.Runways.Count > 0 ? airportManager.Runways[0] : null;
        }

        private bool RequiresRunwaySafetyCheck(AircraftCommand command)
        {
            return command == AircraftCommand.ClearLanding
                || command == AircraftCommand.LineUp
                || command == AircraftCommand.ClearTakeoff;
        }

        private bool RequiresRunwayDirectionBinding(AircraftCommand command, string operationDirection)
        {
            if (command == AircraftCommand.ClearLanding || command == AircraftCommand.TaxiToHold)
            {
                return true;
            }

            return !string.IsNullOrEmpty(operationDirection)
                && (command == AircraftCommand.HoldShort
                    || command == AircraftCommand.LineUp
                    || command == AircraftCommand.ClearTakeoff);
        }

        private void SetupCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            InitializeCameraPresets();
            ApplyCameraPreset(0);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.42f, 0.72f, 0.94f);
            ConfigureAtmosphere();
        }

        private void ConfigureAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.84f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.48f, 0.56f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.28f, 0.34f, 0.3f);
            RenderSettings.fog = false;
        }

        private void InitializeCameraPresets()
        {
            cameraPresets = new[]
            {
                new CameraPreset(
                    "Top View",
                    AirportViewCenter,
                    new Vector3(-3f, 24.8f, -21f),
                    Quaternion.Euler(58f, 0f, 0f),
                    false,
                    true,
                    18.5f,
                    48f),
                new CameraPreset(
                    "Oblique View",
                    AirportViewCenter,
                    new Vector3(0f, 16f, -24f),
                    Quaternion.identity,
                    true,
                    false,
                    15f,
                    50f),
                new CameraPreset(
                    "Wide View",
                    AirportViewCenter,
                    new Vector3(0f, 24f, -41f),
                    Quaternion.identity,
                    true,
                    false,
                    15f,
                    60f)
            };
        }

        private void HandleCameraViewInput()
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                ApplyCameraPreset(currentCameraPresetIndex + 1);
            }
        }

        private void ApplyCameraPreset(int presetIndex)
        {
            if (mainCamera == null || cameraPresets == null || cameraPresets.Length == 0)
            {
                return;
            }

            currentCameraPresetIndex = presetIndex % cameraPresets.Length;
            var preset = cameraPresets[currentCameraPresetIndex];
            mainCamera.transform.position = preset.TargetCenter + preset.PositionOffset;
            if (preset.UseLookAt)
            {
                mainCamera.transform.LookAt(preset.TargetCenter);
            }
            else
            {
                mainCamera.transform.rotation = preset.FixedRotation;
            }

            mainCamera.orthographic = preset.IsOrthographic;
            mainCamera.orthographicSize = preset.OrthographicSize;
            mainCamera.fieldOfView = preset.FieldOfView;
        }

        private void SetupLight()
        {
            var light = FindFirstObjectByType<Light>();
            if (light == null)
            {
                var lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.35f;
            light.transform.rotation = Quaternion.Euler(45f, -35f, 12f);
        }
    }
}
