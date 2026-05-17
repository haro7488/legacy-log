# MVP First Loop 구현 결과 보고

> 작성일: 2026-05-18
> 작성자: Project Claude
> 기반 지시서: `docs/handoff/mvp-first-loop/03-final-work-instruction.md`
> 기반 리뷰: `docs/handoff/mvp-first-loop/02-claude-review.md`

## 변경 파일

| 경로 | 변경 종류 | 비고 |
|---|---|---|
| `scripts/MvpLoopModels.cs` | 신규 | 합류 계약의 모델 6종 + `RunState.ApplyChoice` |
| `scripts/MvpLoopContent.cs` | 신규 | StateDefinitions 3종, 가문/인물 시드, 사건 3개. 상단에 임시 콘텐츠 주석 |
| `scripts/Main.cs` | 신규 | `Control` 루트, 동적 UI, `--smoke` 분기 |
| `scripts/MvpLoopModels.cs.uid` | 신규 | Godot C# 스크립트 UID |
| `scripts/MvpLoopContent.cs.uid` | 신규 | Godot C# 스크립트 UID |
| `scripts/Main.cs.uid` | 신규 | Godot C# 스크립트 UID |
| `Scenes/Main.tscn` | 신규 | 루트 `Control` + `scripts/Main.cs` 어태치 |
| `project.godot` | 수정 | `[application] run/main_scene="res://Scenes/Main.tscn"` 추가 |
| `docs/handoff/mvp-first-loop/04-implementation-report.md` | 신규 | 본 보고 문서 |

### 디스크 케이스 메모

초기 리뷰 요청 문서는 `Scripts/` 표기를 썼지만, 리포 루트의 디렉터리가 이미 `scripts/`(소문자)로 존재하고 있었다(`scripts/godot.cmd`, `scripts/godot.sh` 래퍼가 거기 있었기 때문). Windows 파일시스템은 대소문자를 무시하므로 같은 디렉터리에 신규 .cs 3종이 추가됐다. Godot은 디스크 케이스(`scripts/`)를 따라 `[ScriptPath]`를 생성하므로 `Scenes/Main.tscn`의 외부 리소스 경로도 `res://scripts/Main.cs`로 맞췄다. 게임플레이 구조가 아니라 도구 디렉터리만 사전 존재해 멈춤 지점은 발동하지 않았다.

## 구현된 사용자 흐름

지시서 「완료 기준」 9개 항목 전부 충족.

1. **새 실행 시작**: `Main._Ready()`가 `MvpLoopContent.CreateInitialRunState()`로 초기 상태 생성.
2. **시작 정보 표시**: 헤더 라벨(`{FamilyName} — {FounderName}`) + 시작 화면 도입 문장. "첫 사건으로 들어간다" 버튼.
3. **사건 3개 순서 진행**: `RunState.CurrentEventIndex`가 0→1→2→3 (==Events.Count)로 진행되며 각 단계마다 `ShowEvent`/`ShowResult` 화면이 갱신.
4. **선택지 2개 이상**: 각 사건 모두 2개의 `EventChoice`를 가짐.
5. **결과 문구 표시**: `ShowResult`에서 `choice.ResultText`를 라벨로 표시.
6. **최소 상태 변화 표시**: `ShowResult`에서 각 `StateDelta`를 `{표시명} {±값}` 라벨로 표시. 동시에 상단 상태 패널이 새 값으로 재렌더링됨.
7. **연대기 누적**: `RunState.ApplyChoice`가 `ChronicleEntry`를 누적. `ShowResult`에서 가장 최근 항목을 보여주고, 세대 요약 화면에서 전체를 나열.
8. **3개 사건 후 세대 요약**: `_state.CurrentEventIndex == Events.Count` 시점에 "세대 요약 보기" 버튼이 `ShowSummary`로 진입.
9. **세대 요약 구성**: 누적 연대기, 최종 상태 요약(`{표시명}: {값}`), 짧은 마무리 문장(`"{FounderName}의 첫 장은 이렇게 닫힌다. 다음 세대가 이 기록 위에 새 줄을 쓸 것이다."`).

### 합류 계약 적용 결과

- `StateDefinition`/`StateDelta`/`EventChoice`/`LoopEvent`/`ChronicleEntry`/`RunState` 6종을 `scripts/MvpLoopModels.cs`에 구현. `StateDelta`는 `Key`+`Amount`만 가지며 표시명은 `MvpLoopContent.GetStateLabel(key)`로 조회.
- `RunState.ApplyChoice(LoopEvent, EventChoice)`가 (a) `Stats` 누적, (b) `ChronicleEntry` 추가, (c) `CurrentEventIndex` 증가를 한 메서드에서 처리.
- `MvpLoopContent.StateDefinitions`, `MvpLoopContent.Events`, `MvpLoopContent.CreateInitialRunState()`만 외부 진입점으로 노출. `Main.cs`는 이 세 곳과 `GetStateLabel`만 호출.
- 임시 콘텐츠 주석은 `MvpLoopContent.cs` 최상단에 한 블록으로 남김(결정 004·005·006 참조 포함).
- UI는 코드 동적 생성. `Scenes/Main.tscn`은 자식 노드 없이 루트 `Control` + 스크립트만.

