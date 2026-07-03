using ATCJourneyJapan.Aircraft;
using UnityEngine;

namespace ATCJourneyJapan.Core
{
    // Applies validated player commands to the currently selected aircraft.
    public class CommandSystem : MonoBehaviour
    {
        private GameManager gameManager;

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();
        }

        public void Execute(AircraftCommand command)
        {
            if (gameManager != null && !gameManager.IsTrainingStarted)
            {
                return;
            }

            var selected = SelectionManager.Instance != null ? SelectionManager.Instance.SelectedAircraft : null;
            if (selected == null || !selected.CanExecute(command))
            {
                return;
            }

            selected.Execute(command);
            gameManager?.NotifyCommandExecuted(selected, command);
        }
    }
}
