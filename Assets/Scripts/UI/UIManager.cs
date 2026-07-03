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
            AircraftCommand.TaxiToHold,
            AircraftCommand.LineUp,
            AircraftCommand.ClearTakeoff
        };

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

        public bool IsTutorialBlockingProgress
        {
            get
            {
                var step = CurrentTutorialStep;
                return tutorialActive && step != null && !step.WaitForCommand;
            }
        }

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
            guideText = CreatePanelText("Guide", hudRoot.transform, new Vector2(0f, 30f), new Vector2(780f, 92f), AnchorPreset.BottomCenter, string.Empty, 23, TextAnchor.MiddleCenter);

            selectedPanel = CreatePanel("Selected Aircraft", hudRoot.transform, new Vector2(0f, 0f), new Vector2(330f, 128f), AnchorPreset.BottomLeft, new Vector2(24f, 34f));
            selectedText = CreateText("Selected Text", selectedPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(286f, 94f));

            var commandPanel = CreatePanel("Command Panel", hudRoot.transform, new Vector2(0f, 0f), new Vector2(300f, 410f), AnchorPreset.MiddleRight, new Vector2(-12f, 0f));
            CreateText("Command Title", commandPanel.transform, "指示", 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 154f), new Vector2(250f, 34f));
            commandButton = CreateButton("Recommended Command", commandPanel.transform, string.Empty, new Vector2(0f, 74f), new Vector2(250f, 94f), 19);
            commandButton.onClick.AddListener(ExecuteRecommendedCommand);
            helpText = CreateText("Command Help", commandPanel.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0f, -96f), new Vector2(250f, 94f));
            commandStatusText = CreateText("Command Status", commandPanel.transform, string.Empty, 20, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, 28f), new Vector2(250f, 86f));

            tutorialPanel = CreatePanel("Tutorial Panel", hudRoot.transform, new Vector2(0.5f, 0f), new Vector2(760f, 150f), AnchorPreset.BottomCenter, new Vector2(0f, 138f));
            tutorialText = CreateText("Tutorial Text", tutorialPanel.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(-80f, 10f), new Vector2(520f, 94f));
            tutorialNextButton = CreateButton("Tutorial Next", tutorialPanel.transform, "次へ\nNext", new Vector2(270f, -26f), new Vector2(150f, 58f), 20);
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
            guideText.text = GetCurrentGuide();
            instructorText.text = $"教官コメント\n{GetInstructorHint()}";
            UpdateTutorialPanel();

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
                return;
            }

            tutorialText.text = step.Message;
            tutorialNextButton.gameObject.SetActive(!step.WaitForCommand);
        }

        private string GetCommandStatusText(AircraftController selected, AircraftCommand? recommended)
        {
            if (selected == null)
            {
                return "航空機を選択";
            }

            var step = CurrentTutorialStep;
            if (step != null && step.WaitForCommand && recommended.HasValue && step.ExpectedCommand != recommended.Value)
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
            }

            Refresh();
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
                TutorialStep.Info("ここはA滑走路です。\n離陸と着陸に使います。"),
                TutorialStep.Info("安全のため、滑走路は\n基本的に1機だけ使います。"),
                TutorialStep.Info("A滑走路は空いています。\nAJJ101に着陸許可を出します。"),
                TutorialStep.Command("AJJ101を選択して、\n『着陸許可』を押してください。", AircraftCommand.ClearLanding),
                TutorialStep.Info("AJJ101が着陸します。\n滑走路離脱まで見守ります。"),
                TutorialStep.Command("AJJ101を選択して、\nゲートへ誘導してください。", AircraftCommand.TaxiToGate),
                TutorialStep.Command("AJJ202を選択して、\n滑走路手前へ進めます。", AircraftCommand.TaxiToHold),
                TutorialStep.Command("AJJ202を選択して、\n滑走路上で待機させます。", AircraftCommand.LineUp),
                TutorialStep.Command("滑走路が安全なら、\n離陸許可を出します。", AircraftCommand.ClearTakeoff)
            };
        }

        private void UpdateResult()
        {
            resultText.text = "Stage Clear\n\n"
                + $"Safety: {scoreManager.Safety}\n"
                + $"Delay: {Mathf.FloorToInt(scoreManager.Delay)}\n"
                + $"Handled: {scoreManager.HandledAircraftCount}\n\n"
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
                    ? "滑走路手前へ進めましょう。"
                    : "AJJ202をクリック\n次は出発機を準備します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中\n入る前に一度止めます。";
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
                return "滑走路が空いたら出発機を進めます。";
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
                AircraftCommand.ClearLanding,
                AircraftCommand.TaxiToGate,
                AircraftCommand.TaxiToHold,
                AircraftCommand.LineUp,
                AircraftCommand.ClearTakeoff,
                AircraftCommand.HoldShort
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
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導\nTaxi to Holding Point";
                case AircraftCommand.HoldShort:
                    return "現在位置で待機\nHold Position";
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
                case AircraftCommand.TaxiToHold:
                    return "滑走路手前へ誘導";
                case AircraftCommand.HoldShort:
                    return "現在位置で待機";
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
                case AircraftCommand.TaxiToHold:
                    return "出発機を滑走路手前へ進めます。";
                case AircraftCommand.HoldShort:
                    return "安全確認のため待機させます。";
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
                    return "ゲートへ移動中";
                case AircraftState.AtGate:
                    return "ゲート待機中";
                case AircraftState.TaxiToHold:
                    return "滑走路手前へ移動中";
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

        private class TutorialStep
        {
            public string Message { get; private set; }
            public bool WaitForCommand { get; private set; }
            public AircraftCommand ExpectedCommand { get; private set; }

            public static TutorialStep Info(string message)
            {
                return new TutorialStep { Message = message };
            }

            public static TutorialStep Command(string message, AircraftCommand command)
            {
                return new TutorialStep
                {
                    Message = message,
                    WaitForCommand = true,
                    ExpectedCommand = command
                };
            }
        }
    }
}
