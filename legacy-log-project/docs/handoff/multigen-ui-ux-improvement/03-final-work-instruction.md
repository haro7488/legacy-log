# Multigen MVP UI/UX Improvement Final Work Instruction

> 작성일: 2026-05-19
> 작성자: Project Codex
> 대상: Project Claude
> 모드: 구현 요청
> 기반 리뷰: `docs/handoff/multigen-ui-ux-improvement/02-claude-review.md`
> 사용자 확인: Project Codex 권장 기본안 유지 승인

## Project Claude 전달 문장

`docs/handoff/multigen-ui-ux-improvement/03-final-work-instruction.md`를 읽고 구현 모드로 진행해줘. 지시서의 범위를 벗어난 기능 추가나 리팩터링은 하지 말고, 구현 후 지정된 기능 검증과 수동 UI/UX 검증 기준을 분리해 `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md`에 작성해줘.

## 구현 결과 보고 경로

- 파일명: `04-implementation-report.md`
- 경로: `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md`
- 작성 주체: Project Claude
- 목적: Project Codex가 UI/UX 개선 결과, 기능 보존 여부, 수동 검증 상태를 이어받기 위한 보고 문서

## 목적

현재 다세대 MVP는 시작, 사건, 결과, 세대 요약, 후계, 2세대 진입, 멸문까지 기능 경로가 구현되어 있다. 이번 구현의 목적은 새 게임 기능을 추가하는 것이 아니라, 작은 창과 세로형 화면에서도 사용자가 직접 시작부터 멸문까지 플레이하며 UI/UX를 검증할 수 있도록 정보 구조와 조작 흐름을 정리하는 것이다.

## 수정 범위

다음 파일만 수정 또는 생성한다.

- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`
- `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md`

원칙:

- `Scenes/Main.tscn`은 기존 루트 `Control` + `res://scripts/Main.cs` 어태치 구조를 유지한다. 불필요하면 변경하지 않는다.
- `project.godot`에는 이번 작업에서 `[display]` 설정을 추가하지 않는다.
- 새 UI 빌더 파일은 만들지 않는다. `Main.cs` 안에서 영역별 빌더 메서드로 정리한다.

## 제외 범위

- 새 게임 규칙 추가
- 저장/불러오기
- 재시작 버튼 또는 새 실행 버튼 추가
- 설정 화면 추가
- 완성형 UI/아트 스타일 확정
- 색상, 폰트, 장식 스타일 결정
- 시대/세계관 톤 결정
- 가문/인물 이름 후보 확정
- 상태 값 이름, 구간 라벨 최종 문구, 수치, 밸런스 확정
- 후계 가능성 계산식, 감쇠율, 특성 상속 확률 확정
- 혼인, 혈통, 대량 사건 콘텐츠
- `MultigenFlow`, `MultigenModels`, `MultigenContent`, `MvpLoopModels`, `MvpLoopContent`의 게임 흐름 변경

## 사전 가정

- 현재 다세대 MVP 기능 구현은 출발점으로 삼는다.
- 기존 `--smoke` 기능 흐름은 유지한다.
- UI/UX 개선은 기능 흐름을 깨지 않는 범위에서 진행한다.
- 작은 창과 세로형 화면 대응을 우선한다.
- UI는 탭 기반 보조 정보 구조를 사용한다.
- 기능 검증과 UI/UX 수동 검증은 분리해서 보고한다.
- 자리표시자 문구와 `구간 1`~`구간 4` 상태 라벨은 그대로 둔다.

위 가정이 실제 코드와 다르면 추측으로 진행하지 말고 보고한다.

## 리뷰 반영 요약

### 반영할 제안

- `ContextHeader`, `MainScroll`, `InfoTabs`, `ActionBar` 4영역 구조를 채택한다.
- `MainScroll`만 주요 확장/스크롤 영역으로 둔다.
- 탭 갱신은 단일 진입점으로 모은다.
- 화면 전환 시 본문 스크롤을 최상단으로 돌린다.
- 멸문 화면에는 재시작 버튼을 넣지 않고 비활성 종료 안내를 둔다.
- `--smoke`는 기능 회귀 검증으로 유지하되 UI 회귀 검증 도구가 아님을 보고한다.
- 수동 UI/UX 검증은 별도 섹션으로 보고한다.

