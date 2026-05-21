# Vertical Slice 1 Playtest Build Review Response

> 작성일: 2026-05-21
> 작성자: Project Claude
> 모드: 리뷰 응답 (구현 아님)
> 리뷰 대상: `docs/handoff/vertical-slice-1-playtest-build/01-review-request.md`

## 0. 리뷰 요약

구현 계획은 큰 틀에서 정합적이며, Root 기획 문서(사건 목록, 태그 규칙, 작위 규칙, 세대 흐름, 시작/요약 템플릿, 수동 검증 가이드)와 충돌하는 부분은 없다. 그대로 진행해도 컴파일·진행이 가능한 수준이다.

다만 다음 영역에서 누락 또는 추가 합의가 필요한 지점이 발견됐다. Project Codex가 03 최종 작업 지시서를 쓰기 전에 반영 여부를 판단하면, 구현 중 멈추는 횟수를 줄일 수 있다.

가장 결정적인 보강 3개:

1. **선택지 결과의 "X 또는 Y" 분기 처리 규칙** — 태그 변화 표(`vertical-slice-1-event-tag-change-table.md`)에서 약 18개 행이 "둘 중 하나"인데, 모델/플로우에 분기 결정 방식이 없다.
2. **일반 사건이 일으키는 조기 세대 종결 처리** — HON-01/HON-07/EST-07 같은 일반 사건의 사망/퇴장 가능성이 데이터 모델에 슬롯이 없다.
3. **deterministic smoke의 멸문 발화 보장** — 3개 seed가 우연히 모두 자연 세대 전환이면 멸문 경로가 검증되지 않는다. 현재 MVP smoke의 강제 분기 방식을 VS1에도 유지하는 게 안전하다.

세부 발견은 아래 섹션별로 정리한다. 각 항목 끝에 `[권고]` 또는 `[Root 확인]` 분류를 붙였다.

---

## 1. Vertical Slice 1 데이터 구조 검토

### 1.1 기본 방향 — C# 정적 데이터로 시작

[유지] 외부 JSON/CSV 도입은 VS1 검증 목적(콘텐츠 로더 검증이 아닌 반복 플레이 재미 검증)과 무관하고, 비용만 늘린다. C# 정적 데이터로 시작하는 결정은 적절하다.

### 1.2 모델 구조 — 적절한 영역

- `Vs1ImpactDirection` × `Vs1ImpactMagnitude` 분리는 좋다. 밸런스 수치를 정하지 않은 상태로도 사건 데이터를 채울 수 있다.
- `NobleTitleRank`, `TitleRiskStage`, `Vs1EventCategory`, `Vs1EventRole`, `LegacyTagDuration` enum 구성은 Root 기획문서와 1:1로 일치한다.
- `HeldTitles` 목록 + `RepresentativeTitle` 계산 + `TitleRiskStage` 분리는 `vertical-slice-1-title-progression-rules.md` §대표작위 기준을 만족한다.

### 1.3 데이터 모델 누락/위험

**[권고 A]** `Vs1EventDefinition.IsMajor`만으로는 부족하다. `vertical-slice-1-event-chronicle-draft.md`는 두 개의 직교 축을 둔다:

- **큰/보조 사건** (28 vs 18): 작위/태그/후계/멸문에 직접 영향을 주는가
- **세대 요약 반영** (높음/중간/낮음): 세대 요약 화면 우선순위

`IsMajor` bool 1개만 두면 세대 요약 우선순위 정보가 누락된다. `enum Vs1EventImportance { Minor, Major }` + `enum Vs1SummaryPriority { Low, Medium, High }` 두 필드로 분리 권고.

**[권고 B]** 선택지별 chronicle 변형 슬롯이 필요하다. `vertical-slice-1-event-chronicle-draft.md` §선택지별 연대기 변형 기준은 "사건별 대표 연대기 문장 1개 + 결과 문장 조합"으로 가되, 다음 5케이스에서는 같은 사건 안에서도 결과가 갈라져야 한다고 명시한다:

- 작위 상승 성공/보류
- 대표작위 상실/방어 성공
- 후계 안정/후계 불안
- 멸문/다음 세대 연결
- 강한 가문사 태그(`전쟁 공훈`/`불명예`/`몰락의 징조`) 변화

현재 제안된 `Vs1EventChoice`는 `ResultText`만 있고 chronicle 오버라이드가 없다. `string? ChronicleLineOverride { get; init; }` 슬롯을 추가하지 않으면, 위 5케이스에서 대표 문장만 반복되어 가문사 정체성이 흐려진다. 대부분 선택지는 `null`로 두고, 위 5케이스에 해당하는 선택지(주로 PRO-*, CRI-*, END-* 선택지의 절반)에만 채운다.

**[권고 C]** `Vs1EventCondition`의 단일 `string Key`는 표현력이 좁다. 작위 조건은 "최소/최대/정확", 태그 조건은 "포함/미포함", 상태 조건은 "낮음 이상/높음 이상"이 모두 있다. 단일 string에 `"Baron-Count"` 같은 인코딩을 넣을 수도 있지만, 구체 필드로 두는 게 안전하다:

```csharp
public sealed class Vs1EventCondition
{
    public Vs1ConditionKind Kind { get; init; }
    public NobleTitleRank? MinTitle { get; init; }
    public NobleTitleRank? MaxTitle { get; init; }
    public Vs1StateAxis? StateAxis { get; init; }
    public Vs1StateBand? StateBandAtLeast { get; init; }
    public string? RequiredTagKey { get; init; }
    public string? ForbiddenTagKey { get; init; }
    public string? HeirTraitKey { get; init; }
    public int? MinGeneration { get; init; }
    public string DisplayReason { get; init; }
}
```

이 형태가 사람이 데이터를 채우기 쉽고, 후보 선정 로직의 분기를 명시화한다.

