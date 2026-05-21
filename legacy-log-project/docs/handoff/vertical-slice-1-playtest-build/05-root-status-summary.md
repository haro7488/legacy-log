# Vertical Slice 1 Playtest Build Root Status Summary

> 작성일: 2026-05-22
> 작성자: Project Codex
> 대상: Root Codex
> 기반 구현 보고: `docs/handoff/vertical-slice-1-playtest-build/04-implementation-report.md`
> 기반 지시서: `docs/handoff/vertical-slice-1-playtest-build/03-final-work-instruction.md`
> 수동 검증 기록: `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`

## 현재 구현 완료 여부

Vertical Slice 1: 중세 판타지 귀족가문 반복 플레이 재미 검증 빌드는 구현 완료 상태로 판단한다.

Project Claude는 기존 MVP 자리표시자 흐름을 보존하고, VS1 전용 `Vs1FamilyState`, `Vs1Models`, `Vs1Flow`, `Vs1Content.*`, `Vs1Smoke` 구조로 새 플레이 흐름을 구현했다. `Main.cs`는 VS1 흐름으로 전환됐고, 기존 4영역 UI shell(`ContextHeader`, `MainScroll`, `InfoTabs`, `ActionBar`)은 유지됐다.

Project Codex가 구현 보고 후 자동 검증 3종을 재확인했다. 이후 사용자가 수동 검증 완료를 보고했다.

## 제품 완료 기준별 충족 여부

| 제품 완료 기준 | 상태 | 근거 |
|---|---|---|
| 사용자가 3회 이상 반복 플레이하며 서로 다른 가문사 흐름을 확인할 수 있다 | 사용자 수동 검증 완료 | deterministic smoke 3 RUN이 서로 다른 승격/작위상실/멸문 경로를 출력. 사용자가 수동 검증 완료를 보고함. 상세 회차 기록은 미제공. |
| 한 실행은 보통 4~8세대, 10~20분 내외를 목표로 한다 | 사용자 수동 검증 완료 | smoke는 3~4세대 경로를 확인. 사용자가 수동 검증 완료를 보고함. 상세 시간/세대 기록은 미제공. |
| 한 세대는 보통 5~7개의 상호작용 사건을 가진다 | 충족 | smoke에서 각 세대가 6~7개 사건으로 진행됨. |
| 사건은 작위, 상태, 유산 태그, 후계자 특성 중 최소 하나와 연결된다 | 구현 충족 | 사건 조건/가중치/표시 단서 모델 구현. smoke에서 관련 조건 reason 출력 확인. |
| 작위 상승은 자동 보상이 아니라 선택 가능한 기회로 표현된다 | 충족 | PRO-* 사건 선택 결과로만 승격. smoke에서 PRO-01 청원 후 자작 승격 확인. |
| 작위 상실은 즉시 게임오버가 아니라 쇠락 단계로 표현된다 | 충족 | CRI-02 선택으로 대표작위 하락 확인. 멸문과 분리됨. |
| 멸문은 후계 부재와 사망/퇴장이 결합될 때 발생한다 | 기능 검증 충족 | smoke ASSERT `extinction_after_no_heir_only=True`. |
| 세대 시작 화면에서 이전 세대 결과와 이번 세대 조건이 연결된다 | 사용자 수동 검증 완료 | 세대 시작 정보, 핵심 유산 태그, 상태, 후계 프로필 표시 구현. 사용자가 수동 검증 완료를 보고함. |
| 결과 화면에서 상태 변화, 태그 변화, 작위 변화 또는 위험 변화가 보인다 | 사용자 수동 검증 완료 | `Main.ShowResult`가 상태/태그/작위/연대기 순서로 표시. 사용자가 수동 검증 완료를 보고함. |
| 작은 창과 세로형 화면에서도 핵심 정보 확인과 조작이 가능하다 | 사용자 수동 검증 완료 | 기존 4영역 shell 유지, ActionBar 세로 버튼, 탭 내부 스크롤 유지. 사용자가 수동 검증 완료를 보고함. |

## 검증 결과 요약

Project Claude 구현 보고 기준:

- `dotnet build`: 성공. 보고서에는 경고 34개, 오류 0개로 기록.
- `.\scripts\godot.cmd --headless --import`: 성공.
- `.\scripts\godot.cmd --headless -- --smoke`: 성공, `SMOKE_OK`.

Project Codex 재확인 기준:

- `dotnet build`: 성공, 경고 0개, 오류 0개.
- `.\scripts\godot.cmd --headless --import`: 성공, `DONE`.
- `.\scripts\godot.cmd --headless -- --smoke`: 성공, `SMOKE_OK`.

재확인한 smoke 핵심 ASSERT:

- `promotion_seen=True`
- `title_loss_seen=True`
- `legacy_tag_event_reason_seen=True`
- `extinction_after_no_heir_only=True`

