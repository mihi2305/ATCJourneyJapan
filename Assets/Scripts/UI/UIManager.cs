using System.Collections.Generic;
using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;

namespace ATCJourneyJapan.UI
{
    // Draws the training HUD without external UI package references, so C# can compile even before uGUI finishes resolving.
    public class UIManager : MonoBehaviour
    {
        private readonly AircraftCommand[] commands =
        {
            AircraftCommand.ClearLanding,
            AircraftCommand.TaxiToGate,
            AircraftCommand.TaxiToHold,
            AircraftCommand.HoldShort,
            AircraftCommand.LineUp,
            AircraftCommand.ClearTakeoff,
            AircraftCommand.Stop
        };

        private GameManager gameManager;
        private ScoreManager scoreManager;
        private CommandSystem commandSystem;
        private string warningMessage;
        private string commandHelpMessage = "航空機を選択すると、使える指示と説明が表示されます。";
        private string instructorMessage = "まずは到着機を安全に着陸させよう";
        private bool resultVisible;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle centerTextStyle;
        private GUIStyle warningStyle;
        private GUIStyle buttonStyle;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
        }

        public void Refresh()
        {
            // IMGUI redraws from current state in OnGUI.
        }

        public void ShowWarning(string message)
        {
            warningMessage = message;
            ShowInstructorComment("滑走路に2機を同時に入れないことが基本だ");
        }

        public void ClearWarning()
        {
            warningMessage = string.Empty;
        }

        public void ShowStageClear()
        {
            resultVisible = true;
            ShowInstructorComment("よし、安全に処理できている");
        }

        public void ShowCommandDescription(AircraftCommand command)
        {
            commandHelpMessage = GetCommandDescription(command);
        }

        public void ShowInstructorComment(string message)
        {
            instructorMessage = message;
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawScorePanel();
            DrawSelectedAircraftPanel();
            DrawCommandHelpPanel();
            DrawInstructorPanel();
            DrawCommandPanel();
            DrawGuidePanel();
            DrawWarning();

            if (!gameManager.IsTrainingStarted)
            {
                DrawStartPanel();
            }

            if (resultVisible)
            {
                DrawResultPanel();
            }
        }

        private void DrawScorePanel()
        {
            GUI.Box(new Rect(16f, 16f, 300f, 92f), string.Empty, panelStyle);
            GUI.Label(new Rect(28f, 28f, 276f, 70f), $"Safety: {scoreManager.Safety}\nDelay: {Mathf.FloorToInt(scoreManager.Delay)}\nHandled Aircraft Count: {scoreManager.HandledAircraftCount}", textStyle);
        }

        private void DrawSelectedAircraftPanel()
        {
            var selected = GetSelectedAircraft();
            var rect = new Rect(16f, 120f, 340f, 128f);
            GUI.Box(rect, string.Empty, panelStyle);

            if (selected == null)
            {
                GUI.Label(new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height - 24f), "選択中航空機\n航空機を選択してください", textStyle);
                commandHelpMessage = "航空機を選択すると、次に使う指示の意味を確認できます。";
                return;
            }

