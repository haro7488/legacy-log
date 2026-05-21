# Vertical Slice 1 UI Layout Improvement - Project Claude Review Request

## Project Claude 전달 문장

`docs/handoff/vertical-slice-1-ui-layout-improvement/01-review-request.md`를 읽고 리뷰 모드로만 검토해 주세요. 바로 구현하지 말고, 구현 가능성, 리스크, 누락된 검증 항목, 더 작은 작업 분해가 필요한 지점을 `docs/handoff/vertical-slice-1-ui-layout-improvement/02-claude-review.md`에 작성해 주세요.

## 리뷰 모드

- 이번 문서는 최종 구현 지시서가 아니다.
- Project Claude는 코드 수정 없이 리뷰만 수행한다.
- Project Codex는 리뷰 결과를 검토한 뒤 `03-final-work-instruction.md`를 별도로 확정한다.

## 1. 읽은 문서 목록과 확인한 제품 기준 요약

### 읽은 문서

- `../AGENTS.md`
- `../docs/current-focus.md`
- `../docs/product/prd.md`
- `../docs/product/vertical-slice-1-ui-layout-direction.md`
- `../docs/product/vertical-slice-1-manual-playtest-guide.md`
- `../docs/decisions/002-agent-workflow.md`
- `../docs/decisions/011-ui-ux-validation-workflow.md`
- `../docs/decisions/012-responsive-tabbed-ui-direction.md`
- `../docs/decisions/014-vertical-slice-1-playtest-scope.md`
- `../docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`
- `../docs/decisions/016-root-planning-project-implementation-boundary.md`
- `AGENTS.md`
- `CLAUDE.md`
- `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`
- `docs/handoff/vertical-slice-1-playtest-build/05-root-status-summary.md`
- `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`
- `scripts/Main.cs`

### 확인한 제품 기준

- Vertical Slice 1은 기능 구현과 사용자 수동 확인이 완료된 상태로 보고되었다.
- 이번 작업은 새 규칙, 새 사건, 밸런스 변경이 아니라 UI 구조 개선이다.
- 목표 UI는 상단 헤더, 중간 단일 게임 화면, 하단 탭 메뉴의 3단 구조다.
- 하단 탭은 보조 정보 패널이 아니라 `사건`, `상태`, `연대기`, `가문사` 주 화면 전환 메뉴다.
- 사건 선택지와 진행 버튼은 별도 하단 `ActionBar`보다 사건/결과/요약 화면 내부 주요 행동 영역에 배치하는 방향을 우선한다.
- 작은 창과 세로형 화면에서도 헤더, 본문, 선택지, 하단 탭이 겹치거나 잘리면 안 된다.
- 기능 회귀는 smoke로 확인하고, UI 품질은 수동 검증으로 따로 확인한다.

## 2. 현재 UI 구조 요약

현재 `scripts/Main.cs`는 코드 기반 동적 UI를 구성한다.

- `BuildShell()`은 `ContextHeader`, `MainScroll(MainContent)`, `InfoTabs`, `ActionBar` 4영역을 만든다.
- `ContextHeader`는 두 줄 라벨이다.
  - 1행: 가문명, 대표작위, 세대, 현재 인물
  - 2행: 현재 단계, 화면 목적, 작위 위험 경고
- `MainScroll(MainContent)`는 세대 시작, 사건, 결과, 세대 요약, 멸문 본문을 표시한다.
- `InfoTabs`는 `상태`, `연대기`, `가문사` 3개 탭이지만 현재는 본문 아래의 보조 패널이다.
- `ActionBar`는 화면 최하단에 별도로 있으며 선택지 버튼, 다음 버튼, 멸문 안내 라벨을 담는다.

현재 화면별 표시 방식은 다음과 같다.

