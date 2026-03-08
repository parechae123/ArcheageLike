using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using ArcheageLike.Character;
using ArcheageLike.Combat;
using ArcheageLike.Housing;
using ArcheageLike.Sailing;
using ArcheageLike.Core;
using ArcheageLike.UI;

namespace ArcheageLike.Utils
{
    /// <summary>
    /// Quick scene setup helper for prototyping.
    /// Creates basic terrain, water, player, and test objects.
    /// Runtime NavMesh bake included — no manual bake needed.
    /// Add this to an empty GameObject and press Play.
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool _createTerrain = true;
        [SerializeField] private bool _createWater = true;
        [SerializeField] private bool _createPlayer = true;
        [SerializeField] private bool _createTestEnemy = true;
        [SerializeField] private bool _createHousingZone = true;
        [SerializeField] private bool _createShip = true;
        [SerializeField] private bool _createTradePosts = true;
        [SerializeField] private bool _createFarmPlots = true;
        [SerializeField] private bool _createUI = true;

        [Header("Settings")]
        [SerializeField] private float _terrainSize = 200f;
        [SerializeField] private float _waterLevel = -1f;

        private GameObject _ground;
        private GameObject _player;

        private void Awake()
        {
            // --- Phase 1: Core world ---
            if (_createTerrain) CreateTerrain();
            if (_createWater) CreateWater();
            if (_createPlayer) CreatePlayer();
            if (_createHousingZone) CreateHousingZone();

            // NavMesh를 먼저 Bake한 뒤 Enemy 생성
            if (_createTerrain) BakeRuntimeNavMesh();
            if (_createTestEnemy) CreateTestEnemy();

            // --- Phase 2: Systems ---
            CreateManagers();
            if (_createShip) CreateTestShip();
            if (_createTradePosts) CreateTradePosts();
            if (_createFarmPlots) CreateFarmPlots();

            // UI는 모든 오브젝트 생성 후 마지막에
            if (_createUI) CreateUI();
        }

        // ===============================================
        // Phase 1 — World
        // ===============================================

