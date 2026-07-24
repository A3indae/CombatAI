# CombatAI

SCP: Secret Laboratory  **전투 NPC** 를 만들기 위한 EXILED 프레임워크

공성전 모드를 위해 만들어졌기에 기본적인 설정은 해당 모드를 따릅니다.

# 기본 사용법

CombatAI 플러그인을 이용하는 기본적인 방법

## NavmeshHandler

CombatAI를 생성하기 전에 NavMesh를 필수적으로 생성해야 합니다.

```csharp
// 예시
// 맵 생성이 끝난 시점(라운드 시작 후)에 한 번 호출
NavmeshHandler.AddNavmesh(new Bounds(Vector3.zero, new Vector3(500f, 100f, 500f)));
```

### void AddNavmesh(Bounds bounds)

지정한 영역의 NavMesh를 만듭니다.

### void ClearNavmesh()

**모든** NavMesh 데이터를 지웁니다.
라운드 종료 시 플러그인이 해당 메서드를 자동으로 호출합니다.

### void RecreateNavmesh(Bounds bounds)

기존 NavMesh를 **모두** 없앤 후 재생성합니다.

### bool IsBuilt

NavMesh가 존재하는지 여부를 반환합니다.

### 설정

`AddNavmesh()` 호출 시 반영되는 Agent 설정

| 프로퍼티 | 기본값 | 설명 |
|---|---|---|
| `AgentRadius` | `0.17f` | 에이전트 반지름 |
| `AgentHeight` | `1f` | 에이전트 높이 |
| `AgentClimb` | `0.24f` | 오를 수 있는 턱 높이 |
| `AgentSlope` | `45f` | 오를 수 있는 경사 각도 |
| `NavMeshMask` | `LayerMasks.CharacterCollision` | NavMesh 소스로 수집할 레이어 |

## CombatAIHandler

CombatAI를 관리하는 개체입니다. AI의 생성과 제거는 **반드시** 이곳을 거쳐야 합니다.

```csharp
var ai = CombatAIHandler.AddCombatAI<Gunner>(spawnPosition);
```

### T AddCombatAI\<T\>(Vector3 pos)

해당 위치에 AI를 생성하고 목록에 등록한 뒤 반환합니다.

### void RemoveById(int id)

해당 AI를 제거합니다. `id` 는 `ai.Npc.Id` 입니다.

### void ClearCombatAI()

모든 AI를 제거합니다. NavMesh와 달리 **라운드 종료 시 자동으로 호출되지 않으니** 직접 불러줘야 합니다.

### Dictionary\<int, BaseCombatAI\> List

현재 살아있는 AI 전체입니다. 키는 `Npc.Id`.

## BaseCombatAI

기본적인 CombatAI의 상위 클래스입니다.

### T TryAddModule<T>() where T : BaseCombatAIModule

CombatAI에 모듈을 삽입합니다. 이미 존재할 경우 존재하는 모듈을 반환합니다.

### T TryGetModule<T>() where T : BaseCombatAIModule

CombatAI에서 삽입된 모듈을 불러옵니다. 존재하지 않으면 null을 반환합니다.

### TryDestroyModule<T>() where T : BaseCombatAIModule

인수로 넣은 타입의 모듈을 제거합니다. 모듈이 존재하지 않더라도 예외를 일으키지 않습니다.

# 기본 모듈
CombatAI는 모듈 기능을 이용하여 간결한 코드 작성이 가능하게 합니다.

### Targeter

타겟 지정에 관여하는 모듈입니다.
기본적으로 공성전 모드에 맞도록 기본값이 세팅되어 있습니다.
SearchRange가 1000 이상일 경우 탐색 범위를 무한으로 처리합니다.
NPC끼리 타게팅하지 않습니다.

### Chaser

추격 및 카이팅에 관여하는 모듈입니다. KitingRange 값이 0.5 이하일 경우 근접 유닛처럼 행동합니다.

### Looker

대상을 바라보는것에 관여하는 모듈입니다.

### Acter

대상의 모든 행동에 관여하는 모듈입니다.
아이템 지급 및 들기도 포함됩니다.

# 상속 규칙

BaseCombatAI / BaseCombatAIModule을 상속할 때 유의할 점

## BaseCombatAI
반드시 ExampleUnit.cs의 코드를 참고해 주십시오.

### RoleType / Name

RoleType, Name 필드는 **반드시** 아래 예시와 같이 작성해주세요.

```csharp
public override string Name => "반란죄수";
public override string RoleType => RoleTypeId.ClassD;
```

### 모듈

모듈은 반드시 TryAddModule 메서드를 이용해 주세요.

### void OnDead(DamageHandlerBase handler);

NPC가 사망했을 때 호출되는 메서드입니다.
특별한 이유가 없다면 메모리 누수가 발생하지 않도록 뒷정리 및 API.CombatAIHandler.RemoveById(Npc.Id);를 해주시기 바랍니다.
CombatAIHandler를 통해 삭제될 경우 참조한 모듈과 NPC, OnDead 이벤트는 자동으로 해제합니다.