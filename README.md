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

살아있는 AI를 추적하는 레지스트리입니다. AI의 생성과 제거는 **반드시** 이곳을 거쳐야 합니다.

```csharp
var ai = CombatAIHandler.AddCombatAI<Gunner>(spawnPosition);
```

> `new Gunner(pos)` 로 직접 만들지 마세요. NPC는 스폰되지만 목록에 등록되지 않아,
> `RemoveById()` / `ClearCombatAI()` 로 정리되지 않으며, 라운드가 끝나도 NPC와 코루틴이 그대로 남습니다.

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

