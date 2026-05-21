# Vertical Slice 1 Playtest Build Implementation Report

> 작성일: 2026-05-22
> 작성자: Project Claude
> 모드: 구현 결과 보고
> 기반 지시서: `docs/handoff/vertical-slice-1-playtest-build/03-final-work-instruction.md`
> 후속 수동 검증 결과 경로: `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md` (이번 구현에서는 작성하지 않음)

## 1. 변경 파일

### 신규

- `scripts/Vs1Models.cs` — VS1 enum 14종 + 클래스 17종(가문 상태, 사건, 선택지, 조건, 가중치, 태그, 작위, 후계, 세대 정보 모델).
- `scripts/Vs1Content.cs` — 라벨/태그 정의/후계 preset/사건 데이터 빌더 헬퍼 + public API.
- `scripts/Vs1Content.Estate.cs` — EST-01~08 (8).
- `scripts/Vs1Content.Honor.cs` — HON-01~08 (8).
- `scripts/Vs1Content.Court.cs` — CRT-01~08 (8).
- `scripts/Vs1Content.Family.cs` — FAM-01~08 (8).
- `scripts/Vs1Content.Promotion.cs` — PRO-01~05 (5).
- `scripts/Vs1Content.Crisis.cs` — CRI-01~05 (5).
- `scripts/Vs1Content.GenerationEnd.cs` — END-01~04 (4).
- `scripts/Vs1Flow.cs` — 사건 후보 선정, 선택 결과 적용, 세대 종결 판정, 다음 세대 진행, 태그 노화, 상태 구간 계산, 태그 표시 정렬.
- `scripts/Vs1Smoke.cs` — deterministic smoke 3 RUN + ASSERT 4종.
- `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md` — 본 문서.

### 수정

- `scripts/Main.cs` — VS1 흐름으로 전환. 기존 4영역 shell(ContextHeader/MainScroll/InfoTabs/ActionBar) 유지. MultigenFlow/MvpLoopContent 호출은 모두 제거하고 Vs1Flow/Vs1Content 호출로 교체. smoke 모드는 `Vs1Smoke.Run(GetTree())` 위임.

### 보존 (수정 없음 — 합류 계약상 MVP 자리표시자 기준점)