### 사용자 승인으로 확정한 기본안

- `project.godot` display 설정은 변경하지 않는다. 수동 검증자가 창을 조정해 검증한다.
- 탭은 3개로 둔다: `상태`, `연대기`, `가문사`.
- `유산` 정보는 별도 탭이 아니라 `가문사` 탭 상단 요약으로 흡수한다.
- 상세 숫자/방향은 `상태` 탭 안에서 토글로 유지한다.
- 멸문 화면 ActionBar에는 비활성 종료 안내만 표시한다.
- 화면 전환 시 사용자가 마지막으로 선택한 탭은 유지한다.
- 수동 검증 창 크기 기준은 `360x640`, `540x720`, `1280x720` 3종으로 둔다.

### 반영하지 않을 제안

- `project.godot`에 `[display]` 섹션을 추가하지 않는다.
- UI shell만 확인하는 별도 `--ui-smoke`는 만들지 않는다.
- `유산` 탭은 만들지 않는다.

## 합류 계약

### UI 구조 계약

`scripts/Main.cs` 안에서 다음 영역을 구성한다.

```text
Control (Main, FullRect)
└─ MarginContainer
   └─ VBoxContainer AppRoot
      ├─ ContextHeader
      ├─ ScrollContainer MainScroll
      │  └─ VBoxContainer MainContent
      ├─ TabContainer InfoTabs
      └─ VBoxContainer ActionBar
```

계약:

- 루트 `Control`만 `FullRect`를 직접 잡는다.
- 내부 컨테이너는 부모 컨테이너의 레이아웃을 따른다.
- `MainScroll`이 주요 확장 영역이다.
- `InfoTabs`는 너무 커져 `ActionBar`를 밀어내지 않도록 고정 또는 최소 높이를 신중히 둔다.
- `MarginContainer`는 작은 화면 안전 여백을 제공한다.
- `Scenes/Main.tscn`에는 새 정적 자식 노드를 만들지 않는다.

### 영역별 메서드 계약

`Main.cs` 안에 영역별 빌더/렌더러를 둔다.

- `BuildContextHeader()`
- `BuildMainScroll()`
- `BuildInfoTabs()`
- `BuildActionBar()`
- `RefreshContextHeader(...)`
- `RenderInfoTabs()`
- `SetActionBar(...)`
- `ClearMainContent()`
- `ResetMainScroll()`

정확한 메서드명은 Project Claude가 코드 스타일에 맞춰 조정할 수 있다. 단, 역할 경계는 유지한다.

### 항상 보이는 정보 계약

`ContextHeader`에는 다음을 항상 표시한다.

- 가문명
- `가문력 N년`
- 현재 인물
- 현재 단계
- 화면 목적 문장

작은 폭 대응:

- 헤더를 한 줄에 억지로 넣지 않는다.
- 필요하면 2줄 `VBoxContainer` 또는 짧은 라벨 조합으로 구성한다.
- 현재 단계 문구는 `[단계: ...]` 자리표시자 형식으로 둔다.

### 탭 구조 계약

탭은 3개만 둔다.

| 탭 | 포함 정보 |
|---|---|
| `상태` | 4단계 질적 라벨, 상세 숫자/방향 토글, 현재 인물 특성 스텁 |
| `연대기` | 현재 세대 `ChronicleEntry` 목록, 최신 선택 기록 |
| `가문사` | 유산 요약, 누적 `CarriedOutcomeTags`, 끝난 세대 `History`, 멸문 시 전체 이력 |

`유산`은 별도 탭으로 만들지 않는다.

탭 동작:

- 화면 전환 시 사용자가 마지막으로 연 탭 선택을 유지한다.
- 단, 탭 내용은 화면 전환/선택 적용/세대 전환/멸문 처리 후 최신 상태로 다시 그린다.
- 탭 노드 이름과 표시 라벨을 분리할 수 있으면 분리한다.

### 상태 상세 정보 계약

`상태` 탭 안에서 정보 계층을 유지한다.

- 기본 영역: 상태별 `구간 1`~`구간 4` 질적 라벨
- 상세 영역: 토글로 여닫는 숫자/방향
- 특성 스텁: 표시 전용 정보로 같은 탭에 둔다.

상태 라벨 문구는 변경하지 않는다.

