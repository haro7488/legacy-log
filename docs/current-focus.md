# Current Focus

> 최종 수정: 2026-05-18

## 현재 초점

현재 초점은 **다세대 MVP 구현 위임 준비**이다.

LegacyLog의 정체, 다세대 MVP 범위, 연대기와 세대 요약 문체, 상태 표시 원칙은 루트 PRD와 결정문서에 정리됐다. Root Codex의 다음 작업은 이 제품 결정을 하위 구현 흐름으로 넘길 수 있는 작업 범위와 완료 기준으로 좁히는 것이다.

## 참조 문서

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/003-mvp-first-loop.md`
- `docs/decisions/004-mvp-starting-premise.md`
- `docs/decisions/005-mvp-event-result-feedback.md`
- `docs/decisions/006-mvp-first-loop-product-criteria.md`
- `docs/decisions/007-multigen-game-identity.md`
- `docs/decisions/008-multigen-mvp-scope.md`
- `docs/decisions/009-chronicle-style.md`
- `docs/decisions/010-state-display-layering.md`
- `docs/temp-value-hygiene-delegation-draft.md`
- `legacy-log-project/docs/handoff/mvp-first-loop/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/mvp-first-loop/04-implementation-report.md`

## 현재 상태 판단

- 단일 세대 첫 루프 모듈은 동작 검증 완료로 본다.
- 현재 커밋 `72f0ffc`의 구현은 새 실행, 사건 진행, 선택 결과, 최소 상태 변화, 연대기 누적, 세대 요약을 확인한 상태로 해석한다.
- 다세대 MVP 자체는 아직 구현 완료가 아니다.
- 다세대 MVP의 제품 범위는 최소 2세대 이상, 상한 고정 없이 멸문까지 진행하는 구조로 정리됐다.
- 세대 간 이어짐 매개는 가문 상태, 큰 사건 결과, 연대기다.
- 후계 메커니즘은 혼합형이며, 멸문 조건은 후계 부재 상태에서 현 세대 인물이 사망하거나 퇴장해 다음 세대로 이어질 수 없는 경우다.
- 연대기 문체는 진행 중 1인칭, 세대 끝 후대 기록자 시점으로 정리됐다.
- 상태 표시는 질적 구간 라벨과 세대 끝 서술 요약을 기본으로 하고, 추가 정보창에서 숫자와 변화 방향을 노출하는 원칙으로 정리됐다.
- 현재 콘텐츠와 상태 이름, 상태 수치, 가문/인물 이름, 사건 문안은 검증용 임시 값으로 본다.

## 다음 작업 목적

다세대 MVP 결정을 하위 구현 흐름으로 넘길 수 있는 제품 요구 수준의 위임 지시로 정리한다.

Root Codex는 다음 작업에서 목적, 범위, 제외 범위, 사전 가정, 제품 완료 기준, 보고 형식을 작성한다. 구체적인 아키텍처, 데이터 구조, Godot 씬 구성, C# 구현 방식은 Project Codex와 Project Claude의 하위 구현 흐름에 위임한다.

## 다음 작업에서 정리할 항목

- 다세대 MVP 구현 위임의 목적과 사용자 경험 범위
- 단일 세대 모듈을 다세대 진행으로 연결할 때의 제품 완료 기준
- 후계/멸문 판단, 다음 세대 진입, 가문 종료의 관찰 가능한 완료 조건
- 연대기와 세대 요약 문체 원칙의 구현 확인 기준
- 상태 표시 계층화 원칙의 구현 확인 기준
- 임시값 위생 작업을 다세대 MVP 구현 위임에 포함할지, 별도 위임으로 유지할지에 대한 정리

## 제외 범위

- 구현 파일 수정
- 하위 프로젝트 내부 문서 수정
- PRD에 없는 게임 규칙 확정
- 상태 값 이름, 개수, 수치, 밸런스 확정
- 평판형 감쇠율, 후계 가능성 계산식, 특성 상속 확률 확정
- 사건 데이터 저장 형식 확정
- Godot 씬, 리소스, C# 클래스 구조 결정
- 완성형 UI/아트 스타일 확정
- 저장/불러오기, 혼인, 혈통 시스템 구현 지시
- 시대/세계관 톤과 가문/인물 이름 후보 정리

## Root Codex 완료 기준

- PRD와 결정문서 003-010의 현재 관계를 정합적으로 유지한다.
- 다세대 MVP의 현재 상태를 "제품 범위 결정 완료, 구현 미완"으로 정리한다.
- 다음 하위 구현 흐름으로 넘길 제품 요구와 완료 기준을 좁힌다.
- 결정 보류 항목을 구현 지시에서 확정하지 않고 별도 위임 또는 미결정으로 남긴다.

## 보고 형식

Root Codex는 다음 형식으로 결과를 돌려준다.

- 현재 상태 요약
- 참조한 PRD 및 결정문서
- 하위 구현 흐름으로 넘길 제품 요구
- 제외 범위와 결정 보류 항목
- 다음 작업 초점
