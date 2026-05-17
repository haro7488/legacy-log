# MVP First Loop Project Claude Review Request

> 작성일: 2026-05-18
> 작성자: Project Codex
> 대상: Project Claude
> 모드: 리뷰 요청. 바로 구현하지 않는다.

## Project Claude 전달 문장

`docs/handoff/mvp-first-loop/01-review-request.md`를 읽고 리뷰 모드로 검토해줘. 바로 구현하지 말고, 구현 계획의 범위 초과 여부, 병렬 작업 분해, 합류 계약, 검증 방법을 문서의 "Project Claude 보고 형식"에 맞춰 `docs/handoff/mvp-first-loop/02-claude-review.md`에 작성해줘. 해당 파일이 없으면 새로 생성해줘.

## Project Claude 답변 문서

- 파일명: `02-claude-review.md`
- 경로: `docs/handoff/mvp-first-loop/02-claude-review.md`
- 작성 주체: Project Claude
- 목적: Project Codex가 리뷰 결과를 이어받아 최종 작업 지시서를 작성하기 위한 입력 문서
- 작성 원칙: 구현하지 말고 리뷰 결과만 작성한다.

## Project Codex 이어받기 기준

Project Codex는 Project Claude가 작성한 `docs/handoff/mvp-first-loop/02-claude-review.md`를 읽고 다음 단계로 진행한다.

- 리뷰 의견이 현재 계획과 충돌하지 않으면 반영 여부를 판단해 최종 작업 지시서를 작성한다.
- 리뷰 의견이 PRD, current-focus, 결정 문서와 충돌하면 추측으로 진행하지 않고 사용자 확인을 받는다.
- 리뷰 문서가 없거나 지정된 보고 형식을 따르지 않으면 구현 지시서를 확정하지 않는다.

## 목적

PRD와 `../docs/current-focus.md`에 정의된 MVP 첫 플레이 루프를 Godot C# 프로젝트에 구현하기 전에, 구현 계획의 누락, 범위 초과, 병렬 작업 분해, 검증 방법을 검토한다.

이번 작업의 제품 목적은 완성형 게임 시스템을 만드는 것이 아니라, "선택이 결과와 상태 변화로 피드백되고, 그 결과가 가문 연대기로 누적된다"는 LegacyLog의 핵심 경험을 검증하는 것이다.

## 참조 문서

- `../../../AGENTS.md`
- `../../../CLAUDE.md`
- `../../../../docs/current-focus.md`
- `../../../../docs/product/prd.md`
- `../../../../docs/decisions/002-agent-workflow.md`
- `../../../../docs/decisions/003-mvp-first-loop.md`
- `../../../../docs/decisions/004-mvp-starting-premise.md`
- `../../../../docs/decisions/005-mvp-event-result-feedback.md`
- `../../../../docs/decisions/006-mvp-first-loop-product-criteria.md`

## 현재 구조 관찰

- 현재 프로젝트는 Godot 4.6.2 + C# (.NET 8) 스켈레톤 상태다.
- `LegacyLog.csproj`는 `Godot.NET.Sdk/4.6.2`와 `net8.0`을 사용한다.
- `project.godot`에는 아직 `run/main_scene`이 지정되어 있지 않다.
- 현재 게임플레이용 `.cs` 스크립트와 `.tscn` 씬은 없다.
- Godot 실행은 `scripts/godot.cmd` 또는 `scripts/godot.sh` 래퍼를 사용해야 한다.

## 제품 범위

구현 계획은 다음 사용자 흐름을 만족해야 한다.

1. 플레이어는 새 실행을 시작한다.
2. 플레이어는 고정된 첫 가문과 초대 인물의 시작 정보를 본다.
3. 플레이어는 3개의 사건을 순서대로 진행한다.
4. 각 사건은 제목 또는 짧은 본문과 2개 이상의 선택지를 제공한다.
5. 플레이어는 선택 후 결과 문구를 확인한다.
6. 플레이어는 선택 후 최소 상태 변화 표시를 확인한다.
7. 플레이어는 선택 결과가 연대기 항목으로 누적되는 것을 확인한다.
8. 3개의 사건이 끝나면 세대 요약을 확인한다.
9. 세대 요약은 누적 연대기, 최종 상태 요약, 짧은 마무리 문장을 포함한다.

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

## 제안 아키텍처

### `Scripts/MvpLoopModels.cs`

MVP 첫 루프의 모델과 검증용 고정 데이터를 담당한다.

포함 후보:

- `RunState`: 가문명, 초대 인물명, 현재 사건 인덱스, 상태값, 연대기 목록
- `LoopEvent`: 사건 제목, 본문, 선택지 목록
- `EventChoice`: 선택지 문구, 결과 문구, 연대기 문장, 상태 변화 목록
- `StateDelta`: 상태 키, 표시명, 변화량
- `ChronicleEntry`: 사건 제목, 선택 문구, 기록 문장

상태 항목은 MVP 검증용 임시 값으로 제한한다. 예: `Reputation`, `Stores`, `Cohesion`.

### `Scenes/Main.tscn`

단일 `Control` 기반 MVP 화면을 담당한다.

필요 UI 후보:

- 시작 정보 영역
- 사건 제목/본문 영역
- 선택지 버튼 영역
- 선택 결과 영역
- 상태 표시 영역
- 연대기 표시 영역
- 세대 요약 영역

### `Scripts/Main.cs`

