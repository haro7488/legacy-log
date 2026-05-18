# Project Codex 위임 프롬프트: Vertical Slice 1 구현 계획 수립

> 최종 수정: 2026-05-19

아래 프롬프트는 Project Codex에 전달하기 위한 초안이다.

```text
## 작업명
Vertical Slice 1: 중세 판타지 귀족가문 반복 플레이 재미 검증 빌드 구현 계획 수립

## 역할 경계
당신은 Project Codex다.

Root Codex는 제품 방향, 세부 기획, 사건 목록, 유산 태그, 작위 기준, 수동 검증 기준을 이미 작성했다.
Project Codex는 세부 기획을 새로 확정하지 않는다.

당신의 역할은 Root 기획 문서를 바탕으로 구현 가능한 기술 설계, 데이터 구조 제안, 작업 단계, Project Claude에 넘길 최종 구현 지시서 초안을 작성하는 것이다.

기획 공백이나 모순이 발견되면 임의로 채우지 말고 Root Codex에 되돌릴 질문으로 분리한다.

## 참조 문서
반드시 먼저 읽는다.

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/current-focus.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/011-ui-ux-validation-workflow.md`
- `docs/decisions/012-responsive-tabbed-ui-direction.md`
- `docs/decisions/013-medieval-fantasy-noble-house-direction.md`
- `docs/decisions/014-vertical-slice-1-playtest-scope.md`
- `docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`
- `docs/decisions/016-root-planning-project-implementation-boundary.md`
- `docs/product/vertical-slice-1-event-list.md`
- `docs/product/vertical-slice-1-event-copy-draft.md`
- `docs/product/vertical-slice-1-event-chronicle-draft.md`
- `docs/product/vertical-slice-1-legacy-tag-rules.md`
- `docs/product/vertical-slice-1-title-progression-rules.md`
- `docs/product/vertical-slice-1-generation-event-flow.md`
- `docs/product/vertical-slice-1-event-tag-change-table.md`
- `docs/product/vertical-slice-1-generation-start-and-tag-display.md`
- `docs/product/vertical-slice-1-generation-summary-templates.md`
- `docs/product/vertical-slice-1-manual-playtest-guide.md`

하위 구현 프로젝트 맥락은 다음을 읽는다.

- `legacy-log-project/AGENTS.md`
- `legacy-log-project/CLAUDE.md`
- 최신 관련 handoff 문서

## 목적
Vertical Slice 1은 MVP 완료 여부를 다시 증명하는 빌드가 아니다.

목적은 중세 판타지 귀족가문 게임이 반복 플레이에서 재미를 만들 수 있는지 검증하는 것이다.

검증해야 할 핵심은 다음이다.

- 여러 번 플레이했을 때 서로 다른 가문사가 만들어지는가
- 남작에서 공작까지 작위 상승과 상실이 보상과 부담을 함께 만드는가
- 사건 선택이 재산, 명예, 궁정 영향력, 가문 결속에 의미 있는 변화를 만드는가
- 유산 태그가 다음 세대 사건 풀과 세대 시작 조건에 보이는가
- 후계자 특성이 같은 상태에서도 다른 운영 감각을 만드는가
- UI/UX가 작은 창과 세로형 화면에서도 판단을 방해하지 않는가

## 구현 계획 범위
다음에 대한 구현 계획을 작성한다.

1. Vertical Slice 1 데이터 구조 제안
   - 사건 46개를 다루는 방식
   - 사건 분류, 등장 조건, 선택지, 결과, 유산 태그, 연대기 문장 표현 방식
   - 작위, 작위 위험 단계, 대표작위 변화 표현 방식
   - 세대 시작 정보와 세대 요약 정보 표현 방식

2. 게임 루프 단계 제안
   - 세대 시작
   - 사건 후보 선정
   - 사건 표시
   - 선택 결과 적용
   - 연대기/유산 태그 기록
   - 세대 종결
   - 다음 세대 시작 또는 멸문 종료

3. UI/UX 구현 계획
   - 작은 창과 세로형 화면 대응
   - 항상 보이는 핵심 정보와 탭/보조 영역 정보 분리
   - 사건 화면, 결과 화면, 세대 시작 화면, 세대 요약 화면의 정보 우선순위
   - 수동 검증자가 확인해야 할 정보가 화면에서 드러나는지

4. 검증 계획
   - 기능 검증 항목
   - 반복 플레이 검증 항목
   - UI/UX 수동 검증 항목
   - `docs/product/vertical-slice-1-manual-playtest-guide.md`와 연결되는 보고 방식

5. Project Claude 최종 구현 지시서 초안
   - 목적
   - 범위
   - 제외 범위
   - 사전 가정
   - 구현 단계
   - 완료 기준
   - 검증 방법
   - 보고 형식

## 제외 범위
Project Codex는 다음을 하지 않는다.

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 문체 최종 확정
- 밸런스 수치 확정
- 왕/황제 실제 통치 시스템 설계
- 혼인/혈통 트리/저장 불러오기 같은 비범위 기능 추가
- Root 기획 문서와 다른 게임 규칙 확정

## 사전 가정

- Root 기획 문서가 제품 기준의 source of truth다.
- 구현 세부 구조는 Project Codex가 제안할 수 있다.
- Project Codex가 제안하는 데이터 구조는 Root 기획을 표현하기 위한 수단이어야 하며, 기획을 바꾸면 안 된다.
- 모순이나 누락이 있으면 `Root 확인 필요`로 보고한다.
- 구현은 작은 증분으로 나누고, 각 증분마다 검증 가능해야 한다.

## 제품 완료 기준
Project Codex가 세우는 구현 계획은 최종적으로 다음을 만족해야 한다.

- 사용자가 3회 이상 반복 플레이하며 서로 다른 가문사 흐름을 확인할 수 있다.
- 한 실행은 보통 4~8세대, 10~20분 내외를 목표로 한다.
- 한 세대는 보통 5~7개의 상호작용 사건을 가진다.
- 사건은 작위, 상태, 유산 태그, 후계자 특성 중 최소 하나와 연결된다.
- 작위 상승은 자동 보상이 아니라 선택 가능한 기회로 표현된다.
- 작위 상실은 즉시 게임오버가 아니라 쇠락 단계로 표현된다.
- 멸문은 후계 부재와 사망이 결합될 때 발생한다.
- 세대 시작 화면에서 이전 세대 결과와 이번 세대 조건이 연결된다.
- 결과 화면에서 상태 변화, 태그 변화, 작위 변화 또는 위험 변화가 보인다.
- 작은 창과 세로형 화면에서도 핵심 정보 확인과 조작이 가능하다.

## 보고 형식
다음 순서로 보고한다.

1. 읽은 문서 목록과 확인한 제품 기준 요약
2. 구현 데이터 구조 제안
3. 게임 루프 단계 제안
4. UI/UX 구현 계획
5. 검증 계획
6. Project Claude 최종 구현 지시서 초안
7. Root 확인 필요 질문 목록

## 멈춰야 하는 경우
다음 경우에는 추측으로 진행하지 말고 보고한다.

- Root 기획 문서끼리 모순이 있는 경우
- 사건 46개를 구현 데이터로 옮기기에 필수 정보가 부족한 경우
- 현재 하위 프로젝트 구조가 Vertical Slice 1 구현을 막는 경우
- UI/UX 요구가 현재 구현 구조와 크게 충돌하는 경우
```
