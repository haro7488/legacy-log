# Handoff Documents

이 디렉터리는 Project Codex와 Project Claude 사이의 작업 인계 문서, 그리고 Root Codex가 진행상황을 파악하기 위한 요약 문서를 보관한다.

## 디렉터리 구조

작업 단위마다 별도 디렉터리를 만든다.

```text
docs/handoff/<task-slug>/
```

예:

```text
docs/handoff/mvp-first-loop/
```

## 파일 순서

각 작업 디렉터리는 다음 순서를 따른다.

1. `01-review-request.md` — Project Codex가 Project Claude에 넘기는 리뷰 요청
2. `02-claude-review.md` — Project Claude의 리뷰 결과
3. `03-final-work-instruction.md` — Project Codex가 리뷰를 반영해 작성한 최종 구현 지시서
4. `04-implementation-report.md` — Project Claude의 구현 결과 보고
5. `05-root-status-summary.md` — Project Codex가 Root Codex에 전달하는 진행상황/완료 요약

## Root Codex가 읽을 문서

Root Codex가 하위 구현 진행상황을 파악할 때는 먼저 해당 작업의 `05-root-status-summary.md`를 읽는다.

필요하면 같은 디렉터리의 앞선 문서를 순서대로 확인한다.

```text
01-review-request.md
02-claude-review.md
03-final-work-instruction.md
04-implementation-report.md
05-root-status-summary.md
```

## 작성 원칙

- Handoff 문서는 작업의 근거와 결과를 보존하기 위한 문서다.
- 완료된 작업 문서는 삭제하지 않고 작업 디렉터리에 보관한다.
- 문서 경로를 이동하면 내부의 전달 문장, 답변 문서 경로, 구현 결과 보고 경로도 함께 갱신한다.
- Root Codex용 요약은 제품 완료 기준, 검증 결과, 남은 제약, 다음 판단 필요 사항을 짧게 정리한다.
