# MVP First Loop Final Work Instruction

> 작성일: 2026-05-18
> 작성자: Project Codex
> 대상: Project Claude
> 모드: 구현 요청
> 기반 리뷰: `docs/handoff/mvp-first-loop/02-claude-review.md`

## Project Claude 전달 문장

`docs/handoff/mvp-first-loop/03-final-work-instruction.md`를 읽고 구현 모드로 진행해줘. 지시서의 범위를 벗어난 개선이나 리팩터링은 하지 말고, 구현 후 지정된 검증 명령을 실행한 뒤 결과를 `docs/handoff/mvp-first-loop/04-implementation-report.md`에 작성해줘.

## 구현 결과 보고 경로

- 파일명: `04-implementation-report.md`
- 경로: `docs/handoff/mvp-first-loop/04-implementation-report.md`
- 작성 주체: Project Claude
- 목적: Project Codex가 구현 결과와 검증 결과를 이어받기 위한 보고 문서

## 목적

PRD와 `../docs/current-focus.md`에 정의된 MVP 첫 플레이 루프를 실제 Godot C# 프로젝트에서 플레이 가능한 최소 형태로 구현한다.

이번 구현의 목적은 완성형 게임 시스템이 아니라, "선택이 결과와 상태 변화로 피드백되고, 그 결과가 가문 연대기로 누적된다"는 LegacyLog의 핵심 경험을 검증하는 것이다.

## 수정 범위

다음 파일과 디렉터리만 수정 또는 생성한다.

- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`
- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`
- `docs/handoff/mvp-first-loop/04-implementation-report.md`

필요한 경우 `scripts/`와 `Scenes/` 디렉터리를 생성한다.

## 제외 범위

- 완성형 가문 생성기
- 첫 가문과 초대 인물 이름의 최종 확정
- 첫 MVP 사건들의 최종 문안 확정
- 복잡한 후계, 혼인, 혈통 시스템
- 랜덤 사건 선택
- 저장/불러오기
- 밸런스 수치 확정
- 완성형 UI/아트 스타일 확정
- CI, 배포, 플랫폼 패키징
- 지시서에 없는 리팩터링

## 사전 가정

- 현재 프로젝트는 Godot 4.6.2 + C# (.NET 8) 스켈레톤이다.
- `project.godot`에는 아직 `run/main_scene`이 없을 수 있다.
- 현재 게임플레이용 `.cs` 스크립트와 `.tscn` 씬은 없다는 전제로 진행한다.
- Godot 실행은 반드시 `scripts/godot.cmd` 또는 `scripts/godot.sh` 래퍼를 사용한다.
- 아래에서 지정한 상태 항목, 가문명, 인물명, 사건 문안은 모두 MVP 검증용 임시 값이다. 최종 콘텐츠나 밸런스로 확정하지 않는다.

위 가정이 실제 코드와 다르면 추측으로 진행하지 말고 멈춘 뒤 보고한다.

## 리뷰 반영 요약

### 반영할 제안

- 모델 타입 정의와 MVP 검증용 시드 콘텐츠를 파일 단위로 분리한다.
  - `scripts/MvpLoopModels.cs`
  - `scripts/MvpLoopContent.cs`
- UI는 코드 기반 동적 생성을 채택한다.
  - `Scenes/Main.tscn`은 루트 `Control`에 `Main.cs`를 어태치하는 최소 씬으로 둔다.
  - `Main.cs`는 `_Ready()`에서 필요한 컨테이너, 라벨, 버튼을 생성한다.
- 병렬 작업 분해는 2트랙으로 단순화한다.
  - 작업 A: 모델과 콘텐츠
  - 작업 B: 씬과 진행 로직
- 상태 키와 표시명을 분리한다.
  - 상태 키는 코드 식별자다.
  - 표시명은 한국어 UI 표시용이다.
- `StateDefinition`을 도입하고 `MvpLoopContent`에서 상태 정의를 한 번만 제공한다.
- 선택 결과 적용은 `RunState.ApplyChoice(LoopEvent currentEvent, EventChoice choice)`에서 처리한다.
- `MvpLoopContent.Events`와 `MvpLoopContent.CreateInitialRunState()`를 시드 데이터 접근 진입점으로 둔다.
- 검증 방법에서 `--quit-after 2`를 제거하고 `--smoke` 분기를 추가한다.
- 구현 보고에 수동 GUI 확인 결과와 스모크 출력 발췌를 포함한다.

### 부분 반영할 제안

