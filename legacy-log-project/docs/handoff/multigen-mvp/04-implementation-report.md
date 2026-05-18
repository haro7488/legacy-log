# Multigen MVP 구현 결과 보고

> 작성일: 2026-05-18
> 작성자: Project Claude
> 기반 지시서: `docs/handoff/multigen-mvp/03-final-work-instruction.md`
> 기반 리뷰: `docs/handoff/multigen-mvp/02-claude-review.md`

## 변경 파일

| 경로 | 변경 종류 | 비고 |
|---|---|---|
| `scripts/MvpLoopContent.cs` | 수정 | 가문명/초대 인물명/도입 문장 자리표시자화. `EventChoice` 호출에 `outcomeTag` 인자 추가. `CreateInitialRunState()` 제거 후 `CreateInitialFamilyStats()`로 대체. 파일 상단 주석 갱신. |
| `scripts/MvpLoopModels.cs` | 수정 | `RunState` 책임 좁히기(가문/인물/도입/Stats 제거). `EventChoice.OutcomeTag` 추가. `RunState.ApplyChoice`가 `Dictionary<string,int>` 인자를 받도록 변경. |
| `scripts/MultigenModels.cs` | 신규 | `FamilyRunState`, `GenerationRecord`, `GenerationEndResult`, `SuccessionCandidate`, `TraitStub`, `GenerationEndType`, `StateCarryKind`. |
| `scripts/MultigenContent.cs` | 신규 | 가문/인물명 시드, 도입/요약/종료 유형/상태 서술 자리표시자, `가문력 N년` 포매터. |
| `scripts/MultigenFlow.cs` | 신규 | `CreateNewFamily`/`FinishCurrentGeneration`/`BuildGenerationRecord`/`BuildNextGenerationRun`/`AdvanceToNextGeneration`/`BuildStateBandLabel`/`BuildDetailedStateLines`/`RecordOutcomeTag`. private `PlaceholderEvaluateSuccession`와 `ApplyPlaceholderStateCarry`. 상태 거동 매핑 `StateCarryMap`. |
| `scripts/MultigenModels.cs.uid` | 신규 (자동) | Godot 임포트 결과. |
| `scripts/MultigenContent.cs.uid` | 신규 (자동) | Godot 임포트 결과. |
| `scripts/MultigenFlow.cs.uid` | 신규 (자동) | Godot 임포트 결과. |
| `scripts/Main.cs` | 수정 | `FamilyRunState` 기반 동적 UI. 시작/사건/결과/세대 요약/후계 선택/멸문 6화면. 4단계 질적 라벨 + 상세 토글. `--smoke` 경로 A. |
| `Scenes/Main.tscn` | 변경 없음 | 루트 `Control` + `res://scripts/Main.cs` 어태치 그대로. |
| `project.godot` | 변경 없음 | `run/main_scene="res://Scenes/Main.tscn"` 그대로. |
| `docs/handoff/multigen-mvp/04-implementation-report.md` | 신규 | 본 보고 문서. |

### 작업/커밋 단위 분리

지시서 §"부분 반영할 제안"에 따라 이번 세션은 커밋을 수행하지 않고 작업 단위로 변경을 분리해 보고한다.

| 단위 | 변경 파일 | 내용 |
|---|---|---|
| 1. 임시값 위생 | `MvpLoopContent.cs` | 가문명/초대 인물명/도입 문장만 자리표시자화. 사건 문안 유지. |
| 2. 단일 세대 모델 보강 | `MvpLoopModels.cs`, `MvpLoopContent.cs` | `RunState` 책임 좁히기, `EventChoice.OutcomeTag` 추가, `CreateInitialFamilyStats()` 도입. |
| 3. 다세대 모델/콘텐츠/흐름 | `MultigenModels.cs`, `MultigenContent.cs`, `MultigenFlow.cs` | 신규 3파일. |
| 4. UI와 스모크 | `Main.cs` | 다세대 화면 + 4단계 라벨 + 상세 토글 + `--smoke` 경로 A. |
| 5. 검증과 보고 | 본 문서 | 검증 명령 실행 및 결과 정리. |

## 리뷰 반영 요약

지시서 §"반영할 제안" 11항 모두 코드에 반영됐다.

