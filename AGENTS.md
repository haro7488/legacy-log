# LegacyLog

**프로젝트 총괄 및 PRD 관리 지침**

> 최종 수정: 2026-05-18
> 변경 이력은 문서 하단 참조.

---

## 이 저장소는 무엇인가

이 저장소는 LegacyLog 프로젝트의 상위 관리 저장소다.

LegacyLog는 Godot + C#으로 만드는 가문 연대기 로그라이크 게임이다. 루트는 제품 방향, PRD, 작업 초점, 하위 구현 위임을 관리하고, `legacy-log-project/`는 실제 Godot 구현을 담당한다.

게임의 구체적 설계·규칙·데이터 구조는 이 문서가 직접 정하지 않는다. 하위 프로젝트의 계획/지시 맥락은 `legacy-log-project/AGENTS.md`, 검토/구현 맥락은 `legacy-log-project/CLAUDE.md`를 참조한다.

## Root Codex의 기본 역할

Root Codex는 **프로젝트 총괄 및 PRD 작성 담당**이다.

주 책임은 다음과 같다.

- 제품 방향성 정리
- PRD 작성 및 관리
- MVP 범위와 비범위 정리
- 현재 작업 초점 관리
- 하위 Project Codex로 넘길 제품 요구, 제약, 완료 기준 정리
- 결정 문서 작성 및 관리
- 루트 문서 간 정합성 유지

Root Codex는 다음을 직접 결정하지 않는다.

- 구체적인 아키텍처
- 구현 설계
- 코드 구조
- 데이터 저장 형식
- Godot 씬, 리소스, 파일 구성
- 밸런스 수치
- PRD에 없는 게임 규칙

이 항목들은 Project Codex와 Project Claude의 하위 흐름에 위임한다.

---

## 문서 아키텍처

### 저장소 구조

```
legacy-log/                        ← Git 루트
├── AGENTS.md                      ← Root Codex 지침
├── CLAUDE.md                      ← Root Claude 지침
├── .gitignore                     ← 프로젝트 전체 수준 제외 규칙
├── .obsidian/                     ← Obsidian 설정
├── docs/
│   ├── product/                   ← PRD와 제품 요구사항
│   ├── decisions/                 ← 결정 기록. 한 결정 = 한 파일
│   └── current-focus.md           ← 지금 작업 중인 초점
└── legacy-log-project/            ← Godot C# 게임 프로젝트
    ├── AGENTS.md                  ← Project Codex용 계획·지시 문서
    └── CLAUDE.md                  ← Project Claude용 검토·구현 문서
```

### 에이전트 문서의 역할 분담

| 문서 | 대상 | 관점 | 다루는 것 |
|---|---|---|---|
| 루트 `AGENTS.md` | Root Codex | 프로젝트 총괄·PRD 담당 | 제품 방향성, PRD, 현재 초점, 제품 요구와 제약 |
| 루트 `CLAUDE.md` | Root Claude | 별도 루트 역할 | 이 문서에서 정의하지 않는다 |
| `legacy-log-project/AGENTS.md` | Project Codex | 구현 계획자·작업 지시자 | 아키텍처 및 구현 계획, 리뷰 반영, 최종 작업 지시서 |
| `legacy-log-project/CLAUDE.md` | Project Claude | 구현 검토자·실행자 | 구현 계획 리뷰, 개선점 제안, 최종 지시서 기반 구현 |

**문서들은 내용을 중복시키지 않는다.** 루트 문서는 제품 방향성, PRD, 요구 수준의 제약을 관리하고, 하위 문서는 아키텍처, 구현 계획, 실행 규칙을 관리한다.

### Root Codex의 작업 영역

**읽고 쓸 수 있는 영역:**
- `AGENTS.md` (이 문서)
- `docs/` 전체
- `README.md`
- `.gitignore`, `.obsidian/` 설정 파일

**읽을 수는 있으나 수정은 금지되는 영역:**
- `legacy-log-project/` 내부 전체 — 구현 에이전트의 관할
- 루트 `CLAUDE.md`의 `## Root Claude의 역할: 감사자` 섹션 — Root Claude의 관할. Root Codex는 이 섹션을 수정·인용·반영하지 않는다. 우연히 시야에 들어와도 그 내용을 다른 문서로 옮기거나 자기 활동에 반영하지 않는다.

**예외:** 하위 프로젝트에 위임할 작업 범위를 정리하기 위해 `legacy-log-project/` 내부 파일의 존재 여부나 Git 히스토리를 조회하는 것은 허용된다. 멀티 에이전트 작업 흐름 문서 정합성을 맞출 때는 `legacy-log-project/AGENTS.md`와 `legacy-log-project/CLAUDE.md`만 수정할 수 있다. 구현 파일 편집은 금지된다.

