# Current Focus

> 최종 수정: 2026-05-19

## 현재 초점

현재 초점은 **다세대 MVP UI/UX 수동 검증 및 편의성 개선 기준 정리**이다.

다세대 MVP 기능 구현은 하위 handoff 기준으로 완료 보고됐다. 그러나 현재 UI는 기능 경로를 보여주는 성격이 강할 가능성이 있으므로, 수동 검증 전에 사용자가 불편 없이 플레이할 수 있는 UI/UX 개선 방향을 먼저 정리한다.

## 참조 문서

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/007-multigen-game-identity.md`
- `docs/decisions/008-multigen-mvp-scope.md`
- `docs/decisions/009-chronicle-style.md`
- `docs/decisions/010-state-display-layering.md`
- `docs/decisions/011-ui-ux-validation-workflow.md`
- `docs/decisions/012-responsive-tabbed-ui-direction.md`
- `docs/manual-ui-ux-validation-guide.md`
- `docs/project-codex-ui-ux-improvement-prompt.md`
- `legacy-log-project/docs/handoff/multigen-mvp/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/multigen-mvp/04-implementation-report.md`
- `legacy-log-project/docs/handoff/multigen-mvp/03-final-work-instruction.md`

## 현재 상태 판단

- 단일 세대 첫 루프 모듈은 동작 검증 완료로 본다.
- 다세대 MVP 기능 구현은 하위 handoff 기준으로 완료 보고됐다.
- 자동 검증은 `dotnet build`, Godot headless import, smoke 기준으로 성공 보고됐다.
- 다세대 흐름은 1세대 진행, 후계 진입, 2세대 진행, 멸문 종료까지 확인된 상태로 보고됐다.
- UI/UX 수동 확인은 기능 완료와 별도 축으로 다시 점검해야 한다.
- 수동 검증 전에 작은 창과 세로형 모바일 기기에서도 사용할 수 있는 정보 구조와 조작 흐름을 먼저 개선할 필요가 있다.
- 현재 구현은 자리표시자 콘텐츠와 임시 상태 라벨을 포함하므로, 최종 문체나 아트 품질보다 플레이 흐름의 이해 가능성과 조작 편의성을 우선 검증한다.

## 다음 작업 목적

기능 검증만으로 완료 판단하지 않도록, 다세대 MVP의 실제 플레이 흐름에 대한 UI/UX 개선 방향과 수동 검증 기준을 정리한다.

Root Codex는 다음 작업에서 Project Codex로 넘길 UI/UX 개선 프롬프트와, 사용자가 직접 플레이하며 확인할 체크리스트를 함께 관리한다.

사용자에게 검증을 요청할 때는 `docs/manual-ui-ux-validation-guide.md`를 기준으로, 구현된 항목, 테스트할 사용자 흐름, 피드백 기록 형식을 함께 안내한다.

Project Codex로 개선 계획을 요청할 때는 `docs/project-codex-ui-ux-improvement-prompt.md`를 사용한다.

## UI/UX 개선 방향

- 작은 창과 세로형 모바일 기기 모두에서 사용할 수 있는 세로 흐름을 우선한다.
- 화면은 현재 맥락, 주요 내용, 주요 행동, 보조 정보 역할로 명확히 나눈다.
- 항상 보이는 정보와 탭에서 확인할 정보를 구분한다.
- 현재 가문, 세대, 인물, 현재 화면의 목적, 다음 주요 행동은 항상 확인 가능해야 한다.
- 상태 상세, 연대기, 이전 세대 기록, 이어진 큰 사건 결과, 특성 스텁은 탭 또는 보조 영역에서 확인하게 한다.
- 기본 UI는 모바일 탭 형식으로 오가며 확인할 수 있는 구조를 기준으로 한다.
- 목표는 시작부터 멸문까지 불편 없이 진행하는 것이다.
- 체크리스트 기반 수동 검증을 함께 준비한다.

## 수동 검증에서 확인할 항목

- 시작 화면에서 현재 플레이 목표와 첫 행동이 명확한가
- 사건 화면에서 현재 세대, 인물, 가문 상태가 헷갈리지 않는가
- 선택 결과 화면에서 결과 문구, 상태 변화, 연대기 기록의 관계가 읽히는가
- 상세 상태 토글이 찾기 쉽고 기본 흐름을 방해하지 않는가
- 세대 요약 화면에서 한 세대가 끝났다는 감각과 다음 행동이 명확한가
- 후계 화면에서 다음 세대로 이어지는 이유와 조작이 이해되는가
- 2세대 진입 시 이전 세대의 상태, 큰 사건 결과, 기록 이력이 이어졌다는 점이 보이는가
- 멸문 화면에서 게임 종료와 전체 가문 이력을 충분히 확인할 수 있는가
- 텍스트가 과밀하거나 잘리지 않고, 버튼과 본문이 서로 방해하지 않는가
- 자리표시자 문구가 제품 문안처럼 오해되지 않는가

## 제외 범위

- 구현 파일 수정
- 하위 프로젝트 내부 문서 수정
- 완성형 UI/아트 스타일 확정
- 시대/세계관 톤과 가문/인물 이름 후보 정리
- 상태 값 이름, 구간 라벨 문구, 수치, 밸런스 확정
- 후계 가능성 계산식, 감쇠율, 특성 상속 확률 확정
- 저장/불러오기, 혼인, 혈통, 대량 사건 콘텐츠 구현 지시

## Root Codex 완료 기준

- 기능 구현 완료와 UI/UX 검증 완료를 분리해 현재 상태를 정리한다.
- 사용자가 직접 플레이하며 확인할 UI/UX 체크리스트를 작성한다.
- UI/UX 문제가 발견될 때 Project Codex로 되돌릴 제품 요구 수준의 보고 형식을 정한다.
- 사용자에게 구현된 것과 테스트할 항목을 설명하고, 피드백을 남기도록 유도하는 안내 문서를 제공한다.
- 다음 하위 구현 흐름에 넘길 경우, 작업 의도와 사용자 조작 검증 기준을 함께 포함한다.

## 보고 형식

Root Codex는 다음 형식으로 결과를 돌려준다.

- 현재 기능 구현 상태
- UI/UX 수동 검증 체크리스트
- 수동 검증 결과 기록 형식
- Project Codex로 되돌릴 개선 기준
- 다음 작업 초점