## 실행한 확인 명령과 결과

| 명령 | 결과 |
|---|---|
| `dotnet build` | 성공. 경고 0, 오류 0. (`LegacyLog.dll` 생성) |
| `.\scripts\godot.cmd --headless --import` | 성공. `first_scan_filesystem` 및 `update_scripts_classes` 완료. |
| `.\scripts\godot.cmd --headless -- --smoke` | **종료 코드 0**, stdout 마지막 줄 `SMOKE_OK`. |

## 스모크 stdout 발췌

전체 출력(헤더 라인 제외):

```
[SMOKE] START family=하서가 founder=하서 단
[SMOKE] INTRO 하서가의 단이 처음으로 가문의 첫 장을 연다.
[SMOKE] EVENT 1 title=낯선 사절의 방문 choice=곡식 일부를 내어 우호를 표시한다.
[SMOKE] RESULT 사절은 만족하여 돌아갔다. 비축이 줄었지만 가문의 이름이 이웃에 알려졌다.
[SMOKE] DELTA 명망 +2
[SMOKE] DELTA 비축 -1
[SMOKE] CHRONICLE 첫 해, 가문은 곡식을 나누어 이웃의 이름에 우리 이름을 새겼다.
[SMOKE] EVENT 2 title=마을의 다툼 choice=직접 중재해 양쪽에 양보를 요구한다.
[SMOKE] RESULT 두 가신은 마지못해 손을 잡았다. 결속은 단단해졌지만 양쪽 모두 약간의 불만을 남겼다.
[SMOKE] DELTA 결속 +2
[SMOKE] DELTA 명망 +1
[SMOKE] CHRONICLE 가문주는 우물 곁에서 두 가신의 손을 맞붙였다. 결속은 흔들리지 않았다.
[SMOKE] EVENT 3 title=첫 추수의 밤 choice=잔치를 열어 가신들과 어울린다.
[SMOKE] RESULT 잔치의 불빛은 길게 이어졌다. 가신들의 얼굴은 밝았으나 곳간은 가벼워졌다.
[SMOKE] DELTA 결속 +2
[SMOKE] DELTA 비축 -2
[SMOKE] CHRONICLE 첫 추수의 밤, 가문은 불빛 아래 함께 웃었다.
[SMOKE] SUMMARY family=하서가
[SMOKE] FINAL 명망=3
[SMOKE] FINAL 비축=-3
[SMOKE] FINAL 결속=4
SMOKE_OK
```

## 수동 GUI 확인 결과

**완료.** 사용자 수동 확인 결과 정상 작동.

- `.\scripts\godot.cmd`로 GUI 실행 후 시작 화면, 사건 1~3, 선택 결과, 상태 변화, 연대기 누적, 세대 요약 흐름을 확인했다.
- Codex 자동 재확인에서도 `dotnet build`, `.\scripts\godot.cmd --headless --import`, `.\scripts\godot.cmd --headless -- --smoke`가 성공했고, 스모크 마지막 줄은 `SMOKE_OK`였다.

## 남은 제약 또는 미구현 범위

지시서 「제외 범위」 그대로 미구현이며 본 작업의 책임 범위 밖.

- 완성형 가문 생성기 / 첫 가문·인물 이름 최종 확정 / 사건 문안 최종 확정 / 후계·혼인·혈통 / 랜덤 사건 / 저장·불러오기 / 밸런스 수치 / 완성형 UI·아트 스타일 / CI·배포 / 지시서에 없는 리팩터링.
- `MvpLoopContent.cs`의 상태 키(`Reputation`/`Stores`/`Cohesion`), 표시명(`명망`/`비축`/`결속`), 가문명(`하서가`), 초대 인물명(`하서 단`), 도입 문장, 3개 사건의 모든 문안은 모두 MVP 검증용 임시 값. 결정 004·005·006의 의도적 미확정 영역. 파일 상단에 그 사실을 명시.
- `RunSmoke` 분기는 임시 검증 코드(코드 주석 명시). 통합 테스트 체계 도입 시 제거 대상.

## 컨벤션 반영 메모

본 구현 중 발견한 Godot C# 경로 케이스 문제는 재발 방지를 위해 지침에 반영했다.

- `CLAUDE.md`: `.tscn`의 `ext_resource path`와 C# `[ScriptPath]`가 디스크 케이스와 일치해야 한다는 Godot 예외 점검 규칙을 추가했다.
- `AGENTS.md`: Project Codex가 구현 계획과 최종 작업 지시서를 작성할 때 Godot 경로 케이스 계약을 명시하도록 추가했다.
- 현재 구현은 기존 디렉터리 케이스를 따라 C# 스크립트를 `scripts/`에 두고, 씬 참조도 `res://scripts/Main.cs`로 맞춘다.