- ✅ `RunState`를 리네임 없이 한 세대 진행 상태로 책임 좁힘. 가문명/인물명/도입/Stats는 `FamilyRunState` 또는 `MultigenContent`로 이동.
- ✅ 가문 단위 상태의 진실 출처는 `FamilyRunState.FamilyStats`. `RunState.ApplyChoice`는 동일 Dictionary 인스턴스를 인자로 받아 갱신.
- ✅ `EventChoice.OutcomeTag` 추가. `MvpLoopContent.Events`의 6개 선택지 모두 자리표시자 태그(`outcome-1`/`outcome-2`).
- ✅ 후계 판정은 `PlaceholderEvaluateSuccession`(private)으로 명명하고 함수 주석에 "MVP 검증용 임시 로직이며 후계 가능성 계산식은 결정 보류"를 명시.
- ✅ `GenerationEndType` 5종 정의. 자동 흐름이 발화시키는 유형은 `NaturalDeath`(1세대) / `Extinction`(2세대) 2종.
- ✅ `TraitStub`은 표시 전용. `MultigenFlow`의 어떤 로직 입력에도 사용되지 않음(코드 주석 명시).
- ✅ 상태 구간 라벨 4단계 자리표시자(`구간 1`/`구간 2`/`구간 3`/`구간 4`). 임계값은 자리표시자임을 함수 주석에 명시.
- ✅ 세대 끝 상태 서술은 `MultigenContent.BuildStateNarrative`로 만들고 `BuildSummaryParagraph`가 후대 기록자 요약 단락에 통합.
- ✅ `--smoke`는 한 실행에서 후계 정상 경로(1세대 `NaturalDeath` → 후계 선택 → 2세대 진입) + 멸문 경로(2세대 `Extinction`)를 모두 검증.
- ✅ 병렬 작업 분해는 3트랙(A: 모델/흐름, B: 콘텐츠/위생/UI, D: 스모크/보고)으로 단순화.
- ✅ 임시값 위생 작업은 같은 흐름에 포함하되 작업 단위 1로 분리. 커밋은 수행하지 않으므로 본 문서의 작업 단위 표로 변경을 구분.

## 구현된 사용자 흐름

지시서 §"완료 기준" 16개 항목 전부 충족.

1. **새 실행 시작**: `Main._Ready()`가 `MultigenFlow.CreateNewFamily()`로 `FamilyRunState`를 만든다(가문 `가문 A`, 1세대 인물 `인물 1`, 자리표시자 특성 1개, 가문 단위 Stats 0 초기화).
2. **첫 세대 사건 루프**: `ShowGenerationStart` → `ShowEvent` → `ApplyChoice` → `ShowResult` → 다음 사건 또는 세대 요약. 기존 단일 세대 사건 3개를 한 세대 실행기로 재사용.
3. **선택 피드백**: `ShowResult`에서 결과 문구 + 각 `StateDelta`(`{표시명} {±값}`) + 최신 `ChronicleEntry.RecordedLine` 표시. 상태 패널은 갱신 후 재렌더링.
4. **세대 요약**: `ShowGenerationSummary`에서 `MultigenFlow.FinishCurrentGeneration` 호출. 누적 연대기, 후대 기록자 시점 요약 단락, 지난 세대 이력(2세대 이상에서), 누적 OutcomeTag를 표시.
5. **후대 기록자 요약 단락**: `MultigenContent.BuildSummaryParagraph`가 (a) 이번 세대 의미, (b) 유산, (c) 종료 유형 자리표시자, (d) 세대 말 상태 서술, (e) 다음 세대 암시를 한 단락에 통합.
6. **자체 연호**: `MultigenContent.FormatFamilyEra(n)`이 `가문력 N년`을 반환. 헤더 라벨과 세대 요약 헤더 양쪽에서 표시.
7. **1세대 종료 후 후계 진입**: `ShowSuccession`에서 후계 후보 1명 시 "다음 세대로 진입" 버튼, 2명 이상 시 후보별 선택 버튼. `OnSuccessionPicked`가 `BuildNextGenerationRun` + `AdvanceToNextGeneration`를 호출해 다음 세대로 전환.
8. **다음 세대 이어짐**: `FamilyRunState.FamilyStats`/`History`/`CarriedOutcomeTags`가 그대로 유지되어 2세대 화면에서 이전 세대 흔적이 보인다. 스모크 출력의 `CARRIED` 줄이 이를 검증.
9. **2세대 멸문**: 2세대 종료 시 `PlaceholderEvaluateSuccession`이 후보 0명을 강제(검증용 자리표시자 분기). `ShowExtinction`에서 멸문 요약 + 가문 전체 이력 표시. `FamilyRunState.MarkExtinct()` 호출.
10. **기본 상태 표시**: `RenderStats`가 상태 키마다 `MultigenFlow.BuildStateBandLabel(key, value)`로 4단계 라벨만 표시.
11. **상세 토글**: `_detailToggleButton`을 누르면 `MultigenFlow.BuildDetailedStateLines`의 결과(숫자 + 부호 방향)가 보인다/숨겨진다.
12. **특성 스텁 표시**: `RenderTraits`가 `FamilyRunState.CurrentCharacterTraits.Label`을 한 줄로 표시. 어떤 계산에도 사용되지 않음.

