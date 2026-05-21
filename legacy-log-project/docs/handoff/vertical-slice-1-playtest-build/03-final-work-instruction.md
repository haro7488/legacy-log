# Vertical Slice 1 Playtest Build Final Work Instruction

> 작성일: 2026-05-22
> 작성자: Project Codex
> 모드: Project Claude 구현 요청
> 기반 리뷰: `docs/handoff/vertical-slice-1-playtest-build/02-claude-review.md`
> 구현 결과 보고 경로: `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`

## Project Claude 전달 문장

`docs/handoff/vertical-slice-1-playtest-build/03-final-work-instruction.md`를 읽고 구현 모드로 진행해 주세요. 지시서 범위를 벗어난 리팩터링이나 새 기획 확정은 하지 말고, 구현 결과를 `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`에 작성해 주세요.

## 1. 목적

Vertical Slice 1: 중세 판타지 귀족가문 반복 플레이 재미 검증 빌드를 구현한다.

이 빌드는 MVP 완료 여부를 다시 증명하는 빌드가 아니다. 목적은 46개 사건, 작위 상승/상실, 유산 태그, 후계자 특성, 세대 시작/요약 UI가 반복 플레이에서 서로 다른 가문사를 만드는지 확인 가능한 플레이어블 빌드를 만드는 것이다.

검증해야 할 핵심은 다음이다.

- 여러 번 플레이했을 때 서로 다른 가문사가 만들어지는가
- 남작에서 공작까지 작위 상승과 상실이 보상과 부담을 함께 만드는가
- 사건 선택이 재산, 명예, 궁정 영향력, 가문 결속에 의미 있는 변화를 만드는가
- 유산 태그가 다음 세대 사건 풀과 세대 시작 조건에 보이는가
- 후계자 특성이 같은 상태에서도 다른 운영 감각을 만드는가
- UI/UX가 작은 창과 세로형 화면에서도 판단을 방해하지 않는가

## 2. 수정 범위

### 수정 가능

- `scripts/Main.cs`
- 신규 `scripts/Vs1Models.cs`
- 신규 `scripts/Vs1Flow.cs`
- 신규 `scripts/Vs1Content.cs`
- 신규 `scripts/Vs1Content.Estate.cs`
- 신규 `scripts/Vs1Content.Honor.cs`
- 신규 `scripts/Vs1Content.Court.cs`
- 신규 `scripts/Vs1Content.Family.cs`
- 신규 `scripts/Vs1Content.Promotion.cs`
- 신규 `scripts/Vs1Content.Crisis.cs`
- 신규 `scripts/Vs1Content.GenerationEnd.cs`
- 필요 시 신규 `scripts/Vs1Smoke.cs`
- `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`

### 수정 가능하지만 가급적 보존

