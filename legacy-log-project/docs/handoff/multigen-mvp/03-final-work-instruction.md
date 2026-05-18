# Multigen MVP Final Work Instruction

> 작성일: 2026-05-18
> 작성자: Project Codex
> 대상: Project Claude
> 모드: 구현 요청
> 기반 리뷰: `docs/handoff/multigen-mvp/02-claude-review.md`

## Project Claude 전달 문장

`docs/handoff/multigen-mvp/03-final-work-instruction.md`를 읽고 구현 모드로 진행해줘. 지시서의 범위를 벗어난 개선이나 리팩터링은 하지 말고, 구현 후 지정된 검증 명령을 실행한 뒤 결과를 `docs/handoff/multigen-mvp/04-implementation-report.md`에 작성해줘.

## 구현 결과 보고 경로

- 파일명: `04-implementation-report.md`
- 경로: `docs/handoff/multigen-mvp/04-implementation-report.md`
- 작성 주체: Project Claude
- 목적: Project Codex가 구현 결과와 검증 결과를 이어받기 위한 보고 문서

## 목적

기존 단일 세대 첫 루프 모듈을 다세대 MVP의 한 세대 실행기로 유지하면서, 세대 종료 후 후계 또는 멸문 판단, 다음 세대 진입, 가문 연대기 누적, 상태 표시 계층화를 구현한다.

제품 기준의 목표는 LegacyLog가 "가문이 멸문할 때까지 다세대를 이어가며 가문의 시작과 끝을 경험하는 게임"이라는 정체를 최소 구현으로 검증하는 것이다.

## 수정 범위

다음 파일만 수정 또는 생성한다.

- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`
- `scripts/MultigenModels.cs`
- `scripts/MultigenContent.cs`
- `scripts/MultigenFlow.cs`
- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`
- `docs/handoff/multigen-mvp/04-implementation-report.md`

`project.godot`와 `Scenes/Main.tscn`은 기존 메인 씬 경로와 루트 스크립트 연결이 유지되는지 확인한다. 불필요한 변경은 하지 않는다.

## 제외 범위

- 완성형 가문 생성기
- 저장/불러오기
- 완성형 혼인, 혈통, 계보 시뮬레이션
- 정교한 자원 경제
- 대량 사건 콘텐츠
- 완성형 UI/아트 스타일 확정
- 시대/세계관 톤 결정
- 가문/인물 이름 후보 확정
- 상태 값 이름, 개수, 최종 표시명, 수치, 밸런스 확정
- 평판형 감쇠율 확정
- 후계 가능성 계산식 확정
- 특성 상속 확률 확정
- 상태 구간 라벨의 최종 문구 확정
- 추가 정보창의 구체 UI 디자인 확정
- PRD와 결정문서에 없는 게임 규칙 추가
- 지시서에 없는 리팩터링

## 사전 가정

- 현재 구현은 단일 세대 첫 루프 모듈의 동작 검증 완료 상태다.
- `mvp-first-loop` handoff 결과를 출발점으로 삼는다.
- PRD와 결정문서 003-010은 확정된 제품 요구로 본다.
- 결정 보류 항목은 구현에서 최종 규칙처럼 확정하지 않는다.
- 후계, 상태 감쇠, 상태 구간, 특성은 MVP 검증용 자리표시자 로직으로만 다룬다.
- Godot 실행은 반드시 `scripts/godot.cmd` 또는 `scripts/godot.sh` 래퍼를 사용한다.

위 가정이 실제 코드와 다르면 추측으로 구현하지 말고 보고한다.

## 리뷰 반영 요약

### 반영할 제안

- `RunState`는 리네임하지 않고 한 세대 사건 진행 상태로 책임을 좁힌다.
- 가문명과 가문 단위 상태의 진실 출처는 `FamilyRunState`로 둔다.
- 큰 사건 결과 입력을 위해 `EventChoice.OutcomeTag`를 추가한다.
- 후계 판정은 `Placeholder*` 명명과 코드 주석으로 검증용임을 명확히 한다.
- 세대 종결 enum은 5종을 둘 수 있지만 자동 흐름에서 실제 발화하는 유형은 2종으로 제한한다.
- 특성 스텁은 표시 전용으로 제한하고 어떤 계산에도 넣지 않는다.
- 상태 구간 라벨은 4단계 자리표시자 라벨로 구현한다.
- 세대 끝 상태 서술 요약은 후대 기록자 요약 단락에 통합한다.
- 스모크는 한 실행에서 후계 정상 경로와 멸문 경로를 모두 검증한다.
- 병렬 작업 분해는 3트랙으로 단순화한다.
- 임시값 위생 작업은 같은 구현 흐름에 포함하되 작업/커밋 단위로 분리한다.

