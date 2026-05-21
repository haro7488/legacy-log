# Vertical Slice 1 UI Layout Improvement - Final Work Instruction

## Project Claude 전달 문장

`docs/handoff/vertical-slice-1-ui-layout-improvement/03-final-work-instruction.md`를 읽고 구현 모드로 진행해 주세요. 구현 결과와 검증 결과는 `docs/handoff/vertical-slice-1-ui-layout-improvement/04-implementation-report.md`에 작성해 주세요.

## 구현 결과 보고 경로

- `docs/handoff/vertical-slice-1-ui-layout-improvement/04-implementation-report.md`

## 목적

Vertical Slice 1의 기존 게임 규칙과 플레이 흐름을 유지하면서 UI를 상단 헤더, 중간 단일 게임 화면, 하단 탭 메뉴의 3단 구조로 전환한다.

이번 작업은 반복 플레이 중 사건 판단, 상태 확인, 연대기 확인, 가문사 확인을 작은 창과 세로형 화면에서도 자연스럽게 오갈 수 있게 만드는 UI 구조 개선이다.

## 수정 범위

- `scripts/Main.cs`

주요 변경 범위:

- 기존 `ContextHeader / MainScroll(MainContent) / InfoTabs / ActionBar` 4영역 구조를 `ContextHeader / GameScroll(GameContent) / BottomTabs` 3영역 구조로 바꾼다.
- 기존 `InfoTabs` 보조 패널을 제거하고, 하단 `사건 / 상태 / 연대기 / 가문사` 주 화면 전환 메뉴로 대체한다.
- 기존 `ActionBar`를 제거하고, 사건 선택지와 진행 버튼을 사건 탭 내부 주요 행동 영역에 배치한다.
- 상태, 연대기, 가문사 렌더링은 보조 탭 내부가 아니라 중간 단일 게임 화면 전체를 채우게 한다.
- 기존 VS1 smoke 검증 경로는 유지한다.

## 제외 범위

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 상태 수치, 가중치, 밸런스 변경
- 최종 UI/아트 스타일 확정
- 저장/불러오기, 혼인, 혈통 트리 추가
- `Vs1Flow`, `Vs1Content`, `Vs1Models`의 게임 규칙 변경
- 새 Godot 씬, 새 리소스, 새 이미지 추가
- 탭 라벨 축약 같은 제품 문구 변경

## 사전 가정

- Root 제품 문서가 제품 기준의 source of truth다.
- 현재 VS1 기능은 구현 및 사용자 수동 확인 완료 상태다.
- 이번 변경은 UI 배치와 렌더링 구조 변경이며 게임 규칙 변경이 아니다.
- 하단 탭 전환은 사건 진행 상태를 바꾸지 않는다.
- 자동 smoke는 UI 품질을 보장하지 않으므로 수동 검증을 별도로 보고한다.

## 리뷰 반영 요약

Project Claude 리뷰에서 제안한 사항 중 다음을 반영한다.

- `RenderSelectedTab()`과 `RenderXxxScreen()`은 상태 변경 함수를 호출하지 않는 읽기 전용 렌더링 경로로 만든다.
- `Vs1Flow.ApplyChoice`, `Vs1Flow.FinishCurrentGeneration`, `Vs1Flow.AdvanceToNextGeneration` 같은 mutation은 버튼 핸들러 또는 명시적 진행 함수에서만 호출한다.
- 헤더 2행은 사건 탭에서는 현재 사건 흐름 단계를, 다른 탭에서는 현재 주 화면 이름을 표시한다.
- 활성 탭은 `Disabled = true`와 텍스트 접두를 조합해 구분한다.
- 탭 전환 상태 보존 수동 검증 시나리오를 추가한다.
- 구현은 단일 파일 변경이지만 Phase 단위 체크포인트로 나누어 검증한다.

다음 제안은 부분 반영한다.

- 360x640에서 탭 라벨이 맞지 않을 경우 라벨 축약을 바로 구현하지 않는다. Root가 정한 탭 이름을 유지하고, 맞지 않으면 구현 보고서에 Root 확인 필요로 기록한다.
- 멸문 후 자동으로 가문사 탭으로 전환하지 않는다. 기본은 사건 탭의 멸문 화면 유지이며, 사용자가 하단 탭으로 가문사를 직접 확인할 수 있게 한다.

반영하지 않는 제안은 없다.

## 구체적 구현 단계