- `scripts/MultigenModels.cs`
- `scripts/MultigenFlow.cs`
- `scripts/MultigenContent.cs`
- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`

기존 MVP 흐름은 비교 기준으로 보존한다. VS1은 새 `Vs1FamilyState`와 VS1 전용 모델/흐름으로 구현한다. 기존 `FamilyRunState`를 대규모로 확장하지 않는다.

### 가능하면 변경하지 않음

- `Scenes/Main.tscn`
- `project.godot`

### 수정 금지

- 루트 `docs/product/` 기획 문서
- 루트 `docs/decisions/` 결정 문서
- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경

## 3. 제외 범위

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 문체 최종 확정
- 밸런스 수치 최종 확정
- 왕/황제 실제 통치 시스템
- 혼인/혈통 트리/저장 불러오기
- 정교한 경제 시스템
- 정교한 전쟁 시스템
- AI 귀족 세력 시뮬레이션
- 지역 지도
- 최종 UI/아트 스타일 확정
- 루트 기획 문서 수정

## 4. 사전 가정

- Root 기획 문서가 제품 기준의 source of truth다.
- 46개 사건은 Root 문서의 ID, 제목, 본문, 선택지, 영향 요약, 대표 연대기 문장을 그대로 사용한다.
- 상태 변화량은 최종 밸런스가 아니므로 내부 임시 점수로만 다룬다.
- 기본 UI에는 질적 구간과 변화 방향을 우선 표시한다.
- 상태 상세 토글에는 임시 내부 점수를 표시해도 된다. 단 토글은 닫힌 상태가 기본이다.
- 가문명, 초대 인물명, 후계자 이름은 이번 구현에서 자리표시자를 유지한다.
- 한 세대 5~7개 사건 기준은 smoke에서도 축약하지 않는다.
- seed 정보는 상태 탭 하단 또는 가문사/종료 화면에 노출한다. 헤더가 과밀해지면 헤더에는 넣지 않는다.
- 3개 선택지 사건은 기존 `ActionBar` 세로 버튼 목록으로 처리한다.
- 선택지별 고유 연대기는 모든 선택지에 작성하지 않는다. 리뷰에서 지적된 중요 케이스에만 선택지별 override 슬롯을 사용한다.
- 일반 사건의 부상/사망 위험은 실제 조기 세대 종결을 직접 발화하지 않는다. 실제 세대 종결은 END-* 사건에서만 처리한다.
- 후계 후보 풀은 이번 VS1에서 동적으로 추가하지 않는다. FAM 사건은 후계 안정/불안, 다음 세대 프로필, 유산 태그로 반영한다.
- 유산 태그 만료/노화는 세대 종료 직후, 다음 세대 시작 전 처리한다.
- deterministic smoke는 승격, 작위 위험/상실, 멸문 경로를 강제 검증하는 분기를 포함한다.
- `scripts/` 경로 케이스는 소문자로 유지한다.
- `Scenes/Main.tscn`의 `ext_resource path="res://scripts/Main.cs"`는 디스크 케이스와 일치하게 유지한다.
- 새 C# 스크립트는 씬에 직접 어태치하지 않는다.
- 사전 가정이 실제 코드와 맞지 않으면 추측하지 말고 멈춘 뒤 보고한다.

## 5. 리뷰 반영 요약

### 반영할 제안

- `IsMajor` bool만 두지 않고 사건 중요도와 세대 요약 우선순위를 분리한다.
- `Vs1EventChoice`에 선택지별 연대기 override 슬롯을 둔다.
- `Vs1EventCondition`은 단일 string key가 아니라 작위/상태/태그/세대/후계자 조건 필드를 명시한다.
- 사건 기본 가중치와 조건부 가중치 modifier를 둔다.
- 유산 태그 change kind에는 `Strengthen` 슬롯을 둔다. VS1에서 실제로 쓰지 않더라도 enum 자리는 둔다.
- 유산 태그 노화는 `Vs1Flow.AgeLegacyTags(...)`로 명시한다.
- 기존 `FamilyRunState`를 확장하지 않고 새 `Vs1FamilyState`를 둔다.
- 46개 사건 콘텐츠는 분류별 파일로 나눈다.
- 선택지 결과의 `X 또는 Y` 분기는 모델 슬롯을 만들고, 이번 구현에서는 deterministic seed 기반 fallback으로 처리한다.
- 사건 후보 부족 fallback 규칙을 명시한다.
- RNG seed를 `Vs1FamilyState`에 저장하고 smoke/GUI 양쪽에서 재현 가능하게 한다.
- 상태 4축 이월 임시 매핑을 둔다.
- 작위 위험은 상태 탭에 항상 표시하고, 헤더에는 위기 이상만 경고로 표시한다.
- 태그 변화 표시 우선순위를 `SortTagChangesForDisplay(...)`로 명시한다.
- 세대 시작 태그 정렬은 부정/위험 태그 우선, 상반 태그는 최신 태그 우선으로 처리한다.
- smoke에 ASSERT 줄과 `SMOKE_OK`/`SMOKE_FAIL`을 둔다.
- 수동 플레이테스트 결과 경로를 `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`로 안내한다.

### 부분 반영할 제안

- 일반 사건의 조기 종결 처리는 모델상 확장 가능성을 남기되, 이번 VS1에서는 END-* 종결 사건만 실제 종결을 만든다.
- `Vs1Content` 분할은 `partial static class`가 아니라 일반 `public static partial class Vs1Content`로 구현할 수 있다. 단 Godot 씬에 어태치하지 않으므로 SourceGenerator 경로 문제를 만들지 않는다.
- `Main.cs`를 완전히 분리하지 않는다. Godot에 어태치된 진입점은 유지하되, VS1 렌더링 보조는 static helper나 private 메서드로 좁게 분리할 수 있다.

### 반영하지 않을 제안

- 외부 JSON/CSV 데이터 파일 도입은 하지 않는다. VS1은 콘텐츠 로더 검증이 아니라 반복 플레이 재미 검증 빌드다.
- 후계 후보 풀의 동적 추가/삭제 시스템은 이번 범위에서 구현하지 않는다. 사생아/후계 불안 사건은 태그와 다음 세대 프로필 변형으로 처리한다.
- 모든 선택지의 고유 연대기 문장은 작성하지 않는다. Root 문서가 플레이테스트 후 확장으로 둔 범위다.

## 6. 구현 단계

### 6.1 VS1 모델 추가

`scripts/Vs1Models.cs`를 만든다.

포함할 핵심 enum:

- `Vs1StateAxis`: `Wealth`, `Honor`, `CourtInfluence`, `HouseUnity`
- `Vs1StateBand`: `Low`, `Strained`, `Stable`, `Strong`
- `Vs1ImpactDirection`: `Down`, `Up`
- `Vs1ImpactMagnitude`: `Minor`, `Moderate`, `Major`
- `NobleTitleRank`: `Baron`, `Viscount`, `Count`, `Marquess`, `Duke`
- `TitleRiskStage`: `Stable`, `Watched`, `Crisis`, `RevocationThreat`
- `Vs1EventCategory`: `Estate`, `Honor`, `Court`, `Family`, `Promotion`, `Crisis`, `GenerationEnd`
- `Vs1EventRole`: `Foundation`, `Pressure`, `TurningPoint`, `GenerationEnd`
- `Vs1EventImportance`: `Minor`, `Major`
- `Vs1SummaryPriority`: `Low`, `Medium`, `High`
- `LegacyTagDuration`: `ShortTerm`, `Generation`, `HouseHistory`
- `Vs1LegacyTagChangeKind`: `Add`, `Resolve`, `Replace`, `Weaken`, `Strengthen`
- `Vs1ConditionKind`: `Title`, `StateBand`, `ActiveTag`, `MissingTag`, `Generation`, `HeirTrait`
- `Vs1GenerationEndType`: `NaturalDeath`, `BattleDeath`, `IllnessDeath`, `ForcedAbdication`, `Extinction`, `Other`

포함할 핵심 클래스:

- `Vs1FamilyState`
- `Vs1GenerationRun`
- `Vs1GenerationRecord`
- `Vs1GenerationEndResult`
- `Vs1HeirProfile`
- `Vs1EventDefinition`
- `Vs1EventChoice`
- `Vs1EventCondition`
- `Vs1WeightModifier`
- `Vs1StateImpact`
- `Vs1LegacyTagDefinition`
- `Vs1ActiveLegacyTag`
- `Vs1LegacyTagChange`
- `Vs1TitleEffect`
- `Vs1ChoiceOutcomeVariant`
- `Vs1GenerationStartInfo`
- `Vs1GenerationSummaryInfo`

모델 요구:

- `Vs1FamilyState`는 seed, 가문명, 현재 세대, 현재 인물, 핵심 상태 4축 내부 점수, 보유 작위, 대표작위, 작위 위험 단계, 활성 유산 태그, 현재 세대 run, 끝난 세대 기록, 멸문 여부를 가진다.
- `RepresentativeTitle`은 보유 작위 중 가장 높은 작위로 계산한다.
- `Vs1EventChoice`는 `ChronicleLineOverride`를 nullable string으로 가진다.
- `Vs1EventChoice`는 deterministic 분기용 `OutcomeVariants`를 가진다. variant가 없으면 기본 결과를 적용한다.
- `Vs1EventCondition`은 작위/상태/태그/세대/후계자 조건을 구체 필드로 가진다. 내부 string encoding에 의존하지 않는다.

### 6.2 VS1 콘텐츠 이식

다음 파일을 만든다.

- `scripts/Vs1Content.cs`
- `scripts/Vs1Content.Estate.cs`
- `scripts/Vs1Content.Honor.cs`
- `scripts/Vs1Content.Court.cs`
- `scripts/Vs1Content.Family.cs`
- `scripts/Vs1Content.Promotion.cs`
- `scripts/Vs1Content.Crisis.cs`
- `scripts/Vs1Content.GenerationEnd.cs`

`Vs1Content.cs`는 public API를 제공한다.

- `AllEvents`
- `LegacyTags`
- `GetEventById(string id)`
- `GetStateLabel(Vs1StateAxis axis)`
- `GetTitleLabel(NobleTitleRank rank)`
- `GetTitleRiskLabel(TitleRiskStage stage)`
- `GetLegacyTagLabel(string key)`
- `BuildInitialFamily(int seed)`
- 필요 시 `GetHeirTraitLabel(string key)`

분류별 파일은 Root 문서의 사건을 그대로 이식한다.

이식 기준:

- 사건 ID, 제목, 본문은 `vertical-slice-1-event-copy-draft.md` 기준
- 선택지 행동명과 영향 요약은 `vertical-slice-1-event-copy-draft.md` 기준
- 사건 분류와 등장 조건은 `vertical-slice-1-event-list.md` 기준
- 큰 사건/보조 사건, 요약 우선순위, 대표 연대기 문장은 `vertical-slice-1-event-chronicle-draft.md` 기준
- 태그 변화는 `vertical-slice-1-event-tag-change-table.md` 기준
- 태그 정의와 상반 관계, 세대 시작 문장은 `vertical-slice-1-legacy-tag-rules.md`, `vertical-slice-1-generation-start-and-tag-display.md` 기준
- 작위 효과와 작위 위험은 `vertical-slice-1-title-progression-rules.md` 기준

사건 본문을 재작성하지 않는다.

선택지 결과의 `X 또는 Y` 분기는 다음 원칙으로 구현한다.

- `Vs1ChoiceOutcomeVariant` 목록에 후보 결과를 둔다.
- variant에 Root 조건이 명확한 경우만 조건 필드를 둔다.
- Root 조건이 없는 경우 deterministic seed 기반 fallback으로 variant를 고른다.
- 보고서에 "분기 사건 일부는 seed 기반 fallback으로 처리"라고 명시한다.

### 6.3 VS1 흐름 구현

`scripts/Vs1Flow.cs`를 만든다.

public 진입점 후보:

- `CreateNewFamily(int? seed = null)`
- `BuildGenerationStartInfo(Vs1FamilyState family)`
- `BuildGenerationPlan(Vs1FamilyState family)`
- `GetCurrentEvent(Vs1FamilyState family)`
- `ApplyChoice(Vs1FamilyState family, Vs1EventDefinition ev, Vs1EventChoice choice)`
- `FinishCurrentGeneration(Vs1FamilyState family)`
- `BuildGenerationRecord(Vs1FamilyState family, Vs1GenerationEndResult endResult)`
- `AdvanceToNextGeneration(Vs1FamilyState family, Vs1HeirProfile heir)`
- `AgeLegacyTags(Vs1FamilyState family)`
- `BuildStateBand(Vs1FamilyState family, Vs1StateAxis axis)`
- `BuildDetailedStateLines(Vs1FamilyState family)`
- `SortTagChangesForDisplay(...)`

사건 후보 선정:

1. 세대 슬롯을 만든다: 기반 1~2, 압력 2~3, 전환 0~2, 종결 1
2. 조건 완전 충족 후보를 우선 고른다.
3. 부족하면 조건 일부 충족 후보를 쓴다.
4. 그래도 부족하면 EST/HON/CRT/FAM 중 현재 세대에 적게 나온 분류에서 fallback으로 고른다.
5. 같은 세대 동일 사건 반복 금지.
6. 같은 분류 3회 이상 연속 금지.
7. 한 실행에서 같은 큰 사건 반복은 가능하면 피한다.
8. END-* 사건은 마지막 슬롯으로만 배치한다.

상태 내부 점수:

- `Minor` = 1
- `Moderate` = 2
- `Major` = 3
- `Up`은 더하고 `Down`은 뺀다.

이 수치는 임시값이다. 코드 주석에 최종 밸런스가 아님을 명시한다.

상태 이월 임시 매핑:

- `Wealth`: 누적
- `Honor`: 0 방향으로 1 감쇠
- `CourtInfluence`: 0 방향으로 1 감쇠
- `HouseUnity`: 0 방향으로 절반화

작위 처리:

- 승격은 PRO-* 선택 결과에서만 직접 발생한다.
- 조건 충족은 자동 승격이 아니라 PRO-* 사건 등장 가중치를 올린다.
- 대표작위 상실은 멸문이 아니다.
- 작위 위험 단계가 `Crisis` 이상이면 헤더 경고 후보가 된다.

종결 처리:

- 실제 세대 종결은 END-* 사건에서만 처리한다.
- 일반 사건의 "사망/부상 위험"은 상태/태그/위험 증가로 표현하고 즉시 종결시키지 않는다.
- `Extinction`은 END-* 결과 자체가 아니라 후계 부재와 사망/퇴장 조건 결합으로 발화한다.

후계 처리:

- 후계 후보 풀 동적 추가/삭제 시스템은 구현하지 않는다.
- FAM 사건은 `안정된 후계`, `불안한 후계`, `내부 균열` 등 태그와 다음 세대 프로필 preset에 반영한다.
- 후계자 성향/강점/약점은 사건 후보 가중치와 사건 화면 단서에 약하게 반영한다.
- 후계자 특성은 성공/실패를 단독 결정하지 않는다.

유산 태그 처리:

- 상반 태그는 동시에 오래 유지하지 않는다.
- 새 결과가 기존 활성 태그와 반대면 `Replace`로 처리한다.
- 교체/해소/추가는 결과 화면에 표시한다.
- 세대 시작 화면에는 최대 4개만 노출한다.
- 부정/위험 태그를 먼저 고른다.
- 나머지는 가문사 탭 상세에 둔다.

### 6.4 Main UI 연결

`scripts/Main.cs`를 VS1 흐름으로 전환한다.

기존 4영역 shell 계약은 유지한다.

화면 전환 순서:

1. `RefreshContextHeader(...)`
2. `ClearMainContent()`
3. 본문 구성
4. `SetActionBar(...)`
5. `RenderInfoTabs()`
6. `ResetMainScroll()`

화면별 요구:

세대 시작 화면:

- 가문명, 대표작위, 세대, 현재 인물
- 핵심 유산 태그 2~4개
- 이전 세대 한 줄 요약
- 이번 세대 출발 상태 4축
- 현재 인물 성향/강점/약점
- 이번 세대 압력 또는 기회
- `첫 사건으로 들어간다` 버튼

사건 화면:

- 사건 분류와 성격
- 사건 제목과 본문
- 관련 상태
- 관련 작위 위험
- 관련 유산 태그
- 후계자 단서
- 선택지 버튼 2~3개
- 선택지 영향 요약은 짧게 표시한다. 너무 길면 버튼 아래 보조 라벨로 분리한다.

결과 화면:

1. 결과 문장
2. 상태 변화 요약
3. 태그 변화
4. 작위 변화 또는 작위 위험 변화
5. 연대기 기록
6. 다음 행동 버튼

세대 요약 화면:

- 세대 번호
- 인물명
- 시작 대표작위와 종료 대표작위
- 종결 유형
- 대표 유산 태그 1~3개
- 세대 요약 단락
- 다음 세대 시작 또는 가문사 종료 버튼

멸문 화면:

- 가문 종료 요약
- 전체 가문사 궤적
- 진행 버튼 없음
- 비활성 안내 라벨만 ActionBar에 둔다.

탭:

- `상태`: 상태 4축 질적 구간, 작위 위험 단계, seed, 상세 토글
- `연대기`: 현재 세대 사건 기록, 최신 기록 강조
- `가문사`: 활성 유산 태그, 지난 세대 이력, 작위 이력, 전체 태그 목록

헤더:

- 대표작위는 항상 표시한다.
- 작위 위험은 `Crisis` 이상일 때만 헤더 경고로 표시한다.
- `Stable`/`Watched` 단계는 상태 탭에 표시한다.

### 6.5 Smoke 구현

기존 `--smoke` 인자는 유지하되 VS1 smoke로 동작하게 한다.

권장 구현:

- `scripts/Vs1Smoke.cs`에 smoke 흐름을 분리한다.
- `Main.RunSmoke()`는 `Vs1Smoke.Run()`을 호출한다.
- UI 노드는 smoke 경로에서 만들지 않는다.

Smoke는 최소 3개 실행을 수행한다.

- RUN 1: 자연 진행 중심
- RUN 2: 승격/작위 위험 또는 대표작위 상실 경로 강제 검증
- RUN 3: 멸문 경로 강제 검증

강제 검증은 실제 게임 규칙 확정이 아니라 smoke 검증용이다. 코드 주석과 보고서에 명시한다.

stdout anchor:

```text
[SMOKE] RUN seed=...
[SMOKE] GEN_START gen=... title=... risk=... tags=...
[SMOKE] EVENT id=... category=... reason=...
[SMOKE] CHOICE ...
[SMOKE] RESULT state_changes=... tag_changes=... title_change=...
[SMOKE] GEN_SUMMARY gen=... events=... end=...
[SMOKE] SUCCESSION candidates=...
[SMOKE] RUN_END run=... seed=... generations=... end_type=... promotions=... title_losses=...
[SMOKE] ASSERT promotion_seen=true
[SMOKE] ASSERT title_loss_seen=true
[SMOKE] ASSERT legacy_tag_event_reason_seen=true
[SMOKE] ASSERT extinction_after_no_heir_only=true
SMOKE_OK
```

ASSERT 실패 시:

```text
SMOKE_FAIL
```

하나라도 ASSERT가 실패하면 `SMOKE_OK`를 출력하지 않는다.

### 6.6 구현 보고 작성

`docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`를 작성한다.

보고서에는 다음을 포함한다.

1. 변경 파일
2. 구현된 데이터 구조
3. 46개 사건 이식 상태
4. 구현된 게임 루프
5. 구현된 UI/UX 흐름
6. 실행한 검증 명령과 결과
7. smoke ASSERT 결과
8. 자동 검증 한계
9. 수동 플레이테스트 안내
10. 남은 제약 또는 미구현 범위
11. Root 확인 필요 사항

수동 플레이테스트 결과는 추후 `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`에 기록하도록 안내한다. 구현 세션에서 이 파일을 반드시 작성할 필요는 없다.

## 7. 병렬 작업 분해

### A. 모델/흐름

소유 파일:

- `scripts/Vs1Models.cs`
- `scripts/Vs1Flow.cs`

책임:

- VS1 상태, 사건, 작위, 태그, 후계 모델
- 사건 후보 선정
- 선택 결과 적용
- 세대 시작/요약 데이터 생성
- 태그 노화
- 상태 이월
- 작위/멸문 판정

### B. 콘텐츠 이식

소유 파일:

- `scripts/Vs1Content.cs`
- `scripts/Vs1Content.Estate.cs`
- `scripts/Vs1Content.Honor.cs`
- `scripts/Vs1Content.Court.cs`
- `scripts/Vs1Content.Family.cs`
- `scripts/Vs1Content.Promotion.cs`
- `scripts/Vs1Content.Crisis.cs`
- `scripts/Vs1Content.GenerationEnd.cs`

책임:

- Root 문서의 46개 사건 이식
- 유산 태그 정의 이식
- 작위/상태/태그/후계자 표시 라벨 제공

### C. UI 연결

소유 파일:

- `scripts/Main.cs`

책임:

- VS1 흐름을 기존 4영역 UI shell에 연결
- 화면별 정보 우선순위 반영
- 상태/연대기/가문사 탭 확장
- 작은 창과 세로형 화면에서 ActionBar, 탭, 본문이 과밀하지 않도록 구성

### D. Smoke/보고

소유 파일:

- 필요 시 `scripts/Vs1Smoke.cs`
- `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`

책임:

- deterministic smoke 구현
- ASSERT anchor 출력
- 검증 명령 실행
- 구현 결과 보고 작성

### 합류 순서

1. A의 모델 이름과 필드를 먼저 고정한다.
2. B는 A의 모델을 기준으로 사건을 채운다.
3. C는 A/B의 public API만 호출한다.
4. D는 A/B/C 통합 후 작성한다.

### 병렬화하지 말아야 할 지점

- `Main.cs`와 `Vs1Flow.cs` 사이의 화면 전환 계약은 통합해서 검증한다.
- 사건 데이터 필드명을 바꾸는 작업과 46개 사건 전체 이식은 동시에 진행하지 않는다.
- Root 문서에 없는 새 사건/새 규칙을 병렬 작업자가 임의로 채우지 않는다.

## 8. Godot 경로 케이스 계약

- 기존 씬: `Scenes/Main.tscn`
- 기존 어태치 스크립트: `scripts/Main.cs`
- 신규 스크립트는 모두 기존 `scripts/` 디렉터리 아래에 둔다.
- `Scenes/Main.tscn`의 `ext_resource path="res://scripts/Main.cs"`는 변경하지 않는다.
- 새 C# 스크립트를 씬에 직접 어태치하지 않는다.
- `Scripts/` 같은 새 대문자 변형 디렉터리를 만들지 않는다.
- `.cmd`/`.bat` 파일은 건드리지 않는다.

## 9. 검증 명령

최종 검증:

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

검증 반복 중 C#만 변경한 경우:

```powershell
dotnet build
.\scripts\godot.cmd --headless -- --smoke
```

리소스/씬 파일을 변경하지 않았다면 반복 중 `--import`는 매번 실행하지 않아도 된다. 최종 보고 전에는 전체 3단계를 실행한다.

## 10. 완료 기준

- 46개 사건이 VS1 콘텐츠 데이터에 포함된다.
- 사건 본문과 선택지 문구는 Root 문서에서 재작성 없이 이식된다.
- 한 세대는 보통 5~7개 상호작용 사건으로 진행된다.
- 한 실행에서 여러 세대를 진행할 수 있다.
- 상태 4축이 UI와 선택 결과에 반영된다.
- 승격 기회가 자동 보상이 아니라 선택 사건으로 등장한다.
- 작위 상실 또는 작위 위험이 즉시 게임오버가 아닌 쇠락 단계로 표시된다.
- 유산 태그가 결과 화면과 다음 세대 시작 화면에 보인다.
- 후계자 성향/강점/약점이 세대 시작 화면과 사건 단서/가중치에 약하게 반영된다.
- 멸문은 후계 부재와 사망/퇴장 결합일 때만 발생한다.
- 작은 창과 세로형 화면에서 수동 확인할 수 있는 UI 구조가 유지된다.
- `dotnet build`가 성공한다.
- Godot headless import가 성공한다.
- `--smoke`가 `SMOKE_OK`로 끝난다.
- `04-implementation-report.md`가 작성된다.

## 11. 보고 형식

`04-implementation-report.md`에 다음 순서로 작성한다.

1. 변경 파일
2. 리뷰 반영 요약
3. 구현된 데이터 구조
4. 46개 사건 이식 상태
5. 구현된 게임 루프
6. 구현된 UI/UX 흐름
7. 실행한 검증 명령과 결과
8. smoke ASSERT 결과
9. 자동 검증 한계
10. 수동 플레이테스트 안내
11. 남은 제약 또는 미구현 범위
12. Root 확인 필요 사항

## 12. 멈춤 지점

다음 상황에서는 추측으로 진행하지 말고 구현을 멈춘 뒤 보고한다.

- Root 문서의 사건 본문/선택지/태그 표가 서로 충돌해 같은 사건을 이식할 수 없는 경우
- 46개 사건 중 필수 필드가 부족해 컴파일 가능한 콘텐츠 데이터를 만들 수 없는 경우
- 기존 `Main.cs` 4영역 UI shell을 유지한 채 VS1 화면을 붙일 수 없는 경우
- Godot 경로 케이스 문제로 새 C# 파일이 import/build에서 불안정한 경우
- 멸문 조건을 후계 부재 + 사망/퇴장 결합으로 구현할 수 없는 구조적 문제가 있는 경우
- 새 기획 결정 없이는 선택지 분기/후계/작위 결과를 표현할 수 없는 경우