### 부분 반영할 제안

- 리뷰 문서의 사용자 재확인 권장 4항은 Project Codex 판단으로 최종 지시서에 반영한다. 멈춤 지점은 발동되지 않았고, 권고 항목은 PRD와 결정문서 범위 안의 합류 계약 보강으로 본다.
- 위생 작업 커밋 단위 분리는 구현 세션의 커밋 수행 여부에 맞춰 적용한다. 커밋을 수행한다면 아래 "작업 단위"를 커밋 단위로 분리하고, 커밋하지 않는 세션이면 구현 보고서에서 같은 단위로 변경을 구분해 보고한다.

### 반영하지 않을 제안

- 없음.

## 합류 계약

### 1. `RunState` 책임

`RunState`는 한 세대 사건 진행 상태로 좁힌다.

- 유지:
  - `CurrentEventIndex`
  - `Chronicle`
  - `ApplyChoice(...)`
- 제거 또는 이전:
  - `FamilyName`은 `FamilyRunState.FamilyName`으로 이동한다.
  - `Stats`는 `FamilyRunState.FamilyStats`를 진실 출처로 둔다.
- 의미 재해석:
  - `FounderName`은 더 이상 "초대 인물" 고정 의미로 쓰지 않는다. 가능하면 `RunState`에서 제거하고 `FamilyRunState.CurrentCharacterName`을 사용한다.
  - `IntroLine`도 가능하면 `RunState`에서 제거하고 `MultigenContent` 또는 `FamilyRunState` 흐름에서 제공한다.

대규모 리네임은 하지 않는다. 단, 데이터 중복을 만들지 않는다.

### 2. 큰 사건 결과 표식

`EventChoice`에 `OutcomeTag`를 추가한다.

- 타입: `string`
- 값: `outcome-1`, `outcome-2`처럼 자리표시자 톤이 드러나는 문자열
- 목적: 결정 008의 "큰 사건 결과"를 후계/세대 전환 입력으로 넘기는 최소 표식
- 금지: 최종 사건 분류, 세계관 톤, 밸런스 규칙처럼 보이는 태그명

`MvpLoopContent.Events`의 각 선택지는 `OutcomeTag`를 가진다.

### 3. 가문 상태와 연대기 보관

- `FamilyRunState.FamilyStats`가 가문 단위 상태의 진실 출처다.
- `RunState.Chronicle`은 현재 세대의 항목식 기록만 가진다.
- `FamilyRunState.History`는 끝난 세대들의 `GenerationRecord`를 누적한다.
- 전 가문 단위 항목식 리스트를 별도로 만들지 않는다. 필요하면 `History`를 순회한다.

### 4. 다세대 모델

`scripts/MultigenModels.cs`에 다음 타입을 둔다.

- `FamilyRunState`
  - `string FamilyName`
  - `int CurrentGeneration`
  - `string CurrentCharacterName`
  - `RunState CurrentRun`
  - `Dictionary<string, int> FamilyStats`
  - `List<GenerationRecord> History`
  - `List<string> CarriedOutcomeTags`
  - `List<TraitStub> CurrentCharacterTraits`
  - `bool IsExtinct`
- `GenerationRecord`
  - `int GenerationNumber`
  - `string CharacterName`
  - `GenerationEndType EndType`
  - `IReadOnlyList<ChronicleEntry> Entries`
  - `string SummaryParagraph`
- `GenerationEndResult`
  - `GenerationEndType EndType`
  - `bool IsCurrentCharacterGone`
  - `IReadOnlyList<SuccessionCandidate> Candidates`
  - `bool IsExtinct`
  - `string SummaryParagraph`
- `SuccessionCandidate`
  - `string Name`
  - `string Description`
  - `IReadOnlyList<TraitStub> Traits`
- `TraitStub`
  - `string Key`
  - `string Label`
- `GenerationEndType`
  - `Extinction`
  - `NaturalDeath`
  - `BattleDeath`
  - `IllnessDeath`
  - `Deposed`
