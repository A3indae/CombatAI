# CombatAI

SCP: Secret Laboratory 서버에서 **전투하는 NPC(Dummy)** 를 만들기 위한 EXILED 플러그인 / 프레임워크입니다.

Unity NavMesh를 런타임에 구워서 길찾기를 하고, NPC의 행동을 **모듈** 단위로 조립합니다.
추격·카이팅은 `Chaser`, 사격·아이템 조작은 `Acter` 가 담당하며, 필요한 모듈만 골라 붙이는 구조입니다.

- 언어/런타임: C# / .NET Framework 4.8.1
- 의존성: EXILED (Exiled.API, Exiled.Events, Exiled.Loader), LabApi, MEC, Mirror, UnityEngine (+ AIModule, PhysicsModule)
- 라이선스: [MIT](LICENSE)

---

## 목차

- [빠른 시작](#빠른-시작)
- [프로젝트 구조](#프로젝트-구조)
- [핵심 규칙](#핵심-규칙)
- [API 레퍼런스](#api-레퍼런스)
- [알려진 이슈](#알려진-이슈)

---

## 빠른 시작

### 1. NavMesh 굽기

**이게 제일 중요합니다.** `Chaser` 는 `UnityEngine.AI.NavMesh` 에 전적으로 의존하기 때문에,
NPC를 스폰하기 전에 반드시 해당 구역의 NavMesh가 존재해야 합니다. 없으면 AI는 그냥 제자리에 서 있습니다.

```csharp
using CombatAI.API;
using UnityEngine;

// 라운드 시작 후, 맵 생성이 끝난 시점에 호출할 것
NavmeshHandler.AddNavmesh(new Bounds(center: Vector3.zero, size: new Vector3(500f, 100f, 500f)));
```

`Bounds` 는 AI가 돌아다닐 영역 전체를 덮어야 합니다. 너무 크게 잡으면 굽는 데 시간이 걸리므로
필요한 구역(예: 라이트 컨테인먼트)만 감싸는 게 좋습니다.

### 2. 유닛 정의

`BaseCombatAI` 를 상속해서 유닛 하나를 정의합니다. 생성자에서 필요한 모듈을 붙입니다.

```csharp
using CombatAI.API.Core;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;

public class Gunner : BaseCombatAI
{
    public override RoleTypeId RoleType => RoleTypeId.ClassD;
    public override string Name => "허접AI";

    // 생성자 시그니처는 반드시 (Vector3) 하나여야 합니다. (아래 '핵심 규칙' 참고)
    public Gunner(Vector3 spawnPosition) : base(spawnPosition)
    {
        TryAddModule<CombatAI.API.Module.Chaser>();
        TryAddModule<CombatAI.API.Module.Acter>();
    }

    protected override void OnDead(DamageHandlerBase handler)
    {
        // 사망 처리 (리스폰, 로그, 아이템 드랍 등)
    }
}
```

실제 예제는 [`Example/Unit/Gunner.cs`](CombatAI/Example/Unit/Gunner.cs) 에 있습니다.

### 3. 스폰하고 굴리기

```csharp
using CombatAI.API;
using CombatAI.API.Module;

var ai = CombatAIHandler.AddCombatAI<Gunner>(spawnPosition);

// 총 꺼내기
var acter = ai.TryGetModule<Acter>();
acter.TryEquipItem(ItemType.GunCOM18, returnIfNull: false);

// 추격 시작
var chaser = ai.TryGetModule<Chaser>();
chaser.CurrentTarget = somePlayer;   // 타겟은 직접 넣어줘야 합니다
chaser.KitingRange = 7f;             // 이 거리를 유지하며 싸움
chaser.StartChase();

// 사격
acter.TryShoot(hold: true);
// ... 잠시 후
acter.TryReleaseShoot();
```

### 4. 정리

```csharp
CombatAIHandler.RemoveById(ai.Npc.Id);   // 하나만 제거
CombatAIHandler.ClearCombatAI();         // 전부 제거
NavmeshHandler.ClearNavmesh();           // NavMesh 해제 (라운드 종료 시)
```

`Main.cs` 는 `Server.EndingRound` 이벤트에서 `ClearNavmesh()` 를 자동으로 호출합니다.
**NPC 정리는 자동으로 되지 않으므로** 라운드 종료 시 `ClearCombatAI()` 를 직접 불러야 합니다.

---

## 프로젝트 구조

```
CombatAI/
├─ Main.cs                        플러그인 진입점 (EXILED Plugin<Config>)
├─ API/
│  ├─ CombatAIHandler.cs          살아있는 AI 인스턴스 레지스트리 (static)
│  ├─ NavmeshHandler.cs           런타임 NavMesh 빌드/해제 (static)
│  ├─ Core/
│  │  ├─ BaseCombatAI.cs          모든 AI 유닛의 베이스 (NPC 스폰 + 모듈 컨테이너)
│  │  └─ BaseCombatAIModule.cs    모든 모듈의 베이스
│  └─ Module/
│     ├─ Chaser.cs                이동 / 추격 / 카이팅 / 길찾기
│     ├─ Acter.cs                 사격, 조준, 아이템 장착·사용
│     └─ Targeter.cs              (미구현 — 타겟 선정 예정)
└─ Example/
   └─ Unit/Gunner.cs              사용 예제 유닛
```

설계 요약:

| 계층 | 역할 |
|---|---|
| `CombatAIHandler` | 어떤 AI가 살아있는지 추적. 생성/제거의 단일 창구 |
| `BaseCombatAI` | NPC 하나 = 인스턴스 하나. 모듈을 타입별로 1개씩 보관 |
| `BaseCombatAIModule` | 실제 행동 로직. `Owner` 를 통해 NPC에 접근 |

---

## 핵심 규칙

시간이 지나서 잊기 쉬운, **지키지 않으면 런타임에 터지는** 규약들입니다.

### 1. 유닛 생성자는 `(Vector3)` 하나뿐이어야 합니다

`CombatAIHandler.AddCombatAI<T>()` 가 `Activator.CreateInstance(typeof(T), pos)` 로 리플렉션 생성하기 때문에,
시그니처가 다르면 컴파일은 되지만 **런타임에 `MissingMethodException`** 이 납니다.

```csharp
public Gunner(Vector3 spawnPosition) : base(spawnPosition) { }   // ✅
public Gunner(Vector3 pos, int hp) : base(pos) { }               // ❌ 리플렉션 실패
```

추가 파라미터가 필요하면 생성 후 프로퍼티로 주입하세요.

### 2. 모듈 생성자는 `(BaseCombatAI owner)` 하나뿐이어야 합니다

`TryAddModule<T>()` 도 같은 이유로 `Activator.CreateInstance(typeof(T), this)` 를 씁니다.

```csharp
public Chaser(BaseCombatAI owner) : base(owner) { }   // ✅
```

### 3. `RoleType` / `Name` 은 베이스 생성자에서 호출됩니다

`BaseCombatAI` 생성자가 `Npc.Spawn(Name, RoleType, spawnPosition)` 를 부르는데, 이 시점엔
**파생 클래스의 필드가 아직 초기화되지 않았습니다.** 두 프로퍼티는 반드시 상수/식 기반으로 구현하세요.

```csharp
public override string Name => "허접AI";                  // ✅
public override string Name => _customName;               // ❌ 이 시점엔 null
```

### 4. 모듈은 타입당 1개

`modules` 는 `Dictionary<Type, BaseCombatAIModule>` 입니다. 같은 타입을 두 번 `TryAddModule` 하면
새로 만들지 않고 **기존 인스턴스를 그대로 돌려줍니다.** `Chaser` 를 두 개 붙여 두 대상을 동시에 쫓게 할 수는 없습니다.

### 5. 호출 순서: NavMesh → 스폰 → 추격

`Chaser` 는 생성 시점에 `IFpcRole` 을 캐싱하고, 이동 계산은 전부 NavMesh 위에서 이루어집니다.
NavMesh 없이 `StartChase()` 하면 조용히 아무 일도 일어나지 않습니다.

### 6. `Chaser.CurrentTarget` 은 수동입니다

타겟 자동 선정 로직은 아직 없습니다(`Targeter` 가 비어 있음). 매 프레임/주기마다
직접 `CurrentTarget` 을 갱신해줘야 합니다. `null` 이거나 죽은 플레이어면 추격 루프는 대기만 합니다.

---

## API 레퍼런스

### `CombatAIHandler` (static)

| 멤버 | 설명 |
|---|---|
| `Dictionary<int, BaseCombatAI> List` | 살아있는 AI 전체. 키는 `Npc.Id` |
| `T AddCombatAI<T>(Vector3 pos)` | AI 생성 + 등록 후 반환 |
| `void RemoveById(int id)` | 해당 AI `Destroy()` 후 목록에서 제거 |
| `void ClearCombatAI()` | 전부 `Destroy()` 후 목록 비우기 |

### `NavmeshHandler` (static)

| 멤버 | 기본값 | 설명 |
|---|---|---|
| `float AgentRadius` | `0.17f` | 에이전트 반지름 |
| `float AgentHeight` | `1f` | 에이전트 높이 |
| `float AgentClimb` | `0.24f` | 오를 수 있는 턱 높이 |
| `float AgentSlope` | `45f` | 오를 수 있는 경사 각도 |
| `string[] Layers` | `{ "Default", "InvisibleCollider" }` | NavMesh 소스로 수집할 레이어 |
| `bool IsBuilt` | — | NavMesh가 구워져 있는지 |
| `void AddNavmesh(Bounds)` | — | 해당 영역 NavMesh 빌드 |
| `void ClearNavmesh()` | — | **모든** NavMesh 데이터 제거 |
| `void RecreateNavmesh(Bounds)` | — | Clear 후 Add |

> 에이전트 설정값은 `AddNavmesh()` 호출 **전에** 바꿔야 반영됩니다.

### `BaseCombatAI` (abstract)

| 멤버 | 설명 |
|---|---|
| `Npc Npc { get; }` | Exiled NPC 인스턴스 |
| `abstract RoleTypeId RoleType` | 스폰할 역할 |
| `abstract string Name` | NPC 표시 이름 |
| `T TryAddModule<T>()` | 모듈 추가. 이미 있으면 기존 것 반환 |
| `T TryGetModule<T>()` | 모듈 조회. 없으면 `null` |
| `void TryDestroyModule<T>()` | 모듈 `Destroy()` 후 제거 |
| `void Destroy()` | 사망 이벤트 해제 + NPC 제거 |
| `abstract void OnDead(DamageHandlerBase)` | 사망 콜백 (`OnThisPlayerDied` 에 연결됨) |

### `Chaser` — 이동 / 추격 / 카이팅

타겟과 **일정 거리를 유지하며** 따라다닙니다. `KitingRange` 기준으로 세 가지 상태를 오갑니다.

- 너무 가까움(`< KitingRange - KitingTolerance`) → 후퇴하거나 카이팅 지점 탐색
- 너무 멈(`> KitingRange + KitingTolerance`) → 타겟으로 직진
- 적정 거리 → 좌우로 랜덤하게 흔들며 스트레이핑

| 필드 | 기본값 | 설명 |
|---|---|---|
| `Player CurrentTarget` | `null` | 추격 대상. 직접 설정 |
| `float PathUpdateFrequency` | `0.5f` | 경로/목적지 재계산 주기(초) |
| `float MoveTickFrequency` | `0.2f` | 이동 도착판정 주기(초) |
| `float KitingRange` | `7f` | 유지하려는 거리. **`3f` 미만이면 근접 모드** |
| `float KitingTolerance` | `1.5f` | 허용 오차. 이 범위 안이면 스트레이핑만 함 |
| `int KiteSampleCount` | `12` | 카이팅 후보 지점 샘플 개수 (원주 분할) |
| `float MeleeRetreatStep` | `2f` | 근접 모드 후퇴 거리 / 카이팅 실패 시 폴백 |
| `LayerMask PathfindMask` | `Default, Door, InvisibleCollider` | 시야 레이캐스트용 벽/문 레이어 |

| 메서드 | 설명 |
|---|---|
| `void StartChase()` | MEC 코루틴으로 추격 루프 시작 (중복 실행 방지됨) |
| `void StopChase()` | 코루틴 종료 |

동작 상 참고할 점:

- `KitingRange < 3f` 면 **근접 모드**로 전환되어, 카이팅 지점 탐색 없이 단순 후퇴만 합니다.
- 경로를 따라가는 구간에는 **3초 타임아웃**이 걸려 있습니다. 그 안에 못 가면 다음 주기로 넘어갑니다.
- 경로 계산에 실패하면 `AntiStuck()` 이 NPC를 위로 2미터 순간이동시킵니다(끼임 탈출용).
- `PathfindMask` 는 맵/환경에 따라 직접 맞춰야 합니다.

### `Acter` — 사격 / 조준 / 아이템

SCP:SL의 `DummyActionCollector` 액션을 이름으로 찾아 호출하는 얇은 래퍼입니다.

| 메서드 | 설명 |
|---|---|
| `TryEquipItem(ItemType, bool returnIfNull)` | 인벤토리에 있으면 장착. 없을 때 `returnIfNull: false` 면 지급 후 장착, `true` 면 아무것도 안 함 |
| `TryShoot(bool hold)` | `hold: true` → 연사(Hold), `false` → 단발(Click) |
| `TryReleaseShoot()` | 연사 중지 |
| `TryZoom()` / `TryReleaseZoom()` | 조준 시작 / 해제 |
| `TryUseItem()` / `TryCancelItemAction()` | 사용 아이템 시작 / 취소 |
| `TryInvokeAction(string action)` | 액션 이름 **suffix 매칭**으로 임의 더미 액션 호출 |

> `TryShoot(hold: true)` 는 반드시 `TryReleaseShoot()` 로 풀어줘야 합니다. 안 그러면 탄이 마를 때까지 계속 쏩니다.

### `Targeter`

현재 **빈 껍데기**입니다. 타겟 자동 선정(가시선 판정, 우선순위, 어그로 등)이 들어갈 자리로,
지금은 `Chaser.CurrentTarget` 을 직접 채워줘야 합니다.

---

## 알려진 이슈

작업을 재개할 때 먼저 확인할 것들입니다.

1. **`.csproj` 가 현재 상태와 어긋나 있음** — `API\Module\Attacker.cs`(삭제됨)를 여전히 참조하고,
   `Acter.cs` / `Targeter.cs` / `Example\Unit\Gunner.cs` 는 `<Compile>` 목록에 없습니다. 현재 빌드가 깨집니다.
2. **`Config` 클래스가 없음** — `Main.cs` 의 `Plugin<Config>` 가 참조할 타입이 존재하지 않습니다.
3. **`BaseCombatAI.Destroy()` 가 모듈을 정리하지 않음** — `modules` 딕셔너리를 순회하지 않아
   `Chaser` 의 `while(true)` 코루틴이 NPC 제거 후에도 계속 돕니다. 모듈은 `TryDestroyModule<T>()` 로
   직접 정리하거나, `Destroy()` 에서 전체 모듈을 정리하도록 고쳐야 합니다.
4. **라운드 종료 시 NPC 자동 정리 없음** — `Main.OnEndingRound` 는 NavMesh만 해제합니다.
   `CombatAIHandler.ClearCombatAI()` 호출을 추가하는 게 안전합니다.
5. **`Targeter` 미구현** — 위 참고.

---

## 라이선스

MIT © A3indae — 자세한 내용은 [LICENSE](LICENSE) 참고.
