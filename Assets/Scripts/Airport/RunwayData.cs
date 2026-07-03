namespace ATCJourneyJapan.Airport
{
    public class RunwayData
    {
        public RunwayData(
            string runwayId,
            string simpleNameJa,
            string simpleNameEn,
            string designatorA,
            string designatorB,
            string currentActiveDesignator,
            bool isUnlocked,
            string usageRole)
        {
            RunwayId = runwayId;
            SimpleNameJa = simpleNameJa;
            SimpleNameEn = simpleNameEn;
            DesignatorA = designatorA;
            DesignatorB = designatorB;
            CurrentActiveDesignator = currentActiveDesignator;
            IsUnlocked = isUnlocked;
            UsageRole = usageRole;
        }

        public string RunwayId { get; private set; }
        public string SimpleNameJa { get; private set; }
        public string SimpleNameEn { get; private set; }
        public string DesignatorA { get; private set; }
        public string DesignatorB { get; private set; }
        public string CurrentActiveDesignator { get; private set; }
        public bool IsUnlocked { get; private set; }
        public string UsageRole { get; private set; }
        public string RealDesignatorDisplay => $"RWY {CurrentActiveDesignator}";
        public string BilingualDisplay => $"{SimpleNameJa}（{RealDesignatorDisplay}）";
    }
}