MVP 실행 흐름을 담당한다.

책임:

- 새 실행 초기화
- 시작 화면 표시
- 현재 사건 표시
- 선택지 버튼 생성 또는 바인딩
- 선택 결과 적용
- 상태 변화 표시
- 연대기 항목 누적
- 다음 사건 진행
- 3개 사건 후 세대 요약 표시

### `project.godot`

`Scenes/Main.tscn`을 `run/main_scene`으로 지정한다.

## 병렬 작업 분해

### 작업 A: 모델과 MVP 검증용 콘텐츠

소유 파일:

- `Scripts/MvpLoopModels.cs`

책임:

- 실행 상태, 사건, 선택지, 상태 변화, 연대기 모델 작성
- 3개 사건과 선택지별 결과/연대기/상태 변화 작성
- 모든 문안은 최종 콘텐츠가 아니라 MVP 검증용 문안으로 취급

선행 의존성:

- 없음

충돌 위험:

- `Main.cs`가 참조할 public 타입명과 프로퍼티명이 합류 계약이 된다.

### 작업 B: 씬과 UI 뼈대

소유 파일:

- `Scenes/Main.tscn`
- `project.godot`

책임:

- 메인 `Control` 씬 생성
- 메인 씬 지정
- MVP 흐름에 필요한 최소 UI 노드 구성

선행 의존성:

- 없음

충돌 위험:

- `Main.cs`가 고정 노드 경로를 참조하면 노드 이름이 합류 계약이 된다.

### 작업 C: 진행 로직

소유 파일:

- `Scripts/Main.cs`

책임:

- `MvpLoopModels.cs`의 모델과 데이터를 사용해 MVP 루프 구현
- 시작, 사건, 결과 확인, 다음 사건, 세대 요약 상태 전환 구현
- 선택 후 결과 문구, 상태 변화, 연대기 추가를 한 번에 확인 가능하게 구성

선행 의존성:

- 작업 A의 public 타입명과 데이터 접근 방식
- 작업 B의 UI 노드 계약. 단, UI를 코드에서 동적 생성하면 작업 B와의 충돌을 줄일 수 있다.

충돌 위험:

- `Scenes/Main.tscn`의 노드 경로와 `Main.cs`의 바인딩 방식이 어긋날 수 있다.

## 합류 계약 후보

Project Claude는 리뷰에서 다음 중 어느 방식이 더 적절한지 제안한다.

### 선택지 1: 씬에 노드 고정

- 장점: Godot 씬 구조가 눈에 보인다.
- 단점: 병렬 작업 중 노드명과 경로 충돌 가능성이 있다.

### 선택지 2: `Main.cs`에서 UI 동적 생성

- 장점: `Main.cs`와 `MvpLoopModels.cs`만으로 흐름 구현이 단순해지고 병렬 충돌이 줄어든다.
- 단점: Godot 씬 파일은 빈 루트에 가까워지고 UI 구조가 코드에 집중된다.

Project Codex의 현재 선호는 선택지 2다. 현재 프로젝트가 스켈레톤이고 완성형 UI가 제외 범위이므로, MVP 검증에는 코드 기반 UI 생성이 더 단순하다.

## 검증 방법

최종 구현 후 Project Claude는 다음 명령을 실행하고 결과를 보고한다.

```powershell
dotnet build
.\scripts\godot.cmd --headless --import
```

필요하면 구현 후 실제 실행 확인을 위해 다음 명령도 사용할 수 있다.

```powershell
.\scripts\godot.cmd --headless --quit-after 2
```

단, Godot은 반드시 `scripts/godot.cmd` 또는 `scripts/godot.sh` 래퍼로 실행한다.

## 리뷰 요청 사항

Project Claude는 다음 항목을 우선 검토한다.

1. 위 아키텍처가 현재 Godot C# 스켈레톤에 과한지 여부
2. 병렬 작업 분해가 실제 파일 소유권 기준으로 충돌 없이 가능한지 여부
3. `Main.cs`에서 UI를 동적 생성하는 방식이 이번 MVP 범위에 더 적절한지 여부
4. 상태 항목 3개를 MVP 검증용 임시 모델로 두는 것이 PRD 범위 안인지 여부
5. 고정 데이터를 코드에 두는 방식이 저장 형식 확정으로 오해될 위험이 있는지 여부
6. 검증 명령과 완료 기준에 누락이 있는지 여부
7. 구현 전 Project Codex가 더 좁혀야 할 합류 계약이 있는지 여부

## 멈춤 지점

다음 판단이 필요하면 구현으로 넘어가지 말고 리뷰 의견으로 돌려준다.

- PRD에 없는 게임 규칙을 확정해야 하는 경우
- 상태 항목 이름, 개수, 수치 범위가 MVP 검증용 임시 수준을 넘어서는 경우
- 사건 데이터 저장 형식을 확정해야 하는 경우
- 완성형 UI 구조나 아트 스타일 결정을 요구하는 경우
- 병렬 작업 분해가 오히려 충돌을 키운다고 판단되는 경우

## Project Claude 보고 형식

리뷰 결과는 `docs/handoff/mvp-first-loop/02-claude-review.md`에 다음 형식으로 작성한다.

- 주요 검토 의견
- 범위 초과 또는 PRD 위반 가능성
- 병렬 작업 분해 개선 제안
- 구현 전 반드시 정해야 할 합류 계약
- 검증 방법 보완 제안
