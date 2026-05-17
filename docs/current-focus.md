# Current Focus

> 최종 수정: 2026-05-18

## 현재 초점

Root Codex의 역할을 **프로젝트 총괄 및 PRD 작성 담당**으로 정리한다. 루트 문서는 제품 방향성, PRD, 현재 작업 초점, 하위 구현 위임을 관리하고, 구체적인 아키텍처와 구현 설계는 Project Codex와 Project Claude의 하위 흐름에 위임한다.

현재 제품 방향은 **고정된 첫 가문과 초대 인물로 시작하는 사건 선택 중심 루프 + 연대기 출력**이다. 플레이어는 사건을 읽고 선택지를 고르며, 선택 결과는 결과 문구, 최소 상태 변화, 연대기 항목으로 피드백된다.

## 참조 문서

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/003-mvp-first-loop.md`
- `docs/decisions/004-mvp-starting-premise.md`
- `docs/decisions/005-mvp-event-result-feedback.md`

## 이번 범위

- Root Codex의 역할을 프로젝트 총괄 및 PRD 작성 담당으로 명시한다.
- 루트 활성 문서에서 이전 워크플로우 측정 중심 표현을 제거한다.
- PRD에서 제품 자체와 프로젝트 운영을 분리해 설명한다.
- 현재 MVP 방향은 유지하되, 구현 설계는 하위 흐름에 위임한다는 경계를 유지한다.

## 하지 말아야 할 것

- `legacy-log-project/` 내부 구현 파일을 수정하지 않는다.
- 첫 MVP 사건 문안, 상태 값 이름, 밸런스 수치를 확정하지 않는다.
- 첫 가문과 초대 인물의 이름은 아직 확정하지 않는다.
- 사건 데이터 저장 형식, Godot 씬 구성, C# 클래스 구조를 Root Codex가 확정하지 않는다.
- 루트 `CLAUDE.md`의 Root Claude 전용 섹션을 수정하지 않는다.

## 완료 기준

- `AGENTS.md`가 Root Codex를 프로젝트 총괄 및 PRD 작성 담당으로 설명한다.
- `docs/product/prd.md`가 게임 제품 목표 중심으로 정리된다.
- `docs/decisions/002-agent-workflow.md`가 프로젝트 운영 흐름으로 설명된다.
- 활성 루트 문서에서 이전 워크플로우 측정 중심 설명이 제거된다.
- 변경 파일이 루트 문서와 `docs/` 문서 영역에만 있다.
