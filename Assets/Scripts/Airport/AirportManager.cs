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
        private readonly List<AirportSpotDefinition> spotDefinitions = new List<AirportSpotDefinition>();
        private Material runwayMaterial;
        private Material futureRunwayMaterial;
        private Material runwayEdgeMaterial;
        private Material runwayMarkingMaterial;
        private Material taxiwayMaterial;
        private Material taxiwayMarkingMaterial;
        private Material gateMaterial;
        private Material apronMaterial;
        private Material secondaryApronMaterial;
        private Material militaryApronMaterial;
        private Material passengerApronMaterial;
        private Material hangarMaterial;
        private Material supportBuildingMaterial;
        private Material areaDividerMaterial;
        private Material spotMarkingMaterial;
        private Material groundMaterial;
        private Material seaMaterial;
        private Material shorelineMaterial;
        private Material terminalMaterial;
        private Material boardingBridgeMaterial;
        private Material towerMaterial;
        private Material holdShortMaterial;

        public IReadOnlyList<RunwayController> Runways => runways;
        public IReadOnlyList<RunwayData> RunwayData => runwayData;
        public IReadOnlyList<AirportSpotDefinition> SpotDefinitions => spotDefinitions;
        public RunwayData PrimaryRunwayData => runwayData.Count > 0 ? runwayData[0] : null;
        public Vector3 ArrivalSpawnPosition => new Vector3(-26f, 0.6f, 0f);
        public Vector3 DepartureSpawnPosition => gatePositions.Count > 1 ? gatePositions[1] : new Vector3(-10f, 0.6f, -9f);
        public Vector3 SecondaryArrivalSpawnPosition => new Vector3(-28f, 0.6f, 3.2f);
        public Vector3 SecondaryDepartureSpawnPosition => gatePositions.Count > 3 ? gatePositions[3] : new Vector3(-16f, 0.6f, -11.6f);
        public Vector3 PushbackReadyPosition => new Vector3(13f, 0.6f, -6.5f);
        public Vector3 HoldShortPosition => new Vector3(12f, 0.55f, -3f);
        public Vector3 PrimaryRunwayLineUpPosition => new Vector3(14.5f, 0.6f, 0f);
        public Vector3 PrimaryRunwayTakeoffDirection => GetTakeoffDirection(PrimaryRunwayData);

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

        private Vector3 GetTakeoffDirection(RunwayData runway)
        {
            if (runway != null && runway.CurrentActiveDesignator == "36R")
            {
                return Vector3.left;
            }

            return Vector3.right;
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
                new Vector3(12f, 0.6f, -5f),
                new Vector3(10.5f, 0.6f, -6.8f),
                gatePositions[0]
            };
        }

        public IEnumerable<Vector3> GetPushbackRoute()
        {
            return GetPushbackRoute(DepartureSpawnPosition);
        }

        public IEnumerable<Vector3> GetPushbackRoute(Vector3 startPosition)
        {
            var pushbackLineZ = startPosition.x >= 22f ? -6.2f : -6.45f;
            return new[]
            {
                new Vector3(startPosition.x, 0.6f, pushbackLineZ)
            };
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute()
        {
            return GetTaxiToHoldRoute(PushbackReadyPosition);
        }

        public IEnumerable<Vector3> GetTaxiToHoldRoute(Vector3 startPosition)
        {
            var taxiwayMainEntry = new Vector3(startPosition.x, 0.6f, -5f);
            return new[]
            {
                taxiwayMainEntry,
                new Vector3(12f, 0.6f, -5f),
                HoldShortPosition
            };
        }

        public IEnumerable<Vector3> GetLineUpRoute()
        {
            return GetLineUpRoute(HoldShortPosition);
        }

        public IEnumerable<Vector3> GetLineUpRoute(Vector3 startPosition)
        {
            return new[]
            {
                new Vector3(startPosition.x, 0.6f, -1.05f),
                new Vector3(12f, 0.6f, 0f),
                PrimaryRunwayLineUpPosition
            };
        }

        public IEnumerable<Vector3> GetTakeoffRoute()
        {
            return new[]
            {
                new Vector3(17f, 0.6f, 0f),
                new Vector3(22f, 0.6f, 0f),
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
            militaryApronMaterial = CreateMaterial("Base Apron Concrete", new Color(0.26f, 0.29f, 0.27f));
            passengerApronMaterial = CreateMaterial("Passenger Apron Concrete", new Color(0.35f, 0.39f, 0.38f));
            hangarMaterial = CreateMaterial("Hangar Blockout", new Color(0.38f, 0.42f, 0.4f));
            supportBuildingMaterial = CreateMaterial("Support Building Blockout", new Color(0.48f, 0.52f, 0.47f));
            areaDividerMaterial = CreateMaterial("Area Divider Paint", new Color(0.76f, 0.79f, 0.68f));
            spotMarkingMaterial = CreateMaterial("Spot Marking Paint", new Color(0.95f, 0.96f, 0.84f));
            groundMaterial = CreateMaterial("Ground Green", new Color(0.24f, 0.42f, 0.29f));
            seaMaterial = CreateMaterial("Naha Sea Blockout", new Color(0.03f, 0.48f, 0.72f));
            shorelineMaterial = CreateMaterial("Shoreline Blockout", new Color(0.52f, 0.82f, 0.84f));
            terminalMaterial = CreateMaterial("Terminal Blockout", new Color(0.68f, 0.72f, 0.7f));
            boardingBridgeMaterial = CreateMaterial("Boarding Bridge Blockout", new Color(0.82f, 0.84f, 0.8f));
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

            CreateBox("Main Airport Island Ground", new Vector3(3f, -0.1f, -8.2f), new Vector3(68f, 0.1f, 18.8f), groundMaterial, root.transform);
            CreateNahaStyleBlockout(root.transform);
            CreateRunway(root.transform);
            CreateTaxiways(root.transform);

            CreateSpotDefinitions();
            CreateSpotStands(root.transform);
            CreateTerminalBlockout(root.transform);
            CreateBox("Hold Short A", HoldShortPosition + Vector3.down * 0.5f, new Vector3(2.4f, 0.16f, 1.6f), holdShortMaterial, root.transform);
            CreateBox("Hold Short A Stop Bar", HoldShortPosition + new Vector3(0f, -0.39f, 0.62f), new Vector3(2.35f, 0.045f, 0.08f), runwayMarkingMaterial, root.transform);

            var runwayObject = new GameObject("RunwayController A");
            runwayObject.transform.SetParent(transform);
            var runway = runwayObject.AddComponent<RunwayController>();
            runway.Configure("A", CreateMarker("Runway A Threshold", new Vector3(-12f, 0f, 0f), runwayObject.transform), CreateMarker("Runway A End", new Vector3(18f, 0f, 0f), runwayObject.transform));
            runways.Add(runway);
        }

        private void CreateSpotDefinitions()
        {
            spotDefinitions.Clear();
            gatePositions.Clear();

            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 01",
                "DOM 21-25 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(6.3f, 0.6f, -8.35f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 01"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 02",
                "DOM 23-27 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(10.8f, 0.6f, -7.95f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 02"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 03",
                "DOM 31-37 candidate",
                AirportSpotArea.DOM,
                AircraftSizeClass.Narrowbody,
                true,
                new Vector3(17.1f, 0.6f, -8.15f),
                new Vector3(2.35f, 0.16f, 3.45f),
                "Boarding Bridge SPOT 03"));
            spotDefinitions.Add(new AirportSpotDefinition(
                "SPOT 04",
                "INTL 41-46/51 candidate",
                AirportSpotArea.INTL,
                AircraftSizeClass.Widebody,
                true,
                new Vector3(26.2f, 0.6f, -8.65f),
                new Vector3(3.7f, 0.16f, 4.85f),
                "Boarding Bridge SPOT 04"));

            foreach (var spotDefinition in spotDefinitions)
            {
                gatePositions.Add(spotDefinition.Position);
            }
        }

        private void CreateSpotStands(Transform parent)
        {
            for (var index = 0; index < spotDefinitions.Count; index++)
            {
                var spotDefinition = spotDefinitions[index];
                CreateSpotStand(
                    $"Gate {index + 1} Stand",
                    spotDefinition.TutorialId,
                    spotDefinition.Position,
                    spotDefinition.StandScale,
                    parent);
            }
        }

        private void CreateNahaStyleBlockout(Transform parent)
        {
            CreateBox("East China Sea Inner Water West", new Vector3(-13f, -0.035f, 5.4f), new Vector3(31f, 0.04f, 7.2f), seaMaterial, parent);
            CreateBox("East China Sea Inner Water Center", new Vector3(7f, -0.035f, 5.9f), new Vector3(15f, 0.04f, 4.2f), seaMaterial, parent);
            CreateBox("East China Sea Inner Water East", new Vector3(25f, -0.035f, 5.4f), new Vector3(16f, 0.04f, 7.2f), seaMaterial, parent);
            CreateBox("East China Sea Offshore", new Vector3(3f, -0.035f, 17.2f), new Vector3(76f, 0.04f, 12.8f), seaMaterial, parent);

            CreateFlatPolygon(
                "Central Facility Wedge Shoreline",
                0.006f,
                shorelineMaterial,
                parent,
                new Vector2(-5.5f, 1.28f),
                new Vector2(12.6f, 1.28f),
                new Vector2(14.4f, 3.2f),
                new Vector2(11.5f, 6.9f),
                new Vector2(4.4f, 7.35f),
                new Vector2(-1.7f, 3.6f));
            CreateFlatPolygon(
                "Central Facility Wedge Island",
                0.035f,
                groundMaterial,
                parent,
                new Vector2(-4.6f, 1.5f),
                new Vector2(11.4f, 1.5f),
                new Vector2(13.2f, 3.2f),
                new Vector2(10.5f, 6.25f),
                new Vector2(4.6f, 6.75f),
                new Vector2(-0.8f, 3.35f));
            CreateBox("Right Runway Connector Island", new Vector3(17.1f, -0.03f, 5.1f), new Vector3(4.6f, 0.12f, 10.6f), groundMaterial, parent);
            CreateBox("Future Runway B Island", new Vector3(3f, -0.03f, 10.2f), new Vector3(36.5f, 0.12f, 5.4f), groundMaterial, parent);
            CreateBox("Left Service Causeway", new Vector3(-12.5f, -0.025f, 4.6f), new Vector3(0.65f, 0.08f, 5.4f), shorelineMaterial, parent);

            CreateBox("Shoreline Main Island North", new Vector3(3f, 0.005f, 1.35f), new Vector3(68f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Future Runway Island South", new Vector3(3f, 0.005f, 7.5f), new Vector3(36f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Future Runway Island North", new Vector3(3f, 0.005f, 12.9f), new Vector3(36f, 0.025f, 0.12f), shorelineMaterial, parent);
            CreateBox("Shoreline Offshore", new Vector3(3f, 0.005f, 23.55f), new Vector3(76f, 0.025f, 0.12f), shorelineMaterial, parent);

            CreateBox("Future Runway B 18R 36L", new Vector3(3f, -0.02f, 10.2f), new Vector3(29.2f, 0.12f, 2.8f), futureRunwayMaterial, parent);
            CreateBox("Future Runway B Centerline", new Vector3(3f, 0.065f, 10.2f), new Vector3(24.2f, 0.035f, 0.06f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge North", new Vector3(3f, 0.06f, 11.45f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);
            CreateBox("Future Runway B Edge South", new Vector3(3f, 0.06f, 8.95f), new Vector3(28.1f, 0.035f, 0.055f), runwayEdgeMaterial, parent);

            CreateBox("Taxiway E Right Primary Link", new Vector3(17.1f, 0.025f, 5.1f), new Vector3(1.25f, 0.13f, 10.2f), taxiwayMaterial, parent);
            CreateBox("Taxiway E Right Centerline", new Vector3(17.1f, 0.12f, 5.1f), new Vector3(0.055f, 0.035f, 9.4f), taxiwayMarkingMaterial, parent);
            CreateBox("Left Service Track", new Vector3(-12.5f, 0.01f, 4.6f), new Vector3(0.18f, 0.08f, 4.6f), secondaryApronMaterial, parent);
        }

        private void CreateTerminalBlockout(Transform parent)
        {
            CreateBox("Fighter Alert Base Apron", new Vector3(-13.5f, -0.02f, -9.7f), new Vector3(12.5f, 0.12f, 5.2f), militaryApronMaterial, parent);
            CreateBox("JASDF Support Transport Apron", new Vector3(-1.6f, -0.018f, -10f), new Vector3(11.2f, 0.12f, 5.9f), secondaryApronMaterial, parent);
            CreateBox("Passenger Apron Blockout", new Vector3(17.1f, -0.01f, -10.1f), new Vector3(29.2f, 0.12f, 9.35f), passengerApronMaterial, parent);
            CreateBox("DOM Apron Depth Reserve", new Vector3(12.2f, 0.035f, -8.2f), new Vector3(18.5f, 0.035f, 5.7f), secondaryApronMaterial, parent);
            CreateBox("INTL Widebody Apron Depth Reserve", new Vector3(26.2f, 0.04f, -8.5f), new Vector3(7.6f, 0.04f, 6.95f), secondaryApronMaterial, parent);
            CreateBox("DOM Finger Pushback Line", new Vector3(13.6f, 0.072f, -6.45f), new Vector3(11.6f, 0.035f, 0.08f), taxiwayMarkingMaterial, parent);
            CreateBox("INTL Pushback Line", new Vector3(25.8f, 0.074f, -6.2f), new Vector3(6.9f, 0.035f, 0.08f), taxiwayMarkingMaterial, parent);
            CreateBox("Base Passenger Area Divider", new Vector3(2.7f, 0.04f, -10.2f), new Vector3(0.16f, 0.08f, 9.4f), areaDividerMaterial, parent);

            CreateBox("Fighter Shelter 01", new Vector3(-17.5f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Fighter Shelter 02", new Vector3(-14.6f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Fighter Shelter 03", new Vector3(-11.7f, 0.55f, -13.3f), new Vector3(2.2f, 1.1f, 2f), hangarMaterial, parent);
            CreateBox("Alert Support Building", new Vector3(-17.8f, 0.45f, -6.7f), new Vector3(2.6f, 0.9f, 1.4f), supportBuildingMaterial, parent);

            CreateBox("JASDF Transport Hangar West", new Vector3(-3.6f, 0.9f, -14.4f), new Vector3(4.4f, 1.8f, 2.5f), hangarMaterial, parent);
            CreateBox("JASDF Transport Hangar East", new Vector3(1.6f, 0.85f, -14.5f), new Vector3(4f, 1.7f, 2.4f), hangarMaterial, parent);
            CreateBox("Base Operations Blocks", new Vector3(-0.9f, 0.45f, -6.7f), new Vector3(5.8f, 0.9f, 1.4f), supportBuildingMaterial, parent);

            CreateBox("DOM TERMINAL Main Blockout", new Vector3(12.5f, 0.85f, -15.35f), new Vector3(18.2f, 1.7f, 2.35f), terminalMaterial, parent);
            CreateBox("INTL TERMINAL Main Blockout", new Vector3(25.7f, 0.92f, -15.35f), new Vector3(8.4f, 1.85f, 2.55f), terminalMaterial, parent);
            CreateBox("Passenger Terminal Concourse Joint", new Vector3(21.3f, 0.72f, -14.05f), new Vector3(1.9f, 1.45f, 1.35f), terminalMaterial, parent);
            CreateBox("DOM Finger Pier West Blockout", new Vector3(8.6f, 0.68f, -12.35f), new Vector3(1.55f, 1.35f, 5.25f), terminalMaterial, parent);
            CreateBox("DOM Finger Pier East Blockout", new Vector3(16.3f, 0.68f, -12.35f), new Vector3(1.55f, 1.35f, 5.25f), terminalMaterial, parent);
            CreateBox("INTL Widebody Pier Blockout", new Vector3(25.7f, 0.72f, -12.55f), new Vector3(5.9f, 1.45f, 2.05f), terminalMaterial, parent);
            CreateBox("DOM West Future Stand Reserve", new Vector3(4.4f, 0.045f, -9.25f), new Vector3(2.4f, 0.045f, 5.15f), secondaryApronMaterial, parent);
            CreateBox("DOM Center Pushback Reserve", new Vector3(12.5f, 0.047f, -9f), new Vector3(4.6f, 0.045f, 5.45f), secondaryApronMaterial, parent);
            CreateBox("DOM East Future Stand Reserve", new Vector3(19.4f, 0.045f, -9.15f), new Vector3(2.8f, 0.045f, 5.25f), secondaryApronMaterial, parent);
            CreateBox("INTL Widebody Future Stand Reserve", new Vector3(30.4f, 0.047f, -8.8f), new Vector3(3.6f, 0.045f, 6.5f), secondaryApronMaterial, parent);
            CreateBoardingBridge("Boarding Bridge SPOT 01", new Vector3(7.05f, 0.78f, -10.55f), new Vector3(0.22f, 0.2f, 1.95f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 02", new Vector3(10f, 0.78f, -10.45f), new Vector3(0.22f, 0.2f, 2.15f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 03", new Vector3(17.05f, 0.78f, -10.45f), new Vector3(0.22f, 0.2f, 2.15f), parent);
            CreateBoardingBridge("Boarding Bridge SPOT 04", new Vector3(26.2f, 0.82f, -10.35f), new Vector3(0.28f, 0.22f, 2.8f), parent);

            CreateBox("TWR Shaft Blockout", new Vector3(8.4f, 1.65f, 4.95f), new Vector3(0.85f, 3.3f, 0.85f), towerMaterial, parent);
            CreateBox("TWR Cab Blockout", new Vector3(8.4f, 3.55f, 4.95f), new Vector3(2f, 0.85f, 2f), towerMaterial, parent);
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
            CreateSpotStand(standName, spotLabel, spotPosition, new Vector3(3.2f, 0.16f, 2.55f), parent);
        }

        private void CreateSpotStand(string standName, string spotLabel, Vector3 spotPosition, Vector3 standScale, Transform parent)
        {
            var basePosition = spotPosition + Vector3.down * 0.5f;
            var halfWidth = standScale.x * 0.5f;
            var halfDepth = standScale.z * 0.5f;
            CreateBox(standName, basePosition, standScale, gateMaterial, parent);
            CreateBox($"{spotLabel} Front Line", basePosition + new Vector3(0f, 0.12f, halfDepth - 0.22f), new Vector3(standScale.x - 0.45f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Rear Line", basePosition + new Vector3(0f, 0.12f, -halfDepth + 0.22f), new Vector3(standScale.x - 0.45f, 0.04f, 0.07f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Left Line", basePosition + new Vector3(-halfWidth + 0.2f, 0.12f, 0f), new Vector3(0.07f, 0.04f, standScale.z - 0.55f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Right Line", basePosition + new Vector3(halfWidth - 0.2f, 0.12f, 0f), new Vector3(0.07f, 0.04f, standScale.z - 0.55f), spotMarkingMaterial, parent);
            CreateBox($"{spotLabel} Stop Bar", basePosition + new Vector3(0f, 0.13f, halfDepth * 0.36f), new Vector3(standScale.x * 0.42f, 0.045f, 0.08f), runwayMarkingMaterial, parent);
        }

        private void CreateBoardingBridge(string bridgeName, Vector3 position, Vector3 scale, Transform parent)
        {
            CreateBox(bridgeName, position, scale, boardingBridgeMaterial, parent);
            CreateBox($"{bridgeName} Head", position + new Vector3(0f, -0.04f, scale.z * 0.5f), new Vector3(0.58f, 0.28f, 0.44f), boardingBridgeMaterial, parent);
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

        private GameObject CreateFlatPolygon(string objectName, float y, Material material, Transform parent, params Vector2[] points)
        {
            var polygon = new GameObject(objectName);
            polygon.transform.SetParent(parent);

            var vertices = new Vector3[points.Length];
            for (var index = 0; index < points.Length; index++)
            {
                vertices[index] = new Vector3(points[index].x, y, points[index].y);
            }

            var triangles = new int[(points.Length - 2) * 3];
            var triangleIndex = 0;
            for (var index = 1; index < points.Length - 1; index++)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = index + 1;
                triangles[triangleIndex++] = index;
            }

            var mesh = new Mesh
            {
                name = objectName,
                vertices = vertices,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            polygon.AddComponent<MeshFilter>().sharedMesh = mesh;
            polygon.AddComponent<MeshRenderer>().sharedMaterial = material;
            return polygon;
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