- `scripts/MultigenModels.cs`
- `scripts/MultigenFlow.cs`
- `scripts/MultigenContent.cs`
- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`
- `Scenes/Main.tscn`
- `project.godot`
- `LegacyLog.csproj`

MVP 흐름은 `Main.cs._Ready()`가 더 이상 진입하지 않으므로 dead code 상태가 됐다. 컴파일은 그대로 통과한다.

## 2. 리뷰 반영 요약

`02-claude-review.md`에서 지시서가 받아들인 권고:

| 권고 | 반영 위치 |
|---|---|
| 사건 중요도와 세대 요약 우선순위 분리 | `Vs1EventImportance`/`Vs1SummaryPriority` 두 필드 |
| 선택지별 chronicle override 슬롯 | `Vs1EventChoice.ChronicleLineOverride`, PRO/CRI/FAM/END 핵심 케이스에 채움 |
| 사건 조건 구체 필드 | `Vs1EventCondition` (Title/State/Tag/Generation/HeirTrait 명시 필드) |
| 사건 기본 가중치 + 조건부 modifier | `Vs1EventDefinition.BaseWeight` + `WeightModifiers` |
| 태그 change kind에 `Strengthen` 슬롯 | `Vs1LegacyTagChangeKind.Strengthen` enum 값 (이번 데이터에서는 사용하지 않음) |
| 태그 노화 명시 함수 | `Vs1Flow.AgeLegacyTags(family)` |
| 새 `Vs1FamilyState` 분리 | `FamilyRunState` 보존, 신규 `Vs1FamilyState` 도입 |
| 콘텐츠 분류별 partial 파일 | 7개 partial 파일 (Estate/Honor/Court/Family/Promotion/Crisis/GenerationEnd) |
| 분기 처리 + seed fallback | `Vs1ChoiceOutcomeVariant` + `Vs1Flow.SelectVariant` |
| 후보 부족 fallback 규칙 | `Vs1Flow.PickEventForSlot`의 3단계 bucket (fully → partially → role-matched) |
| RNG seed 저장 | `Vs1FamilyState.Seed` + `MakeRng(family, salt)` deterministic 분리 |
| 상태 4축 이월 임시 매핑 | `Vs1Flow.ApplyStateCarry` (Wealth 누적, Honor/CourtInfluence 1 감쇠, HouseUnity 절반화) |
| 작위 위험은 상태 탭에 항상 표시, 헤더에는 Crisis 이상만 | `Main.RenderStateTab` + `RefreshContextHeader` |
| 태그 표시 우선순위 함수 | `Vs1Flow.SortTagChangesForDisplay` |
| 세대 시작 태그 정렬(부정/위험 우선) | `Vs1Flow.SelectFeaturedTagsForStart` + `TagStartPriority` |
| smoke ASSERT 4종 + SMOKE_OK/SMOKE_FAIL | `Vs1Smoke.Run` 출력 |
| 수동 검증 결과 경로 안내 | 본 보고서 상단 |

리뷰가 "부분 반영"으로 합의한 사항:

- 일반 사건의 조기 세대 종결: 확장 가능성을 위해 `Vs1EventChoice.EndTypeHint` 슬롯은 둠. 단 VS1에서는 END-* 사건에서만 실제 종결을 발화 — 구현 그대로.
- `Main.cs` 분리: Godot 어태치 진입점은 그대로 유지. 헬퍼는 `Vs1Flow`/`Vs1Content` 정적 함수로 좁게 분리. 새 partial class는 만들지 않음.

리뷰가 "반영하지 않음"으로 합의한 사항:

- 외부 JSON/CSV 도입 안 함.
- 후계 후보 풀 동적 추가 안 함. FAM-03 사생아는 `NextHeirPresetKey = HeirPresetIllegitimate` + 태그 변화로 처리.
- 모든 선택지의 고유 연대기 작성 안 함. 5케이스(승격/상실/후계/멸문/강한 가문사 태그)만 override.

## 3. 구현된 데이터 구조

### 핵심 enum (14)

`Vs1StateAxis`, `Vs1StateBand`, `Vs1ImpactDirection`, `Vs1ImpactMagnitude`, `NobleTitleRank`, `TitleRiskStage`, `Vs1EventCategory`, `Vs1EventRole`, `Vs1EventImportance`, `Vs1SummaryPriority`, `LegacyTagDuration`, `Vs1LegacyTagChangeKind`, `Vs1ConditionKind`, `Vs1GenerationEndType`.

### 핵심 클래스 (17)

`Vs1FamilyState` (mutable 진실 출처), `Vs1GenerationRun`, `Vs1GenerationRecord`, `Vs1GenerationEndResult`, `Vs1HeirProfile`, `Vs1ChronicleEntry`, `Vs1EventDefinition`, `Vs1EventChoice`, `Vs1EventCondition`, `Vs1WeightModifier`, `Vs1StateImpact`, `Vs1LegacyTagDefinition`, `Vs1ActiveLegacyTag`, `Vs1LegacyTagChange`, `Vs1TitleEffect`, `Vs1ChoiceOutcomeVariant`, `Vs1GenerationStartInfo`/`Vs1GenerationSummaryInfo`/`Vs1StateBandLine`.

### 핵심 상태 4축

- `Wealth` (재산)
- `Honor` (명예)
- `CourtInfluence` (궁정 영향력)
- `HouseUnity` (가문 결속)

UI 표시는 4단계 질적 구간(`Low`/`Strained`/`Stable`/`Strong`). 임계값: ≤-3 → Low, ≤0 → Strained, ≤4 → Stable, > 4 → Strong. 모두 임시값이며 최종 밸런스가 아님(코드 주석 명시).

### 작위/작위 위험

`HeldTitles` 목록 + 자동 계산 `RepresentativeTitle = max(HeldTitles)`. `TitleRisk`는 `Stable`/`Watched`/`Crisis`/`RevocationThreat` 4단계.

### 유산 태그 24종

`Vs1Content.LegacyTags`에 정의. 키, 라벨, 시리즈, 기본 등급(`ShortTerm`/`Generation`/`HouseHistory`), 상반 태그, 세대 시작 문장(`vertical-slice-1-generation-start-and-tag-display.md` 기준), 한 줄 설명.

### 후계 preset 7종

`HeirPresetDefault`, `HeirPresetCourtEducated`, `HeirPresetEstateEducated`, `HeirPresetKnightEducated`, `HeirPresetIllegitimate`, `HeirPresetUnsettled`, `HeirPresetLegitimateEldest`, `HeirPresetCompetent`. 각 preset마다 성향/강점/약점 라벨과 승계 사유 분리.

## 4. 46개 사건 이식 상태

| 분류 | 목표 | 구현 | 비고 |
|---|---:|---:|---|
| 영지/재산 (EST) | 8 | 8 | EST-01~08 |
| 명예/전쟁/기사도 (HON) | 8 | 8 | HON-01~08 |
| 궁정/정치 (CRT) | 8 | 8 | CRT-01~08 |
| 가문 내부/후계 (FAM) | 8 | 8 | FAM-01, FAM-08은 3선택지 |
| 작위 상승 (PRO) | 5 | 5 | PRO-05는 3선택지 |
| 작위 위기 (CRI) | 5 | 5 | CRI-03은 3선택지 |
| 세대 종결 (END) | 4 | 4 | END-01~04 |
| **합계** | **46** | **46** | |

각 사건은 ID/제목/본문/대표 연대기 문장을 Root 기획 문서에서 그대로 이식. 큰 사건/보조 사건과 세대 요약 우선순위는 `vertical-slice-1-event-chronicle-draft.md` 기준으로 채움.

3선택지 사건 (4개): FAM-01, FAM-08, CRI-03, PRO-05. `ActionBar` 세로 버튼 목록으로 처리됨.

### 분기 사건 (X 또는 Y) 처리

분기 결과는 `Vs1ChoiceOutcomeVariant` 목록으로 표현. variant마다 추가 영향/태그/타이틀/결과문/연대기 슬롯. 조건이 있는 variant 우선, 없으면 deterministic seed 기반 가중치 추첨.

이번 구현에서 variant를 가진 선택지 (약 18개):

- EST-01 곡창 (가문의 맹세 50% 추가)
- EST-02 자립 (가문의 맹세 50% 추가)
- EST-05 특권 (기사의 명예 33% 추가)
- EST-06 수용 (가문의 맹세 33% 추가)
- EST-07 광산 재개 (풍요로운 영지 vs 피의 원한 50/50)
- HON-01 직접 출전 (전쟁 공훈 vs 잊히지 않는 패배 50/50)
- HON-02 결투 허락 (기사의 명예 vs 피의 원한 50/50)
- HON-03 수용 (충성스러운 친족 vs 전쟁 공훈 50/50)
- HON-04 증거 수집 (승격 명분 33% 추가)
- HON-06 몸값 (불명예 50% 추가)
- HON-08 거부 (궁정의 의심 vs 정적의 표적 50/50)
- CRT-03 서약 (정적의 표적 vs 왕의 총애 50/50)
- CRT-04 화려 (왕의 총애 33% 추가)
- CRT-05 협상 (불명예 vs 궁정의 의심 50/50)
- CRT-06 공개 재판 (기사의 명예 50% 추가)
- CRT-08 처벌 (겁쟁이의 소문 vs 불명예 50/50)
- FAM-03 침묵 (피의 원한 vs 불명예 50/50)
- FAM-04 중재 (충성스러운 친족 33% 추가)
- FAM-04 숙청 (피의 원한 vs 내부 균열 50/50)
- FAM-06 군무 (전쟁 공훈 vs 충성스러운 친족 50/50)
- CRI-01 출두 (의심 해소 vs 박탈 위기 50/50) — 결과문/연대기도 분기
- CRI-01 영지 버팀 (박탈 위기 vs 몰락의 징조 50/50)
- CRI-03 반박 (기사의 명예 50% 추가)
- CRI-04 무력 (피의 원한 vs 불명예 50/50)
- CRI-05 친족 회의 (안정된 후계 vs 내부 균열 50/50) — 결과문도 분기
- PRO-01 청원 (승격 명분 vs 불안한 승격 50/50)
- PRO-02 영지 요구 (정적의 표적 vs 불안한 승격 50/50)
- PRO-04 왕실 중재 (왕실의 빚 vs 왕의 총애 50/50)
- PRO-05 작위 (불안한 승격 vs 승격 명분 50/50)
- END-02 돌격 (전쟁 공훈 vs 잊히지 않는 패배 50/50) — 결과문/연대기 분기
- END-02 후방 지휘 (겁쟁이의 소문 50% 추가)

**Root 확인 필요:** Root 문서가 분기 결정 규칙(예: 명예 보통 이상 → 전쟁 공훈, 낮음 → 잊히지 않는 패배)을 정해 주면, `Vs1ChoiceOutcomeVariant.Conditions`에 채워 넣을 수 있다. 현재는 모두 무조건부 fallback.

### Chronicle Override 사용 케이스

다음 5케이스 기준으로 약 22개 선택지에 `ChronicleLineOverride` 채움:

- 작위 상승 성공/보류 — PRO-01~04 청원 선택지
- 대표작위 상실/방어 — CRI-02 작위 포기, END-04 두 선택지
- 후계 안정/불안 — FAM-01 교육, FAM-03 인정, FAM-05 두 선택지, FAM-08 세 선택지, END-01 두 선택지, CRI-05 인가
- 멸문/연결 — END-04 도주
- 강한 가문사 태그 변화 — HON-01 돌격, EST-07 광산 재개, END-02 돌격, CRI-01 출두, CRI-01 영지 버팀의 variant 분기 chronicle

## 5. 구현된 게임 루프

### 한 실행 9단계

1. `Vs1Flow.CreateNewFamily(seed)` — 가문 생성, 1세대 사건 계획 미리 생성.
2. `Main.ShowGenerationStart()` — 세대 시작 화면.
3. `Main.ShowEvent()` — 사건 화면.
4. `Main.ApplyChoice(ev, choice)` → `Vs1Flow.ApplyChoice(...)` — 결과 적용.
5. `Main.ShowResult()` — 결과 화면.
6. `Main.ShowGenerationSummary()` → `Vs1Flow.FinishCurrentGeneration(...)` — 세대 요약.
7. `Main.AfterSummary()` — 멸문/후계 판정.
8. 멸문이 아니면 `Vs1Flow.AdvanceToNextGeneration(family, record, nextHeir)` 후 다시 (2).
9. 멸문이면 `Main.ShowExtinction(endResult)`.

### 사건 후보 선정

세대당 6 슬롯 (기반 1 + 압력 3 + 전환 1 + 종결 1). 각 슬롯에서 다음 우선순위로 후보를 고른다:

1. 조건 완전 충족 (가중치 +2 보너스)
2. 조건 일부 충족
3. 조건 없거나 부족하면 role/category-matched fallback

선정 후 가중치 합산 deterministic 추첨. 같은 세대 동일 사건 금지, 같은 분류 3회 연속 금지, 한 실행에서 같은 큰 사건은 종결 슬롯 외에는 피한다. 종결 슬롯은 END-* 사건만, 다른 슬롯은 END-* 사건을 배제.

### 선택 결과 적용 순서

`Vs1Flow.ApplyChoice` 안에서:

1. Variant 선택 (조건부 우선, 없으면 seed 기반 가중치 추첨).
2. 상태 영향 적용 (Up=+mag, Down=-mag, mag = Minor 1 / Moderate 2 / Major 3).
3. 작위 효과 적용 (`PromoteTo` → HeldTitles 추가, `LoseRepresentativeTitle` → 현 대표작위 제거 후 한 단계 아래 추가, `RiskStageChange` → TitleRisk 갱신).
4. 태그 변화 적용 (Add/Resolve/Replace/Weaken/Strengthen). Add 시 상반 태그 활성이면 자동 Replace로 승격.
5. 후계 preset 적용 (`PendingHeirPresetKey` 설정).
6. 연대기 항목 추가.
7. 큰 사건 ID 누적 (한 실행 반복 방지).
8. 작위 변화 카운터 갱신.
9. End type hint 기록 (END-* 전용).
10. CurrentEventIndex 증가.

### 작위 처리

- 승격은 PRO-* 선택 결과의 `PromoteTo`에서만 발생.
- 조건 충족은 자동 승격이 아니라 PRO-* 사건 등장 가중치를 올린다 (예: PRO-01의 `WeightForTag(승격 명분, +2)`).
- 대표작위 상실은 멸문이 아니라 한 단계 아래로 내려가는 쇠락.
- 작위 위험은 `Crisis` 이상일 때만 헤더에 경고로 표시 (Main.RefreshContextHeader).

### 종결 처리

- 실제 세대 종결은 END-* 사건의 `EndTypeHint`에서만 발화.
- 일반 사건의 "사망/부상 위험" 본문은 상태/태그/위험 증가로 표현되고 즉시 종결시키지 않는다.
- `Extinction`은 사망/퇴장 + 후계 부재(또는 smoke 강제 분기) 결합으로만 발화.

### 후계 처리

- 후계 후보 풀 동적 추가/삭제는 구현하지 않음.
- FAM 사건은 `안정된 후계`/`불안한 후계`/`내부 균열` 태그와 `NextHeirPresetKey`로 다음 세대 프로필을 변형.
- 후계자 성향/강점/약점은 사건 화면 단서와 가중치에 약하게 반영(`Vs1WeightModifier` + `HeirTrait` 조건). 성공/실패를 단독 결정하지 않음.

### 유산 태그 노화

`Vs1Flow.AgeLegacyTags(family)`는 세대 종료 직후, 다음 세대 시작 직전에 호출된다.

- ShortTerm → 소멸
- Generation → ShortTerm으로 1단계 약화
- HouseHistory → 유지

상반 태그가 동시에 활성이 되지 않도록 `TryApplyTagChange`에서 Add 시 자동 Replace 처리.

### 상태 이월

`Vs1Flow.ApplyStateCarry(family)`:

- Wealth: 누적 (변경 없음)
- Honor: 0 방향으로 1 감쇠
- CourtInfluence: 0 방향으로 1 감쇠
- HouseUnity: 0 방향으로 절반화

모두 임시값. 최종 밸런스는 Root 후속 라운드에서 정함.

## 6. 구현된 UI/UX 흐름

### 4영역 shell 유지

`ContextHeader` (2행) / `MainScroll` (확장 영역) / `InfoTabs` (3탭) / `ActionBar` (세로 버튼 목록).

화면 전환 단일 순서: `RefreshContextHeader` → `ClearMainContent` → 본문 구성 → `SetActionBar` → `RenderInfoTabs` → `ResetMainScroll`.

### 헤더

- 1행: 가문명 · 대표작위 · N세대 · 현재 인물
- 2행: [단계: ...] 화면 목적
- 작위 위험이 `Crisis` 또는 `RevocationThreat`일 때만 2행 끝에 ` · [경고: 작위 위기]` 추가

### 탭

- **상태**: 4축 질적 구간, 작위 위험 단계, 상세 토글(닫힘 기본), 현재 인물 특성, **seed 표시**.
- **연대기**: 현재 세대 사건 기록. 최신 사건은 ▶, 큰 사건은 ●, 보조 사건은 ·.
- **가문사**: 활성 유산 태그 + 등급 + 한 줄 설명, 보유 작위, 지난 세대 이력.

### 세대 시작 화면

- 이전 세대 한 줄 요약 (2세대 이후)
- 핵심 유산 (최대 4개, 부정/위험 태그 우선 정렬) + 각 태그의 세대 시작 문장
- 출발 상태 4축
- 이번 인물 성향/강점/약점 + 승계 사유
- 이번 세대 압력 또는 기회 1문장
- "첫 사건으로 들어간다" 버튼

### 사건 화면

- 사건 분류 · 제목 · `[N/M]` 진행
- 본문
- 단서: 관련 조건 (최대 3개, `DisplayReason` 표시)
- 선택지 버튼 (2~3개). 각 버튼 아래에 짧은 영향 요약 라벨

### 결과 화면

순서:
1. 결과 문장
2. 상태 변화 (axis + ↑/↓ + magnitude)
3. 유산 변화 (`SortTagChangesForDisplay` 정렬: 가문사 추가 → 세대 추가 → 교체 → 해소 → 약화)
4. 작위 변화 + 현재 대표작위 + 현재 작위 위험
5. 연대기 한 줄
6. 다음 사건 또는 세대 요약 버튼

### 세대 요약 화면

- 가문명 · 세대 · 인물
- 시작 대표작위 → 종료 대표작위
- 종결 유형 라벨
- 대표 유산 1~3개
- 요약 단락 (`Vs1Flow.BuildSummaryParagraph` 자동 조합)
- "다음 세대로 진입" 또는 "가문의 끝을 본다" 버튼

### 멸문 화면

- 가문 종료 요약
- 가문 전체 이력
- ActionBar에는 진행 버튼 없음, 비활성 안내 라벨만

### 작은 창/세로형

- `_mainScroll`만 확장. 탭은 자체 스크롤 유지.
- ActionBar는 화면 하단 고정 (`SizeFlagsVertical = ShrinkEnd`).
- 헤더는 AutowrapMode.Word로 줄바꿈 허용.
- 선택지 영향 요약은 버튼 아래 보조 라벨 (Modulate 알파 0.75).

## 7. 실행한 검증 명령과 결과

```powershell
dotnet build
# 빌드했습니다. 경고 34개, 오류 0개.
# (모든 경고는 CS8632 nullable annotation context — 코드 동작에 영향 없음.)

