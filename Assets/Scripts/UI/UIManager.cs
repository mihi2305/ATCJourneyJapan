using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCJourneyJapan.UI
{
    // Minimal training HUD. Non-button graphics never block world clicks; only actual Buttons receive UI raycasts.
    public class UIManager : MonoBehaviour
    {
        private readonly Dictionary<AircraftCommand, Button> commandButtons = new Dictionary<AircraftCommand, Button>();
        private GameManager gameManager;
        private ScoreManager scoreManager;
        private CommandSystem commandSystem;
        private Font defaultFont;
        private GameObject startPanel;
        private GameObject normalHudRoot;
        private GameObject selectedPanel;
        private GameObject commandHelpPanel;
        private GameObject commandPanel;
        private GameObject resultPanel;
        private Text scoreText;
        private Text guideText;
        private Text instructorText;
        private Text selectedText;
        private Text commandHelpText;
        private Text warningText;
        private Text resultText;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();
            CreateCanvas();
            SetTrainingUiVisible(false);
            startPanel.SetActive(true);
            resultPanel.SetActive(false);
        }

        public void Refresh()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Safety: {scoreManager.Safety}\nDelay: {Mathf.FloorToInt(scoreManager.Delay)}\nHandled: {scoreManager.HandledAircraftCount}";
            }

            if (!gameManager.IsTrainingStarted)
            {
                SetTrainingUiVisible(false);
                startPanel.SetActive(true);
                return;
            }

            if (gameManager.StageClear)
            {
                SetTrainingUiVisible(false);
                resultPanel.SetActive(true);
                return;
            }

            startPanel.SetActive(false);
            SetTrainingUiVisible(true);
            guideText.text = GetCurrentGuide();
            instructorText.text = $"教官コメント\n{GetInstructorHint()}";
            RefreshSelectedAircraftPanel();
            RefreshCommandButtons();
        }

        public void ShowWarning(string message)
        {
            if (warningText != null)
            {
                warningText.text = message;
                warningText.gameObject.SetActive(true);
            }
        }

        public void ClearWarning()
        {
            if (warningText != null)
            {
                warningText.text = string.Empty;
                warningText.gameObject.SetActive(false);
            }
        }

        public void ShowStageClear()
        {
            SetTrainingUiVisible(false);
            resultText.text = "Stage Clear\n\n"
                              + $"Safety: {scoreManager.Safety}\n"
                              + $"Delay: {Mathf.FloorToInt(scoreManager.Delay)}\n"
                              + $"Handled Aircraft Count: {scoreManager.HandledAircraftCount}\n\n"
                              + "講評:\n初回訓練完了。到着機と出発機を安全に処理できました。\n次は、複数機が重なる状況に挑戦します。";
            resultPanel.SetActive(true);
        }

        public void ShowCommandDescription(AircraftCommand command)
        {
            if (commandHelpText != null)
            {
                commandHelpText.text = GetCommandDescription(command);
            }
        }

        public void ShowInstructorComment(string message)
        {
            if (instructorText != null)
            {
                instructorText.text = $"教官コメント\n{message}";
            }
        }

        private void SetTrainingUiVisible(bool visible)
        {
            if (normalHudRoot != null)
            {
                normalHudRoot.SetActive(visible);
            }
        }

        private void RefreshSelectedAircraftPanel()
        {
            var selected = GetSelectedAircraft();
            var hasSelection = selected != null;
            selectedPanel.SetActive(hasSelection);
            commandHelpPanel.SetActive(hasSelection);
            commandPanel.SetActive(hasSelection);

            if (!hasSelection)
            {
                return;
            }

            var recommended = GetRecommendedCommand(selected);
            selectedText.text = $"便名: {selected.FlightNumber}\n状態: {GetStateLabel(selected.CurrentState)}\n推奨: {(recommended.HasValue ? GetCommandLabel(recommended.Value) : "監視")}";
            commandHelpText.text = recommended.HasValue ? GetCommandDescription(recommended.Value) : "安全を確認しながら、次の指示を待ちます。";
        }

        private void RefreshCommandButtons()
        {
            var selected = GetSelectedAircraft();
            foreach (var pair in commandButtons)
            {
                var valid = selected != null && selected.CanExecute(pair.Key);
                pair.Value.gameObject.SetActive(valid);
                pair.Value.interactable = valid;
            }
        }

        private string GetCurrentGuide()
        {
            var arrival = FindAircraft("AJJ101");
            var departure = FindAircraft("AJJ202");
            var selected = GetSelectedAircraft();

            if (arrival != null && arrival.CurrentState == AircraftState.Inbound)
            {
                return selected == arrival ? "Clear Landing を押してください" : "AJJ101をクリックしてください";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.FinalApproach)
            {
                return "AJJ101が着陸中です";
            }

            if (arrival != null && (arrival.CurrentState == AircraftState.LandingRoll || arrival.CurrentState == AircraftState.VacatingRunway))
            {
                return "滑走路離脱を待っています";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.Waiting)
            {
                return selected == arrival ? "Taxi to Gate を押してください" : "AJJ101をクリックしてください";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がゲートへ移動中です";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure ? "Taxi to Hold を押してください" : "AJJ202をクリックしてください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中です";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure ? "Line Up を押してください" : "AJJ202をクリックしてください";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure ? "Clear Takeoff を押してください" : "AJJ202をクリックしてください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TakeoffRoll)
            {
                return "AJJ202が離陸中です";
            }

            return gameManager.StageClear ? "訓練完了です" : "次の航空機を選択してください";
        }

        private string GetInstructorHint()
        {
            var guide = GetCurrentGuide();
            if (guide.Contains("AJJ101をクリック"))
            {
                return "まずは到着機を選び、着陸許可の判断をします。";
            }

            if (guide.Contains("Taxi to Gate"))
            {
                return "滑走路が空いたら、到着機をゲートへ逃がします。";
            }

            if (guide.Contains("AJJ202"))
            {
                return "到着機を処理したら、出発機を進めます。";
            }

            if (guide.Contains("Line Up") || guide.Contains("Clear Takeoff"))
            {
                return "滑走路に他機がいないことを確認してから指示します。";
            }

            return "滑走路に2機を同時に入れないことが基本です。";
        }

        private AircraftCommand? GetRecommendedCommand(AircraftController aircraft)
        {
            var commands = new[]
            {
                AircraftCommand.ClearLanding,
                AircraftCommand.TaxiToGate,
                AircraftCommand.TaxiToHold,
                AircraftCommand.LineUp,
                AircraftCommand.ClearTakeoff,
                AircraftCommand.HoldShort
            };

            foreach (var command in commands)
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

        private void CreateCanvas()
        {
            var canvasObject = new GameObject("ATC Tutorial HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            startPanel = CreatePanel("Start Panel", canvasObject.transform, Vector2.zero, new Vector2(560f, 230f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.88f);
            CreateText("Start Title", startPanel.transform, "Basic Runway Training", new Vector2(0f, -24f), new Vector2(500f, 34f), TextAnchor.MiddleCenter, 24, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            CreateText("Start Body", startPanel.transform, "新人管制官として、まずはA滑走路のみの基本運用を担当します。\n到着機を安全に着陸させ、出発機を離陸させましょう。", new Vector2(0f, -78f), new Vector2(500f, 68f), TextAnchor.MiddleCenter, 17, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            var startButton = CreateButton("Start Training Button", startPanel.transform, "Start Training", new Vector2(0f, 26f), new Vector2(210f, 42f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            startButton.onClick.AddListener(() =>
            {
                startPanel.SetActive(false);
                gameManager.StartTraining();
                Refresh();
            });

            normalHudRoot = new GameObject("Training HUD");
            normalHudRoot.transform.SetParent(canvasObject.transform);

            scoreText = CreateText("Score", normalHudRoot.transform, string.Empty, new Vector2(16f, -16f), new Vector2(220f, 62f), TextAnchor.UpperLeft, 15, new Vector2(0f, 1f), new Vector2(0f, 1f));
            guideText = CreateText("Guide", normalHudRoot.transform, string.Empty, new Vector2(0f, 18f), new Vector2(520f, 28f), TextAnchor.MiddleCenter, 18, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            instructorText = CreatePanelText("Instructor Panel", normalHudRoot.transform, string.Empty, new Vector2(-16f, -16f), new Vector2(300f, 72f), TextAnchor.UpperLeft, 14, new Vector2(1f, 1f), new Vector2(1f, 1f), 0.58f);
            warningText = CreateText("Warning", normalHudRoot.transform, string.Empty, new Vector2(0f, -12f), new Vector2(620f, 28f), TextAnchor.MiddleCenter, 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            warningText.color = new Color(1f, 0.32f, 0.22f);
            warningText.gameObject.SetActive(false);

            selectedPanel = CreatePanel("Selected Aircraft Panel", normalHudRoot.transform, new Vector2(16f, 16f), new Vector2(260f, 78f), new Vector2(0f, 0f), new Vector2(0f, 0f), 0.58f);
            selectedText = CreateText("Selected Text", selectedPanel.transform, string.Empty, new Vector2(10f, -8f), new Vector2(240f, 62f), TextAnchor.UpperLeft, 14, new Vector2(0f, 1f), new Vector2(0f, 1f));
            commandHelpPanel = CreatePanel("Command Help Panel", normalHudRoot.transform, new Vector2(286f, 16f), new Vector2(330f, 58f), new Vector2(0f, 0f), new Vector2(0f, 0f), 0.58f);
            commandHelpText = CreateText("Command Help Text", commandHelpPanel.transform, string.Empty, new Vector2(10f, -8f), new Vector2(310f, 42f), TextAnchor.UpperLeft, 13, new Vector2(0f, 1f), new Vector2(0f, 1f));
            commandPanel = CreatePanel("Command Buttons", normalHudRoot.transform, new Vector2(-16f, 16f), new Vector2(220f, 286f), new Vector2(1f, 0f), new Vector2(1f, 0f), 0.45f);
            CreateCommandButtons(commandPanel.transform);

            resultPanel = CreatePanel("Stage Clear Result Panel", canvasObject.transform, Vector2.zero, new Vector2(520f, 286f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.9f);
            resultText = CreateText("Result Text", resultPanel.transform, string.Empty, new Vector2(0f, -24f), new Vector2(470f, 240f), TextAnchor.UpperCenter, 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        }

        private void CreateCommandButtons(Transform parent)
        {
            var commands = new[]
            {
                AircraftCommand.ClearLanding,
                AircraftCommand.TaxiToGate,
                AircraftCommand.TaxiToHold,
                AircraftCommand.HoldShort,
                AircraftCommand.LineUp,
                AircraftCommand.ClearTakeoff,
                AircraftCommand.Stop
            };

            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var button = CreateButton($"{command} Button", parent, GetCommandLabel(command), new Vector2(0f, -12f - i * 38f), new Vector2(188f, 32f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
                button.onClick.AddListener(() => commandSystem.Execute(command));
                commandButtons[command] = button;
            }
        }

        private GameObject CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Vector2 pivot, float alpha)
        {
            var panel = new GameObject(objectName);
            panel.transform.SetParent(parent);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.05f, 0.08f, 0.1f, alpha);
            image.raycastTarget = false;
            return panel;
        }

        private Text CreatePanelText(string objectName, Transform parent, string text, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize, Vector2 anchor, Vector2 pivot, float alpha)
        {
            var panel = CreatePanel(objectName, parent, anchoredPosition, size, anchor, pivot, alpha);
            return CreateText("Text", panel.transform, text, new Vector2(10f, -8f), size - new Vector2(20f, 16f), alignment, fontSize, new Vector2(0f, 1f), new Vector2(0f, 1f));
        }

        private Text CreateText(string objectName, Transform parent, string textValue, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize, Vector2 anchor, Vector2 pivot)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.text = textValue;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            var buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.13f, 0.24f, 0.28f, 0.96f);
            image.raycastTarget = true;
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var labelText = CreateText("Label", buttonObject.transform, label, Vector2.zero, size, TextAnchor.MiddleCenter, 14, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            labelText.raycastTarget = false;
            return button;
        }

        private string GetCommandLabel(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "Clear Landing";
                case AircraftCommand.TaxiToGate:
                    return "Taxi to Gate";
                case AircraftCommand.TaxiToHold:
                    return "Taxi to Hold";
                case AircraftCommand.HoldShort:
                    return "Hold Short";
                case AircraftCommand.LineUp:
                    return "Line Up";
                case AircraftCommand.ClearTakeoff:
                    return "Clear Takeoff";
                case AircraftCommand.Stop:
                    return "Stop";
                default:
                    return command.ToString();
            }
        }

        private string GetCommandDescription(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "着陸許可。滑走路が空いているときだけ出します。";
                case AircraftCommand.TaxiToGate:
                    return "着陸後、ゲートへ移動させます。";
                case AircraftCommand.TaxiToHold:
                    return "出発機を滑走路手前まで移動させます。";
                case AircraftCommand.HoldShort:
                    return "滑走路手前で停止させます。";
                case AircraftCommand.LineUp:
                    return "滑走路上で離陸待機させます。";
                case AircraftCommand.ClearTakeoff:
                    return "滑走路が安全なときに出す離陸許可です。";
                case AircraftCommand.Stop:
                    return "移動を止め、安全確認をやり直します。";
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
    }
}
