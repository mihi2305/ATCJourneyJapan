using ATCJourneyJapan.Aircraft;
using UnityEngine;

namespace ATCJourneyJapan.Core
{
    // Applies validated player commands to the currently selected aircraft.
    public class CommandSystem : MonoBehaviour
    {
        public void Execute(AircraftCommand command)
        {
            var selected = SelectionManager.Instance != null ? SelectionManager.Instance.SelectedAircraft : null;
            if (selected == null || !selected.CanExecute(command))
            {
                return;
            }

            selected.Execute(command);
        }
    }
}
