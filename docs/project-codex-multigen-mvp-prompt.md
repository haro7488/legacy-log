# Project Codex 위임 프롬프트: 다세대 MVP 구현 계획

아래 프롬프트를 `legacy-log-project/`에서 Project Codex에게 전달한다.

```markdown
## 작업명
다세대 MVP 구현 계획 및 Project Claude 리뷰 요청서 작성

## 역할
너는 `legacy-log-project/`의 Project Codex다.

Root Codex가 정리한 PRD와 결정문서를 바탕으로, 다세대 MVP 구현을 위한 아키텍처 및 구현 계획을 작성한다. 이번 단계에서는 직접 구현하지 않는다. Project Claude가 리뷰할 수 있는 계획과 리뷰 요청서를 작성하는 것이 목표다.

## 배경
LegacyLog의 MVP는 더 이상 단일 세대 첫 루프가 아니다. 현재 제품 정체는 "가문이 멸문할 때까지 다세대를 이어가며 가문의 시작과 끝을 경험하는 게임"이다.

현재 구현은 단일 세대 첫 루프 모듈의 동작 검증 완료 상태로 본다. 이 모듈은 새 실행, 사건 진행, 선택 결과, 최소 상태 변화, 연대기 누적, 세대 요약을 확인한 출발점이다.

## 반드시 참조할 문서
루트 저장소 기준:

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/current-focus.md`
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

하위 프로젝트 기준:

- `legacy-log-project/AGENTS.md`
- `legacy-log-project/CLAUDE.md`
- 현재 구현 파일과 씬 구성

## 구현 목표
Project Claude가 구현할 수 있도록, 다세대 MVP 구현 계획과 리뷰 요청서를 작성한다.

제품 기준의 목표는 다음과 같다.

- 단일 세대 모듈을 다세대 진행으로 연결한다.
- 다세대 MVP는 최소 2세대 이상을 다루며, 상한을 고정하지 않고 가문이 멸문할 때까지 진행한다.
- 세대 간 이어짐 매개는 가문 상태, 큰 사건 결과, 연대기다.
- 후계 메커니즘은 혼합형으로 둔다.
- 기본 멸문 조건은 후계 부재 상태에서 현 세대 인물이 사망하거나 퇴장해 다음 세대로 이어질 수 없는 경우다.
- 특성 시스템은 최소 스텁으로만 도입한다.
- 세대 종결 방식 다종성은 결과 유형과 요약 문장 수준에서 구현한다.
- 연대기 문체는 진행 중 1인칭, 세대 끝 후대 기록자 시점으로 분리한다.
- 세대 요약은 이전 세대의 유산과 다음 세대가 직면할 상황 암시를 포함한다.
- 상태 표시는 기본적으로 질적 구간 라벨과 세대 끝 서술 요약을 사용하고, 추가 정보창에서 숫자와 변화 방향을 볼 수 있게 한다.
- 현재 임시 가문명, 인물명, 도입 문장, 세대 마무리 문장은 자리표시자형 표현으로 교체하는 위생 작업을 계획에 포함할지 검토한다.

## 제외 범위
다음은 이번 구현 계획에서 확정하거나 구현 대상으로 넓히지 않는다.

- 완성형 가문 생성기
- 저장/불러오기
- 완성형 혼인, 혈통, 계보 시뮬레이션
- 정교한 자원 경제
- 대량 사건 콘텐츠
- 완성형 UI/아트 스타일 확정
- 시대/세계관 톤 결정
- 가문/인물 이름 후보 확정
- 상태 값 이름, 개수, 수치, 밸런스 확정
- 평판형 감쇠율 확정
- 후계 가능성 계산식 확정
- 특성 상속 확률 확정
- 상태 구간 라벨의 최종 문구 확정
- 추가 정보창의 구체 UI 디자인 확정
- PRD와 결정문서에 없는 게임 규칙 추가

## 사전 가정
- 현재 구현은 단일 세대 첫 루프 모듈로 유지한다.
- 기존 `mvp-first-loop` handoff 결과를 출발점으로 삼는다.
- PRD와 결정문서 007-010은 확정된 제품 요구로 본다.
- 결정 보류 항목은 구현 계획에서 임의로 확정하지 않는다.
- 구현 세부 아키텍처, 데이터 구조, 파일 구성, Godot 씬 구성, C# 클래스 구조는 Project Codex가 하위 프로젝트 맥락을 확인한 뒤 제안한다.
- 실제 코드는 Project Claude가 최종 작업 지시서 이후 구현한다.

## 작성할 산출물
Project Codex는 다음 산출물을 작성한다.

1. 다세대 MVP 구현 계획
   - 현재 단일 세대 구현의 어떤 부분을 유지할지
   - 어떤 기능 축을 추가할지
   - 예상 파일/모듈 단위의 변경 방향
   - 기존 임시 검증 코드와 임시 콘텐츠를 어떻게 다룰지
   - 결정 보류 항목을 어떻게 임시 처리 또는 명시적으로 제외할지

2. Project Claude 리뷰 요청서
   - 목적
   - 수정 범위
   - 제외 범위
   - 사전 가정
   - 구현 단계 초안
   - 검증 기준
   - 리뷰에서 특히 확인받을 위험 지점

3. 보고 요약
   - 계획 요약
   - 결정 보류로 남긴 항목
   - Project Claude에게 확인받을 질문
   - Root Codex 판단이 다시 필요한 항목

가능하면 기존 handoff 체계를 따라 `legacy-log-project/docs/handoff/` 아래에 새 작업 폴더를 만들고, 파일명은 기존 `mvp-first-loop` 흐름과 일관되게 잡는다. 단, 하위 프로젝트 지침과 충돌하면 하위 프로젝트 지침을 우선한다.

## 검증 기준
이번 Project Codex 단계의 완료 기준은 코드 실행이 아니다.

- PRD와 결정문서 003-010의 제품 요구가 구현 계획에 반영되어 있다.
- 단일 세대 모듈과 다세대 MVP의 관계가 명확하다.
- 구현 범위와 제외 범위가 분리되어 있다.
- 결정 보류 항목을 임의로 확정하지 않았다.
- Project Claude가 리뷰할 수 있는 구체적 계획과 질문이 있다.
- 실제 구현 전 리뷰가 필요한 위험 지점이 드러나 있다.

## 보고 형식
다음 순서로 보고한다.

1. 작성한 파일 경로
2. 구현 계획 핵심 요약
3. Project Claude 리뷰 요청 핵심 질문
4. 결정 보류 또는 Root Codex 재확인이 필요한 항목
5. 다음 단계
```
