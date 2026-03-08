# ArcheageLike 프로토타입 - 씬 셋업 가이드

## 빠른 시작 (Quick Start)

### 1. 자동 셋업
1. 새로운 Scene을 만들거나 `SampleScene`을 엽니다
2. 빈 GameObject를 만들고 `SceneSetupHelper` 컴포넌트를 추가합니다
3. Play 버튼을 누르면 자동으로 테스트 환경이 구성됩니다

### 2. 수동 셋업

#### Player 구성
```
Player (GameObject)
├── CharacterController
├── CharacterStats
├── ThirdPersonController
├── CharacterAnimController
├── TargetingSystem
├── SkillSystem
├── BuildingPlacer
└── Animator (Humanoid AnimatorController 할당 필요)
```
- Tag: `Player`
- Layer: `Default`

#### Camera
```
Main Camera
└── ThirdPersonCamera (Target: Player)
```

#### Enemy 구성
```
Enemy (GameObject)
├── CharacterStats
├── Targetable (Faction: Hostile)
├── NavMeshAgent
├── EnemyAI
└── Collider
```
- NavMesh 베이크 필요 (Window > AI > Navigation)

#### Ship 구성
```
Ship (GameObject)
├── Rigidbody (Mass: 500, Drag: 1)
├── ShipController (ShipData SO 할당)
├── ShipInteraction
├── Collider(s)
├── Helm Position (empty child)
├── Boarding Position (empty child)
└── Buoyancy Points (empty children, 배 모서리에 배치)
```

#### Housing Zone
```
HousingZone (GameObject)
├── BoxCollider (isTrigger: true)
└── HousingZone
```

#### UI (Canvas)
```
Canvas
├── HUDManager
├── PlayerFrame
│   ├── HealthBar (Slider)
│   ├── ManaBar (Slider)
│   └── StaminaBar (Slider)
├── TargetFrame
│   ├── TargetName (TMP_Text)
│   └── TargetHealthBar (Slider)
├── SkillBar
│   ├── SkillSlot_1 (SkillSlotUI)
│   ├── SkillSlot_2
│   ├── SkillSlot_3
│   └── SkillSlot_4
├── ShipHUD (기본 비활성)
├── InteractionPrompt (TMP_Text)
└── Minimap
    └── MinimapCamera (별도 Camera)
```

## Input 설정

### New Input System 사용 시
Input Actions Asset에 다음 액션을 추가하세요:

| Action | Binding | Type |
|--------|---------|------|
| Move | WASD | Vector2 |
| Look | Mouse Delta | Vector2 |
| Run | Left Shift | Button |
| Jump | Space | Button |
| Attack | Left Mouse | Button |
| Skill1 | 1 | Button |
| Skill2 | 2 | Button |
| Skill3 | 3 | Button |
| Skill4 | 4 | Button |
| TabTarget | Tab | Button |
| Interact | F | Button |
| Inventory | I | Button |
| Escape | Escape | Button |
| RightMouse | Right Mouse | Button |
| Scroll | Mouse Scroll | Vector2 |
| RotateBuilding | R | Button |
| PlaceBuilding | Left Mouse | Button |
| CancelBuilding | Escape | Button |

## ScriptableObject 생성

### Skill 생성
`Assets > Create > ArcheageLike > Skill Data`
- 물리 공격, 마법 공격, 힐 등 다양한 스킬을 만들 수 있습니다
- `comboNextSkill`로 콤보 체인 구성 가능

### Ship 생성
`Assets > Create > ArcheageLike > Ship Data`

### Building 생성
`Assets > Create > ArcheageLike > Building Data`

## 레이어 설정 (권장)

| Layer | Name | 용도 |
|-------|------|------|
| 6 | Ground | 지면 (캐릭터 이동/건설) |
| 7 | Water | 수면 |
| 8 | Enemy | 적 NPC |
| 9 | Interactable | 상호작용 가능 오브젝트 |
| 10 | Building | 건물 |

## 폴더 구조

```
Assets/_ArcheageLike/
├── Scripts/
│   ├── Core/         - Singleton, GameManager, EventBus, ObjectPool
│   ├── Character/    - Controller, Stats, Camera, Animation
│   ├── Combat/       - Targeting, Skills, EnemyAI
│   ├── Sailing/      - Ship, Water
│   ├── Housing/      - Building Placement, Zones
│   ├── UI/           - HUD, Skill Slots, Damage Popup, Minimap
│   ├── Data/         - ScriptableObject 정의
│   └── Utils/        - 유틸리티 (Scene Setup, FPS Counter)
├── Prefabs/
├── Materials/
├── ScriptableObjects/
├── Scenes/
└── Shaders/
```

## 다음 단계 (TODO)

- [ ] Animator Controller 생성 (Idle/Walk/Run/Attack/Skill/Swim 상태)
- [ ] NavMesh 베이크
- [ ] 캐릭터/적 3D 모델 교체
- [ ] 선박 3D 모델 추가
- [ ] 건물 프리팹 제작
- [ ] 인벤토리 시스템 확장
- [ ] 무역/교역 시스템
- [ ] 농사 시스템 (Farm building + crop growth)
- [ ] 멀티플레이어 (Netcode for GameObjects 또는 Mirror)
- [ ] 사운드/BGM
- [ ] 파티클/VFX
