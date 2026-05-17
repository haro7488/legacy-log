# Current Focus

> 최종 수정: 2026-05-18

## 현재 초점

MVP 첫 플레이 루프와 시작 전제를 PRD에 반영하고, 다음 단계에서 Project Codex로 넘길 첫 구현 작업의 범위를 좁힌다.

현재 제품 방향은 **고정된 첫 가문과 초대 인물로 시작하는 사건 선택 중심 루프 + 연대기 출력**이다. 플레이어는 사건을 읽고 선택지를 고르며, 선택 결과는 결과 문구, 최소 상태 변화, 연대기 항목으로 피드백된다.

Root Codex의 책임은 PRD와 제품 요구 수준의 제약까지다. 아키텍처 설계, 구현 설계, 데이터 저장 형식, 코드 구조는 Project Codex와 Project Claude의 하위 흐름에 위임한다.

## 참조 문서

- `docs/product/prd.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/003-mvp-first-loop.md`
- `docs/decisions/004-mvp-starting-premise.md`
- `docs/decisions/005-mvp-event-result-feedback.md`

## 이번 범위

- MVP 첫 플레이 루프를 PRD에 명시한다.
- MVP 시작 방식을 고정된 첫 가문과 초대 인물로 명시한다.
- MVP 사건 결과가 서사 결과, 연대기 항목, 최소 상태 변화를 함께 제공한다고 명시한다.
- MVP 포함/제외 범위를 분리한다.
- Root Codex가 아키텍처와 구현 설계를 확정하지 않는다는 경계를 명시한다.
- 첫 구현으로 넘기기 전 미결정 사항을 남긴다.
- 다음 구현 위임이 가능한 완료 기준을 정리한다.

## 하지 말아야 할 것

- `legacy-log-project/` 내부 구현 파일을 수정하지 않는다.
- 첫 MVP 사건 문안, 상태 값 이름, 밸런스 수치를 확정하지 않는다.
- 첫 가문과 초대 인물의 이름은 아직 확정하지 않는다.
- 사건 데이터 저장 형식, Godot 씬 구성, C# 클래스 구조를 Root Codex가 확정하지 않는다.
- 구체적인 상태 값 이름, 개수, 데이터 구조는 Root Codex가 확정하지 않는다.
- 다세대 후계, 혼인, 혈통, 장기 가문 경영을 MVP 필수 범위로 넣지 않는다.

## 완료 기준

- `docs/decisions/003-mvp-first-loop.md`가 존재한다.
- `docs/decisions/004-mvp-starting-premise.md`가 존재한다.
- `docs/decisions/005-mvp-event-result-feedback.md`가 존재한다.
- `docs/product/prd.md`가 사건 선택 중심 MVP 루프를 설명한다.
- `docs/product/prd.md`가 고정된 첫 가문과 초대 인물 시작 방식을 설명한다.
- `docs/product/prd.md`가 사건 결과 피드백을 결과 문구, 최소 상태 변화, 연대기 항목으로 설명한다.
- `docs/product/prd.md`와 `docs/decisions/002-agent-workflow.md`가 Root Codex의 역할 경계를 설명한다.
- PRD가 MVP 포함 범위와 제외 범위를 구분한다.
- PRD에 다음 구현 전에 결정할 미결정 사항이 남아 있다.
- 변경 파일이 문서 영역에만 있다.
