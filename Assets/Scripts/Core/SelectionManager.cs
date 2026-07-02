using ATCJourneyJapan.Aircraft;
using UnityEngine;

namespace ATCJourneyJapan.Core
{
    // Central selection state for aircraft and UI command targeting.
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        public AircraftController SelectedAircraft { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SelectAircraft(AircraftController aircraft)
        {
            if (SelectedAircraft == aircraft)
            {
                return;
            }

            if (SelectedAircraft != null)
            {
                SelectedAircraft.SetSelected(false);
            }

            SelectedAircraft = aircraft;

            if (SelectedAircraft != null)
            {
                SelectedAircraft.SetSelected(true);
            }
        }
    }
}