## 수동 확인 여부

수동 GUI 또는 실제 플레이 검증은 사용자 보고 기준으로 완료됐다.

확인 주체: 사용자

확인 일자: 2026-05-22

확인 결과: 수동 검증 확인됨

상세 회차별 기록은 이번 보고에 포함되지 않았다. 따라서 다음 항목은 "사용자 확인 완료"로 닫되, 세부 회차 비교 데이터는 별도 플레이테스트 기록이 필요할 때 `06-manual-playtest-results.md`에 보강한다.

- 3회 이상 반복 플레이에서 서로 다른 가문사가 체감되는지
- 10~20분 플레이 리듬이 맞는지
- 4~8세대 목표 범위가 실제로 체감되는지
- 작위 상승/상실이 보상과 부담으로 읽히는지
- 세대 시작 화면이 10초 안에 이전 세대와 이번 세대 조건을 연결하는지
- 결과 화면의 상태/태그/작위 변화가 과밀하지 않은지
- 360x640, 540x720, 1280x720 창에서 버튼/탭/본문이 잘리지 않는지

수동 플레이테스트 결과 기록 경로:

- `docs/handoff/vertical-slice-1-playtest-build/06-manual-playtest-results.md`

기록 양식 기준:

- `D:\Project\Godot\legacy-log\docs\product\vertical-slice-1-manual-playtest-guide.md`

## 남은 제약 또는 미구현 범위

의도적 제외 범위:

- 새 사건 추가
- 사건 본문 재작성
- 작위 체계 변경
- 유산 태그 체계 변경
- 문체 최종 확정
- 밸런스 수치 최종 확정
- 왕/황제 실제 통치 시스템
- 혼인/혈통 트리/저장 불러오기
- 정교한 경제/전쟁/AI 시뮬레이션
- 지역 지도
- 최종 UI/아트 스타일

구현상 남은 제약:

- 선택지 결과의 `X 또는 Y` 분기는 현재 seed 기반 deterministic fallback이다. Root가 사건별 조건 규칙을 정하면 더 의미 있는 분기로 바꿀 수 있다.
- 사건 가중치, 상태 구간 임계값, 상태 이월 비율은 모두 임시값이다.
- 후계자 특성 기반 weight modifier 모델은 있으나, 이번 사건 데이터에는 후계자 특성 기반 modifier가 아직 충분히 채워지지 않았다.
- 세대 요약 문장은 템플릿 기반 자동 조합이라 여러 세대에서 단조롭게 느껴질 수 있다.
- 기존 MVP 파일(`Multigen*`, `MvpLoop*`)은 컴파일되는 dead code로 보존돼 있다.
- 작은 화면 GUI 검증은 사용자 보고 기준 완료됐다. 세부 창 크기별 기록은 미제공.

## Root Codex 판단 필요 사항

1. 이번 구현을 **기능 구현 및 사용자 수동 검증 완료** 상태로 닫을지 판단해야 한다.
2. 선택지 결과 분기(`X 또는 Y`)를 계속 seed fallback으로 둘지, Root가 사건별 조건 규칙을 추가 기획할지 결정해야 한다.
3. 일반 사건의 부상/사망 위험을 현재처럼 END-* 종결 사건에서만 실제 종결로 처리하는 단순화를 확정할지 판단해야 한다.
4. 후계 후보 풀 동적 추가/삭제를 이번 VS1 범위 밖으로 계속 둘지 판단해야 한다.
5. 상태 4축 이월 규칙과 상태 구간 임계값을 수동 검증 후 조정할지 판단해야 한다.
6. 기존 MVP dead code를 당분간 보존할지, 별도 정리 작업으로 제거할지 판단해야 한다.
7. `PRO-05 왕의 특별 포상`의 현재 발화 조건이 `왕의 총애` 중심이라, Root가 의도한 "큰 공훈 보유" 조건이 더 필요하면 후속 기획/구현이 필요하다.
8. 수동 플레이테스트 결과 문서 `06-manual-playtest-results.md`는 작성됐다. 다만 상세 회차 기록은 미제공이므로, 필요하면 후속으로 보강할지 판단해야 한다.

## 다음 작업 후보

1. 필요 시 `06-manual-playtest-results.md`에 상세 회차별 수동 검증 기록 보강
2. 수동 검증에서 발견된 문제가 있다면 UI/UX 과밀, 작은 창 문제, 정보 부족/과다로 분류
3. 분기 사건의 조건 규칙 추가 기획
4. 상태 구간/가중치/이월 임시값 조정
5. 세대 요약 문체와 선택지별 연대기 문장 확장 여부 판단
6. 후계자 특성 기반 사건 가중치 보강
7. 필요 시 기존 MVP dead code 정리 작업 분리