## 단일 세대 모듈 유지/변경 내용

### 유지

- `LoopEvent`/`ChronicleEntry`/`StateDelta`/`StateDefinition`: 시그니처 변경 없음.
- `MvpLoopContent.Events`의 3개 사건 + 6개 선택지 문안: 유지(`OutcomeTag`만 추가).
- `MvpLoopContent.StateDefinitions`: 3개 임시 상태(`Reputation`/`Stores`/`Cohesion`).
- `Scenes/Main.tscn` + `project.godot`: 변경 없음.

### 좁힘

- `RunState`: `FamilyName`/`FounderName`/`IntroLine`/`Stats` 제거. 남은 책임은 `CurrentEventIndex`와 `Chronicle`. 가문 단위 상태는 `ApplyChoice` 인자로 받음.
- `MvpLoopContent.CreateInitialRunState()` 제거 → `CreateInitialFamilyStats()`로 대체(가문 단위 상태 Dictionary 시드만 반환).

### 추가

- `EventChoice.OutcomeTag`(string): 결정 008의 "큰 사건 결과" 자리. 각 선택지에 자리표시자 태그.

## 다세대 흐름 구현 내용

### 모델 (`MultigenModels.cs`)

지시서 §4의 7개 타입 모두 구현.

- `FamilyRunState`: 가문 단위 진실 출처. `AdvanceToNextGeneration`/`RecordCurrentGenerationEnd`/`MarkExtinct`/`AddCarriedOutcomeTag` 메서드 제공.
- `GenerationRecord`/`GenerationEndResult`/`SuccessionCandidate`/`TraitStub`: 합류 계약 시그니처 그대로.
- `GenerationEndType` 5종 / `StateCarryKind` 3종.

### 콘텐츠 (`MultigenContent.cs`)

자리표시자 시드와 문장 템플릿만.

- `PlaceholderFamilyName = "가문 A"`, `GetFirstCharacterName() => "인물 1"`.
- `GetCharacterIntroLine(name, gen)`: `[자리표시자: 도입 문장 — {name} ({gen}세대)]`.
- `BuildSuccessionCandidatesForGeneration(nextGen)`: 후보 1명(`인물 N` + 자리표시자 설명 + `trait-1`).
- `GetEndTypeDisplay`: 종료 유형별 자리표시자 문구.
- `BuildSummaryParagraph`: 후대 기록자 요약 단락 통합(의미/유산/종료/상태 서술/다음 세대 암시).
- `FormatFamilyEra(n) => "가문력 N년"`.
- `BuildStateNarrative`: 세대 말 상태 서술 자리표시자.

### 흐름 (`MultigenFlow.cs`)

지시서 §5의 6개 public 진입점 + `RecordOutcomeTag` 헬퍼 1개.

