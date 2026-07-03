namespace ATCJourneyJapan.Radio
{
    // Phrase data for one command. Phase 1.8 displays only ControllerJapaneseText.
    public class CommandPhrase
    {
        public CommandPhrase(
            string commandId,
            string controllerJapaneseText,
            string controllerEnglishText,
            string futurePilotReadbackJapaneseText,
            string futurePilotReadbackEnglishText,
            string futureAudioKey)
        {
            CommandId = commandId;
            ControllerJapaneseText = controllerJapaneseText;
            ControllerEnglishText = controllerEnglishText;
            FuturePilotReadbackJapaneseText = futurePilotReadbackJapaneseText;
            FuturePilotReadbackEnglishText = futurePilotReadbackEnglishText;
            FutureAudioKey = futureAudioKey;
        }

        public string CommandId { get; private set; }
        public string ControllerJapaneseText { get; private set; }
        public string ControllerEnglishText { get; private set; }
        public string FuturePilotReadbackJapaneseText { get; private set; }
        public string FuturePilotReadbackEnglishText { get; private set; }
        public string FutureAudioKey { get; private set; }
    }
}
