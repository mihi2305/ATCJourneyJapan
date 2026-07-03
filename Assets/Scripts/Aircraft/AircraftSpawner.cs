using ATCJourneyJapan.Airport;
using ATCJourneyJapan.Core;
using UnityEngine;

namespace ATCJourneyJapan.Aircraft
{
    // Creates the initial playable traffic sample. Later stages can replace this with schedule-driven spawning.
    public class AircraftSpawner : MonoBehaviour
    {
        [SerializeField] private Material arrivalMaterial;
        [SerializeField] private Material departureMaterial;
        [SerializeField] private Material selectedMaterial;

        private AirportManager airportManager;
        private GameManager gameManager;

        public void Initialize(AirportManager airport, GameManager manager)
        {
            airportManager = airport;
            gameManager = manager;
            CreateMaterials();
        }

        public void SpawnInitialAircraft()
        {
            var runway = airportManager.PrimaryRunwayData;
            var arrivalData = new AircraftData("AJJ101", "B737", "Arrival", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_01", "SPOT 01", "宮崎", "那覇", scheduledArrivalTime: "08:05", estimatedArrivalTime: "08:05");
            var departureData = new AircraftData("AJJ202", "A320", "Departure", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_02", "SPOT 02", "那覇", "東京", scheduledDepartureTime: "08:10", estimatedDepartureTime: "08:10");

            SpawnAircraft(arrivalData, true, AircraftState.Inbound, airportManager.ArrivalSpawnPosition, PrimitiveType.Capsule, arrivalMaterial);
            SpawnAircraft(departureData, false, AircraftState.AtGate, airportManager.DepartureSpawnPosition, PrimitiveType.Capsule, departureMaterial);
        }

        private void CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            arrivalMaterial = new Material(shader) { name = "Arrival Aircraft Blue", color = new Color(0.15f, 0.42f, 0.8f) };
            departureMaterial = new Material(shader) { name = "Departure Aircraft White", color = new Color(0.86f, 0.88f, 0.84f) };
            selectedMaterial = new Material(shader) { name = "Selected Aircraft Orange", color = new Color(1f, 0.55f, 0.18f) };
        }

        private AircraftController SpawnAircraft(AircraftData flightData, bool arrival, AircraftState state, Vector3 position, PrimitiveType primitive, Material material)
        {
            var aircraftObject = GameObject.CreatePrimitive(primitive);
            aircraftObject.name = $"{flightData.FlightId} Aircraft";
            aircraftObject.transform.position = position;
            aircraftObject.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);

            var controller = aircraftObject.AddComponent<AircraftController>();
            controller.Configure(flightData, arrival, state, airportManager, gameManager, material, selectedMaterial);
            gameManager.RegisterAircraft(controller);
            return controller;
        }
    }
}