**[권고 D]** 사건별 가중치 슬롯이 필요하다. `vertical-slice-1-generation-event-flow.md`는 "조건이 맞는 사건을 우선 고르고, 부족하면 일반 조건 사건으로 채운다", "후계자 특성이 사건 후보 가중치를 변경한다"고 명시한다. 그러나 `Vs1EventDefinition`에 `BaseWeight`나 `WeightModifiers`가 없다. 임시값으로라도 다음 정도는 데이터에 둬야 후보 선정이 deterministic하게 재현된다:

```csharp
public sealed class Vs1EventDefinition
{
    // ...
    public int BaseWeight { get; init; } = 1;                          // 1~3
    public IReadOnlyList<Vs1WeightModifier> WeightModifiers { get; init; } = Array.Empty<Vs1WeightModifier>();
}

public sealed class Vs1WeightModifier
{
    public Vs1ConditionKind TriggerKind { get; init; }   // 예: ActiveTag, HeirTrait, TitleAtLeast
    public string TriggerKey { get; init; }
    public int Delta { get; init; }                       // +1, +2
}
```

[Root 확인 필요] 가중치 수치 자체는 임시값이라도 Root가 "이 정도 차이가 나면 후보 우선도 변화가 체감된다"는 기준이 있어야 한다. 아니면 Project Codex가 임시값을 정하고 보고서에서 표시하는 것이 최소한.

**[권고 E]** `Vs1LegacyTagChangeKind`에 `Strengthen`(약화의 반대) 슬롯이 없다. legacy tag rules는 "약화될 수 있으나 가문사에는 남음"(`잊히지 않는 패배` 등) 같은 등급 변동을 시사한다. VS1 범위에서 등급 동적 변동이 명시 요구되지 않으므로 지금은 보류 가능하지만, 향후 확장을 위해 enum slot만 미리 두는 것을 권고. 또는 Root 확인 후 명시적으로 "VS1에서 등급은 정의값으로 고정"이라고 정리.

**[권고 F]** `Vs1ActiveLegacyTag.Duration`이 단일 등급 필드인데, 등급별 만료 처리 시점이 게임 루프에 명시 안 됨. 세대 시작 시점 vs 세대 종료 시점 중 하나를 골라야 한다. 권고는:

- **세대 종료 직후**(BuildNextGenerationRun 호출 시): 단기 태그는 소멸, 세대 태그는 등급 하락(→ 단기), 가문사 태그는 유지.
- 다음 세대 시작 시점에는 만료 결과가 이미 반영된 상태로 노출.

이 규칙을 `Vs1Flow.AgeLegacyTags(family)` 명시 함수로 두면 검증이 쉽다.

**[권고 G]** `FamilyRunState` 확장 vs VS1 전용 상태 — 리뷰 시점 결정 권고.

리뷰 요청 §2.1은 "리뷰 후 실제 변경 방식 결정이 필요하다"고 둔다. 코드를 확인한 결과:

- `FamilyRunState`는 `MultigenContent`/`MultigenFlow`와 강하게 묶여 있다. `FamilyStats`는 `Dictionary<string,int>` 형태로 상태 키가 외부에서 주입되므로 4축 확장은 호환 유지가 가능하다.
- 그러나 `CurrentCharacterTraits`(MVP `TraitStub`)와 VS1 `Vs1HeirProfile`은 표현 단위가 다르다. trait list만으로는 성향/강점/약점/승계 사유를 분리 표현할 수 없다.
- `History`도 `GenerationRecord`(자유 entry list)와 VS1 `Vs1GenerationSummary`(작위/태그/요약 단락)가 다르다.

권고: **새 `Vs1FamilyState` 클래스를 도입하고, `FamilyRunState`는 보존만 한다.** Main.cs는 VS1 모드 분기에서 `Vs1FamilyState`만 들고 다닌다. MVP 흐름은 `MvpLoopContent`/`MultigenFlow`와 함께 dead code로 남되, 합류 계약상 보존(MVP 자리표시자를 기준점으로 둠).

대안 (확장): `FamilyRunState`에 `Vs1Extension` 컴포지션 필드를 단다. 그러나 이 경우 두 표현이 공존하며 어떤 흐름이 누구를 갱신하는지 모호해진다. 권고하지 않는다.

[Root 확인 필요 — 또는 Project Codex 판단] MVP 코드를 "보존만 하고 활성화하지 않는" 방향이 합류 계약과 맞는지. `MvpLoopContent.cs는 보존한다`는 명시는 있지만, Main.cs `_Ready()`가 VS1 모드만 진입하면 MVP 자리표시자 기준이 코드 안에서는 더 이상 비교 대상이 안 된다.

---

## 2. 46개 사건 이식 방식 검토

### 2.1 이식 자체

[유지] Root 문서의 ID/제목/본문/선택지 행동명/영향 요약/대표 연대기 문장을 그대로 옮기는 결정은 적절하다. 새 사건 추가나 본문 재작성을 명시적으로 금지한 것도 일치한다.

### 2.2 파일 분할

**[권고 H]** 46개 × 평균 2.4 선택지 = 약 110개 선택지의 정적 C# 초기화 코드가 `VerticalSliceContent.cs` 한 파일에 들어가면 1500~2000줄이 된다. 컴파일/유지는 가능하지만, 사건 본문 수정 시 diff 면적이 커지고 PR review가 힘들다.

권고: `VerticalSliceContent`를 `partial class`로 두고 분류별로 7개 파일로 나눈다.

- `scripts/Vs1Content.cs` — public API + 사건 전체 합치기
- `scripts/Vs1Content.Estate.cs` — EST-01~08
- `scripts/Vs1Content.Honor.cs` — HON-01~08
- `scripts/Vs1Content.Court.cs` — CRT-01~08
- `scripts/Vs1Content.Family.cs` — FAM-01~08
- `scripts/Vs1Content.Promotion.cs` — PRO-01~05
- `scripts/Vs1Content.Crisis.cs` — CRI-01~05
- `scripts/Vs1Content.GenerationEnd.cs` — END-01~04