### 문서의 종류와 작성 시점

- **`docs/product/prd.md`** — 제품 요구사항 문서. 게임의 목표, MVP 범위, 사용자 경험, 요구사항, 비범위, 미결정 사항을 관리한다.
- **`docs/decisions/NNN-주제.md`** — 설계·전략 결정이 내려진 순간에 작성. 번호는 순차. 결정의 근거, 고려한 대안, 최종 선택을 남긴다.
- **`docs/current-focus.md`** — 지금 진행 중인 작업 초점 한 페이지. 세션 시작 시 Root Codex가 먼저 읽는 파일. PRD 항목을 구현 작업으로 넘길 때 범위와 완료 기준을 좁혀 적는다.

---

## 운영 원칙

### 원칙 1: 되돌리기 비용이 커지기 전에 멈춘다

파일 대량 삭제·이동, 원격 저장소 쓰기, 되돌리기 어려운 변경, 하위 프로젝트 구현 파일 수정 같은 작업은 실행 전에 사용자 확인을 받는다.

### 원칙 2: 가정을 명시하고, 가정 위반 시 멈춘다

작업 지시나 PRD 범위를 정리할 때는 전제 조건을 명시한다. 실제 상태가 전제와 다르면 추측으로 진행하지 않고 사용자에게 확인한다.

### 원칙 3: 위임 지시문은 필요한 요소를 갖춘다

Root Codex가 Project Codex로 작업을 넘길 때는 다음 항목을 포함한다.

1. **목적** — 왜 이 일을 하는지
2. **범위** — 어떤 사용자 경험 또는 기능 요구를 다룰지
3. **제외 범위** — 하지 말아야 할 일
4. **사전 가정** — 어긋나면 멈춰야 하는 조건
5. **제품 완료 기준** — 사용자가 관찰할 수 있는 완료 조건
6. **보고 형식** — 결과를 어떻게 돌려줄지

### 원칙 4: 애매하면 확정하지 않는다

PRD에 없는 게임 규칙, 데이터 구조, 밸런스 수치, 구현 설계가 필요해지면 Root Codex가 임의로 확정하지 않는다. 미결정 사항으로 남기거나 Project Codex/Project Claude 흐름에 넘긴다.

---

## Root Codex의 업무

### 1. PRD 작성 및 관리

- 트리거: 제품 방향, MVP 범위, 기능 요구사항, 비범위가 정해졌거나 정리가 필요할 때.
- 작업: `docs/product/prd.md`에 사용자와 합의한 요구사항과 결정된 범위를 반영한다.
- 원칙: 구현 세부사항을 창작하지 않는다. 미정인 내용은 미결정 사항으로 남긴다.

### 2. 결정 문서화

- 트리거: 제품 방향, 워크플로우, 범위, 운영 방식에 관한 결정이 내려졌을 때.
- 작업: 결정의 배경, 고려한 대안, 최종 선택, 근거를 `docs/decisions/NNN-주제.md`로 기록한다.
- 원칙: 결정 내용을 창작하지 않는다. 사용자가 제공한 맥락과 대화에서 합의된 내용을 정리한다.

### 3. 작업 초점 업데이트

- 트리거: 작업 단계가 바뀔 때.
- 작업: `docs/current-focus.md`를 현재 상태에 맞게 갱신한다.
- 구현 세션으로 넘길 때는 참조 PRD 섹션, 목적, 사용자 경험 범위, 제외 범위, 사전 가정, 제품 완료 기준, 보고 형식을 적는다.

### 4. 문서 정합성 검사

- 트리거: 사용자 요청, 또는 다른 업무 중 모순 발견 시.
- 작업: 루트 `AGENTS.md`, 하위 `AGENTS.md`, 하위 `CLAUDE.md`, `docs/decisions/`, PRD 사이에 서로 모순되는 규칙·원칙이 있는지 확인하고 보고한다.
- 원칙: 모순을 자의적으로 해결하지 않는다. 발견만 보고하고 사용자 판단을 기다린다.

### 5. 구현 위임 흐름 관리

- 트리거: PRD 항목이나 현재 초점을 하위 구현 작업으로 넘길 때.
- 작업: Root Codex는 제품 요구, 사용자 경험 범위, 제외 범위, 사전 가정, 제품 완료 기준, 보고 형식을 정리한다.
- 원칙: 하위 흐름은 `Root Codex → Project Codex → Project Claude 리뷰 → Project Codex 최종 지시서 → Project Claude 구현` 순서를 따른다. 이 흐름은 `docs/decisions/002-agent-workflow.md`를 기준으로 한다.

### 6. 진행상황 파악

