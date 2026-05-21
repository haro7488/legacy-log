# Vertical Slice 1 UI Layout Improvement - Project Claude Review Response

> 작성일: 2026-05-22
> 작성자: Project Claude
> 모드: 리뷰 응답 (구현 아님)
> 리뷰 대상: `docs/handoff/vertical-slice-1-ui-layout-improvement/01-review-request.md`

## 0. 리뷰 요약

리뷰 요청서의 3단 UI 구조(상단 헤더 / 중간 단일 게임 화면 / 하단 탭 메뉴) 전환 계획은 Root 제품 문서(`vertical-slice-1-ui-layout-direction.md`)와 충돌 없이 정합적이다. 단일 `Main.cs` 변경 범위로 구현 가능성도 높다.

다만 다음 영역에서 위험과 누락이 있다. Project Codex가 03 최종 작업 지시서를 쓰기 전에 반영 여부를 결정하면, 구현 중 멈출 횟수와 미세 수동 검증 반복을 줄일 수 있다.

가장 결정적인 리스크 3개:

1. **현재 `Show*` 함수의 부수효과(side-effect)와 렌더링이 한 함수에 묶여 있다** — 탭 전환 시 화면 상태가 유지되려면 "렌더링"과 "상태 변이(Vs1Flow.FinishCurrentGeneration, AdvanceToNextGeneration)"를 분리해야 한다. 분리하지 않고 `RenderSelectedTab()`만 도입하면 탭을 떠났다 돌아올 때 세대가 두 번 종결되거나 후계가 이중 진행되는 회귀가 생긴다.
2. **헤더 2행의 "현재 단계 / 화면 목적"이 비-사건 탭에서 비정합** — `상태` 탭에서 보이는 동안에도 헤더에 `[단계: 결과 확인] 선택의 결과를 확인합니다`가 그대로 남는다. 헤더가 어떤 의미를 표시해야 하는지 명세 누락.
3. **탭 전환과 사건 흐름 진행 버튼의 상호작용 규칙 누락** — "다음 사건으로", "다음 세대로 진입" 같은 진행 버튼이 사건 탭 내부에만 있으므로 사용자는 항상 사건 탭에 있을 때만 누를 수 있다. 이 명시가 빠져 있어, 시각적 피드백 없이 "왜 다음으로 못 가지?" 혼란을 부를 수 있다.

세부 발견은 아래 섹션별로 정리한다. 각 항목 끝에 `[권고]` 또는 `[Root 확인]` 분류를 붙였다.

---

## 1. 구현 가능성

### 1.1 변경 범위의 현실성

[유지] `scripts/Main.cs` 단일 파일 변경만으로 3단 구조 전환이 가능하다. 현재 코드는 이미 `BuildShell()` → 4영역 생성 → 각 화면 `Show*` 패턴이라 노드 책임 재배치만으로 처리 가능. 새 씬, 새 리소스, 새 의존성 없음.

[유지] `Vs1Flow`, `Vs1Content`, `Vs1Models`는 손대지 않아도 됨. UI 셸 외부 호출 API는 그대로 유지된다.

[유지] smoke 경로는 `_Ready()`가 `Vs1Smoke.Run(...)`을 먼저 호출하고 return하므로 UI 셸을 만들지 않는다. UI 리팩터링이 smoke를 깨뜨릴 가능성은 낮다.

### 1.2 Godot 4.6.2 노드 구조 선택

리뷰 요청 §3.3은 `TabContainer`보다 `HBoxContainer + 4개 Button` 방식을 권장한다.

**[권고 A]** 두 선택 모두 가능하지만 트레이드오프를 명시할 필요가 있다:

| 방식 | 장점 | 단점 |
|---|---|---|
| `TabContainer.TabsPosition = Bottom` | Godot 내장 탭 활성 상태 관리, 탭 라벨 표시, 키보드 포커스 자동 | 탭마다 별도 자식 노드를 두는 구조라 "단일 GameContent 컨테이너"와 충돌 |
| `HBoxContainer + Button × 4` | `GameContent` 단일 컨테이너에 자유롭게 렌더링, 명시적 탭 상태 관리 | 활성 탭 시각 표시를 직접 구현, 키보드 포커스 별도 처리 |

권장 방식은 `HBoxContainer + Button × 4`이며 이유는 "단일 `GameContent`에 4개 화면을 번갈아 렌더링" 패턴이 더 단순하기 때문이다. 다만 활성 탭 표시 방식을 03 지시서에서 구체화해야 한다(아래 §2.3).

