# Current Focus

> 최종 수정: 2026-05-22

## 현재 초점

현재 초점은 **Vertical Slice 1 UI 레이아웃 개선 handoff 완료 확인**이다.

Vertical Slice 1 플레이테스트 빌드는 하위 handoff 기준으로 구현 완료 및 수동 검증 완료 상태로 보고됐다. 이어서 진행한 UI 레이아웃 개선도 `vertical-slice-1-ui-layout-improvement` handoff 기준으로 구현, 자동 검증, 사용자 수동 UI 확인이 완료된 상태로 보고됐다.

Root Codex는 이번 작업을 직접 구현하지 않는다. 현재 역할은 하위 handoff 결과를 제품 기준으로 회수하고, 다음 판단 항목을 정리하는 것이다.

## 참조 문서

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/product/vertical-slice-1-ui-layout-direction.md`
- `docs/product/vertical-slice-1-ui-layout-project-codex-prompt.md`
- `docs/product/vertical-slice-1-manual-playtest-guide.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/011-ui-ux-validation-workflow.md`
- `docs/decisions/012-responsive-tabbed-ui-direction.md`
- `docs/decisions/014-vertical-slice-1-playtest-scope.md`
- `docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`
- `docs/decisions/016-root-planning-project-implementation-boundary.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-playtest-build/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-ui-layout-improvement/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-ui-layout-improvement/06-manual-playtest-results.md`

## 현재 상태 판단

- Vertical Slice 1 플레이테스트 빌드는 `vertical-slice-1-playtest-build` handoff 기준 완료로 본다.
- Vertical Slice 1 UI 레이아웃 개선은 `vertical-slice-1-ui-layout-improvement` handoff 기준 완료로 본다.
- UI 레이아웃 개선은 상단 헤더, 중간 단일 게임 화면, 하단 탭 메뉴 3단 구조를 구현한 것으로 보고됐다.
- 하단 탭은 `사건`, `상태`, `연대기`, `가문사`를 주 화면 전환 메뉴로 사용한다.
- 사건 선택지와 진행 버튼은 별도 하단 ActionBar가 아니라 사건 화면 내부 행동 영역으로 이동한 것으로 보고됐다.
- 자동 검증은 현재 파일 기준 재확인했다.
- 사용자 수동 UI 확인도 handoff에 완료로 기록됐다.
- 현재 Root 기준으로 구현을 막는 추가 확인 사항은 없다.

## 검증 결과

현재 파일 기준 Root Codex가 재확인한 자동 검증:

- `dotnet build`: 성공, 경고 0개, 오류 0개
- `.\scripts\godot.cmd --headless --import`: 성공
- `.\scripts\godot.cmd --headless -- --smoke`: 성공, `SMOKE_OK`

handoff 기준 수동 확인:

- 확인 주체: 사용자
- 확인 결과: UI/UX 수동 검증 완료
- 상세 창 크기별 기록, 스크린샷, 체크 테이블은 제공되지 않았다.

## 남은 제약

- 최종 UI/아트 스타일은 확정하지 않았다.
- 탭 라벨 축약은 현재 보류 상태다.
- 멸문 직후 자동으로 `가문사` 탭으로 이동하지 않는다.
- 상태 상세 접기/펼치기 시 스크롤 위치 보존은 구현하지 않았다.
- smoke는 UI 겹침, 잘림, 가독성을 자동 검증하지 않는다.
- 상세 회차별 수동 플레이테스트 기록은 제공되지 않았다.

## Root Codex 판단 필요 사항

현재 구현 완료를 막는 Root 확인 필요 사항은 없다.

후속 판단 후보는 다음이다.

1. 실제 반복 플레이 중 `360x640`에서 하단 탭 라벨이 잘리면 탭 라벨 축약을 허용할지 판단한다.
2. 멸문 화면에서 사용자가 다음 확인 위치를 이해하지 못하면 가문사 탭 안내 문구를 추가할지 판단한다.
3. 멸문 직후 자동으로 가문사 탭으로 이동하는 흐름을 허용할지 판단한다.
4. 헤더가 작은 창에서 과도하게 길면 헤더 정보 일부를 줄이거나 접을지 판단한다.
5. 상태 상세 접기/펼치기 후 스크롤 리셋이 불편하면 별도 UX 개선으로 분리할지 판단한다.

## 다음 작업 후보

1. 이번 UI 레이아웃 개선 작업을 커밋 단위로 정리한다.
2. 반복 플레이 중 발견되는 UI 문제만 작은 후속 작업으로 분리한다.
3. UI 문제가 없으면 Vertical Slice 1의 다음 제품 판단은 분기 사건 조건, 상태 구간/이월, 세대 요약 문체, 후계자 특성 가중치 중 하나로 이동한다.

## 제외 범위

- Root Codex의 구현 파일 수정
- 하위 프로젝트 내부 handoff 문서 직접 수정
- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 데이터 파일 형식, 로딩 구조, Godot 노드 구조 확정
- 최종 UI/아트 스타일 확정
- 저장/불러오기, 혼인, 혈통 트리, 정교한 경제/전쟁/AI 시뮬레이션

## 보고 형식

Root Codex는 다음 형식으로 결과를 돌려준다.

- 구현 완료 여부
- 제품 완료 기준 충족 여부
- 검증 결과
- 수동 확인 여부
- 남은 제약
- Root Codex가 판단해야 할 사항
- 다음 작업 후보
