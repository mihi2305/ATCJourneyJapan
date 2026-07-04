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
            var arrivalData = new AircraftData("AJJ101", "B737", "Arrival", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_01", "SPOT 01", "Miyazaki", "Naha", scheduledArrivalTime: "08:05", estimatedArrivalTime: "08:05");
            var secondArrivalData = new AircraftData("AJJ103", "B737", "Arrival", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_03", "SPOT 03", "Fukuoka", "Naha", scheduledArrivalTime: "08:12", estimatedArrivalTime: "08:12");
            var departureData = new AircraftData("AJJ202", "A320", "Departure", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_02", "SPOT 02", "Naha", "Tokyo", scheduledDepartureTime: "08:10", estimatedDepartureTime: "08:10");
            var secondDepartureData = new AircraftData("AJJ204", "A320", "Departure", runway.RunwayId, runway.SimpleNameJa, runway.CurrentActiveDesignator, "SPOT_04", "SPOT 04", "Naha", "Osaka", scheduledDepartureTime: "08:18", estimatedDepartureTime: "08:18");

            SpawnAircraft(arrivalData, true, AircraftState.Inbound, airportManager.ArrivalSpawnPosition, arrivalMaterial);
            SpawnAircraft(secondArrivalData, true, AircraftState.Inbound, airportManager.SecondaryArrivalSpawnPosition, arrivalMaterial);
            SpawnAircraft(departureData, false, AircraftState.AtGate, airportManager.DepartureSpawnPosition, departureMaterial);
            SpawnAircraft(secondDepartureData, false, AircraftState.HoldingShort, airportManager.HoldShortPosition + new Vector3(-2.2f, 0f, -0.8f), departureMaterial);
        }

        private void CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            arrivalMaterial = new Material(shader) { name = "Arrival Aircraft Blue", color = new Color(0.15f, 0.42f, 0.8f) };
            departureMaterial = new Material(shader) { name = "Departure Aircraft White", color = new Color(0.86f, 0.88f, 0.84f) };
            selectedMaterial = new Material(shader) { name = "Selected Aircraft Orange", color = new Color(1f, 0.55f, 0.18f) };
        }

        private AircraftController SpawnAircraft(AircraftData flightData, bool arrival, AircraftState state, Vector3 position, Material material)
        {
            var visualSpec = AircraftVisualSpec.Resolve(flightData.AircraftType);
            var aircraftObject = new GameObject($"{flightData.FlightId} Aircraft");
            aircraftObject.name = $"{flightData.FlightId} Aircraft";
            aircraftObject.transform.position = position;

            var collider = aircraftObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0f, 0.05f);
            collider.size = new Vector3(visualSpec.ClickBoxSize, 0.75f, visualSpec.ClickBoxSize);
            CreateAircraftVisual(aircraftObject.transform, material, visualSpec);

            var controller = aircraftObject.AddComponent<AircraftController>();
            controller.Configure(flightData, arrival, state, airportManager, gameManager, material, selectedMaterial, visualSpec);
            gameManager.RegisterAircraft(controller);
            return controller;
        }

        private void CreateAircraftVisual(Transform parent, Material material, AircraftVisualSpec visualSpec)
        {
            var visualRoot = new GameObject("Simple 3D Aircraft Visual");
            visualRoot.transform.SetParent(parent, false);

            var length = visualSpec.VisualLength;
            var wingspan = visualSpec.VisualWingspan;
            var height = visualSpec.VisualHeight;
            CreateVisualPart("Fuselage", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, 0f, 0.03f), new Vector3(wingspan * 0.2f, height, length * 0.82f), material);
            CreateVisualPart("Nose", visualRoot.transform, PrimitiveType.Sphere, new Vector3(0f, 0f, length * 0.48f), new Vector3(wingspan * 0.22f, height, wingspan * 0.22f), material);
            CreateVisualPart("Main Wing", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, -0.01f, length * 0.03f), new Vector3(wingspan, 0.07f, length * 0.18f), material);
            CreateVisualPart("Tail Wing", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, 0.02f, -length * 0.37f), new Vector3(wingspan * 0.45f, 0.06f, length * 0.14f), material);
            CreateVisualPart("Vertical Tail", visualRoot.transform, PrimitiveType.Cube, new Vector3(0f, height * 0.72f, -length * 0.4f), new Vector3(wingspan * 0.08f, height * 1.15f, length * 0.15f), material);
        }

        private void CreateVisualPart(string partName, Transform parent, PrimitiveType primitive, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;

            var partCollider = part.GetComponent<Collider>();
            if (partCollider != null)
            {
                Destroy(partCollider);
            }
        }
    }
}