- `StateCarryKind`
  - `Resource`
  - `Reputation`
  - `Relation`

`TraitStub`는 표시 전용이다. `MultigenFlow`의 후계, 상태, 사건, 멸문 로직 입력으로 쓰지 않는다.

### 5. 다세대 흐름 진입점

`scripts/MultigenFlow.cs`는 UI와 콘텐츠 문장을 직접 만들지 않는다. 다음 public 진입점만 제공한다.

- `CreateNewFamily()`
- `FinishCurrentGeneration(FamilyRunState family)`
- `BuildNextGenerationRun(FamilyRunState family, SuccessionCandidate chosen)`
- `AdvanceToNextGeneration(FamilyRunState family, SuccessionCandidate chosen, RunState nextRun)`
- `BuildStateBandLabel(string key, int value)`
- `BuildDetailedStateLines(FamilyRunState family)`

후계 판정 함수는 private으로 두고 이름에 임시성을 드러낸다.

- 예: `PlaceholderEvaluateSuccession(...)`

함수 주석에는 "MVP 검증용 임시 로직이며 후계 가능성 계산식은 결정 보류"라고 명시한다.

### 6. 상태 거동과 라벨

- 상태 거동 매핑은 `MultigenFlow`에 둔다.
- 매핑은 MVP 검증용 임시 매핑임을 주석으로 명시한다.
- `StateDefinition`에는 `StateCarryKind`를 추가하지 않는다.
- 질적 구간 라벨은 4단계로 둔다.
- 라벨 문구와 임계값은 자리표시자다.
- `BuildStateBandLabel` 또는 동등 함수에 임시 임계값임을 주석으로 남긴다.

### 7. 세대 종료 유형 발화

enum은 5종을 정의해도 된다. 다만 이번 자동 흐름에서 실제 발화시키는 종료 유형은 다음 2종으로 제한한다.

- `NaturalDeath`: 1세대 종료, 후계 있음
- `Extinction`: 2세대 종료, 후계 없음

`BattleDeath`, `IllnessDeath`, `Deposed`는 자리만 확보하고 실제 트리거 시스템을 만들지 않는다.

### 8. 연대기와 세대 요약

- 진행 중 기록은 `ChronicleEntry.RecordedLine`으로 유지한다.
- 세대 끝 기록은 `GenerationRecord.SummaryParagraph` 한 단락으로 둔다.
- 이 단락은 다음을 함께 포함한다.
  - 이번 세대의 의미
  - 이전 또는 이번 세대의 유산
  - 다음 세대가 직면할 상황 암시
  - 세대 말 상태의 서술 요약
- 세대 요약 헤더에는 최소 한 번 `가문력 N년` 형식의 시간 표시를 넣는다.
- 최종 연호 명칭, 문체, 세계관 어휘는 확정하지 않는다.

### 9. 자리표시자 규칙

- 가문명: `가문 A`
- 인물명: `인물 1`, `인물 2`
- 특성 키/라벨: `trait-1`, `특성 1` 같은 자리표시자
- 문장: `[자리표시자: 도입 문장]`, `[자리표시자: 후대 기록자 요약]`처럼 자리표시자임이 드러나야 한다.
- 후대 기록자 요약 자리표시자는 영광/오욕 같은 평가 어휘를 확정하지 않는다.
- 기존 사건 문안 전체는 이번 범위에서 교체하지 않는다.

## 구체적 구현 단계

### 작업 단위 1: 임시값 위생

소유 파일:

- `scripts/MvpLoopContent.cs`

수행:

- 가문명, 초대 인물명, 도입 문장을 자리표시자형 표현으로 바꾼다.
- 사건 제목/본문/선택지/결과/연대기 문장은 유지한다.
- 파일 상단 주석의 임시 콘텐츠 경고를 유지하거나 보강한다.

권장 검증:

```powershell
dotnet build
```

### 작업 단위 2: 단일 세대 모델 합류 계약 보강

소유 파일:

- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`

수행:

- `RunState`를 한 세대 사건 진행 상태로 좁힌다.
- `EventChoice.OutcomeTag`를 추가한다.
- `MvpLoopContent.Events`의 선택지에 자리표시자 `OutcomeTag`를 채운다.
- `MvpLoopContent.CreateInitialRunState()`가 새 `RunState` 책임과 맞게 수정되도록 한다.

권장 검증:

```powershell
dotnet build
```

### 작업 단위 3: 다세대 모델/콘텐츠/흐름 추가

소유 파일:

- `scripts/MultigenModels.cs`
- `scripts/MultigenContent.cs`
- `scripts/MultigenFlow.cs`

수행:

- 합류 계약의 다세대 모델 타입을 구현한다.
- `MultigenContent`에 자리표시자 문장, 인물명 시드, 후계 후보 시드, 종료 유형 표시 문장을 둔다.
- `MultigenFlow`에 새 가문 생성, 세대 종료, 후계/멸문 판단, 다음 세대 생성, 상태 라벨/상세 라인 생성 함수를 둔다.
- 후계 판정과 상태 매핑은 검증용임을 코드 주석으로 명시한다.
- 특성 스텁은 표시 전용으로만 둔다.

권장 검증:

```powershell
dotnet build
```

### 작업 단위 4: UI와 스모크 확장

소유 파일:

- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`

수행:

- 기존 코드 기반 동적 UI를 유지한다.
- 시작 화면을 다세대 `FamilyRunState` 기반으로 초기화한다.
- 사건/결과/세대 요약 흐름을 다세대 상태와 연결한다.
- 세대 요약 후 후계 후보가 있으면 후계 선택 또는 다음 세대 확인 화면을 보여준다.
- 후계가 없고 멸문이면 멸문 화면을 보여준다.
- 기본 상태는 4단계 질적 라벨로 보여준다.
- 상세 토글 영역에서 숫자와 변화 방향을 보여준다.
- `--smoke`는 아래 경로 A를 한 실행에서 수행한다.

스모크 경로 A:

1. 1세대 시작
2. 1세대 사건 3개 자동 진행
3. 1세대 `NaturalDeath` 종료
4. 후계 후보 표시
5. 후계 선택 또는 확인
6. 2세대 진입
7. 2세대 사건 3개 자동 진행
8. 2세대 `Extinction` 종료
9. 멸문 표시
10. `SMOKE_OK` 출력 후 종료

스모크 stdout에는 최소 다음 정보가 있어야 한다.

```text
[SMOKE] FAMILY ...
[SMOKE] GEN 1 ...
[SMOKE] EVENT ...
[SMOKE] DELTA ...
[SMOKE] CHRONICLE ...
[SMOKE] GEN_END type=NaturalDeath ...
[SMOKE] SUCCESSION candidates=...
[SMOKE] SUCCESSION_PICK ...
[SMOKE] GEN 2 ...
[SMOKE] CARRIED ...
[SMOKE] GEN_END type=Extinction ...
[SMOKE] EXTINCT ...
SMOKE_OK
```

### 작업 단위 5: 검증과 보고

소유 파일:

- `docs/handoff/multigen-mvp/04-implementation-report.md`

수행:

- 지정된 검증 명령을 실행한다.
- 가능한 경우 수동 GUI 확인을 1회 수행한다.
- 구현 결과 보고서를 작성한다.

## Godot 경로 케이스 계약

현재 실제 디스크 경로를 따른다.

- `scripts/`
- `Scenes/`
- `Scenes/Main.tscn`
- `scripts/Main.cs`
- `scripts/MvpLoopModels.cs`
- `scripts/MvpLoopContent.cs`

새 C# 파일은 기존 소문자 디렉터리 `scripts/` 아래에 둔다.

새 파일:

- `scripts/MultigenModels.cs`
- `scripts/MultigenContent.cs`
- `scripts/MultigenFlow.cs`

씬 참조:

- `Scenes/Main.tscn`은 기존 `res://scripts/Main.cs`만 참조한다.
- 새 C# 파일은 `.tscn`에 직접 어태치하지 않는다.
- `run/main_scene="res://Scenes/Main.tscn"`를 유지한다.

`Scripts/` 같은 케이스 변형 디렉터리를 만들지 않는다.

## 병렬 작업 분해

### 트랙 A: 모델과 흐름

소유 파일:

- `scripts/MvpLoopModels.cs`
- `scripts/MultigenModels.cs`
- `scripts/MultigenFlow.cs`

책임:

- `RunState` 책임 좁히기
- `EventChoice.OutcomeTag` 추가
- 다세대 모델 작성
- 다세대 흐름 작성
- 상태 거동 매핑과 질적 라벨 함수 작성

### 트랙 B: 콘텐츠, 위생, UI