[대안] 한 파일로 두고 사건 사이를 `#region`으로 분리. 한 파일이라 grep/네비게이션은 편하지만 diff 면적은 그대로다. Project Codex 판단에 맡긴다.

### 2.3 선택지 결과의 "X 또는 Y" 분기 처리 — 가장 결정적 누락

**[권고 I + Root 확인 필요]** `vertical-slice-1-event-tag-change-table.md`에는 다음 선택지가 "X 또는 Y" 분기로 표시돼 있다. 셈해 보면 약 18개 케이스:

| 사건/선택 | 분기 |
|---|---|
| EST-01 곡창 | 없음 또는 `가문의 맹세` |
| EST-02 거절 | 없음 또는 `가문의 맹세` |
| EST-06 수용 | 없음 또는 `가문의 맹세` |
| EST-07 재개 | `풍요로운 영지` 또는 `피의 원한` |
| HON-01 직접 | `전쟁 공훈` 또는 `잊히지 않는 패배` |
| HON-02 결투 허락 | `기사의 명예` 또는 `피의 원한` |
| HON-03 수용 | `충성스러운 친족` 또는 `전쟁 공훈` |
| HON-04 증거 수집 | 없음 또는 `승격 명분` |
| HON-06 몸값 | 없음 또는 `불명예` |
| HON-08 명령 따름 | `불명예` (+ `왕의 총애` 추가 가능) |
| CRT-03 서약 | `정적의 표적` 또는 `왕의 총애` |
| CRT-04 화려 | 없음 또는 `왕의 총애` |
| CRT-05 협상 | `불명예` 또는 `궁정의 의심` |
| CRT-08 처벌 | `겁쟁이의 소문` 또는 `불명예` |
| FAM-04 중재 | 없음 또는 `충성스러운 친족` |
| FAM-04 숙청 | `피의 원한` 또는 `내부 균열` |
| FAM-06 군무 | `전쟁 공훈` 또는 `충성스러운 친족` |
| CRI-01 출두 | `궁정의 의심` 해소 또는 `박탈 위기` |
| CRI-04 진압 | `피의 원한` 또는 `불명예` |
| END-02 돌격 | `전쟁 공훈` 또는 `잊히지 않는 패배` |
| END-02 후방 | `겁쟁이의 소문` 또는 없음 |
| PRO-01 청원 | `승격 명분` 또는 `불안한 승격` |
| PRO-02 영지 요구 | `정적의 표적` 또는 `불안한 승격` |
| PRO-04 계승권 | `정적의 표적`, `불안한 승격` (한꺼번에) |
| PRO-05 작위 | `불안한 승격` 또는 `승격 명분` |

처리 방식이 정해지지 않으면 다음 중 하나를 임의로 골라야 하는데, 어느 방향이든 반복 플레이 다양성에 영향을 준다:

1. **단순 무작위(50/50)** — 가장 단순. seed에 따라 결과가 갈린다.
2. **상태/태그 조건부 결정** — 예: HON-01 직접 출전 → 명예가 이미 높으면 `전쟁 공훈`, 낮으면 `잊히지 않는 패배`. 가장 서사적이지만 사건별 분기 규칙을 Root가 채워야 한다.
3. **후계자 특성에 가중치** — 명예 중시 성향 → `전쟁 공훈` 우세. 후계자 약하게 반영 원칙과 일관.
4. **둘 다 추가하고 약화는 다음 큰 사건이 결정** — 가장 복잡. VS1 범위 초과.

권고는 **(2)와 (3)의 조합**: 사건 데이터에 `IReadOnlyList<Vs1ConditionalOutcome>`을 두고, 각 결과에 작은 조건식(예: "명예 보통 이상") + 기본값을 둔다. 단, 사건별 분기 조건은 Root가 채워야 의미 있는 가문사가 나온다.

최소 안전 fallback: VS1 구현에서는 일단 50/50 무작위로 진행하고, 보고서에서 "분기 사건 N개를 무작위로 처리했다, Root의 조건 규칙 후 재진행 필요"라고 명시. [Root 확인 필요]

### 2.4 일반 사건의 조기 종결 처리

**[권고 J + Root 확인 필요]** 일부 일반 사건은 본문/영향 요약에서 "사망/부상 위험"을 명시한다:

- HON-01 직접 출전: "부상이나 세대 종결 위험"
- HON-07 위험한 출전: "큰 위험"
- EST-07 광산 재개: "사고와 `피의 원한` 위험"
- HON-02 결투 허락: "피가 흐른다"
- CRI-04 무력 진압: 위험 함의

이 사건이 세대 종결 사건 슬롯(END-*) 앞 단계에서 발화하면, 다음 두 시나리오가 충돌한다:

- 일반 사건이 사망을 일으키면 → END-* 슬롯이 사용 안 됨 → 세대 요약 화면이 어떤 종결 유형을 표시하는가?
- END-* 종결 사건이 마지막 슬롯으로 강제되면 → 일반 사건의 "사망 위험" 본문은 fluff가 되어 플레이어 신뢰가 떨어진다

권고는 다음 단순화 중 하나:

**옵션 1 (단순):** VS1에서는 END-* 종결 사건만 실제 종결을 만든다. 일반 사건의 "위험" 본문은 상태 영향(명예/결속 큰 하락)으로 변환되고, 그 결과 태그가 END-* 종결 사건의 선택지 우선도를 바꾼다(예: HON-07 위험 출전 결과 → `잊히지 않는 패배` → END-02 후방 지휘 시 `겁쟁이의 소문` 가능성 상승). **권고: 옵션 1.**

