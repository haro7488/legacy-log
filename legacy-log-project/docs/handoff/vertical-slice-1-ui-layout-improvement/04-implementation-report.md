# Vertical Slice 1 UI Layout Improvement - Implementation Report

> 작성일: 2026-05-22
> 작성자: Project Claude
> 모드: 구현 결과 보고
> 기반 지시서: `docs/handoff/vertical-slice-1-ui-layout-improvement/03-final-work-instruction.md`
> 기반 리뷰: `docs/handoff/vertical-slice-1-ui-layout-improvement/02-claude-review.md`

## 1. 구현 요약

`scripts/Main.cs`를 3단 UI 구조(상단 헤더 / 중간 단일 게임 화면 / 하단 탭 메뉴)로 전환했다. 기존 `InfoTabs` 보조 패널과 `ActionBar`를 제거하고, 하단 `사건 / 상태 / 연대기 / 가문사` 4탭 주 화면 전환 메뉴로 대체했다. 사건 선택지와 진행 버튼은 `GameContent` 내부 마지막 행동 영역에 배치한다.

리뷰가 가장 강조한 mutation/render 분리 계약을 지키기 위해, `Render*` 함수는 모두 `_family` 상태를 변경하지 않는 읽기 전용 렌더링으로 만들었다. `Vs1Flow.ApplyChoice`, `FinishCurrentGeneration`, `AdvanceToNextGeneration` 같은 mutation은 명시적 진행 함수(`ApplyChoiceAndShowResult`, `FinishGenerationAndShowSummary`, `ContinueAfterSummary`)에서만 호출된다.

`Vs1Flow`, `Vs1Content`, `Vs1Models`, `Vs1Smoke`는 변경하지 않았다. 게임 규칙, 사건 데이터, 작위/태그/상태 체계는 모두 그대로다.

## 2. 변경 파일 목록

### 수정

- `scripts/Main.cs` — 4영역 셸 → 3영역 셸 전환, 탭 상태 + 사건 흐름 상태 분리, mutation/render 분리, 모든 렌더링 경로를 `GameContent` 단일 컨테이너로 통합.

### 변경 없음

- `scripts/Vs1Models.cs`
- `scripts/Vs1Content.cs` + 7개 분류별 partial 파일
- `scripts/Vs1Flow.cs`
- `scripts/Vs1Smoke.cs`
- `scripts/Multigen*.cs` (보존된 MVP 자리표시자)
- `scripts/MvpLoop*.cs` (보존된 MVP 자리표시자)
- `Scenes/Main.tscn`
- `project.godot`
- `LegacyLog.csproj`

## 3. 리뷰 반영 사항

### 완전 반영

| 리뷰 권고 | 반영 위치 |
|---|---|
| mutation/render 분리 계약 (§2.1) | `RefreshAll`, `Render*Screen`은 읽기 전용. `ApplyChoiceAndShowResult`/`FinishGenerationAndShowSummary`/`ContinueAfterSummary`만 mutation. 파일 상단 주석에 계약 명시. |
| 헤더 2행 정책 옵션 C (§2.2) | Event 탭: `[단계: …] …`, 그 외 탭: `[화면: …] …`. `RefreshContextHeader()` 안 switch. |
| 활성 탭 표시 = Disabled + 텍스트 접두 (§2.3) | `ConfigureTab(button, label, isActive)`: 활성 시 `Text = "> {label}"`, `Disabled = true`. |
| `_pendingEnd` 중복 호출 방지 | `FinishGenerationAndShowSummary` 안에 `if (_pendingEnd == null)` 보호 가드. |
| 단일 갱신 진입점 `RefreshAll()` | 헤더 갱신 + 탭 표시 갱신 + 화면 렌더링 + 스크롤 리셋을 한 함수로 통합. |
| Phase 1~8 명세 | 단일 파일이라 phase별 커밋 분리는 하지 않았지만, 코드 구조는 phase 순서대로 정렬되어 있다(enum → 필드 → 셸 → 진입/dispatch → 화면 렌더 → 진행/mutation → 헬퍼). |

