# Vertical Slice 1 Playtest Build Review Request

> 작성일: 2026-05-21
> 작성자: Project Codex
> 모드: Project Claude 리뷰 요청
> 답변 문서 경로: `docs/handoff/vertical-slice-1-playtest-build/02-claude-review.md`

## Project Claude 전달 문장

`docs/handoff/vertical-slice-1-playtest-build/01-review-request.md`를 읽고 리뷰 모드로 검토해 주세요. 바로 구현하지 말고, 리뷰 결과를 `docs/handoff/vertical-slice-1-playtest-build/02-claude-review.md`에 작성해 주세요. 특히 Vertical Slice 1 데이터 구조, 46개 사건 이식 방식, 게임 루프, 반응형 탭 UI, 스모크/수동 검증 계획의 누락과 위험을 우선 검토해 주세요.

## 1. 읽은 문서와 제품 기준 요약

### 읽은 문서

- `AGENTS.md`
- `CLAUDE.md`
- `D:\Project\Godot\legacy-log\docs\product\prd.md`
- `D:\Project\Godot\legacy-log\docs\current-focus.md`
- `D:\Project\Godot\legacy-log\docs\decisions\002-agent-workflow.md`
- `D:\Project\Godot\legacy-log\docs\decisions\011-ui-ux-validation-workflow.md`
- `D:\Project\Godot\legacy-log\docs\decisions\012-responsive-tabbed-ui-direction.md`
- `D:\Project\Godot\legacy-log\docs\decisions\013-medieval-fantasy-noble-house-direction.md`
- `D:\Project\Godot\legacy-log\docs\decisions\014-vertical-slice-1-playtest-scope.md`
- `D:\Project\Godot\legacy-log\docs\decisions\015-vertical-slice-1-gameplay-content-and-validation.md`
- `D:\Project\Godot\legacy-log\docs\decisions\016-root-planning-project-implementation-boundary.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-event-list.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-event-copy-draft.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-event-chronicle-draft.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-legacy-tag-rules.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-title-progression-rules.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-generation-event-flow.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-event-tag-change-table.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-generation-start-and-tag-display.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-generation-summary-templates.md`
- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-manual-playtest-guide.md`
- `docs/handoff/multigen-mvp/04-implementation-report.md`
- `docs/handoff/multigen-mvp/05-root-status-summary.md`
- `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md`
- `docs/handoff/multigen-ui-ux-improvement/05-root-status-summary.md`

### 확인한 제품 기준

- Vertical Slice 1의 목적은 MVP 완료 재증명이 아니라 중세 판타지 귀족가문 반복 플레이 재미 검증이다.
- 한 실행은 보통 4~8세대, 10~20분 내외를 목표로 한다.
- 한 세대는 보통 5~7개의 상호작용 사건을 가진다.
- 사건은 총 46개이며, 영지/재산 8개, 명예/전쟁/기사도 8개, 궁정/정치 8개, 가문 내부/후계 8개, 작위 상승 5개, 작위 상실/위기 5개, 세대 종결 4개다.
- 핵심 상태는 재산, 명예, 궁정 영향력, 가문 결속이다.
- 작위는 남작, 자작, 백작, 후작, 공작까지 실제 플레이 영향이 있어야 한다. 왕/황제 통치 시스템은 제외한다.
- 작위 상승은 자동 보상이 아니라 조건 충족 후 등장하는 선택 가능한 승격 기회 사건이다.
- 작위 상실은 즉시 게임오버가 아니라 대표작위 하락, 박탈 위기, 폐위, 쇠락 같은 중간 단계로 표현한다.
- 멸문은 후계 부재와 사망/퇴장이 결합될 때 발생한다.
- 유산 태그는 다음 세대 사건 조건, 세대 시작 상황, 연대기 정체성에 영향을 준다.
- 후계자 성향/강점/약점은 사건 조건과 결과에 약하게만 영향을 주며 성공/실패를 단독 결정하지 않는다.
- UI/UX는 기능 검증의 보조가 아니라 이번 빌드의 핵심 검증 대상이다.
- 작은 창과 세로형 화면에서도 현재 가문, 대표작위, 세대, 인물, 현재 단계, 상태 변화, 작위 위험, 유산 태그, 연대기 흔적을 확인할 수 있어야 한다.

### 현재 구현 구조 관찰

- `Scenes/Main.tscn`은 루트 `Control`에 `res://scripts/Main.cs`만 붙은 구조다.
- `scripts/Main.cs`가 런타임에 `ContextHeader`, `MainScroll`, `InfoTabs`, `ActionBar` 4영역 UI를 동적 생성한다.
- 현재 탭은 `상태`, `연대기`, `가문사` 3개다. 유산 요약은 `가문사` 탭 상단에 흡수돼 있다.
- 현재 다세대 흐름은 `FamilyRunState`, `RunState`, `MultigenFlow`, `MultigenContent`, `MvpLoopContent`로 나뉘어 있다.
- 현재 상태와 사건은 MVP 자리표시자다. 상태는 `Reputation`, `Stores`, `Cohesion` 3개이고, 사건은 3개뿐이다.
- `--smoke`는 UI 노드를 만들지 않는 기능 검증 경로다.
- Godot 경로 케이스는 현재 `scripts/` 소문자, `Scenes/` 대문자 S다. 새 C# 파일은 기존 `scripts/` 아래에 둔다.