- `CreateNewFamily()`: 가문 + 1세대 인물 + 특성 + Stats 초기화.
- `FinishCurrentGeneration(family)`: 후계 판정(`PlaceholderEvaluateSuccession`) + 종료 유형 결정(1세대 `NaturalDeath` / 2세대 `Extinction`) + 후대 기록자 요약 단락 생성.
- `BuildGenerationRecord(family, endResult)`: `History` 누적용 레코드.
- `BuildNextGenerationRun(family, chosen)`: `ApplyPlaceholderStateCarry` 적용 후 새 `RunState` 반환.
- `AdvanceToNextGeneration(family, chosen, nextRun)`: `FamilyRunState`에 위임.
- `BuildStateBandLabel(key, value)`: 4단계 라벨, 임계값 자리표시자.
- `BuildDetailedStateLines(family)`: 상세 토글용 숫자/방향 줄 목록.
- private `PlaceholderEvaluateSuccession`: 입력 자리(`FamilyStats`/`CarriedOutcomeTags`)만 확보, 실제 분기 규칙은 두지 않음. 후보 시드를 `MultigenContent`에서 가져옴.
- private `ApplyPlaceholderStateCarry`: `Resource` 누적/`Reputation` 1만큼 0방향 끌어당김/`Relation` 절반화. 모든 거동은 자리표시자.

### 상태 거동 매핑

`MultigenFlow.StateCarryMap`(private static):

- `Reputation` → `StateCarryKind.Reputation`
- `Stores` → `StateCarryKind.Resource`
- `Cohesion` → `StateCarryKind.Relation`

매핑이 사실상 규칙으로 굳지 않도록 코드 주석에 "MVP 검증용 임시 매핑. 상태 키 셋과 거동 분류는 결정 008의 결정 보류 영역."을 명시.

### 후계/멸문 발화

- 1세대 종료 → `NaturalDeath` + 후보 1명. 2세대로 진입.
- 2세대 종료 → 검증용 강제 분기로 후보 0명 + `Extinction`. 멸문 화면.
- `BattleDeath`/`IllnessDeath`/`Deposed`는 enum 멤버 정의와 `GetEndTypeDisplay` 자리표시자 매핑만. 자동 흐름에서 발화시키지 않음.

### 특성 스텁

- 첫 세대 인물: `trait-1`/`특성 1` 자리표시자 1개.
- 후계 후보: `trait-1`/`특성 1` 자리표시자 1개.
- 어떤 흐름 로직(`PlaceholderEvaluateSuccession`, `ApplyPlaceholderStateCarry`, `BuildStateBandLabel`)에도 입력으로 사용되지 않음. `MultigenModels.cs` 주석에 표시 전용임을 명시.

## 임시값 위생 작업 내용

지시서 §"작업 단위 1"과 `docs/temp-value-hygiene-delegation-draft.md` 범위에 한정.

- `하서가` → `가문 A` (이전: 한국 가문 톤이 굳을 위험)
- `하서 단` → `인물 1` (이전: 특정 시대/문화권 인물명처럼 굳을 위험)
- `하서가의 단이 처음으로 가문의 첫 장을 연다.` → `[자리표시자: 도입 문장]` 시드. 다세대 흐름에서는 `MultigenContent.GetCharacterIntroLine`이 인물명/세대 번호를 끼워 `[자리표시자: 도입 문장 — 인물 1 (1세대)]` 형태로 반환.
- 사건 제목/본문/선택지/결과/연대기 문장: 유지(범위 밖). 파일 상단 주석에 이번 위생 작업의 경계를 명시.

## 실행한 확인 명령과 결과

| 명령 | 결과 |
|---|---|
| `dotnet build` | 성공. 경고 0, 오류 0. `LegacyLog.dll` 생성. |
| `./scripts/godot.sh --headless --import` | 성공. `first_scan_filesystem` + `update_scripts_classes` 6 steps 완료. 새 `.cs` 3종에 대해 `.uid` 자동 생성. |
| `./scripts/godot.sh --headless -- --smoke` | **종료 코드 0**, stdout 마지막 줄 `SMOKE_OK`. |

빌드 환경 메모: 본 세션은 PowerShell이 아닌 Bash 셸을 사용했으므로 `.\scripts\godot.cmd` 대신 동등 래퍼인 `./scripts/godot.sh`를 사용했다. 두 래퍼는 `CLAUDE.md` §"Godot 호출 규칙"에 따라 동일한 GODOT_BIN 탐색 규칙을 갖는다.

작업 단위 단위 빌드 검증은 단위 1 종료 후 1회, 단위 4 종료 후(통합) 1회 수행했다. 단위 2와 단위 3 종료 시점에는 `Main.cs`가 옛 `RunState` API를 사용하고 있어 빌드가 실패할 것이 자명하므로(`RunState`에서 `FamilyName` 등 제거) 권장 검증을 생략하고 단위 4 통합 빌드로 합쳤다.