소유 파일:

- `scripts/MvpLoopContent.cs`
- `scripts/MultigenContent.cs`
- `scripts/Main.cs`
- `Scenes/Main.tscn`
- `project.godot`

책임:

- 임시값 자리표시자화
- 다세대 자리표시자 콘텐츠 작성
- 동적 UI 확장
- 후계/멸문 화면 작성
- 상세 상태 토글 작성

### 트랙 D: 스모크와 보고

소유 파일:

- `scripts/Main.cs`
- `docs/handoff/multigen-mvp/04-implementation-report.md`

책임:

- 다세대 `--smoke` 경로 A 구현 또는 최종 조정
- 최종 검증 실행
- 구현 결과 보고서 작성

트랙 D는 트랙 A와 B 통합 후에만 진행한다. 검증 명령은 최종 통합 후 순차 실행한다.

## 검증 명령

Project Claude는 구현 후 다음 명령을 실행하고 결과를 보고한다.

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
.\scripts\godot.cmd --headless -- --smoke
```

가능하면 수동 GUI 확인도 1회 수행한다.

```powershell
.\scripts\godot.cmd
```

수동 GUI 확인을 실행하지 못하면 이유를 보고서에 적는다.

## 완료 기준

- 새 실행을 시작할 수 있다.
- 첫 세대가 기존 단일 세대 사건 루프를 진행한다.
- 선택 결과가 결과 문구, 상태 변화, 진행 중 연대기로 피드백된다.
- 세대 요약에서 후대 기록자 시점 요약 단락을 볼 수 있다.
- 세대 요약 단락은 유산, 다음 세대 상황 암시, 세대 말 상태 서술을 포함한다.
- 세대 요약에 `가문력 N년` 형식의 시간 표시가 최소 1회 보인다.
- 1세대 종료 후 후계 후보를 확인하고 2세대로 진입할 수 있다.
- 2세대에서 이전 세대의 상태, 큰 사건 결과 태그, 연대기 이력이 이어진 것을 확인할 수 있다.
- 2세대 종료 후 후계 부재와 사망/퇴장 조건으로 멸문 화면을 볼 수 있다.
- 기본 상태 표시는 4단계 질적 라벨을 사용한다.
- 상세 토글 영역에서 숫자와 변화 방향을 확인할 수 있다.
- 특성 스텁은 표시 전용으로만 보인다.
- `dotnet build`가 성공한다.
- `.\scripts\godot.cmd --headless --import`가 성공한다.
- `.\scripts\godot.cmd --headless -- --smoke`가 종료 코드 0으로 끝나고 stdout 마지막 줄에 `SMOKE_OK`를 출력한다.
- 스모크 stdout에 1세대 후계 정상 경로와 2세대 멸문 경로가 모두 나타난다.
- 구현 결과 보고 문서가 작성된다.

## 보고 형식

Project Claude는 `docs/handoff/multigen-mvp/04-implementation-report.md`에 다음 형식으로 결과를 작성한다.

- 변경 파일
- 리뷰 반영 요약
- 구현된 사용자 흐름
- 단일 세대 모듈 유지/변경 내용
- 다세대 흐름 구현 내용
- 임시값 위생 작업 내용
- 실행한 확인 명령과 결과
- 스모크 stdout 발췌
- 수동 GUI 확인 결과
- 남은 제약 또는 미구현 범위
- 다음 작업 후보

## 멈춤 지점

다음 상황에서는 추측으로 구현하지 말고 보고한다.

- PRD에 없는 게임 규칙을 확정해야 하는 경우
- 상태 항목 이름, 개수, 수치 범위, 감쇠율을 임시 검증 수준 이상으로 확정해야 하는 경우
- 후계 가능성 계산식 또는 확률을 최종 규칙처럼 정해야 하는 경우
- 특성 상속 확률, 특성 목록, 특성 효과를 정해야 하는 경우
- 사건 데이터 저장 형식을 확정해야 하는 경우
- 완성형 UI 구조나 아트 스타일 결정을 요구하는 경우
- `RunState` 책임 좁히기가 기존 단일 세대 루프를 깨뜨려 작은 수정으로 복구하기 어려운 경우
- `project.godot`의 메인 씬 지정이 다른 기존 흐름과 충돌하는 경우
- 지정된 검증 명령이 도구 부재나 환경 문제로 실행되지 않는 경우