## 2. 구현 데이터 구조 제안

### 기본 방향

Vertical Slice 1은 데이터 로더 자체를 검증하는 작업이 아니다. 따라서 첫 구현은 외부 JSON/CSV 파서보다 **타입이 있는 C# 정적 콘텐츠 데이터**로 시작한다.

권장 신규 파일:

- `scripts/VerticalSliceModels.cs`
- `scripts/VerticalSliceContent.cs`
- `scripts/VerticalSliceFlow.cs`

기존 파일과의 관계:

- `MvpLoopContent.cs`는 보존한다. Vertical Slice 1이 안정되기 전까지 과거 MVP 콘텐츠의 기준점으로 둔다.
- `MvpLoopModels.cs`의 일부 모델은 바로 확장하지 말고, VS1 전용 모델을 만든다. 기존 `EventChoice`, `LoopEvent`는 46개 사건의 조건/태그/작위/요약 구조를 담기에는 좁다.
- `MultigenModels.cs`의 `FamilyRunState`는 직접 대규모 확장하기보다 VS1용 상태 타입을 추가하거나 감싼다. 리뷰 후 실제 변경 방식 결정이 필요하다.

### 상태 모델

제품 상태 4축을 명시적으로 둔다.

```csharp
public enum Vs1StateAxis
{
    Wealth,
    Honor,
    CourtInfluence,
    HouseUnity,
}

public enum Vs1ImpactDirection
{
    Down,
    Up,
}

public enum Vs1ImpactMagnitude
{
    Minor,
    Moderate,
    Major,
}
```

수치 밸런스를 확정하지 않기 위해 사건 데이터는 `+2`, `-3` 같은 숫자를 직접 중심에 두지 않는다. 선택지에는 `StateImpact(axis, direction, magnitude)`를 저장하고, `VerticalSliceFlow`가 임시 내부 점수로 변환한다.

UI 기본 표시는 질적 구간과 변화 방향만 사용한다.

- 예: `재산: 낮음`, `명예: 상승`, `가문 결속: 하락`
- 정확한 숫자가 필요하면 `상태` 탭의 상세 토글에만 임시 내부값을 표시한다.

Root 확인 필요 여부: 임시 내부값이 상세 토글에 노출되는 것이 수동 검증에 방해된다면, 상세 토글도 질적 변화만 보여주는 쪽으로 조정해야 한다.

### 사건 데이터

46개 사건은 `Vs1EventDefinition` 배열로 둔다.

```csharp
public sealed class Vs1EventDefinition
{
    public string Id { get; init; }
    public string Title { get; init; }
    public Vs1EventCategory Category { get; init; }
    public Vs1EventRole Role { get; init; }
    public bool IsMajor { get; init; }
    public string Body { get; init; }
    public IReadOnlyList<Vs1EventCondition> Conditions { get; init; }
    public IReadOnlyList<Vs1EventChoice> Choices { get; init; }
    public string ChronicleLine { get; init; }
}
```

필수 필드:

- `Id`: `EST-01` 같은 Root 문서 ID를 그대로 사용한다.
- `Category`: `Estate`, `HonorWarChivalry`, `CourtPolitics`, `FamilySuccession`, `Promotion`, `Crisis`, `GenerationEnd`
- `Role`: `Foundation`, `Pressure`, `TurningPoint`, `GenerationEnd`
- `IsMajor`: `vertical-slice-1-event-chronicle-draft.md`의 큰 사건/보조 사건 기준 반영
- `Body`: `vertical-slice-1-event-copy-draft.md` 본문
- `Choices`: 선택지 행동명, 선택 전 영향 요약, 결과 문장, 상태 영향, 태그 변화, 작위 변화, 후계 영향, 종결 후보
- `ChronicleLine`: 대표 연대기 문장. 선택지별 고유 연대기는 문서상 플레이테스트 후 확장으로 남아 있으므로 이번 구현은 대표 문장 + 결과 문장 + 태그/작위 변화 조합으로 표현한다.

선택지 구조:

