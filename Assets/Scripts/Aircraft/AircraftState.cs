namespace ATCJourneyJapan.Aircraft
{
    // Aircraft lifecycle states used by command availability, scoring, and runway safety checks.
    public enum AircraftState
    {
        Inbound,
        FinalApproach,
        LandingRoll,
        VacatingRunway,
        TaxiToGate,
        AtGate,
        Pushbacking,
        PushbackReady,
        TaxiToHold,
        TaxiHeld,
        HoldingPoint,
        HoldingShort,
        LiningUp,
        TakeoffRoll,
        AirborneDeparture,
        Waiting
    }
}
