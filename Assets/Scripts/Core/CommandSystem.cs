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
            Execute(command, string.Empty);
        }

        public void Execute(AircraftCommand command, string operationDirection)
        {
            var selected = SelectionManager.Instance != null ? SelectionManager.Instance.SelectedAircraft : null;
            if (gameManager != null && !gameManager.IsTrainingStarted)
            {
                return;
            }

            if (selected == null || !selected.CanExecute(command))
            {
                return;
            }

            if (gameManager != null && !gameManager.CanExecuteRunwaySafetyCommand(selected, command))
            {
                gameManager.RejectUnsafeRunwayCommand(selected, command);
                return;
            }

            if (gameManager != null && !gameManager.CanAcceptCommand(selected, command))
            {
                return;
            }

            gameManager?.BindRunwayDirectionForCommand(selected, command, operationDirection);
            selected.Execute(command);
            gameManager?.NotifyCommandExecuted(selected, command);
        }
    }
}