- Claude 리뷰의 `StateDelta` 제안은 `Key`와 `Amount`만 갖는 방향으로 반영한다. 표시명은 `StateDefinition`에서 조회한다.
- 수동 GUI 확인은 가능한 경우 `.\scripts\godot.cmd` 런타임 실행으로 확인한다. 환경 문제로 GUI 실행이 불가능하면 실행하지 못한 이유를 보고 문서에 명시한다.

### 반영하지 않을 제안

- 없음.

### 사용자 확인 필요 항목

- 없음. Claude 리뷰 기준 PRD 위반과 멈춤 지점은 없다.

## 합류 계약

### 모델 타입

`scripts/MvpLoopModels.cs`에 다음 타입을 둔다.

- `StateDefinition`
  - `string Key`
  - `string Label`
- `StateDelta`
  - `string Key`
  - `int Amount`
- `EventChoice`
  - `string Label`
  - `string ResultText`
  - `string ChronicleText`
  - `IReadOnlyList<StateDelta> Deltas`
- `LoopEvent`
  - `string Title`
  - `string Body`
  - `IReadOnlyList<EventChoice> Choices`
- `ChronicleEntry`
  - `string EventTitle`
  - `string ChoiceLabel`
  - `string RecordedLine`
- `RunState`
  - `string FamilyName`
  - `string FounderName`
  - `string IntroLine`
  - `int CurrentEventIndex`
  - `Dictionary<string, int> Stats`
  - `List<ChronicleEntry> Chronicle`
  - `ApplyChoice(LoopEvent currentEvent, EventChoice choice)` 메서드

`ApplyChoice`는 다음을 함께 수행한다.

- 선택지의 모든 `StateDelta`를 `Stats`에 누적한다.
- 선택 결과를 `ChronicleEntry`로 추가한다.
- `CurrentEventIndex`를 1 증가시킨다.

### 콘텐츠 진입점

`scripts/MvpLoopContent.cs`에 다음 정적 진입점을 둔다.

- `MvpLoopContent.StateDefinitions`
- `MvpLoopContent.Events`
- `MvpLoopContent.CreateInitialRunState()`
- 상태 표시명 조회에 필요한 헬퍼가 필요하면 같은 파일에 둔다.

`Main.cs`는 MVP 시드 데이터에 접근할 때 위 진입점만 사용한다.

### UI 계약

- `Scenes/Main.tscn`은 루트 `Control` 하나를 갖고 `scripts/Main.cs`를 어태치한다.
- 씬 파일에는 자식 UI 노드를 고정하지 않는다.
- `scripts/Main.cs`는 `_Ready()`에서 UI 노드를 동적으로 생성한다.
- `Main.cs`는 시작, 사건, 결과, 세대 요약 상태를 명확히 분리해 렌더링한다.

## 구체적 구현 단계

1. `scripts/`와 `Scenes/` 디렉터리가 없으면 생성한다.
2. `scripts/MvpLoopModels.cs`를 생성한다.
   - 위 합류 계약의 타입을 구현한다.
   - `RunState.ApplyChoice`에서 상태 누적과 연대기 추가를 처리한다.
3. `scripts/MvpLoopContent.cs`를 생성한다.
   - 파일 상단에 상태 키, 상태 표시명, 가문명, 인물명, 사건 문안은 모두 MVP 검증용 임시 값이라는 주석을 남긴다.
   - 상태 정의는 3개만 둔다. 예: `Reputation`/`명망`, `Stores`/`비축`, `Cohesion`/`결속`.
   - 고정된 첫 가문명, 초대 인물명, 도입 문장을 둔다.
   - 3개의 사건을 순서대로 둔다.
   - 각 사건은 2개 이상의 선택지를 가진다.
   - 각 선택지는 결과 문구, 연대기 문장, 하나 이상의 상태 변화를 가진다.
4. `scripts/Main.cs`를 생성한다.
   - 루트는 `Godot.Control`을 상속한다.
   - `_Ready()`에서 UI를 동적으로 생성한다.
   - 일반 실행에서는 시작 화면을 표시한다.
   - 시작 버튼을 누르면 첫 사건 화면으로 들어간다.
   - 선택지를 누르면 결과 문구, 상태 변화, 추가된 연대기 항목을 보여준다.
   - 결과 확인 후 다음 사건으로 진행한다.
   - 3개 사건 후 세대 요약을 표시한다.
   - `--smoke` 인자를 감지하면 UI 조작 없이 자동 스모크를 실행한다.
5. `Scenes/Main.tscn`을 생성한다.
   - 루트 `Control`에 `scripts/Main.cs`를 어태치한다.
6. `project.godot`의 `[application]` 섹션에 메인 씬을 지정한다.
   - `run/main_scene="res://Scenes/Main.tscn"`