```csharp
public sealed class Vs1EventChoice
{
    public string Label { get; init; }
    public string ImpactPreview { get; init; }
    public string ResultText { get; init; }
    public IReadOnlyList<Vs1StateImpact> StateImpacts { get; init; }
    public IReadOnlyList<Vs1LegacyTagChange> TagChanges { get; init; }
    public Vs1TitleEffect? TitleEffect { get; init; }
    public Vs1HeirEffect? HeirEffect { get; init; }
    public GenerationEndType? EndTypeHint { get; init; }
}
```

`ImpactPreview`는 선택지 버튼 또는 버튼 아래 짧은 라벨에 사용한다. 버튼 텍스트는 행동명만 두고, 영향 요약은 같은 ActionBar 안에 짧게 붙이거나 결과 전 확인용 본문에 둔다.

### 조건 데이터

사건 조건은 내부적으로 다음 종류를 지원한다.

- 대표작위 조건: 최소/최대 작위 또는 정확한 작위
- 상태 구간 조건: 특정 상태가 낮음/보통/높음 이상
- 유산 태그 조건: 활성 태그 포함/미포함
- 세대 조건: 최소 세대, 장기 진행
- 후계자 특성 조건: 성향/강점/약점 포함

```csharp
public sealed class Vs1EventCondition
{
    public Vs1ConditionKind Kind { get; init; }
    public string Key { get; init; }
    public string DisplayReason { get; init; }
}
```

`DisplayReason`은 사건 화면의 "관련 유산", "관련 상태", "관련 작위 위험"처럼 플레이어에게 보이는 짧은 이유로 사용한다. 모든 내부 조건을 노출하지 않고, 판단에 필요한 1~3개만 노출한다.

### 작위와 작위 위험

```csharp
public enum NobleTitleRank
{
    Baron,
    Viscount,
    Count,
    Marquess,
    Duke,
}

public enum TitleRiskStage
{
    Stable,
    Watched,
    Crisis,
    RevocationThreat,
}
```

가문 상태는 다음을 가진다.

- `HeldTitles`: 보유 작위 목록. VS1에서는 한 작위만 보유해도 되지만, 대표작위 상실 후 하위 작위 유지 표현을 위해 목록 구조를 둔다.
- `RepresentativeTitle`: 보유 작위 중 가장 높은 작위로 계산한다.
- `TitleRiskStage`: 안정, 견제, 위기, 박탈 위협
- `TitlePressureTags`: `불안한 승격`, `박탈 위기`, `변경의 책무` 같은 작위 관련 활성 태그를 통해 설명한다.

작위 효과:

```csharp
public sealed class Vs1TitleEffect
{
    public NobleTitleRank? PromoteTo { get; init; }
    public bool LoseRepresentativeTitle { get; init; }
    public TitleRiskStage? RiskStageChange { get; init; }
    public string ResultSummary { get; init; }
}
```

승격은 `PRO-*` 사건 선택 결과에서만 직접 발생한다. 조건 충족 자체는 자동 승급이 아니라 `PRO-*` 사건 후보 가중치를 높인다.

### 유산 태그

```csharp
public enum LegacyTagDuration
{
    ShortTerm,
    Generation,
    HouseHistory,
}

public sealed class Vs1LegacyTagDefinition
{
    public string Key { get; init; }
    public string Label { get; init; }
    public string FamilyStartDescription { get; init; }
    public LegacyTagDuration DefaultDuration { get; init; }
    public IReadOnlyList<string> OpposedTags { get; init; }
}
```

활성 태그 상태:

```csharp
public sealed class Vs1ActiveLegacyTag
{
    public string Key { get; init; }
    public LegacyTagDuration Duration { get; init; }
    public int CreatedGeneration { get; init; }
    public bool IsActive { get; set; }
}
```

태그 변화:

```csharp
public sealed class Vs1LegacyTagChange
{
    public Vs1LegacyTagChangeKind Kind { get; init; } // Add, Resolve, Replace, Weaken
    public string TagKey { get; init; }
    public string? ReplacedTagKey { get; init; }
    public string DisplayText { get; init; }
}
```

상반 태그는 `OpposedTags`와 `Replace` 변화로 처리한다. 교체가 발생하면 결과 화면에 반드시 표시한다.

세대 시작 화면 태그 노출:

- 기본 화면 최대 4개
- 우선순위: 멸문/작위/후계 위험, 이번 세대 사건 풀 변경, 직전 큰 사건, 대표작위 연결, 상태 설명
- 나머지는 `가문사` 탭 상세로 보낸다.

### 후계자

Root 문서 후보를 enum 또는 string key로 둔다. 기획 문구 변경 가능성을 고려해 표시 라벨은 `VerticalSliceContent`에 둔다.

```csharp
public sealed class Vs1HeirProfile
{
    public string Name { get; init; }
    public string DispositionKey { get; init; }
    public string StrengthKey { get; init; }
    public string WeaknessKey { get; init; }
    public string SuccessionReason { get; init; }
}
```

후계자 특성은 다음에만 약하게 반영한다.