### 1.3 변경 영향 면적

수정해야 할 부분의 라인 수 추정:

- `BuildShell()` + 영역 빌더 함수 4개: 약 100줄 → 약 80~120줄 (`BuildInfoTabs`/`BuildActionBar` 제거, `BuildBottomTabs` 신설)
- `Render*Tab()` 3개 → `Render*Screen()` 3개: 시그니처/타겟 컨테이너만 교체. 내부 로직 거의 그대로.
- `Show*` 6개 (`ShowGenerationStart`/`ShowEvent`/`ApplyChoice`/`ShowResult`/`ShowGenerationSummary`/`ShowExtinction`): 행동 버튼을 `SetActionBar` 호출 대신 `_mainContent.AddChild`로 옮김. 약 20~30줄 변경.
- 신규: `SelectTab()`, `RenderSelectedTab()`, `RenderEventScreen()`, `BuildChoiceBlock()` 헬퍼: 약 80~120줄.

총 변경량 약 200~300줄. 현재 Main.cs 730줄 기준 30~40%. 단일 PR로 가능한 규모.

### 1.4 기존 smoke와의 호환성

[유지] smoke가 UI 노드를 만들지 않으므로 리팩터링 자체가 smoke를 깨뜨리지 않는다. 단 smoke가 검증하는 것은 `Vs1Flow` 로직과 사건 데이터이며, UI 회귀(잘림, 겹침, 헤더 비정합)는 검증하지 않는다. 이건 제품 기준에 이미 명시돼 있음.

---

## 2. 리스크

### 2.1 [결정적] Show* 함수의 부수효과와 렌더링 결합

**문제:** 현재 코드는 다음과 같이 부수효과와 렌더링이 한 함수에 섞여 있다:

- `ShowGenerationSummary()` (Main.cs L621): 함수 안에서 `Vs1Flow.FinishCurrentGeneration(_family)`를 호출 — 이는 mutation(상태 변경)이며 `_pendingEnd`에 저장. 이후 본문 렌더링.
- `AfterSummary()` (L669): `Vs1Flow.BuildGenerationRecord`/`AdvanceToNextGeneration` 호출 — 상태 변경.
- `ApplyChoice()` (L527): `Vs1Flow.ApplyChoice(...)` 호출 — 상태 변경.

**리스크:** 리뷰 요청 §3.5의 "탭 전환은 사건 진행 상태를 바꾸지 않는다" 원칙을 지키려면 `RenderSelectedTab()`은 순수 렌더링이어야 한다. 그런데:

- 사용자가 결과 화면에서 `상태` 탭으로 전환 → `상태` 화면 렌더 → 다시 `사건` 탭으로 돌아옴 → `_eventScreen = Result`이라 `ShowResult()` 또는 그 일부를 다시 호출.
- 만약 사건 탭 렌더링 경로가 `ShowGenerationSummary()`를 다시 부르면 `Vs1Flow.FinishCurrentGeneration(_family)`가 **두 번 실행**되어 태그 노화, 큰 사건 ID 누적, EndType 결정이 중복 처리될 수 있다.

**권고:** `Show*` 함수를 두 층으로 분리한다:

```text
사건 진행 이벤트 (버튼 onClick 등):
  → 상태 변이 (Vs1Flow.* 호출, _pendingEnd 저장 등)
  → _eventScreen = ...
  → RenderSelectedTab() 호출

탭 전환 (SelectTab 호출):
  → _selectedTab = ...
  → RenderSelectedTab() 호출 (renderer는 순수, mutation 없음)

RenderSelectedTab():
  → switch (_selectedTab)
      → Event: RenderEventScreen() — 순수 렌더링만
      → State: RenderStateScreen() — 순수 렌더링만
      → ...

RenderEventScreen():
  → switch (_eventScreen)
      → GenerationStart: 본문 + 첫 사건 버튼 (Vs1Flow.BuildGenerationStartInfo만 호출 — 읽기 전용)
      → Event: 본문 + 선택지 (Vs1Flow.GetCurrentEvent만 호출 — 읽기 전용)
      → Result: 본문 + 다음 버튼 (저장된 _lastApplyResult 사용)
      → Summary: 본문 + 진행 버튼 (저장된 _pendingEnd 사용)
      → Extinction: 본문 + 종료 안내 (저장된 _pendingEnd 사용)
```