### Phase 1: 탭 상태와 렌더 dispatch 추가

1. `scripts/Main.cs`에 `MainTab` enum을 추가한다.

```csharp
private enum MainTab
{
    Event,
    State,
    Chronicle,
    FamilyHistory,
}
```

2. `EventScreen` enum을 추가한다.

```csharp
private enum EventScreen
{
    GenerationStart,
    Event,
    Result,
    GenerationSummary,
    Extinction,
}
```

3. 필드를 추가한다.

- `_selectedTab`
- `_eventScreen`

4. `SelectTab(MainTab tab)`과 `RenderSelectedTab()` 골격을 추가한다.

이 단계에서는 기존 UI 동작을 깨지 않게 작게 추가하고 `dotnet build`를 확인한다.

### Phase 2: UI shell을 3영역으로 전환

1. `BuildShell()`을 다음 순서로 바꾼다.

- `BuildContextHeader(appRoot)`
- `BuildGameScroll(appRoot)`
- `BuildBottomTabs(appRoot)`

2. 기존 `BuildInfoTabs()`와 `BuildActionBar()` 호출을 제거한다.
3. 기존 필드를 제거한다.

- `_infoTabs`
- `_stateTabContent`
- `_chronicleTabContent`
- `_familyHistoryTabContent`
- `_actionBar`

4. 신규 또는 대체 필드를 둔다.

- `_gameScroll`
- `_gameContent`
- `_bottomTabsContainer`
- `_eventTabButton`
- `_stateTabButton`
- `_chronicleTabButton`
- `_familyHistoryTabButton`

기존 `_mainScroll`, `_mainContent` 이름을 유지해도 되지만, 역할이 3단 구조의 중간 화면임을 주석과 함수명에서 명확히 한다.

### Phase 3: ActionBar 제거와 사건 탭 내부 행동 영역 이동

1. `SetActionBar()` 함수와 모든 호출을 제거한다.
2. 진행 버튼과 선택지 버튼은 `GameContent` 내부 마지막 행동 영역에 직접 추가한다.
3. 행동 영역은 다음 패턴을 따른다.

- `HSeparator`
- 행동 영역 제목 또는 짧은 안내 라벨
- 버튼 또는 선택지 블록

4. 사건 선택지 블록은 선택 행동명 버튼과 영향 요약 라벨을 함께 묶는다.
5. 멸문 화면은 진행 버튼 없이 종료 안내 라벨만 화면 내부 마지막에 둔다.

### Phase 4: mutation/render 분리

이 단계가 가장 중요하다.

다음 계약을 반드시 지킨다.

- `RenderSelectedTab()`은 상태를 변경하지 않는다.
- `RenderEventScreen()`은 상태를 변경하지 않는다.
- `RenderStateScreen()`, `RenderChronicleScreen()`, `RenderFamilyHistoryScreen()`은 상태를 변경하지 않는다.
- `Vs1Flow.ApplyChoice()`는 선택지 버튼 핸들러에서만 호출한다.
- `Vs1Flow.FinishCurrentGeneration()`은 세대 요약으로 처음 진입하는 진행 버튼 핸들러에서만 호출하고, 결과는 `_pendingEnd`에 저장한다.
- `Vs1Flow.AdvanceToNextGeneration()`은 세대 요약 이후 다음 세대 진입 버튼 핸들러에서만 호출한다.

권장 흐름:

- `GoToGenerationStart()`: `_eventScreen = GenerationStart`, `_selectedTab = Event`, 렌더
- `GoToEvent()`: `_eventScreen = Event`, `_selectedTab = Event`, 렌더
- `ApplyChoiceAndShowResult(ev, choice)`: `ApplyChoice` 호출, `_lastApplyResult` 저장, `_eventScreen = Result`, 렌더
- `FinishGenerationAndShowSummary()`: `_pendingEnd`가 없을 때만 `FinishCurrentGeneration` 호출, `_eventScreen = GenerationSummary`, 렌더
- `ContinueAfterSummary()`: `_pendingEnd` 기준으로 멸문 또는 다음 세대 진입 처리
- `ShowExtinctionScreen(endResult)`: `_eventScreen = Extinction`, 렌더

세대 요약 렌더링은 `_pendingEnd`를 읽기만 해야 한다. 탭을 떠났다가 돌아와도 `FinishCurrentGeneration()`이 다시 호출되면 안 된다.