- 사건 후보 가중치
- 선택지 결과의 영향 크기 보정
- 사건 화면의 관련 단서

특성은 성공/실패를 단독 결정하지 않는다.

### 세대 시작/요약 데이터

가문 상태는 세대 전환 시 다음 정보를 만든다.

```csharp
public sealed class Vs1GenerationStartInfo
{
    public string PreviousGenerationSummary { get; init; }
    public IReadOnlyList<Vs1ActiveLegacyTag> FeaturedTags { get; init; }
    public IReadOnlyList<Vs1StateBandLine> StateBands { get; init; }
    public Vs1HeirProfile CurrentCharacter { get; init; }
    public string PressureLine { get; init; }
}

public sealed class Vs1GenerationSummary
{
    public int GenerationNumber { get; init; }
    public string CharacterName { get; init; }
    public NobleTitleRank StartTitle { get; init; }
    public NobleTitleRank EndTitle { get; init; }
    public GenerationEndType EndType { get; init; }
    public IReadOnlyList<string> MajorEventIds { get; init; }
    public IReadOnlyList<string> FeaturedLegacyTags { get; init; }
    public string SummaryParagraph { get; init; }
}
```

요약 문장 조합은 Root 템플릿의 구조를 따른다. 최종 문체를 새로 확정하지 않는다.

## 3. 게임 루프 단계 제안

### 전체 실행

1. 새 실행 생성
2. 1세대 시작 정보 구성
3. 세대 시작 화면 표시
4. 세대 사건 루프
5. 세대 종결 사건 처리
6. 세대 요약 표시
7. 후계/멸문 판단
8. 다음 세대 시작 또는 멸문 화면
9. 실행 종료 시 전체 가문사 확인

### 세대 시작

입력:

- 이전 세대 요약
- 활성 유산 태그
- 대표작위와 작위 위험
- 핵심 상태 4축
- 현재 인물 성향/강점/약점

출력:

- 이전 세대 한 줄 요약
- 핵심 유산 태그 2~4개
- 핵심 상태 4개 질적 구간
- 후계자/가주 정보
- 이번 세대 압력 또는 기회 1~2문장

### 사건 후보 선정

세대별 사건 슬롯:

- 기반 사건 1~2개
- 압력 사건 2~3개
- 전환 사건 0~2개
- 세대 종결 사건 1개

선정 원칙:

- 같은 세대에 동일 사건 반복 금지
- 같은 세대에 같은 분류 3회 이상 연속 금지
- 한 실행에서 같은 큰 사건은 가능하면 반복 금지
- 조건이 맞는 사건을 우선 고르고, 부족하면 일반 조건 사건으로 채운다.
- 사건 등장 이유는 대표작위, 상태, 태그, 후계자 특성 중 최소 하나로 설명 가능해야 한다.

구현 제안:

- `VerticalSliceFlow.BuildEventPlanForGeneration(family, rng)`가 현재 세대의 사건 ID 목록을 먼저 만든다.
- `FamilyRunState.CurrentRun`은 현재 사건 인덱스와 이번 세대의 사건 ID 목록을 가진다.
- `GenerationEnd` 분류는 마지막 슬롯으로 강제 배치한다.

### 사건 표시

사건 화면 기본 표시:

- 사건 분류
- 사건 제목과 본문
- 관련 상태
- 관련 작위 위험
- 관련 유산 태그
- 후계자 특성과 연결되는 단서
- 선택지: 짧은 행동명 + 짧은 영향 요약

정확한 숫자, 전체 조건, 전체 태그 목록은 탭으로 보낸다.

### 선택 결과 적용

선택 적용 순서:

1. 상태 영향 적용
2. 작위 변화 또는 작위 위험 변화 적용
3. 태그 추가/해소/교체/약화 적용
4. 후계자 안정 또는 후계 후보 상태 영향 적용
5. 연대기 항목 추가
6. 현재 세대의 큰 사건 목록 갱신
7. 결과 화면 표시

결과 화면 순서:

1. 결과 문장
2. 상태 변화 요약
3. 태그 변화
4. 작위 변화 또는 작위 위험 변화
5. 연대기 기록
6. 다음 행동

### 연대기/유산 태그 기록

- 진행 중 연대기는 사건별 대표 연대기 문장을 기본 사용한다.
- 선택지별 결과 차이는 결과 문장, 상태 변화, 태그 변화, 작위 변화로 드러낸다.
- 다음 경우는 같은 사건 안에서도 연대기 문장 또는 요약 문장에 차이를 드러낸다.
  - 작위 상승 성공과 보류
  - 대표작위 상실과 방어 성공
  - 후계 안정과 후계 불안
  - 멸문과 다음 세대 연결
  - 강한 가문사 태그 변화

### 세대 종결