**옵션 2 (서사):** 일반 사건에 `EndTypeHint`를 추가하고, 선택지가 `EndTypeHint`를 활성화하면 해당 세대를 즉시 END-* 종결 슬롯으로 점프. 다만 어떤 END-*로 점프할지 매핑이 필요하다.

옵션 1을 택하면 데이터 모델의 `EndTypeHint? EndTypeHint`는 END-* 사건 전용으로만 쓰이고, 일반 사건에서는 항상 null. 옵션 2를 택하면 일반 사건의 데이터 무게가 늘어난다. [Root 확인 필요]

### 2.5 사건 풀 후보 부족 fallback

**[권고 K]** 1세대 시작 시점은 보통 남작이다. 남작 단계에서 조건이 맞는 사건 풀:

- EST-01~08 (8개, 대부분 작위 조건 없음)
- HON-01 (남작 이상), HON-02 (조건 약함), HON-03/05/07 (조건 약함)
- CRT-01/04 (자작 이상 필요), CRT-05~08 (대부분 태그 조건이 필요)
- FAM-01~08 (대부분 조건 약함)
- PRO-01 (남작, 재산/명예 높음)

즉 1세대 1턴에서 사용 가능한 후보 사건은 약 20~25개. 한 세대 5~7개 사건을 뽑아도 충분하다.

그러나 2세대 이후에 작위가 백작 이상으로 올라가고 활성 태그가 많아지면, 후보 사건 풀이 좁아질 수 있다(예: 공작 + 박탈 위기 + 불안한 후계 동시 활성 시, 모든 조건을 만족하는 사건 풀이 부족할 수 있음).

후보 부족 시 fallback 규칙을 명시하지 않으면 무한 루프 또는 사건 풀 비어버림이 생긴다. 권고:

1. 조건 완전 충족 → 우선 후보
2. 조건 절반 충족 → 차순위 후보
3. 둘 다 부족하면 카테고리 균형(EST/HON/CRT/FAM 중 가장 적게 등장한 분류) 기준 무작위
4. 어떤 경우에도 END-* 종결 슬롯은 반드시 채운다

이 규칙을 `Vs1Flow.SelectEventCandidates(...)` 안의 fallback 분기로 명시.

### 2.6 작위 조건 표시 약속

[유지] 사건 화면에 "관련 작위" "관련 유산" 표시는 `Vs1EventCondition.DisplayReason`으로 처리. 정확하다.

다만 한 사건이 여러 조건을 가질 때 "1~3개만 노출" 원칙(`vertical-slice-1-generation-event-flow.md`)이 데이터 모델에서는 명시 안 됨. 권고: `Vs1EventCondition`에 `bool ShouldDisplay { get; init; }` (default true) + flow에서 상위 3개만 표시. 또는 데이터 작성 시점에 우선 3개만 `ShouldDisplay = true`로 표시.

---

## 3. 게임 루프 검토

### 3.1 전체 흐름

[유지] 9단계 흐름은 현재 MVP 흐름과 일관되며 Root 문서와 충돌이 없다.

### 3.2 사건 후보 선정 — 누락 보강

**[권고 L]** RNG seed 인터페이스가 명시 안 됨. `BuildEventPlanForGeneration(family, rng)`라고 표기됐지만, `rng`가 어디에서 오는지(코드 내부 정적, 환경변수, smoke 인자) 안 정해졌다. 권고:

- VS1 `FamilyRunState`(또는 새 `Vs1FamilyState`)에 `int Seed` 필드를 두고, `System.Random`을 거기서 파생.
- `--smoke` 인자로 `--seed=12345` 형태를 받고, 없으면 코드 상수 3개(예: 1, 42, 12345)를 순차 실행.
- 정상 모드(`_Ready` GUI)에서는 `Environment.TickCount`로 자동 부여.

이 시드 표기는 [Root 확인 7번 — 실행별 seed 표시 UI]와도 연결된다. 권고: 헤더에 작게 `(seed N)` 표시 + 가문사 종료 화면에 명시. 작은 화면에서 헤더 길이가 부담되면 `상태` 탭 하단으로 빼는 방법도 가능.

**[권고 M]** 후보 가중치 적용 순서가 명시 안 됨. 권고:

1. 조건 완전 충족 사건 집합 만들기
2. 같은 세대 동일 사건 제외, 같은 분류 3회 연속 제외, 한 실행 동일 큰 사건 제외
3. 각 사건의 `BaseWeight + ∑ WeightModifiers`로 가중 합산
4. 슬롯 종류(기반/압력/전환)별 카테고리 필터링 후 가중치 무작위 선택
5. 종결 슬롯은 END-* 분류에서만 선택

이 순서를 `Vs1Flow.BuildGenerationPlan(...)` 안에 명시.

### 3.3 결과 적용 순서 — 후계자 영향 시점

**[권고 N + Root 확인 필요]** 본문 §3.5 결과 적용 4번 "후계자 안정 또는 후계 후보 상태 영향 적용"의 의미가 모호하다. 두 해석이 가능:

- **현재 인물의 후계자 풀**에 영향 (FAM-03 사생아 → 새 후보 추가, FAM-05 병약 → 기존 후보 약화)
- **다음 세대 가주 프로필**에 영향 (FAM-01 교육 → 다음 세대 성향/강점)

코드 흐름상 둘 다 필요하다. 권고는 데이터 모델 분리:

```csharp
public sealed class Vs1HeirEffect
{
    public Vs1HeirCandidatePoolChange? PoolChange { get; init; }   // 사생아 추가 등
    public Vs1NextHeirProfilePresetKey? PresetForNextGen { get; init; } // 교육 결과
    public Vs1HeirStabilityChange? StabilityChange { get; init; } // 안정/불안 변화
}
```

