namespace ATCJourneyJapan.Radio
{
    // Phrase data for one command. English fields are reserved for future voice playback.
    public class CommandPhrase
    {
        public CommandPhrase(
            string commandId,
            string japaneseControllerText,
            string englishControllerPhrase,
            string japanesePilotReadbackText,
            string englishPilotReadbackPhrase,
            string futureAudioKey)
        {
            CommandId = commandId;
            JapaneseControllerText = japaneseControllerText;
            EnglishControllerPhrase = englishControllerPhrase;
            JapanesePilotReadbackText = japanesePilotReadbackText;
            EnglishPilotReadbackPhrase = englishPilotReadbackPhrase;
            FutureAudioKey = futureAudioKey;
        }

        public string CommandId { get; private set; }
        public string JapaneseControllerText { get; private set; }
        public string EnglishControllerPhrase { get; private set; }
        public string JapanesePilotReadbackText { get; private set; }
        public string EnglishPilotReadbackPhrase { get; private set; }
        public string FutureAudioKey { get; private set; }

        public string ControllerJapaneseText => JapaneseControllerText;
        public string ControllerEnglishText => EnglishControllerPhrase;
        public string FuturePilotReadbackJapaneseText => JapanesePilotReadbackText;
        public string FuturePilotReadbackEnglishText => EnglishPilotReadbackPhrase;
    }
}