### 부분 반영

- 탭 라벨 축약: 360x640 검증을 실행 환경 제약상 수동으로 못 했으므로 라벨 축약이 필요한지 확정 불가. Root 기준 라벨(`사건`/`상태`/`연대기`/`가문사`) 유지. 활성 표시는 `> 사건` 5글자, 비활성은 3~4글자라 `ClipText = false`로 잘림을 막고 `SizeFlagsHorizontal = ExpandFill`로 균등 분할.
- 멸문 후 자동 탭 전환: 구현하지 않음. 사용자가 직접 가문사 탭으로 이동.

### 부분 반영 — 스크롤 정책 단순화

- 리뷰는 "토글 시 스크롤 유지 vs 리셋" 옵션 선택을 권고. 03 지시서는 "단순성을 위해 상단으로 이동해도 된다"고 했고, 구현은 `ToggleStateDetail` → `RefreshAll()` → `ResetScroll()`로 매번 스크롤 리셋한다. 토글 시 시각 점프가 발생하지만 일관성 우선.

## 4. UI 구조 변경 요약

### Before (4영역)

```text
ContextHeader (2행)
─────────────────────
MainScroll (사건/결과/세대 시작/세대 요약/멸문 본문)
─────────────────────
InfoTabs (상태/연대기/가문사 — 보조 패널)
─────────────────────
ActionBar (선택지/진행 버튼)
```

### After (3영역)

```text
ContextHeader (2행)
─────────────────────
GameScroll → GameContent
  (선택된 탭의 전체 내용)
  - 사건 탭: 세대 시작/사건/결과/세대 요약/멸문 + 행동 영역(선택지/진행 버튼)
  - 상태 탭: 4축 상태/작위 위험/현재 인물/상세 토글/seed
  - 연대기 탭: 최신 기록/큰 사건/보조 사건
  - 가문사 탭: 활성 유산 태그/보유 작위/지난 세대 이력
─────────────────────
BottomTabs (HBoxContainer + 4개 Button: 사건/상태/연대기/가문사)
```

### 노드 변경

| 영역 | Before | After |
|---|---|---|
| 헤더 | `_contextHeader` + 2 Label | 동일 |
| 본문 | `_mainScroll` / `_mainContent` (VBoxContainer) | `_gameScroll` / `_gameContent` (VBoxContainer, 동일 구조 다른 이름) |
| 정보 패널 | `_infoTabs` (TabContainer) + 3개 자식 컨테이너 | 제거 |
| 행동 영역 | `_actionBar` (VBoxContainer) | 제거. 행동 버튼은 `_gameContent` 내부에 직접 추가 |
| 하단 탭 | — | `_bottomTabsContainer` (HBoxContainer) + 4개 `Button` 필드 |

### 상태 변수 변경

| 상태 | Before | After |
|---|---|---|
| 사건 흐름 | `Show*()` 함수가 호출 순서로 결정 (암묵적) | `_eventScreen` enum(`GenerationStart`/`Event`/`Result`/`GenerationSummary`/`Extinction`) 명시 |
| 선택 탭 | — (`InfoTabs.CurrentTab`이 보조 정보) | `_selectedTab` enum(`MainTab.Event`/`State`/`Chronicle`/`FamilyHistory`) |
| 최근 적용 결과 | `_lastApplyResult`, `_lastEvent`, `_lastChoice` | 동일 (이름만 `_lastAppliedEvent`/`_lastAppliedChoice`로 변경) |
| 세대 종결 결과 | `_pendingEnd` | 동일. 더 명시적인 보호 가드(`if (_pendingEnd == null)`)로 중복 호출 방지 |
| 상태 상세 토글 | `_stateDetailVisible` | 동일 |

### 호출 흐름 변경