- 세대 시작: 이전 세대 결과, 핵심 유산, 출발 상태, 현 인물 특성, 압박 문장을 `MainContent`에 표시하고 시작 버튼은 `ActionBar`에 둔다.
- 사건: 사건 진행도, 분류, 제목, 본문, 조건 단서를 `MainContent`에 표시하고 선택지 버튼과 영향 요약은 `ActionBar`에 둔다.
- 결과: 결과 문장, 상태 변화, 유산 변화, 작위 변화/위험, 연대기 기록을 `MainContent`에 표시하고 다음 버튼은 `ActionBar`에 둔다.
- 세대 요약: 세대 인물, 시작/종료 작위, 종료 유형, 대표 유산, 요약 문단을 `MainContent`에 표시하고 다음 세대/멸문 버튼은 `ActionBar`에 둔다.
- 멸문: 멸문 제목, 요약, 전체 이력을 `MainContent`에 표시하고 종료 안내는 `ActionBar`에 둔다.
- 상태/연대기/가문사: `InfoTabs`의 보조 탭으로 항상 본문 아래에 존재한다.

현재 구조와 Root 방향의 핵심 차이는 두 가지다.

- `InfoTabs`가 주 화면 전환 메뉴가 아니라 보조 패널이다.
- 선택지와 진행 버튼이 사건 화면 내부가 아니라 별도 하단 `ActionBar`에 있다.

## 3. 3단 UI 구조 전환 계획

### 목표 노드 구조

`Main.cs`의 UI shell을 다음 3영역으로 재구성한다.

- `ContextHeader`
- `GameScroll(GameContent)`
- `BottomTabs`

권장 구현 방식은 현재 단일 `Main.cs` 동적 UI 구조를 유지하면서 노드 책임만 재배치하는 것이다. 이번 변경만으로 별도 씬이나 리소스 파일을 추가할 필요는 없다.

### 상단 헤더에 남길 정보

상단 헤더는 항상 보이는 현재 맥락만 유지한다.

- 가문명
- 대표작위
- 세대
- 현재 인물
- 현재 단계
- 화면 목적
- `Crisis` 이상 작위 위험 경고

상태 상세, 연대기, 유산 태그, seed는 헤더에서 제외한다.

### 중간 단일 게임 화면으로 옮길 정보

`GameScroll(GameContent)`는 선택된 하단 탭의 전체 내용을 표시한다.

- `사건` 탭: 세대 시작, 사건, 결과, 세대 요약, 멸문 화면
- `상태` 탭: 상태 질적 구간, 작위 위험, 현재 인물 특성, 상세 수치 접기/펼치기
- `연대기` 탭: 최신 기록, 큰 사건, 보조 사건
- `가문사` 탭: 활성 유산 태그, 보유 작위, 지난 세대 이력

탭 전환 시 현재 사건 진행 상태를 변경하지 않고 표시 화면만 바꾼다.

### 하단 탭 메뉴 구성

현재 `InfoTabs`는 제거하거나 `BottomTabs`로 역할을 바꾼다. 권장안은 `TabContainer`를 중간 본문 아래에 두는 방식이 아니라, 하단에 고정된 `HBoxContainer` 또는 `TabBar` 성격의 버튼 4개를 두고 `GameContent`를 다시 렌더링하는 방식이다.

하단 탭:

- `사건`
- `상태`
- `연대기`
- `가문사`

Godot 4.6.2 C#에서 단순하고 검증하기 쉬운 선택지는 `HBoxContainer` + 4개 `Button`이다. 선택된 탭 버튼은 `Disabled` 또는 테마 변형 대신 텍스트 접두/상태 색상처럼 최소 변경으로 구분한다.

### 탭 전환 상태 관리

신규 enum을 `Main.cs` 내부에 둔다.

```csharp
private enum MainTab
{
    Event,
    State,
    Chronicle,
    FamilyHistory,
}
```

`Main.cs`에 다음 상태를 추가한다.

- `_selectedTab`
- `_eventScreen`

`_eventScreen`은 사건 탭 내부의 실제 흐름 상태를 나타낸다.

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