## 스모크 stdout 발췌

`./scripts/godot.sh --headless -- --smoke` 전체 출력(엔진 헤더 라인 제외):

```
[SMOKE] FAMILY name=가문 A first_character=인물 1
[SMOKE] GEN 1 character=인물 1 era=가문력 1년
[SMOKE] INTRO [자리표시자: 도입 문장 — 인물 1 (1세대)]
[SMOKE] EVENT gen=1 idx=1 title=낯선 사절의 방문 choice=곡식 일부를 내어 우호를 표시한다. tag=outcome-1
[SMOKE] RESULT 사절은 만족하여 돌아갔다. 비축이 줄었지만 가문의 이름이 이웃에 알려졌다.
[SMOKE] DELTA 명망 +2
[SMOKE] DELTA 비축 -1
[SMOKE] CHRONICLE 첫 해, 가문은 곡식을 나누어 이웃의 이름에 우리 이름을 새겼다.
[SMOKE] EVENT gen=1 idx=2 title=마을의 다툼 choice=직접 중재해 양쪽에 양보를 요구한다. tag=outcome-1
[SMOKE] RESULT 두 가신은 마지못해 손을 잡았다. 결속은 단단해졌지만 양쪽 모두 약간의 불만을 남겼다.
[SMOKE] DELTA 결속 +2
[SMOKE] DELTA 명망 +1
[SMOKE] CHRONICLE 가문주는 우물 곁에서 두 가신의 손을 맞붙였다. 결속은 흔들리지 않았다.
[SMOKE] EVENT gen=1 idx=3 title=첫 추수의 밤 choice=잔치를 열어 가신들과 어울린다. tag=outcome-1
[SMOKE] RESULT 잔치의 불빛은 길게 이어졌다. 가신들의 얼굴은 밝았으나 곳간은 가벼워졌다.
[SMOKE] DELTA 결속 +2
[SMOKE] DELTA 비축 -2
[SMOKE] CHRONICLE 첫 추수의 밤, 가문은 불빛 아래 함께 웃었다.
[SMOKE] STATE_END gen=1 stats=[Reputation=3,Stores=-3,Cohesion=4]
[SMOKE] BAND 명망=구간 3 value=3
[SMOKE] BAND 비축=구간 2 value=-3
[SMOKE] BAND 결속=구간 3 value=4
[SMOKE] GEN_END type=NaturalDeath is_current_character_gone=True is_extinct=False
[SMOKE] SUMMARY [자리표시자: 1세대 인물 1의 시대 의미] [자리표시자: 1세대가 남긴 유산] [자리표시자: 자연사 요약] [자리표시자: 세대 말 상태 서술 — 명망 구간 3, 비축 구간 2, 결속 구간 3] [자리표시자: 다음 세대가 직면할 상황 암시]
[SMOKE] SUCCESSION candidates=1
[SMOKE] CANDIDATE idx=0 name=인물 2 traits=1
[SMOKE] SUCCESSION_PICK name=인물 2
[SMOKE] GEN 2 character=인물 2
[SMOKE] CARRIED stats_before=[Reputation=3,Stores=-3,Cohesion=4] stats_after_carry=[Reputation=2,Stores=-3,Cohesion=2] outcome_tags=outcome-1,outcome-1,outcome-1 chronicle_history_count=1
[SMOKE] GEN 2 character=인물 2 era=가문력 2년
[SMOKE] INTRO [자리표시자: 도입 문장 — 인물 2 (2세대)]
... (2세대 사건 3개 진행, 1세대와 동일 형식) ...
[SMOKE] STATE_END gen=2 stats=[Reputation=5,Stores=-6,Cohesion=6]
[SMOKE] BAND 명망=구간 4 value=5
[SMOKE] BAND 비축=구간 1 value=-6
[SMOKE] BAND 결속=구간 4 value=6
[SMOKE] GEN_END type=Extinction is_current_character_gone=True is_extinct=True
[SMOKE] SUMMARY [자리표시자: 2세대 인물 2의 시대 의미] [자리표시자: 2세대가 남긴 유산] [자리표시자: 멸문 요약] [자리표시자: 세대 말 상태 서술 — 명망 구간 4, 비축 구간 1, 결속 구간 4] [자리표시자: 가문이 더는 이어지지 않는다]
[SMOKE] SUCCESSION candidates=0
[SMOKE] EXTINCT family=가문 A total_generations=2
SMOKE_OK
```