7. 자동 스모크를 구현한다.
   - 새 실행을 초기화한다.
   - 각 사건에서 첫 번째 선택지를 자동 선택한다.
   - 시작 정보, 사건별 결과 문구, 상태 변화, 연대기 항목을 stdout에 출력한다.
   - 세대 요약까지 도달하면 `SMOKE_OK`를 출력하고 `GetTree().Quit()`을 호출한다.
   - 이 분기는 MVP 검증용 임시 코드이며 추후 테스트 체계 도입 시 제거 대상이라는 주석을 남긴다.
8. `docs/handoff/mvp-first-loop/04-implementation-report.md`를 작성한다.
   - 아래 보고 형식에 맞춰 변경 파일, 구현 흐름, 검증 결과, 남은 제약을 정리한다.

## 병렬 작업 분해

### 작업 A: 모델과 MVP 검증용 콘텐츠

소유 파일:

- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`

책임:

- 모델 타입 구현
- `RunState.ApplyChoice` 구현
- 상태 정의, 시작 상태, 3개 사건, 선택지별 결과/연대기/상태 변화 작성
- MVP 검증용 임시 값 주석 작성

선행 의존성:

- 없음.

합류 지점:

- 작업 B는 `MvpLoopContent.StateDefinitions`, `MvpLoopContent.Events`, `MvpLoopContent.CreateInitialRunState()`만 호출한다.

### 작업 B: 씬과 진행 로직

소유 파일:

- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`
- `docs/handoff/mvp-first-loop/04-implementation-report.md`

책임:

- 루트 씬 생성
- 메인 씬 지정
- 동적 UI 생성
- 시작, 사건, 결과, 세대 요약 흐름 구현
- `--smoke` 자동 스모크 구현
- 검증 명령 실행 및 보고 문서 작성

선행 의존성:

- 작업 A의 public 타입과 콘텐츠 진입점 계약.

충돌 위험:

- `MvpLoopModels.cs`의 타입명이나 프로퍼티명이 합류 계약과 달라지면 `Main.cs` 컴파일이 깨진다. 계약 변경이 필요하면 임의 변경하지 말고 보고한다.

### 병렬화하지 말아야 할 작업

- 최종 검증은 모든 구현이 합쳐진 뒤 순차 실행한다.
- 보고 문서는 구현과 검증이 끝난 뒤 작성한다.

## 검증 명령

Project Claude는 구현 후 다음을 실행하고 결과를 보고한다.

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

가능하면 수동 GUI 확인도 1회 수행한다.

```powershell
.\scripts\godot.cmd
```

수동 GUI 확인을 실행하지 못하면 이유를 `docs/handoff/mvp-first-loop/04-implementation-report.md`에 적는다.

## 완료 기준

- 새 실행을 시작할 수 있다.
- 고정된 첫 가문과 초대 인물의 시작 정보를 볼 수 있다.
- 3개의 사건을 순서대로 진행할 수 있다.
- 각 사건에서 2개 이상의 선택지를 고를 수 있다.
- 선택 후 결과 문구와 최소 상태 변화를 볼 수 있다.
- 선택 결과가 연대기 항목으로 누적된다.
- 3개의 사건 후 세대 요약에서 누적 연대기, 최종 상태 요약, 짧은 마무리 문장을 볼 수 있다.
- `dotnet build`가 성공한다.
- `.\scripts\godot.cmd --headless --import`가 성공한다.
- `.\scripts\godot.cmd --headless -- --smoke`가 종료 코드 0으로 끝나고 stdout에 `SMOKE_OK`를 출력한다.
- 구현 결과 보고 문서가 작성된다.

## 보고 형식

Project Claude는 `docs/handoff/mvp-first-loop/04-implementation-report.md`에 다음 형식으로 결과를 작성한다.

- 변경 파일
- 구현된 사용자 흐름
- 실행한 확인 명령과 결과
- 스모크 stdout 발췌
- 수동 GUI 확인 결과
- 남은 제약 또는 미구현 범위

## 멈춤 지점

다음 상황에서는 추측으로 구현하지 말고 보고한다.

- 이미 존재하는 게임플레이 구조가 이 지시서의 스켈레톤 전제와 다를 때
- `project.godot`의 메인 씬 지정이 다른 기존 흐름과 충돌할 때
- PRD에 없는 게임 규칙, 상태 모델, 밸런스 수치를 확정해야 할 때
- 저장 형식, 외부 데이터 로딩, 완성형 UI 구조를 정해야 할 때
- 지정된 검증 명령이 도구 부재나 환경 문제로 실행되지 않을 때