핵심: **Vs1Flow의 mutation 함수(`ApplyChoice`, `FinishCurrentGeneration`, `AdvanceToNextGeneration`)는 버튼 onClick 핸들러 안에서만 호출**하고, render 경로에서는 호출되지 않게 한다.

이를 강제하기 위해 `_pendingEnd`/`_lastApplyResult`는 onClick에서 미리 채워두고, renderer는 그 값을 읽기만 한다. 현재 `ShowGenerationSummary`처럼 "함수 진입 직후 mutation"하는 구조는 회귀 위험이 매우 크다.

**[권고]** 03 지시서에 다음 계약을 명시:

> RenderSelectedTab()과 RenderXxxScreen()은 Vs1FamilyState/Vs1Flow의 mutation 함수를 호출하지 않는다. mutation은 진행 버튼 onClick 또는 ApplyChoice 처리 시점에서만 일어난다.

### 2.2 [결정적] 헤더 2행의 비정합

**문제:** 현재 `RefreshContextHeader(stepLabel, purposeLine)`는 사건 흐름 단계(예: "사건 진행", "결과 확인")를 헤더 2행에 둔다. 사용자가 `상태` 탭으로 전환하면 헤더 2행은 여전히 "선택의 결과를 확인합니다"라고 나오는데, 본문은 상태 정보다 — 정보 비정합.

**리스크:** 사용자가 헤더를 신뢰할 수 없게 되어 "이 화면이 무엇인지" 매번 본문을 봐야 한다. 작은 화면일수록 이 비정합의 인지 비용이 크다.

**권고 옵션:**

| 옵션 | 헤더 2행 동작 | 트레이드오프 |
|---|---|---|
| A. 사건 흐름 단계 유지 | 비-Event 탭에서도 "결과 확인" 그대로 | 비정합. 비권장 |
| B. 탭 라벨로 교체 | `[탭: 상태] 핵심 상태 확인` | 사건 진행 중인지 정보 손실 |
| C. 사건 탭에서만 단계 표시, 다른 탭은 탭 이름 | Event 탭일 때만 "[단계: 사건 진행]", 나머지는 "[화면: 상태]" | 가장 직관적, 권장 |
| D. 두 줄 모두 사건 흐름만 표시, 탭 전환은 하단 탭 시각 표시로 구분 | 헤더는 사건 흐름의 진실 출처 | 사건 외 탭에서도 사건 단계가 보이는 단점은 약함 |

**[권고]** 옵션 C 권장. 03 지시서에 다음 동작을 명시:

```text
SelectTab(MainTab.Event) → 헤더 2행: "[단계: <event-screen-label>] <purpose>"
SelectTab(MainTab.State) → 헤더 2행: "[화면: 상태] 핵심 상태와 현재 인물을 확인합니다"
SelectTab(MainTab.Chronicle) → 헤더 2행: "[화면: 연대기] 이번 세대의 기록을 확인합니다"
SelectTab(MainTab.FamilyHistory) → 헤더 2행: "[화면: 가문사] 활성 유산과 지난 세대를 확인합니다"
```

작위 위험 경고는 모든 탭에서 동일하게 헤더 1행 또는 2행 끝에 표시.

### 2.3 [결정적] 활성 탭 시각 표시 방식

리뷰 요청 §3.3은 "선택된 탭 버튼은 `Disabled` 또는 테마 변형 대신 텍스트 접두/상태 색상처럼 최소 변경으로 구분한다"고 적었다.

**리스크:** 시각 구분이 약하면 사용자가 "지금 어느 탭에 있는지" 매번 본문을 봐야 한다. 텍스트 접두("▶ 사건")는 작동하지만 미적으로 거칠고, "상태 색상"은 Godot 기본 테마와 어떻게 충돌할지 불명확.

**권고 옵션:**

| 옵션 | 구현 | 트레이드오프 |
|---|---|---|
| A. `Disabled = true` on 활성 탭 | 가장 단순. 비활성 시각도 자동 | 활성 탭을 다시 클릭할 수 없음 (재진입으로 스크롤 리셋 시도가 막힘) |
| B. 텍스트 접두 `▶ 사건` | 명시적. 폰트 자체에 영향 없음 | 한국어 문자 폭 변동으로 탭 너비가 흔들림 |
| C. `Modulate` 알파 또는 `add_theme_color_override` | 활성/비활성 색 분리 | Godot 기본 테마 의존 |
| D. `ToggleButton` 그룹 | Godot 표준 패턴 | 4개 ButtonGroup 설정 필요. 약간 더 복잡 |