지시서 §"작업 단위 4 스모크 stdout" 필수 줄 11종(`FAMILY`/`GEN 1`/`EVENT`/`DELTA`/`CHRONICLE`/`GEN_END NaturalDeath`/`SUCCESSION candidates`/`SUCCESSION_PICK`/`GEN 2`/`CARRIED`/`GEN_END Extinction`/`EXTINCT`/`SMOKE_OK`) 모두 출력 확인.

세대 간 이어짐 검증 포인트(`CARRIED` 줄):

- `stats_before=[Reputation=3,Stores=-3,Cohesion=4]`: 1세대 종료 시 가문 단위 상태.
- `stats_after_carry=[Reputation=2,Stores=-3,Cohesion=2]`: 상태 거동 매핑 적용 후 — `Reputation`은 1만큼 0방향 끌어당김(3→2), `Stores`는 누적(변경 없음), `Cohesion`은 절반화(4→2). 결정 008의 재산형/평판형/관계형 거동이 자리표시자 수준에서 발화한다.
- `outcome_tags=outcome-1,outcome-1,outcome-1`: 1세대 3개 선택지의 `OutcomeTag`가 가문 단위 누적에 반영됨.
- `chronicle_history_count=1`: 1세대 `GenerationRecord`가 `History`에 추가됨.

## 수동 GUI 확인 결과

수동 GUI 확인은 **이번 에이전트 세션에서 수행하지 않음**.

근거는 `CLAUDE.md` §"이 에이전트 컨텍스트에서 Godot GUI 실행을 신뢰 도구로 쓰지 말 것" 항목이다. 이 컨텍스트에는 디스플레이 인터랙션 루프가 없어 사람이 클릭으로 9단계+ 흐름을 진행할 수 없고, `timeout`/외부 신호로 강제 종료하면 `Fatal error. Internal CLR error.` 류의 비정상 종료 산물이 stdout으로 새서 결과 해석을 흐릴 수 있다.

대신 자동 스모크가 다음 단계를 화면 코드 경로와 동일한 시퀀스로 검증했다.

- 1세대 시작 → 사건 3개 → 결과 표시 → 연대기 누적
- 세대 요약(후대 기록자 요약 단락, `가문력 N년` 표시 포함)
- 후계 후보 표시 + 후계 선택
- 2세대 진입 + 이전 세대 상태/태그/이력 이어짐
- 2세대 종료 → 멸문 화면

GUI 한정 검증 항목(예: 상세 토글 영역의 표시/숨김 인터랙션, 화면 전환 시 레이아웃 깨짐 여부)은 사람 검증자에게 남긴다. 실행 명령:

```powershell
.\scripts\godot.cmd
```

## 남은 제약 또는 미구현 범위

지시서 §"제외 범위" 그대로 미구현이며 본 작업의 책임 범위 밖.

### 결정 보류 영역(코드/주석으로 명시)

- 상태 키 셋(`Reputation`/`Stores`/`Cohesion`)과 거동 매핑(`StateCarryMap`)은 검증용 임시 매핑. `MultigenFlow.StateCarryMap` 주석 명시.
- 4단계 구간 라벨 임계값(`-5`/`0`/`4`)은 검증용 자리표시자. `BuildStateBandLabel` 주석 명시.
- `PlaceholderEvaluateSuccession`은 입력 자리만 확보. 후계 가능성 계산식 미확정. 함수 주석 명시.
- `ApplyPlaceholderStateCarry`는 자리표시자 거동(1만큼 끌어당김/절반화). 감쇠율·재구성 강도 미확정. 함수 주석 명시.
- `GenerationEndType.BattleDeath`/`IllnessDeath`/`Deposed`는 자리만 확보. 자동 흐름에서 발화시키지 않음.
- `TraitStub`은 표시 전용. 효과/상속 확률 미정의. 어떤 흐름 입력에도 사용 안 함.

### 콘텐츠 결정 보류 영역(자리표시자로 명시)