세대 종결 사건은 항상 마지막에 놓는다. 종결 유형은 현재 상태와 태그, 종결 사건 선택 결과를 반영한다.

멸문 조건:

- 현재 인물의 사망/퇴장 유형이 발생한다.
- 후계 후보가 없거나 후계 권리가 무효화된다.
- 두 조건이 결합될 때만 멸문 처리한다.

작위 상실은 멸문이 아니다. 대표작위 하락이나 폐위 후 방계 승계로 다음 세대를 이어갈 수 있다.

### 다음 세대 시작

다음 세대 생성 시:

- 세대 태그와 가문사 태그를 유지/약화/해소한다.
- 단기 태그는 현재 세대 안에서 해소하거나 다음 세대 시작 전에 정리한다.
- 상태는 내부 점수를 일부 이월하되, UI에는 질적 구간만 표시한다.
- 후계자 프로필을 현재 인물로 승계한다.
- 이전 세대 요약과 핵심 태그로 세대 시작 문장을 만든다.

## 4. UI/UX 구현 계획

### 기본 구조

기존 4영역 UI shell을 유지한다.

- `ContextHeader`: 항상 보이는 가문명, 대표작위, 세대, 현재 인물, 현재 단계
- `MainScroll`: 현재 화면의 핵심 본문
- `InfoTabs`: 상태, 연대기, 가문사
- `ActionBar`: 선택지와 다음 진행 버튼

`Scenes/Main.tscn`은 계속 루트 `Control` + `res://scripts/Main.cs` 구조로 둔다. 정적 씬 자식 노드를 새로 만들 필요가 없다.

### 헤더

헤더 1행:

- 가문명
- 대표작위
- 가문력/세대
- 현재 인물

헤더 2행:

- 단계: 세대 시작, 사건 진행, 결과 확인, 세대 요약, 후계 선택, 멸문
- 위험 경고 1개: 작위 위험이 `위기` 이상이거나 멸문 위험이 높을 때만 표시

### 탭

기존 3탭을 유지하되 내용은 확장한다.

- `상태`: 재산, 명예, 궁정 영향력, 가문 결속 질적 구간. 상세 토글에는 내부 점수 또는 변화 방향.
- `연대기`: 현재 세대의 사건 기록. 최신 기록 강조.
- `가문사`: 활성 유산 태그 요약, 지난 세대 이력, 전체 태그 목록, 작위 이력.

새 `유산` 탭을 추가하지 않는다. 탭 수가 늘면 작은 화면에서 주요 흐름을 방해할 가능성이 크다.

### 세대 시작 화면

정보 우선순위:

1. 가문/작위/세대/인물 헤더
2. 핵심 유산 태그 2~4개
3. 이전 세대 한 줄 요약
4. 이번 세대 출발 상태
5. 후계자 성향/강점/약점
6. 이번 세대 압력 또는 기회
7. `첫 사건으로 들어간다` 버튼

작은 화면에서는 문장을 줄이고 태그 라벨과 변화 방향을 우선한다.

### 사건 화면

정보 우선순위:

1. 사건 분류와 성격
2. 사건 제목
3. 짧은 본문
4. 관련 상태/작위 위험/유산 태그/후계자 단서
5. 선택지 버튼
6. 선택지 영향 요약

선택지는 현재 2~3개까지를 ActionBar 세로 목록에 둔다. 3개 선택지를 가진 사건은 `FAM-01`, `FAM-08`, `CRI-03`, `PRO-05` 등이 있으므로 ActionBar는 3개 버튼까지 정상 처리해야 한다.

### 결과 화면

정보 우선순위:

1. 결과 문장
2. 상태 변화 요약
3. 새 유산/해소/교체
4. 대표작위 변화 또는 작위 위험 변화
5. 연대기 기록
6. 다음 사건 또는 세대 요약 버튼

태그 변화는 해당 선택으로 변한 태그만 보여준다. 전체 태그 목록은 가문사 탭에 둔다.

### 세대 요약 화면

기본 화면:

- 세대 번호
- 인물명
- 시작 대표작위와 종료 대표작위
- 종결 유형
- 대표 유산 태그 1~3개
- 세대 요약 단락
- 다음 세대 시작 또는 가문사 종료 버튼

상세:

- 전체 사건 목록
- 전체 상태 변화
- 전체 태그 변화
- 작위 위험 변화
- 후계자 변화

### 작은 창/세로형 기준

검증 기준 창:

- `360x640`
- `540x720`
- `1280x720`

구현 기준:

- `MainScroll`만 확장 영역으로 둔다.
- 탭 내부는 자체 스크롤을 유지한다.
- ActionBar는 화면 하단에서 눌릴 수 있어야 한다.
- 헤더는 2행 이상 줄바꿈을 허용한다.
- 버튼은 세로 목록으로 쌓는다.
- 선택지 영향 요약이 길어 버튼 텍스트를 밀어내면, 버튼 아래 보조 라벨로 분리한다.