| 흐름 | Before | After |
|---|---|---|
| 첫 화면 진입 | `_Ready()` → `ShowGenerationStart()` | `_Ready()` → `_selectedTab = Event`, `_eventScreen = GenerationStart` → `RefreshAll()` |
| 첫 사건 진입 | `ShowGenerationStart()` 안의 startButton → `ShowEvent()` | `GoToFirstEvent()` (mutation 함수: `_eventScreen = Event` → `RefreshAll()`) |
| 선택지 적용 | `ApplyChoice(choice)` → `ShowResult()` | `ApplyChoiceAndShowResult(ev, choice)`: `Vs1Flow.ApplyChoice` → 결과 저장 → `_eventScreen = Result` → `RefreshAll()` |
| 다음 사건 | `ShowResult()` 안의 nextButton → `ShowEvent()` 또는 `ShowGenerationSummary()` | nextButton → `GoToNextEvent()` 또는 `FinishGenerationAndShowSummary()` |
| 세대 종결 mutation | `ShowGenerationSummary()` 진입 시점에 호출 (renderer 안 mutation) | `FinishGenerationAndShowSummary()` 핸들러에서 한 번만 호출, `_pendingEnd`에 저장 |
| 다음 세대 진입 | `AfterSummary()` → `Vs1Flow.AdvanceToNextGeneration` → `ShowGenerationStart()` | `ContinueAfterSummary()`: `AdvanceToNextGeneration` 후 `_eventScreen = GenerationStart` → `RefreshAll()` |
| 멸문 진입 | `AfterSummary()` → `ShowExtinction()` | `ContinueAfterSummary()`: 멸문 시 `_eventScreen = Extinction` → `RefreshAll()` |
| 탭 전환 | (없음) | `SelectTab(MainTab)` → `RefreshAll()` |

## 5. 자동 검증 결과

```powershell
dotnet build
# 빌드했습니다. 오류 0개.

.\scripts\godot.cmd --headless --import
# DONE: first_scan_filesystem, loading_editor_layout.

.\scripts\godot.cmd --headless -- --smoke
# 3 RUN 실행. 마지막 줄 SMOKE_OK.
```

### Smoke ASSERT 결과

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

이전 VS1 빌드 단계의 smoke 출력과 동일. `Vs1Flow`/`Vs1Content`/사건 데이터에 기능 회귀 없음을 확인.

## 6. 수동 UI 검증 결과

**수행 여부: 미수행.**

이번 구현 세션은 에이전트 컨텍스트로, GUI 디스플레이 인터랙션 루프가 없다(CLAUDE.md §"이 에이전트 컨텍스트에서 Godot GUI 실행을 신뢰 도구로 쓰지 말 것"). 따라서 사용자가 직접 GUI를 띄워 다음 항목을 수동으로 확인해야 한다.

수동 검증 권장 절차:

1. `./scripts/godot.sh -e` 또는 `.\scripts\godot.cmd -e`로 에디터 실행.
2. Main 씬을 F5 또는 ▶ 버튼으로 실행.
3. 창 크기를 `360x640`, `540x720`, `1280x720` 3가지로 바꾸며 아래 체크리스트 확인.

### 권장 확인 체크리스트 (지시서 §"수동 UI 검증" 기준)

#### 기본 확인

| 항목 | 결과 | 메모 |
|---|---|---|
| 헤더만 보고 가문명, 대표작위, 세대, 현재 인물, 현재 단계가 이해되는가 | _ | _ |
| 하단 탭으로 `사건`, `상태`, `연대기`, `가문사`가 전환되는가 | _ | _ |
| 하단 탭이 보조 패널이 아니라 주 화면 전환 메뉴로 동작하는가 | _ | _ |
| 사건 화면에서 선택지와 영향 요약이 함께 읽히는가 | _ | _ |
| 결과 화면에서 상태 변화, 유산 태그 변화, 작위 변화/위험 변화, 연대기 기록 순서가 이해되는가 | _ | _ |
| 상태 화면에서 숫자 상세를 열지 않아도 현재 위험과 강점이 보이는가 | _ | _ |
| 연대기 화면에서 최신 기록과 큰 사건/보조 사건이 구분되는가 | _ | _ |
| 가문사 화면에서 활성 유산 태그와 지난 세대 이력이 현재 세대와 연결되어 보이는가 | _ | _ |
| 하단 탭, 선택지, 본문이 겹치거나 잘리지 않는가 (360/540/1280) | _ | _ |

