using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCJourneyJapan.UI
{
    public class UIManager : MonoBehaviour
    {
        private readonly AircraftCommand[] commandOrder =
        {
            AircraftCommand.ClearLanding,
            AircraftCommand.TaxiToGate,
            AircraftCommand.Pushback,
            AircraftCommand.TaxiToHold,
            AircraftCommand.HoldShort,
            AircraftCommand.LineUp,
            AircraftCommand.ClearTakeoff
        };

        private readonly Dictionary<Renderer, Color> highlightedObjectColors = new Dictionary<Renderer, Color>();
        private readonly List<string> commandLogEntries = new List<string>();
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
        private Text tutorialText;
        private Text warningText;
        private Text resultText;
        private Button commandButton;
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
            var step = CurrentTutorialStep;
            return step == null || (step.WaitForCommand && step.ExpectedCommand == command);
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
            guideText = CreatePanelText("Command Log", hudRoot.transform, new Vector2(0f, 12f), new Vector2(780f, 84f), AnchorPreset.BottomCenter, string.Empty, 16, TextAnchor.UpperLeft);
            guidePanel = guideText.transform.parent.gameObject;
            guidePanel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.7f);

            selectedPanel = CreatePanel("Selected Aircraft", hudRoot.transform, new Vector2(0f, 0f), new Vector2(330f, 128f), AnchorPreset.BottomLeft, new Vector2(24f, 34f));
            selectedText = CreateText("Selected Text", selectedPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(286f, 94f));

            var commandPanel = CreatePanel("Command Panel", hudRoot.transform, new Vector2(0f, 0f), new Vector2(300f, 410f), AnchorPreset.MiddleRight, new Vector2(-12f, 0f));
            CreateText("Command Title", commandPanel.transform, "指示", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 154f), new Vector2(250f, 34f));
            commandButton = CreateButton("Recommended Command", commandPanel.transform, string.Empty, new Vector2(0f, 74f), new Vector2(250f, 94f), 19);
            commandButton.onClick.AddListener(ExecuteRecommendedCommand);
            helpText = CreateText("Command Help", commandPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0f, -96f), new Vector2(250f, 94f));
            commandStatusText = CreateText("Command Status", commandPanel.transform, string.Empty, 20, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, 28f), new Vector2(250f, 86f));

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
            AdvanceTutorialIfReady();
            UpdateTutorialPanel();
            UpdateCommandLogPanel();

            selectedPanel.SetActive(selected != null);

            if (selected != null)
            {
                selectedText.text = $"便名: {selected.FlightNumber}\n状態: {GetStateLabel(selected.CurrentState)}\n推奨: {(recommended.HasValue ? GetCommandShortLabel(recommended.Value) : "監視")}";
                helpText.text = recommended.HasValue ? GetCommandDescription(recommended.Value) : commandHelpMessage;
            }
            else
            {
                helpText.text = "航空機を選択すると、使える指示が表示されます。";
            }

            var commandAllowedByTutorial = recommended.HasValue && CanAcceptCommand(recommended.Value);
            commandButton.gameObject.SetActive(commandAllowedByTutorial && selected != null && selected.CanExecute(recommended.Value));
            commandStatusText.gameObject.SetActive(!commandButton.gameObject.activeSelf);
            commandStatusText.text = GetCommandStatusText(selected, recommended);
            if (commandButton.gameObject.activeSelf)
            {
                commandButtonText.text = GetCommandLabel(recommended.Value);
            }

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
            var phrase = GetCommandPhrase(command, aircraft);
            if (phrase == null)
            {
                return;
            }

            commandLogEntries.Add(phrase.ControllerJapaneseText);
            while (commandLogEntries.Count > 2)
            {
                commandLogEntries.RemoveAt(0);
            }

            Refresh();
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
                TutorialStep.Command("滑走路が空いています。\nAJJ101に着陸許可を出しましょう。", AircraftCommand.ClearLanding, TutorialHighlight.ClearLanding),
                TutorialStep.RunwayExitReady("AJJ101が着陸中です。\n滑走路が使用中になります。", TutorialHighlight.RunwayInUse),
                TutorialStep.Info("着陸後は、次の飛行機のために\n滑走路を空けます。", TutorialHighlight.RunwayInUse),
                TutorialStep.Command("AJJ101をGate 1へ\n誘導しましょう。", AircraftCommand.TaxiToGate, TutorialHighlight.TaxiToGate),
                TutorialStep.ArrivalComplete("AJJ101がGate 1へ移動中です。\n到着完了まで見守ります。", TutorialHighlight.Gate1),
                TutorialStep.Info("AJJ101がGate 1に到着しました。\n到着機の基本処理は完了です。", TutorialHighlight.Gate1),
                TutorialStep.Info("出発訓練\nAJJ202を離陸させましょう。"),
                TutorialStep.Info("まずGate 2から\n出発準備をします。", TutorialHighlight.Gate2),
                TutorialStep.Command("AJJ202を選択し、\nプッシュバックします。", AircraftCommand.Pushback, TutorialHighlight.Pushback),
                TutorialStep.PushbackComplete("AJJ202が後退中です。\n地上走行の準備をします。", TutorialHighlight.Pushback),
                TutorialStep.Command("滑走路手前まで\n地上走行させましょう。", AircraftCommand.TaxiToHold, TutorialHighlight.TaxiToHold),
                TutorialStep.HoldingPointReady("AJJ202が誘導路を走行中です。\n滑走路手前で止めます。", TutorialHighlight.TaxiToHold),
                TutorialStep.Command("滑走路に入る前に\n手前で待機させます。", AircraftCommand.HoldShort, TutorialHighlight.HoldShort),
                TutorialStep.Info("Hold Shortは手前、\nLine Upは滑走路上で待機です。", TutorialHighlight.HoldShort),
                TutorialStep.Command("滑走路が空いたので\n滑走路上で待機させます。", AircraftCommand.LineUp, TutorialHighlight.LineUp),
                TutorialStep.Command("滑走路が安全なら、\n離陸許可を出しましょう。", AircraftCommand.ClearTakeoff)
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
                    : "AJJ101をクリック\nまず到着機を選びます。";
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
                    ? "ゲートへ誘導しましょう。"
                    : "AJJ101をクリック\nゲートへ誘導します。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がゲートへ移動中\n滑走路が空いたら出発機へ。";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure
                    ? "プッシュバックしましょう。"
                    : "AJJ202をクリック\n出発準備を始めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.Pushbacking)
            {
                return "AJJ202が後退中\n地上走行の準備中です。";
            }

            if (departure != null && departure.CurrentState == AircraftState.PushbackReady)
            {
                return selected == departure
                    ? "滑走路手前へ進めましょう。"
                    : "AJJ202をクリック\n滑走路手前へ誘導します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中\n入る前に一度止めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingPoint)
            {
                return selected == departure
                    ? "滑走路手前で待機させましょう。"
                    : "AJJ202をクリック\n手前で待機させます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure
                    ? "滑走路上で待機させましょう。"
                    : "AJJ202をクリック\n滑走路上で待機させます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure
                    ? "離陸許可を出しましょう。"
                    : "AJJ202をクリック\n離陸許可を出します。";
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

            if (guide.Contains("Taxi to Gate") || guide.Contains("ゲートへ誘導"))
            {
                return "着陸後は滑走路を空けましょう。";
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
                    return "ゲートへ誘導\nTaxi to Gate";
                case AircraftCommand.Pushback:
                    return "プッシュバック\nPushback";
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導\nTaxi to Holding Point";
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

        private string GetCommandShortLabel(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "着陸許可";
                case AircraftCommand.TaxiToGate:
                    return "ゲートへ誘導";
                case AircraftCommand.Pushback:
                    return "プッシュバック";
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導";
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
                    return "着陸後、ゲートへ移動させます。";
                case AircraftCommand.Pushback:
                    return "ゲートから後退し、出発準備をします。";
                case AircraftCommand.TaxiToHold:
                    return "Groundの基本。滑走路手前へ進めます。";
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

        private CommandPhrase GetCommandPhrase(AircraftCommand command, AircraftController aircraft)
        {
            var flightNumber = aircraft != null ? aircraft.FlightNumber : "航空機";
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、A滑走路への着陸を許可します。",
                        $"{flightNumber}, cleared to land Runway A.",
                        string.Empty,
                        string.Empty,
                        "tower_clear_landing_a");
                case AircraftCommand.TaxiToGate:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、Gate 1へ地上走行してください。",
                        $"{flightNumber}, taxi to Gate 1.",
                        string.Empty,
                        string.Empty,
                        "ground_taxi_gate_1");
                case AircraftCommand.Pushback:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、プッシュバックを許可します。",
                        $"{flightNumber}, pushback approved.",
                        string.Empty,
                        string.Empty,
                        "ground_pushback_approved");
                case AircraftCommand.TaxiToHold:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、A滑走路手前まで地上走行してください。",
                        $"{flightNumber}, taxi to holding point Runway A.",
                        string.Empty,
                        string.Empty,
                        "ground_taxi_holding_point_a");
                case AircraftCommand.HoldShort:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、A滑走路手前で待機してください。",
                        $"{flightNumber}, hold short of Runway A.",
                        string.Empty,
                        string.Empty,
                        "ground_hold_short_a");
                case AircraftCommand.LineUp:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、A滑走路に入り、待機してください。",
                        $"{flightNumber}, line up and wait Runway A.",
                        string.Empty,
                        string.Empty,
                        "tower_line_up_wait_a");
                case AircraftCommand.ClearTakeoff:
                    return new CommandPhrase(
                        command.ToString(),
                        $"管制官：{flightNumber}、A滑走路からの離陸を許可します。",
                        $"{flightNumber}, cleared for takeoff Runway A.",
                        string.Empty,
                        string.Empty,
                        "tower_clear_takeoff_a");
                default:
                    return null;
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
                    return "ゲートへ移動中";
                case AircraftState.AtGate:
                    return "ゲート待機中";
                case AircraftState.Pushbacking:
                    return "プッシュバック中";
                case AircraftState.PushbackReady:
                    return "地上走行準備完了";
                case AircraftState.TaxiToHold:
                    return "滑走路手前へ移動中";
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

        private enum AnchorPreset
        {
            Center,
            TopLeft,
            TopCenter,
            TopRight,
            MiddleRight,
            BottomLeft,
            BottomCenter
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

            public static TutorialStep Command(string message, AircraftCommand command, TutorialHighlight highlight = TutorialHighlight.None)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitMode = TutorialWaitMode.Command,
                    ExpectedCommand = command,
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

        private class CommandPhrase
        {
            public CommandPhrase(
                string commandId,
                string controllerJapaneseText,
                string controllerEnglishText,
                string futurePilotReadbackJapaneseText,
                string futurePilotReadbackEnglishText,
                string futureAudioKey)
            {
                CommandId = commandId;
                ControllerJapaneseText = controllerJapaneseText;
                ControllerEnglishText = controllerEnglishText;
                FuturePilotReadbackJapaneseText = futurePilotReadbackJapaneseText;
                FuturePilotReadbackEnglishText = futurePilotReadbackEnglishText;
                FutureAudioKey = futureAudioKey;
            }

            public string CommandId { get; private set; }
            public string ControllerJapaneseText { get; private set; }
            public string ControllerEnglishText { get; private set; }
            public string FuturePilotReadbackJapaneseText { get; private set; }
            public string FuturePilotReadbackEnglishText { get; private set; }
            public string FutureAudioKey { get; private set; }
        }
    }
}