## 5. 검증 계획

### 기능 검증

자동 검증 명령:

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

`--smoke` 권장 범위:

- 최소 3개의 deterministic 실행을 seed별로 수행한다.
- 각 실행은 4세대 이상 또는 멸문까지 진행한다.
- 각 세대가 5~7개 상호작용 사건을 가지는지 출력한다.
- 46개 사건 중 조건에 맞는 사건 후보 선정이 되는지 확인한다.
- 적어도 한 실행에서 승격 기회 사건이 등장한다.
- 적어도 한 실행에서 작위 위험 또는 대표작위 상실 경로가 등장한다.
- 적어도 한 실행에서 유산 태그가 다음 세대 사건 등장 이유로 출력된다.
- 멸문은 후계 부재 + 사망/퇴장 결합일 때만 출력된다.
- 마지막 줄은 `SMOKE_OK`를 유지한다.

스모크 출력에 포함할 핵심 줄:

- `[SMOKE] RUN seed=...`
- `[SMOKE] GEN_START gen=... title=... risk=... tags=...`
- `[SMOKE] EVENT id=... category=... reason=...`
- `[SMOKE] CHOICE ...`
- `[SMOKE] RESULT state_changes=... tag_changes=... title_change=...`
- `[SMOKE] GEN_SUMMARY gen=... events=... end=...`
- `[SMOKE] SUCCESSION candidates=...`
- `[SMOKE] EXTINCT ...`
- `SMOKE_OK`

### 반복 플레이 검증

수동 플레이 후 `vertical-slice-1-manual-playtest-guide.md` 양식으로 기록한다.

최소 조건:

- 3회 이상 반복 플레이
- 1회 일반 창
- 1회 작은 창
- 1회 세로형 화면

회차별 기록:

- 도달 세대
- 종료 대표작위
- 종료 유형
- 기억나는 사건
- 가장 의미 있었던 선택
- 가장 이해하기 어려웠던 선택
- 작위 상승/부담/상실 체감
- 유산 태그와 후계자 차이 체감
- UI에서 자주 확인한 정보와 찾기 어려웠던 정보

### UI/UX 수동 검증

확인 항목:

- 현재 가문, 대표작위, 세대, 현재 인물, 단계가 항상 보이는가
- 사건 선택이 어떤 상태와 작위 위험에 영향을 주는지 보이는가
- 선택 결과가 상태, 연대기, 유산 태그, 작위 변화에 어떻게 남았는지 보이는가
- 세대 시작 화면에서 이전 세대 결과와 이번 세대 조건이 연결되는가
- 작위 상승, 작위 상실, 쇠락, 멸문 위험의 이유가 보이는가
- 작은 창과 세로형 화면에서 버튼, 탭, 텍스트가 잘리거나 겹치지 않는가
- 상세 정보는 필요한 경우 찾을 수 있고 기본 흐름을 방해하지 않는가

자동 검증과 수동 검증은 보고서에서 분리한다.

## 6. Project Claude 최종 구현 지시서 초안

주의: 이 섹션은 최종 지시서가 아니라 리뷰 대상 초안이다. `02-claude-review.md` 검토 후 Project Codex가 반영 여부를 판단하고 `03-final-work-instruction.md`를 별도로 작성해야 한다.

### 구현 결과 보고 경로

`docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`

### 목적

Vertical Slice 1: 중세 판타지 귀족가문 반복 플레이 재미 검증 빌드를 구현한다. 목표는 MVP 완료 재검증이 아니라, 46개 사건, 작위 상승/상실, 유산 태그, 후계자 특성, 세대 시작/요약 UI가 반복 플레이에서 서로 다른 가문사를 만드는지 확인 가능한 플레이어블 빌드를 만드는 것이다.

### 수정 범위

수정 가능:

- `scripts/Main.cs`
- `scripts/MultigenModels.cs`
- `scripts/MultigenFlow.cs`
- `scripts/MultigenContent.cs`
- 신규 `scripts/VerticalSliceModels.cs`
- 신규 `scripts/VerticalSliceContent.cs`
- 신규 `scripts/VerticalSliceFlow.cs`
- 필요 시 신규 `scripts/VerticalSliceSmoke.cs`
- `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`

가능하면 변경하지 않음:

- `Scenes/Main.tscn`
- `project.godot`

수정 금지:

- 루트 `docs/product/` 기획 문서
- 루트 `docs/decisions/` 결정 문서
- 새 사건 추가 또는 사건 본문 재작성

### 제외 범위

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 문체 최종 확정
- 밸런스 수치 최종 확정
- 왕/황제 실제 통치 시스템
- 혼인/혈통 트리/저장 불러오기
- 정교한 경제/전쟁/AI 귀족 시뮬레이션
- 최종 UI/아트 스타일 확정

