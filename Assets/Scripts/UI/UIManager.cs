using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCJourneyJapan.UI
{
    // Creates and refreshes the training HUD: start panel, guide, aircraft status, command help, instructor comments, and result panel.
    public class UIManager : MonoBehaviour
    {
        private readonly Dictionary<AircraftCommand, Button> commandButtons = new Dictionary<AircraftCommand, Button>();
        private GameManager gameManager;
        private ScoreManager scoreManager;
        private CommandSystem commandSystem;
        private Text scoreText;
        private Text selectedText;
        private Text guideText;
        private Text commandHelpText;
        private Text instructorText;
        private Text warningText;
        private Text resultText;
        private GameObject startPanel;
        private GameObject resultPanel;
        private Font defaultFont;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();
            CreateCanvas();
            ShowInstructorComment("まずは到着機を安全に着陸させよう");
            ShowCommandHelp("航空機を選択すると、使える指示と説明が表示されます。");
        }

        public void Refresh()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Safety: {scoreManager.Safety}\nDelay: {Mathf.FloorToInt(scoreManager.Delay)}\nHandled Aircraft Count: {scoreManager.HandledAircraftCount}";
            }

            RefreshSelectedAircraftPanel();
            RefreshGuide();
            RefreshCommandButtons();
        }

        public void ShowWarning(string message)
        {
            if (warningText == null)
            {
                return;
            }

            warningText.text = message;
            warningText.gameObject.SetActive(true);
            ShowInstructorComment("滑走路に2機を同時に入れないことが基本だ");
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
            if (resultPanel == null || resultText == null)
            {
                return;
            }

            resultText.text = "Stage Clear\n\n"
                              + $"Safety: {scoreManager.Safety}\n"
                              + $"Delay: {Mathf.FloorToInt(scoreManager.Delay)}\n"
                              + $"Handled Aircraft Count: {scoreManager.HandledAircraftCount}\n\n"
                              + "講評:\n初回訓練完了。到着機と出発機を安全に処理できました。\n次は、複数機が重なる状況に挑戦します。";
            resultPanel.SetActive(true);
            ShowInstructorComment("よし、安全に処理できている");
        }

        public void ShowCommandDescription(AircraftCommand command)
        {
            ShowCommandHelp(GetCommandDescription(command));
        }

        public void ShowInstructorComment(string message)
        {
            if (instructorText != null)
            {
                instructorText.text = $"教官コメント\n{message}";
            }
        }

        private void ShowCommandHelp(string message)
        {
            if (commandHelpText != null)
            {
                commandHelpText.text = $"コマンド説明\n{message}";
            }
        }

        private void RefreshSelectedAircraftPanel()
        {
            var selected = GetSelectedAircraft();
            if (selectedText == null)
            {
                return;
            }

            if (selected == null)
            {
                selectedText.text = "選択中航空機\n航空機を選択してください";
                ShowCommandHelp("航空機を選択すると、次に使う指示の意味を確認できます。");
                return;
            }

            var recommended = GetRecommendedCommand(selected);
            var recommendedText = recommended.HasValue ? GetCommandLabel(recommended.Value) : "現在は待機・監視";
            selectedText.text = $"選択中航空機\n便名: {selected.FlightNumber}\n現在の状態: {GetStateLabel(selected.CurrentState)}\n次の推奨指示: {recommendedText}";

            if (recommended.HasValue)
            {
                ShowCommandHelp(GetCommandDescription(recommended.Value));
            }
        }

        private void RefreshGuide()
        {
            if (guideText != null)
            {
                guideText.text = $"操作ガイド\n{GetCurrentGuide()}";
            }
        }

        private void RefreshCommandButtons()
        {
            var selected = GetSelectedAircraft();
            foreach (var pair in commandButtons)
            {
                var valid = gameManager.IsTrainingStarted && selected != null && selected.CanExecute(pair.Key);
                pair.Value.gameObject.SetActive(valid);
                pair.Value.interactable = valid;
            }
        }

        private string GetCurrentGuide()
        {
            if (!gameManager.IsTrainingStarted)
            {
                return "Start Training を押して訓練を開始してください";
            }

            var arrival = FindAircraft("AJJ101");
            var departure = FindAircraft("AJJ202");
            var selected = GetSelectedAircraft();

            if (arrival != null && arrival.CurrentState == AircraftState.Inbound)
            {
                return selected == arrival
                    ? "Clear Landing を押して、着陸許可を出してください"
                    : "AJJ101をクリックしてください";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.FinalApproach)
            {
                return "AJJ101が着陸中です。滑走路が空くまで監視してください";
            }

            if (arrival != null && (arrival.CurrentState == AircraftState.LandingRoll || arrival.CurrentState == AircraftState.VacatingRunway))
            {
                return "AJJ101が滑走路を離脱中です。完了したらTaxi to Gateを出します";
            }

            if (arrival != null && arrival.IsArrivalAircraft && arrival.CurrentState == AircraftState.Waiting)
            {
                return selected == arrival
                    ? "着陸後、Taxi to Gate を押してください"
                    : "AJJ101をクリックして、ゲートへ誘導してください";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がゲートへ移動中です。到着完了まで待ちましょう";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure
                    ? "Taxi to Hold を押して、滑走路手前まで移動させてください"
                    : "AJJ202をクリックしてください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中です。停止位置まで監視してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure
                    ? "Line Up を押して、滑走路上で待機させてください"
                    : "AJJ202をクリックして、Line Upを出してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure
                    ? "Clear Takeoff を押して、離陸許可を出してください"
                    : "AJJ202をクリックして、離陸許可を出してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TakeoffRoll)
            {
                return "AJJ202が離陸中です。離陸完了まで監視してください";
            }

            if (gameManager.StageClear)
            {
                return "訓練完了です。結果パネルを確認してください";
            }

            return "滑走路の安全を確認しながら、次の航空機を選択してください";
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
                if (gameManager.IsTrainingStarted && aircraft.CanExecute(command))
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
            var canvasObject = new GameObject("ATC Prototype HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            scoreText = CreateText("Score Text", canvasObject.transform, new Vector2(16f, -16f), new Vector2(290f, 92f), TextAnchor.UpperLeft, 18);
            selectedText = CreatePanelText("Selected Aircraft Panel", canvasObject.transform, new Vector2(16f, -118f), new Vector2(330f, 124f), TextAnchor.UpperLeft, 16);
            guideText = CreatePanelText("Guide Panel", canvasObject.transform, new Vector2(0f, 16f), new Vector2(760f, 76f), TextAnchor.MiddleCenter, 18, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            commandHelpText = CreatePanelText("Command Help Panel", canvasObject.transform, new Vector2(16f, -254f), new Vector2(330f, 116f), TextAnchor.UpperLeft, 15);
            instructorText = CreatePanelText("Instructor Comment Panel", canvasObject.transform, new Vector2(-16f, -16f), new Vector2(330f, 132f), TextAnchor.UpperLeft, 16, new Vector2(1f, 1f), new Vector2(1f, 1f));

            warningText = CreateText("Warning Text", canvasObject.transform, new Vector2(0f, -18f), new Vector2(760f, 44f), TextAnchor.UpperCenter, 20);
            warningText.color = new Color(1f, 0.35f, 0.24f);
            warningText.gameObject.SetActive(false);

            CreateCommandPanel(canvasObject.transform);
            CreateStartPanel(canvasObject.transform);
            CreateResultPanel(canvasObject.transform);
        }

        private void CreateCommandPanel(Transform parent)
        {
            var commandPanel = CreatePanel("Command Panel", parent, new Vector2(-16f, 16f), new Vector2(250f, 310f), new Vector2(1f, 0f), new Vector2(1f, 0f));

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
                CreateButton(commandPanel.transform, commands[i], i);
            }
        }

        private void CreateStartPanel(Transform parent)
        {
            startPanel = CreatePanel("Stage Start Panel", parent, Vector2.zero, new Vector2(620f, 250f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var title = CreateText("Title", startPanel.transform, new Vector2(0f, -24f), new Vector2(560f, 44f), TextAnchor.UpperCenter, 24, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            title.text = "Basic Runway Training";

            var body = CreateText("Body", startPanel.transform, new Vector2(0f, -72f), new Vector2(540f, 88f), TextAnchor.UpperCenter, 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            body.text = "新人管制官として、まずはA滑走路のみの基本運用を担当します。\n到着機を安全に着陸させ、出発機を離陸させましょう。";

            var button = CreatePlainButton("Start Training Button", startPanel.transform, new Vector2(0f, 28f), new Vector2(220f, 44f), "Start Training");
            button.onClick.AddListener(() =>
            {
                startPanel.SetActive(false);
                gameManager.StartTraining();
                Refresh();
            });
        }

        private void CreateResultPanel(Transform parent)
        {
            resultPanel = CreatePanel("Stage Clear Result Panel", parent, Vector2.zero, new Vector2(560f, 320f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            resultText = CreateText("Result Text", resultPanel.transform, new Vector2(0f, -26f), new Vector2(500f, 270f), TextAnchor.UpperCenter, 20, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            resultText.color = Color.white;
            resultPanel.SetActive(false);
        }

        private GameObject CreatePanel(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchor, Vector2 pivot)
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
            image.color = new Color(0.05f, 0.08f, 0.1f, 0.86f);
            return panel;
        }

        private Text CreatePanelText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize)
        {
            return CreatePanelText(objectName, parent, anchoredPosition, size, alignment, fontSize, new Vector2(0f, 1f), new Vector2(0f, 1f));
        }

        private Text CreatePanelText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize, Vector2 anchor, Vector2 pivot)
        {
            var panel = CreatePanel(objectName, parent, anchoredPosition, size, anchor, pivot);
            return CreateText("Text", panel.transform, new Vector2(12f, -10f), size - new Vector2(24f, 20f), alignment, fontSize);
        }

        private Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize)
        {
            return CreateText(objectName, parent, anchoredPosition, size, anchor, fontSize, anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f), anchor == TextAnchor.UpperCenter || anchor == TextAnchor.MiddleCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f));
        }

        private Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, TextAnchor alignment, int fontSize, Vector2 anchor, Vector2 pivot)
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
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void CreateButton(Transform parent, AircraftCommand command, int index)
        {
            var button = CreatePlainButton($"{command} Button", parent, new Vector2(0f, -index * 42f), new Vector2(220f, 36f), GetCommandLabel(command), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            button.onClick.AddListener(() => commandSystem.Execute(command));
            commandButtons[command] = button;
        }

        private Button CreatePlainButton(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, string label)
        {
            return CreatePlainButton(objectName, parent, anchoredPosition, size, label, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        }

        private Button CreatePlainButton(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, string label, Vector2 anchor, Vector2 pivot)
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
            image.color = new Color(0.14f, 0.24f, 0.28f, 0.96f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var labelText = CreateText("Label", buttonObject.transform, Vector2.zero, size, TextAnchor.MiddleCenter, 15, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            labelText.text = label;
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
                    return "Clear Landing: 着陸許可。滑走路が空いているときだけ出します。";
                case AircraftCommand.TaxiToGate:
                    return "Taxi to Gate: 着陸後、ゲートへ移動させます。";
                case AircraftCommand.TaxiToHold:
                    return "Taxi to Hold: 出発機を滑走路手前まで移動させます。";
                case AircraftCommand.HoldShort:
                    return "Hold Short: 滑走路手前で停止させ、安全を確保します。";
                case AircraftCommand.LineUp:
                    return "Line Up: 滑走路上で離陸待機させます。";
                case AircraftCommand.ClearTakeoff:
                    return "Clear Takeoff: 離陸許可。滑走路が安全なときだけ出します。";
                case AircraftCommand.Stop:
                    return "Stop: 移動を止めます。安全確認をやり直したいときに使います。";
                default:
                    return "選択中の航空機に必要な指示を選びます。";
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
                    return "ゲートへ地上走行中";
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