권장: **A (Disabled) + B (텍스트 접두)** 조합. Disabled 상태는 활성 탭의 시각/상호작용 표시를 동시에 처리하고, 텍스트 접두는 시각 단서를 보강한다. "현재 탭을 다시 클릭 → 새로고침"이 필요한 UX는 VS1에 없으므로 Disabled로 막아도 무방.

**[권고]** 03 지시서에 활성 탭 시각 표시 방식을 단일 선택지로 고정하라(반복 검증 비용 절감).

### 2.4 작은 창(360x640)에서 탭 라벨 4개 너비

**문제:** 한 줄에 4개 버튼 + 양쪽 12px 마진:

- 가용 너비: 360 - 24 = 336px
- 버튼 너비 ≈ 84px (균등 분할)
- 한국어 라벨: "사건", "상태", "연대기", "가문사" — 3글자 라벨이 2개
- 텍스트 접두("▶ 연대기") 시 4글자 라벨 + 화살표 = 약 5~6글자

**리스크:** Godot 기본 폰트(약 16px)로 6글자는 88~96px 필요. 84px 한계와 거의 일치. 폰트 변경, 줄바꿈, 패딩 차이에 따라 잘림 가능.

**[권고]** 03 지시서에:

- 버튼 `ClipText = false`, `AutowrapMode = Off`로 두고 줄바꿈을 막는다.
- HBoxContainer에 `Theme.Constant("separation") = 4` 정도로 최소 간격.
- 360x640에서 잘리면 활성 표시를 `▶` 대신 `*` 같은 1글자로 축약 가능. 또는 활성 탭만 굵게.

**[Root 확인]** 만약 360x640에서 어떤 조합으로도 라벨 4개가 안정적으로 안 들어가면, Root 확인으로 되돌아가 "사건/상태/연대기/가문사" 중 일부를 더 짧은 라벨로 바꿀지 결정해야 한다(요청서 §7의 분기와 일치).

### 2.5 사건 탭 내부 진행 버튼의 가시성

**문제:** "다음 사건으로", "다음 세대로 진입", "가문의 끝을 본다" 같은 진행 버튼은 사건 탭 내부 본문 마지막에 배치된다. 사용자가 다른 탭을 보다가 이 버튼을 누르려면 사건 탭으로 돌아와야 한다.

**리스크:** 작은 화면에서 사건 본문이 길어 진행 버튼이 스크롤 아래에 가려진다. 사용자가 "다음" 버튼을 못 찾고 헤맬 수 있다. 특히 결과 화면(결과 + 상태변화 + 태그변화 + 작위변화 + 연대기 = 5블록)이 길어진다.

**[권고]**

1. 진행 버튼은 본문 맨 아래 `행동 영역 블록`에 배치하되, `HSeparator` + 살짝 강조된 텍스트("→ 다음 사건으로")로 시각 신호를 준다.
2. 또는 사건 탭 내부에 한정한 "고정 하단 버튼" 영역을 둔다 — 단 이건 별도 ActionBar의 재도입이 되므로 Root 방향과 충돌 가능. 권장하지 않음.
3. 03 지시서에 "진행 버튼은 본문 마지막에 배치한다"는 원칙 + "사건 탭 외 다른 탭에서는 진행 버튼이 보이지 않는다"는 명시.

### 2.6 스크롤 리셋 의미론

**문제:** 현재 `ResetMainScroll()`은 모든 `Show*` 끝에서 호출된다. 새 구조에서 다음 4종류 갱신 케이스의 스크롤 리셋 정책이 다를 수 있다:

| 갱신 종류 | 스크롤 리셋? |
|---|---|
| 사건 흐름 진행 (Show* → 새 본문) | 리셋 (사용자가 새 화면을 위에서부터 읽어야 함) |
| 탭 전환 (SelectTab) | 리셋 (새 화면 시작) |
| 상태 상세 토글 (`_stateDetailVisible`) | **유지**가 자연스러움 (사용자가 현재 위치 유지) |
| 같은 탭 재진입 (활성 탭 클릭) | 옵션 A로 `Disabled`면 불가능 |