### 주요 행동 영역 계약

`ActionBar`는 현재 화면의 주요 행동을 보여준다.

- 시작: `첫 사건으로`
- 사건: 선택지 버튼 2개
- 결과: `다음 사건으로` 또는 `세대 요약 보기`
- 세대 요약: `후계를 본다` 또는 `가문의 끝을 본다`
- 후계: 후계 후보 선택 또는 다음 세대로 진입
- 멸문: 진행 버튼 없음. 비활성 종료 안내 라벨만 표시

사건 화면 선택지는 `ActionBar`에 둔다.

제약:

- 현재 MVP 선택지는 2개이므로 `ActionBar`에 둔다.
- 선택지가 3개를 초과하는 미래 상황은 이번 범위에서 처리하지 않는다. 필요 시 후속 작업으로 넘긴다.

### 화면 전환 계약

- 각 화면 메서드는 `RefreshContextHeader`, `ClearMainContent`, `SetActionBar`, `RenderInfoTabs`, `ResetMainScroll` 흐름을 명확히 호출한다.
- `History.Add` 시점은 기존과 동일하게 `AfterSummary`에서 유지한다.
- 세대 요약 화면에서 `가문사` 탭은 아직 현재 세대를 `History`에 포함하지 않았다는 점을 깨뜨리지 않는다.
- 멸문 화면 진입 시 `_family.IsExtinct`를 기준으로 진행 버튼을 만들지 않는다.

### 스모크 계약

- 기존 `RunSmoke()` 경로와 stdout 형식을 변경하지 않는다.
- `--smoke`는 UI 노드를 만들지 않는 기능 검증용 경로로 유지한다.
- 구현 보고서에 `--smoke`가 UI 회귀를 검증하지 못한다는 한계를 명시한다.

## 구체적 구현 단계

### 1. 기존 UI 필드 정리

`Main.cs`에서 기존 UI 필드를 새 영역 구조에 맞춰 정리한다.

예상 제거 또는 대체:

- `_content`
- `_statsPanel`
- `_headerLabel`
- `_detailToggleButton`
- `_detailPanel`
- `_detailVisible`

예상 신규 또는 대체:

- `_contextHeader`
- `_mainScroll`
- `_mainContent`
- `_infoTabs`
- `_actionBar`
- `_stateTabContent`
- `_chronicleTabContent`
- `_familyHistoryTabContent`
- `_stateDetailVisible`
- `_currentStepLabel` 또는 현재 단계 상태값

정확한 필드명은 Project Claude가 정한다.

### 2. UI shell 재구성

`BuildShell()`을 4영역 구조로 재작성한다.

- `MarginContainer` 추가
- `ContextHeader` 생성
- `ScrollContainer` + `MainContent` 생성
- `TabContainer` + 3개 탭 생성
- `ActionBar` 생성

`Scenes/Main.tscn`은 변경하지 않는 것이 원칙이다.

### 3. 화면별 렌더링 분리

각 화면 메서드를 주요 내용과 행동 버튼 중심으로 정리한다.

- `ShowGenerationStart`
- `ShowEvent`
- `ShowResult`
- `ShowGenerationSummary`
- `ShowSuccession`
- `ShowExtinction`

각 화면은 다음을 명확히 설정한다.

- 현재 단계
- 화면 목적 문장
- 주요 본문
- 주요 행동 버튼
- 탭 정보 최신화

### 4. 탭 렌더링 구현

`RenderInfoTabs()` 또는 동등 메서드에서 3개 탭을 갱신한다.

`상태` 탭:

- 상태별 4단계 라벨
- 상세 숫자/방향 토글
- 현재 인물 특성 스텁

`연대기` 탭:

- 현재 세대의 `ChronicleEntry`
- 기록이 없으면 자리표시자 안내

`가문사` 탭:

- 유산 요약
- 누적 `CarriedOutcomeTags`
- `History`의 세대별 요약
- 멸문 화면에서는 전체 이력

### 5. 작은 창/세로형 대응

- 긴 텍스트 라벨은 `AutowrapMode`를 설정한다.
- 본문은 `MainScroll` 안에서만 스크롤된다.
- 화면 전환 시 `MainScroll`을 최상단으로 돌린다.
- 버튼은 세로 목록으로 둔다.
- 탭은 3개로 유지한다.

