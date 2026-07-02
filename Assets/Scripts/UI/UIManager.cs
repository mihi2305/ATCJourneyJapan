using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCJourneyJapan.UI
{
    // Creates and refreshes the prototype HUD: score, selected aircraft, valid command buttons, warnings, and clear state.
    public class UIManager : MonoBehaviour
    {
        private readonly Dictionary<AircraftCommand, Button> commandButtons = new Dictionary<AircraftCommand, Button>();
        private GameManager gameManager;
        private ScoreManager scoreManager;
        private CommandSystem commandSystem;
        private Text scoreText;
        private Text selectedText;
        private Text warningText;
        private Text clearText;
        private Font defaultFont;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            EnsureEventSystem();
            CreateCanvas();
        }

        public void Refresh()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Safety: {scoreManager.Safety}\nDelay: {Mathf.FloorToInt(scoreManager.Delay)}\nHandled Aircraft Count: {scoreManager.HandledAircraftCount}";
            }

            var selected = SelectionManager.Instance != null ? SelectionManager.Instance.SelectedAircraft : null;
            selectedText.text = selected == null ? "Selected: None" : $"Selected: {selected.FlightNumber}\nState: {selected.CurrentState}";

            foreach (var pair in commandButtons)
            {
                var valid = selected != null && selected.CanExecute(pair.Key);
                pair.Value.gameObject.SetActive(valid);
                pair.Value.interactable = valid;
            }
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
            if (clearText != null)
            {
                clearText.gameObject.SetActive(true);
            }
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
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            scoreText = CreateText("Score Text", canvasObject.transform, new Vector2(16f, -16f), new Vector2(270f, 92f), TextAnchor.UpperLeft, 18);
            selectedText = CreateText("Selected Aircraft Text", canvasObject.transform, new Vector2(16f, -120f), new Vector2(310f, 74f), TextAnchor.UpperLeft, 17);
            warningText = CreateText("Warning Text", canvasObject.transform, new Vector2(0f, -18f), new Vector2(720f, 44f), TextAnchor.UpperCenter, 20);
            warningText.color = new Color(1f, 0.35f, 0.24f);
            warningText.gameObject.SetActive(false);

            clearText = CreateText("Stage Clear Text", canvasObject.transform, new Vector2(0f, -78f), new Vector2(420f, 60f), TextAnchor.UpperCenter, 34);
            clearText.text = "Stage Clear";
            clearText.color = new Color(0.42f, 1f, 0.62f);
            clearText.gameObject.SetActive(false);

            var commandPanel = new GameObject("Command Panel");
            commandPanel.transform.SetParent(canvasObject.transform);
            var panelRect = commandPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.anchoredPosition = new Vector2(-16f, 16f);
            panelRect.sizeDelta = new Vector2(230f, 310f);

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

        private Text CreateText(string objectName, Transform parent, Vector2 anchoredPosition, Vector2 size, TextAnchor anchor, int fontSize)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = anchor == TextAnchor.UpperCenter ? new Vector2(0.5f, 1f) : new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<Text>();
            text.font = defaultFont;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private void CreateButton(Transform parent, AircraftCommand command, int index)
        {
            var buttonObject = new GameObject($"{command} Button");
            buttonObject.transform.SetParent(parent);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -index * 42f);
            rect.sizeDelta = new Vector2(0f, 36f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.18f, 0.22f, 0.92f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => commandSystem.Execute(command));
            commandButtons[command] = button;

            var label = CreateText("Label", buttonObject.transform, Vector2.zero, new Vector2(210f, 34f), TextAnchor.MiddleCenter, 15);
            label.text = GetCommandLabel(command);
            label.color = Color.white;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
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
    }
}
