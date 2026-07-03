using ATCJourneyJapan.Aircraft;

namespace ATCJourneyJapan.Radio
{
    public static class CommandPhraseCatalog
    {
        public static CommandPhrase Get(AircraftCommand command)
        {
            switch (command)
            {
                case AircraftCommand.ClearLanding:
                    return new CommandPhrase(
                        "clear_to_land",
                        "管制官：AJJ101、A滑走路への着陸を許可します。",
                        "AJJ101, cleared to land Runway A.",
                        "AJJ101、A滑走路へ着陸します。",
                        "Cleared to land Runway A, AJJ101.",
                        "ajj101_clear_to_land");
                case AircraftCommand.TaxiToGate:
                    return new CommandPhrase(
                        "taxi_to_gate",
                        "管制官：AJJ101、SPOT 01へ地上走行してください。",
                        "AJJ101, taxi to Spot 01.",
                        "SPOT 01へ向かいます、AJJ101。",
                        "Taxi to Spot 01, AJJ101.",
                        "ajj101_taxi_to_gate");
                case AircraftCommand.Pushback:
                    return new CommandPhrase(
                        "pushback",
                        "管制官：AJJ202、プッシュバックを許可します。",
                        "AJJ202, pushback approved.",
                        "プッシュバックします、AJJ202。",
                        "Pushback approved, AJJ202.",
                        "ajj202_pushback");
                case AircraftCommand.TaxiToHold:
                    return new CommandPhrase(
                        "taxi_to_holding_point",
                        "管制官：AJJ202、A滑走路手前まで地上走行してください。",
                        "AJJ202, taxi to holding point Runway A.",
                        "A滑走路手前まで進みます、AJJ202。",
                        "Taxi to holding point Runway A, AJJ202.",
                        "ajj202_taxi_to_holding_point");
                case AircraftCommand.HoldShort:
                    return new CommandPhrase(
                        "hold_short",
                        "管制官：AJJ202、A滑走路手前で待機してください。",
                        "AJJ202, hold short of Runway A.",
                        "A滑走路手前で待機します、AJJ202。",
                        "Hold short of Runway A, AJJ202.",
                        "ajj202_hold_short");
                case AircraftCommand.LineUp:
                    return new CommandPhrase(
                        "line_up_and_wait",
                        "管制官：AJJ202、A滑走路に入り、待機してください。",
                        "AJJ202, line up and wait Runway A.",
                        "A滑走路に入り、待機します、AJJ202。",
                        "Line up and wait Runway A, AJJ202.",
                        "ajj202_line_up_and_wait");
                case AircraftCommand.ClearTakeoff:
                    return new CommandPhrase(
                        "cleared_for_takeoff",
                        "管制官：AJJ202、A滑走路からの離陸を許可します。",
                        "AJJ202, cleared for takeoff Runway A.",
                        "A滑走路から離陸します、AJJ202。",
                        "Cleared for takeoff Runway A, AJJ202.",
                        "ajj202_cleared_for_takeoff");
                default:
                    return null;
            }
        }
    }
}
