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
                        "{CALLSIGN}、{RUNWAY}への着陸を許可します。",
                        "{CALLSIGN}, cleared to land {RUNWAY}.",
                        "{RUNWAY}、着陸します。",
                        "Cleared to land {RUNWAY}, {CALLSIGN}.",
                        "ajj101_clear_to_land");
                case AircraftCommand.TaxiToGate:
                    return new CommandPhrase(
                        "taxi_to_gate",
                        "{CALLSIGN}、{SPOT}へ地上走行してください。",
                        "{CALLSIGN}, taxi to {SPOT}.",
                        "{SPOT}へ向かいます。",
                        "Taxi to {SPOT}, {CALLSIGN}.",
                        "ajj101_taxi_to_gate");
                case AircraftCommand.Pushback:
                    return new CommandPhrase(
                        "pushback",
                        "{CALLSIGN}、プッシュバックを許可します。",
                        "{CALLSIGN}, pushback approved.",
                        "プッシュバックします。",
                        "Pushback approved, {CALLSIGN}.",
                        "ajj202_pushback");
                case AircraftCommand.TaxiToHold:
                    return new CommandPhrase(
                        "taxi_to_holding_point",
                        "{CALLSIGN}、{RUNWAY}手前まで地上走行してください。",
                        "{CALLSIGN}, taxi to holding point {RUNWAY}.",
                        "{RUNWAY}手前まで進みます。",
                        "Taxi to holding point {RUNWAY}, {CALLSIGN}.",
                        "ajj202_taxi_to_holding_point");
                case AircraftCommand.HoldTaxi:
                    return new CommandPhrase(
                        "hold_taxi",
                        "{CALLSIGN}、現在位置で待機してください。",
                        "{CALLSIGN}, hold position.",
                        "現在位置で待機します。",
                        "Holding position, {CALLSIGN}.",
                        "ajj202_hold_taxi");
                case AircraftCommand.ResumeTaxi:
                    return new CommandPhrase(
                        "resume_taxi",
                        "{CALLSIGN}、地上走行を再開してください。",
                        "{CALLSIGN}, resume taxi.",
                        "地上走行を再開します。",
                        "Resume taxi, {CALLSIGN}.",
                        "ajj202_resume_taxi");
                case AircraftCommand.HoldShort:
                    return new CommandPhrase(
                        "hold_short",
                        "{CALLSIGN}、{RUNWAY}手前で待機してください。",
                        "{CALLSIGN}, hold short of {RUNWAY}.",
                        "{RUNWAY}手前で待機します。",
                        "Hold short of {RUNWAY}, {CALLSIGN}.",
                        "ajj202_hold_short");
                case AircraftCommand.LineUp:
                    return new CommandPhrase(
                        "line_up_and_wait",
                        "{CALLSIGN}、{RUNWAY}に入り、待機してください。",
                        "{CALLSIGN}, line up and wait {RUNWAY}.",
                        "{RUNWAY}に入り、待機します。",
                        "Line up and wait {RUNWAY}, {CALLSIGN}.",
                        "ajj202_line_up_and_wait");
                case AircraftCommand.ClearTakeoff:
                    return new CommandPhrase(
                        "cleared_for_takeoff",
                        "{CALLSIGN}、{RUNWAY}からの離陸を許可します。",
                        "{CALLSIGN}, cleared for takeoff {RUNWAY}.",
                        "{RUNWAY}から離陸します。",
                        "Cleared for takeoff {RUNWAY}, {CALLSIGN}.",
                        "ajj202_cleared_for_takeoff");
                default:
                    return null;
            }
        }
    }
}
