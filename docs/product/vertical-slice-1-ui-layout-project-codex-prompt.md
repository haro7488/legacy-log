# Project Codex 위임 프롬프트: Vertical Slice 1 UI 레이아웃 개선 계획 수립

> 작성일: 2026-05-22

아래 프롬프트는 Project Codex에 전달하기 위한 초안이다.

```text
## 작업명
Vertical Slice 1 UI 레이아웃 개선 계획 수립

## 역할 경계
당신은 Project Codex다.

Root Codex는 제품 방향과 UI 배치 기준을 정리했다.
Project Codex는 이 기준을 바탕으로 구현 가능한 기술 설계, 변경 범위, 검증 계획, Project Claude에 넘길 최종 구현 지시서 초안을 작성한다.

기획 공백이나 제품 기준 충돌이 발견되면 임의로 채우지 말고 Root Codex에 되돌릴 질문으로 분리한다.

## 반드시 읽을 문서

루트 제품 문서:

- `AGENTS.md`
- `docs/current-focus.md`
- `docs/product/prd.md`
- `docs/product/vertical-slice-1-ui-layout-direction.md`
- `docs/product/vertical-slice-1-manual-playtest-guide.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/011-ui-ux-validation-workflow.md`
- `docs/decisions/012-responsive-tabbed-ui-direction.md`
- `docs/decisions/014-vertical-slice-1-playtest-scope.md`
- `docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`
- `docs/decisions/016-root-planning-project-implementation-boundary.md`

하위 구현 프로젝트 문서:

- `legacy-log-project/AGENTS.md`
- `legacy-log-project/CLAUDE.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-playtest-build/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`

## 목적

Vertical Slice 1은 이미 기능 구현과 수동 확인이 완료된 상태로 보고됐다.

이번 작업의 목적은 새 게임 규칙을 추가하는 것이 아니라, 현재 VS1 UI를 반복 플레이 검증에 더 적합한 화면 구조로 정리하는 것이다.

제품 방향은 다음이다.

- 상단에는 현재 맥락을 보여주는 헤더를 둔다.
- 중간에는 선택된 화면의 전체 내용을 보여주는 단일 게임 화면을 둔다.
- 하단에는 `사건`, `상태`, `연대기`, `가문사` 탭 메뉴를 둔다.
- 하단 탭은 보조 패널이 아니라 주 화면 전환 메뉴로 동작한다.
- 사건 선택지와 주요 진행 버튼은 별도 하단 ActionBar가 아니라 사건/결과/요약 화면 내부의 주요 행동 영역에 배치하는 방향을 우선 검토한다.

## 구현 계획 범위

다음에 대한 구현 계획을 작성한다.

1. 현재 VS1 UI 구조 파악
   - 현재 상단 헤더, 본문, 탭, ActionBar 구조
   - 사건, 결과, 세대 시작, 세대 요약, 멸문 화면의 현재 표시 방식
   - 상태, 연대기, 가문사 정보의 현재 표시 방식

2. 3단 UI 구조 전환 계획
   - 상단 헤더에 남길 정보
   - 중간 단일 게임 화면으로 옮길 정보
   - 하단 탭 메뉴 구성
   - `사건`, `상태`, `연대기`, `가문사` 탭 전환 상태 관리

3. 사건 탭 설계
   - 세대 시작 화면
   - 사건 화면
   - 결과 화면
   - 세대 요약 화면
   - 멸문 또는 실행 종료 화면
   - 선택지와 진행 버튼의 위치

4. 상태/연대기/가문사 탭 설계
   - 상태 화면의 질적 구간, 작위 위험, 현재 인물 특성 표시
   - 연대기 화면의 최신 기록, 큰 사건, 보조 사건 표시
   - 가문사 화면의 활성 유산 태그, 보유 작위, 지난 세대 이력 표시

5. 검증 계획
   - 기존 기능 smoke 검증 유지 방안
   - UI 수동 검증 절차
   - 360x640, 540x720, 1280x720 창 크기 검증
   - 선택지와 하단 탭이 겹치거나 잘리지 않는지 확인
   - 상태/연대기/가문사 탭을 반복 플레이 중 자연스럽게 오갈 수 있는지 확인

6. Project Claude 최종 구현 지시서 초안
   - 목적
   - 범위
   - 제외 범위
   - 사전 가정
   - 구현 단계
   - 완료 기준
   - 검증 방법
   - 보고 형식

## 제품 완료 기준

구현 계획은 최종적으로 다음을 만족해야 한다.

- 상단 헤더만 보고 현재 가문, 대표작위, 세대, 현재 인물, 현재 단계가 이해된다.
- 중간 화면은 선택된 탭의 내용을 읽는 단일 주 화면으로 동작한다.
- 하단 탭으로 `사건`, `상태`, `연대기`, `가문사`를 전환할 수 있다.
- 사건 화면에서 선택지와 영향 요약이 함께 읽힌다.
- 결과 화면에서 상태 변화, 유산 태그 변화, 작위 변화 또는 위험 변화, 연대기 기록이 순서대로 이해된다.
- 상태 화면에서 숫자 상세를 열지 않아도 현재 가문의 위험과 강점을 판단할 수 있다.
- 연대기 화면에서 최신 기록과 큰 사건이 구분된다.
- 가문사 화면에서 이전 세대의 흔적이 현재 세대와 연결되어 보인다.
- 360x640, 540x720, 1280x720에서 하단 탭, 선택지, 본문이 겹치거나 잘리지 않는다.
- 기존 VS1 기능 smoke 검증은 유지된다.

## 제외 범위

Project Codex는 이번 계획에서 다음을 하지 않는다.

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 상태 수치, 가중치, 밸런스 확정
- 최종 UI/아트 스타일 확정
- 저장/불러오기, 혼인, 혈통 트리 같은 비범위 기능 추가
- Root 제품 기준과 다른 게임 규칙 확정

## 사전 가정

- Root 제품 문서가 제품 기준의 source of truth다.
- 구현 세부 구조와 Godot 노드 구성은 Project Codex가 현재 코드 기준으로 제안한다.
- UI 개선은 기존 VS1 플레이 흐름을 유지하는 범위에서 진행한다.
- 자동 smoke는 기능 회귀를 확인하고, UI 품질은 수동 검증으로 확인한다.
- 구현 변경은 작은 증분으로 나누고, 검증 가능한 단위로 Project Claude에 넘긴다.

## 보고 형식

다음 순서로 보고한다.

1. 읽은 문서 목록과 확인한 제품 기준 요약
2. 현재 UI 구조 요약
3. 3단 UI 구조 전환 계획
4. 사건/상태/연대기/가문사 탭별 표시 계획
5. 검증 계획
6. Project Claude 최종 구현 지시서 초안
7. Root 확인 필요 질문 목록

## 멈춰야 하는 경우

다음 경우에는 추측으로 진행하지 말고 보고한다.

- 현재 구현 구조가 3단 UI 구조와 크게 충돌하는 경우
- 하단 탭 전환이 기존 사건 진행 상태를 깨뜨릴 위험이 큰 경우
- 작은 창 검증 기준을 만족하려면 제품 기준 변경이 필요한 경우
- Root 문서끼리 UI 배치 기준이 충돌하는 경우
```