- 가문명(`가문 A`)/인물명(`인물 1`/`인물 2`)
- 도입/요약/유산/암시/상태 서술 문장: `[자리표시자: ...]` 형식
- 종료 유형 표시 문장: `[자리표시자: 자연사 요약]` 등
- 특성 키/라벨: `trait-1`/`특성 1`
- 자체 연호: `가문력 N년`(결정 009의 예시 그대로)
- 사건 문안(3개 사건, 6개 선택지): 단일 세대 검증 콘텐츠 그대로 유지(이번 범위 밖)

### 흐름 측면 미구현

- 후계 후보 2명 이상 분기는 UI 코드(`ShowSuccession`)에 두지만 시드(`BuildSuccessionCandidatesForGeneration`)는 1명만 반환한다. 합류 계약상 2명 이상이면 선택 화면이 발화하지만 현재 시드로는 발화하지 않는다.
- 3세대 이상 진행은 코드 경로는 가능하나 검증용 자리표시자 분기로 2세대에서 무조건 멸문. 실제 멸문 조건(후계 부재 + 사망/퇴장 동시 충족)을 다양화하려면 후계 가능성 계산식이 결정되어야 함.
- 큰 사건 결과 태그 누적이 현재는 매 선택지에서 발생한다. 진짜 "큰 사건"만 누적되려면 사건/선택지에 `IsMajor` 같은 추가 표식이 필요하지만, 이번 범위는 OutcomeTag 자리 확보까지로 제한.

### 환경 측면 메모

- `Scenes/Main.tscn`/`project.godot`은 합류 계약대로 무변경. `ext_resource path="res://scripts/Main.cs"` 유지. 새 `.cs` 3종은 씬에 직접 어태치하지 않음(케이스 mismatch 함정 회피).
- `scripts/` 디렉터리는 소문자 그대로. 새 파일은 모두 같은 디렉터리에 추가. Godot은 디스크 케이스를 따른다.

## 다음 작업 후보

Project Codex가 이어받을 때 다음 항목을 우선 검토할 수 있다.

1. **후계 후보 2명 이상 시드** — `MultigenContent.BuildSuccessionCandidatesForGeneration`을 검증용으로 일부 세대에 2명을 반환하게 두면 선택 UI 분기가 자동 검증된다. 단 결정 보류 영역(후계 가능성 계산식) 침범 위험이 있어 분기 트리거를 결정 보류 영역 외 입력(예: 단순 세대 번호 모듈로)으로 두어야 한다.
2. **큰 사건 표식 보강** — `LoopEvent.IsMajor` 또는 `EventChoice.IsMajor` 추가로 결정 009의 정보량 차등 자리를 마련. 본 단계에서는 `OutcomeTag`만으로 자리를 확보했지만 결정 009의 정보량 차등은 별도 표식이 더 자연스럽다.
3. **상태 거동 매핑 위치 재검토** — `StateCarryMap`이 `MultigenFlow` 안에 있는데, 본 리뷰의 권장이었지만 합류 계약 §6에서도 "MultigenFlow에 둔다"를 못 박았으므로 현재 위치 유지. 향후 상태 키 셋이 더 늘어나면 별도 정적 클래스 분리 가능.
4. **연대기 시점 분리의 콘텐츠 적용** — 현재 1세대/2세대 사건 본문은 같은 한국어 문안을 재사용하므로 1인칭 시점이 명시적으로 보이지 않는다. 결정 009의 시점 분리(진행 중 1인칭 + 세대 끝 후대 기록자)는 후속 콘텐츠 작성 단계에서 사건 문안 자체를 손볼 때 자연스럽게 적용된다.
5. **시간 표시 위치 확장** — 현재 `가문력 N년`은 헤더와 세대 요약 헤더 2곳에서만 표시. 진행 중 1인칭 기록에 시간 표시를 넣을지는 결정 009의 결정 보류 영역.
6. **사건 문안의 다세대화** — 같은 사건이 1세대와 2세대에 모두 나오는 현 구조는 검증용. 세대별 사건 풀 또는 사건 가중치 시스템은 PRD/결정 008의 "대량 사건 콘텐츠" 제외 범위.
7. **수동 GUI 확인 1회** — 본 세션에서 수행하지 못한 수동 GUI 확인을 사람 검증자가 1회 수행. 특히 상세 토글 인터랙션, 후계 후보가 1명일 때 버튼 라벨(`인물 N로 다음 세대로 진입`), 멸문 화면의 가문 전체 이력 표시가 화면에서 의도대로 보이는지 확인.