리뷰 요청 §3.5의 `SelectTab`은 "스크롤 상단 이동"을 포함한다. 사건 흐름 진행 시 스크롤 리셋도 포함되는지 명시 누락.

**[권고]** 03 지시서에:

- `RenderSelectedTab()`은 매번 스크롤 리셋한다.
- 단 `상태 상세 토글`처럼 같은 화면 내부 부분 갱신은 별도 경로를 둔다 (또는 토글이 `RenderStateScreen()` 전체를 다시 호출해도 무방하면 그대로 두고 사용자가 다시 스크롤).

가장 단순한 정책: **렌더링 단일 진입점은 항상 스크롤 리셋**. 토글 사용성이 약간 떨어지지만 일관성 확보. Root 검증 가능.

### 2.7 멸문 상태에서 탭 전환

**문제:** 멸문 후 사용자는 가문 전체 이력을 가문사 탭에서 보고 싶을 수 있다. `IsExtinct = true` 상태에서 탭 전환은 어떻게 동작하는가?

- 사건 탭: 멸문 화면(진행 버튼 없음).
- 상태 탭: 마지막 세대의 상태 표시. 정보로서 가치 있음.
- 연대기 탭: 마지막 세대 연대기 표시. 정보로서 가치 있음.
- 가문사 탭: 전체 이력 표시. 가장 의미 있는 탭.

**[권고]** 03 지시서에 "멸문 후에도 4개 탭 모두 정상 전환 가능"을 명시. 추가로 멸문 시 자동으로 가문사 탭으로 전환할지(자동 전환) 또는 사용자가 직접 전환할지(수동) Root 결정 필요. 기본은 수동(현재 사건 탭 유지)이 단순.

**[Root 확인]** 멸문 시 사건 탭이 종료 안내만 표시되므로, 사용자에게 "이제 무엇을 봐야 하는지" 시각 단서가 부족할 수 있다. 종료 안내 마지막에 "가문사 탭에서 전체 이력을 확인할 수 있습니다" 문구를 추가할지.

### 2.8 사건 화면의 본문 + 선택지 + 영향 요약이 한 화면에서 보이는가

리뷰 요청 §3.4 사건 탭은:

- 본문 블록
- 판단 단서/영향 요약 블록 (조건 단서, 최대 3개)
- `HSeparator`
- 행동 영역 블록 (선택지 버튼 + 영향 요약)

**리스크:** 360x640에서 헤더(약 60~80px) + 하단 탭(약 40~48px) = 약 110~130px가 고정. 남은 약 510~530px에서 본문 + 단서 + 선택지 3개 + 영향 요약 3개가 들어가야 한다. 3선택지 사건(FAM-01, FAM-08, CRI-03, PRO-05)은 4영역에서 가장 빡빡한 케이스.

**[권고]**

- 사건 본문은 `AutowrapMode = Word` 유지하되 매우 길지 않게(현재 Root 본문 길이는 평균 100~150자, 360x640에서 약 4~6줄).
- 단서는 1줄로 압축 (`Vs1EventCondition.DisplayReason` ` · ` 연결).
- 선택지 영향 요약은 한 줄 최대 24자 권장(이미 §1.3 권고와 동일).
- 03 지시서에 "사건 화면은 본문 우선, 선택지 영향 요약은 짧게 한 줄, 스크롤 필요 시 진행 버튼이 보일 때까지 스크롤"이라는 원칙 명시.

### 2.9 InfoTabs 노드와 부속 컨테이너 제거

**문제:** 현재 `_infoTabs`, `_stateTabContent`, `_chronicleTabContent`, `_familyHistoryTabContent` 4개 필드가 있다. 리팩터링 후 이들 노드/필드를 모두 제거해야 한다.

**리스크:** 필드 참조가 남으면 컴파일 오류(null 참조) 또는 dead code. 특히 `RenderStateTab` → `RenderStateScreen` 같은 rename 시 그 안에서 `_stateTabContent.AddChild` 호출이 남아있으면 NullReferenceException.

**[권고]** 03 지시서에 명시:

- `_infoTabs`, `_stateTabContent`, `_chronicleTabContent`, `_familyHistoryTabContent` 4개 필드 모두 제거.
- `_actionBar` 필드도 제거.
- 새 필드: `_gameContent` (`GameContent` VBoxContainer), `_bottomTabsContainer` (`HBoxContainer`), `_tabButtons` (`Button[]` 또는 4개 개별 필드).
- `_stateDetailVisible`은 유지.