탭 전환은 `SelectTab(MainTab tab)`으로 통일한다.

- `_selectedTab` 갱신
- 헤더 갱신
- `GameContent` 재렌더
- 하단 탭 선택 상태 갱신
- 스크롤 상단 이동

사건 진행 함수는 흐름 상태를 바꾼 뒤 `RenderSelectedTab()`을 호출한다.

## 4. 사건/상태/연대기/가문사 탭별 표시 계획

### 사건 탭

사건 탭은 기존 세대 흐름을 그대로 유지한다.

- 세대 시작 화면
- 사건 화면
- 결과 화면
- 세대 요약 화면
- 멸문 또는 실행 종료 화면

선택지와 진행 버튼은 `GameContent` 안의 하단부 주요 행동 영역에 배치한다. 별도 `ActionBar`는 만들지 않는다.

권장 구조:

- 본문 블록
- 판단 단서/영향 요약 블록
- `HSeparator`
- 행동 영역 블록

사건 화면에서는 각 선택지를 하나의 세로 블록으로 렌더링한다.

- 버튼: 선택 행동명
- 바로 아래: 영향 요약

결과 화면은 Root 기준 순서를 유지한다.

1. 결과 문장
2. 상태 변화
3. 유산 태그 변화
4. 작위 변화 또는 위험 변화
5. 연대기 기록
6. 다음 버튼

세대 시작과 세대 요약 화면도 진행 버튼을 화면 내부 마지막 행동 영역에 둔다.

멸문 화면은 진행 버튼 없이 종료 안내를 화면 내부 마지막에 둔다.

### 상태 탭

상태 탭은 숫자 없이도 판단 가능한 요약을 먼저 보여준다.

우선순위:

1. 핵심 상태 4축의 질적 구간
2. 작위 위험 단계
3. 현재 인물 성향, 강점, 약점
4. 상세 수치 접기/펼치기
5. seed

기존 `_stateDetailVisible`은 유지한다. 단, `RenderStateTab()`은 보조 탭 내부가 아니라 `GameContent`를 채우는 `RenderStateScreen()`으로 바꾼다.

### 연대기 탭

연대기 탭은 최신 기록과 큰 사건이 구분되어야 한다.

표시 우선순위:

1. 현재 세대 연대기 제목
2. 최신 기록 1건
3. 큰 사건 목록
4. 보조 사건 목록
5. 기록이 없을 때 안내 문장

기존 `Vs1ChronicleEntry.IsMajor`를 사용해 큰 사건과 보조 사건을 분리한다. 새 규칙은 추가하지 않는다.

### 가문사 탭

가문사 탭은 이전 세대의 흔적이 현재 세대와 연결되어 보이게 한다.

표시 우선순위:

1. 활성 유산 태그와 지속 범위
2. 유산 태그 짧은 설명
3. 보유 작위
4. 지난 세대 이력
5. 멸문 상태에서는 전체 이력과 종료 이유

기존 `ActiveTags`, `HeldTitles`, `History`, `IsExtinct` 데이터를 사용한다. 새 태그, 새 종료 사유, 새 작위 규칙은 만들지 않는다.

## 5. 검증 계획

### 기능 smoke 검증

기존 VS1 기능 smoke는 유지한다.

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

검증 목표:

- 빌드 오류 없음
- Godot headless import 성공
- `SMOKE_OK`
- 기존 3-run/4-assert smoke 성격 유지
- UI 리팩터가 `Vs1Flow`, `Vs1Content`, 사건 데이터에 기능 회귀를 만들지 않음

### UI 수동 검증

수동 검증은 `../docs/product/vertical-slice-1-manual-playtest-guide.md`와 이번 UI 방향 문서의 체크리스트를 함께 따른다.

검증 창 크기:

- `360x640`
- `540x720`
- `1280x720`

확인 항목:

