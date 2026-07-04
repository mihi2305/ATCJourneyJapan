using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Aircraft
{
    public class AircraftVisualSpec
    {
        private static readonly Dictionary<string, AircraftVisualSpec> Specs = new Dictionary<string, AircraftVisualSpec>
        {
            { "B737", new AircraftVisualSpec("B737-800", 39.5f, 35.8f, 12.6f, 1.25f, 1.35f, 0.34f, 1.65f, 1.32f) },
            { "B737-800", new AircraftVisualSpec("B737-800", 39.5f, 35.8f, 12.6f, 1.25f, 1.35f, 0.34f, 1.65f, 1.32f) },
            { "A320", new AircraftVisualSpec("A320/B737 class", 37.6f, 35.8f, 11.8f, 1.22f, 1.32f, 0.33f, 1.62f, 1.3f) },
            { "B787", new AircraftVisualSpec("B787-8", 56.7f, 60.1f, 16.9f, 1.75f, 2.05f, 0.44f, 2.3f, 1.55f) },
            { "B787-8", new AircraftVisualSpec("B787-8", 56.7f, 60.1f, 16.9f, 1.75f, 2.05f, 0.44f, 2.3f, 1.55f) },
            { "B777", new AircraftVisualSpec("B777-300ER", 73.9f, 64.8f, 18.6f, 2.08f, 2.22f, 0.49f, 2.65f, 1.68f) },
            { "B777-300ER", new AircraftVisualSpec("B777-300ER", 73.9f, 64.8f, 18.6f, 2.08f, 2.22f, 0.49f, 2.65f, 1.68f) }
        };

        private AircraftVisualSpec(
            string displayName,
            float realLengthMeters,
            float realWingspanMeters,
            float realHeightMeters,
            float visualLength,
            float visualWingspan,
            float visualHeight,
            float clickBoxSize,
            float labelHeight)
        {
            DisplayName = displayName;
            RealLengthMeters = realLengthMeters;
            RealWingspanMeters = realWingspanMeters;
            RealHeightMeters = realHeightMeters;
            VisualLength = visualLength;
            VisualWingspan = visualWingspan;
            VisualHeight = visualHeight;
            ClickBoxSize = clickBoxSize;
            LabelHeight = labelHeight;
        }

        public string DisplayName { get; private set; }
        public float RealLengthMeters { get; private set; }
        public float RealWingspanMeters { get; private set; }
        public float RealHeightMeters { get; private set; }
        public float VisualLength { get; private set; }
        public float VisualWingspan { get; private set; }
        public float VisualHeight { get; private set; }
        public float ClickBoxSize { get; private set; }
        public float LabelHeight { get; private set; }

        public static AircraftVisualSpec Resolve(string aircraftType)
        {
            if (!string.IsNullOrEmpty(aircraftType) && Specs.TryGetValue(aircraftType.ToUpperInvariant(), out var spec))
            {
                return spec;
            }

            return Specs["B737"];
        }
    }
}