        private void CreateTerrain()
        {
            _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _ground.name = "Ground";
            _ground.transform.localScale = new Vector3(_terrainSize / 10f, 1f, _terrainSize / 10f);
            _ground.layer = LayerMask.NameToLayer("Default");
            _ground.isStatic = true;

            // Green-ish color
            var renderer = _ground.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.3f, 0.6f, 0.2f);
                renderer.material = mat;
            }
        }

        private void BakeRuntimeNavMesh()
        {
            if (_ground == null) return;

            // NavMeshSurface로 런타임 Bake
            var surface = _ground.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();

            Debug.Log("[Setup] NavMesh runtime bake complete.");
        }

        private void CreateWater()
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Water";
            water.transform.position = new Vector3(0f, _waterLevel, 0f);
            water.transform.localScale = new Vector3(_terrainSize / 5f, 1f, _terrainSize / 5f);

            var renderer = water.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetFloat("_Surface", 1); // transparent
                mat.color = new Color(0.1f, 0.4f, 0.8f, 0.5f);
                renderer.material = mat;
            }

            // Remove MeshCollider (concave mesh can't be trigger), use BoxCollider instead
            var meshCol = water.GetComponent<MeshCollider>();
            if (meshCol != null) DestroyImmediate(meshCol);
            var boxCol = water.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(1f, 0.1f, 1f);

            water.AddComponent<SimpleWater>();
        }

        private void CreatePlayer()
        {
            _player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            _player.name = "Player";
            _player.tag = "Player";
            _player.transform.position = new Vector3(0f, 1f, 0f);

            // Remove default collider (CharacterController adds its own)
            DestroyImmediate(_player.GetComponent<CapsuleCollider>());

            // CharacterController
            var cc = _player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0f, 1f, 0f);

            // Core components
            _player.AddComponent<CharacterStats>();
            _player.AddComponent<ThirdPersonController>();
            _player.AddComponent<TargetingSystem>();
            _player.AddComponent<SkillSystem>();
            _player.AddComponent<BuildingPlacer>();

            // Animator (visual feedback only)
            _player.AddComponent<Animator>();
            _player.AddComponent<CharacterAnimController>();
            _player.AddComponent<RuntimeAnimatorSetup>();

            // Phase 2 components
            _player.AddComponent<Inventory>();

            // Visual
            var meshRenderer = _player.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.5f, 1f);
                meshRenderer.material = mat;
            }

            // Setup camera to follow player
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                var camController = mainCam.gameObject.AddComponent<ThirdPersonCamera>();
                // ThirdPersonCamera will auto-find Player tag in Start()
            }

            Debug.Log("[Setup] Player created with all systems.");
        }

        private void CreateTestEnemy()
        {
            for (int i = 0; i < 3; i++)
            {
                var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = $"Enemy_{i}";
                enemy.transform.position = new Vector3(5f + i * 4f, 0.5f, 10f);
                enemy.layer = LayerMask.NameToLayer("Default");

                enemy.AddComponent<CharacterStats>();
                enemy.AddComponent<Targetable>();

                var agent = enemy.AddComponent<NavMeshAgent>();
                agent.speed = 3.5f;

                enemy.AddComponent<EnemyAI>();
                enemy.AddComponent<RuntimeAnimatorSetup>();

                var renderer = enemy.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(0.8f, 0.2f, 0.2f);
                    renderer.material = mat;
                }
            }

            Debug.Log("[Setup] Test enemies created on NavMesh.");
        }

        private void CreateHousingZone()
        {
            var zone = new GameObject("HousingZone");
            zone.transform.position = new Vector3(-20f, 0f, 0f);

            var boxCol = zone.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(30f, 5f, 30f);
            boxCol.isTrigger = true;

            zone.AddComponent<HousingZone>();

            Debug.Log("[Setup] Housing zone created at (-20, 0, 0)");
        }

        // ===============================================
        // Phase 2 — Managers & Systems
        // ===============================================

        private void CreateManagers()
        {
            // SoundManager (Singleton — 자동 생성)
            if (SoundManager.Instance == null)
            {
                var soundGo = new GameObject("SoundManager");
                soundGo.AddComponent<SoundManager>();
            }

            // HitEffectSystem (전투 VFX)
            var hitFxGo = new GameObject("HitEffectSystem");
            hitFxGo.AddComponent<HitEffectSystem>();

            // TradeSystem
            var tradeGo = new GameObject("TradeSystem");
            tradeGo.AddComponent<TradeSystem>();

            Debug.Log("[Setup] Phase 2 managers initialized.");
        }

        private void CreateUI()
        {
            var uiGo = new GameObject("RuntimeUI");
            var builder = uiGo.AddComponent<RuntimeUIBuilder>();

            Debug.Log("[Setup] Runtime UI created.");
        }

        // ===============================================
        // Phase 2 — Ship (Runtime Prefab)
        // ===============================================

        private void CreateTestShip()
        {
            // 아키에이지 스타일 범선 (간단한 프리미티브 조합)
            var ship = new GameObject("TestShip_Clipper");
            ship.transform.position = new Vector3(30f, _waterLevel + 0.5f, -20f);

            // 선체 (Hull) — 늘린 큐브
            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "Hull";
            hull.transform.SetParent(ship.transform);
            hull.transform.localPosition = Vector3.zero;
            hull.transform.localScale = new Vector3(3f, 1f, 8f);
            var hullMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            hullMat.color = new Color(0.55f, 0.35f, 0.15f); // wood brown
            hull.GetComponent<MeshRenderer>().material = hullMat;

            // 갑판 (Deck) — 얇은 큐브
            var deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "Deck";
            deck.transform.SetParent(ship.transform);
            deck.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            deck.transform.localScale = new Vector3(2.8f, 0.1f, 7.5f);
            var deckMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            deckMat.color = new Color(0.65f, 0.45f, 0.25f); // lighter wood
            deck.GetComponent<MeshRenderer>().material = deckMat;

            // 돛대 (Mast) — 가는 실린더
            var mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mast.name = "Mast";
            mast.transform.SetParent(ship.transform);
            mast.transform.localPosition = new Vector3(0f, 3.5f, 0f);
            mast.transform.localScale = new Vector3(0.15f, 3f, 0.15f);
            var mastMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mastMat.color = new Color(0.4f, 0.25f, 0.1f);
            mast.GetComponent<MeshRenderer>().material = mastMat;

            // 돛 (Sail) — 얇은 큐브
            var sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sail.name = "Sail";
            sail.transform.SetParent(ship.transform);
            sail.transform.localPosition = new Vector3(0f, 3.5f, 0.5f);
            sail.transform.localScale = new Vector3(0.05f, 3f, 3f);
            var sailMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sailMat.color = new Color(0.9f, 0.9f, 0.85f); // off-white canvas
            sail.GetComponent<MeshRenderer>().material = sailMat;

            // 조타석 마커 (Helm) — 작은 구
            var helm = new GameObject("HelmPoint");
            helm.transform.SetParent(ship.transform);
            helm.transform.localPosition = new Vector3(0f, 0.7f, -3f);

            // Rigidbody (부력 & 항해)
            var rb = ship.AddComponent<Rigidbody>();
            rb.mass = 500f;
            rb.linearDamping = 2f;
            rb.angularDamping = 3f;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // BoxCollider for whole ship
            var shipCol = ship.AddComponent<BoxCollider>();
            shipCol.size = new Vector3(3f, 1.5f, 8f);
            shipCol.center = new Vector3(0f, 0.5f, 0f);

            // Ship components
            var sc = ship.AddComponent<ShipController>();
            var si = ship.AddComponent<ShipInteraction>();

            Debug.Log("[Setup] Test ship (Clipper) created at sea.");
        }

        // ===============================================
        // Phase 2 — Trade Posts
        // ===============================================

        private void CreateTradePosts()
        {
            // 내륙 교역소
            CreateSingleTradePost("Solzreed Trade Post", new Vector3(-40f, 0.5f, 30f), false);
            // 해안 항구
            CreateSingleTradePost("Austera Sea Port", new Vector3(40f, _waterLevel + 0.5f, -30f), true);

            Debug.Log("[Setup] Trade posts created.");
        }

        private void CreateSingleTradePost(string postName, Vector3 position, bool isSeaPort)
        {
            var post = new GameObject(postName);
            post.transform.position = position;

            // 시각적 표시 — 기둥
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            pillar.transform.SetParent(post.transform);
            pillar.transform.localPosition = new Vector3(0f, 2f, 0f);
            pillar.transform.localScale = new Vector3(0.5f, 2f, 0.5f);

            var pillarMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            pillarMat.color = isSeaPort ? new Color(0.2f, 0.6f, 0.9f) : new Color(0.9f, 0.7f, 0.2f);
            pillar.GetComponent<MeshRenderer>().material = pillarMat;

            // 표지판 — 상단 구
            var sign = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sign.name = "Sign";
            sign.transform.SetParent(post.transform);
            sign.transform.localPosition = new Vector3(0f, 4.5f, 0f);
            sign.transform.localScale = new Vector3(1f, 1f, 1f);

            var signMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            signMat.color = isSeaPort ? Color.cyan : Color.yellow;
            signMat.SetFloat("_Smoothness", 0.8f);
            sign.GetComponent<MeshRenderer>().material = signMat;

            // Trigger zone for delivery
            var triggerCol = post.AddComponent<SphereCollider>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 5f;

            // TradePostMarker
            var marker = post.AddComponent<TradePostMarker>();

            Debug.Log($"[Setup] Trade post '{postName}' at {position}");
        }

        // ===============================================
        // Phase 2 — Farm Plots
        // ===============================================

        private void CreateFarmPlots()
        {
            // 하우징 존 근처에 3x3 농장 배치
            Vector3 farmOrigin = new Vector3(-25f, 0.01f, -10f);

            for (int x = 0; x < 3; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Vector3 pos = farmOrigin + new Vector3(x * 3f, 0f, z * 3f);
                    CreateSingleFarmPlot($"FarmPlot_{x}_{z}", pos);
                }
            }

            Debug.Log("[Setup] 9 farm plots created near housing zone.");
        }

        private void CreateSingleFarmPlot(string plotName, Vector3 position)
        {
            var plot = new GameObject(plotName);
            plot.transform.position = position;

            // 땅 표시 — 얇은 큐브
            var soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            soil.name = "Soil";
            soil.transform.SetParent(plot.transform);
            soil.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            soil.transform.localScale = new Vector3(2.5f, 0.1f, 2.5f);

            var soilMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            soilMat.color = new Color(0.4f, 0.25f, 0.1f); // dark soil
            soil.GetComponent<MeshRenderer>().material = soilMat;

            // FarmPlot component
            plot.AddComponent<FarmPlot>();

            // Interaction trigger
            var col = plot.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(2.5f, 1f, 2.5f);
            col.center = new Vector3(0f, 0.5f, 0f);
        }
    }
}