`Vs1HeirProfile`(현 가주용)과 `Vs1HeirCandidate`(다음 가주 후보 풀)는 별개 모델로 두는 게 명확하다. [Root 확인 필요 — VS1에서 후계 후보 풀이 동적으로 변하는가, 아니면 세대 종료 시점에 1회만 생성하는가]

### 3.4 상태 임시 점수의 4축 이월 규칙

**[권고 O]** 본문 §3.7 "상태는 내부 점수를 일부 이월하되, UI에는 질적 구간만 표시한다"는 명시했지만, 4축 각각의 이월 규칙은 안 정해졌다. 현재 MVP `MultigenFlow.ApplyPlaceholderStateCarry`는 Resource/Reputation/Relation 3거동 매핑이고, VS1 4축(Wealth/Honor/CourtInfluence/HouseUnity)의 거동이 다르다.

권고 임시 매핑:

- Wealth → 누적 (자원형, 변경 없음)
- Honor → 감쇠 (절댓값 1씩 0 방향)
- CourtInfluence → 감쇠 (절댓값 1씩 0 방향)
- HouseUnity → 재구성 (인물 교체 시 절반화)

이 매핑은 임시값이며 Root의 후속 라운드에서 조정한다는 주석을 코드에 둔다.

### 3.5 유산 태그 만료 처리

[권고 F와 동일] 단기/세대/가문사 태그의 만료 처리 시점은 게임 루프의 명시 단계가 필요. 권고: 세대 종료 직후(다음 세대 RunState 생성 직전)에 `Vs1Flow.AgeLegacyTags(family)`가 실행된다.

### 3.6 멸문 판정

[유지] "후계 부재 + 사망/퇴장 결합"만 멸문 처리. 작위 상실은 멸문이 아니라 쇠락. 정확하다.

다만 `vertical-slice-1-title-progression-rules.md` §작위 상실 방식의 "폐위/추방"은 "현 세대 인물이 퇴장한다"고 정의한다. 즉 END-04 강제 폐위 선택지 "도주해 방계에 맡긴다"는 사망/퇴장의 한 종류이지만, 후계가 방계로 이어지면 멸문이 아니다. 모델에서 이걸 어떻게 표현하는가?

권고: `GenerationEndType`을 다음 6종으로 확장 (현재 MVP 5종에 `ForcedAbdication` 추가):

```csharp
public enum Vs1GenerationEndType
{
    NaturalDeath,
    BattleDeath,
    IllnessDeath,
    ForcedAbdication,   // END-04
    Extinction,
    Other,
}
```

`Extinction`은 세대 종결 사건이 아니라 후계 부재 + 사망/퇴장 조건이 결합됐을 때 발화하는 별도 상태. END-04 선택 결과 + 후계 부재가 결합되면 `Extinction`. END-04 선택 결과 + 방계 후계 있음이면 `ForcedAbdication`.

---

## 4. 반응형 탭 UI 검토

### 4.1 4영역 shell + 3탭 유지

[유지] 결정 012의 원칙과 일치. 새 "유산" 탭 추가 거부도 정확. 작은 화면 영향 최소.

### 4.2 ActionBar 3개 선택지 검증

**[권고 P]** `vertical-slice-1-event-list.md`와 `event-copy-draft.md`를 확인하면 3개 선택지 사건은 다음 4개:

- FAM-01 후계자의 교육 (3개)
- FAM-08 후계자 간 경쟁 (3개)
- CRI-03 정적의 탄핵 (3개)
- PRO-05 왕의 특별 포상 (3개)

3개 선택지를 ActionBar 세로 버튼 목록으로 두는 결정은 맞다. 다만 작은 창(360x640)에서:

- 헤더 2행 (각 ~24px = 48px) + HSeparator (2px) = 50px
- MainScroll 사건 본문 + 단서 (최소 200~250px 필요)
- HSeparator (2px)
- InfoTabs `CustomMinimumSize.y = 180`
- HSeparator (2px)
- ActionBar 3버튼 × ~36px = 108px (영향 요약 1줄 추가 시 ~180px)

합계 ≈ 540~600px. 360x640 화면에서는 좌우 여백 + 안전 마진 12px씩 빼도 가까스로 들어간다. 단 영향 요약을 버튼 아래 한 줄로 두면 3선택지 사건은 ActionBar가 화면의 30% 가까이 점유한다.

권고 보강:

1. 3선택지 사건에서는 영향 요약 라벨 폰트를 한 단계 작게.
2. 선택지 본문 길이 제한: `Vs1EventChoice.ImpactPreview`는 한 줄(약 24자 내) 강제.
3. InfoTabs `CustomMinimumSize.y`를 360x640 환경에서는 동적으로 줄이는 것 검토(예: 화면 높이의 25%). 다만 이건 작은 창 진입 시 reflow 비용이 있다.

### 4.3 사건 화면의 단서 표시 위치

**[권고 Q]** 본문 §4.5는 사건 화면 정보 우선순위 6항목을 두고, 4번째 "관련 상태/작위 위험/유산 태그/후계자 단서"가 어디에 표시되는지 명시 안 됨. MainScroll 안 사건 본문 아래 또는 InfoTabs 영역의 관련 탭 강조 중 어느 쪽인가?

권고는 MainScroll 안 사건 본문 아래에 짧은 chip/badge 행 1줄로 두는 것. 길어지면 줄바꿈 1회까지 허용. InfoTabs 안으로 보내면 사용자가 탭 전환을 해야 단서를 본다 — 사건 화면의 의미가 약해진다.

### 4.4 작위 위험 단계 표시

**[권고 R]** `vertical-slice-1-title-progression-rules.md` §UI/UX 표시 기준은 "작위 위험 단계가 위기 이상일 때 헤더에 경고 1개"를 두고, 안정/견제 상태는 헤더에 안 보이게 한다. 헤더 2행에 단계 라벨 + 위험 경고를 함께 두는 본문 §4.2와 일관.