### 사전 가정

- Root 기획 문서가 제품 기준의 source of truth다.
- 46개 사건은 Root 문서의 ID, 제목, 본문, 선택지, 영향 요약, 대표 연대기 문장을 그대로 사용한다.
- 상태 변화량은 최종 밸런스가 아니므로 내부 임시 점수로만 다룬다.
- 플레이어 기본 UI에는 질적 구간과 변화 방향을 우선 표시한다.
- `scripts/` 경로 케이스는 소문자로 유지한다.
- `Scenes/Main.tscn`의 `ext_resource path="res://scripts/Main.cs"`는 디스크 케이스와 일치하게 유지한다.
- 사전 가정이 실제 코드와 다르면 추측하지 말고 멈춘다.

### 구체적 구현 단계

1. VS1 모델 추가
   - `VerticalSliceModels.cs`에 상태 4축, 작위, 작위 위험, 사건 분류, 사건 조건, 선택지, 태그 정의, 후계자 프로필 모델을 추가한다.
   - 기존 MVP 모델을 무리하게 확장하지 않는다.

2. VS1 콘텐츠 이식
   - `VerticalSliceContent.cs`에 Root 문서의 46개 사건을 정적 데이터로 옮긴다.
   - 사건 ID, 분류, 본문, 선택지 행동명, 영향 요약, 대표 연대기 문장, 큰 사건 여부, 태그 변화 후보를 포함한다.
   - 사건 문안은 재작성하지 않는다.

3. VS1 가문 상태와 세대 상태 추가
   - 핵심 상태 4축, 보유 작위, 대표작위, 작위 위험 단계, 활성 유산 태그, 현재 인물 성향/강점/약점, 세대 사건 계획을 저장한다.
   - 기존 `FamilyRunState`를 확장할지, VS1 전용 상태를 둘지는 코드 충돌이 적은 쪽을 선택한다.

4. 사건 후보 선정 구현
   - 세대 시작 시 5~7개 사건 계획을 만든다.
   - 기반/압력/전환/종결 슬롯을 구분한다.
   - 작위, 상태 구간, 활성 태그, 후계자 특성 중 하나 이상을 후보 선정에 반영한다.
   - 같은 세대 동일 사건 반복을 막는다.
   - 같은 분류가 3회 이상 연속되지 않게 한다.

5. 선택 결과 적용 구현
   - 상태 영향, 태그 변화, 작위 변화/위험 변화, 후계 영향, 연대기 기록을 한 흐름으로 처리한다.
   - 작위 승격은 `PRO-*` 선택 결과에서만 직접 발생하게 한다.
   - 작위 상실은 멸문이 아니라 쇠락/대표작위 변화로 처리한다.
   - 멸문은 후계 부재와 사망/퇴장 결합일 때만 처리한다.

6. 세대 시작/요약 구현
   - 세대 시작 화면에 이전 세대 요약, 핵심 태그 2~4개, 출발 상태, 후계자 특성, 이번 세대 압력을 표시한다.
   - 세대 요약 화면에 시작/종료 대표작위, 종결 유형, 대표 유산 태그, 요약 단락, 다음 행동을 표시한다.

7. UI 확장
   - 기존 4영역 UI shell을 유지한다.
   - 헤더에 대표작위와 위험 경고를 추가한다.
   - 사건 화면에 분류, 관련 상태, 작위 위험, 유산 태그, 후계자 단서를 표시한다.
   - 결과 화면 순서를 Root 기준에 맞춘다.
   - 3개 선택지 사건이 ActionBar에서 정상 표시되게 한다.
   - 탭은 `상태`, `연대기`, `가문사` 3개를 유지한다.

8. 스모크 갱신
   - `--smoke`가 VS1 흐름을 deterministic seed로 최소 3회 실행하게 한다.
   - 승격, 작위 위험/상실, 태그 이월, 세대 전환, 멸문 조건을 stdout으로 확인한다.
   - 마지막 줄 `SMOKE_OK`를 유지한다.

9. 구현 보고 작성
   - 변경 파일
   - 구현된 사용자 흐름
   - 실행한 검증 명령과 결과
   - 자동 검증 범위와 한계
   - 수동 UI/UX 검증 필요 항목
   - 남은 제약 또는 미구현 범위
   - Root 확인 필요 사항을 `04-implementation-report.md`에 작성한다.

### 병렬 작업 분해

병렬화 가능 후보:

- A. 모델/흐름: `VerticalSliceModels.cs`, `VerticalSliceFlow.cs`
- B. 콘텐츠 이식: `VerticalSliceContent.cs`
- C. UI 연결: `Main.cs`
- D. 스모크/보고: `VerticalSliceSmoke.cs`, `04-implementation-report.md`

합류 순서:

