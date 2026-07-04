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
        private readonly List<RunwayData> runwayData = new List<RunwayData>();
        private readonly List<Vector3> gatePositions = new List<Vector3>();
        private Material runwayMaterial;
        private Material futureRunwayMaterial;
        private Material runwayEdgeMaterial;
        private Material runwayMarkingMaterial;
        private Material taxiwayMaterial;
        private Material taxiwayMarkingMaterial;
        private Material gateMaterial;
        private Material apronMaterial;
        private Material secondaryApronMaterial;
        private Material spotMarkingMaterial;
        private Material groundMaterial;
        private Material seaMaterial;
        private Material shorelineMaterial;
        private Material terminalMaterial;
        private Material towerMaterial;
        private Material holdShortMaterial;

        public IReadOnlyList<RunwayController> Runways => runways;
        public IReadOnlyList<RunwayData> RunwayData => runwayData;
        public RunwayData PrimaryRunwayData => runwayData.Count > 0 ? runwayData[0] : null;
        public Vector3 ArrivalSpawnPosition => new Vector3(-26f, 0.6f, 0f);
        public Vector3 DepartureSpawnPosition => gatePositions.Count > 1 ? gatePositions[1] : new Vector3(-10f, 0.6f, -9f);
        public Vector3 SecondaryArrivalSpawnPosition => new Vector3(-28f, 0.6f, 3.2f);
        public Vector3 SecondaryDepartureSpawnPosition => gatePositions.Count > 3 ? gatePositions[3] : new Vector3(-16f, 0.6f, -11.6f);
        public Vector3 PushbackReadyPosition => new Vector3(-11f, 0.6f, -6.5f);
        public Vector3 HoldShortPosition => new Vector3(-7f, 0.55f, -3f);

        public void Initialize()
        {
            CreateMaterials();
            CreateRunwayData();
            CreateEnvironment();
        }

        public RunwayData GetRunwayData(string runwayId)
        {
            foreach (var runway in runwayData)
            {
                if (runway.RunwayId == runwayId)
                {
                    return runway;
                }
            }

            return null;
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

        public IEnumerable<Vector3> GetPushbackRoute()
        {
            return new[]
            {
                new Vector3(-11f, 0.6f, -8f),
                PushbackReadyPosition
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
            runwayMaterial = CreateMaterial("Runway Asphalt", new Color(0.13f, 0.14f, 0.145f));
            futureRunwayMaterial = CreateMaterial("Future Runway Asphalt", new Color(0.31f, 0.34f, 0.35f));
            runwayEdgeMaterial = CreateMaterial("Runway Edge Paint", new Color(0.9f, 0.92f, 0.86f));
            runwayMarkingMaterial = CreateMaterial("Runway Marking Paint", new Color(0.98f, 0.96f, 0.86f));
            taxiwayMaterial = CreateMaterial("Taxiway Asphalt", new Color(0.22f, 0.29f, 0.32f));
            taxiwayMarkingMaterial = CreateMaterial("Taxiway Centerline Paint", new Color(1f, 0.76f, 0.22f));
            gateMaterial = CreateMaterial("Spot Apron Concrete", new Color(0.22f, 0.44f, 0.42f));
            apronMaterial = CreateMaterial("Terminal Apron Concrete", new Color(0.29f, 0.37f, 0.36f));
            secondaryApronMaterial = CreateMaterial("Secondary Apron Concrete", new Color(0.25f, 0.34f, 0.34f));
            spotMarkingMaterial = CreateMaterial("Spot Marking Paint", new Color(0.95f, 0.96f, 0.84f));
            groundMaterial = CreateMaterial("Ground Green", new Color(0.24f, 0.42f, 0.29f));
            seaMaterial = CreateMaterial("Naha Sea Blockout", new Color(0.03f, 0.48f, 0.72f));
            shorelineMaterial = CreateMaterial("Shoreline Blockout", new Color(0.52f, 0.82f, 0.84f));
            terminalMaterial = CreateMaterial("Terminal Blockout", new Color(0.68f, 0.72f, 0.7f));
            towerMaterial = CreateMaterial("Tower Blockout", new Color(0.86f, 0.88f, 0.82f));
            holdShortMaterial = CreateMaterial("Hold Yellow", new Color(0.85f, 0.68f, 0.18f));
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

        private void CreateRunwayData()
        {
            runwayData.Clear();
            runwayData.Add(new RunwayData("RWY_A", "A滑走路", "Runway A", "18L", "36R", "18L", true, "ArrivalAndDeparture"));
            runwayData.Add(new RunwayData("RWY_B", "B滑走路", "Runway B", "18R", "36L", "18R", false, "FutureExpansion"));
        }

        private void CreateEnvironment()
        {
            var root = new GameObject($"{airportDisplayName} Layout");
            root.transform.SetParent(transform);

            CreateBox("Airport Island Ground", new Vector3(0f, -0.1f, -0.6f), new Vector3(66f, 0.1f, 34f), groundMaterial, root.transform);
            CreateNahaStyleBlockout(root.transform);
            CreateRunway(root.transform);
            CreateTaxiways(root.transform);

            gatePositions.Add(new Vector3(7.5f, 0.6f, -9.2f));
            gatePositions.Add(new Vector3(-5.5f, 0.6f, -9.2f));
            gatePositions.Add(new Vector3(13.5f, 0.6f, -11.8f));
            gatePositions.Add(new Vector3(-14.5f, 0.6f, -11.8f));
            CreateSpotStand("Gate 1 Stand", "SPOT 01", gatePositions[0], root.transform);
            CreateSpotStand("Gate 2 Stand", "SPOT 02", gatePositions[1], root.transform);
            CreateSpotStand("Gate 3 Stand", "SPOT 03", gatePositions[2], root.transform);
            CreateSpotStand("Gate 4 Stand", "SPOT 04", gatePositions[3], root.transform);
            CreateTerminalBlockout(root.transform);
            CreateBox("Hold Short A", HoldShortPosition + Vector3.down * 0.5f, new Vector3(2.4f, 0.16f, 1.6f), holdShortMaterial, root.transform);
            CreateBox("Hold Short A Stop Bar", HoldShortPosition + new Vector3(0f, -0.39f, 0.62f), new Vector3(2.35f, 0.045f, 0.08f), runwayMarkingMaterial, root.transform);

            var runwayObject = new GameObject("RunwayController A");
            runwayObject.transform.SetParent(transform);
            var runway = runwayObject.AddComponent<RunwayController>();
            runway.Configure("A", CreateMarker("Runway A Threshold", new Vector3(-12f, 0f, 0f), runwayObject.transform), CreateMarker("Runway A End", new Vector3(18f, 0f, 0f), runwayObject.transform));
            runways.Add(runway);
        }

        private void CreateNahaStyleBlockout(Transform parent)
        {
            CreateBox("East China Sea Between Parallel Runways", new Vector3(3f, -0.035f, 5.2f), new Vector3(66f, 0.04f, 4f), seaMaterial, parent);
            CreateBox("East China Sea Offshore", new Vector3(3f, -0.035f, 17.2f), new Vector3(72f, 0.04f, 11.5f), seaMaterial, parent);
            CreateBox("Shoreline Terminal Side", new Vector3(3f, 0.005f, 3.12f), new Vector3(66f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Future Runway Side", new Vector3(3f, 0.005f, 7.28f), new Vector3(66f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Offshore", new Vector3(3f, 0.005f, 11.72f), new Vector3(72f, 0.025f, 0.12f), shorelineMaterial, parent);

            CreateBox("Future Runway B 18R 36L", new Vector3(3f, -0.02f, 10.2f), new Vector3(29.2f, 0.12f, 2.8f), futureRunwayMaterial, parent);
            CreateBox("Future Runway B Centerline", new Vector3(3f, 0.065f, 10.2f), new Vector3(24.2f, 0.035f, 0.06f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge North", new Vector3(3f, 0.06f, 11.45f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge South", new Vector3(3f, 0.06f, 8.95f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);

            CreateBox("Taxiway E West Link", new Vector3(-7f, 0.025f, 5.1f), new Vector3(1.05f, 0.13f, 10.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway E East Link", new Vector3(12f, 0.025f, 5.1f), new Vector3(1.05f, 0.13f, 10.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway E West Centerline", new Vector3(-7f, 0.12f, 5.1f), new Vector3(0.055f, 0.035f, 9.4f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway E East Centerline", new Vector3(12f, 0.12f, 5.1f), new Vector3(0.055f, 0.035f, 9.4f), taxiwayMarkingMaterial, parent);
        }

        private void CreateTerminalBlockout(Transform parent)
        {
            CreateBox("WEST APRON Blockout", new Vector3(-13f, -0.02f, -9.8f), new Vector3(12f, 0.12f, 5.8f), secondaryApronMaterial, parent);
            CreateBox("NR APRON Blockout", new Vector3(4f, -0.015f, -9.6f), new Vector3(19f, 0.12f, 6.4f), apronMaterial, parent);
            CreateBox("INTL APRON Blockout", new Vector3(15f, -0.01f, -10.9f), new Vector3(10f, 0.12f, 6.2f), secondaryApronMaterial, parent);
            CreateBox("DOM TERMINAL Blockout", new Vector3(4f, 0.85f, -15.2f), new Vector3(18f, 1.7f, 2.3f), terminalMaterial, parent);
            CreateBox("INTL TERMINAL Blockout", new Vector3(17f, 0.75f, -15.3f), new Vector3(9f, 1.5f, 2.2f), terminalMaterial, parent);

            CreateBox("TWR Shaft Blockout", new Vector3(8f, 1.65f, -7.1f), new Vector3(0.85f, 3.3f, 0.85f), towerMaterial, parent);
            CreateBox("TWR Cab Blockout", new Vector3(8f, 3.55f, -7.1f), new Vector3(2f, 0.85f, 2f), towerMaterial, parent);
        }

        private void CreateRunway(Transform parent)
        {
            CreateBox("Runway A", new Vector3(3f, 0f, 0f), new Vector3(32.5f, 0.2f, 2.35f), runwayMaterial, parent);
            CreateBox("Runway A Edge North", new Vector3(3f, 0.13f, 1.03f), new Vector3(31.9f, 0.04f, 0.08f), runwayEdgeMaterial, parent);
            CreateBox("Runway A Edge South", new Vector3(3f, 0.13f, -1.03f), new Vector3(31.9f, 0.04f, 0.08f), runwayEdgeMaterial, parent);
            CreateBox("Runway A Threshold 18L", new Vector3(-12.8f, 0.14f, 0f), new Vector3(0.28f, 0.05f, 1.72f), runwayMarkingMaterial, parent);
            CreateBox("Runway A End Marking", new Vector3(18.8f, 0.14f, 0f), new Vector3(0.28f, 0.05f, 1.72f), runwayMarkingMaterial, parent);

            for (var index = 0; index < 9; index++)
            {
                var x = -9.4f + index * 3.1f;
                CreateBox($"Runway A Centerline {index + 1}", new Vector3(x, 0.15f, 0f), new Vector3(1.15f, 0.045f, 0.08f), runwayMarkingMaterial, parent);
            }
        }

        private void CreateTaxiways(Transform parent)
        {
            CreateBox("Taxiway Main", new Vector3(0f, 0.02f, -5f), new Vector3(26f, 0.14f, 1.24f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link West", new Vector3(-7f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("Taxiway Runway Link East", new Vector3(12f, 0.03f, -2.5f), new Vector3(1.12f, 0.14f, 5f), taxiwayMaterial, parent);
            CreateBox("Taxiway W Apron Spur", new Vector3(-11f, 0.025f, -7.1f), new Vector3(1f, 0.13f, 4.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway T Terminal Spur", new Vector3(5.5f, 0.025f, -7.1f), new Vector3(1f, 0.13f, 4.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway D Intl Spur", new Vector3(15f, 0.025f, -7.2f), new Vector3(1f, 0.13f, 4.4f), taxiwayMaterial, parent);
            CreateBox("Taxiway C Diagonal Connector", new Vector3(2.5f, 0.028f, -2.5f), new Vector3(1.05f, 0.13f, 6f), taxiwayMaterial, Quaternion.Euler(0f, 28f, 0f), parent);
            CreateBox("Taxiway Main Centerline", new Vector3(0f, 0.12f, -5f), new Vector3(25.2f, 0.035f, 0.055f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway West Centerline", new Vector3(-7f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway East Centerline", new Vector3(12f, 0.13f, -2.5f), new Vector3(0.055f, 0.035f, 4.7f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway W Spur Centerline", new Vector3(-11f, 0.12f, -7.1f), new Vector3(0.055f, 0.035f, 3.8f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway T Spur Centerline", new Vector3(5.5f, 0.12f, -7.1f), new Vector3(0.055f, 0.035f, 3.8f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway D Spur Centerline", new Vector3(15f, 0.12f, -7.2f), new Vector3(0.055f, 0.035f, 4f), taxiwayMarkingMaterial, parent);
            CreateBox("Taxiway C Diagonal Centerline", new Vector3(2.5f, 0.12f, -2.5f), new Vector3(0.055f, 0.035f, 5.6f), taxiwayMarkingMaterial, Quaternion.Euler(0f, 28f, 0f), parent);
        }

        private void CreateSpotStand(string standName, string spotLabel, Vector3 spotPosition, Transform parent)
        {
            var basePosition = spotPosition + Vector3.down * 0.5f;
            CreateBox(standName, basePosition, new Vector3(3.2f, 0.16f, 2.55f), gateMaterial, parent);
            CreateBox($"{spotLabel} Front Line", basePosition + new Vector3(0f, 0.12f, 1.02f), new Vector3(2.75f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Rear Line", basePosition + new Vector3(0f, 0.12f, -1.02f), new Vector3(2.75f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Left Line", basePosition + new Vector3(-1.36f, 0.12f, 0f), new Vector3(0.07f, 0.04f, 2.1f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Right Line", basePosition + new Vector3(1.36f, 0.12f, 0f), new Vector3(0.07f, 0.04f, 2.1f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Stop Bar", basePosition + new Vector3(0f, 0.13f, 0.56f), new Vector3(1.25f, 0.045f, 0.08f), runwayMarkingMaterial, parent);
        }

        private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            return CreateBox(objectName, position, scale, material, Quaternion.identity, parent);
        }

        private GameObject CreateBox(string objectName, Vector3 position, Vector3 scale, Material material, Quaternion rotation, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.rotation = rotation;
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