다만 "안정/견제" 상태에서 위험 정보를 어디서 보는가? 권고: `상태` 탭의 4축 표시 아래 작위 위험 단계를 항상 표시. 헤더에서는 위기/박탈만 경고.

### 4.5 결과 화면의 태그 변화 표시

[유지] "해당 선택으로 변한 태그만 보여준다. 전체 태그 목록은 가문사 탭에 둔다." 정확.

다만 `vertical-slice-1-event-tag-change-table.md` §UI/UX 노출 기준은 5단계 우선순위(가문사 태그 추가 → 세대 태그 추가 → 상반 교체 → 부정 해소 → 단기 추가)를 둔다. 본문은 이 우선순위를 명시 안 함. 권고: `Vs1Flow.SortTagChangesForDisplay(...)` 명시 함수에 이 순서를 채운다.

### 4.6 세대 시작 화면 정보 우선순위

[유지] §4.4의 7개 항목 순서는 `vertical-slice-1-generation-start-and-tag-display.md`와 일관.

다만 `vertical-slice-1-generation-start-and-tag-display.md` §여러 태그가 있을 때 문장 구성은 "긍정/부정 동시 시 부정 먼저"를 명시한다. 본문은 이 규칙을 안 옮겼다. 권고: `Vs1Content.BuildGenerationStartLine(tags)` 안에 부정 태그 우선 정렬 + 상반 시 최신 우선 규칙 명시.

---

## 5. 스모크/수동 검증 계획 검토

### 5.1 deterministic seed 3회 실행

[유지] 큰 틀에서 합리적.

### 5.2 검증 보장 — 시드 의존성 위험

**[권고 S]** 본문 §5.1 권장 범위는 "적어도 한 실행에서 승격 기회 사건이 등장한다" "적어도 한 실행에서 작위 위험 또는 대표작위 상실 경로가 등장한다" "적어도 한 실행에서 유산 태그가 다음 세대 사건 등장 이유로 출력된다" "멸문은 후계 부재 + 사망/퇴장 결합일 때만 출력된다" 4개의 ASSERT를 둔다.

3개 seed가 우연히 모두 안정 가문사라면 위 ASSERT가 깨질 수 있다. 특히 멸문은 `vertical-slice-1-generation-event-flow.md` §한 실행의 기본 길이가 "보통 4~8세대"라고 했으니 자연 발화 확률이 낮다.

권고 — 다음 중 하나로 강제 보장:

**옵션 1 (강제 분기):** 4번째 smoke seed에서 "멸문 강제 모드"를 켠다. 즉 후계 부재 + 사망/퇴장 분기를 코드에서 직접 발화. 현재 MVP smoke가 이미 "2세대에서 멸문 강제"를 쓰니 동일 패턴 유지.

**옵션 2 (조건부 시드):** 시드 3개를 미리 골라 둔다 — 1번은 승격 경로, 2번은 작위 상실 경로, 3번은 멸문 경로가 나오는 것이 확인된 시드. 시드 자체가 데이터처럼 hard-code되며, 콘텐츠 변경 시 재선정 필요. **유지 비용 큼, 권고 안 함.**

**옵션 3 (대안):** smoke에 `--smoke-extinction` 인자를 추가해 멸문 시나리오를 명시적으로 실행. 일반 smoke 3회는 자유, 강제 멸문 1회는 별도.

권고: **옵션 1.** 현재 MVP smoke 패턴을 VS1에서도 이어 받아 "RUN 1: 자연 진행, RUN 2: 작위 상실 강제, RUN 3: 멸문 강제" 분기 구조로.

### 5.3 ASSERT 출력 방식

**[권고 T]** 본문 §5.1은 마지막 줄을 `SMOKE_OK`로 유지하라고 명시. 그러나 ASSERT가 실패해도 `SMOKE_OK`만 나오면 자동 검증이 가치가 없다.

권고: 각 ASSERT 결과를 `[SMOKE] ASSERT key=value`로 출력하고, 모든 ASSERT가 통과하면 `SMOKE_OK`, 하나라도 실패하면 `SMOKE_FAIL`을 출력. 종료 코드는 통일해 0으로 두되, stdout grep으로 검증.

```text
[SMOKE] ASSERT promotion_seen=true
[SMOKE] ASSERT title_loss_seen=true
[SMOKE] ASSERT legacy_tag_event_reason_seen=true
[SMOKE] ASSERT extinction_after_no_heir_only=true
SMOKE_OK
```

또는 ASSERT 실패 시 `SMOKE_FAIL` + 실패 ASSERT 목록.

### 5.4 stdout 양

**[권고 U]** 3회 실행 × 평균 6세대 × 평균 6사건 × 사건당 약 7~10줄 + 세대 시작/종료 × 약 5줄 = 약 900~1200줄.

Bash tool은 30000자 캡이 있어 절대 양은 문제 안 됨. 다만 사람이 grep으로 확인할 때 anchor가 필요하다.

권고: 회당 한 줄 요약 anchor를 추가.

```text
[SMOKE] RUN_END run=1 seed=1 generations=5 end_type=NaturalDeath promotions=2 title_losses=0
[SMOKE] RUN_END run=2 seed=42 generations=3 end_type=ForcedAbdication promotions=1 title_losses=1
[SMOKE] RUN_END run=3 seed=12345 generations=4 end_type=Extinction promotions=0 title_losses=0
```

### 5.5 수동 검증 결과 보고 경로

**[권고 V]** 본문 §5.2는 `vertical-slice-1-manual-playtest-guide.md` 양식으로 기록하라고만 명시. 그러나 누가, 어디에 기록하는지가 모호하다.

권고: `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`(또는 비슷한 경로) 같은 결과 문서 경로를 03 최종 작업 지시서에 사전 명시. 작성 주체는 "사용자가 직접" 또는 "Root Codex가 사용자 피드백을 받아 채움"으로 분리.

