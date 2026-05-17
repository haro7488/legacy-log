# Current Focus

> 최종 수정: 2026-05-18

## 현재 초점

Root Codex, Project Codex, Project Claude로 이어지는 기본 작업 흐름을 프로젝트 문서에 정착시킨다. 루트는 PRD와 전체 방향성을 관리하고, 하위 프로젝트는 계획·검토·구현을 분리해 진행한다.

## 참조 PRD

- `docs/product/prd.md`
- `docs/decisions/002-agent-workflow.md`

## 이번 범위

- 루트 문서에 기본 작업 흐름과 역할 분담을 반영한다.
- 하위 프로젝트에 Project Codex용 계획·지시 문서를 추가한다.
- 하위 Project Claude 문서에 리뷰와 최종 지시서 기반 구현 절차를 반영한다.
- PRD의 구현 연결 방식이 새 흐름을 참조하도록 정리한다.

## 하지 말아야 할 것

- `legacy-log-project/` 내부 구현 파일을 수정하지 않는다.
- 아직 확정되지 않은 게임 규칙, 데이터 구조, 밸런스 수치를 창작하지 않는다.
- Project Codex와 Project Claude의 역할을 섞어서 한 에이전트가 계획·리뷰·구현을 모두 한다고 쓰지 않는다.

## 완료 기준

- `docs/decisions/002-agent-workflow.md`가 존재한다.
- 루트 `AGENTS.md`와 `CLAUDE.md`가 기본 작업 흐름을 참조한다.
- `docs/product/prd.md`가 Root Codex에서 하위 구현으로 이어지는 절차를 설명한다.
- `legacy-log-project/AGENTS.md`가 Project Codex의 계획·지시 역할을 설명한다.
- `legacy-log-project/CLAUDE.md`가 Project Claude의 리뷰·구현 역할을 설명한다.
- 변경 파일이 문서 영역에만 있다.