            var recommended = GetRecommendedCommand(selected);
            var recommendedText = recommended.HasValue ? GetCommandLabel(recommended.Value) : "現在は待機・監視";
            GUI.Label(new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, rect.height - 24f),
                $"選択中航空機\n便名: {selected.FlightNumber}\n現在の状態: {GetStateLabel(selected.CurrentState)}\n次の推奨指示: {recommendedText}", textStyle);

            if (recommended.HasValue)
            {
                commandHelpMessage = GetCommandDescription(recommended.Value);
            }
        }

        private void DrawCommandHelpPanel()
        {
            var rect = new Rect(16f, 262f, 340f, 118f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, rect.height - 20f), $"コマンド説明\n{commandHelpMessage}", textStyle);
        }

        private void DrawInstructorPanel()
        {
            var rect = new Rect(Screen.width - 356f, 16f, 340f, 136f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, rect.height - 20f), $"教官コメント\n{instructorMessage}", textStyle);
        }

        private void DrawCommandPanel()
        {
            var selected = GetSelectedAircraft();
            var rect = new Rect(Screen.width - 276f, Screen.height - 326f, 260f, 310f);
            GUI.Box(rect, string.Empty, panelStyle);

            for (var i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                var valid = gameManager.IsTrainingStarted && selected != null && selected.CanExecute(command);
                if (!valid)
                {
                    continue;
                }

                var buttonRect = new Rect(rect.x + 18f, rect.y + 18f + i * 40f, rect.width - 36f, 34f);
                if (GUI.Button(buttonRect, GetCommandLabel(command), buttonStyle))
                {
                    commandSystem.Execute(command);
                }
            }
        }

        private void DrawGuidePanel()
        {
            var rect = new Rect((Screen.width - 760f) * 0.5f, Screen.height - 94f, 760f, 78f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, rect.height - 20f), $"操作ガイド\n{GetCurrentGuide()}", centerTextStyle);
        }

        private void DrawWarning()
        {
            if (string.IsNullOrEmpty(warningMessage))
            {
                return;
            }

            GUI.Label(new Rect((Screen.width - 780f) * 0.5f, 18f, 780f, 40f), warningMessage, warningStyle);
        }

        private void DrawStartPanel()
        {
            var rect = new Rect((Screen.width - 640f) * 0.5f, (Screen.height - 260f) * 0.5f, 640f, 260f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 24f, rect.width - 48f, 40f), "Basic Runway Training", titleStyle);
            GUI.Label(new Rect(rect.x + 44f, rect.y + 78f, rect.width - 88f, 90f),
                "新人管制官として、まずはA滑走路のみの基本運用を担当します。\n到着機を安全に着陸させ、出発機を離陸させましょう。", centerTextStyle);

            if (GUI.Button(new Rect(rect.x + 210f, rect.y + 186f, 220f, 44f), "Start Training", buttonStyle))
            {
                gameManager.StartTraining();
            }
        }

        private void DrawResultPanel()
        {
            var rect = new Rect((Screen.width - 580f) * 0.5f, (Screen.height - 330f) * 0.5f, 580f, 330f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 30f, rect.y + 26f, rect.width - 60f, rect.height - 52f),
                "Stage Clear\n\n"
                + $"Safety: {scoreManager.Safety}\n"
                + $"Delay: {Mathf.FloorToInt(scoreManager.Delay)}\n"
                + $"Handled Aircraft Count: {scoreManager.HandledAircraftCount}\n\n"
                + "講評:\n初回訓練完了。到着機と出発機を安全に処理できました。\n次は、複数機が重なる状況に挑戦します。",
                centerTextStyle);
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
                return selected == arrival ? "Clear Landing を押して、着陸許可を出してください" : "AJJ101をクリックしてください";
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
                return selected == arrival ? "着陸後、Taxi to Gate を押してください" : "AJJ101をクリックして、ゲートへ誘導してください";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がゲートへ移動中です。到着完了まで待ちましょう";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure ? "Taxi to Hold を押して、滑走路手前まで移動させてください" : "AJJ202をクリックしてください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中です。停止位置まで監視してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure ? "Line Up を押して、滑走路上で待機させてください" : "AJJ202をクリックして、Line Upを出してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure ? "Clear Takeoff を押して、離陸許可を出してください" : "AJJ202をクリックして、離陸許可を出してください";
            }

            if (departure != null && departure.CurrentState == AircraftState.TakeoffRoll)
            {
                return "AJJ202が離陸中です。離陸完了まで監視してください";
            }

            return gameManager.StageClear ? "訓練完了です。結果パネルを確認してください" : "滑走路の安全を確認しながら、次の航空機を選択してください";
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

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.grayTexture },
                padding = new RectOffset(12, 12, 10, 10)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            centerTextStyle = new GUIStyle(textStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };

            warningStyle = new GUIStyle(centerTextStyle)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.35f, 0.24f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
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