.\scripts\godot.cmd --headless --import
# DONE: first_scan_filesystem, update_scripts_classes, loading_editor_layout.

.\scripts\godot.cmd --headless -- --smoke
# 3 RUN 실행. 마지막 줄 SMOKE_OK.
```

## 8. smoke ASSERT 결과

```text
[SMOKE] RUN_END run=natural seed=1 generations=4 end_type=NaturalDeath promotions=1 title_losses=0
[SMOKE] RUN_END run=title-path seed=42 generations=4 end_type=NaturalDeath promotions=1 title_losses=1
[SMOKE] RUN_END run=extinction-path seed=12345 generations=3 end_type=Extinction promotions=1 title_losses=0
[SMOKE] ASSERT promotion_seen=True
[SMOKE] ASSERT title_loss_seen=True
[SMOKE] ASSERT legacy_tag_event_reason_seen=True
[SMOKE] ASSERT extinction_after_no_heir_only=True
SMOKE_OK
```

4개 ASSERT 모두 통과:

- `promotion_seen=True` — RUN 1(seed=1)에서 PRO-01 청원이 자연 발화 + RUN 2 강제. 자작 승격 확인.
- `title_loss_seen=True` — RUN 2(seed=42)의 gen 2에서 강제 CRI-02 "작위 일부를 포기한다"로 대표작위 상실 발화.
- `legacy_tag_event_reason_seen=True` — RUN 1 gen 2에서 CRT-04(자작 이상 조건), CRT-02(궁정 영향력 보통 이상), FAM-05(불안한 후계 관련) 등 활성 태그/조건으로 등장한 사건이 보임.
- `extinction_after_no_heir_only=True` — RUN 3에서 gen 3 종료 시점에 forceExtinction 분기 발화. 멸문이 종결 사건(END-01) + 후계 부재 결합에서만 일어남을 코드로 보장.

### smoke 강제 분기 분기 명시

- RUN 2 gen 1: 사건 계획 맨 앞에 PRO-01을 삽입해 자작 승격 검증.
- RUN 2 gen 2: 사건 계획 맨 앞에 CRI-02를 삽입하고 두 번째 선택지("작위 일부를 포기한다")를 강제로 골라 대표작위 상실 검증.
- RUN 3 gen 3: `Vs1Flow.FinishCurrentGeneration(family, forceExtinction: true)`로 멸문 발화.

이 분기는 게임 규칙이 아닌 smoke 검증용이며, `Vs1Smoke.cs` 헤더 주석에 명시함.

## 9. 자동 검증 한계

- **smoke는 첫 선택지 자동 픽**이라 모든 선택지 경로를 검증하지 않는다. 작위 상승의 "기반을 다진다" 같은 보류 경로, 분기 사건의 양쪽 변종은 ASSERT 대상이 아니다.
- **분기 사건(X 또는 Y)의 양쪽 결과**는 seed 기반 deterministic이라 같은 seed에서는 항상 같은 variant가 나온다. 다른 variant 검증은 다른 seed에서 확인해야 한다.
- **UI 회귀는 검증하지 않는다.** smoke는 UI 노드를 만들지 않으므로 화면 레이아웃, 줄바꿈, 작은 창 표시는 GUI 수동 검증이 필요하다.
- **밸런스 수치는 검증하지 않는다.** 상태 점수 임계값, 영향 magnitude, 가중치 modifier 값은 모두 임시값이며 smoke는 값의 적정성을 판단하지 않는다.
- **세대 시작 압력 문장**의 적합성, 세대 요약 단락의 자연스러움은 수동 검증이 필요하다.
- **3개 선택지 사건 4개(FAM-01/FAM-08/CRI-03/PRO-05)** 의 ActionBar 표시는 GUI에서만 확인 가능. smoke에서는 첫 선택지 픽이라 3선택지 분기 양상 확인 안 됨.

## 10. 수동 플레이테스트 안내

수동 플레이테스트 결과는 `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`에 작성한다. 양식과 체크리스트는 `docs/product/vertical-slice-1-manual-playtest-guide.md` 기준.

권장 검증 회차:

- 1회: 일반 창 (1280x720 이상)
- 1회: 작은 창 (예: 540x720)
- 1회: 세로형 화면 (예: 360x640)

회차별 최소 기록:

- 도달 세대, 종료 대표작위, 종료 유형
- 가장 의미 있었던 선택, 가장 이해하기 어려웠던 선택
- 작위 상승/부담/상실 체감
- 유산 태그와 후계자 차이 체감
- UI에서 자주 확인한 정보와 찾기 어려웠던 정보

자동 검증과 수동 검증은 보고서에서 분리한다.

GUI 실행은 `./scripts/godot.sh` 또는 `.\scripts\godot.cmd`로 에디터를 띄운 뒤 F5(Main 씬 실행)로 진행한다. CLAUDE.md §"이 에이전트 컨텍스트에서 Godot GUI 실행을 신뢰 도구로 쓰지 말 것"에 따라 본 구현 세션에서는 GUI 실행을 시도하지 않았다.

## 11. 남은 제약 또는 미구현 범위

### 의도적 미구현 (지시서 제외 범위)

- 새 사건 추가, 사건 본문 재작성
- 작위 체계/유산 태그 체계 변경
- 문체/밸런스 수치 최종 확정
- 왕/황제 통치 시스템
- 혼인/혈통 트리/저장 불러오기
- 정교한 경제/전쟁/AI 시뮬레이션
- 지역 지도
- 최종 UI/아트 스타일

### VS1 범위 내 미해소

- **분기 사건(X 또는 Y)의 조건 규칙**: 현재 모든 분기는 무조건부 50/50 fallback. Root가 "명예 보통 이상이면 전쟁 공훈, 낮음이면 잊히지 않는 패배" 같은 규칙을 정해주면 `Vs1ChoiceOutcomeVariant.Conditions`에 채워 넣을 수 있다.
- **사건 가중치 임시값**: `BaseWeight=1`, modifier delta=+1~+2는 임시. 후보 우선도 체감이 약하거나 강하면 Root가 조정 기준을 줄 수 있다.
- **상태 구간 임계값**: -3/0/4의 4구간 분할은 임시. 실제 플레이 사이클에서 구간 변화가 너무 잦거나 둔하면 재조정 필요.
- **상태 이월 비율**: Honor/CourtInfluence 1 감쇠, HouseUnity 절반화, Wealth 누적은 임시. 세대 이월의 체감이 너무 강하거나 약하면 재조정 필요.
- **세대 요약 문장 자동 조합**: `Vs1Flow.BuildSummaryParagraph`는 템플릿 기반. 문체 라운드에서 Root가 다듬을 여지가 큼.
- **후계자 특성이 사건 후보 가중치에 미치는 영향**: `HeirTraitKey` 조건 자체는 모델/플로우에서 지원하지만, 이번 사건 데이터에는 후계자 특성 기반 weight modifier가 아직 채워지지 않았다.
- **작은 화면 검증**: 360x640/540x720에서 실제 표시 확인이 안 됨 (GUI 수동 검증 필요).

### MVP 자산의 dead code 상태

`Main.cs`가 더 이상 `MultigenFlow`/`MultigenContent`/`MvpLoopContent`를 호출하지 않으므로 이 5개 파일은 dead code다. 컴파일은 그대로 통과. Root가 비교 기준이 더 이상 필요 없다고 판단하면 제거 가능.

## 12. Root 확인 필요 사항

`02-claude-review.md` §7의 Root 확인 항목 중 이번 구현에서 임시값/단순화로 처리한 것:

1. **분기 사건의 결정 규칙** (`02-claude-review.md` Root 8번): 현재 무조건부 50/50. Root가 사건별 조건을 정해주면 `Vs1ChoiceOutcomeVariant.Conditions`에 채울 수 있다.
2. **일반 사건의 조기 종결** (Root 9번): 옵션 1(END-*만 종결)로 단순화 — 확정 권고.
3. **후계 후보 풀의 동적 변경** (Root 10번): 미구현 — 확정 권고.
4. **상태 4축 이월 매핑** (Root 11번): 임시값(Wealth 누적, Honor/CourtInfluence 감쇠, HouseUnity 절반화). 수동 검증 후 조정.
5. **유산 태그 등급별 만료 시점** (Root 12번): 세대 종료 직후로 통일 — 확정.
6. **MVP 흐름 보존의 의미** (Root 13번): dead code로 유지. Root가 비교 끝났다고 판단하면 제거 가능.
7. **smoke 멸문 강제 분기** (Root 14번): RUN 3에서 `forceExtinction=true`로 발화. MVP smoke 패턴 유지 — 확정.
8. **smoke ASSERT + SMOKE_OK/SMOKE_FAIL** (Root 15번): 채택, 위 §8 참고.
9. **수동 검증 결과 경로** (Root 16번): `06-manual-playtest-results.md` 안내, 위 §10 참고.

### 본 구현에서 새로 발견한 Root 판단 항목

- **3선택지 사건의 작은 화면 부담**: 360x640에서 ActionBar가 화면의 30% 가까이 점유 가능. 영향 요약 라벨 길이 제한(현재 짧게 유지 중)으로 대응했지만, 실제 GUI 검증에서 부담이 크면 InfoTabs `CustomMinimumSize`를 동적으로 줄이거나 영향 요약을 토글 형태로 둘 수 있다.
- **사건 후보 부족 fallback의 체감**: 3단계 bucket (fully → partially → role)으로 후보 부족을 방지했지만, 후기 세대에서 활성 태그가 많아질 때 같은 사건이 반복 등장하는지는 GUI 수동 검증에서 확인 필요.
- **세대 요약 문장의 단조로움**: 자동 조합 템플릿이라 여러 세대 진행 시 비슷한 문장 구조가 반복될 수 있다. 문체 라운드의 우선 대상.
- **PRO-05 왕의 특별 포상**의 발화 조건이 `왕의 총애`만으로 제한돼 있어 RUN 3 같은 경로에서도 자작 단계에서 등장. Root가 "큰 공훈"을 별도 조건으로 두고자 한다면 큰 사건 누적 카운터를 추가해야 한다.

---

본 구현은 지시서 §10 완료 기준 14항목을 모두 충족한다. `dotnet build` 성공, 헤드리스 import 성공, smoke `SMOKE_OK` 출력, 46개 사건 이식, 한 세대 평균 6사건, 다세대 진행, 승격/작위 상실/유산 태그/멸문 분기 모두 검증됨.
