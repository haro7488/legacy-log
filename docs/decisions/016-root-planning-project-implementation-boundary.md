# 016. Root 기획 담당과 Project 구현 담당 경계

## 상태
확정 (2026-05-19)

## 맥락

기존 문서 일부는 세부 콘텐츠, 사건 목록, 게임 규칙, 밸런스 방향까지 Project Codex 또는 하위 구현 흐름에 넘기는 것처럼 읽힐 수 있었다.

그러나 LegacyLog의 운영 경계는 다음과 같이 정리한다.

- Root Codex: 제품 방향, PRD, 게임 기획, 세부 콘텐츠 기준, 검증 기준 담당
- Project Codex: Root가 확정한 기획을 구현 가능한 구조, 데이터, 코드 작업으로 옮기는 구현 계획 담당
- Project Claude: 최종 지시서 기반 구현과 검증 담당

## 결정

Root Codex는 **기획 담당**이다.

Root Codex는 다음을 결정하고 문서화한다.

- 제품 방향
- 게임 정체
- MVP와 Vertical Slice 범위
- 게임 규칙의 제품 수준 정의
- 작위, 상태, 이벤트, 유산 태그, 후계자, 문체, 이름, UI/UX 검증 기준
- 사건 분류, 사건 목록, 사건별 기획 의도, 선택지 의미, 결과의 기획 기준
- 반복 플레이 검증 질문과 피드백 양식

Project Codex는 **구현 담당**이다.

Project Codex는 Root가 확정한 기획을 기준으로 다음을 담당한다.

- 구현 가능성 검토
- 기술 설계
- 데이터 파일 형식 제안
- Godot/C# 구조 설계
- 작업 단위 분해
- Project Claude가 실행할 최종 작업 지시서 작성

Project Codex는 Root가 확정하지 않은 기획 내용을 임의로 확정하지 않는다. 구현 과정에서 기획 공백이 발견되면 Root로 되돌려 질문해야 한다.

## 근거

LegacyLog는 기획 의도, 연대기 감각, 작위와 사건의 의미 연결이 핵심인 게임이다. 세부 기획을 구현 흐름에서 임의로 채우면 PRD, 결정문서, 실제 구현이 서로 다른 방향으로 갈 가능성이 커진다.

반대로 기술 구조와 데이터 저장 형식까지 Root가 확정하면 하위 프로젝트의 구현 판단 여지를 지나치게 제한한다. 따라서 Root는 **무엇을 만들지**와 **왜 필요한지**를 정하고, Project Codex는 **어떻게 구현할지**를 정한다.

## 영향

`docs/decisions/002-agent-workflow.md`의 기본 흐름은 유지한다. 다만 Project Codex의 역할은 세부 기획 결정이 아니라 구현 계획과 작업 지시서 작성으로 한정된다.

`docs/product/prd.md`, `docs/current-focus.md`, `docs/decisions/014-vertical-slice-1-playtest-scope.md`, `docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`에서 하위 구현 흐름에 콘텐츠 상세를 넘기는 것처럼 보이는 문구는 이 결정에 맞게 해석하거나 정정한다.

Vertical Slice 1의 사건별 목록, 사건별 기획 의도, 선택지 의미, 유산 태그 적용 기준, 피드백 기록 양식은 Root Codex가 후속 기획 라운드에서 정한다.

데이터 파일 형식, 로딩 구조, C# 타입, Godot 씬/리소스 구성, 자동 검증 방식은 Project Codex와 Project Claude의 구현 흐름에서 정한다.

## 관련

- `docs/decisions/002-agent-workflow.md`
- `docs/product/prd.md`
- `docs/current-focus.md`
- `docs/decisions/014-vertical-slice-1-playtest-scope.md`
- `docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md`
