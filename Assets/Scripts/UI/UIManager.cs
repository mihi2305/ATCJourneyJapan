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
        private readonly List<string> commandLogEntries = new List<string>();
        private readonly List<Button> stripCommandOptionButtons = new List<Button>();
        private readonly List<Text> stripCommandOptionTexts = new List<Text>();
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
        private GameObject guidePanel;
        private GameObject tutorialPanel;
        private GameObject warningPanel;
        private GameObject resultPanel;
        private Text scoreText;
        private Text guideText;
        private Text instructorText;
        private Text selectedText;
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
            }

            commandButton.gameObject.SetActive(false);
            commandStatusText.gameObject.SetActive(false);
            commandStatusText.text = GetCommandStatusText(selected, recommended);

            warningPanel.SetActive(!string.IsNullOrEmpty(warningMessage));
            warningText.text = warningMessage;
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

            commandLogEntries.Add(GetControllerLogText(aircraft, phrase.ControllerJapaneseText));
            while (commandLogEntries.Count > 2)
            {
                commandLogEntries.RemoveAt(0);
            }

            Refresh();
        }

        private string GetControllerLogText(AircraftController aircraft, string phraseText)
        {
            if (aircraft == null)
            {
                return phraseText;
            }

            var runwayLabel = aircraft.FlightData != null ? aircraft.FlightData.RunwayShortDisplay : "A滑走路";
            return phraseText
                .Replace("AJJ101", aircraft.FlightNumber)
                .Replace("AJJ202", aircraft.FlightNumber)
                .Replace("A滑走路", runwayLabel);
        }

        private void UpdateCommandLogPanel()
        {
            var hasLogs = commandLogEntries.Count > 0;
            guidePanel.SetActive(hasLogs || CurrentTutorialStep == null);
            guideText.text = hasLogs
                ? $"管制ログ\n{string.Join("\n", commandLogEntries)}"
                : "管制ログ\n指示を出すと、ここに日本語で記録されます。";
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
            flightStripPanel = CreatePanel("Flight Strip Panel", hudRoot.transform, new Vector2(0f, 1f), new Vector2(340f, 500f), AnchorPreset.TopLeft, new Vector2(24f, -164f));
            flightStripPanel.GetComponent<Image>().color = new Color(0.025f, 0.032f, 0.04f, 0.82f);
            CreateText("Strip Title", flightStripPanel.transform, "STRIPS", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-122f, 218f), new Vector2(76f, 28f));
            CreateText("Arrival Header", flightStripPanel.transform, "到着  ARRIVAL", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-78f, 180f), new Vector2(220f, 28f));
            CreateText("Departure Header", flightStripPanel.transform, "出発  DEPARTURE", 17, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-78f, -34f), new Vector2(220f, 28f));
            CreateFlightStripButton("AJJ101", new Vector2(0f, 128f));
            CreateFlightStripButton("AJJ202", new Vector2(0f, -86f));

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
            CreateText("Mini Map Runway Header", minimapPanel.transform, "RWY 18L / 18R", 15, FontStyle.Bold, TextAnchor.MiddleRight, new Vector2(118f, 116f), new Vector2(140f, 28f));

            var contentObject = new GameObject("Mini Map Content");
            contentObject.transform.SetParent(minimapPanel.transform, false);
            minimapContent = contentObject.AddComponent<RectTransform>();
            ApplyAnchor(minimapContent, AnchorPreset.Center);
            minimapContent.sizeDelta = new Vector2(376f, 206f);
            minimapContent.anchoredPosition = new Vector2(0f, -22f);

            CreateMiniMapBlock("Mini Map Sea Background", Vector2.zero, new Vector2(MiniMapWidth, MiniMapHeight), new Color(0.02f, 0.28f, 0.42f, 0.82f));
            CreateMiniMapBlock("Mini Map Main Airport Island", WorldToMiniMap(new Vector3(3f, 0f, -8.2f)), WorldSizeToMiniMap(new Vector2(68f, 18.8f)), new Color(0.08f, 0.19f, 0.12f, 0.88f));
            CreateMiniMapBlock("Mini Map Future Runway Island", WorldToMiniMap(new Vector3(3f, 0f, 10.2f)), WorldSizeToMiniMap(new Vector2(36.5f, 5.4f)), new Color(0.11f, 0.21f, 0.13f, 0.88f));
            CreateMiniMapBlock("Mini Map Central Wedge Base", WorldToMiniMap(new Vector3(4.3f, 0f, 2.7f)), WorldSizeToMiniMap(new Vector2(16.8f, 2.8f)), new Color(0.1f, 0.23f, 0.14f, 0.9f));
            CreateMiniMapBlock("Mini Map Central Wedge Nose", WorldToMiniMap(new Vector3(6.6f, 0f, 5.1f)), WorldSizeToMiniMap(new Vector2(9.6f, 2.9f)), new Color(0.1f, 0.23f, 0.14f, 0.9f));
            CreateMiniMapBlock("Mini Map Right Connector Island", WorldToMiniMap(new Vector3(17.1f, 0f, 5.1f)), WorldSizeToMiniMap(new Vector2(4.6f, 10.6f)), new Color(0.1f, 0.22f, 0.14f, 0.9f));

            CreateMiniMapBlock("Mini Map Fighter Base Area", WorldToMiniMap(new Vector3(-13.5f, 0f, -9.7f)), WorldSizeToMiniMap(new Vector2(12.5f, 5.2f)), new Color(0.25f, 0.31f, 0.24f, 0.96f));
            CreateMiniMapBlock("Mini Map Support Base Area", WorldToMiniMap(new Vector3(-1.6f, 0f, -10f)), WorldSizeToMiniMap(new Vector2(11.2f, 5.9f)), new Color(0.29f, 0.34f, 0.29f, 0.96f));
            CreateMiniMapBlock("Mini Map Passenger Apron", WorldToMiniMap(new Vector3(17.1f, 0f, -10.1f)), WorldSizeToMiniMap(new Vector2(29.2f, 9.35f)), new Color(0.35f, 0.39f, 0.38f, 0.92f));
            CreateMiniMapBlock("Mini Map DOM Terminal", WorldToMiniMap(new Vector3(12.5f, 0f, -15.35f)), WorldSizeToMiniMap(new Vector2(18.2f, 2.35f)), new Color(0.58f, 0.64f, 0.62f, 0.98f));
            CreateMiniMapBlock("Mini Map INTL Terminal", WorldToMiniMap(new Vector3(25.7f, 0f, -15.35f)), WorldSizeToMiniMap(new Vector2(8.4f, 2.55f)), new Color(0.64f, 0.67f, 0.65f, 0.98f));
            CreateMiniMapBlock("Mini Map DOM Finger West", WorldToMiniMap(new Vector3(8.6f, 0f, -12.35f)), WorldSizeToMiniMap(new Vector2(1.55f, 5.25f)), new Color(0.58f, 0.64f, 0.62f, 0.98f));
            CreateMiniMapBlock("Mini Map DOM Finger East", WorldToMiniMap(new Vector3(16.3f, 0f, -12.35f)), WorldSizeToMiniMap(new Vector2(1.55f, 5.25f)), new Color(0.58f, 0.64f, 0.62f, 0.98f));
            CreateMiniMapBlock("Mini Map INTL Pier", WorldToMiniMap(new Vector3(25.7f, 0f, -12.55f)), WorldSizeToMiniMap(new Vector2(5.9f, 2.05f)), new Color(0.64f, 0.67f, 0.65f, 0.98f));

            minimapRunwayImage = CreateMiniMapBlock("Mini Map Runway A", WorldToMiniMap(new Vector3(3f, 0f, 0f)), WorldSizeToMiniMap(new Vector2(32.5f, 2.35f)), new Color(0.38f, 0.4f, 0.42f, 0.98f));
            CreateMiniMapBlock("Mini Map Runway B", WorldToMiniMap(new Vector3(3f, 0f, 10.2f)), WorldSizeToMiniMap(new Vector2(29.2f, 2.8f)), new Color(0.31f, 0.34f, 0.35f, 0.78f));
            CreateMiniMapBlock("Mini Map Taxiway Main", WorldToMiniMap(new Vector3(0f, 0f, -5f)), WorldSizeToMiniMap(new Vector2(26f, 1.24f)), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            CreateMiniMapBlock("Mini Map Taxiway West", WorldToMiniMap(new Vector3(-7f, 0f, -2.5f)), WorldSizeToMiniMap(new Vector2(1.7f, 5f)), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            CreateMiniMapBlock("Mini Map Taxiway East", WorldToMiniMap(new Vector3(12f, 0f, -2.5f)), WorldSizeToMiniMap(new Vector2(1.7f, 5f)), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            CreateMiniMapBlock("Mini Map Taxiway E Right", WorldToMiniMap(new Vector3(17.1f, 0f, 5.1f)), WorldSizeToMiniMap(new Vector2(1.25f, 10.2f)), new Color(0.2f, 0.34f, 0.42f, 0.98f));
            CreateMiniMapBlock("Mini Map Hold A", WorldToMiniMap(new Vector3(12f, 0f, -3f)), new Vector2(18f, 12f), new Color(0.92f, 0.72f, 0.16f, 0.98f));

            CreateMiniMapSpots();
            CreateText("Mini Map Runway A Label", minimapContent, "18L", 12, FontStyle.Bold, TextAnchor.MiddleCenter, WorldToMiniMap(new Vector3(3f, 0f, 1.8f)), new Vector2(54f, 18f));
            CreateText("Mini Map Runway B Label", minimapContent, "18R", 12, FontStyle.Bold, TextAnchor.MiddleCenter, WorldToMiniMap(new Vector3(3f, 0f, 12.2f)), new Vector2(54f, 18f));
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
            var position = WorldToMiniMap(worldPosition);
            var size = WorldSizeToMiniMap(new Vector2(standScale.x, standScale.z));
            size = new Vector2(Mathf.Max(size.x, aircraftSizeClass == AircraftSizeClass.Widebody ? 22f : 18f), Mathf.Max(size.y, aircraftSizeClass == AircraftSizeClass.Widebody ? 18f : 14f));
            CreateMiniMapBlock($"Mini Map {label}", position, size, GetMiniMapSpotColor(area, aircraftSizeClass));
            CreateText($"Mini Map {label} Label", minimapContent, label.Replace("SPOT ", "S"), 11, FontStyle.Bold, TextAnchor.MiddleCenter, position + new Vector2(0f, -20f), new Vector2(50f, 16f));
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
            var blockObject = new GameObject(name);
            blockObject.transform.SetParent(minimapContent, false);
            var rectTransform = blockObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
            var image = blockObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void UpdateMiniMap(AircraftController selected)
        {
            if (minimapPanel == null)
            {
                return;
            }

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
            var x = Mathf.InverseLerp(MiniMapWorldMinX, MiniMapWorldMaxX, worldPosition.x) * MiniMapWidth - MiniMapWidth * 0.5f;
            var y = Mathf.InverseLerp(MiniMapWorldMinZ, MiniMapWorldMaxZ, worldPosition.z) * MiniMapHeight - MiniMapHeight * 0.5f;
            return new Vector2(Mathf.Round(x), Mathf.Round(y));
        }

        private Vector2 WorldSizeToMiniMap(Vector2 worldSize)
        {
            var x = Mathf.Max(3f, worldSize.x / (MiniMapWorldMaxX - MiniMapWorldMinX) * MiniMapWidth);
            var y = Mathf.Max(3f, worldSize.y / (MiniMapWorldMaxZ - MiniMapWorldMinZ) * MiniMapHeight);
            return new Vector2(Mathf.Round(x), Mathf.Round(y));
        }

        private void CreateFlightStripButton(string flightNumber, Vector2 anchoredPosition)
        {
            var stripObject = new GameObject($"Flight Strip {flightNumber}");
            stripObject.transform.SetParent(flightStripPanel.transform, false);
            var rectTransform = stripObject.AddComponent<RectTransform>();
            ApplyAnchor(rectTransform, AnchorPreset.Center);
            rectTransform.sizeDelta = new Vector2(292f, 72f);
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

            var stripText = CreateText($"{flightNumber} Strip Text", stripObject.transform, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(250f, 52f));
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
        }

        private Vector2 GetFlightStripPosition(bool isArrival, int index)
        {
            var baseY = isArrival ? 128f : -86f;
            return new Vector2(0f, baseY - index * 82f);
        }

        private string GetFlightStripText(AircraftController aircraft, AircraftCommand? recommended)
        {
            var data = aircraft.FlightData;
            var target = GetStripTargetLabel(data);

            return $"<size=23>{data.FlightId}</size>\n<size=16>{target}</size>";
        }

        private void UpdateStripCommandButton(AircraftController selected, AircraftCommand? recommended, Vector2 selectedStripPosition, bool hasSelectedStrip)
        {
            if (stripCommandPopup == null || stripCommandOptionButtons.Count == 0)
            {
                return;
            }

            var canShow = hasSelectedStrip
                && selected != null
                && recommended.HasValue
                && selected.CanExecute(recommended.Value)
                && (CanAcceptCommand(selected, recommended.Value)
                    || !gameManager.CanExecuteRunwaySafetyCommand(selected, recommended.Value));

            stripCommandPopup.SetActive(canShow);
            if (!canShow)
            {
                SetStripCommandOptionButtonsActive(0);
                return;
            }

            var runwayOptions = GetCommandRunwayOptions(recommended.Value);
            var optionCount = runwayOptions.Count > 0 ? runwayOptions.Count : 1;
            var popupTransform = stripCommandPopup.GetComponent<RectTransform>();
            popupTransform.sizeDelta = GetStripCommandPopupSize(optionCount);
            popupTransform.anchoredPosition = new Vector2(selectedStripPosition.x + 306f, selectedStripPosition.y);
            ConfigureStripCommandOptions(recommended.Value, runwayOptions, gameManager.CanExecuteRunwaySafetyCommand(selected, recommended.Value));
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

        private void ConfigureStripCommandOptions(AircraftCommand command, IReadOnlyList<string> runwayOptions, bool safe)
        {
            var optionCount = runwayOptions.Count > 0 ? runwayOptions.Count : 1;
            EnsureStripCommandOptionButtonCount(optionCount);
            SetStripCommandOptionButtonsActive(optionCount);

            for (var index = 0; index < optionCount; index++)
            {
                var runwayDesignator = runwayOptions.Count > 0 ? runwayOptions[index] : string.Empty;
                var button = stripCommandOptionButtons[index];
                var label = stripCommandOptionTexts[index];
                var rectTransform = button.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = GetStripCommandOptionPosition(index, optionCount);
                label.text = string.IsNullOrEmpty(runwayDesignator)
                    ? GetCommandLabel(command)
                    : GetRunwaySpecificCommandLabel(command, runwayDesignator);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ExecuteStripCommand(command, runwayDesignator));
                ApplyStripCommandButtonSafetyStyle(button, safe);
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
            var routeLine = data.OperationType == "Arrival"
                ? $"ARRIVAL  FROM {data.Origin}"
                : $"DEPARTURE  TO {data.Destination}";
            return $"{data.FlightId}  {data.AircraftType}\n"
                + $"{routeLine}\n\n"
                + $"{timeLine}\n"
                + $"RWY：{data.ActiveRunwayDesignator}\n"
                + $"SPOT：{GetSpotNumber(data.SpotDisplayName)}\n"
                + $"状態：{data.CurrentState}\n"
                + $"担当：{data.ControllerPosition}\n"
                + $"次の指示：{recommendedLabel}";
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
                TutorialStep.Command("AJJ202のストリップから\n滑走路手前へ誘導します。", AircraftCommand.TaxiToHold, TutorialHighlight.TaxiToHold, "AJJ202"),
                TutorialStep.HoldingPointReady("AJJ202が誘導路を走行中です。\n滑走路手前で止めます。", TutorialHighlight.TaxiToHold),
                TutorialStep.Command("AJJ202のストリップから\n手前で待機させます。", AircraftCommand.HoldShort, TutorialHighlight.HoldShort, "AJJ202"),
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
                    return "滑走路手前へ誘導\nTaxi to Holding Point";
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
                case AircraftCommand.LineUp:
                    return $"滑走路上で待機 {runwayLabel}\nLine Up and Wait {runwayLabel}";
                case AircraftCommand.ClearTakeoff:
                    return $"離陸許可 {runwayLabel}\nCleared for Takeoff {runwayLabel}";
                default:
                    return GetCommandLabel(command);
            }
        }

        private List<string> GetCommandRunwayOptions(AircraftCommand command)
        {
            var options = new List<string>();
            if (!RequiresRunwayOption(command) || gameManager == null || gameManager.Airport == null)
            {
                return options;
            }

            foreach (var runwayDesignator in gameManager.Airport.GetPrimaryRunwayDirectionOptions())
            {
                options.Add(runwayDesignator);
            }

            return options;
        }

        private bool RequiresRunwayOption(AircraftCommand command)
        {
            return command == AircraftCommand.ClearLanding
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
