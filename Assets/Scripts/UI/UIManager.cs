using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Radio;
using ATCJourneyJapan.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCJourneyJapan.UI
{
    public class UIManager : MonoBehaviour
    {
        private const float MiniMapWorldMinX = -31f;
        private const float MiniMapWorldMaxX = 38f;
        private const float MiniMapWorldMinZ = -18f;
        private const float MiniMapWorldMaxZ = 24f;
        private const float MiniMapWidth = 376f;
        private const float MiniMapHeight = 206f;
        private const float OverlayMapWidth = 780f;
        private const float OverlayMapHeight = 455f;
        private const string RouteSelectionModeDeparture = "DepartureRoute";
        private const string RouteSelectionModeArrivalExit = "ArrivalExit";
        private const string RouteSelectionModeArrivalSpotTaxi = "ArrivalSpotTaxi";

        private static readonly Dictionary<string, string> JapaneseCityNames = new Dictionary<string, string>
        {
            { "tokyo", "東京" },
            { "osaka", "大阪" },
            { "fukuoka", "福岡" },
            { "sapporo", "札幌" },
            { "nagoya", "名古屋" },
            { "naha", "那覇" },
            { "miyazaki", "宮崎" }
        };

        private readonly AircraftCommand[] commandOrder =
        {
            AircraftCommand.ClearLanding,
            AircraftCommand.TaxiToGate,
            AircraftCommand.Pushback,
            AircraftCommand.TaxiToHold,
            AircraftCommand.HoldTaxi,
            AircraftCommand.ResumeTaxi,
            AircraftCommand.HoldShort,
            AircraftCommand.LineUp,
            AircraftCommand.ClearTakeoff
        };

        private readonly Dictionary<Renderer, Color> highlightedObjectColors = new Dictionary<Renderer, Color>();
        private readonly Dictionary<string, Button> flightStripButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> flightStripTexts = new Dictionary<string, Text>();
        private readonly Dictionary<string, Image> flightStripImages = new Dictionary<string, Image>();
        private readonly Dictionary<string, RectTransform> minimapAircraftDots = new Dictionary<string, RectTransform>();
        private readonly Dictionary<string, Text> minimapAircraftArrows = new Dictionary<string, Text>();
        private readonly Dictionary<string, Outline> minimapAircraftOutlines = new Dictionary<string, Outline>();
        private readonly Dictionary<string, Text> minimapAircraftLabels = new Dictionary<string, Text>();
        private readonly List<Image> minimapRouteHighlightSegments = new List<Image>();
        private readonly List<string> commandLogEntries = new List<string>();
        private readonly List<Button> stripCommandOptionButtons = new List<Button>();
        private readonly List<Text> stripCommandOptionTexts = new List<Text>();
        private readonly List<Button> routeSelectionOptionButtons = new List<Button>();
        private readonly List<Text> routeSelectionOptionTexts = new List<Text>();
        private readonly List<Image> routeSelectionPreviewSegments = new List<Image>();
        private readonly List<Image> routeSelectionHighlightSegments = new List<Image>();
        private readonly List<Text> routeSelectionEntryLabelTexts = new List<Text>();
        private readonly List<TaxiRouteCandidate> activeRouteSelectionCandidates = new List<TaxiRouteCandidate>();
        private readonly List<string> arrivalRunwaySelectionCompletedFlightIds = new List<string>();
        private GameManager gameManager;
        private ScoreManager scoreManager;
        private CommandSystem commandSystem;
        private Font uiFont;
        private string warningMessage;
        private string instructorMessage = "まずは到着機を安全に着陸させよう";
        private string commandHelpMessage = "航空機を選択すると、次の指示を確認できます。";
        private bool resultVisible;

        private GameObject startBackdrop;
        private GameObject startPanel;
        private GameObject hudRoot;
        private GameObject flightStripPanel;
        private GameObject stripCommandPopup;
        private GameObject commandPanel;
        private GameObject instructorPanel;
        private GameObject minimapPanel;
        private RectTransform minimapContent;
        private Image minimapRunwayImage;
        private GameObject selectedPanel;
        private GameObject runwaySelectionOverlay;
        private GameObject runwaySelectionMapContent;
        private GameObject routeSelectionOverlay;
        private GameObject routeSelectionMapContent;
        private GameObject guidePanel;
        private GameObject tutorialPanel;
        private GameObject warningPanel;
        private GameObject resultPanel;
        private Text scoreText;
        private Text guideText;
        private Text instructorText;
        private Text selectedText;
        private Text controlSectorSummaryText;
        private Text runwaySelectionTitleText;
        private Text runwaySelectionHintText;
        private Text runwaySelectionStatusText;
        private Text runwaySelection18LButtonText;
        private Text runwaySelection36RButtonText;
        private Text routeSelectionTitleText;
        private Text routeSelectionHintText;
        private Image runwaySelection18LMarker;
        private Image runwaySelection36RMarker;
        private Image runwaySelectionRunwayHighlight;
        private Text routeSelectionRunwayStatusText;
        private Image routeSelectionRunwayHighlight;
        private Image routeSelection18LMarker;
        private Image routeSelection36RMarker;
        private Text helpText;
        private Text commandStatusText;
        private Text stripCommandButtonText;
        private Text tutorialText;
        private Text warningText;
        private Text resultText;
        private Button commandButton;
        private Button stripCommandButton;
        private Text commandButtonText;
        private Button tutorialNextButton;
        private string lastLoggedMiniMapRouteKey = string.Empty;
        private AircraftController runwaySelectionAircraft;
        private string routeSelectionPurpose = string.Empty;
        private string routeSelectionMode = string.Empty;
        private AircraftController routeSelectionAircraft;
        private TutorialStep[] tutorialSteps;
        private int tutorialStepIndex = -1;
        private bool tutorialActive;
        private TutorialHighlight activeTutorialHighlight = TutorialHighlight.None;

        public bool IsTutorialBlockingProgress
        {
            get
            {
                var step = CurrentTutorialStep;
                return tutorialActive && step != null && step.WaitMode == TutorialWaitMode.Info;
            }
        }

        public bool IsTutorialActive => CurrentTutorialStep != null;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            BuildUi();
            Refresh();
        }

        public void Refresh()
        {
            if (hudRoot == null)
            {
                return;
            }

            startBackdrop.SetActive(!gameManager.IsTrainingStarted);
            startPanel.SetActive(!gameManager.IsTrainingStarted);
            hudRoot.SetActive(gameManager.IsTrainingStarted && !gameManager.StageClear && !resultVisible);
            resultPanel.SetActive(gameManager.StageClear || resultVisible);

            if (!gameManager.IsTrainingStarted)
            {
                return;
            }

            if (gameManager.StageClear || resultVisible)
            {
                UpdateResult();
                return;
            }

            UpdateTrainingHud();
        }

        public void ShowWarning(string message)
        {
            warningMessage = message;
            instructorMessage = "滑走路に2機を同時に入れないことが基本です。";
            Refresh();
        }

        public void ClearWarning()
        {
            warningMessage = string.Empty;
            Refresh();
        }

        public void ShowStageClear()
        {
            resultVisible = true;
            instructorMessage = "よし、安全に処理できている。";
            ApplyTutorialHighlight(TutorialHighlight.None);
            Refresh();
        }

        public void ShowCommandDescription(AircraftCommand command)
        {
            commandHelpMessage = GetCommandDescription(command);
            Refresh();
        }

        public void ShowInstructorComment(string message)
        {
            instructorMessage = message;
            Refresh();
        }

        public void StartTutorial()
        {
            EnsureTutorialSteps();
            tutorialActive = true;
            tutorialStepIndex = 0;
            activeTutorialHighlight = TutorialHighlight.None;
            Refresh();
        }

        public bool CanAcceptCommand(AircraftCommand command)
        {
            return CanAcceptCommand(GetSelectedAircraft(), command);
        }

        public bool CanAcceptCommand(AircraftController aircraft, AircraftCommand command)
        {
            if (command == AircraftCommand.HoldTaxi || command == AircraftCommand.ResumeTaxi)
            {
                return true;
            }

            var step = CurrentTutorialStep;
            if (step == null)
            {
                return true;
            }

            return step.WaitForCommand
                && step.ExpectedCommand == command
                && step.AcceptsAircraft(aircraft);
        }

        public void NotifyCommandExecuted(AircraftCommand command)
        {
            var step = CurrentTutorialStep;
            if (step != null && step.WaitForCommand && step.ExpectedCommand == command)
            {
                AdvanceTutorial();
            }
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("Training HUD Canvas");
            canvasObject.transform.SetParent(transform, false);
            EnsureEventSystem();

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            canvas.pixelPerfect = true;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            startBackdrop = CreateFullscreenImage("Start Backdrop", canvasObject.transform, new Color(0.02f, 0.03f, 0.04f, 0.68f));
            startPanel = CreatePanel("Start Panel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(780f, 360f));
            CreateText("Start Title", startPanel.transform, "Basic Runway Training", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 118f), new Vector2(680f, 54f));
            CreateText("Start Body", startPanel.transform, "A滑走路の基本訓練です。\n到着機を着陸させ、出発機を離陸させましょう。", 25, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, 28f), new Vector2(660f, 86f));
            var startButton = CreateButton("Start Button", startPanel.transform, "訓練開始\nStart Training", new Vector2(0f, -116f), new Vector2(260f, 72f), 22);
            startButton.onClick.AddListener(() => gameManager.StartTraining());

            hudRoot = CreateRoot("HUD Root", canvasObject.transform);
            scoreText = CreatePanelText("Score", hudRoot.transform, new Vector2(24f, -22f), new Vector2(320f, 126f), AnchorPreset.TopLeft, string.Empty, 24, TextAnchor.UpperLeft);
            instructorText = CreatePanelText("Instructor", hudRoot.transform, new Vector2(-24f, -22f), new Vector2(430f, 112f), AnchorPreset.TopRight, "教官コメント", 19, TextAnchor.UpperLeft);
            instructorPanel = instructorText.transform.parent.gameObject;
            instructorPanel.SetActive(false);
            BuildMiniMapPanel();
            BuildFlightStripPanel();
            guideText = CreatePanelText("Command Log", hudRoot.transform, new Vector2(0f, 12f), new Vector2(780f, 84f), AnchorPreset.BottomCenter, string.Empty, 16, TextAnchor.UpperLeft);
            guidePanel = guideText.transform.parent.gameObject;
            guidePanel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.7f);

            selectedPanel = CreatePanel("Selected Aircraft", hudRoot.transform, new Vector2(0f, 0f), new Vector2(360f, 256f), AnchorPreset.BottomRight, new Vector2(-24f, 34f));
            selectedText = CreateText("Selected Text", selectedPanel.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(316f, 222f));

            commandPanel = CreatePanel("Command Panel", hudRoot.transform, new Vector2(0f, 0f), new Vector2(300f, 410f), AnchorPreset.MiddleRight, new Vector2(-12f, 0f));
            CreateText("Command Title", commandPanel.transform, "指示", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 154f), new Vector2(250f, 34f));
            commandButton = CreateButton("Recommended Command", commandPanel.transform, string.Empty, new Vector2(0f, 74f), new Vector2(250f, 94f), 19);
            commandButton.onClick.AddListener(ExecuteRecommendedCommand);
            helpText = CreateText("Command Help", commandPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0f, -96f), new Vector2(250f, 94f));
            commandStatusText = CreateText("Command Status", commandPanel.transform, string.Empty, 20, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, 28f), new Vector2(250f, 86f));
            commandPanel.SetActive(false);

            tutorialPanel = CreatePanel("Tutorial Panel", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(660f, 116f), AnchorPreset.BottomCenter, new Vector2(0f, 108f));
            tutorialPanel.GetComponent<Image>().color = new Color(0.01f, 0.015f, 0.02f, 0.94f);
            tutorialText = CreateText("Tutorial Text", tutorialPanel.transform, string.Empty, 25, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-74f, 2f), new Vector2(448f, 78f));
            tutorialNextButton = CreateButton("Tutorial Next", tutorialPanel.transform, "次へ", new Vector2(240f, -20f), new Vector2(132f, 52f), 20);
            tutorialNextButton.onClick.AddListener(AdvanceTutorial);

            warningPanel = CreatePanel("Warning", hudRoot.transform, new Vector2(0f, 0f), new Vector2(760f, 44f), AnchorPreset.TopCenter, new Vector2(0f, -22f));
            warningText = CreateText("Warning Text", warningPanel.transform, string.Empty, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(720f, 32f));
            warningText.color = new Color(1f, 0.42f, 0.3f);

            resultPanel = CreatePanel("Result Panel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(620f, 330f));
            resultText = CreateText("Result Text", resultPanel.transform, string.Empty, 23, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(540f, 270f));
            BuildRunwaySelectionOverlay();
            BuildRouteSelectionOverlay();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void UpdateTrainingHud()
        {
            var selected = GetSelectedAircraft();
            var recommended = selected != null ? GetRecommendedCommand(selected) : null;

            scoreText.text = $"安全度：  {scoreManager.Safety}\n遅延：    {Mathf.FloorToInt(scoreManager.Delay)}\n処理機数：{scoreManager.HandledAircraftCount}";
            instructorText.text = $"教官コメント\n{GetInstructorHint()}";
            instructorPanel.SetActive(false);
            AdvanceTutorialIfReady();
            UpdateTutorialPanel();
            UpdateCommandLogPanel();
            UpdateFlightStrips(selected);
            UpdateMiniMap(selected);

            selectedPanel.SetActive(selected != null);
            commandPanel.SetActive(false);

            if (selected != null)
            {
                selectedText.text = GetSelectedAircraftText(selected, recommended);
                helpText.text = recommended.HasValue ? GetCommandDescription(recommended.Value) : commandHelpMessage;
            }
            else
            {
                helpText.text = "航空機を選択すると、使える指示が表示されます。";
                CloseRouteSelectionOverlay();
            }

            commandButton.gameObject.SetActive(false);
            commandStatusText.gameObject.SetActive(false);
            commandStatusText.text = GetCommandStatusText(selected, recommended);

            warningPanel.SetActive(!string.IsNullOrEmpty(warningMessage));
            warningText.text = warningMessage;
        }

        private void CloseRouteSelectionOverlay()
        {
            if (routeSelectionOverlay != null)
            {
                routeSelectionOverlay.SetActive(false);
            }

            ClearRouteSelectionPreview();
            ClearRouteSelectionHighlight();
            ClearRouteSelectionEntryLabels();
            ClearRouteSelectionRunwayVisual();
            routeSelectionAircraft = null;
        }

        private void OpenRunwaySelectionOverlay(AircraftController aircraft)
        {
            if (aircraft == null || aircraft.FlightData == null || runwaySelectionOverlay == null)
            {
                return;
            }

            runwaySelectionAircraft = aircraft;
            var currentRunway = aircraft.FlightData.HasActiveRunwayDesignator
                ? aircraft.FlightData.ActiveRunwayDesignator
                : "18L";
            var isArrival = aircraft.FlightData.OperationType == "Arrival";
            runwaySelectionTitleText.text = $"滑走路を選択\n{aircraft.FlightData.FlightId} / {(isArrival ? "Arrival" : "Departure")}";
            runwaySelectionHintText.text = isArrival
                ? "着陸に使うA滑走路の方向を選択します。選択後、この滑走路への着陸許可を出します。"
                : "A滑走路の使用方向を選択します。選択後、そのRWYに対応するタクシールート候補だけを表示します。";
            if (runwaySelection18LButtonText != null)
            {
                runwaySelection18LButtonText.text = isArrival ? "RWY 18L\n18Lへ着陸" : "RWY 18L\n18L側を使用";
            }

            if (runwaySelection36RButtonText != null)
            {
                runwaySelection36RButtonText.text = isArrival ? "RWY 36R\n36Rへ着陸" : "RWY 36R\n36R側を使用";
            }

            runwaySelectionOverlay.SetActive(true);
            UpdateRunwaySelectionVisual(currentRunway);
        }

        private void CloseRunwaySelectionOverlay()
        {
            if (runwaySelectionOverlay != null)
            {
                runwaySelectionOverlay.SetActive(false);
            }

            runwaySelectionAircraft = null;
        }

        private void SelectRunwayFromOverlay(string runwayDesignator)
        {
            var targetAircraft = runwaySelectionAircraft;
            CloseRunwaySelectionOverlay();
            SelectRunwayForAircraft(targetAircraft, runwayDesignator);
        }

        private void OpenRouteSelectionOverlay(AircraftController aircraft, string mode)
        {
            if (aircraft == null || aircraft.FlightData == null)
            {
                return;
            }

            activeRouteSelectionCandidates.Clear();
            var candidates = GetRouteSelectionCandidates(aircraft, mode);
            EnsureRouteSelectionOptionButtonCount(candidates.Count);
            for (var index = 0; index < candidates.Count; index++)
            {
                activeRouteSelectionCandidates.Add(candidates[index]);
            }

            if (activeRouteSelectionCandidates.Count == 1)
            {
                AutoSelectSingleRouteCandidate(aircraft, mode, activeRouteSelectionCandidates[0]);
                return;
            }

            if (activeRouteSelectionCandidates.Count <= 1)
            {
                return;
            }

            routeSelectionAircraft = aircraft;
            routeSelectionMode = mode;
            routeSelectionPurpose = GetRouteSelectionPurpose(aircraft);
            var data = aircraft.FlightData;
            var runwayDesignator = GetRouteSelectionRunwayDesignator(data);
            routeSelectionTitleText.text = $"{GetRouteSelectionOverlayTitle(mode)}\n{data.FlightId} / {GetOperationLabel(data.OperationType)} / RWY {runwayDesignator}";
            routeSelectionHintText.text = GetRouteSelectionHint(mode);
            LogRouteSelectionCandidates(aircraft, runwayDesignator);

            var selectedRouteId = GetSelectedRouteId(data, routeSelectionPurpose);
            for (var index = 0; index < routeSelectionOptionButtons.Count; index++)
            {
                var hasCandidate = index < activeRouteSelectionCandidates.Count;
                routeSelectionOptionButtons[index].gameObject.SetActive(hasCandidate);
                if (!hasCandidate)
                {
                    continue;
                }

                var candidate = activeRouteSelectionCandidates[index];
                routeSelectionOptionButtons[index].GetComponent<RectTransform>().anchoredPosition = GetRouteSelectionOptionPosition(index, activeRouteSelectionCandidates.Count);
                routeSelectionOptionTexts[index].text = GetPlayerRouteOptionText(candidate, index, mode);
                ApplyRouteSelectionOptionStyle(routeSelectionOptionButtons[index], IsRouteSelectionCandidateSelected(candidate, selectedRouteId));
            }

            routeSelectionOverlay.SetActive(true);
            UpdateRouteSelectionRunwayVisual(runwayDesignator);
            ShowRouteSelectionPreview();
            ShowRouteSelectionHighlight(GetInitialRouteSelectionHighlightIndex(selectedRouteId));
            UpdateRouteSelectionEntryLabels(mode);
        }

        private void AutoSelectSingleRouteCandidate(AircraftController aircraft, string mode, TaxiRouteCandidate candidate, bool refreshAfterSelection = true)
        {
            routeSelectionPurpose = GetRouteSelectionPurpose(aircraft);
            aircraft.SetSelectedTaxiRoute(routeSelectionPurpose, candidate.RouteId);
            Debug.Log(
                $"Route selection auto-selected: {aircraft.FlightNumber} mode={mode} {routeSelectionPurpose} "
                + $"RWY {candidate.RunwayDesignator} {candidate.SpotId} routeId={candidate.RouteId} "
                + $"entryUsage={candidate.RunwayEntryUsageId} segments={FormatRouteSegmentIds(candidate)} "
                + $"firstWaypoint={FormatRouteWaypoint(candidate, true)} lastWaypoint={FormatRouteWaypoint(candidate, false)} fallback=false");
            if (refreshAfterSelection)
            {
                Refresh();
            }
        }

        private void SelectRouteSelectionCandidate(int index)
        {
            if (routeSelectionAircraft == null || routeSelectionAircraft.FlightData == null || index < 0 || index >= activeRouteSelectionCandidates.Count)
            {
                return;
            }

            var candidate = activeRouteSelectionCandidates[index];
            ShowRouteSelectionHighlight(index);
            routeSelectionAircraft.SetSelectedTaxiRoute(routeSelectionPurpose, candidate.RouteId);
            if (routeSelectionMode == RouteSelectionModeArrivalExit)
            {
                Debug.Log(
                    $"Arrival exit selected: {routeSelectionAircraft.FlightNumber} runway={candidate.RunwayDesignator} "
                    + $"exitUsage={candidate.RunwayExitUsageId} routeId={candidate.RouteId} "
                    + $"segments={FormatRouteSegmentIds(candidate)} firstWaypoint={FormatRouteWaypoint(candidate, true)} "
                    + $"lastWaypoint={FormatRouteWaypoint(candidate, false)}");
            }

            Debug.Log(
                $"Route selection overlay selected: {routeSelectionAircraft.FlightNumber} {routeSelectionMode} {routeSelectionPurpose} "
                + $"RWY {candidate.RunwayDesignator} {candidate.SpotId} {GetPlayerRouteName(candidate, index, routeSelectionMode)} "
                + $"routeId={candidate.RouteId} entryUsage={candidate.RunwayEntryUsageId} access={GetRouteAccessSummary(candidate)} "
                + $"exitUsage={candidate.RunwayExitUsageId} segments={FormatRouteSegmentIds(candidate)} "
                + $"firstWaypoint={FormatRouteWaypoint(candidate, true)} lastWaypoint={FormatRouteWaypoint(candidate, false)} fallback=false");
            CloseRouteSelectionOverlay();
            Refresh();
        }

        private List<TaxiRouteCandidate> GetRouteSelectionCandidates(AircraftController selected, string mode)
        {
            var candidates = new List<TaxiRouteCandidate>();
            if (selected == null || selected.FlightData == null || gameManager == null || gameManager.Airport == null)
            {
                return candidates;
            }

            var data = selected.FlightData;
            var runwayDesignator = GetRouteSelectionRunwayDesignator(data);
            var sourceCandidates = data.OperationType == "Arrival"
                ? gameManager.Airport.GetArrivalTaxiRouteCandidates(data.SpotId, runwayDesignator)
                : gameManager.Airport.GetDepartureTaxiRouteCandidates(data.SpotId, runwayDesignator);

            foreach (var candidate in sourceCandidates)
            {
                var hasRelevantUsage = mode == RouteSelectionModeDeparture
                    ? !string.IsNullOrEmpty(candidate.RunwayEntryUsageId)
                    : mode == RouteSelectionModeArrivalExit
                        ? !string.IsNullOrEmpty(candidate.RunwayExitUsageId)
                        : candidate.SegmentIds.Count > 0;
                if (hasRelevantUsage)
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count > 0)
            {
                SortRouteSelectionCandidates(candidates, mode);
                return candidates;
            }

            foreach (var candidate in sourceCandidates)
            {
                candidates.Add(candidate);
            }

            SortRouteSelectionCandidates(candidates, mode);
            return candidates;
        }

        private void SortRouteSelectionCandidates(List<TaxiRouteCandidate> candidates, string mode)
        {
            if (mode != RouteSelectionModeDeparture && mode != RouteSelectionModeArrivalExit)
            {
                return;
            }

            candidates.Sort((first, second) => GetRouteCandidateRunwayPosition(first).CompareTo(GetRouteCandidateRunwayPosition(second)));
        }

        private float GetRouteCandidateRunwayPosition(TaxiRouteCandidate candidate)
        {
            if (!string.IsNullOrEmpty(candidate.RunwayExitUsageId))
            {
                var exitUsage = gameManager != null && gameManager.Airport != null ? gameManager.Airport.GetRunwayAccessUsage(candidate.RunwayExitUsageId) : null;
                return exitUsage != null ? exitUsage.RunwayPositionRatio : 1f;
            }

            if (HasRouteSegment(candidate, "A_CONNECTOR_MID_01"))
            {
                return 0.5f;
            }

            var usage = gameManager != null && gameManager.Airport != null ? gameManager.Airport.GetRunwayAccessUsage(candidate.RunwayEntryUsageId) : null;
            return usage != null ? usage.RunwayPositionRatio : 1f;
        }

        private string GetRouteSelectionModeForStrip(AircraftController selected)
        {
            if (selected == null || selected.FlightData == null)
            {
                return string.Empty;
            }

            if (selected.FlightData.OperationType == "Departure")
            {
                return selected.CurrentState == AircraftState.AtGate ? RouteSelectionModeDeparture : string.Empty;
            }

            if (selected.CurrentState == AircraftState.LandingRoll)
            {
                return RouteSelectionModeArrivalExit;
            }

            return selected.CurrentState == AircraftState.Waiting ? RouteSelectionModeArrivalSpotTaxi : string.Empty;
        }

        private string GetRouteSelectionRunwayDesignator(AircraftData data)
        {
            if (data.HasActiveRunwayDesignator)
            {
                return data.ActiveRunwayDesignator;
            }

            if (gameManager != null && gameManager.Airport != null)
            {
                foreach (var runwayDesignator in gameManager.Airport.GetPrimaryRunwayDirectionOptions())
                {
                    return runwayDesignator;
                }
            }

            return "18L";
        }

        private string GetRouteSelectionPurpose(AircraftController selected)
        {
            return selected != null && selected.FlightData != null && selected.FlightData.OperationType == "Arrival"
                ? "Arrival"
                : "Departure";
        }

        private string GetSelectedRouteId(AircraftData data, string purpose)
        {
            return purpose == "Arrival" ? data.SelectedArrivalRouteId : data.SelectedDepartureRouteId;
        }

        private string GetPlayerRouteOptionText(TaxiRouteCandidate candidate, int index, string mode)
        {
            var optionName = GetPlayerRouteName(candidate, index, mode);
            var description = GetRouteSelectionDescription(candidate, mode);
            return $"<size=22>{optionName}</size>\n<size=15>{description}</size>";
        }

        private bool IsRouteSelectionCandidateSelected(TaxiRouteCandidate candidate, string selectedRouteId)
        {
            return !string.IsNullOrEmpty(selectedRouteId) ? candidate.RouteId == selectedRouteId : candidate.IsDefault;
        }

        private string FormatRouteSegmentIds(TaxiRouteCandidate candidate)
        {
            var values = new List<string>();
            foreach (var segmentId in candidate.SegmentIds)
            {
                values.Add(segmentId);
            }

            return values.Count > 0 ? string.Join(", ", values.ToArray()) : "none";
        }

        private string FormatRouteWaypoint(TaxiRouteCandidate candidate, bool first)
        {
            if (candidate == null || candidate.Waypoints.Count == 0)
            {
                return "none";
            }

            var point = first ? candidate.Waypoints[0] : candidate.Waypoints[candidate.Waypoints.Count - 1];
            return $"({point.x:0.0}, {point.y:0.0}, {point.z:0.0})";
        }

        private string FormatRouteCandidateRouteIds(IReadOnlyList<TaxiRouteCandidate> candidates)
        {
            var values = new List<string>();
            foreach (var candidate in candidates)
            {
                values.Add(candidate.RouteId);
            }

            return values.Count > 0 ? string.Join(", ", values.ToArray()) : "none";
        }

        private string FormatRouteCandidateEntryUsageIds(IReadOnlyList<TaxiRouteCandidate> candidates)
        {
            var values = new List<string>();
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate.RunwayEntryUsageId))
                {
                    values.Add(candidate.RunwayEntryUsageId);
                }
            }

            return values.Count > 0 ? string.Join(", ", values.ToArray()) : "none";
        }

        private void LogRouteSelectionCandidates(AircraftController aircraft, string runwayDesignator)
        {
            var values = new List<string>();
            for (var index = 0; index < activeRouteSelectionCandidates.Count; index++)
            {
                var candidate = activeRouteSelectionCandidates[index];
                values.Add(
                    $"{GetPlayerRouteName(candidate, index, routeSelectionMode)} "
                    + $"routeId={candidate.RouteId} entryUsage={candidate.RunwayEntryUsageId} exitUsage={candidate.RunwayExitUsageId} "
                    + $"access={GetRouteAccessSummary(candidate)} segments={FormatRouteSegmentIds(candidate)} fallback=false");
            }

            Debug.Log(
                $"Route selection candidates: {aircraft.FlightNumber} mode={routeSelectionMode} "
                + $"runway={runwayDesignator} spot={aircraft.FlightData.SpotId} count={activeRouteSelectionCandidates.Count} "
                + string.Join(" | ", values.ToArray()));
        }

        private string GetRouteAccessSummary(TaxiRouteCandidate candidate)
        {
            var usageId = routeSelectionMode == RouteSelectionModeArrivalExit || string.IsNullOrEmpty(candidate.RunwayEntryUsageId)
                ? candidate.RunwayExitUsageId
                : candidate.RunwayEntryUsageId;
            var usage = gameManager != null && gameManager.Airport != null ? gameManager.Airport.GetRunwayAccessUsage(usageId) : null;
            var accessPoint = usage != null ? gameManager.Airport.GetRunwayAccessPoint(usage.AccessPointId) : null;
            if (usage == null || accessPoint == null)
            {
                return "none";
            }

            return $"{usage.UsageId}/{accessPoint.AccessPointId}/{accessPoint.ConnectedSegmentId}";
        }

        private bool HasRouteSegment(TaxiRouteCandidate candidate, string segmentId)
        {
            foreach (var candidateSegmentId in candidate.SegmentIds)
            {
                if (candidateSegmentId == segmentId)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetPlayerRouteName(TaxiRouteCandidate candidate, int index, string mode)
        {
            if (mode == RouteSelectionModeArrivalExit)
            {
                return $"Exit {(char)('A' + index)}";
            }

            return $"Route {(char)('A' + index)}";
        }

        private string GetRouteSelectionDescription(TaxiRouteCandidate candidate, string mode)
        {
            if (mode == RouteSelectionModeArrivalExit)
            {
                return GetRunwayExitPositionDescription(candidate);
            }

            if (mode == RouteSelectionModeDeparture)
            {
                return GetRunwayEntryPositionDescription(candidate);
            }

            return GetSpotTaxiRoutePositionDescription(candidate);
        }

        private string GetRouteSelectionOverlayTitle(string mode)
        {
            switch (mode)
            {
                case RouteSelectionModeDeparture:
                    return "タクシールートを選択";
                case RouteSelectionModeArrivalExit:
                    return "滑走路離脱誘導路を選択";
                case RouteSelectionModeArrivalSpotTaxi:
                    return "スポット経路を選択";
                default:
                    return "Route Selection";
            }
        }

        private string GetRouteSelectionHint(string mode)
        {
            switch (mode)
            {
                case RouteSelectionModeDeparture:
                    return "出発前にタクシールートを選択します。Route A/B/Cは表示名で、内部ではrouteIdを保存します。";
                case RouteSelectionModeArrivalExit:
                    return "着陸後に使う滑走路離脱誘導路を選択します。表示名ではなく、内部ではrunwayExitUsageIdを持つrouteIdを保存します。";
                case RouteSelectionModeArrivalSpotTaxi:
                    return "滑走路離脱後のスポットまでの候補を選択します。候補が一意の場合、この選択UIは表示しません。";
                default:
                    return string.Empty;
            }
        }

        private string GetRouteSelectionStripLabel(string mode, AircraftController selected)
        {
            switch (mode)
            {
                case RouteSelectionModeDeparture:
                    return "タクシールートを選択\nTaxi Route";
                case RouteSelectionModeArrivalExit:
                    return "離脱方式を選択\nSelect Exit";
                case RouteSelectionModeArrivalSpotTaxi:
                    return $"{selected.FlightData.SpotDisplayName}へ経路選択\nSelect Route";
                default:
                    return "Route選択\nSelect Route";
            }
        }

        private string GetRunwayEntryPositionDescription(TaxiRouteCandidate candidate)
        {
            if (HasRouteSegment(candidate, "A_CONNECTOR_MID_01"))
            {
                return "中央入口";
            }

            if (!string.IsNullOrEmpty(candidate.RunwayEntryUsageId) && candidate.RunwayEntryUsageId.Contains("_END"))
            {
                return GetAccessPhysicalSideDescription(candidate.RunwayEntryUsageId, "端入口");
            }

            return GetAccessPhysicalSideDescription(candidate.RunwayEntryUsageId, "入口");
        }

        private string GetRunwayExitPositionDescription(TaxiRouteCandidate candidate)
        {
            var typeLabel = IsRapidExitCandidate(candidate) ? "高速脱出誘導路" : "通常取付誘導路";
            return $"{typeLabel} / {GetAccessPhysicalSideDescription(candidate.RunwayExitUsageId, "離脱")}";
        }

        private string GetSpotTaxiRoutePositionDescription(TaxiRouteCandidate candidate)
        {
            var usageId = !string.IsNullOrEmpty(candidate.RunwayExitUsageId) ? candidate.RunwayExitUsageId : candidate.RunwayEntryUsageId;
            return GetAccessPhysicalSideDescription(usageId, "ルート");
        }

        private string GetAccessPhysicalSideDescription(string usageId, string suffix)
        {
            var usage = gameManager != null && gameManager.Airport != null ? gameManager.Airport.GetRunwayAccessUsage(usageId) : null;
            var accessPoint = usage != null ? gameManager.Airport.GetRunwayAccessPoint(usage.AccessPointId) : null;
            if (accessPoint == null)
            {
                return $"別の{suffix}";
            }

            switch (accessPoint.PhysicalSide)
            {
                case RunwayAccessPhysicalSide.Near18L:
                    return $"18L側{suffix}";
                case RunwayAccessPhysicalSide.MidRunway:
                    return $"中央{suffix}";
                case RunwayAccessPhysicalSide.Near36R:
                    return $"36R側{suffix}";
                default:
                    return $"別の{suffix}";
            }
        }

        private bool IsRapidExitCandidate(TaxiRouteCandidate candidate)
        {
            var usage = gameManager != null && gameManager.Airport != null ? gameManager.Airport.GetRunwayAccessUsage(candidate.RunwayExitUsageId) : null;
            if (usage == null)
            {
                return false;
            }

            return usage.AccessType == RunwayAccessType.RapidExit
                || usage.MaxExitSpeedLevel == RunwayExitSpeedLevel.Medium
                || usage.MaxExitSpeedLevel == RunwayExitSpeedLevel.High
                || usage.UsageId.Contains("_MID");
        }

        private void ApplyRouteSelectionOptionStyle(Button button, bool selected)
        {
            var colors = button.colors;
            colors.normalColor = selected ? new Color(0.22f, 0.5f, 0.64f, 0.98f) : new Color(0.14f, 0.52f, 0.22f, 0.95f);
            colors.highlightedColor = selected ? new Color(0.28f, 0.62f, 0.78f, 1f) : new Color(0.18f, 0.66f, 0.28f, 1f);
            colors.pressedColor = selected ? new Color(0.16f, 0.42f, 0.56f, 1f) : new Color(0.1f, 0.42f, 0.18f, 1f);
            button.colors = colors;
        }

        private void UpdateTutorialPanel()
        {
            var step = CurrentTutorialStep;
            tutorialPanel.SetActive(step != null);
            if (step == null)
            {
                ApplyTutorialHighlight(TutorialHighlight.None);
                return;
            }

            tutorialText.text = step.Message;
            tutorialNextButton.gameObject.SetActive(step.WaitMode == TutorialWaitMode.Info);
            ApplyTutorialHighlight(step.Highlight);
        }

        public void AddControllerCommandLog(AircraftController aircraft, AircraftCommand command)
        {
            var phrase = CommandPhraseCatalog.Get(command);
            if (phrase == null)
            {
                return;
            }

            commandLogEntries.Add(BuildAtcCommunicationLogEntry(aircraft, phrase));
            while (commandLogEntries.Count > 1)
            {
                commandLogEntries.RemoveAt(0);
            }

            Refresh();
        }

        private string BuildAtcCommunicationLogEntry(AircraftController aircraft, CommandPhrase phrase)
        {
            var callsign = aircraft != null ? aircraft.FlightNumber : "航空機";
            var controllerText = FormatAtcPhraseText(aircraft, phrase.JapaneseControllerText);
            var readbackText = FormatAtcPhraseText(aircraft, phrase.JapanesePilotReadbackText);
            return $"管制官：{controllerText}\n{callsign}：{readbackText}";
        }

        private string FormatAtcPhraseText(AircraftController aircraft, string phraseText)
        {
            if (aircraft == null)
            {
                return phraseText;
            }

            var runwayLabel = GetRadioRunwayLabel(aircraft);
            var spotLabel = aircraft.FlightData != null ? aircraft.FlightData.SpotDisplayName : "SPOT";
            return phraseText
                .Replace("{CALLSIGN}", aircraft.FlightNumber)
                .Replace("{RUNWAY}", runwayLabel)
                .Replace("{SPOT}", spotLabel)
                .Replace("AJJ101", aircraft.FlightNumber)
                .Replace("AJJ202", aircraft.FlightNumber)
                .Replace("A滑走路", runwayLabel)
                .Replace("SPOT 01", spotLabel);
        }

        private string GetRadioRunwayLabel(AircraftController aircraft)
        {
            if (aircraft != null && aircraft.FlightData != null && aircraft.FlightData.HasActiveRunwayDesignator)
            {
                return $"RWY{aircraft.FlightData.ActiveRunwayDesignator}";
            }

            return "A滑走路";
        }

        private void UpdateCommandLogPanel()
        {
            var hasLogs = commandLogEntries.Count > 0;
            guidePanel.SetActive(hasLogs || CurrentTutorialStep == null);
            guideText.text = hasLogs
                ? $"管制交信ログ\n{string.Join("\n\n", commandLogEntries)}"
                : "管制交信ログ\n指示を出すと、管制官の発話とパイロット復唱が日本語で記録されます。";
        }

        private string GetCommandStatusText(AircraftController selected, AircraftCommand? recommended)
        {
            if (selected == null)
            {
                return "航空機を選択";
            }

            var step = CurrentTutorialStep;
            if (step != null && step.WaitForCommand)
            {
                return "説明に沿って操作";
            }

            return "現在は監視";
        }

        private void BuildFlightStripPanel()
        {
            flightStripPanel = CreatePanel("Flight Strip Panel", hudRoot.transform, new Vector2(0f, 1f), new Vector2(340f, 560f), AnchorPreset.TopLeft, new Vector2(24f, -164f));
            flightStripPanel.GetComponent<Image>().color = new Color(0.025f, 0.032f, 0.04f, 0.82f);
            CreateText("Strip Title", flightStripPanel.transform, "STRIPS", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-122f, 248f), new Vector2(76f, 28f));
            CreateText("Arrival Header", flightStripPanel.transform, "到着  ARRIVAL", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-78f, 210f), new Vector2(220f, 28f));
            CreateText("Departure Header", flightStripPanel.transform, "出発  DEPARTURE", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-78f, -18f), new Vector2(220f, 28f));
            controlSectorSummaryText = CreateText("Control Sector Summary", flightStripPanel.transform, string.Empty, 13, FontStyle.Bold, TextAnchor.UpperLeft, new Vector2(0f, -246f), new Vector2(292f, 48f));
            CreateFlightStripButton("AJJ101", new Vector2(0f, 156f));
            CreateFlightStripButton("AJJ202", new Vector2(0f, -72f));

            stripCommandPopup = CreatePanel("Strip Command Popup", flightStripPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(292f, 96f));
            stripCommandPopup.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.032f, 0.94f);
            CreateStripCommandOptionButton(0);
            CreateStripCommandOptionButton(1);
            stripCommandPopup.SetActive(false);
        }

        private void BuildMiniMapPanel()
        {
            minimapPanel = CreatePanel("Mini Map Panel", hudRoot.transform, new Vector2(1f, 1f), new Vector2(420f, 280f), AnchorPreset.TopRight, new Vector2(-24f, -22f));
            minimapPanel.GetComponent<Image>().color = new Color(0.015f, 0.025f, 0.032f, 0.82f);
            minimapPanel.GetComponent<Image>().raycastTarget = false;
            CreateText("Mini Map Title", minimapPanel.transform, "AIRPORT MAP", 20, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-114f, 116f), new Vector2(170f, 30f));
            CreateText("Mini Map Runway Header", minimapPanel.transform, "A RWY 18L / 36R", 15, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(112f, 116f), new Vector2(160f, 28f));

            var contentObject = new GameObject("Mini Map Content");
            contentObject.transform.SetParent(minimapPanel.transform, false);
            minimapContent = contentObject.AddComponent<RectTransform>();
            ApplyAnchor(minimapContent, AnchorPreset.Center);
            minimapContent.sizeDelta = new Vector2(MiniMapWidth, MiniMapHeight);
            minimapContent.anchoredPosition = new Vector2(0f, -22f);

            DrawAirportMapBase(minimapContent, new Vector2(MiniMapWidth, MiniMapHeight), "Mini Map", true, true, true, true);
        }

        private void BuildRunwaySelectionOverlay()
        {
            runwaySelectionOverlay = CreatePanel("Runway Selection Overlay", hudRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(1120f, 680f));
            runwaySelectionOverlay.GetComponent<Image>().color = new Color(0.01f, 0.018f, 0.025f, 0.96f);
            runwaySelectionTitleText = CreateText("Runway Selection Title", runwaySelectionOverlay.transform, "滑走路を選択", 27, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-384f, 292f), new Vector2(470f, 52f));
            CreateButton("Runway Selection Close", runwaySelectionOverlay.transform, "閉じる", new Vector2(492f, 292f), new Vector2(112f, 42f), 16)
                .onClick.AddListener(CloseRunwaySelectionOverlay);

            var mapPanel = CreatePanel("Runway Selection Map", runwaySelectionOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(830f, 505f), AnchorPreset.Center, new Vector2(-116f, 24f));
            mapPanel.GetComponent<Image>().color = new Color(0.035f, 0.075f, 0.085f, 0.96f);

            runwaySelectionMapContent = new GameObject("Runway Selection Map Content");
            runwaySelectionMapContent.transform.SetParent(mapPanel.transform, false);
            var mapRect = runwaySelectionMapContent.AddComponent<RectTransform>();
            ApplyAnchor(mapRect, AnchorPreset.Center);
            mapRect.sizeDelta = new Vector2(OverlayMapWidth, OverlayMapHeight);
            mapRect.anchoredPosition = Vector2.zero;

            DrawAirportMapBase(runwaySelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Runway Map", false, true, true, true);
            runwaySelectionRunwayHighlight = CreateAirportMapBlock(runwaySelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Runway Map Selected Direction", new Vector3(3f, 0f, 0f), new Vector2(32.5f, 0.7f), new Color(1f, 0.86f, 0.18f, 0.28f), false);
            runwaySelection36RMarker = CreateAirportMapBlock(runwaySelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Runway Map 36R Marker", new Vector3(-13.25f, 0f, 1.8f), new Vector2(7.8f, 1.9f), new Color(1f, 0.86f, 0.18f, 0.18f), false);
            runwaySelection18LMarker = CreateAirportMapBlock(runwaySelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Runway Map 18L Marker", new Vector3(19.25f, 0f, 1.8f), new Vector2(7.8f, 1.9f), new Color(1f, 0.86f, 0.18f, 0.18f), false);
            runwaySelectionStatusText = CreateAirportMapText("Runway Selection Status", runwaySelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), string.Empty, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(3f, 0f, 6.9f), new Vector2(240f, 20f), Vector2.zero, false);

            var button18L = CreateButton("Runway Selection 18L", runwaySelectionOverlay.transform, "RWY 18L\n18L側を使用", new Vector2(424f, 112f), new Vector2(220f, 86f), 19);
            runwaySelection18LButtonText = button18L.GetComponentInChildren<Text>();
            button18L.onClick.AddListener(() => SelectRunwayFromOverlay("18L"));
            AddRunwaySelectionHoverTrigger(button18L, "18L");
            var button36R = CreateButton("Runway Selection 36R", runwaySelectionOverlay.transform, "RWY 36R\n36R側を使用", new Vector2(424f, 2f), new Vector2(220f, 86f), 19);
            runwaySelection36RButtonText = button36R.GetComponentInChildren<Text>();
            button36R.onClick.AddListener(() => SelectRunwayFromOverlay("36R"));
            AddRunwaySelectionHoverTrigger(button36R, "36R");

            runwaySelectionHintText = CreateText("Runway Selection Hint", runwaySelectionOverlay.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(-116f, -298f), new Vector2(830f, 50f));
            runwaySelectionOverlay.SetActive(false);
        }

        private Image CreateRunwayMapBlock(string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            return CreateMapBlock(name, runwaySelectionMapContent.transform, anchoredPosition, size, color);
        }

        private void AddRunwaySelectionHoverTrigger(Button button, string runwayDesignator)
        {
            var trigger = button.gameObject.AddComponent<EventTrigger>();
            var pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener(_ => UpdateRunwaySelectionVisual(runwayDesignator));
            trigger.triggers.Add(pointerEnter);
        }

        private void UpdateRunwaySelectionVisual(string runwayDesignator)
        {
            if (runwaySelectionRunwayHighlight == null || runwaySelection18LMarker == null || runwaySelection36RMarker == null || runwaySelectionStatusText == null)
            {
                return;
            }

            var is18L = runwayDesignator == "18L";
            var is36R = runwayDesignator == "36R";
            runwaySelection18LMarker.color = is18L ? new Color(1f, 0.86f, 0.18f, 0.76f) : new Color(0.74f, 0.82f, 0.86f, 0.12f);
            runwaySelection36RMarker.color = is36R ? new Color(1f, 0.86f, 0.18f, 0.76f) : new Color(0.74f, 0.82f, 0.86f, 0.12f);
            runwaySelectionRunwayHighlight.color = new Color(1f, 0.86f, 0.18f, is18L || is36R ? 0.32f : 0.14f);
            runwaySelectionStatusText.text = string.IsNullOrEmpty(runwayDesignator)
                ? string.Empty
                : $"選択候補 RWY {runwayDesignator}";
        }

        private void BuildRouteSelectionOverlay()
        {
            routeSelectionOverlay = CreatePanel("Route Selection Overlay", hudRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(1180f, 720f));
            routeSelectionOverlay.GetComponent<Image>().color = new Color(0.01f, 0.018f, 0.025f, 0.96f);
            routeSelectionTitleText = CreateText("Route Selection Title", routeSelectionOverlay.transform, "タクシールートを選択", 27, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-420f, 312f), new Vector2(500f, 52f));
            CreateButton("Route Selection Close", routeSelectionOverlay.transform, "閉じる", new Vector2(520f, 312f), new Vector2(116f, 42f), 16)
                .onClick.AddListener(CloseRouteSelectionOverlay);

            var mapPanel = CreatePanel("Route Selection Map", routeSelectionOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(830f, 505f), AnchorPreset.Center, new Vector2(-150f, 24f));
            mapPanel.GetComponent<Image>().color = new Color(0.035f, 0.075f, 0.085f, 0.96f);

            routeSelectionMapContent = new GameObject("Route Selection Map Content");
            routeSelectionMapContent.transform.SetParent(mapPanel.transform, false);
            var mapRect = routeSelectionMapContent.AddComponent<RectTransform>();
            ApplyAnchor(mapRect, AnchorPreset.Center);
            mapRect.sizeDelta = new Vector2(OverlayMapWidth, OverlayMapHeight);
            mapRect.anchoredPosition = Vector2.zero;

            DrawAirportMapBase(routeSelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Route Map", false, true, true, false);
            routeSelectionRunwayHighlight = CreateAirportMapBlock(routeSelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Route Map Selected Runway Highlight", new Vector3(3f, 0f, 0f), new Vector2(32.5f, 0.7f), new Color(1f, 0.86f, 0.18f, 0.28f), false);
            routeSelection36RMarker = CreateAirportMapBlock(routeSelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Route Map Selected 36R Marker", new Vector3(-13.25f, 0f, 1.8f), new Vector2(7.8f, 1.9f), new Color(1f, 0.86f, 0.18f, 0.18f), false);
            routeSelection18LMarker = CreateAirportMapBlock(routeSelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), "Route Map Selected 18L Marker", new Vector3(19.25f, 0f, 1.8f), new Vector2(7.8f, 1.9f), new Color(1f, 0.86f, 0.18f, 0.18f), false);
            routeSelectionRunwayStatusText = CreateAirportMapText("Route Map Selected Runway Text", routeSelectionMapContent.transform, new Vector2(OverlayMapWidth, OverlayMapHeight), string.Empty, 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(3f, 0f, 6.9f), new Vector2(220f, 20f), Vector2.zero, false);

            routeSelectionHintText = CreateText("Route Selection Hint", routeSelectionOverlay.transform, string.Empty, 16, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(-150f, -318f), new Vector2(830f, 54f));

            for (var index = 0; index < 3; index++)
            {
                CreateRouteSelectionOptionButton(index);
            }

            routeSelectionOverlay.SetActive(false);
        }

        private Image CreateRouteMapBlock(string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            return CreateMapBlock(name, routeSelectionMapContent.transform, anchoredPosition, size, color);
        }

        private void UpdateRouteSelectionRunwayVisual(string runwayDesignator)
        {
            if (routeSelectionRunwayHighlight == null || routeSelection18LMarker == null || routeSelection36RMarker == null || routeSelectionRunwayStatusText == null)
            {
                return;
            }

            routeSelectionRunwayHighlight.gameObject.SetActive(!string.IsNullOrEmpty(runwayDesignator));
            routeSelection18LMarker.gameObject.SetActive(true);
            routeSelection36RMarker.gameObject.SetActive(true);

            var is18L = runwayDesignator == "18L";
            var is36R = runwayDesignator == "36R";
            routeSelection18LMarker.color = is18L ? new Color(1f, 0.86f, 0.18f, 0.72f) : new Color(0.74f, 0.82f, 0.86f, 0.12f);
            routeSelection36RMarker.color = is36R ? new Color(1f, 0.86f, 0.18f, 0.72f) : new Color(0.74f, 0.82f, 0.86f, 0.12f);
            routeSelectionRunwayHighlight.color = new Color(1f, 0.86f, 0.18f, is18L || is36R ? 0.3f : 0.14f);
            routeSelectionRunwayStatusText.text = string.IsNullOrEmpty(runwayDesignator)
                ? string.Empty
                : $"選択RWY {runwayDesignator}";
            routeSelectionRunwayStatusText.gameObject.SetActive(!string.IsNullOrEmpty(runwayDesignator));
        }

        private void ClearRouteSelectionRunwayVisual()
        {
            if (routeSelectionRunwayHighlight != null)
            {
                routeSelectionRunwayHighlight.gameObject.SetActive(false);
            }

            if (routeSelection18LMarker != null)
            {
                routeSelection18LMarker.gameObject.SetActive(false);
            }

            if (routeSelection36RMarker != null)
            {
                routeSelection36RMarker.gameObject.SetActive(false);
            }

            if (routeSelectionRunwayStatusText != null)
            {
                routeSelectionRunwayStatusText.gameObject.SetActive(false);
            }
        }

        private void UpdateRouteSelectionEntryLabels(string mode)
        {
            if ((mode != RouteSelectionModeDeparture && mode != RouteSelectionModeArrivalExit) || gameManager == null || gameManager.Airport == null)
            {
                ClearRouteSelectionEntryLabels();
                return;
            }

            var usedUsageIds = new List<string>();
            var labelIndex = 0;
            foreach (var candidate in activeRouteSelectionCandidates)
            {
                var usageId = mode == RouteSelectionModeArrivalExit ? candidate.RunwayExitUsageId : candidate.RunwayEntryUsageId;
                if (string.IsNullOrEmpty(usageId) || usedUsageIds.Contains(usageId))
                {
                    continue;
                }

                var usage = gameManager.Airport.GetRunwayAccessUsage(usageId);
                var accessPoint = usage != null ? gameManager.Airport.GetRunwayAccessPoint(usage.AccessPointId) : null;
                if (usage == null || accessPoint == null)
                {
                    continue;
                }

                EnsureRouteSelectionEntryLabelCount(labelIndex + 1);
                var label = routeSelectionEntryLabelTexts[labelIndex++];
                label.gameObject.SetActive(true);
                label.text = mode == RouteSelectionModeArrivalExit ? GetRunwayExitPositionDescription(candidate) : GetRunwayEntryPositionDescription(candidate);
                label.color = new Color(1f, 0.92f, 0.56f, 0.96f);
                label.transform.SetAsLastSibling();
                var rectTransform = label.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = RouteWorldToMap(accessPoint.Position) + new Vector2(0f, -30f);
                usedUsageIds.Add(usageId);
            }

            for (var index = labelIndex; index < routeSelectionEntryLabelTexts.Count; index++)
            {
                routeSelectionEntryLabelTexts[index].gameObject.SetActive(false);
            }
        }

        private void EnsureRouteSelectionEntryLabelCount(int labelCount)
        {
            while (routeSelectionEntryLabelTexts.Count < labelCount)
            {
                var text = CreateText(
                    $"Route Selection Entry Label {routeSelectionEntryLabelTexts.Count + 1}",
                    routeSelectionMapContent.transform,
                    string.Empty,
                    12,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter,
                    Vector2.zero,
                    new Vector2(96f, 20f));
                text.raycastTarget = false;
                routeSelectionEntryLabelTexts.Add(text);
            }
        }

        private void ClearRouteSelectionEntryLabels()
        {
            foreach (var text in routeSelectionEntryLabelTexts)
            {
                text.gameObject.SetActive(false);
            }
        }

        private void CreateRouteSelectionOptionButton(int index)
        {
            var button = CreateButton($"Route Selection Option {index + 1}", routeSelectionOverlay.transform, string.Empty, GetRouteSelectionOptionPosition(index, 3), new Vector2(258f, 78f), 16);
            var optionIndex = index;
            button.onClick.AddListener(() => SelectRouteSelectionCandidate(optionIndex));
            AddRouteSelectionHoverTrigger(button, optionIndex);
            routeSelectionOptionButtons.Add(button);
            routeSelectionOptionTexts.Add(button.GetComponentInChildren<Text>());
        }

        private void EnsureRouteSelectionOptionButtonCount(int candidateCount)
        {
            while (routeSelectionOptionButtons.Count < candidateCount)
            {
                CreateRouteSelectionOptionButton(routeSelectionOptionButtons.Count);
            }
        }

        private Vector2 GetRouteSelectionOptionPosition(int index, int candidateCount)
        {
            var spacing = candidateCount > 3 ? 86f : 104f;
            return new Vector2(430f, 168f - index * spacing);
        }

        private void AddRouteSelectionHoverTrigger(Button button, int optionIndex)
        {
            var trigger = button.gameObject.AddComponent<EventTrigger>();
            var pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener(_ => ShowRouteSelectionHighlight(optionIndex));
            trigger.triggers.Add(pointerEnter);
        }

        private int GetInitialRouteSelectionHighlightIndex(string selectedRouteId)
        {
            if (!string.IsNullOrEmpty(selectedRouteId))
            {
                for (var index = 0; index < activeRouteSelectionCandidates.Count; index++)
                {
                    if (activeRouteSelectionCandidates[index].RouteId == selectedRouteId)
                    {
                        return index;
                    }
                }
            }

            for (var index = 0; index < activeRouteSelectionCandidates.Count; index++)
            {
                if (activeRouteSelectionCandidates[index].IsDefault)
                {
                    return index;
                }
            }

            return activeRouteSelectionCandidates.Count > 0 ? 0 : -1;
        }

        private void ShowRouteSelectionPreview()
        {
            var segmentCount = 0;
            foreach (var candidate in activeRouteSelectionCandidates)
            {
                segmentCount += Mathf.Max(0, candidate.Waypoints.Count - 1);
            }

            EnsureRouteSelectionPreviewSegmentCount(segmentCount);
            var segmentIndex = 0;
            foreach (var candidate in activeRouteSelectionCandidates)
            {
                for (var waypointIndex = 0; waypointIndex < candidate.Waypoints.Count - 1; waypointIndex++)
                {
                    var image = routeSelectionPreviewSegments[segmentIndex++];
                    image.gameObject.SetActive(true);
                    image.color = new Color(0.78f, 0.9f, 1f, 0.24f);
                    ApplyRouteSelectionLineSegment(
                        image.GetComponent<RectTransform>(),
                        RouteWorldToMap(candidate.Waypoints[waypointIndex]),
                        RouteWorldToMap(candidate.Waypoints[waypointIndex + 1]),
                        4f);
                }
            }

            for (var index = segmentIndex; index < routeSelectionPreviewSegments.Count; index++)
            {
                routeSelectionPreviewSegments[index].gameObject.SetActive(false);
            }
        }

        private void ShowRouteSelectionHighlight(int candidateIndex)
        {
            if (candidateIndex < 0 || candidateIndex >= activeRouteSelectionCandidates.Count)
            {
                ClearRouteSelectionHighlight();
                return;
            }

            var waypoints = activeRouteSelectionCandidates[candidateIndex].Waypoints;
            var segmentCount = Mathf.Max(0, waypoints.Count - 1);
            EnsureRouteSelectionHighlightSegmentCount(segmentCount);
            SetRouteSelectionHighlightedOption(candidateIndex);

            for (var index = 0; index < routeSelectionHighlightSegments.Count; index++)
            {
                var image = routeSelectionHighlightSegments[index];
                var active = index < segmentCount;
                image.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                image.color = new Color(1f, 0.86f, 0.18f, 0.96f);
                image.transform.SetAsLastSibling();
                ApplyRouteSelectionLineSegment(
                    image.GetComponent<RectTransform>(),
                    RouteWorldToMap(waypoints[index]),
                    RouteWorldToMap(waypoints[index + 1]),
                    9f);
            }
        }

        private void EnsureRouteSelectionPreviewSegmentCount(int segmentCount)
        {
            while (routeSelectionPreviewSegments.Count < segmentCount)
            {
                var image = CreateRouteMapBlock(
                    $"Route Selection Preview {routeSelectionPreviewSegments.Count + 1}",
                    Vector2.zero,
                    new Vector2(20f, 4f),
                    new Color(0.78f, 0.9f, 1f, 0.24f));
                image.transform.SetAsLastSibling();
                routeSelectionPreviewSegments.Add(image);
            }
        }

        private void EnsureRouteSelectionHighlightSegmentCount(int segmentCount)
        {
            while (routeSelectionHighlightSegments.Count < segmentCount)
            {
                var image = CreateRouteMapBlock(
                    $"Route Selection Highlight {routeSelectionHighlightSegments.Count + 1}",
                    Vector2.zero,
                    new Vector2(20f, 8f),
                    new Color(1f, 0.86f, 0.18f, 0.9f));
                image.transform.SetAsLastSibling();
                routeSelectionHighlightSegments.Add(image);
            }
        }

        private void ApplyRouteSelectionLineSegment(RectTransform rectTransform, Vector2 start, Vector2 end, float thickness)
        {
            var delta = end - start;
            rectTransform.anchoredPosition = (start + end) * 0.5f;
            rectTransform.sizeDelta = new Vector2(Mathf.Max(12f, delta.magnitude), thickness);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private Vector2 RouteWorldToMap(Vector3 worldPosition)
        {
            return ConvertAirportWorldToMapPoint(worldPosition, new Vector2(OverlayMapWidth, OverlayMapHeight), false);
        }

        private void ClearRouteSelectionPreview()
        {
            foreach (var image in routeSelectionPreviewSegments)
            {
                image.gameObject.SetActive(false);
            }
        }

        private void ClearRouteSelectionHighlight()
        {
            foreach (var image in routeSelectionHighlightSegments)
            {
                image.gameObject.SetActive(false);
            }
        }

        private void SetRouteSelectionHighlightedOption(int highlightedIndex)
        {
            var selectedRouteId = routeSelectionAircraft != null && routeSelectionAircraft.FlightData != null
                ? GetSelectedRouteId(routeSelectionAircraft.FlightData, routeSelectionPurpose)
                : string.Empty;
            for (var index = 0; index < routeSelectionOptionButtons.Count; index++)
            {
                if (!routeSelectionOptionButtons[index].gameObject.activeSelf || index >= activeRouteSelectionCandidates.Count)
                {
                    continue;
                }

                var selected = index == highlightedIndex || IsRouteSelectionCandidateSelected(activeRouteSelectionCandidates[index], selectedRouteId);
                ApplyRouteSelectionOptionStyle(routeSelectionOptionButtons[index], selected);
            }
        }

        private void DrawAirportMapBase(Transform parent, Vector2 mapSize, string prefix, bool includeBRunway, bool includeSpots, bool includeTerminal, bool includeEntryLabels)
        {
            var round = prefix == "Mini Map";
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Sea Background", new Vector3((MiniMapWorldMinX + MiniMapWorldMaxX) * 0.5f, 0f, (MiniMapWorldMinZ + MiniMapWorldMaxZ) * 0.5f), new Vector2(MiniMapWorldMaxX - MiniMapWorldMinX, MiniMapWorldMaxZ - MiniMapWorldMinZ), new Color(0.02f, 0.28f, 0.42f, 0.82f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Main Airport Island", new Vector3(3f, 0f, -8.2f), new Vector2(68f, 18.8f), new Color(0.08f, 0.19f, 0.12f, 0.88f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Central Wedge Base", new Vector3(4.3f, 0f, 2.7f), new Vector2(16.8f, 2.8f), new Color(0.1f, 0.23f, 0.14f, 0.9f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Central Wedge Nose", new Vector3(6.6f, 0f, 5.1f), new Vector2(9.6f, 2.9f), new Color(0.1f, 0.23f, 0.14f, 0.9f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Right Connector Island", new Vector3(17.1f, 0f, 5.1f), new Vector2(4.6f, 10.6f), new Color(0.1f, 0.22f, 0.14f, 0.9f));

            if (includeBRunway)
            {
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Future Runway Island", new Vector3(3f, 0f, 10.2f), new Vector2(36.5f, 5.4f), new Color(0.11f, 0.21f, 0.13f, 0.88f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Runway B", new Vector3(3f, 0f, 10.2f), new Vector2(29.2f, 2.8f), new Color(0.31f, 0.34f, 0.35f, 0.78f));
                CreateAirportMapText($"{prefix} Runway B Label", parent, mapSize, "B RWY 18R / 36L", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(3f, 0f, 12.2f), new Vector2(110f, 18f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Runway B 36L End", parent, mapSize, "36L", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(-11.6f, 0f, 13.35f), new Vector2(44f, 18f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Runway B 18R End", parent, mapSize, "18R", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(17.6f, 0f, 13.35f), new Vector2(44f, 18f), Vector2.zero, round);
            }

            if (includeTerminal)
            {
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Fighter Base Area", new Vector3(-13.5f, 0f, -9.7f), new Vector2(12.5f, 5.2f), new Color(0.25f, 0.31f, 0.24f, 0.96f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Support Base Area", new Vector3(-1.6f, 0f, -10f), new Vector2(11.2f, 5.9f), new Color(0.29f, 0.34f, 0.29f, 0.96f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Passenger Apron", new Vector3(17.1f, 0f, -10.1f), new Vector2(29.2f, 9.35f), new Color(0.35f, 0.39f, 0.38f, 0.92f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} DOM Terminal", new Vector3(12.5f, 0f, -15.35f), new Vector2(18.2f, 2.35f), new Color(0.58f, 0.64f, 0.62f, 0.98f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} INTL Terminal", new Vector3(25.7f, 0f, -15.35f), new Vector2(8.4f, 2.55f), new Color(0.64f, 0.67f, 0.65f, 0.98f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} DOM Finger West", new Vector3(8.6f, 0f, -12.35f), new Vector2(1.55f, 5.25f), new Color(0.58f, 0.64f, 0.62f, 0.98f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} DOM Finger East", new Vector3(16.3f, 0f, -12.35f), new Vector2(1.55f, 5.25f), new Color(0.58f, 0.64f, 0.62f, 0.98f));
                DrawAirportMapBlock(parent, mapSize, round, $"{prefix} INTL Pier", new Vector3(25.7f, 0f, -12.55f), new Vector2(5.9f, 2.05f), new Color(0.64f, 0.67f, 0.65f, 0.98f));
            }

            var runwayA = DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Runway A", new Vector3(3f, 0f, 0f), new Vector2(32.5f, 2.35f), new Color(0.38f, 0.4f, 0.42f, 0.98f));
            if (prefix == "Mini Map")
            {
                minimapRunwayImage = runwayA;
            }

            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Taxiway Main", new Vector3(2.7f, 0f, -5f), new Vector2(52f, 1.24f), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Taxiway 36R End", new Vector3(-10.8f, 0f, -2.5f), new Vector2(1.7f, 5f), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Taxiway 36R Near", new Vector3(-7f, 0f, -2.5f), new Vector2(1.7f, 5f), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapLineSegment(parent, mapSize, round, $"{prefix} Rapid Exit 36R Side", new Vector3(-2.6f, 0f, 0f), new Vector3(0.2f, 0f, -5f), 1.55f, new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapLineSegment(parent, mapSize, round, $"{prefix} Rapid Exit 18L Side", new Vector3(8f, 0f, 0f), new Vector3(5.2f, 0f, -5f), 1.55f, new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Taxiway 18L Near", new Vector3(12f, 0f, -2.5f), new Vector2(1.7f, 5f), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Taxiway 18L End", new Vector3(15.2f, 0f, -2.5f), new Vector2(1.7f, 5f), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Hold A 36R", new Vector3(-7f, 0f, -3f), new Vector2(3.2f, 2.4f), new Color(0.92f, 0.72f, 0.16f, 0.72f));
            DrawAirportMapBlock(parent, mapSize, round, $"{prefix} Hold A 18L", new Vector3(12f, 0f, -3f), new Vector2(3.2f, 2.4f), new Color(0.92f, 0.72f, 0.16f, 0.58f));

            if (includeSpots)
            {
                DrawAirportMapSpots(parent, mapSize, round, prefix);
            }

            CreateAirportMapText($"{prefix} Runway A Label", parent, mapSize, "A RWY 18L / 36R", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(3f, 0f, 1.8f), new Vector2(112f, 18f), Vector2.zero, round);
            CreateAirportMapText($"{prefix} Runway A 36R End", parent, mapSize, "36R", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(-13.25f, 0f, 1.8f), new Vector2(44f, 18f), Vector2.zero, round);
            CreateAirportMapText($"{prefix} Runway A 18L End", parent, mapSize, "18L", 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(19.25f, 0f, 1.8f), new Vector2(44f, 18f), Vector2.zero, round);

            if (includeEntryLabels)
            {
                CreateAirportMapText($"{prefix} Entry 36R End", parent, mapSize, "36R端", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(-10.8f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Entry 36R Near", parent, mapSize, "36R側", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(-7f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Rapid 36R Side", parent, mapSize, "高速", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(0.2f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Rapid 18L Side", parent, mapSize, "高速", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(5.2f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Entry 18L Near", parent, mapSize, "18L側", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(12f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
                CreateAirportMapText($"{prefix} Entry 18L End", parent, mapSize, "18L端", 9, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector3(15.2f, 0f, -6.7f), new Vector2(48f, 14f), Vector2.zero, round);
            }
        }

        private Image DrawAirportMapBlock(Transform parent, Vector2 mapSize, bool round, string name, Vector3 worldCenter, Vector2 worldSize, Color color)
        {
            return CreateAirportMapBlock(parent, mapSize, name, worldCenter, worldSize, color, round);
        }

        private Image DrawAirportMapLineSegment(Transform parent, Vector2 mapSize, bool round, string name, Vector3 worldStart, Vector3 worldEnd, float worldThickness, Color color)
        {
            var start = ConvertAirportWorldToMapPoint(worldStart, mapSize, round);
            var end = ConvertAirportWorldToMapPoint(worldEnd, mapSize, round);
            var line = CreateMapBlock(name, parent, Vector2.zero, new Vector2(20f, 4f), color);
            var rectTransform = line.GetComponent<RectTransform>();
            var thickness = ConvertAirportWorldSizeToMapSize(new Vector2(worldThickness, worldThickness), mapSize, round).y;
            ApplyRouteSelectionLineSegment(rectTransform, start, end, Mathf.Max(4f, thickness));
            return line;
        }

        private Image CreateAirportMapBlock(Transform parent, Vector2 mapSize, string name, Vector3 worldCenter, Vector2 worldSize, Color color, bool round)
        {
            return CreateMapBlock(
                name,
                parent,
                ConvertAirportWorldToMapPoint(worldCenter, mapSize, round),
                ConvertAirportWorldSizeToMapSize(worldSize, mapSize, round),
                color);
        }

        private Text CreateAirportMapText(string name, Transform parent, Vector2 mapSize, string content, int size, FontStyle style, TextAnchor anchor, Vector3 worldPosition, Vector2 uiSize, Vector2 offset, bool round)
        {
            return CreateText(
                name,
                parent,
                content,
                size,
                style,
                anchor,
                ConvertAirportWorldToMapPoint(worldPosition, mapSize, round) + offset,
                uiSize);
        }

        private void DrawAirportMapSpots(Transform parent, Vector2 mapSize, bool round, string prefix)
        {
            var airport = gameManager != null ? gameManager.Airport : null;
            if (airport == null || airport.SpotDefinitions.Count == 0)
            {
                DrawAirportMapSpot(parent, mapSize, round, prefix, "SPOT 01", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(6.3f, 0f, -8.35f), new Vector3(2.35f, 0f, 3.45f));
                DrawAirportMapSpot(parent, mapSize, round, prefix, "SPOT 02", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(10.8f, 0f, -7.95f), new Vector3(2.35f, 0f, 3.45f));
                DrawAirportMapSpot(parent, mapSize, round, prefix, "SPOT 03", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(17.1f, 0f, -8.15f), new Vector3(2.35f, 0f, 3.45f));
                DrawAirportMapSpot(parent, mapSize, round, prefix, "SPOT 04", AirportSpotArea.INTL, AircraftSizeClass.Widebody, new Vector3(26.2f, 0f, -8.65f), new Vector3(3.7f, 0f, 4.85f));
                return;
            }

            foreach (var spotDefinition in airport.SpotDefinitions)
            {
                DrawAirportMapSpot(parent, mapSize, round, prefix, spotDefinition.TutorialId, spotDefinition.Area, spotDefinition.AircraftSizeClass, spotDefinition.Position, spotDefinition.StandScale);
            }
        }

        private void DrawAirportMapSpot(Transform parent, Vector2 mapSize, bool round, string prefix, string label, AirportSpotArea area, AircraftSizeClass aircraftSizeClass, Vector3 worldPosition, Vector3 standScale)
        {
            DrawAirportMapStandLeadIn(parent, mapSize, round, prefix, label, worldPosition);

            var worldSize = new Vector2(standScale.x, standScale.z);
            var uiSize = ConvertAirportWorldSizeToMapSize(worldSize, mapSize, round);
            uiSize = new Vector2(Mathf.Max(uiSize.x, aircraftSizeClass == AircraftSizeClass.Widebody ? 22f : 18f), Mathf.Max(uiSize.y, aircraftSizeClass == AircraftSizeClass.Widebody ? 18f : 14f));
            CreateMapBlock($"{prefix} {label}", parent, ConvertAirportWorldToMapPoint(worldPosition, mapSize, round), uiSize, GetMiniMapSpotColor(area, aircraftSizeClass));
            CreateAirportMapText($"{prefix} {label} Label", parent, mapSize, label.Replace("SPOT ", "S"), 11, FontStyle.Bold, TextAnchor.MiddleCenter, worldPosition, new Vector2(50f, 16f), new Vector2(0f, -20f), round);
        }

        private void DrawAirportMapStandLeadIn(Transform parent, Vector2 mapSize, bool round, string prefix, string label, Vector3 spotWorldPosition)
        {
            var taxiwayZ = -5f;
            var leadInDepth = Mathf.Abs(spotWorldPosition.z - taxiwayZ);
            if (leadInDepth <= 0.1f)
            {
                return;
            }

            var leadInCenter = new Vector3(spotWorldPosition.x, 0f, (spotWorldPosition.z + taxiwayZ) * 0.5f);
            CreateAirportMapBlock(parent, mapSize, $"{prefix} {label} Lead In", leadInCenter, new Vector2(0.55f, leadInDepth), new Color(0.24f, 0.42f, 0.48f, 0.96f), round);
        }

        private void CreateMiniMapSpots()
        {
            var airport = gameManager != null ? gameManager.Airport : null;
            if (airport == null || airport.SpotDefinitions.Count == 0)
            {
                CreateMiniMapSpot("SPOT 01", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(6.3f, 0f, -8.35f), new Vector3(2.35f, 0f, 3.45f));
                CreateMiniMapSpot("SPOT 02", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(10.8f, 0f, -7.95f), new Vector3(2.35f, 0f, 3.45f));
                CreateMiniMapSpot("SPOT 03", AirportSpotArea.DOM, AircraftSizeClass.Narrowbody, new Vector3(17.1f, 0f, -8.15f), new Vector3(2.35f, 0f, 3.45f));
                CreateMiniMapSpot("SPOT 04", AirportSpotArea.INTL, AircraftSizeClass.Widebody, new Vector3(26.2f, 0f, -8.65f), new Vector3(3.7f, 0f, 4.85f));
                return;
            }

            foreach (var spotDefinition in airport.SpotDefinitions)
            {
                CreateMiniMapSpot(
                    spotDefinition.TutorialId,
                    spotDefinition.Area,
                    spotDefinition.AircraftSizeClass,
                    spotDefinition.Position,
                    spotDefinition.StandScale);
            }
        }

        private void CreateMiniMapSpot(string label, AirportSpotArea area, AircraftSizeClass aircraftSizeClass, Vector3 worldPosition, Vector3 standScale)
        {
            CreateMiniMapStandLeadIn(label, worldPosition);

            var position = WorldToMiniMap(worldPosition);
            var size = WorldSizeToMiniMap(new Vector2(standScale.x, standScale.z));
            size = new Vector2(Mathf.Max(size.x, aircraftSizeClass == AircraftSizeClass.Widebody ? 22f : 18f), Mathf.Max(size.y, aircraftSizeClass == AircraftSizeClass.Widebody ? 18f : 14f));
            CreateMiniMapBlock($"Mini Map {label}", position, size, GetMiniMapSpotColor(area, aircraftSizeClass));
            CreateText($"Mini Map {label} Label", minimapContent, label.Replace("SPOT ", "S"), 11, FontStyle.Bold, TextAnchor.MiddleCenter, position + new Vector2(0f, -20f), new Vector2(50f, 16f));
        }

        private void CreateMiniMapStandLeadIn(string label, Vector3 spotWorldPosition)
        {
            var taxiwayZ = -5f;
            var leadInDepth = Mathf.Abs(spotWorldPosition.z - taxiwayZ);
            if (leadInDepth <= 0.1f)
            {
                return;
            }

            var leadInCenter = new Vector3(spotWorldPosition.x, 0f, (spotWorldPosition.z + taxiwayZ) * 0.5f);
            CreateMiniMapBlock(
                $"Mini Map {label} Lead In",
                WorldToMiniMap(leadInCenter),
                WorldSizeToMiniMap(new Vector2(0.55f, leadInDepth)),
                new Color(0.24f, 0.42f, 0.48f, 0.96f));
        }

        private Color GetMiniMapSpotColor(AirportSpotArea area, AircraftSizeClass aircraftSizeClass)
        {
            if (area == AirportSpotArea.INTL || aircraftSizeClass == AircraftSizeClass.Widebody)
            {
                return new Color(0.12f, 0.58f, 0.78f, 0.98f);
            }

            if (area == AirportSpotArea.DOM)
            {
                return new Color(0.1f, 0.64f, 0.56f, 0.98f);
            }

            return new Color(0.56f, 0.56f, 0.46f, 0.98f);
        }

        private Image CreateMiniMapBlock(string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            return CreateMapBlock(name, minimapContent, anchoredPosition, size, color);
        }

        private Image CreateMapBlock(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var blockObject = new GameObject(name);
            blockObject.transform.SetParent(parent, false);
            var rectTransform = blockObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
            var image = blockObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void UpdateMiniMapSelectedDepartureRoute(AircraftController selected)
        {
            var candidate = ResolveMiniMapDepartureRouteCandidate(selected, out var fallbackUsed);
            if (candidate == null || candidate.Waypoints.Count < 2)
            {
                ClearMiniMapRouteHighlight();
                lastLoggedMiniMapRouteKey = string.Empty;
                return;
            }

            var segmentCount = candidate.Waypoints.Count - 1;
            EnsureMiniMapRouteHighlightSegmentCount(segmentCount);
            for (var index = 0; index < minimapRouteHighlightSegments.Count; index++)
            {
                var image = minimapRouteHighlightSegments[index];
                var active = index < segmentCount;
                image.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                image.color = new Color(1f, 0.86f, 0.18f, 0.9f);
                image.transform.SetAsLastSibling();
                ApplyRouteSelectionLineSegment(
                    image.GetComponent<RectTransform>(),
                    WorldToMiniMap(candidate.Waypoints[index]),
                    WorldToMiniMap(candidate.Waypoints[index + 1]),
                    5f);
            }

            LogMiniMapRouteHighlight(selected, candidate, fallbackUsed);
        }

        private TaxiRouteCandidate ResolveMiniMapDepartureRouteCandidate(AircraftController selected, out bool fallbackUsed)
        {
            fallbackUsed = false;
            if (selected == null || selected.FlightData == null || selected.FlightData.OperationType != "Departure" || gameManager == null || gameManager.Airport == null)
            {
                return null;
            }

            var data = selected.FlightData;
            if (!data.HasActiveRunwayDesignator)
            {
                return null;
            }

            if (string.IsNullOrEmpty(data.SelectedDepartureRouteId))
            {
                return null;
            }

            var candidates = gameManager.Airport.GetDepartureTaxiRouteCandidates(data.SpotId, data.ActiveRunwayDesignator);
            TaxiRouteCandidate fallbackCandidate = null;
            foreach (var candidate in candidates)
            {
                if (fallbackCandidate == null)
                {
                    fallbackCandidate = candidate;
                }

                if (candidate.IsDefault)
                {
                    fallbackCandidate = candidate;
                }

                if (!string.IsNullOrEmpty(data.SelectedDepartureRouteId) && candidate.RouteId == data.SelectedDepartureRouteId)
                {
                    return candidate;
                }
            }

            fallbackUsed = fallbackCandidate != null && !string.IsNullOrEmpty(data.SelectedDepartureRouteId);
            return fallbackCandidate;
        }

        private void EnsureMiniMapRouteHighlightSegmentCount(int segmentCount)
        {
            while (minimapRouteHighlightSegments.Count < segmentCount)
            {
                var image = CreateMiniMapBlock(
                    $"Mini Map Selected Route Highlight {minimapRouteHighlightSegments.Count + 1}",
                    Vector2.zero,
                    new Vector2(20f, 5f),
                    new Color(1f, 0.86f, 0.18f, 0.9f));
                image.transform.SetAsLastSibling();
                minimapRouteHighlightSegments.Add(image);
            }
        }

        private void ClearMiniMapRouteHighlight()
        {
            foreach (var image in minimapRouteHighlightSegments)
            {
                image.gameObject.SetActive(false);
            }
        }

        private void LogMiniMapRouteHighlight(AircraftController selected, TaxiRouteCandidate candidate, bool fallbackUsed)
        {
            if (selected == null || selected.FlightData == null || candidate == null)
            {
                return;
            }

            var firstWaypoint = candidate.Waypoints[0];
            var lastWaypoint = candidate.Waypoints[candidate.Waypoints.Count - 1];
            var firstMiniMapPoint = WorldToMiniMap(firstWaypoint);
            var lastMiniMapPoint = WorldToMiniMap(lastWaypoint);
            var routeKey = $"{selected.FlightNumber}|{selected.FlightData.ActiveRunwayDesignator}|{candidate.RouteId}|{fallbackUsed}";
            if (routeKey == lastLoggedMiniMapRouteKey)
            {
                return;
            }

            lastLoggedMiniMapRouteKey = routeKey;
            Debug.Log(
                $"MiniMap route highlight: {selected.FlightNumber} "
                + $"selectedRunwayDesignator={selected.FlightData.ActiveRunwayDesignator} "
                + $"selectedDepartureRouteId={selected.FlightData.SelectedDepartureRouteId} actualRouteId={candidate.RouteId} "
                + $"runwayEntryUsageId={candidate.RunwayEntryUsageId} segments={FormatRouteSegmentIds(candidate)} "
                + $"firstWaypoint={FormatRouteWaypoint(candidate, true)} lastWaypoint={FormatRouteWaypoint(candidate, false)} "
                + $"minimapFirst=({firstMiniMapPoint.x:0.0}, {firstMiniMapPoint.y:0.0}) "
                + $"minimapLast=({lastMiniMapPoint.x:0.0}, {lastMiniMapPoint.y:0.0}) fallbackUsed={fallbackUsed}");
        }

        private void UpdateMiniMap(AircraftController selected)
        {
            if (minimapPanel == null)
            {
                return;
            }

            UpdateMiniMapSelectedDepartureRoute(selected);

            foreach (var dot in minimapAircraftDots)
            {
                dot.Value.gameObject.SetActive(false);
            }

            foreach (var aircraft in gameManager.Aircraft)
            {
                if (aircraft == null || aircraft.FlightData == null)
                {
                    continue;
                }

                var flightNumber = aircraft.FlightNumber;
                if (!minimapAircraftDots.ContainsKey(flightNumber))
                {
                    CreateMiniMapAircraftDot(flightNumber);
                }

                var isSelected = aircraft == selected;
                var dotTransform = minimapAircraftDots[flightNumber];
                var minimapPosition = WorldToMiniMap(aircraft.transform.position);
                dotTransform.gameObject.SetActive(true);
                dotTransform.SetAsLastSibling();
                dotTransform.anchoredPosition = minimapPosition;
                dotTransform.sizeDelta = isSelected ? new Vector2(38f, 38f) : new Vector2(28f, 28f);

                var arrow = minimapAircraftArrows[flightNumber];
                arrow.color = GetMiniMapAircraftColor(aircraft, isSelected);
                arrow.fontSize = isSelected ? 31 : 23;
                arrow.GetComponent<RectTransform>().sizeDelta = dotTransform.sizeDelta;
                arrow.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, aircraft.FlightData.HeadingDegrees);

                var outline = minimapAircraftOutlines[flightNumber];
                outline.enabled = isSelected;
                outline.effectColor = new Color(1f, 1f, 1f, 0.95f);
                outline.effectDistance = isSelected ? new Vector2(2f, -2f) : new Vector2(1f, -1f);

                var label = minimapAircraftLabels[flightNumber];
                label.text = isSelected ? flightNumber : string.Empty;
                label.gameObject.SetActive(isSelected);
                var labelTransform = label.GetComponent<RectTransform>();
                labelTransform.anchoredPosition = dotTransform.anchoredPosition.x > 118f ? new Vector2(-86f, 0f) : new Vector2(22f, 0f);
                label.alignment = dotTransform.anchoredPosition.x > 118f ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            }

            if (minimapRunwayImage != null)
            {
                minimapRunwayImage.color = gameManager.IsPrimaryRunwayOccupied
                    ? new Color(0.86f, 0.48f, 0.18f, 0.98f)
                    : new Color(0.38f, 0.4f, 0.42f, 0.98f);
            }
        }

        private void CreateMiniMapAircraftDot(string flightNumber)
        {
            var dotObject = new GameObject($"Mini Map Aircraft {flightNumber}");
            dotObject.transform.SetParent(minimapContent, false);
            var rectTransform = dotObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = new Vector2(28f, 28f);

            var arrow = CreateText($"{flightNumber} Mini Map Arrow", dotObject.transform, "▲", 23, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(28f, 28f));
            var outline = arrow.gameObject.AddComponent<Outline>();
            outline.enabled = false;
            outline.effectColor = new Color(1f, 1f, 1f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            var label = CreateText($"{flightNumber} Mini Map Label", dotObject.transform, string.Empty, 12, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(22f, 0f), new Vector2(70f, 18f));
            minimapAircraftDots[flightNumber] = rectTransform;
            minimapAircraftArrows[flightNumber] = arrow;
            minimapAircraftOutlines[flightNumber] = outline;
            minimapAircraftLabels[flightNumber] = label;
        }

        private Color GetMiniMapAircraftColor(AircraftController aircraft, bool isSelected)
        {
            if (isSelected)
            {
                return new Color(1f, 0.92f, 0.22f, 1f);
            }

            return aircraft.IsArrivalAircraft
                ? new Color(0.36f, 0.86f, 1f, 0.95f)
                : new Color(1f, 0.58f, 0.22f, 0.95f);
        }

        private Vector2 WorldToMiniMap(Vector3 worldPosition)
        {
            return ConvertAirportWorldToMapPoint(worldPosition, new Vector2(MiniMapWidth, MiniMapHeight), true);
        }

        private Vector2 WorldSizeToMiniMap(Vector2 worldSize)
        {
            return ConvertAirportWorldSizeToMapSize(worldSize, new Vector2(MiniMapWidth, MiniMapHeight), true);
        }

        private Vector2 ConvertAirportWorldToMapPoint(Vector3 worldPosition, Vector2 mapSize, bool round)
        {
            var x = Mathf.InverseLerp(MiniMapWorldMinX, MiniMapWorldMaxX, worldPosition.x) * mapSize.x - mapSize.x * 0.5f;
            var y = Mathf.InverseLerp(MiniMapWorldMinZ, MiniMapWorldMaxZ, worldPosition.z) * mapSize.y - mapSize.y * 0.5f;
            return round ? new Vector2(Mathf.Round(x), Mathf.Round(y)) : new Vector2(x, y);
        }

        private Vector2 ConvertAirportWorldSizeToMapSize(Vector2 worldSize, Vector2 mapSize, bool round)
        {
            var x = Mathf.Max(3f, worldSize.x / (MiniMapWorldMaxX - MiniMapWorldMinX) * mapSize.x);
            var y = Mathf.Max(3f, worldSize.y / (MiniMapWorldMaxZ - MiniMapWorldMinZ) * mapSize.y);
            return round ? new Vector2(Mathf.Round(x), Mathf.Round(y)) : new Vector2(x, y);
        }

        private void CreateFlightStripButton(string flightNumber, Vector2 anchoredPosition)
        {
            var stripObject = new GameObject($"Flight Strip {flightNumber}");
            stripObject.transform.SetParent(flightStripPanel.transform, false);
            var rectTransform = stripObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = new Vector2(292f, 64f);
            rectTransform.anchoredPosition = anchoredPosition;

            var image = stripObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.095f, 0.1f, 0.94f);

            var button = stripObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.96f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            button.colors = colors;
            button.onClick.AddListener(() => SelectFlightStripAircraft(flightNumber));

            var stripText = CreateText($"{flightNumber} Strip Text", stripObject.transform, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(252f, 48f));
            flightStripButtons[flightNumber] = button;
            flightStripTexts[flightNumber] = stripText;
            flightStripImages[flightNumber] = image;
        }

        private void SelectFlightStripAircraft(string flightNumber)
        {
            var aircraft = FindAircraft(flightNumber);
            if (aircraft == null)
            {
                return;
            }

            SelectionManager.Instance?.SelectAircraft(aircraft);
            Refresh();
        }

        private void UpdateFlightStrips(AircraftController selected)
        {
            foreach (var strip in flightStripButtons)
            {
                strip.Value.gameObject.SetActive(false);
            }

            var selectedStripPosition = Vector2.zero;
            var selectedRecommended = selected != null ? GetRecommendedCommand(selected) : null;
            var hasSelectedStrip = false;
            var arrivalIndex = 0;
            var departureIndex = 0;
            foreach (var aircraft in gameManager.Aircraft)
            {
                if (aircraft == null || aircraft.FlightData == null)
                {
                    continue;
                }

                var flightNumber = aircraft.FlightData.FlightId;
                if (!flightStripTexts.ContainsKey(flightNumber))
                {
                    CreateFlightStripButton(flightNumber, Vector2.zero);
                }

                var recommended = GetRecommendedCommand(aircraft);
                var isArrival = aircraft.FlightData.OperationType == "Arrival";
                var stripIndex = isArrival ? arrivalIndex++ : departureIndex++;
                var stripTransform = flightStripButtons[flightNumber].GetComponent<RectTransform>();
                stripTransform.anchoredPosition = GetFlightStripPosition(isArrival, stripIndex);
                flightStripButtons[flightNumber].gameObject.SetActive(true);
                flightStripTexts[flightNumber].text = GetFlightStripText(aircraft, recommended);
                flightStripImages[flightNumber].color = GetFlightStripColor(aircraft, selected, recommended.HasValue);

                if (aircraft == selected)
                {
                    selectedStripPosition = stripTransform.anchoredPosition;
                    hasSelectedStrip = true;
                }
            }

            UpdateStripCommandButton(selected, selectedRecommended, selectedStripPosition, hasSelectedStrip);
            UpdateControlSectorSummary(selected);
        }

        private Vector2 GetFlightStripPosition(bool isArrival, int index)
        {
            var baseY = isArrival ? 156f : -72f;
            return new Vector2(0f, baseY - index * 72f);
        }

        private string GetFlightStripText(AircraftController aircraft, AircraftCommand? recommended)
        {
            var data = aircraft.FlightData;
            var target = GetStripTargetLabel(data);
            var phase = GetControlSectorLabel(aircraft);
            var nextAction = recommended.HasValue ? GetCommandShortLabel(recommended.Value) : "監視";

            return $"<size=23>{data.FlightId}</size>   <size=13>{phase}</size>\n<size=15>{target}  /  {nextAction}</size>";
        }

        private void UpdateControlSectorSummary(AircraftController selected)
        {
            if (controlSectorSummaryText == null || gameManager == null)
            {
                return;
            }

            var approachCount = 0;
            var towerCount = 0;
            var groundCount = 0;
            var departureCount = 0;
            foreach (var aircraft in gameManager.Aircraft)
            {
                var sector = GetControlSectorLabel(aircraft);
                switch (sector)
                {
                    case "APPROACH":
                        approachCount++;
                        break;
                    case "TOWER":
                        towerCount++;
                        break;
                    case "GROUND":
                        groundCount++;
                        break;
                    case "DEPARTURE":
                        departureCount++;
                        break;
                }
            }

            var selectedSector = selected != null ? GetControlSectorLabel(selected) : "-";
            controlSectorSummaryText.text =
                $"管制フェーズ  選択: {selectedSector}\n"
                + $"APP {approachCount} / TWR {towerCount} / GND {groundCount} / DEP {departureCount}";
        }

        private string GetControlSectorLabel(AircraftController aircraft)
        {
            if (aircraft == null || aircraft.FlightData == null)
            {
                return "-";
            }

            if (aircraft.FlightData.OperationType == "Arrival")
            {
                switch (aircraft.CurrentState)
                {
                    case AircraftState.Inbound:
                    case AircraftState.FinalApproach:
                    case AircraftState.LandingRoll:
                        return "TOWER";
                    case AircraftState.VacatingRunway:
                    case AircraftState.TaxiToGate:
                    case AircraftState.Waiting:
                    case AircraftState.AtGate:
                        return "GROUND";
                    default:
                        return "TOWER";
                }
            }

            switch (aircraft.CurrentState)
            {
                case AircraftState.AtGate:
                case AircraftState.Pushbacking:
                case AircraftState.PushbackReady:
                case AircraftState.TaxiToHold:
                case AircraftState.TaxiHeld:
                    return "GROUND";
                case AircraftState.HoldingPoint:
                case AircraftState.HoldingShort:
                case AircraftState.LiningUp:
                case AircraftState.TakeoffRoll:
                    return "TOWER";
                case AircraftState.AirborneDeparture:
                    return "DEPARTURE";
                default:
                    return "GROUND";
            }
        }

        private void UpdateStripCommandButton(AircraftController selected, AircraftCommand? recommended, Vector2 selectedStripPosition, bool hasSelectedStrip)
        {
            if (stripCommandPopup == null || stripCommandOptionButtons.Count == 0)
            {
                return;
            }

            var actions = hasSelectedStrip && selected != null
                ? BuildStripActions(selected, recommended)
                : new List<StripAction>();
            var canShow = actions.Count > 0;

            stripCommandPopup.SetActive(canShow);
            if (!canShow)
            {
                SetStripCommandOptionButtonsActive(0);
                return;
            }

            var popupTransform = stripCommandPopup.GetComponent<RectTransform>();
            popupTransform.sizeDelta = GetStripCommandPopupSize(actions.Count);
            popupTransform.anchoredPosition = new Vector2(selectedStripPosition.x + 306f, selectedStripPosition.y);
            ConfigureStripActions(actions);
        }

        private void CreateStripCommandOptionButton(int index)
        {
            var optionButton = CreateButton($"Strip Command Option {index + 1}", stripCommandPopup.transform, string.Empty, Vector2.zero, new Vector2(260f, 72f), 16);
            var optionText = optionButton.GetComponentInChildren<Text>();
            stripCommandOptionButtons.Add(optionButton);
            stripCommandOptionTexts.Add(optionText);

            if (index == 0)
            {
                stripCommandButton = optionButton;
                stripCommandButtonText = optionText;
            }
        }

        private List<StripAction> BuildStripActions(AircraftController selected, AircraftCommand? recommended)
        {
            var actions = new List<StripAction>();
            if (TryBuildDeparturePrePushbackActions(actions, selected))
            {
                return actions;
            }

            if (TryBuildArrivalPreLandingActions(actions, selected))
            {
                return actions;
            }

            AddRouteSelectionStripAction(actions, selected);
            if (!recommended.HasValue
                || !selected.CanExecute(recommended.Value)
                || (!CanAcceptCommand(selected, recommended.Value)
                    && gameManager.CanExecuteRunwaySafetyCommand(selected, recommended.Value)))
            {
                return actions;
            }

            var runwayOptions = GetCommandRunwayOptions(recommended.Value, selected);
            var safe = gameManager.CanExecuteRunwaySafetyCommand(selected, recommended.Value);
            if (runwayOptions.Count == 0)
            {
                actions.Add(new StripAction(GetCommandLabel(recommended.Value), () => ExecuteStripCommand(recommended.Value, string.Empty), safe));
                return actions;
            }

            foreach (var runwayDesignator in runwayOptions)
            {
                var capturedRunwayDesignator = runwayDesignator;
                actions.Add(new StripAction(
                    GetRunwaySpecificCommandLabel(recommended.Value, capturedRunwayDesignator),
                    () => ExecuteStripCommand(recommended.Value, capturedRunwayDesignator),
                    safe));
            }

            return actions;
        }

        private bool TryBuildDeparturePrePushbackActions(List<StripAction> actions, AircraftController selected)
        {
            if (selected == null || selected.FlightData == null || selected.FlightData.OperationType != "Departure" || selected.CurrentState != AircraftState.AtGate)
            {
                return false;
            }

            if (!selected.FlightData.HasActiveRunwayDesignator)
            {
                AddDepartureRunwaySelectionActions(actions, selected);
                return true;
            }

            var candidates = GetRouteSelectionCandidates(selected, RouteSelectionModeDeparture);
            if (candidates.Count == 1 && !HasValidSelectedDepartureRoute(selected.FlightData, candidates))
            {
                AutoSelectSingleRouteCandidate(selected, RouteSelectionModeDeparture, candidates[0], false);
                return false;
            }

            if (candidates.Count > 1 && !HasValidSelectedDepartureRoute(selected.FlightData, candidates))
            {
                var label = GetRouteSelectionStripLabel(RouteSelectionModeDeparture, selected);
                actions.Add(new StripAction(label, () => OpenRouteSelectionOverlay(selected, RouteSelectionModeDeparture), true));
                return true;
            }

            return false;
        }

        private bool TryBuildArrivalPreLandingActions(List<StripAction> actions, AircraftController selected)
        {
            if (selected == null
                || selected.FlightData == null
                || selected.FlightData.OperationType != "Arrival"
                || selected.CurrentState != AircraftState.Inbound
                || HasArrivalRunwaySelectionCompleted(selected))
            {
                return false;
            }

            actions.Add(new StripAction(
                "滑走路を選択\nSelect Runway",
                () => OpenRunwaySelectionOverlay(selected),
                true));
            return true;
        }

        private bool HasArrivalRunwaySelectionCompleted(AircraftController selected)
        {
            return selected != null
                && selected.FlightData != null
                && arrivalRunwaySelectionCompletedFlightIds.Contains(selected.FlightData.FlightId);
        }

        private void AddDepartureRunwaySelectionActions(List<StripAction> actions, AircraftController selected)
        {
            if (gameManager == null || gameManager.Airport == null)
            {
                return;
            }

            actions.Add(new StripAction(
                "滑走路を選択\nSelect Runway",
                () => OpenRunwaySelectionOverlay(selected),
                true));
        }

        private void SelectRunwayForAircraft(AircraftController selected, string runwayDesignator)
        {
            if (selected == null || selected.FlightData == null)
            {
                return;
            }

            if (selected.FlightData.OperationType == "Arrival")
            {
                SelectArrivalRunway(selected, runwayDesignator);
                return;
            }

            SelectDepartureRunway(selected, runwayDesignator);
        }

        private void SelectDepartureRunway(AircraftController selected, string runwayDesignator)
        {
            if (selected == null || selected.FlightData == null || gameManager == null || gameManager.Airport == null)
            {
                return;
            }

            selected.BindRunwayDirection(gameManager.Airport.PrimaryRunwayData, runwayDesignator);
            selected.SetSelectedTaxiRoute("Departure", string.Empty);
            var candidates = GetRouteSelectionCandidates(selected, RouteSelectionModeDeparture);
            Debug.Log(
                $"Departure runway selected: {selected.FlightNumber} "
                + $"selectedRunwayDesignator={runwayDesignator} selectedDepartureRouteId=none spot={selected.FlightData.SpotId} "
                + $"candidateCount={candidates.Count} routeIds={FormatRouteCandidateRouteIds(candidates)} "
                + $"entryUsages={FormatRouteCandidateEntryUsageIds(candidates)}");
            Refresh();
        }

        private void SelectArrivalRunway(AircraftController selected, string runwayDesignator)
        {
            if (selected == null || selected.FlightData == null || gameManager == null || gameManager.Airport == null)
            {
                return;
            }

            selected.BindRunwayDirection(gameManager.Airport.PrimaryRunwayData, runwayDesignator);
            if (!arrivalRunwaySelectionCompletedFlightIds.Contains(selected.FlightData.FlightId))
            {
                arrivalRunwaySelectionCompletedFlightIds.Add(selected.FlightData.FlightId);
            }

            Debug.Log(
                $"Arrival runway selected: {selected.FlightNumber} "
                + $"operationType={selected.FlightData.OperationType} selectedRunwayDesignator={runwayDesignator}");
            Refresh();
        }

        private bool HasValidSelectedDepartureRoute(AircraftData data, IReadOnlyList<TaxiRouteCandidate> candidates)
        {
            if (data == null || string.IsNullOrEmpty(data.SelectedDepartureRouteId))
            {
                return false;
            }

            foreach (var candidate in candidates)
            {
                if (candidate.RouteId == data.SelectedDepartureRouteId)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddRouteSelectionStripAction(List<StripAction> actions, AircraftController selected)
        {
            var mode = GetRouteSelectionModeForStrip(selected);
            if (string.IsNullOrEmpty(mode))
            {
                return;
            }

            var candidates = GetRouteSelectionCandidates(selected, mode);
            if (candidates.Count <= 1)
            {
                return;
            }

            if (mode == RouteSelectionModeDeparture && HasValidSelectedDepartureRoute(selected.FlightData, candidates))
            {
                return;
            }

            if (mode == RouteSelectionModeArrivalSpotTaxi && HasValidSelectedArrivalRoute(selected.FlightData, candidates))
            {
                return;
            }

            var label = GetRouteSelectionStripLabel(mode, selected);
            actions.Add(new StripAction(label, () => OpenRouteSelectionOverlay(selected, mode), true));
        }

        private bool HasValidSelectedArrivalRoute(AircraftData data, IReadOnlyList<TaxiRouteCandidate> candidates)
        {
            if (data == null || string.IsNullOrEmpty(data.SelectedArrivalRouteId))
            {
                return false;
            }

            foreach (var candidate in candidates)
            {
                if (candidate.RouteId == data.SelectedArrivalRouteId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ConfigureStripActions(IReadOnlyList<StripAction> actions)
        {
            var optionCount = actions.Count;
            EnsureStripCommandOptionButtonCount(optionCount);
            SetStripCommandOptionButtonsActive(optionCount);

            for (var index = 0; index < optionCount; index++)
            {
                var button = stripCommandOptionButtons[index];
                var label = stripCommandOptionTexts[index];
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = GetStripCommandOptionPosition(index, optionCount);
                label.text = actions[index].Label;
                button.onClick.RemoveAllListeners();
                var actionIndex = index;
                button.onClick.AddListener(() => actions[actionIndex].Execute());
                ApplyStripCommandButtonSafetyStyle(button, actions[index].Safe);
            }
        }

        private void EnsureStripCommandOptionButtonCount(int optionCount)
        {
            while (stripCommandOptionButtons.Count < optionCount)
            {
                CreateStripCommandOptionButton(stripCommandOptionButtons.Count);
            }
        }

        private Vector2 GetStripCommandOptionPosition(int index, int optionCount)
        {
            var spacing = 82f;
            var topY = (optionCount - 1) * spacing * 0.5f;
            return new Vector2(0f, topY - index * spacing);
        }

        private void SetStripCommandOptionButtonsActive(int activeCount)
        {
            for (var index = 0; index < stripCommandOptionButtons.Count; index++)
            {
                stripCommandOptionButtons[index].gameObject.SetActive(index < activeCount);
            }
        }

        private void ApplyStripCommandButtonSafetyStyle(Button button, bool safe)
        {
            var colors = button.colors;
            if (safe)
            {
                colors.normalColor = new Color(0.14f, 0.52f, 0.22f, 0.95f);
                colors.highlightedColor = new Color(0.18f, 0.66f, 0.28f, 1f);
                colors.pressedColor = new Color(0.1f, 0.42f, 0.18f, 1f);
            }
            else
            {
                colors.normalColor = new Color(0.58f, 0.18f, 0.12f, 0.95f);
                colors.highlightedColor = new Color(0.72f, 0.24f, 0.16f, 1f);
                colors.pressedColor = new Color(0.42f, 0.1f, 0.08f, 1f);
            }

            button.colors = colors;
        }

        private Vector2 GetStripCommandPopupSize(int commandCount)
        {
            var safeCommandCount = Mathf.Clamp(commandCount, 1, 4);
            return new Vector2(292f, 24f + safeCommandCount * 72f + (safeCommandCount - 1) * 10f);
        }

        private string GetStripTargetLabel(AircraftData data)
        {
            if (data.OperationType == "Arrival")
            {
                return data.NextTargetType == "Spot" ? data.SpotDisplayName : data.RunwayShortDisplay;
            }

            return data.NextTargetType == "Runway" || data.NextTargetType == "HoldingPoint"
                ? data.RunwayShortDisplay
                : data.SpotDisplayName;
        }

        private Color GetFlightStripColor(AircraftController aircraft, AircraftController selected, bool hasRecommendedCommand)
        {
            if (aircraft == selected)
            {
                return new Color(0.2f, 0.36f, 0.58f, 0.96f);
            }

            if (IsTutorialHighlightingAircraft(aircraft.FlightNumber))
            {
                return new Color(0.18f, 0.42f, 0.44f, 0.96f);
            }

            if (hasRecommendedCommand)
            {
                return new Color(0.12f, 0.2f, 0.13f, 0.96f);
            }

            return new Color(0.08f, 0.095f, 0.1f, 0.94f);
        }

        private bool IsTutorialHighlightingAircraft(string flightNumber)
        {
            switch (activeTutorialHighlight)
            {
                case TutorialHighlight.ArrivalAircraft:
                case TutorialHighlight.ClearLanding:
                case TutorialHighlight.RunwayInUse:
                case TutorialHighlight.Gate1:
                case TutorialHighlight.TaxiToGate:
                    return flightNumber == "AJJ101";
                case TutorialHighlight.Gate2:
                case TutorialHighlight.Pushback:
                case TutorialHighlight.TaxiToHold:
                case TutorialHighlight.HoldShort:
                case TutorialHighlight.LineUp:
                    return flightNumber == "AJJ202";
                default:
                    return false;
            }
        }

        private string GetSelectedAircraftText(AircraftController selected, AircraftCommand? recommended)
        {
            var data = selected.FlightData;
            if (data == null)
            {
                return $"便名: {selected.FlightNumber}\n状態: {GetStateLabel(selected.CurrentState)}\n推奨: {(recommended.HasValue ? GetCommandShortLabel(recommended.Value) : "監視")}";
            }

            var recommendedLabel = recommended.HasValue ? GetCommandShortLabel(recommended.Value) : "監視";
            var timeLine = data.OperationType == "Arrival"
                ? $"予定到着：{GetTimeLabel(data.ScheduledArrivalTime)}"
                : $"予定出発：{GetTimeLabel(data.ScheduledDepartureTime)}";
            var routeLine = GetSelectedCityRouteLine(data);
            return $"{data.FlightId}  {data.AircraftType}\n"
                + $"{routeLine}\n\n"
                + $"{timeLine}\n"
                + $"RWY：{(data.HasActiveRunwayDesignator ? data.ActiveRunwayDesignator : "-")}\n"
                + $"SPOT：{GetSpotNumber(data.SpotDisplayName)}\n"
                + $"{GetSelectedRouteStatusLine(selected)}\n"
                + $"状態：{data.CurrentState}\n"
                + $"担当フェーズ：{GetControlSectorLabel(selected)}\n"
                + $"次の指示：{recommendedLabel}";
        }

        private string GetSelectedRouteStatusLine(AircraftController selected)
        {
            var data = selected.FlightData;
            var purpose = GetRouteSelectionPurpose(selected);
            var selectedRouteId = GetSelectedRouteId(data, purpose);
            if (string.IsNullOrEmpty(selectedRouteId))
            {
                return "Route：未選択";
            }

            var mode = data.OperationType == "Arrival" && selected.CurrentState == AircraftState.LandingRoll
                ? RouteSelectionModeArrivalExit
                : data.OperationType == "Arrival"
                    ? RouteSelectionModeArrivalSpotTaxi
                    : RouteSelectionModeDeparture;
            var candidates = GetRouteSelectionCandidates(selected, mode);
            for (var index = 0; index < candidates.Count; index++)
            {
                if (candidates[index].RouteId != selectedRouteId)
                {
                    continue;
                }

                var label = GetPlayerRouteName(candidates[index], index, mode);
                return mode == RouteSelectionModeArrivalExit ? $"離脱方式：{label}" : $"選択Route：{label}";
            }

            return "Route：選択済み";
        }

        private string GetSelectedCityRouteLine(AircraftData data)
        {
            var isArrival = data.OperationType == "Arrival";
            var flightType = isArrival ? "到着便" : "出発便";
            var cityRoute = GetCityRouteLabel(data);
            return string.IsNullOrEmpty(cityRoute) ? flightType : $"{flightType} / {cityRoute}";
        }

        private string GetCompactCityRouteLabel(AircraftData data)
        {
            var cityRoute = GetCityRouteLabel(data);
            if (!string.IsNullOrEmpty(cityRoute))
            {
                return cityRoute;
            }

            return data.OperationType == "Arrival" ? "到着便" : "出発便";
        }

        private string GetCityRouteLabel(AircraftData data)
        {
            var isArrival = data.OperationType == "Arrival";
            var cityName = isArrival ? data.Origin : data.Destination;
            var displayCity = GetJapaneseCityName(cityName);
            if (string.IsNullOrEmpty(displayCity) || IsNahaCityName(cityName) || IsNahaCityName(displayCity))
            {
                return string.Empty;
            }

            return isArrival ? $"{displayCity}発" : $"{displayCity}行き";
        }

        private string GetJapaneseCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return string.Empty;
            }

            var normalized = cityName.Trim().ToLowerInvariant();
            return JapaneseCityNames.TryGetValue(normalized, out var japaneseName) ? japaneseName : cityName.Trim();
        }

        private bool IsNahaCityName(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return false;
            }

            var normalized = cityName.Trim().ToLowerInvariant();
            return normalized == "naha" || normalized == "那覇";
        }

        private string GetTimeLabel(string time)
        {
            return string.IsNullOrEmpty(time) ? "-" : time;
        }

        private string GetSpotNumber(string spotDisplayName)
        {
            return spotDisplayName.Replace("SPOT ", string.Empty);
        }

        private void AdvanceTutorial()
        {
            if (!tutorialActive)
            {
                return;
            }

            tutorialStepIndex++;
            if (tutorialStepIndex >= tutorialSteps.Length)
            {
                tutorialActive = false;
                tutorialStepIndex = -1;
                ApplyTutorialHighlight(TutorialHighlight.None);
            }

            Refresh();
        }

        private void AdvanceTutorialIfReady()
        {
            var step = CurrentTutorialStep;
            if (step == null)
            {
                return;
            }

            if (step.WaitMode == TutorialWaitMode.RunwayExitReady)
            {
                var arrival = FindAircraft("AJJ101");
                if (arrival != null && (arrival.CurrentState == AircraftState.VacatingRunway || arrival.CurrentState == AircraftState.Waiting))
                {
                    AdvanceTutorial();
                }

                return;
            }

            if (step.WaitMode == TutorialWaitMode.ArrivalComplete)
            {
                var arrival = FindAircraft("AJJ101");
                if (arrival != null && arrival.CurrentState == AircraftState.AtGate)
                {
                    AdvanceTutorial();
                }

                return;
            }

            if (step.WaitMode == TutorialWaitMode.PushbackComplete)
            {
                var departure = FindAircraft("AJJ202");
                if (departure != null && departure.CurrentState == AircraftState.PushbackReady)
                {
                    AdvanceTutorial();
                }

                return;
            }

            if (step.WaitMode == TutorialWaitMode.HoldingPointReady)
            {
                var departure = FindAircraft("AJJ202");
                if (departure != null && departure.CurrentState == AircraftState.HoldingPoint)
                {
                    AdvanceTutorial();
                }
            }
        }

        private TutorialStep CurrentTutorialStep
        {
            get
            {
                if (!tutorialActive || tutorialSteps == null || tutorialStepIndex < 0 || tutorialStepIndex >= tutorialSteps.Length)
                {
                    return null;
                }

                return tutorialSteps[tutorialStepIndex];
            }
        }

        private void EnsureTutorialSteps()
        {
            if (tutorialSteps != null)
            {
                return;
            }

            tutorialSteps = new[]
            {
                TutorialStep.Info("到着訓練\nAJJ101を着陸させましょう。"),
                TutorialStep.Info("ここはA滑走路です。\n飛行機が着陸・離陸する場所です。", TutorialHighlight.RunwayA),
                TutorialStep.Info("安全のため、1本の滑走路には\n基本的に1機だけ入れます。", TutorialHighlight.RunwayA),
                TutorialStep.Info("AJJ101がA滑走路に\n近づいています。", TutorialHighlight.ArrivalAircraft),
                TutorialStep.Command("AJJ101のストリップから\nClear to Land RWY 18Lを選びましょう。", AircraftCommand.ClearLanding, TutorialHighlight.ClearLanding, "AJJ101"),
                TutorialStep.RunwayExitReady("AJJ101が着陸中です。\n滑走路が使用中になります。", TutorialHighlight.RunwayInUse),
                TutorialStep.Info("着陸後は、次の飛行機のために\n滑走路を空けます。", TutorialHighlight.RunwayInUse),
                TutorialStep.Command("AJJ101のストリップから\nSPOT 01へ誘導しましょう。", AircraftCommand.TaxiToGate, TutorialHighlight.TaxiToGate, "AJJ101"),
                TutorialStep.ArrivalComplete("AJJ101がSPOT 01へ移動中です。\n到着完了まで見守ります。", TutorialHighlight.Gate1),
                TutorialStep.Info("AJJ101がSPOT 01に到着しました。\n到着機の基本処理は完了です。", TutorialHighlight.Gate1),
                TutorialStep.Info("出発訓練\nAJJ202を離陸させましょう。"),
                TutorialStep.Info("まずSPOT 02から\n出発準備をします。", TutorialHighlight.Gate2),
                TutorialStep.Command("AJJ202のストリップから\nプッシュバックします。", AircraftCommand.Pushback, TutorialHighlight.Pushback, "AJJ202"),
                TutorialStep.PushbackComplete("AJJ202が後退中です。\n地上走行の準備をします。", TutorialHighlight.Pushback),
                TutorialStep.Command("AJJ202のストリップから\nTaxi to RWY 18Lを選びましょう。", AircraftCommand.TaxiToHold, TutorialHighlight.TaxiToHold, "AJJ202"),
                TutorialStep.HoldingPointReady("AJJ202が誘導路を走行中です。\n滑走路手前で止めます。", TutorialHighlight.TaxiToHold),
                TutorialStep.Command("AJJ202のストリップから\nHold Short RWY 18Lを選びましょう。", AircraftCommand.HoldShort, TutorialHighlight.HoldShort, "AJJ202"),
                TutorialStep.Info("Hold Shortは手前、\nLine Upは滑走路上で待機です。", TutorialHighlight.HoldShort),
                TutorialStep.Command("AJJ202のストリップから\nLine Up and Wait RWY 18Lを選びましょう。", AircraftCommand.LineUp, TutorialHighlight.LineUp, "AJJ202"),
                TutorialStep.Command("AJJ202のストリップから\nCleared for Takeoff RWY 18Lを選びましょう。", AircraftCommand.ClearTakeoff, TutorialHighlight.None, "AJJ202")
            };
        }

        private void ApplyTutorialHighlight(TutorialHighlight highlight)
        {
            if (activeTutorialHighlight == highlight)
            {
                return;
            }

            ResetTutorialHighlights();
            activeTutorialHighlight = highlight;

            switch (highlight)
            {
                case TutorialHighlight.RunwayA:
                    HighlightObject("Runway A", new Color(0.95f, 0.78f, 0.18f));
                    break;
                case TutorialHighlight.RunwayInUse:
                    HighlightObject("Runway A", new Color(1f, 0.46f, 0.18f));
                    HighlightAircraft("AJJ101");
                    break;
                case TutorialHighlight.ArrivalAircraft:
                    HighlightAircraft("AJJ101");
                    break;
                case TutorialHighlight.ClearLanding:
                    HighlightObject("Runway A", new Color(0.3f, 0.8f, 0.3f));
                    HighlightAircraft("AJJ101");
                    break;
                case TutorialHighlight.Gate1:
                    HighlightObject("Gate 1 Stand", new Color(0.15f, 0.75f, 0.65f));
                    HighlightAircraft("AJJ101");
                    break;
                case TutorialHighlight.TaxiToGate:
                    HighlightObject("Gate 1 Stand", new Color(0.15f, 0.75f, 0.65f));
                    HighlightAircraft("AJJ101");
                    break;
                case TutorialHighlight.Gate2:
                    HighlightObject("Gate 2 Stand", new Color(0.15f, 0.75f, 0.65f));
                    HighlightAircraft("AJJ202");
                    break;
                case TutorialHighlight.Pushback:
                    HighlightObject("Gate 2 Stand", new Color(0.15f, 0.75f, 0.65f));
                    HighlightAircraft("AJJ202");
                    break;
                case TutorialHighlight.TaxiToHold:
                    HighlightObject("Taxiway Main", new Color(0.25f, 0.62f, 0.78f));
                    HighlightObject("Hold Short A", new Color(0.9f, 0.72f, 0.18f));
                    HighlightAircraft("AJJ202");
                    break;
                case TutorialHighlight.HoldShort:
                    HighlightObject("Hold Short A", new Color(0.9f, 0.72f, 0.18f));
                    HighlightAircraft("AJJ202");
                    break;
                case TutorialHighlight.LineUp:
                    HighlightObject("Runway A", new Color(0.3f, 0.8f, 0.3f));
                    HighlightAircraft("AJJ202");
                    break;
            }
        }

        private void HighlightObject(string objectName, Color color)
        {
            var target = GameObject.Find(objectName);
            var renderer = target != null ? target.GetComponent<Renderer>() : null;
            if (renderer == null)
            {
                return;
            }

            if (!highlightedObjectColors.ContainsKey(renderer))
            {
                highlightedObjectColors.Add(renderer, renderer.material.color);
            }

            renderer.material.color = color;
        }

        private void HighlightAircraft(string flightNumber)
        {
            var aircraft = FindAircraft(flightNumber);
            if (aircraft != null)
            {
                aircraft.SetTutorialHighlighted(true);
            }
        }

        private void ResetTutorialHighlights()
        {
            foreach (var item in highlightedObjectColors)
            {
                if (item.Key != null)
                {
                    item.Key.material.color = item.Value;
                }
            }

            highlightedObjectColors.Clear();

            foreach (var aircraft in gameManager.Aircraft)
            {
                if (aircraft != null)
                {
                    aircraft.SetTutorialHighlighted(false);
                }
            }
        }

        private void UpdateResult()
        {
            resultText.text = "訓練完了\n\n"
                + $"安全度：{scoreManager.Safety}\n"
                + $"遅延：{Mathf.FloorToInt(scoreManager.Delay)}\n"
                + $"処理機数：{scoreManager.HandledAircraftCount}\n\n"
                + "講評:\n到着機と出発機を安全に処理できました。";
        }

        private void ExecuteRecommendedCommand()
        {
            var selected = GetSelectedAircraft();
            var recommended = selected != null ? GetRecommendedCommand(selected) : null;
            if (recommended.HasValue && selected.CanExecute(recommended.Value))
            {
                commandSystem.Execute(recommended.Value);
            }
        }

        private void ExecuteStripCommand()
        {
            ExecuteRecommendedCommand();
        }

        private void ExecuteStripCommand(AircraftCommand command, string runwayDesignator)
        {
            var selected = GetSelectedAircraft();
            if (selected != null && selected.CanExecute(command))
            {
                commandSystem.Execute(command, runwayDesignator);
            }
        }

        private GameObject CreateRoot(string name, Transform parent)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rectTransform = root.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return root;
        }

        private GameObject CreateFullscreenImage(string name, Transform parent, Color color)
        {
            var imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);
            var rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return imageObject;
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 pivot, Vector2 size)
        {
            return CreatePanel(name, parent, pivot, size, AnchorPreset.Center, Vector2.zero);
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 pivot, Vector2 size, AnchorPreset preset, Vector2 anchoredPosition)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rectTransform = panel.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, preset);
            if (preset == AnchorPreset.Center)
            {
                rectTransform.pivot = pivot;
            }
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            var image = panel.AddComponent<Image>();
            image.color = new Color(0.05f, 0.07f, 0.09f, 0.78f);
            return panel;
        }

        private Text CreatePanelText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, AnchorPreset preset, string text, int fontSize, TextAnchor alignment)
        {
            var panel = CreatePanel($"{name} Panel", parent, new Vector2(0.5f, 0.5f), size, preset, anchoredPosition);
            return CreateText($"{name} Text", panel.transform, text, fontSize, FontStyle.Normal, alignment, Vector2.zero, new Vector2(size.x - 48f, size.y - 28f));
        }

        private Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, AnchorPreset preset = AnchorPreset.Center)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rectTransform = textObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, preset);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            var uiText = textObject.AddComponent<Text>();
            uiText.text = text;
            uiText.font = uiFont;
            uiText.fontSize = fontSize;
            uiText.fontStyle = fontStyle;
            uiText.alignment = alignment;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Truncate;
            uiText.supportRichText = true;
            uiText.color = Color.white;
            uiText.raycastTarget = false;
            return uiText;
        }

        private Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var rectTransform = buttonObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.14f, 0.52f, 0.22f, 0.95f);

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.14f, 0.52f, 0.22f, 0.95f);
            colors.highlightedColor = new Color(0.18f, 0.66f, 0.28f, 1f);
            colors.pressedColor = new Color(0.1f, 0.42f, 0.18f, 1f);
            button.colors = colors;

            var labelText = CreateText("Label", buttonObject.transform, label, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(size.x - 24f, size.y - 14f));
            if (name == "Recommended Command")
            {
                commandButtonText = labelText;
            }

            return button;
        }

        private void ApplyAnchor(RectTransform rectTransform, AnchorPreset preset)
        {
            switch (preset)
            {
                case AnchorPreset.TopLeft:
                    rectTransform.anchorMin = new Vector2(0f, 1f);
                    rectTransform.anchorMax = new Vector2(0f, 1f);
                    rectTransform.pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.TopCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.TopRight:
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(1f, 1f);
                    break;
                case AnchorPreset.MiddleRight:
                    rectTransform.anchorMin = new Vector2(1f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    rectTransform.pivot = new Vector2(1f, 0.5f);
                    break;
                case AnchorPreset.BottomLeft:
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.zero;
                    rectTransform.pivot = Vector2.zero;
                    break;
                case AnchorPreset.BottomCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.BottomRight:
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = new Vector2(1f, 0f);
                    break;
                default:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
        }

        private string GetCurrentGuide()
        {
            var arrival = FindAircraft("AJJ101");
            var departure = FindAircraft("AJJ202");
            var selected = GetSelectedAircraft();

            if (arrival != null && arrival.CurrentState == AircraftState.Inbound)
            {
                return selected == arrival
                    ? "着陸許可を出しましょう。"
                    : "AJJ101のストリップを選択\nまず到着機を選びます。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.FinalApproach)
            {
                return "AJJ101が着陸中\n滑走路は1機ずつ使います。";
            }

            if (arrival != null && (arrival.CurrentState == AircraftState.LandingRoll || arrival.CurrentState == AircraftState.VacatingRunway))
            {
                return "滑走路離脱を待機\n出発機はまだ入れません。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.Waiting)
            {
                return selected == arrival
                    ? "スポットへ誘導しましょう。"
                    : "AJJ101のストリップを選択\nSPOT 01へ誘導します。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がSPOT 01へ移動中\n滑走路が空いたら出発機へ。";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure
                    ? "プッシュバックしましょう。"
                    : "AJJ202のストリップを選択\n出発準備を始めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.Pushbacking)
            {
                return "AJJ202が後退中\n地上走行の準備中です。";
            }

            if (departure != null && departure.CurrentState == AircraftState.PushbackReady)
            {
                return selected == departure
                    ? "滑走路手前へ進めましょう。"
                    : "AJJ202のストリップを選択\n滑走路手前へ誘導します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中\n入る前に一度止めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingPoint)
            {
                return selected == departure
                    ? "滑走路手前で待機させましょう。"
                    : "AJJ202のストリップを選択\n手前で待機させます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure
                    ? "滑走路上で待機させましょう。"
                    : "AJJ202のストリップを選択\n滑走路上で待機させます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure
                    ? "離陸許可を出しましょう。"
                    : "AJJ202のストリップを選択\n離陸許可を出します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TakeoffRoll)
            {
                return "AJJ202が離陸中\n完了まで監視します。";
            }

            return "次の航空機を選択してください";
        }

        private string GetInstructorHint()
        {
            var guide = GetCurrentGuide();
            if (guide.Contains("AJJ101"))
            {
                return "滑走路は基本的に1機ずつ使います。";
            }

            if (guide.Contains("Taxi to Spot") || guide.Contains("スポットへ誘導"))
            {
                return "着陸後はスポットへ誘導します。";
            }

            if (guide.Contains("AJJ202") || guide.Contains("Taxi to Holding Point"))
            {
                return "Groundでは滑走路手前までを整理します。";
            }

            if (guide.Contains("Line Up"))
            {
                return "ほかの機体がいないことを確認します。";
            }

            if (guide.Contains("Cleared for Takeoff"))
            {
                return "安全なら離陸許可を出しましょう。";
            }

            return instructorMessage;
        }

        private AircraftCommand? GetRecommendedCommand(AircraftController aircraft)
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
                if (aircraft.CanExecute(command))
                {
                    return command;
                }
            }

            return null;
        }

        private AircraftController FindAircraft(string flightNumber)
        {
            foreach (var aircraft in gameManager.Aircraft)
            {
                if (aircraft != null && aircraft.FlightNumber == flightNumber)
                {
                    return aircraft;
                }
            }

            return null;
        }

        private AircraftController GetSelectedAircraft()
        {
            return SelectionManager.Instance != null ? SelectionManager.Instance.SelectedAircraft : null;
        }

        private string GetCommandLabel(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "着陸許可\nClear to Land";
                case AircraftCommand.TaxiToGate:
                    return "スポットへ誘導\nTaxi to Spot";
                case AircraftCommand.Pushback:
                    return "プッシュバック\nPushback";
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導\nTaxi to Runway";
                case AircraftCommand.HoldTaxi:
                    return "停止\nHold Taxi";
                case AircraftCommand.ResumeTaxi:
                    return "再開\nResume Taxi";
                case AircraftCommand.HoldShort:
                    return "滑走路手前で待機\nHold Short";
                case AircraftCommand.LineUp:
                    return "滑走路上で待機\nLine Up and Wait";
                case AircraftCommand.ClearTakeoff:
                    return "離陸許可\nCleared for Takeoff";
                case AircraftCommand.Stop:
                    return "現在位置で待機\nHold Position";
                default:
                    return command.ToString();
            }
        }

        private string GetRunwaySpecificCommandLabel(AircraftCommand command, string runwayDesignator)
        {
            var runwayLabel = $"RWY {runwayDesignator}";
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return $"着陸許可 {runwayLabel}\nClear to Land {runwayLabel}";
                case AircraftCommand.TaxiToHold:
                    return $"滑走路手前へ誘導 {runwayLabel}\nTaxi to {runwayLabel}";
                case AircraftCommand.HoldShort:
                    return $"滑走路手前で待機 {runwayLabel}\nHold Short {runwayLabel}";
                case AircraftCommand.LineUp:
                    return $"滑走路上で待機 {runwayLabel}\nLine Up and Wait {runwayLabel}";
                case AircraftCommand.ClearTakeoff:
                    return $"離陸許可 {runwayLabel}\nCleared for Takeoff {runwayLabel}";
                default:
                    return GetCommandLabel(command);
            }
        }

        private List<string> GetCommandRunwayOptions(AircraftCommand command, AircraftController selected)
        {
            var options = new List<string>();
            if (!RequiresRunwayOption(command) || gameManager == null || gameManager.Airport == null)
            {
                return options;
            }

            if (command == AircraftCommand.TaxiToHold
                && selected != null
                && selected.FlightData != null
                && selected.FlightData.OperationType == "Departure"
                && selected.FlightData.HasActiveRunwayDesignator)
            {
                options.Add(selected.FlightData.ActiveRunwayDesignator);
                return options;
            }

            if (command == AircraftCommand.ClearLanding
                && selected != null
                && selected.FlightData != null
                && selected.FlightData.OperationType == "Arrival"
                && selected.FlightData.HasActiveRunwayDesignator)
            {
                options.Add(selected.FlightData.ActiveRunwayDesignator);
                return options;
            }

            if (command == AircraftCommand.ClearLanding || command == AircraftCommand.TaxiToHold)
            {
                foreach (var runwayDesignator in gameManager.Airport.GetPrimaryRunwayDirectionOptions())
                {
                    options.Add(runwayDesignator);
                }

                return options;
            }

            if (selected != null && selected.FlightData != null && selected.FlightData.HasActiveRunwayDesignator)
            {
                options.Add(selected.FlightData.ActiveRunwayDesignator);
            }

            return options;
        }

        private bool RequiresRunwayOption(AircraftCommand command)
        {
            return command == AircraftCommand.ClearLanding
                || command == AircraftCommand.TaxiToHold
                || command == AircraftCommand.HoldShort
                || command == AircraftCommand.LineUp
                || command == AircraftCommand.ClearTakeoff;
        }

        private string GetCommandShortLabel(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "着陸許可";
                case AircraftCommand.TaxiToGate:
                    return "スポットへ誘導";
                case AircraftCommand.Pushback:
                    return "プッシュバック";
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導";
                case AircraftCommand.HoldTaxi:
                    return "停止";
                case AircraftCommand.ResumeTaxi:
                    return "再開";
                case AircraftCommand.HoldShort:
                    return "滑走路手前で待機";
                case AircraftCommand.LineUp:
                    return "滑走路上で待機";
                case AircraftCommand.ClearTakeoff:
                    return "離陸許可";
                case AircraftCommand.Stop:
                    return "現在位置で待機";
                default:
                    return command.ToString();
            }
        }

        private string GetCommandDescription(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "滑走路が空いている時だけ出します。";
                case AircraftCommand.TaxiToGate:
                    return "着陸後、スポットへ移動させます。";
                case AircraftCommand.Pushback:
                    return "スポットから後退し、出発準備をします。";
                case AircraftCommand.TaxiToHold:
                    return "Groundの基本。滑走路手前へ進めます。";
                case AircraftCommand.HoldTaxi:
                    return "地上走行中の航空機を一時停止します。";
                case AircraftCommand.ResumeTaxi:
                    return "停止中の航空機の地上走行を再開します。";
                case AircraftCommand.HoldShort:
                    return "滑走路に入る前に止めます。";
                case AircraftCommand.LineUp:
                    return "離陸前に滑走路上で待機させます。";
                case AircraftCommand.ClearTakeoff:
                    return "滑走路が安全な時の離陸許可です。";
                case AircraftCommand.Stop:
                    return "移動を止めて安全確認します。";
                default:
                    return "次に必要な指示を選びます。";
            }
        }

        private string GetStateLabel(AircraftState state)
        {
            switch (state)
            {
                case AircraftState.Inbound:
                    return "到着待ち";
                case AircraftState.FinalApproach:
                    return "最終進入中";
                case AircraftState.LandingRoll:
                    return "着陸滑走中";
                case AircraftState.VacatingRunway:
                    return "滑走路離脱中";
                case AircraftState.TaxiToGate:
                    return "スポットへ移動中";
                case AircraftState.AtGate:
                    return "スポット待機中";
                case AircraftState.Pushbacking:
                    return "プッシュバック中";
                case AircraftState.PushbackReady:
                    return "地上走行準備完了";
                case AircraftState.TaxiToHold:
                    return "滑走路手前へ移動中";
                case AircraftState.TaxiHeld:
                    return "現在位置で待機中";
                case AircraftState.HoldingPoint:
                    return "滑走路手前に到着";
                case AircraftState.HoldingShort:
                    return "滑走路手前で待機中";
                case AircraftState.LiningUp:
                    return "滑走路上で待機中";
                case AircraftState.TakeoffRoll:
                    return "離陸滑走中";
                case AircraftState.AirborneDeparture:
                    return "離陸完了";
                case AircraftState.Waiting:
                    return "次の指示待ち";
                default:
                    return state.ToString();
            }
        }

        private string GetOperationLabel(string operationType)
        {
            switch (operationType)
            {
                case "Arrival":
                    return "到着";
                case "Departure":
                    return "出発";
                default:
                    return operationType;
            }
        }

        private enum AnchorPreset
        {
            Center,
            TopLeft,
            TopCenter,
            TopRight,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight
        }

        private class StripAction
        {
            public StripAction(string label, UnityEngine.Events.UnityAction execute, bool safe)
            {
                Label = label;
                Execute = execute;
                Safe = safe;
            }

            public string Label { get; private set; }
            public UnityEngine.Events.UnityAction Execute { get; private set; }
            public bool Safe { get; private set; }
        }

        private enum TutorialWaitMode
        {
            Info,
            Command,
            RunwayExitReady,
            ArrivalComplete,
            PushbackComplete,
            HoldingPointReady
        }

        private enum TutorialHighlight
        {
            None,
            RunwayA,
            RunwayInUse,
            ArrivalAircraft,
            ClearLanding,
            Gate1,
            TaxiToGate,
            Gate2,
            Pushback,
            TaxiToHold,
            HoldShort,
            LineUp
        }

        private class TutorialStep
        {
            public string Message { get; private set; }
            public TutorialWaitMode WaitMode { get; private set; }
            public bool WaitForCommand => WaitMode == TutorialWaitMode.Command;
            public AircraftCommand ExpectedCommand { get; private set; }
            public string ExpectedFlightNumber { get; private set; }
            public TutorialHighlight Highlight { get; private set; }

            public static TutorialStep Info(string message, TutorialHighlight highlight = TutorialHighlight.None)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.Info,
                    Highlight = highlight
                };
            }

            public bool AcceptsAircraft(AircraftController aircraft)
            {
                return string.IsNullOrEmpty(ExpectedFlightNumber)
                    || (aircraft != null && aircraft.FlightNumber == ExpectedFlightNumber);
            }

            public static TutorialStep Command(string message, AircraftCommand command, TutorialHighlight highlight = TutorialHighlight.None, string expectedFlightNumber = "")
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.Command,
                    ExpectedCommand = command,
                    ExpectedFlightNumber = expectedFlightNumber,
                    Highlight = highlight
                };
            }

            public static TutorialStep ArrivalComplete(string message, TutorialHighlight highlight)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.ArrivalComplete,
                    Highlight = highlight
                };
            }

            public static TutorialStep RunwayExitReady(string message, TutorialHighlight highlight)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.RunwayExitReady,
                    Highlight = highlight
                };
            }

            public static TutorialStep PushbackComplete(string message, TutorialHighlight highlight)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.PushbackComplete,
                    Highlight = highlight
                };
            }

            public static TutorialStep HoldingPointReady(string message, TutorialHighlight highlight)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.HoldingPointReady,
                    Highlight = highlight
                };
            }
        }

    }
}