- 헤더만 보고 가문명, 대표작위, 세대, 현재 인물, 현재 단계가 이해되는가
- 하단 탭으로 `사건`, `상태`, `연대기`, `가문사`가 전환되는가
- 하단 탭이 보조 패널처럼 보이지 않고 주 화면 전환으로 동작하는가
- 사건 탭을 떠났다가 돌아와도 현재 사건/결과/요약 상태가 유지되는가
- 사건 화면에서 선택지와 영향 요약이 함께 읽히는가
- 결과 화면에서 상태 변화, 유산 태그 변화, 작위 변화/위험 변화, 연대기 기록 순서가 이해되는가
- 상태 탭에서 숫자 상세를 열지 않아도 현재 위험과 강점이 보이는가
- 연대기 탭에서 최신 기록과 큰 사건/보조 사건이 구분되는가
- 가문사 탭에서 활성 유산 태그와 지난 세대 이력이 현재 세대와 연결되어 보이는가
- `360x640`, `540x720`, `1280x720`에서 하단 탭, 선택지, 본문이 겹치거나 잘리지 않는가

### 보고 방식

Project Claude 구현 보고서에는 다음을 분리해 적는다.

- 자동 검증 결과
- 수동 UI 검증 수행 여부
- 각 창 크기별 확인 결과
- 확인하지 못한 항목과 이유
- Root 제품 기준과 충돌하거나 판단이 필요한 항목

## 6. Project Claude 최종 구현 지시서 초안

아래는 리뷰 대상 초안이다. Project Claude 리뷰를 반영한 뒤 `03-final-work-instruction.md`에서 확정한다.

### 목적

Vertical Slice 1의 기존 기능 흐름을 유지하면서 UI를 상단 헤더, 중간 단일 게임 화면, 하단 탭 메뉴의 3단 구조로 전환한다. 반복 플레이 중 사건 판단, 상태 확인, 연대기 확인, 가문사 확인이 작은 창과 세로형 화면에서도 자연스럽게 오가도록 만든다.

### 범위

- `scripts/Main.cs`의 동적 UI shell 구조 변경
- 기존 `InfoTabs` 보조 패널을 하단 주 화면 전환 탭으로 변경
- 기존 `ActionBar`의 선택지/진행 버튼을 사건 탭 내부 행동 영역으로 이동
- 사건, 상태, 연대기, 가문사 렌더링 함수를 주 화면 렌더링 방식으로 재구성
- 기존 VS1 smoke 검증 유지
- 수동 UI 검증 체크리스트 기반 보고

### 제외 범위

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 상태 수치, 가중치, 밸런스 변경
- 최종 UI/아트 스타일 확정
- 저장/불러오기, 혼인, 혈통 트리 추가
- Godot 씬 분리 또는 새 리소스 구조 도입
- `Vs1Flow`, `Vs1Content`, `Vs1Models`의 게임 규칙 변경

### 사전 가정

- Root 제품 문서가 제품 기준의 source of truth다.
- 현재 VS1 기능은 구현 및 사용자 수동 확인 완료 상태다.
- 이번 변경은 UI 배치와 렌더링 구조 변경이며 게임 규칙 변경이 아니다.
- 탭 전환은 사건 진행 상태를 바꾸지 않는다.
- 자동 smoke는 UI 품질을 보장하지 않으므로 수동 검증을 별도로 보고한다.

### 구현 단계