### 2.10 ActionBar 제거 후 `SetActionBar` 호출처 정리

**문제:** 현재 `SetActionBar(...)`는 6개 화면에서 호출된다(`ShowGenerationStart`, `ShowEvent`, `ShowResult`, `ShowGenerationSummary`, `ShowExtinction`, `AfterSummary` 흐름).

**리스크:** 한 곳이라도 `SetActionBar` 호출을 놓치면 컴파일 오류(메서드 제거 시) 또는 동작 누락. 깜빡 잊고 남은 호출이 dead path가 됨.

**[권고]** 03 지시서에 "`SetActionBar` 함수와 모든 호출처를 제거. 대신 진행 버튼은 `RenderEventScreen()` 안에서 `_gameContent.AddChild`로 추가"라고 명시.

### 2.11 사건 화면 단서 라인 분리

현재 코드는 `단서: A · B · C` 한 줄에 최대 3개 조건을 표시(L477~497). 리뷰 요청 §3.2 "판단 단서/영향 요약 블록"은 단서를 별도 블록으로 둘 것을 시사하지만 형태 명세가 없다.

**[권고]** 03 지시서에 단서 블록의 표시 형태를 단일 선택지로 고정:

| 옵션 | 형태 | 트레이드오프 |
|---|---|---|
| A. 한 줄로 ` · ` 연결 (현재) | `단서: 백작 이상 또는 왕의 총애 · 자작 이상 · ...` | 가장 컴팩트 |
| B. 줄별 항목 | `단서: 백작 이상 또는 왕의 총애 / 자작 이상 / ...` (3줄) | 더 명확하지만 세로 공간 차지 |

권장: **A 유지**. 현재 코드가 이미 충분히 컴팩트.

---

## 3. 누락된 검증 항목

리뷰 요청 §5의 수동 검증 체크리스트는 좋다. 다만 다음이 추가로 필요하다.

### 3.1 탭 전환 상태 유지 검증

**누락:** 리뷰 요청은 "사건 탭을 떠났다가 돌아와도 현재 사건/결과/요약 상태가 유지되는가"만 명시. 다음 시나리오도 명시적으로 검증해야 한다:

| 시나리오 | 검증 항목 |
|---|---|
| 사건 화면(선택지 표시 중) → 상태 탭 → 사건 탭 | 같은 사건, 같은 선택지가 다시 보이는가. 사건 인덱스가 진행되지 않았는가. |
| 결과 화면 → 가문사 탭 → 사건 탭 | 결과 문장, 상태 변화, 태그 변화, 연대기 표시가 그대로인가. `_lastApplyResult` 보존 확인. |
| 세대 요약 화면 → 연대기 탭 → 사건 탭 | 세대 요약이 그대로 보이는가. `Vs1Flow.FinishCurrentGeneration`이 두 번 호출되지 않았는가(태그 노화 중복 회귀 검증). |
| 결과 화면 → "다음 사건" 누름 → 즉시 상태 탭 → 사건 탭 | 다음 사건 화면이 보이는가. 이전 결과로 되돌아가지 않는가. |
| 세대 요약 → "다음 세대로 진입" 누름 → 즉시 가문사 탭 → 사건 탭 | 새 세대 시작 화면이 보이는가. 다음 세대 정보가 정확한가. |
| 멸문 화면 → 가문사 탭 → 사건 탭 | 멸문 종료 안내가 그대로 보이는가. 진행 버튼이 새로 생겼는가(생기면 안 됨). |

이 6개 시나리오는 §2.1의 mutation/render 분리 회귀를 직접 잡아내는 검증 케이스다.

**[권고]** 03 지시서의 수동 검증 체크리스트에 이 6개 시나리오를 추가.

### 3.2 헤더 동작 검증

**누락:**

- 사건 탭에서 탭 전환 시 헤더 2행이 어떻게 변하는가 (§2.2의 옵션 결정에 따라).
- 작위 위험 `Crisis` 이상이 활성일 때 모든 탭에서 헤더 경고가 보이는가.
- 360x640에서 헤더 라인 1+2가 줄바꿈 포함 4줄 이상이 되어 본문을 밀어내지 않는가.

**[권고]** 수동 검증 체크리스트에 추가.

### 3.3 진행 버튼 가시성 검증

**누락:**

