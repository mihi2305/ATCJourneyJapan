using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Airport
{
    // Builds and owns prototype airport data. Later this can read AirportData/StageData without changing aircraft commands.
    public class AirportManager : MonoBehaviour
    {
        [SerializeField] private string airportId = "PrototypeIsland";
        [SerializeField] private string airportDisplayName = "Prototype Island Airport";

        private readonly List<RunwayController> runways = new List<RunwayController>();
        private readonly List<Vector3> gatePositions = new List<Vector3>();
        private Material runwayMaterial;
        private Material taxiwayMaterial;
        private Material gateMaterial;
        private Material groundMaterial;

        public IReadOnlyList<RunwayController> Runways => runways;
        public Vector3 ArrivalSpawnPosition => new Vector3(-26f, 0.6f, 0f);
        public Vector3 DepartureSpawnPosition => gatePositions.Count > 1 ? gatePositions[1] : new Vector3(-10f, 0.6f, -9f);
        public Vector3 HoldShortPosition => new Vector3(-7f, 0.55f, -3f);

        public void Initialize()
        {
            CreateMaterials();
            CreateEnvironment();
        }

        public IEnumerable<Vector3> GetArrivalFinalRoute()
        {
            return new[]
            {
                new Vector3(-18f, 0.6f, 0f),
                new Vector3(-12f, 0.6f, 0f),
                new Vector3(-6f, 0.6f, 0f)
            };
        }

        public IEnumerable<Vector3> GetLandingRollRoute()
        {
            return new[]
            {
                new Vector3(3f, 0.6f, 0f),
                new Vector3(11.5f, 0.6f, 0f)
            };
        }

        public IEnumerable<Vector3> GetVacateRunwayRoute()
        {
            return new[]
            {
                new Vector3(12f, 0.6f, -2.5f),
                new Vector3(12f, 0.6f, -5f)
            };
        }

        public IEnumerable<Vector3> GetTaxiToAvailableGateRoute()
        {
            return new[]
            {
                new Vector3(7f, 0.6f, -5f),
                new Vector3(2f, 0.6f, -5f),
                gatePositions[0]
            };
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute()
        {
            return new[]
            {
                new Vector3(-11f, 0.6f, -5f),
                new Vector3(-7f, 0.6f, -5f),
                HoldShortPosition
            };
        }

        public IEnumerable<Vector3> GetLineUpRoute()
        {
            return new[]
            {
                new Vector3(-7f, 0.6f, 0f),
                new Vector3(-3f, 0.6f, 0f)
            };
        }

        public IEnumerable<Vector3> GetTakeoffRoute()
        {
            return new[]
            {
                new Vector3(5f, 0.6f, 0f),
                new Vector3(15f, 0.6f, 0f),
                new Vector3(26f, 2.4f, 0f)
            };
        }

        private void CreateMaterials()
        {
            runwayMaterial = CreateMaterial("Runway Dark", new Color(0.12f, 0.13f, 0.14f));
            taxiwayMaterial = CreateMaterial("Taxiway Blue Gray", new Color(0.22f, 0.28f, 0.32f));
            gateMaterial = CreateMaterial("Gate Teal", new Color(0.08f, 0.38f, 0.36f));
            groundMaterial = CreateMaterial("Ground Green", new Color(0.18f, 0.32f, 0.22f));
        }

        private Material CreateMaterial(string materialName, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = materialName,
                color = color
            };
            return material;
        }

        private void CreateEnvironment()
        {
            var root = new GameObject($"{airportDisplayName} Layout");
            root.transform.SetParent(transform);

            CreateBox("Ground", new Vector3(0f, -0.05f, -1f), new Vector3(42f, 0.1f, 26f), groundMaterial, root.transform);
            CreateBox("Runway A", new Vector3(3f, 0f, 0f), new Vector3(30f, 0.18f, 2.2f), runwayMaterial, root.transform);
            CreateBox("Taxiway Main", new Vector3(0f, 0.02f, -5f), new Vector3(26f, 0.12f, 1.2f), taxiwayMaterial, root.transform);
            CreateBox("Taxiway Runway Link West", new Vector3(-7f, 0.03f, -2.5f), new Vector3(1.1f, 0.12f, 5f), taxiwayMaterial, root.transform);
            CreateBox("Taxiway Runway Link East", new Vector3(12f, 0.03f, -2.5f), new Vector3(1.1f, 0.12f, 5f), taxiwayMaterial, root.transform);

            gatePositions.Add(new Vector3(2f, 0.6f, -9f));
            gatePositions.Add(new Vector3(-11f, 0.6f, -9f));
            CreateBox("Gate 1 Stand", gatePositions[0] + Vector3.down * 0.55f, new Vector3(3f, 0.12f, 2.5f), gateMaterial, root.transform);
            CreateBox("Gate 2 Stand", gatePositions[1] + Vector3.down * 0.55f, new Vector3(3f, 0.12f, 2.5f), gateMaterial, root.transform);
            CreateBox("Hold Short A", HoldShortPosition + Vector3.down * 0.52f, new Vector3(2.4f, 0.12f, 1.6f), CreateMaterial("Hold Yellow", new Color(0.85f, 0.68f, 0.18f)), root.transform);

            var runwayObject = new GameObject("RunwayController A");
            runwayObject.transform.SetParent(transform);
            var runway = runwayObject.AddComponent<RunwayController>();
            runway.Configure("A", CreateMarker("Runway A Threshold", new Vector3(-12f, 0f, 0f), runwayObject.transform), CreateMarker("Runway A End", new Vector3(18f, 0f, 0f), runwayObject.transform));
            runways.Add(runway);
        }

        private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = scale;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private Transform CreateMarker(string markerName, Vector3 position, Transform parent)
        {
            var marker = new GameObject(markerName);
            marker.transform.SetParent(parent);
            marker.transform.position = position;
            return marker.transform;
        }
    }
}