#### 탭 전환 상태 보존 (mutation/render 분리 회귀 핵심 검증)

| 시나리오 | 결과 | 메모 |
|---|---|---|
| 사건 화면 → `상태` 탭 → `사건` 탭 → 같은 사건/같은 선택지가 보이는가 | _ | _ |
| 결과 화면 → `가문사` 탭 → `사건` 탭 → 같은 결과가 보이는가 | _ | _ |
| 세대 요약 화면 → `연대기` 탭 → `사건` 탭 → 세대 요약이 그대로 보이는가 | _ | _ |
| 세대 요약 화면 탭 전환 중 세대 요약 단락 텍스트가 변하지 않는가(`FinishCurrentGeneration` 중복 호출 없음 확인) | _ | _ |
| 결과 화면 → "다음 사건으로" 클릭 → 즉시 `상태` 탭 → `사건` 탭 → 다음 사건이 보이는가 | _ | _ |
| 멸문 화면 → 다른 탭 → `사건` 탭 → 멸문 종료 안내가 유지되는가 | _ | _ |

#### 헤더 확인

| 항목 | 결과 | 메모 |
|---|---|---|
| 사건 탭에서 `[단계: ...]`가 현재 사건 흐름과 맞는가 | _ | _ |
| 상태/연대기/가문사 탭에서 `[화면: ...]`가 선택된 탭과 맞는가 | _ | _ |
| 작위 위험 `Crisis` 이상일 때 모든 탭에서 헤더 경고가 보이는가 | _ | _ |

## 7. 확인하지 못한 항목

다음은 GUI 실행 없이는 검증할 수 없는 항목이다. 모두 사용자의 수동 검증이 필요하다.

- **360x640에서 하단 탭 라벨 4개 표시 안정성** — 4개 버튼(`> 사건`, `상태`, `연대기`, `가문사`)이 가용 너비 약 336px 내에서 잘리지 않는지 확인 필요. 잘림이 발생하면 §8 Root 확인 항목.
- **헤더 2행이 작은 화면에서 줄바꿈 시 본문 영역 압박** — `Crisis`/`RevocationThreat` 경고 활성 상태에서 360x640 줄바꿈 동작.
- **결과 화면의 다음 버튼이 360x640에서 스크롤로 도달 가능한지** — 결과 화면은 결과 문장 + 상태 변화 + 유산 변화 + 작위 변화 + 연대기 + 다음 버튼이 모두 들어가 가장 긴 화면.
- **3선택지 사건(FAM-01/FAM-08/CRI-03/PRO-05)의 ActionBar 부담 해소 여부** — 선택지 영향 요약이 짧게 한 줄로 보이는지.
- **상태 토글이 시각적으로 자연스러운지** — 토글 시 매번 스크롤 리셋되어 사용자가 다시 토글 위치까지 스크롤해야 하는 비용.
- **`> ` 텍스트 접두가 폰트와 어울리는지** — 미적 판단이라 사용자 피드백 필요.

## 8. Root 확인 필요 사항

03 지시서 §"Root 확인 필요" 5개 항목 중 이번 구현 시점에서 발생한 것:

| 항목 | 발생 여부 | 비고 |
|---|---|---|
| 360x640에서 4개 탭 라벨 축약 필요 | 미확인 | GUI 검증 후 사용자가 잘림 보고 시 Root 확인. 현재는 Root 기준 라벨 유지. |
| 멸문 직후 자동 가문사 탭 이동 필요 | 미발생 | 03 지시서 명시대로 사건 탭의 멸문 화면 유지. 사용자가 직접 가문사 탭 이동. |
| 멸문 안내 새 문구 추가 필요 | 미발생 | 기존 "가문이 멸문하여 더 이상 진행할 수 없습니다." 유지. |
| 헤더 정보가 작은 창에서 과도하게 길어짐 | 미확인 | GUI 검증 필요. |
| Root 문서에 없는 새 요약 문구/분류명 필요 | 미발생 | 모든 표시 문구는 기존 Root 문서 또는 기존 코드(Vs1Content/Vs1Flow)에서 옴. |

이번 구현이 새로 발견한 사항:

1. **헤더 2행의 탭별 설명 문구는 코드 내부 상수다.** "`[화면: 상태] 핵심 상태와 현재 인물을 확인합니다`" 같은 문구는 03 지시서 §"헤더 2행 정책"에서 그대로 가져왔지만, 엄밀히는 Root `vertical-slice-1-ui-layout-direction.md` 문서에는 없다. 03 지시서가 만든 문구이므로 Root가 후속 문체 라운드에서 다듬을 수 있다.
2. **활성 탭 접두 `> `의 시각 위계.** Disabled 버튼은 Godot 기본 테마로 흐릿하게 표시된다. `> ` 접두 + Disabled의 조합이 실제 사용에서 충분히 분명한지 GUI 검증 필요.
3. **상태 토글의 스크롤 리셋이 UX에 미치는 영향.** 03 지시서가 단순성을 우선했지만, 토글 후 매번 위로 점프하는 동작이 답답하면 후속 개선 후보.

## 9. 후속 개선 후보

이번 구현 범위 밖이지만 차후 다룰 만한 항목.

### UI 폴리시

- **상태 토글 시 스크롤 보존**: `RefreshAll` 대신 `RenderSelectedTab`만 호출하고 `ResetScroll` 생략하는 별도 경로. 코드 단순성 vs UX 트레이드오프.
- **활성 탭 시각 표시 강화**: `> ` 접두 외에 색상 또는 폰트 굵기 추가.
- **사건 진행 버튼 강조**: 현재는 일반 버튼. "→ 다음 사건으로" 같은 화살표 또는 폰트 색 강조.
- **결과 화면 길이 축소**: 변화 없는 항목(상태 변화 없음, 태그 변화 없음 등)을 명시적으로 "변화 없음"으로 표시 vs 생략 — 03 지시서는 생략을 권장하므로 현재 구조 유지하되 추후 사용자 피드백 반영.

### mutation/render 분리 검증 자동화

- 현재 smoke는 탭 전환을 시뮬레이션하지 않는다. mutation/render 분리 회귀를 자동 검출하려면 smoke에 "Render*Screen()을 N번 반복 호출 후 `_family` 상태가 변하지 않는지" 검증 분기를 추가할 수 있다. 단 smoke가 UI 노드를 만들지 않는 합류 계약과 충돌하므로 별도 헬퍼로 분리 필요.

### 탭 라벨 안정성

- 360x640에서 잘림이 확인되면 라벨 축약("연대기" → "기록", "가문사" → "이력") 또는 폰트 크기 조정. Root 결정 필요.

### 멸문 화면 UX

- 멸문 안내에 "가문사 탭에서 전체 이력을 확인할 수 있습니다" 같은 가이드 문구 추가 검토. Root 결정 필요.

### 키보드 포커스

- 현재 구현은 마우스/터치 기준. 키보드 Tab/Enter 네비게이션 동작은 미검증. Godot 기본 동작에 의존.

### 아트/스타일

- 현재는 Godot 기본 테마. 폰트, 색상, 패딩, 아이콘 등 최종 아트 스타일은 별도 작업.