- 결과 화면(가장 긴 화면 — 5블록)이 360x640에서 다음 버튼까지 스크롤 도달 가능한가.
- 사건 화면(3선택지)에서 마지막 선택지 + 영향 요약이 화면에 들어가는가 또는 자연스럽게 스크롤되는가.

**[권고]** 수동 검증 체크리스트에 추가.

### 3.4 상태 토글의 스크롤 영향

**누락:** 상태 탭 상세 토글 클릭 시:

- 스크롤이 위로 점프하는가(§2.6의 정책에 따라).
- 토글 후 상세 점수 4줄이 추가되어 화면이 길어지는가.

**[권고]** 03 지시서에 상태 토글의 스크롤 동작을 명시. 추가로 토글 자체의 시각 변화(텍스트 "상세 정보 보기" ↔ "상세 정보 숨기기")를 유지하는 것을 확인.

### 3.5 자동 smoke가 잡지 못하는 회귀

**[권고]** 03 지시서에 명시적으로:

- smoke는 UI 노드를 만들지 않으므로 UI 회귀(겹침, 잘림, 비정합 헤더)를 검증하지 않는다.
- smoke의 `SMOKE_OK`는 `Vs1Flow`/`Vs1Content` 기능 회귀가 없음을 의미할 뿐이다.
- mutation/render 분리 회귀(§2.1)는 smoke가 부분적으로 검출할 수 있다 — 만약 `FinishCurrentGeneration`이 두 번 호출되면 태그 노화가 두 번 일어나 ASSERT가 깨질 수 있다. 다만 smoke는 탭 전환을 시뮬레이션하지 않으므로 직접 검출은 불가.

수동 검증 외에 회귀를 조금이라도 자동 검출하려면 다음 옵션을 추가할 수 있다 — 단 03 범위 외:

- 옵션: smoke에 "탭 전환 시뮬레이션" 모드를 추가해 `RenderSelectedTab()`을 여러 번 호출하고 `_family` 상태가 변하지 않는지 검증. 다만 smoke가 UI 노드를 만들지 않는 합류 계약과 충돌하므로 권장하지 않음.

[Root 확인 불필요] 수동 검증으로 충분.

---

## 4. 더 작은 작업 분해

리뷰 요청 §6은 11단계 구현 단계를 둔다. 단일 작업 흐름으로 진행한다고 명시. 동의한다 — 단일 `Main.cs` 파일이라 병렬 분해의 가치가 낮다.

다만 **체크포인트 분해**는 가능하다. 각 체크포인트마다 `dotnet build` + headless import + smoke를 돌려 회귀를 좁게 잡을 수 있다.

### 4.1 권장 체크포인트 분해

**Phase 1: 탭 상태와 dispatch 골격 추가 (기존 UI 그대로)**

- `MainTab`, `EventScreen` enum 추가.
- `_selectedTab`, `_eventScreen` 필드 추가.
- `SelectTab(MainTab)`, `RenderSelectedTab()` 메서드 추가 (아직 호출되지 않음).
- 기존 4영역 UI는 그대로 동작. smoke 정상.

체크포인트: `dotnet build` + smoke pass. (실질적 행위 변화 없음, 컴파일 확인.)

**Phase 2: 셸 재구성 (4영역 → 3영역)**

- `BuildShell()` 수정: `BuildInfoTabs`, `BuildActionBar` 제거. `BuildBottomTabs` 추가. `MainScroll` → `GameScroll`, `MainContent` → `GameContent` 이름 변경 또는 유지.
- `_infoTabs`, `_stateTabContent`, `_chronicleTabContent`, `_familyHistoryTabContent`, `_actionBar` 필드 제거.
- 신규 필드: `_bottomTabsContainer`, `_tabButtons[4]`, `_gameContent` (또는 기존 이름 유지).

체크포인트: 컴파일 통과. smoke pass. **GUI 실행은 깨짐(`SetActionBar` 호출이 컴파일 오류).** 다음 phase로 빠르게 이동.

**Phase 3: 사건 흐름 렌더링 재배치 + ActionBar 제거**

- `Show*` 6개 함수를 `_eventScreen` 갱신 + `RenderSelectedTab()` 호출로 재구성.
- `RenderEventScreen()` 신설. 안에서 `_eventScreen`에 따라 본문 렌더 + 진행 버튼을 `_gameContent`에 직접 추가.
- `SetActionBar` 호출 제거 및 함수 자체 제거.
- `Vs1Flow` mutation 호출은 onClick 핸들러에 한정.