1. `Main.cs` 상단 합류 계약 주석을 3영역 구조로 갱신한다.
2. `MainTab`, `EventScreen` enum과 `_selectedTab`, `_eventScreen` 상태를 추가한다.
3. `BuildShell()`을 `ContextHeader`, `GameScroll(GameContent)`, `BottomTabs` 3영역 생성으로 바꾼다.
4. `BuildInfoTabs()`와 `BuildActionBar()` 의존을 제거하거나 새 `BuildBottomTabs()`로 대체한다.
5. `SetActionBar()`를 제거하고, 선택지/진행 버튼 생성은 사건 탭 렌더링 내부 행동 영역 헬퍼로 옮긴다.
6. `ShowGenerationStart()`, `ShowEvent()`, `ShowResult()`, `ShowGenerationSummary()`, `ShowExtinction()`은 흐름 상태를 갱신한 뒤 사건 탭 렌더링을 호출하도록 정리한다.
7. 기존 `RenderStateTab()`, `RenderChronicleTab()`, `RenderFamilyHistoryTab()`을 각각 `RenderStateScreen()`, `RenderChronicleScreen()`, `RenderFamilyHistoryScreen()`으로 바꿔 `GameContent`를 채우게 한다.
8. `RenderSelectedTab()`과 `SelectTab()`을 추가해 하단 탭 전환을 단일 경로로 처리한다.
9. 사건 탭을 떠났다가 돌아와도 `_eventScreen`, `_lastApplyResult`, `_pendingEnd`가 유지되는지 확인한다.
10. `dotnet build`, Godot import, smoke를 실행한다.
11. `360x640`, `540x720`, `1280x720`에서 수동 UI 검증을 수행하거나, 수행하지 못했다면 미수행 사유를 보고한다.

### Godot 경로 케이스 계약

- 수정 파일: `scripts/Main.cs`
- 참조 씬: `Scenes/Main.tscn`
- 기존 `res://scripts/Main.cs` 경로 케이스를 유지한다.
- 새 `Scripts/` 또는 `scenes/` 디렉터리를 만들지 않는다.
- 이번 범위에서는 새 `.tscn`, `.tres`, 이미지 리소스를 만들지 않는다.

### 병렬 작업 분해

이번 변경은 `scripts/Main.cs` 단일 파일의 UI 상태와 렌더링 함수가 강하게 결합되어 있어 병렬 구현에 적합하지 않다. Project Claude는 한 작업 흐름으로 순차 구현한다.

리뷰 단계에서 병렬화를 제안하려면 다음 조건을 만족해야 한다.

- `Main.cs`의 서로 다른 함수만 독립적으로 수정해도 충돌이 작을 것
- 사건 진행 상태 계약을 먼저 확정할 것
- 마지막 합류 시 `RenderSelectedTab()` 단일 경로로 통합할 것

### 완료 기준

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

### 검증 방법

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

수동 검증:

- Godot 실행 후 `360x640`, `540x720`, `1280x720`에서 확인
- 최소 1회 플레이에서 사건, 상태, 연대기, 가문사 탭을 반복 전환
- 사건 선택 전후와 세대 요약 전후에 탭 전환 상태 유지 확인
- UI 검증 결과를 구현 보고서에 자동 검증과 분리해 기록

### 보고 형식

Project Claude는 구현 후 `docs/handoff/vertical-slice-1-ui-layout-improvement/04-implementation-report.md`에 다음을 작성한다.

- 구현 요약
- 변경 파일 목록
- UI 구조 변경 요약
- 자동 검증 결과
- 수동 UI 검증 결과
- 확인하지 못한 항목
- Root 확인 필요 사항
- 후속 개선 후보

## 7. Root 확인 필요 질문 목록

현재 구현 계획 수립을 막는 Root 확인 필요 사항은 없다.

다만 Project Claude 리뷰 또는 실제 구현 중 아래 상황이 발견되면 Root 확인으로 되돌린다.

- 하단 탭이 360x640에서 4개 라벨을 모두 안정적으로 담기 어려워 탭 이름 축약이 필요해지는 경우
- 선택지/영향 요약을 사건 화면 내부에 넣었을 때 사건 본문과 선택지가 한 화면에서 지나치게 멀어져 제품 기준 조정이 필요한 경우
- 상태/연대기/가문사 탭에서 새 요약 문구나 새 분류명이 필요해져 기존 Root 문서에 없는 표현을 만들어야 하는 경우
- 작은 창 검증을 통과하려면 헤더 정보 일부를 숨기거나 접어야 하는 경우