### 5.6 검증 명령 환경 의존성

[유지] `dotnet build` → `--headless --import` → `--headless -- --smoke` 순서는 정확. CLAUDE.md §검증 반복 비용에 맞다.

다만 C# 변경만 있는 경우 `--import` 생략 가능 + `--smoke`로 빠르게 검증한다는 가이드(CLAUDE.md §검증 반복 비용)를 03 작업 지시서에서도 명시 권고.

---

## 6. 범위 / 병렬 작업 분해 검토

### 6.1 수정 범위

[유지] 수정 가능/금지 목록은 명확하다.

**[권고 W]** `MultigenContent.cs`, `MultigenFlow.cs`, `MultigenModels.cs`는 수정 가능 목록에 있지만, 권고 G에서 제시한 "새 `Vs1FamilyState`로 분리" 방향을 따르면 이 3파일은 실질적으로 손대지 않게 된다. 03 작업 지시서에서 "수정은 가능하나 가급적 손대지 않음 — VS1 코드는 별도 클래스 트리"로 명시.

다만 `Main.cs`는 반드시 큰 변경이 필요. VS1 모드 분기 + 새 화면 메서드들이 들어간다. 현재 800줄 → 1500줄로 늘어날 수 있다.

**[권고 X]** `Main.cs` 분리 권고. 화면 빌더 로직(ShowGenerationStart, ShowEvent, ShowResult, ShowGenerationSummary, ShowSuccession, ShowExtinction의 VS1 버전들)을 `scripts/Vs1ScreenBuilder.cs` 같은 partial class 또는 static helper로 빼면 Main.cs는 shell + 분기 + 4영역 갱신 진입점만 보관. 합류 계약(현재 Main.cs 첫 주석)을 깨지 않으면서 가능.

다만 partial class를 쓰면 Godot SourceGenerator의 `[ScriptPath]` 매핑이 헷갈릴 수 있다 — Main.cs 본체는 `Scenes/Main.tscn`에 어태치된 상태로 유지하고, Vs1ScreenBuilder.cs는 어태치되지 않은 별도 static utility 클래스로 두는 게 안전. (CLAUDE.md §`.tscn`의 `ext_resource path`는 디스크 케이스와 정확히 일치해야 한다 — 이 함정을 피하려면 새 partial class는 만들지 않는 것이 좋다.)

### 6.2 병렬 작업 분해

[유지] A(모델/플로우) → B(콘텐츠) + C(UI) → D(스모크/보고) 분해는 합리적. A의 모델 필드 명을 먼저 고정해야 B/C가 안정.

**[권고 Y]** 보강:

- A의 첫 PR은 모델 클래스 + enum만, 메서드 본문은 throw new NotImplementedException. B/C가 컴파일 통과 가능한 형태로 일찍 부분 머지.
- B와 C는 A 머지 후 병렬. 단 B(46개 사건 이식)는 분기 결정 규칙(권고 I) + 작위 위험 매핑(권고 R) 등 Root 확인 항목이 결정된 후에야 가치 있는 데이터가 나온다. **Root 확인 항목이 미정인 상태에서 B를 시작하면, 분기 사건의 데이터를 "일단 무작위/일단 첫 결과 고정"으로 채워야 하고, Root 확정 후 다시 채워야 한다.** Project Codex는 Root 확인 항목을 먼저 받고 B를 시작하길 권고.

### 6.3 합류 계약

**[권고 Z]** Main.cs와 Vs1Flow 사이의 화면 전환 계약은 현재 Main.cs 주석에 명시된 4영역 흐름(RefreshContextHeader → ClearMainContent → 본문 구성 → SetActionBar → RenderInfoTabs → ResetMainScroll)을 그대로 유지. VS1에서도 이 순서를 깨지 않도록 03 작업 지시서에 명시.

---

## 7. Root 확인 요청 보강

본문 §7의 7개 질문에 더해, 본 리뷰에서 추가 발견한 Root 확인 항목:

8. **선택지 결과의 "X 또는 Y" 분기 처리 규칙** — 무작위/조건부/후계자 가중치 중 어느 방향? (권고 I — 가장 결정적)
9. **일반 사건의 조기 종결 처리** — 옵션 1(END-*만 종결)/옵션 2(EndTypeHint 점프) 중 어느 방향? (권고 J)
10. **후계 후보 풀의 동적 변경 가능 여부** — FAM-03 사생아 사건이 후계 풀에 후보를 추가하는가? (권고 N)
11. **상태 4축의 세대 이월 거동 매핑** — 4축 각각 누적/감쇠/재구성 중 무엇인가? (권고 O — 임시값으로 두고 Root가 후속 라운드에서 조정)
12. **유산 태그 등급별 만료 시점** — 세대 종료 직후로 통일해도 되는가? (권고 F)
13. **VS1 활성화 후 MVP 흐름 보존의 의미** — `MvpLoopContent`/`MultigenFlow`를 dead code로 두는 게 합류 계약상 충분한가? (권고 G)
14. **deterministic smoke의 멸문 시나리오 강제 분기** — 현재 MVP smoke의 강제 분기 방식을 VS1에서도 유지해도 되는가? (권고 S — 옵션 1)
15. **smoke의 ASSERT 출력과 SMOKE_OK/SMOKE_FAIL 분리** — 일관된 검증 anchor 형식을 03 지시서에 고정해도 되는가? (권고 T)
16. **수동 플레이테스트 결과 보고 경로** — `06-manual-playtest-results.md`(또는 다른 경로) 사전 약속이 필요한가? (권고 V)

본문 §7의 기존 질문에 대한 Project Claude 의견:

1. 내부 임시 점수 + 질적 표시 — **권고: 그대로 진행.** 결정 010과 일관.
2. 상세 토글에 내부 숫자 — **권고: 임시 내부값 표시 가능.** 다만 토글이 닫혀 있는 게 기본. 수동 검증자가 필요할 때만 열게.
3. 가문명/인물명 자리표시자 — **권고: 자리표시자 유지.** 현재 MVP가 자리표시자며, 이름 풀 도입은 VS1 범위 초과. Root가 이름 풀을 제공하면 그때 갱신.
4. 5~7개 사건 deterministic smoke 축약 — **권고: 축약하지 않음.** 검증 가치가 사라진다. 사이클이 길어지는 비용이 작다(5~10초/회).
5. seed 표시 UI — **권고: 헤더 2행 끝에 작은 라벨 또는 상태 탭 하단.** 작은 화면에서 헤더가 더 길어지는 부담이 있으면 상태 탭으로.
6. 3선택지 ActionBar — **권고: 그대로 진행 + 권고 P(라벨 길이 제한).** 현재 ActionBar VBoxContainer 구조로 3개 처리 가능 검증됨.
7. 선택지별 고유 연대기 미작성 처리 — **권고: 권고 B의 5케이스에만 chronicle 오버라이드를 채운다.** 나머지는 대표 문장 + 결과 문장 조합.

---

## 8. Project Codex 리뷰 질문 답변

> Q1. C# 정적 콘텐츠 데이터 방식이 현재 프로젝트 규모와 VS1 목적에 맞는가, 아니면 외부 데이터 파일을 도입해야 할 만큼 이식/검증 비용이 큰가?

**답: 정적 C# 방식이 맞다.** 외부 데이터 파일은 비용만 늘리고 VS1 목적(반복 플레이 재미 검증)과 무관하다. 다만 단일 파일이 1500~2000줄로 늘어나므로 분류별 partial class 분할(권고 H) 권고.

> Q2. 기존 `FamilyRunState`를 확장하는 방식과 VS1 전용 상태를 새로 두는 방식 중 어느 쪽이 현재 코드 변경 위험이 낮은가?

**답: VS1 전용 새 클래스 `Vs1FamilyState` 도입(권고 G).** 기존 `FamilyRunState`/`MultigenContent`/`MultigenFlow`는 dead code로 보존. `TraitStub` vs `Vs1HeirProfile`, `GenerationRecord` vs `Vs1GenerationSummary`의 표현 차이가 너무 커 확장보다 분리가 깔끔하다.

> Q3. 46개 사건과 3개 선택지 사건을 기존 `ActionBar` 구조에서 작은 창 기준으로 감당할 수 있는가?

**답: 감당 가능.** 현재 ActionBar VBoxContainer는 N개 버튼을 세로로 쌓는 구조라 3개도 정상 처리한다. 다만 360x640 환경에서는 버튼 아래 영향 요약 라벨이 길어지면 ActionBar가 화면의 30%를 점유한다. `Vs1EventChoice.ImpactPreview` 길이 제한(약 24자 한 줄, 권고 P) 권고.

> Q4. `--smoke`를 3회 deterministic 실행으로 확장할 때 Godot 헤드리스 시간과 출력량이 과하지 않은가?

**답: 과하지 않다.** 사이클당 5~10초 × 3회 = 15~30초 헤드리스 실행. stdout 약 900~1200줄로 Bash tool 30000자 캡 안. 다만 회당 anchor + ASSERT 출력 형식 정리(권고 T, U) 권고.

> Q5. 위 구현 단계 중 Project Claude가 멈춰야 할 기술적 충돌 또는 범위 초과가 있는가?

**답: 멈춰야 할 충돌은 없다.** 다만 Root 확인 항목(특히 신규 8/9/10번 — 분기 처리, 조기 종결, 후계 풀)이 미정인 상태에서 46개 사건 이식(B 작업)을 시작하면, 분기 사건의 데이터를 임시값으로 채우고 Root 확정 후 다시 채워야 한다. **Root 확인이 완료된 후 B 작업을 시작하길 권고.** A(모델) 작업은 분기 슬롯/조기 종결 슬롯의 모델 필드만 미리 두면 Root 확인 결과를 기다리지 않고 시작 가능.

또한 본문 §6 구현 단계의 수정 범위에 `MultigenModels.cs`, `MultigenFlow.cs`, `MultigenContent.cs`가 포함되지만, 권고 G(별도 클래스 트리)를 따르면 이 3파일은 사실상 손대지 않게 된다. 03 작업 지시서에서 "이 3파일은 보존, VS1은 새 파일 트리"로 명시하면 합류 계약이 더 명확해진다.

---

## 9. 종합 평가

**리뷰 결론: 큰 틀에서 진행 가능. 단 본 리뷰의 권고 A~Z + Root 확인 항목 8~16번을 03 최종 작업 지시서에 반영하길 권고.**

가장 중요한 3개 우선순위:

1. **사건 결과 "X 또는 Y" 분기 처리 규칙** (권고 I, Root 8번) — 데이터 모델에 분기 슬롯 필요, Root가 분기 조건 채워야 의미 있는 가문사
2. **일반 사건의 조기 종결 처리** (권고 J, Root 9번) — 옵션 1(단순화) 권고
3. **deterministic smoke의 멸문 강제 분기** (권고 S, Root 14번) — MVP smoke 패턴 유지

다음 우선순위:

4. 선택지별 chronicle 오버라이드 슬롯 (권고 B)
5. `Vs1FamilyState` 분리 결정 (권고 G, Root 13번)
6. 사건 콘텐츠 partial 분할 (권고 H)
7. 후계 후보 풀 분리 (권고 N, Root 10번)

3개 핵심 우선순위가 결정되지 않은 채 46개 사건 이식이 진행되면, 분기 사건 약 18개의 데이터를 임시 처리 후 재작업해야 한다. Root 확인 라운드를 한 번 더 거친 뒤 03 최종 작업 지시서를 쓰는 것이 안전하다.
