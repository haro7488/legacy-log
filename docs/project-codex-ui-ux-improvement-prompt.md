# Project Codex 위임 프롬프트: 다세대 MVP UI/UX 편의성 개선

아래 프롬프트를 `legacy-log-project/`에서 Project Codex에게 전달한다.

```markdown
## 작업명
다세대 MVP UI/UX 편의성 개선 계획 및 Project Claude 리뷰 요청서 작성

## 역할
너는 `legacy-log-project/`의 Project Codex다.

Root Codex가 정리한 PRD와 UI/UX 관련 결정문서를 바탕으로, 현재 구현된 다세대 MVP UI를 사용자가 직접 플레이하며 검증할 수 있는 수준으로 개선하기 위한 계획을 작성한다. 이번 단계에서는 직접 구현하지 않는다. Project Claude가 리뷰할 수 있는 UI/UX 개선 계획과 리뷰 요청서를 작성하는 것이 목표다.

## 배경
다세대 MVP 기능은 하위 handoff 기준으로 구현 완료 보고됐다.

그러나 현재 UI는 기능 경로를 보여주는 성격이 강할 가능성이 있다. 수동 검증을 시작하기 전에, 작은 창과 세로형 모바일 기기에서도 사용할 수 있는 범용적인 정보 구조와 조작 흐름을 먼저 정리해야 한다.

기능 검증만으로 작업을 닫지 않는다. 이번 계획은 사용자가 직접 플레이하며 UI/UX를 확인할 수 있도록 만드는 데 초점을 둔다.

## 반드시 참조할 문서
루트 저장소 기준:

- `AGENTS.md`
- `docs/product/prd.md`
- `docs/current-focus.md`
- `docs/decisions/002-agent-workflow.md`
- `docs/decisions/007-multigen-game-identity.md`
- `docs/decisions/008-multigen-mvp-scope.md`
- `docs/decisions/009-chronicle-style.md`
- `docs/decisions/010-state-display-layering.md`
- `docs/decisions/011-ui-ux-validation-workflow.md`
- `docs/decisions/012-responsive-tabbed-ui-direction.md`
- `docs/manual-ui-ux-validation-guide.md`
- `legacy-log-project/docs/handoff/multigen-mvp/05-root-status-summary.md`
- `legacy-log-project/docs/handoff/multigen-mvp/04-implementation-report.md`

하위 프로젝트 기준:

- `legacy-log-project/AGENTS.md`
- `legacy-log-project/CLAUDE.md`
- 현재 UI 구현 파일
- 현재 Godot 씬 구성

## 제품 방향
다음 방향을 따른다.

- 작은 창과 세로형 모바일 기기 모두에서 사용 가능한 범용 디자인을 고려한다.
- 이후 UI 변경, 추가, 삭제가 쉽도록 명확한 레이아웃 구조를 고려한다.
- UI 구현 방식은 Project Codex가 현재 코드와 Godot 구조를 보고 편하다고 판단되는 방식으로 직접 제안한다.
- 항상 보이는 정보와 특정 탭에서 보이는 정보를 구분한다.
- 기본 UI는 모바일 탭 형식으로 오가며 확인할 수 있는 방식을 기준으로 확장한다.
- 목표는 사용자가 시작부터 멸문까지 불편 없이 진행하는 것이다.
- 체크리스트 기반 수동 검증도 가능해야 한다.

## 개선 목표
Project Claude가 구현할 수 있도록, 다음 목표를 만족하는 UI/UX 개선 계획을 작성한다.

- 시작, 사건, 결과, 세대 요약, 후계, 2세대 진입, 멸문 화면의 목적이 명확하다.
- 각 화면에서 주요 행동 버튼이 분명하다.
- 현재 가문, 세대, 인물, 현재 단계는 항상 확인 가능하다.
- 사건 본문, 선택지, 결과, 상태 변화, 연대기 기록이 서로 섞여 보이지 않는다.
- 상태, 연대기, 이전 세대 기록, 큰 사건 결과, 특성 스텁은 탭 또는 보조 영역에서 확인할 수 있다.
- 작은 창과 세로형 모바일 화면에서도 텍스트와 버튼이 과밀하지 않다.
- 상세 정보는 기본 플레이 흐름을 방해하지 않는다.
- 수동 검증자가 무엇을 확인해야 하는지 명확한 체크리스트가 있다.

## 제외 범위
이번 계획에서 다음을 확정하거나 구현 범위로 넓히지 않는다.

- 완성형 UI/아트 스타일
- 최종 색상, 폰트, 장식 스타일
- 시대/세계관 톤
- 가문/인물 이름 후보
- 상태 값 이름, 구간 라벨 최종 문구, 수치, 밸런스
- 후계 가능성 계산식, 감쇠율, 특성 상속 확률
- 저장/불러오기
- 혼인, 혈통, 대량 사건 콘텐츠
- PRD와 결정문서에 없는 새 게임 규칙

## 사전 가정
- 현재 다세대 MVP 기능 구현은 출발점으로 삼는다.
- 기존 기능 흐름을 깨지 않고 UI/UX 편의성을 개선하는 계획을 세운다.
- 작은 창과 세로형 모바일 대응을 우선한다.
- 탭 기반 정보 구조를 기본 방향으로 삼는다.
- 기능 완료와 UI/UX 검증 완료를 분리해서 보고한다.
- 실제 구현은 Project Claude가 최종 작업 지시서 이후 수행한다.

## 작성할 산출물
Project Codex는 다음 산출물을 작성한다.

1. UI/UX 개선 계획
   - 현재 UI의 예상 문제점
   - 화면별 목적과 주요 행동
   - 항상 보이는 정보
   - 탭 또는 보조 영역으로 분리할 정보
   - 작은 창/세로형 모바일 대응 전략
   - 구현 예상 변경 파일
   - 기능 보존 위험과 대응

2. Project Claude 리뷰 요청서
   - 목적
   - 수정 범위
   - 제외 범위
   - 사전 가정
   - 구현 단계 초안
   - 수동 UI/UX 검증 기준
   - 리뷰에서 확인받을 위험 지점

3. 사용자 수동 검증 안내 초안
   - 구현 후 사용자가 어떤 순서로 플레이할지
   - 화면별로 무엇을 확인할지
   - 어떤 형식으로 피드백을 남길지
   - 어떤 문제가 있으면 Project Codex로 되돌릴지

가능하면 기존 handoff 체계를 따라 `legacy-log-project/docs/handoff/` 아래에 새 작업 폴더를 만들고, 파일명은 기존 흐름과 일관되게 잡는다. 단, 하위 프로젝트 지침과 충돌하면 하위 프로젝트 지침을 우선한다.

## 검증 기준
이번 Project Codex 단계의 완료 기준은 코드 실행이 아니다.

- UI/UX 개선 의도가 화면별로 명확하다.
- 작은 창과 세로형 모바일 대응 기준이 계획에 포함되어 있다.
- 항상 보이는 정보와 탭 정보가 구분되어 있다.
- 기능 검증과 UI/UX 수동 검증 기준이 분리되어 있다.
- 사용자가 직접 플레이하며 피드백할 수 있는 안내가 있다.
- Project Claude가 리뷰할 수 있는 구체적 계획과 질문이 있다.

## 보고 형식
다음 순서로 보고한다.

1. 작성한 파일 경로
2. UI/UX 개선 계획 핵심 요약
3. 제안한 탭 구조와 항상 보이는 정보
4. Project Claude 리뷰 요청 핵심 질문
5. 사용자 수동 검증 안내 요약
6. Root Codex 판단이 필요한 항목
7. 다음 단계
```