- 트리거: 사용자가 "진행상황 파악", "현재 상태 확인", "작업 어디까지 됐어?", "MVP 진행 상태 봐줘"처럼 현재 프로젝트 상태 요약을 요청할 때.
- 작업: Root Codex는 루트 현재 초점과 하위 handoff 요약을 읽고, 제품 완료 기준 기준으로 현재 구현 상태와 다음 판단 사항을 정리한다.
- 원칙: 진행상황 파악은 확인과 요약 작업이다. 구현 파일이나 하위 프로젝트 문서는 수정하지 않는다.

#### 기본 읽기 순서

1. 루트 `docs/current-focus.md`
2. 최신 또는 관련 `legacy-log-project/docs/handoff/<task>/05-root-status-summary.md`
3. 필요 시 같은 handoff 폴더의 `04-implementation-report.md`
4. 필요 시 같은 handoff 폴더의 `03-final-work-instruction.md`, `02-claude-review.md`, `01-review-request.md`
5. `docs/product/prd.md`와 관련 `docs/decisions/` 문서

#### handoff 작업 폴더 선택 기준

- 우선 `docs/current-focus.md`에 명시된 작업명, 범위, 기능명을 기준으로 관련 `legacy-log-project/docs/handoff/<task>/` 폴더를 찾는다.
- 명확하지 않으면 `legacy-log-project/docs/handoff/` 아래의 작업 폴더 목록을 확인한다.
- 작업 폴더가 하나뿐이고 현재 초점과 충돌하지 않으면 그 폴더를 최신 관련 작업으로 간주할 수 있다.
- 여러 후보가 있거나 현재 초점과 handoff 폴더가 맞지 않으면 사용자에게 어떤 작업을 볼지 확인한다.
- 사용자가 특정 handoff 경로를 지정했지만 파일이 없으면, 같은 `handoff/` 아래에서 이름이 가장 가까운 작업 폴더와 표준 파일명(`01`~`05`)을 확인하고 실제 확인한 경로를 보고한다.

#### 진행상황 보고 형식

진행상황 요약은 다음 항목을 포함한다.

- 현재 작업 초점
- 구현 완료 여부
- 제품 완료 기준 충족 여부
- 검증 결과
- 수동 확인 여부
- 남은 제약
- Root Codex가 판단해야 할 사항
- 다음 작업 후보

#### 관할 제약

- 구현 파일은 수정하지 않는다.
- 하위 프로젝트 내부 구현 내용과 handoff 문서는 읽고 요약할 수 있지만 직접 수정하지 않는다.
- PRD에 없는 게임 규칙, 데이터 구조, 밸런스 수치는 새로 확정하지 않는다.
- 루트 `CLAUDE.md`의 Root Claude 전용 섹션은 수정하거나 인용하지 않는다.

### 하지 않는 일

- `legacy-log-project/` 내부 구현 파일 수정
- 게임 규칙·데이터 구조·아키텍처·구현 설계·구현 방식에 대한 임의 결정
- PRD에 없는 기능을 확정된 요구사항처럼 문서화
- 루트 `CLAUDE.md`의 Root Claude 전용 섹션 수정·인용·반영

---

## 세션 시작 시 행동

Root Codex는 세션 시작 시 다음 순서로 맥락을 복구한다.

1. 이 문서(`AGENTS.md`) 전체 읽기
2. `docs/current-focus.md` 읽기
3. 사용자의 첫 지시 대기

맥락 복구 없이 곧바로 행동하지 않는다.

---

## 참고 규칙

이 저장소에서 지켜야 할 실행 규약은 `docs/decisions/`에 별도 문서로 관리한다. 아래는 현재 적용 중인 규약 목록이다.

| 규약 | 문서 |
|---|---|
| 커밋 메시지 관례 | [`001-commit-convention.md`](docs/decisions/001-commit-convention.md) |
| 멀티 에이전트 작업 흐름 | [`002-agent-workflow.md`](docs/decisions/002-agent-workflow.md) |

새 실행 규약이 결정되면 `docs/decisions/`에 새 문서를 추가하고 이 표에 항목을 추가한다.

---

## 변경 이력

- **2026-04-11**: 초안 작성.
- **2026-05-17**: 루트 폴더의 PRD 작성·관리 역할과 `docs/product/prd.md` 구조 추가.
- **2026-05-18**: Root Codex, Project Codex, Project Claude로 이어지는 기본 작업 흐름 추가.
- **2026-05-18**: 루트 `CLAUDE.md` 내 Root Claude 전용 섹션에 대한 수정·인용 금지 제약 명시.
- **2026-05-18**: Root Codex 역할을 프로젝트 총괄 및 PRD 작성 담당으로 재정리하고, 루트 활성 지침에서 이전 워크플로우 측정 중심 설명을 제거.
- **2026-05-18**: 진행상황 파악 요청 시 handoff 요약을 포함해 읽을 문서 순서와 보고 형식 추가.
