using ATCJourneyJapan.Aircraft;
using ATCJourneyJapan.Core;
using ATCJourneyJapan.Scoring;
using UnityEngine;

namespace ATCJourneyJapan.UI
{
    // Package-independent tutorial HUD. Uses IMGUI so the project compiles even if uGUI package resolution is broken.
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
        private string warningMessage;
        private string instructorMessage = "まずは到着機を安全に着陸させよう";
        private string commandHelpMessage = "航空機を選択すると、次に使う指示の意味を確認できます。";
        private bool resultVisible;
        private GUIStyle panelStyle;
        private GUIStyle textStyle;
        private GUIStyle titleStyle;
        private GUIStyle guideStyle;
        private GUIStyle buttonStyle;
        private GUIStyle recommendedButtonStyle;
        private GUIStyle warningStyle;

        public void Initialize(GameManager manager, ScoreManager scoring)
        {
            gameManager = manager;
            scoreManager = scoring;
            commandSystem = manager.GetComponent<CommandSystem>();
        }

        public void Refresh()
        {
            // OnGUI reads current game state directly.
        }

        public void ShowWarning(string message)
        {
            warningMessage = message;
            instructorMessage = "滑走路に2機を同時に入れないことが基本です。";
        }

        public void ClearWarning()
        {
            warningMessage = string.Empty;
        }

        public void ShowStageClear()
        {
            resultVisible = true;
            instructorMessage = "よし、安全に処理できている。";
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

            if (!gameManager.IsTrainingStarted)
            {
                DrawStartPanel();
                return;
            }

            if (gameManager.StageClear || resultVisible)
            {
                DrawResultPanel();
                return;
            }

            DrawTrainingHud();
        }

        private void DrawStartPanel()
        {
            var rect = CenteredRect(560f, 220f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 22f, rect.width - 48f, 34f), "Basic Runway Training", titleStyle);
            GUI.Label(new Rect(rect.x + 40f, rect.y + 72f, rect.width - 80f, 64f),
                "新人管制官として、まずはA滑走路のみの基本運用を担当します。\n到着機を安全に着陸させ、出発機を離陸させましょう。",
                guideStyle);