체크포인트: 컴파일 통과. smoke pass. **GUI에서 사건 흐름 정상.** 단 탭 전환 안 됨.

**Phase 4: 상태/연대기/가문사 렌더링 재배치**

- `RenderStateTab/ChronicleTab/FamilyHistoryTab` → `RenderStateScreen/ChronicleScreen/FamilyHistoryScreen`.
- 타겟 컨테이너를 `_stateTabContent` → `_gameContent`로 교체.
- 내부 로직은 거의 그대로.

체크포인트: 컴파일 통과. smoke pass. GUI에서 사건 + 다른 탭 정상.

**Phase 5: 탭 전환 활성화**

- `BottomTabs` 버튼의 onClick에 `SelectTab(MainTab.X)` 연결.
- `SelectTab` 안에서 탭 활성 시각 표시 갱신 (§2.3).
- 헤더 2행 갱신 정책 (§2.2 옵션 C) 구현.

체크포인트: 컴파일 통과. smoke pass. **GUI 수동 검증으로 §3.1 6개 시나리오 모두 확인.**

**Phase 6: 폴리시**

- 헤더 2행 길이 검증.
- 스크롤 리셋 정책.
- 360x640 탭 너비 검증.
- 상세 토글 동작 확인.

체크포인트: `dotnet build` + headless import + smoke + 수동 검증 (3개 창 크기).

### 4.2 체크포인트 분해의 가치

각 phase 끝에 `dotnet build` + smoke 한 번씩 돌리면 회귀를 phase 단위로 좁힐 수 있다. 단일 commit/PR로 가지 말고 phase별 commit 권장. 03 지시서에 명시 여부는 Project Codex 판단.

### 4.3 병렬화 불가능 영역

리뷰 요청 §6 "병렬 작업 분해" 섹션은 정확하다. `Main.cs` 단일 파일 + 강하게 결합된 UI 상태는 병렬 작업에 부적합. 동의.

---

## 5. Project Claude 리뷰 질문 답변

> Q1. 구현 가능성

**답: 높음.** 단일 `Main.cs` 변경 약 200~300줄. 새 씬/리소스 없음. `Vs1Flow`/`Vs1Content` 변경 없음.

> Q2. 리스크

**답: 위 §2의 11개 항목.** 가장 결정적인 3개:

1. Show* 함수의 mutation/render 결합 — RenderSelectedTab 도입 시 회귀 위험 큼 (§2.1).
2. 헤더 2행의 비-Event 탭 비정합 (§2.2).
3. 활성 탭 시각 표시 방식 미명세 (§2.3).

> Q3. 누락된 검증 항목

**답: 위 §3의 5개 항목.** 가장 중요한 것은 §3.1의 탭 전환 상태 유지 6개 시나리오 — 이건 §2.1의 회귀를 수동으로 잡아내는 유일한 길.

> Q4. 더 작은 작업 분해

**답: 위 §4의 6단계 체크포인트 분해.** 병렬 분해는 불필요/불가능. 체크포인트 단위 commit이 회귀 좁히기에 유리.

---

## 6. 종합 평가

**리뷰 결론: 큰 틀에서 진행 가능. 03 최종 작업 지시서에 다음 5개를 명시 권고.**

1. **mutation/render 분리 계약** (§2.1) — RenderSelectedTab/RenderXxxScreen은 Vs1Flow mutation을 호출하지 않는다.
2. **헤더 2행 정책** (§2.2 옵션 C) — Event 탭일 때만 "[단계: ...]", 나머지는 "[화면: ...]".
3. **활성 탭 시각 표시 방식** (§2.3) — Disabled + 텍스트 접두 조합 권장.
4. **6개 탭 전환 시나리오 수동 검증** (§3.1) — Show* 회귀를 잡는 핵심 케이스.
5. **체크포인트 분해** (§4.1) — Phase 1~6 단위로 commit 권장.

추가 [Root 확인] 항목:

- 멸문 후 자동 탭 전환 여부 (§2.7).
- 360x640에서 탭 라벨 4개가 안정적으로 안 들어가면 라벨 축약 필요 (§2.4).

03 지시서가 위 5개를 반영하고 구현 phase가 6 체크포인트를 따른다면, 단일 작업 흐름으로 무리 없이 진행 가능하다. mutation/render 분리 계약만 잘 지키면 가장 큰 회귀 위험은 회피된다.