### Phase 5: 상태/연대기/가문사 화면 렌더링 전환

1. 기존 `RenderStateTab()`을 `RenderStateScreen()`으로 바꾼다.
2. 기존 `RenderChronicleTab()`을 `RenderChronicleScreen()`으로 바꾼다.
3. 기존 `RenderFamilyHistoryTab()`을 `RenderFamilyHistoryScreen()`으로 바꾼다.
4. 각 함수는 보조 탭 컨테이너가 아니라 `GameContent`에 직접 내용을 추가한다.
5. 기존 표시 정보는 유지한다.

상태 화면 우선순위:

- 핵심 상태 4축 질적 구간
- 작위 위험 단계
- 현재 인물 성향, 강점, 약점
- 상세 수치 접기/펼치기
- seed

연대기 화면 우선순위:

- 최신 기록 1건
- 큰 사건 목록
- 보조 사건 목록
- 기록 없음 안내

`Vs1ChronicleEntry.IsMajor`를 사용해 큰 사건과 보조 사건을 분리한다.

가문사 화면 우선순위:

- 활성 유산 태그와 지속 범위
- 유산 태그 짧은 설명
- 보유 작위
- 지난 세대 이력
- 멸문 상태에서는 전체 이력

### Phase 6: 하단 탭 동작과 헤더 정책 구현

1. `BuildBottomTabs()`는 `HBoxContainer`와 4개 버튼으로 구성한다.
2. 버튼 텍스트는 Root 기준 탭명을 유지한다.

- `사건`
- `상태`
- `연대기`
- `가문사`

3. 각 버튼은 `SelectTab(MainTab.X)`를 호출한다.
4. 활성 탭은 `Disabled = true`로 두고 텍스트 앞에 `> `를 붙인다.
5. 비활성 탭은 `Disabled = false`로 두고 원래 탭명을 표시한다.

헤더 2행 정책:

- 사건 탭: `[단계: <event-screen-label>] <purpose>`
- 상태 탭: `[화면: 상태] 핵심 상태와 현재 인물을 확인합니다`
- 연대기 탭: `[화면: 연대기] 이번 세대의 기록을 확인합니다`
- 가문사 탭: `[화면: 가문사] 유산과 지난 세대 이력을 확인합니다`

작위 위험 경고는 기존 기준을 유지한다.

- `Crisis`
- `RevocationThreat`

위 두 단계에서는 모든 탭에서 헤더 경고가 보이게 한다.

### Phase 7: 스크롤 정책 정리

- 탭 전환 시 `GameScroll`은 상단으로 이동한다.
- 사건 흐름 진행 시 `GameScroll`은 상단으로 이동한다.
- 상태 상세 접기/펼치기 시에도 단순성을 위해 상단으로 이동해도 된다.
- 별도 스크롤 위치 보존 기능은 이번 범위에 추가하지 않는다.

### Phase 8: 주석과 dead code 정리

1. `Main.cs` 상단 합류 계약 주석을 3영역 구조로 갱신한다.
2. `InfoTabs`, `ActionBar`, `SetActionBar` 관련 dead code를 남기지 않는다.
3. 새 기능처럼 보이는 speculative helper를 만들지 않는다.

## Godot 경로 케이스 계약

- 수정 파일: `scripts/Main.cs`
- 참조 씬: `Scenes/Main.tscn`
- 기존 `res://scripts/Main.cs` 경로 케이스를 유지한다.
- 새 `Scripts/` 또는 `scenes/` 디렉터리를 만들지 않는다.
- 이번 범위에서는 새 `.tscn`, `.tres`, 이미지 리소스를 만들지 않는다.

## 병렬 작업 분해

이번 작업은 병렬 구현하지 않는다.

이유:

- `scripts/Main.cs` 단일 파일 변경이다.
- UI shell, 탭 상태, 사건 진행 상태, 렌더링 경로가 강하게 결합되어 있다.
- 병렬 변경 시 `Show*`, `Render*`, `SetActionBar` 제거 지점에서 충돌 위험이 높다.

대신 Phase 1~8 순서로 작게 진행하고 각 주요 단계마다 빌드 가능한 상태를 유지한다.

## 검증 명령

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

## 수동 UI 검증

창 크기:

- `360x640`
- `540x720`
- `1280x720`

기본 확인:

- 헤더만 보고 가문명, 대표작위, 세대, 현재 인물, 현재 단계가 이해되는가
- 하단 탭으로 `사건`, `상태`, `연대기`, `가문사`가 전환되는가
- 하단 탭이 보조 패널이 아니라 주 화면 전환 메뉴로 동작하는가
- 사건 화면에서 선택지와 영향 요약이 함께 읽히는가
- 결과 화면에서 상태 변화, 유산 태그 변화, 작위 변화/위험 변화, 연대기 기록 순서가 이해되는가
- 상태 화면에서 숫자 상세를 열지 않아도 현재 위험과 강점이 보이는가
- 연대기 화면에서 최신 기록과 큰 사건/보조 사건이 구분되는가
- 가문사 화면에서 활성 유산 태그와 지난 세대 이력이 현재 세대와 연결되어 보이는가
- 하단 탭, 선택지, 본문이 겹치거나 잘리지 않는가

탭 전환 상태 보존 확인:

- 사건 화면에서 `상태` 탭으로 갔다가 `사건` 탭으로 돌아오면 같은 사건과 같은 선택지가 보이는가
- 결과 화면에서 `가문사` 탭으로 갔다가 `사건` 탭으로 돌아오면 같은 결과가 보이는가
- 세대 요약 화면에서 `연대기` 탭으로 갔다가 `사건` 탭으로 돌아오면 세대 요약이 그대로 보이는가
- 세대 요약 화면 탭 전환 중 `FinishCurrentGeneration()`이 중복 호출된 흔적이 없는가
- 결과 화면에서 `다음 사건으로`를 누른 직후 `상태` 탭으로 갔다가 `사건` 탭으로 돌아오면 다음 사건이 보이는가
- 멸문 화면에서 다른 탭을 오간 뒤 `사건` 탭으로 돌아오면 멸문 종료 안내가 유지되는가

헤더 확인:

- 사건 탭에서는 `[단계: ...]`가 현재 사건 흐름과 맞는가
- 상태/연대기/가문사 탭에서는 `[화면: ...]`가 선택된 탭과 맞는가
- 작위 위험 `Crisis` 이상일 때 모든 탭에서 경고가 보이는가

## 완료 기준

- 상단 헤더만 보고 현재 가문, 대표작위, 세대, 현재 인물, 현재 단계가 이해된다.
- 중간 화면은 선택된 탭의 단일 주 화면으로 동작한다.
- 하단 탭으로 `사건`, `상태`, `연대기`, `가문사`를 전환할 수 있다.
- 사건 화면에서 선택지와 영향 요약이 함께 읽힌다.
- 결과 화면에서 상태 변화, 유산 태그 변화, 작위 변화/위험 변화, 연대기 기록이 순서대로 이해된다.
- 상태 화면에서 숫자 상세를 열지 않아도 현재 가문의 위험과 강점을 판단할 수 있다.
- 연대기 화면에서 최신 기록과 큰 사건/보조 사건이 구분된다.
- 가문사 화면에서 이전 세대의 흔적이 현재 세대와 연결되어 보인다.
- `360x640`, `540x720`, `1280x720`에서 하단 탭, 선택지, 본문이 겹치거나 잘리지 않는다.
- 기존 VS1 smoke 검증이 유지된다.

## Root 확인 필요

구현 중 아래 상황이 발생하면 임의로 확정하지 말고 `04-implementation-report.md`에 Root 확인 필요로 기록한다.

- `360x640`에서 `사건 / 상태 / 연대기 / 가문사` 전체 탭 라벨을 유지할 수 없어 축약이 필요해지는 경우
- 멸문 직후 자동으로 `가문사` 탭으로 이동해야 한다고 판단되는 경우
- 멸문 안내에 새 문구를 추가해야만 사용자가 다음 확인 위치를 이해할 수 있다고 판단되는 경우
- 헤더 정보가 작은 창에서 과도하게 길어 일부 정보를 숨기거나 접어야 하는 경우
- 기존 Root 문서에 없는 상태/연대기/가문사 요약 문구나 분류명이 필요해지는 경우

## 보고 형식

`04-implementation-report.md`에는 다음 순서로 작성한다.

1. 구현 요약
2. 변경 파일 목록
3. 리뷰 반영 사항
4. UI 구조 변경 요약
5. 자동 검증 결과
6. 수동 UI 검증 결과
7. 확인하지 못한 항목
8. Root 확인 필요 사항
9. 후속 개선 후보