### 6. 기능 검증 실행

다음 명령을 실행한다.

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

셸 환경상 `godot.sh`를 사용해야 하면 동등 래퍼를 사용하고 이유를 보고한다.

### 7. 수동 UI/UX 검증 준비 및 보고

구현 후 수동 검증 기준을 `04-implementation-report.md`에 포함한다.

Project Claude가 직접 GUI를 신뢰할 수 없는 환경이면 그 이유를 쓰고, 사용자가 수행할 체크리스트를 남긴다.

## 검증 기준

### 기능 검증 기준

- `dotnet build` 성공
- Godot headless import 성공
- `--smoke` 종료 코드 0
- `--smoke` 마지막 줄 `SMOKE_OK`
- 1세대 후계 정상 경로와 2세대 멸문 경로가 기존처럼 유지됨

### UI/UX 수동 검증 기준

다음 3개 창 크기에서 시작부터 멸문까지 1회씩 확인한다.

- `360x640`
- `540x720`
- `1280x720`

확인 항목:

- 현재 가문, 세대, 인물, 단계가 항상 보인다.
- 각 화면의 주요 행동 버튼을 찾을 수 있다.
- 사건 본문과 선택지 버튼이 구분된다.
- 결과 문구, 상태 변화, 연대기 기록이 구분된다.
- 세대 요약에서 유산과 다음 세대 암시가 보인다.
- 후계 화면에서 다음 세대로 이어지는 조작이 명확하다.
- 2세대 진입 후 `가문사` 탭에서 이전 세대 정보와 유산 요약을 확인할 수 있다.
- 멸문 화면에서 진행 버튼이 없고 종료 안내가 보인다.
- `상태`, `연대기`, `가문사` 탭이 최신 정보로 갱신된다.
- 탭 선택 유지 동작이 계획과 일치한다.
- 화면 전환 시 본문 스크롤이 최상단으로 돌아간다.
- 작은 창에서 텍스트와 버튼이 잘리거나 겹치지 않는다.
- 자리표시자 문구가 최종 제품 문안처럼 보이지 않는다.

## 완료 기준

- 기존 다세대 MVP 기능 흐름이 유지된다.
- `ContextHeader`, `MainScroll`, `InfoTabs`, `ActionBar` 4영역 구조가 구현된다.
- 항상 보이는 맥락 정보가 표시된다.
- 탭은 `상태`, `연대기`, `가문사` 3개로 구성된다.
- `유산` 정보는 `가문사` 탭에 흡수된다.
- 상태 상세 숫자/방향은 `상태` 탭 안에서 토글로 확인할 수 있다.
- 멸문 화면에는 재시작 버튼 없이 종료 안내가 표시된다.
- `project.godot` display 설정은 변경하지 않는다.
- `Scenes/Main.tscn`은 기존 스크립트 참조를 유지한다.
- 기능 검증 결과와 UI/UX 수동 검증 기준이 구현 보고서에서 분리된다.

## 보고 형식

Project Claude는 `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md`에 다음 형식으로 작성한다.

- 변경 파일
- 리뷰 반영 요약
- 구현된 UI 구조
- 화면별 변경 내용
- 탭 구조와 정보 배치
- 작은 창/세로형 대응 내용
- 기능 보존 검증 결과
- 자동 검증 결과
- 수동 UI/UX 검증 결과 또는 미수행 사유
- 3개 창 크기별 수동 검증 체크리스트
- 남은 제약 또는 미구현 범위
- 다음 작업 후보

## 멈춤 지점

다음 상황에서는 추측으로 구현하지 말고 보고한다.

- UI 개선을 위해 PRD에 없는 새 게임 기능이 필요할 때
- 저장/불러오기, 재시작, 설정 화면이 필요할 때
- 색상, 폰트, 장식 스타일 결정을 해야 할 때
- 상태 라벨 문구나 밸런스 수치를 바꿔야 할 때
- 3개 탭 구조로는 현재 정보를 담기 어렵다고 판단될 때
- `project.godot` display 설정 변경이 필요하다고 판단될 때
- `Main.cs` 단일 파일 유지가 어렵고 새 UI 파일 분리가 필요하다고 판단될 때
- `Scenes/Main.tscn`의 정적 자식 노드 추가가 필요하다고 판단될 때
