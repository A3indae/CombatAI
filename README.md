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

# 상속 규칙

BaseCombatAI / BaseCombatAIModule을 상속할 때 유의할 점

## BaseCombatAI

```csharp
// 예시
public class Sniper : BaseCombatAI
{
    public override RoleTypeId RoleType => RoleTypeId.ChaosRifleman;
    public override string Name => "저격수";

    public Sniper(Vector3 spawnPosition) : base(spawnPosition)
    {
        var chaser = TryAddModule<Chaser>();
        chaser.KitingRange = 100f;

        TryAddModule<Targeter>();
        TryAddModule<Acter>();
    }

    protected override void OnDead(DamageHandlerBase handler)
    {
        CombatAIHandler.RemoveById(Npc.Id);
    }
}
```

### RoleType / Name 은 필드를 참조하면 안 됩니다

`BaseCombatAI` 생성자가 `Npc.Spawn(Name, RoleType, spawnPosition)` 을 호출하는 시점에는
**파생 클래스의 필드가 아직 초기화되지 않았습니다.**

```csharp
public override string Name => "저격수";      // O
public override string Name => _customName;   // X, 이 시점엔 null
```

이름을 동적으로 바꾸려면 스폰 이후에 `Npc` 쪽에서 직접 수정하세요.

### 모듈은 생성자에서 붙입니다

기본값 세팅도 여기서 함께 처리하면 스폰 직후부터 의도한 상태로 동작합니다.
같은 타입의 모듈은 **1개만** 존재할 수 있어, 두 번 `TryAddModule` 해도 기존 인스턴스가 반환됩니다.

### OnDead 는 반드시 구현해야 합니다

`OnThisPlayerDied` 에 연결되는 사망 콜백입니다.
사망해도 `CombatAIHandler.List` 에서 자동으로 빠지지 않으므로,
리스폰시키지 않을 거라면 여기서 `RemoveById(Npc.Id)` 를 불러 정리하세요.

### Destroy() 는 오버라이드할 수 없습니다

`virtual` 이 아닙니다. 제거 시점의 추가 정리가 필요하면 모듈의 `Destroy()` 나 `OnDead` 에 넣으세요.

## BaseCombatAIModule

```csharp
public class Screamer : BaseCombatAIModule
{
    private CoroutineHandle loop;

    public Screamer(BaseCombatAI owner) : base(owner) { }

    public void Start()
    {
        if (!loop.IsRunning)
            loop = Timing.RunCoroutine(Loop());
    }

    private IEnumerator<float> Loop()
    {
        while (true)
        {
            // Owner.Npc 로 NPC에 접근
            yield return Timing.WaitForSeconds(5f);
        }
    }

    public override void Destroy() => Timing.KillCoroutines(loop);
}
```

### 생성자는 `(BaseCombatAI owner)` 하나여야 합니다

`TryAddModule<T>()` 도 `Activator.CreateInstance(typeof(T), this)` 로 생성합니다.
유닛과 마찬가지로 시그니처가 다르면 런타임에 터집니다.

### Destroy() 에서 반드시 뒷정리를 하세요

`abstract` 이므로 구현은 강제되지만, 내용이 비어 있으면 아무것도 정리되지 않습니다.
코루틴(`Timing.KillCoroutines`), 이벤트 구독 해제, 타이머는 전부 여기서 끊어야 합니다.
`while(true)` 루프를 돌리는 모듈이 이걸 빠뜨리면 **NPC가 사라진 뒤에도 코루틴이 계속 돕니다.**

### NPC 접근은 Owner 를 통해서 합니다

모듈이 생성되는 시점에는 `Owner.Npc` 가 이미 스폰을 마친 상태이므로 생성자에서 바로 참조해도 됩니다.
단, 역할(Role)에 의존하는 값을 캐싱한다면 역할이 바뀔 때 무효화되므로 주기적으로 갱신해야 합니다.

### 모듈 인스턴스를 직접 만들지 마세요

`new Screamer(ai)` 로 만들면 `Owner` 의 모듈 목록에 등록되지 않아
`TryGetModule` 로 찾을 수 없고 `TryDestroyModule` 로도 정리되지 않습니다. 반드시 `TryAddModule<T>()` 를 쓰세요.

### 새 파일은 csproj에 추가해야 합니다

이 프로젝트는 SDK 스타일이 아닌 구식 csproj라 자동 포함이 되지 않습니다.
새 `.cs` 를 만들었다면 `CombatAI.csproj` 의 `<Compile Include="..." />` 목록에 직접 추가하세요.