            if (GUI.Button(new Rect(rect.x + 190f, rect.y + 158f, 180f, 38f), "Start Training", buttonStyle))
            {
                gameManager.StartTraining();
            }
        }

        private void DrawTrainingHud()
        {
            DrawScore();
            DrawGuide();
            DrawInstructor();
            DrawSelectedAircraftUi();
            DrawWarning();
        }

        private void DrawScore()
        {
            GUI.Label(new Rect(16f, 14f, 260f, 62f),
                $"Safety: {scoreManager.Safety}\nDelay: {Mathf.FloorToInt(scoreManager.Delay)}\nHandled: {scoreManager.HandledAircraftCount}",
                textStyle);
        }

        private void DrawGuide()
        {
            var rect = new Rect((Screen.width - 520f) * 0.5f, Screen.height - 54f, 520f, 34f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, rect.height - 12f), GetCurrentGuide(), guideStyle);
        }

        private void DrawInstructor()
        {
            var rect = new Rect(Screen.width - 316f, 16f, 300f, 70f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, rect.height - 16f), $"教官コメント\n{GetInstructorHint()}", textStyle);
        }

        private void DrawSelectedAircraftUi()
        {
            var selected = GetSelectedAircraft();
            if (selected == null)
            {
                return;
            }

            var selectedRect = new Rect(16f, Screen.height - 132f, 270f, 82f);
            GUI.Box(selectedRect, string.Empty, panelStyle);

            var recommended = GetRecommendedCommand(selected);
            GUI.Label(new Rect(selectedRect.x + 10f, selectedRect.y + 8f, selectedRect.width - 20f, selectedRect.height - 16f),
                $"便名: {selected.FlightNumber}\n状態: {GetStateLabel(selected.CurrentState)}\n推奨: {(recommended.HasValue ? GetCommandLabel(recommended.Value) : "監視")}",
                textStyle);

            var helpRect = new Rect(296f, Screen.height - 108f, 330f, 58f);
            GUI.Box(helpRect, string.Empty, panelStyle);
            GUI.Label(new Rect(helpRect.x + 10f, helpRect.y + 8f, helpRect.width - 20f, helpRect.height - 16f),
                recommended.HasValue ? GetCommandDescription(recommended.Value) : commandHelpMessage,
                textStyle);

            DrawCommandButtons(selected);
        }

        private void DrawCommandButtons(AircraftController selected)
        {
            var rect = new Rect(Screen.width - 236f, Screen.height - 300f, 220f, 250f);
            GUI.Box(rect, string.Empty, panelStyle);

            var recommended = GetRecommendedCommand(selected);
            if (!recommended.HasValue)
            {
                GUI.Label(new Rect(rect.x + 16f, rect.y + 16f, rect.width - 32f, 44f), "現在は監視します", guideStyle);
                return;
            }

            foreach (var command in commandOrder)
            {
                if (command != recommended.Value || !selected.CanExecute(command))
                {
                    continue;
                }

                var buttonRect = new Rect(rect.x + 16f, rect.y + 18f, rect.width - 32f, 58f);
                var previousColor = GUI.backgroundColor;
                var pulse = 0.72f + Mathf.PingPong(Time.time * 1.8f, 0.28f);
                GUI.backgroundColor = new Color(0.16f, pulse, 0.24f, 1f);
                if (GUI.Button(buttonRect, GetCommandLabel(command), recommendedButtonStyle))
                {
                    commandSystem.Execute(command);
                }
                GUI.backgroundColor = previousColor;
                break;
            }
        }

        private void DrawWarning()
        {
            if (string.IsNullOrEmpty(warningMessage))
            {
                return;
            }

            GUI.Label(new Rect((Screen.width - 620f) * 0.5f, 14f, 620f, 30f), warningMessage, warningStyle);
        }

        private void DrawResultPanel()
        {
            var rect = CenteredRect(520f, 280f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 24f, rect.y + 22f, rect.width - 48f, rect.height - 44f),
                "Stage Clear\n\n"
                + $"Safety: {scoreManager.Safety}\n"
                + $"Delay: {Mathf.FloorToInt(scoreManager.Delay)}\n"
                + $"Handled Aircraft Count: {scoreManager.HandledAircraftCount}\n\n"
                + "講評:\n初回訓練完了。到着機と出発機を安全に処理できました。\n次は、複数機が重なる状況に挑戦します。",
                guideStyle);
        }

        private Rect CenteredRect(float width, float height)
        {
            return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        }

        private string GetCurrentGuide()
        {
            var arrival = FindAircraft("AJJ101");
            var departure = FindAircraft("AJJ202");
            var selected = GetSelectedAircraft();

            if (arrival != null && arrival.CurrentState == AircraftState.Inbound)
            {
                return selected == arrival
                    ? "着陸許可 / Clear to Land を押してください。滑走路が空いている時だけ着陸できます。"
                    : "AJJ101をクリックしてください。まず到着機を選びます。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.FinalApproach)
            {
                return "AJJ101が着陸中です。滑走路は1機ずつ安全に使います。";
            }

            if (arrival != null && (arrival.CurrentState == AircraftState.LandingRoll || arrival.CurrentState == AircraftState.VacatingRunway))
            {
                return "滑走路離脱を待っています。出発機はまだ滑走路へ入れません。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.Waiting)
            {
                return selected == arrival
                    ? "ゲートへ誘導 / Taxi to Gate を押してください。着陸後は滑走路を空けます。"
                    : "AJJ101をクリックしてください。着陸後はゲートへ誘導します。";
            }

            if (arrival != null && arrival.CurrentState == AircraftState.TaxiToGate)
            {
                return "AJJ101がゲートへ移動中です。滑走路が空いたら出発機を進めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.AtGate)
            {
                return selected == departure
                    ? "滑走路手前へ誘導 / Taxi to Holding Point を押してください。出発機を離陸準備位置へ進めます。"
                    : "AJJ202をクリックしてください。次は出発機を準備します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TaxiToHold)
            {
                return "AJJ202が滑走路手前へ移動中です。滑走路へ入る前に一度止めます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.HoldingShort)
            {
                return selected == departure
                    ? "滑走路上で待機 / Line Up and Wait を押してください。離陸前に滑走路上で待機させます。"
                    : "AJJ202をクリックしてください。滑走路上で待機させます。";
            }

            if (departure != null && departure.CurrentState == AircraftState.LiningUp)
            {
                return selected == departure
                    ? "離陸許可 / Cleared for Takeoff を押してください。滑走路が安全なら離陸できます。"
                    : "AJJ202をクリックしてください。離陸許可を出します。";
            }

            if (departure != null && departure.CurrentState == AircraftState.TakeoffRoll)
            {
                return "AJJ202が離陸中です。離陸完了まで監視します。";
            }

            return "次の航空機を選択してください";
        }

        private string GetInstructorHint()
        {
            var guide = GetCurrentGuide();
            if (guide.Contains("AJJ101"))
            {
                return "滑走路は離着陸に使う場所です。安全のため基本的に1機ずつ使います。";
            }

            if (guide.Contains("Taxi to Gate") || guide.Contains("ゲートへ誘導"))
            {
                return "着陸後は滑走路を空ける必要があります。到着機をゲートへ誘導しましょう。";
            }

            if (guide.Contains("AJJ202") || guide.Contains("Taxi to Holding Point"))
            {
                return "滑走路が空いたら、出発機を滑走路手前まで移動させます。";
            }

            if (guide.Contains("Line Up"))
            {
                return "離陸前に滑走路上で待機させます。ほかの機体がいないことを確認します。";
            }

            if (guide.Contains("Cleared for Takeoff"))
            {
                return "滑走路が安全なら、離陸許可を出しましょう。";
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

        private void EnsureStyles()
        {
            if (panelStyle != null)
            {
                return;
            }

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = Texture2D.grayTexture },
                padding = new RectOffset(8, 8, 6, 6)
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            titleStyle = new GUIStyle(textStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };

            guideStyle = new GUIStyle(textStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            recommendedButtonStyle = new GUIStyle(buttonStyle)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            warningStyle = new GUIStyle(guideStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.35f, 0.24f) }
            };
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

        private string GetCommandDescription(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return "滑走路が空いているときだけ出す着陸許可です。";
                case AircraftCommand.TaxiToGate:
                    return "着陸後、滑走路を空けるためゲートへ移動させます。";
                case AircraftCommand.TaxiToHold:
                    return "出発機を離陸準備のため滑走路手前まで移動させます。";
                case AircraftCommand.HoldShort:
                    return "安全確認のため現在位置で待機させます。";
                case AircraftCommand.LineUp:
                    return "離陸前に滑走路上で待機させます。";
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