1. A의 모델 이름과 필드가 먼저 확정되어야 B와 C가 안정적으로 붙는다.
2. B는 A의 모델을 기준으로 46개 사건을 채운다.
3. C는 A/B가 제공하는 public API만 호출한다.
4. D는 A/B/C 통합 후 작성한다.

병렬화하지 말아야 할 지점:

- `Main.cs`와 `VerticalSliceFlow.cs` 사이의 화면 전환 계약은 한 번에 합쳐 검증한다.
- 사건 데이터 필드명을 바꾸는 작업과 46개 사건 이식은 병렬화하지 않는다.

### Godot 경로 케이스 계약

- 기존 씬: `Scenes/Main.tscn`
- 기존 스크립트: `scripts/Main.cs`
- 신규 스크립트는 모두 `scripts/` 아래 소문자 디렉터리에 둔다.
- `Scenes/Main.tscn`의 `ext_resource path="res://scripts/Main.cs"`는 변경하지 않는다.
- 새 C# 스크립트를 씬에 직접 어태치하지 않는다.

### 검증 명령

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

### 완료 기준

- 46개 사건이 VS1 콘텐츠 데이터에 포함된다.
- 한 세대는 보통 5~7개 사건으로 진행된다.
- 한 실행에서 여러 세대를 진행할 수 있다.
- deterministic smoke에서 최소 3회 실행이 완료된다.
- 승격 기회가 자동 보상이 아니라 선택 사건으로 등장한다.
- 작위 상실 또는 작위 위험이 즉시 게임오버가 아닌 쇠락 단계로 표시된다.
- 유산 태그가 결과 화면과 다음 세대 시작 화면에 보인다.
- 후계자 성향/강점/약점이 세대 시작 화면과 사건 조건/단서에 약하게 반영된다.
- 멸문은 후계 부재와 사망/퇴장 결합일 때만 발생한다.
- 작은 창과 세로형 화면에서 수동 확인할 수 있는 UI 구조가 유지된다.
- `dotnet build`, Godot headless import, `--smoke`가 성공한다.

### 보고 형식

`04-implementation-report.md`에 다음 순서로 작성한다.

1. 변경 파일
2. 구현된 데이터 구조
3. 구현된 게임 루프
4. 구현된 UI/UX 흐름
5. 실행한 검증 명령과 결과
6. 자동 검증 한계
7. 수동 플레이테스트 안내
8. 남은 제약 또는 미구현 범위
9. Root 확인 필요 사항

## 7. Root 확인 필요 질문 목록

현재 구현 계획을 막는 치명적 공백은 발견하지 못했다. 다만 아래 항목은 구현 전 또는 구현 보고 시 Root 판단 대상으로 분리한다.

1. 상태 변화량을 내부 임시 점수로 구현하고 UI 기본 표시를 질적 구간/방향으로 제한해도 되는가?
2. 상세 토글에 임시 내부 숫자를 보여줘도 되는가, 아니면 Vertical Slice 1에서는 상세도 질적 표현만 보여줘야 하는가?
3. 첫 가문명, 초대 인물명, 후계자 이름을 계속 자리표시자로 둘지, VS1용 임시 중세 판타지 이름 풀을 Root가 제공할지 확인이 필요하다.
4. 세대별 5~7개 사건 기준을 deterministic smoke에서는 빠른 검증을 위해 축약하지 않고 그대로 돌릴지 확인이 필요하다.
5. 3회 이상 반복 플레이를 위해 실행별 seed 표시 UI가 필요한지, 아니면 보고서/스모크 stdout에만 seed를 남기면 충분한지 확인이 필요하다.
6. 3개 선택지 사건은 ActionBar 세로 버튼 목록으로 처리하는 계획에 문제가 없는지 확인이 필요하다.
7. 선택지별 고유 연대기 문장이 아직 없는 사건은 대표 연대기 문장 + 결과/태그/작위 변화 조합으로 처리하는 것이 Root 기준과 맞는지 확인이 필요하다.

## Project Claude 리뷰 질문

1. C# 정적 콘텐츠 데이터 방식이 현재 프로젝트 규모와 VS1 목적에 맞는가, 아니면 외부 데이터 파일을 도입해야 할 만큼 이식/검증 비용이 큰가?
2. 기존 `FamilyRunState`를 확장하는 방식과 VS1 전용 상태를 새로 두는 방식 중 어느 쪽이 현재 코드 변경 위험이 낮은가?
3. 46개 사건과 3개 선택지 사건을 기존 `ActionBar` 구조에서 작은 창 기준으로 감당할 수 있는가?
4. `--smoke`를 3회 deterministic 실행으로 확장할 때 Godot 헤드리스 시간과 출력량이 과하지 않은가?
5. 위 구현 단계 중 Project Claude가 멈춰야 할 기술적 충돌 또는 범위 초과가 있는가?
