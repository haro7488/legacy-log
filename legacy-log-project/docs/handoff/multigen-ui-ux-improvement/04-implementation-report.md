# Multigen MVP UI/UX Improvement Implementation Report

> 작성일: 2026-05-19
> 작성자: Project Claude
> 모드: 구현 보고
> 기반 지시서: `docs/handoff/multigen-ui-ux-improvement/03-final-work-instruction.md`

## 1. 변경 파일

- `scripts/Main.cs` — 4영역 UI shell 재구성 및 화면/탭 렌더링 전면 리팩터링.
- `docs/handoff/multigen-ui-ux-improvement/04-implementation-report.md` — 본 보고서(신규).

지시서 수정 범위 중 다음 두 파일은 의도적으로 **변경하지 않았다**.

- `Scenes/Main.tscn` — 루트 `Control` + `res://scripts/Main.cs` 어태치 구조가 그대로 충분하다. 새 정적 자식 노드를 만들지 않는다는 합류 계약을 유지.
- `project.godot` — 사용자 승인 기본안에 따라 `[display]` 섹션을 추가하지 않는다. 수동 검증자가 창을 직접 조정한다.

## 2. 리뷰 반영 요약

`02-claude-review.md`에서 합의된 합류 계약과 사용자 승인 기본안을 다음과 같이 반영했다.

| 항목 | 반영 결과 |
|---|---|
| 4영역 구조 (`ContextHeader`/`MainScroll`/`InfoTabs`/`ActionBar`) | `BuildShell()` → 4개 영역 빌더 메서드로 분리. 루트 `Control`만 `FullRect`, 내부는 `MarginContainer` + `VBoxContainer AppRoot`. |
| `MainScroll`만 주요 확장 영역 | `MainScroll`은 `ExpandFill`, `ContextHeader`/`InfoTabs`/`ActionBar`는 각각 `ShrinkBegin`/`ShrinkBegin`/`ShrinkEnd`. |
| 탭 3개로 축소 (`상태`/`연대기`/`가문사`) | `BuildInfoTabs()` → 3개 탭만 생성. `유산` 탭은 만들지 않음. |
| `유산` 정보는 `가문사` 탭 상단에 흡수 | `RenderFamilyHistoryTab()` 상단에 `누적 큰 사건 결과 태그` 라인 + 자리표시자(없을 때). |
| 상세 숫자/방향은 `상태` 탭 안 토글로 유지 | `RenderStateTab()`에서 토글 버튼 + `_stateDetailVisible` 플래그로 sub-section 분리. |
| 탭 갱신 단일 진입점 | `RenderInfoTabs()` 하나만 외부에서 호출. 내부에서 3개 탭 렌더러를 부른다. |
| ActionBar 갱신 단일 진입점 | 모든 화면이 `SetActionBar(IEnumerable<Control>)` 한 번만 호출. |
| 사건 선택지는 `ActionBar`에 배치 | `ShowEvent()`가 본문(제목/Body)을 `MainContent`에, 선택지 2개를 `ActionBar`에 둔다. |
| 멸문 화면 진행 버튼 없음 | `ShowExtinction()`이 비활성 안내 라벨(`Modulate alpha=0.6`)만 `ActionBar`에 둔다. |
| 화면 전환 시 본문 스크롤 최상단 | `ResetMainScroll()` 호출(모든 `ShowXxx` 말미). |
| 탭 사용자 선택 유지 | `InfoTabs.CurrentTab`을 화면 전환 시 강제하지 않는다. |
| `ContextHeader` 2줄 구조 | 1행 = 가문/세대/인물, 2행 = `[단계: ...]` + `[자리표시자: 화면 목적]`. 작은 폭 대응. |
| 자리표시자/구간 라벨 유지 | `구간 1~4`, `[자리표시자: ...]`, `[단계: ...]` 형식 그대로 유지. |
| `--smoke` UI 노드 미생성 + stdout 형식 유지 | `RunSmoke()`/`RunSmokeGeneration()`/`PrintSmokeGenerationEnd()` 변경 없음. |

반영하지 않은 항목(승인된 제외 사항):

- `project.godot` `[display]` 섹션 추가.
- `--ui-smoke` 별도 경로.
- `유산` 탭 별도 분리.

## 3. 구현된 UI 구조

`BuildShell()`이 다음 노드 트리를 코드로 구성한다. `Scenes/Main.tscn`에는 정적 자식이 없다.

```text
Control (Main, FullRect)
└─ MarginContainer (margin_*=12)
   └─ VBoxContainer AppRoot (separation=8)
      ├─ VBoxContainer ContextHeader   [ShrinkBegin]
      │  ├─ Label  (가문 · 가문력 · 인물)
      │  └─ Label  ([단계: ...] [자리표시자: 화면 목적 — ...])
      ├─ HSeparator
      ├─ ScrollContainer MainScroll    [ExpandFill]
      │  └─ VBoxContainer MainContent  (separation=6)
      ├─ TabContainer InfoTabs         [ShrinkBegin, CustomMinimumSize.Y=180]
      │  ├─ ScrollContainer StateTab           → "상태"
      │  │  └─ VBoxContainer _stateTabContent
      │  ├─ ScrollContainer ChronicleTab       → "연대기"
      │  │  └─ VBoxContainer _chronicleTabContent
      │  └─ ScrollContainer FamilyHistoryTab   → "가문사"
      │     └─ VBoxContainer _familyHistoryTabContent
      ├─ HSeparator
      └─ VBoxContainer ActionBar       [ShrinkEnd, separation=6]
```

### 합류 계약 충족 포인트

- 루트 `Control`만 `FullRect`를 직접 잡는다. 내부 컨테이너는 부모 컨테이너 레이아웃을 따른다(중첩 anchors 제거).
- `MainScroll`만 `ExpandFill`. 본문이 영역을 차지하고 `ContextHeader`/`InfoTabs`/`ActionBar`는 자기 최소 높이만 차지한다.
- `InfoTabs`는 `CustomMinimumSize.Y = 180`을 박았다. 각 탭 내부에도 `ScrollContainer`를 두어 탭 내용이 길어져도 `ActionBar`를 밀어내지 않는다.
- 탭 노드 이름(영문)과 표시 라벨(한글)을 `TabContainer.SetTabTitle()`로 분리.

## 4. 화면별 변경 내용

모든 `ShowXxx`는 다음 순서를 명확히 호출한다:

```text
RefreshContextHeader(step, purpose)
ClearMainContent()
(MainContent 채우기)
SetActionBar(buttons)
RenderInfoTabs()
ResetMainScroll()
```

| 화면 | 단계 라벨 | 목적 자리표시자 | `MainContent` | `ActionBar` |
|---|---|---|---|---|
| `ShowGenerationStart` | `세대 시작` | `[자리표시자: 화면 목적 — 새 세대를 시작합니다]` | 인물 도입 문장 | `첫 사건으로 들어간다` |
| `ShowEvent` | `사건 진행` | `[자리표시자: 화면 목적 — 사건의 선택을 합니다]` | 사건 인덱스/제목/본문 | 선택지 2개 |
| `ShowResult` | `결과 확인` | `[자리표시자: 화면 목적 — 선택의 결과를 확인합니다]` | 결과/델타/연대기 | `다음 사건으로` 또는 `세대 요약 보기` |
| `ShowGenerationSummary` | `세대 요약` | `[자리표시자: 화면 목적 — 세대를 마감합니다]` | 세대 헤더/누적 연대기/후대 요약 | `후계를 본다` 또는 `가문의 끝을 본다` |
| `ShowSuccession` | `후계 선택` | `[자리표시자: 화면 목적 — 다음 세대로 이어갈 후계를 정합니다]` | 후계 후보 이름/설명/특성 | 후보별 선택 버튼 |
| `ShowExtinction` | `멸문` | `[자리표시자: 화면 목적 — 가문이 끝났습니다]` | 멸문 헤더/요약/전체 이력 | 비활성 안내 라벨만 |

### 행동 영역 계약 충족

- 사건 선택지가 `ActionBar`에 들어가, 본문 길이와 무관하게 항상 보인다(현재 모든 사건 2지선다).
- 후계 후보 선택 버튼도 `ActionBar`에 들어간다. 현재 시드는 후보 1명이므로 단일 버튼.
- 멸문 화면 ActionBar에 진행 버튼이 없다. 비활성 안내 라벨(Modulate alpha=0.6)만 둔다.

### `_pendingEnd` 및 `History.Add` 시점

`History.Add`는 기존과 동일하게 `AfterSummary()`에서만 호출된다. 즉 `ShowGenerationSummary` 시점의 `가문사` 탭은 아직 현재 세대를 포함하지 않는다(의도된 동작). 이는 합류 계약 §"화면 전환 계약"을 그대로 따른다.

## 5. 탭 구조와 정보 배치

### `상태` 탭 (`RenderStateTab`)

- 헤더: `현재 상태 (질적 표시)`
- `MvpLoopContent.StateDefinitions` × `MultigenFlow.BuildStateBandLabel()` 결과를 행마다 `{Label}: {band}` 형식으로 렌더.
- 토글 버튼: `상세 정보 보기` ↔ `상세 정보 숨기기`. `_stateDetailVisible` 플래그를 뒤집고 `RenderStateTab()`만 다시 호출.
- 토글 ON 시 `[자리표시자: 추가 정보창]` 헤더 + `MultigenFlow.BuildDetailedStateLines()` 한 줄씩.
- `HSeparator` 후 `현재 인물 특성` 섹션. 특성 없으면 `[자리표시자: 특성 없음]`, 있으면 콤마 결합.
- **토글 상태는 화면 전환 후에도 보존된다.**

### `연대기` 탭 (`RenderChronicleTab`)

- 헤더: `가문력 N년 연대기`.
- 현재 세대 `RunState.Chronicle`을 시간순으로 나열.
- 최신 기록 한 줄에 `▶` 프리픽스, 이전 기록은 `·`. (지시서의 "최신 선택 기록"을 시각적으로 강조)
- 비어 있으면 `[자리표시자: 아직 기록된 사건이 없습니다]`.

### `가문사` 탭 (`RenderFamilyHistoryTab`)

- 헤더: `가문사`.
- **유산 요약(상단 흡수)**: `유산 요약` 헤더 + 누적 `CarriedOutcomeTags` 라인. 비어 있으면 `[자리표시자: 아직 이어진 유산이 없습니다]`.
- `HSeparator`.
- `_family.IsExtinct ? "가문 전체 이력" : "지난 세대 이력"` 헤더 + `_family.History` 항목 나열.
- `History`가 비어 있으면 `[자리표시자: 아직 끝난 세대가 없습니다]`.
- 멸문 시 `IsExtinct=true`로 헤더가 자동 전환된다.

## 6. 작은 창/세로형 대응 내용

- `MarginContainer` 사방 12px 여백 → 작은 화면에서 텍스트/버튼이 가장자리에 닿지 않는다.
- 본문/사건/요약/후계/멸문 화면의 긴 라벨에 `AutowrapMode = Word` 적용.
- `MainScroll`이 본문 영역의 단일 스크롤 진입점.
- 각 탭 내부에 `ScrollContainer`를 두어 탭 내용도 자기 영역 안에서만 스크롤된다 → 탭 내용이 길어도 `ActionBar`를 화면 밖으로 밀지 않는다.
- `InfoTabs.CustomMinimumSize.Y = 180`으로 탭 영역 하한 고정.
- 화면 전환 시 `ResetMainScroll()`로 `MainScroll`의 vertical/horizontal scroll을 0으로 리셋.
- `ContextHeader`를 2줄(`VBoxContainer`)로 분할 → 작은 폭에서 헤더가 한 줄에 억지로 들어가지 않는다.
- 사건 선택지/후계 선택 버튼은 `ActionBar` 안에서 `VBoxContainer` 세로 목록.

## 7. 기능 보존 검증 결과

- `MultigenFlow` 및 `MultigenContent` 시그니처와 호출 패턴 변경 없음.
- `History.Add`는 여전히 `AfterSummary` 시점.
- `_family.IsExtinct`는 `ShowExtinction()` 진입 직후 `MarkExtinct()`로 켜진다(기존과 동일).
- `RunSmoke()` 경로 — UI 노드를 만들지 않고 `MultigenFlow` public API만 호출. stdout 라인 100% 동일.
- 사건 → 선택 적용 → 결과 → 다음 사건 → 세대 요약 → 후계 → 다음 세대 진입 → 멸문 흐름의 호출 순서와 데이터 변화는 그대로.

## 8. 자동 검증 결과 (기능 회귀)

지시서 검증 명령 3종을 순서대로 실행했다.

| 명령 | 실행 환경 | 결과 |
|---|---|---|
| `dotnet build` | Bash | **성공** — 경고 0, 오류 0, ~2.4초 |
| `./scripts/godot.sh --headless --import` | Git Bash | **성공** — 5단계 진행 후 `DONE` |
| `./scripts/godot.sh --headless -- --smoke` | Git Bash | **성공** — `SMOKE_OK` 출력, 종료 코드 `0` |

> 셸 경로 메모: 본 세션의 Bash 도구는 POSIX 셸을 거치므로 `.\scripts\godot.cmd`(CRLF cmd 래퍼)는 경로 파싱에 실패했다. 동등 래퍼인 `scripts/godot.sh`(POSIX)로 전환해 동일 결과를 얻었다. PowerShell 셸에서는 `scripts\godot.cmd`가 정상 동작한다(CLAUDE.md "Godot 호출 규칙" 참조).

스모크 출력 핵심 라인 발췌:

```
[SMOKE] GEN_END type=NaturalDeath is_current_character_gone=True is_extinct=False
[SMOKE] SUCCESSION_PICK name=인물 2
[SMOKE] GEN 2 character=인물 2
[SMOKE] CARRIED stats_before=[Reputation=3,Stores=-3,Cohesion=4] stats_after_carry=[Reputation=2,Stores=-3,Cohesion=2]
[SMOKE] GEN_END type=Extinction is_current_character_gone=True is_extinct=True
[SMOKE] EXTINCT family=가문 A total_generations=2
SMOKE_OK
```

> **`--smoke`의 검증 한계 (보고서 명시 사항)**: 스모크 통과는 `MultigenFlow` 흐름 회귀가 없다는 뜻일 뿐이다. UI 노드를 만들지 않으므로 본 작업의 UI shell 재구성, 탭 갱신 시점, ActionBar 갱신, 멸문 화면 진행 버튼 부재, 스크롤 리셋 동작 등 **UI 회귀는 자동 검증으로 잡히지 않는다.** UI 회귀는 §10 수동 체크리스트로만 보고한다.

## 9. 수동 UI/UX 검증 결과 또는 미수행 사유

### 미수행 사유

본 세션은 헤드리스 자동화 컨텍스트로, Godot GUI를 사용자 인터랙션 루프 없이 안정적으로 실행할 수 없다. CLAUDE.md "이 에이전트 컨텍스트에서 Godot GUI 실행(`godot.cmd` 단독)을 신뢰 도구로 쓰지 말 것" 정책을 따라 Project Claude는 **수동 UI/UX 검증을 수행하지 않았다.** 9단계 사용자 흐름 같은 기능 검증은 `--smoke`로 처리했고, 시각적 UI/UX 검증은 사람 검증자에게 넘긴다.

### 사용자가 수행할 체크리스트 → §10

`docs/manual-ui-ux-validation-guide.md`에 기존 가이드가 있다면 함께 참고. 본 보고서 §10에서 지시서 "UI/UX 수동 검증 기준"의 항목을 3종 창 크기별로 분리해 두었다.

## 10. 3개 창 크기별 수동 검증 체크리스트

검증 절차:

1. `./scripts/godot.sh -e` 또는 `.\scripts\godot.cmd -e`로 에디터를 열고 F5(`Main.tscn`)로 실행.
2. 실행 창을 아래 3종 크기로 조정해 1회씩 시작 → 멸문까지 진행한다.
3. 각 크기에서 아래 공통 체크리스트와 크기별 추가 체크 항목을 확인한다.
4. 1회 플레이 분량은 약 30~60초.

### 10-A. 공통 체크리스트 (모든 창 크기에서 확인)

#### ContextHeader (항상 보임)

- [ ] `가문 A · 가문력 N년 · 인물 N`이 1행에 보인다.
- [ ] `[단계: ...]`와 `[자리표시자: 화면 목적 — ...]`이 2행에 보인다.
- [ ] 화면 전환 시 단계 라벨/목적 문장이 즉시 갱신된다.

#### 화면별 주요 행동 식별

- [ ] 시작 화면: ActionBar에 `첫 사건으로 들어간다` 버튼이 보인다.
- [ ] 사건 화면: 선택지 2개 버튼이 ActionBar에 보이고 본문은 MainContent에 있다.
- [ ] 결과 화면: `다음 사건으로` 또는 `세대 요약 보기` 버튼이 ActionBar에 보인다.
- [ ] 세대 요약: `후계를 본다` 또는 `가문의 끝을 본다` 버튼이 ActionBar에 보인다.
- [ ] 후계 화면: 후보별 선택 버튼이 ActionBar에 보인다(`인물 N(으)로 다음 세대 진입`).
- [ ] 멸문 화면: ActionBar에 클릭 가능한 진행 버튼이 **없고**, 비활성 안내 라벨만 보인다.

#### 본문/선택지 시각 구분

- [ ] 사건 본문 텍스트와 선택지 버튼이 시각적으로 분리돼 있다(본문은 MainContent, 버튼은 ActionBar 영역).
- [ ] 결과 화면에서 결과 문구/상태 변화 라인/연대기 라인이 줄 단위로 구분된다.
- [ ] 세대 요약에서 `이번 세대 누적 연대기` / `후대 기록자의 요약`이 헤더로 구분된다.

#### 탭 갱신

- [ ] 선택 적용 직후 `연대기` 탭에 새 항목이 추가되어 있다.
- [ ] 선택 적용 직후 `상태` 탭의 4단계 라벨이 갱신되어 있다.
- [ ] 세대 요약 화면 시점의 `가문사` 탭에는 **아직 현재 세대가 포함되지 않는다**(`AfterSummary` 전).
- [ ] 후계 선택 후 다음 세대 진입 화면에서 `가문사` 탭에 직전 세대가 추가되어 있다.
- [ ] 멸문 화면에서 `가문사` 탭 헤더가 `가문 전체 이력`으로 바뀌고 1·2세대 모두 보인다.

#### 탭 사용자 선택 유지

- [ ] `연대기` 또는 `가문사` 탭을 연 채로 다음 화면(사건/결과/요약 등)으로 진행해도 사용자가 마지막으로 연 탭 선택이 **유지된다**(자동으로 `상태` 탭으로 리셋되지 않는다).

#### 상세 정보 토글

- [ ] `상태` 탭의 `상세 정보 보기` 버튼을 누르면 `[자리표시자: 추가 정보창]` 헤더와 숫자/방향 라인이 보인다.
- [ ] `상세 정보 숨기기`로 다시 접을 수 있다.
- [ ] 상세를 펼친 채로 다음 화면으로 진행해도 토글 상태가 유지된다.

#### 본문 스크롤 리셋

- [ ] MainScroll을 아래까지 내린 뒤 다음 버튼을 눌렀을 때, 다음 화면에서 MainScroll이 최상단부터 시작한다.

#### 멸문 종료성

- [ ] 멸문 화면에서 클릭 가능한 요소가 **탭 헤더와 상태 탭의 토글 버튼뿐**이다.
- [ ] ActionBar에 진행 버튼이 없고, 안내 라벨이 약간 흐리게(60% 알파) 표시된다.

#### 자리표시자 가시성

- [ ] `[자리표시자: ...]`, `[단계: ...]`, `구간 1`~`구간 4`가 시각적으로 자리표시자임을 알 수 있다.
- [ ] 자리표시자 문구가 최종 제품 문안처럼 오해될 위험은 없다.

#### 유산 정보 위치

- [ ] 2세대 진입 후 `가문사` 탭 상단에 `누적 큰 사건 결과 태그: outcome-1, outcome-1, outcome-1` 라인이 보인다.
- [ ] 1세대 사건 시작 직후엔 `[자리표시자: 아직 이어진 유산이 없습니다]`가 보인다.

### 10-B. `360x640` (세로형 모바일 가상 화면)

목적: 가장 좁고 짧은 화면에서 텍스트 자름/겹침/ActionBar 가림 여부를 본다.

- [ ] ContextHeader 2행이 한 줄에 억지로 압축되지 않는다.
- [ ] 사건 본문이 줄바꿈(Word wrap)으로 잘리지 않고 표시된다.
- [ ] 선택지 버튼 2개가 ActionBar 안에서 세로로 쌓이고 둘 다 누를 수 있다.
- [ ] InfoTabs 영역이 너무 커서 ActionBar를 화면 밖으로 밀어내지 않는다(`CustomMinimumSize.Y=180` 적용).
- [ ] `상태` 탭에서 상세를 펼쳐도 탭 내부 스크롤로 처리되고 ActionBar는 그대로 보인다.
- [ ] 멸문 화면의 `가문 전체 이력`이 탭 내부 스크롤로 모두 확인 가능하다.

### 10-C. `540x720` (좁은 데스크톱 창)

목적: 일반적인 작은 창 사용 시나리오. 정보 밀도와 가독성 균형을 본다.

- [ ] ContextHeader 두 행 모두 가독성 있는 폭으로 표시된다.
- [ ] 후계 화면에서 후보 이름/설명/특성이 본문에, 선택 버튼이 ActionBar에 명확히 분리돼 보인다.
- [ ] 세대 요약 화면의 후대 기록자 요약 단락이 줄바꿈으로 정상 표시된다.
- [ ] 탭 헤더 `상태` / `연대기` / `가문사`가 한 줄에 표시되고 좌우 스크롤 버튼이 나타나지 않는다.

### 10-D. `1280x720` (일반 데스크톱 가로 창)

목적: 기존 데스크톱 기본 사용 시나리오에서 빈 공간/정보 분포를 본다.

- [ ] MainScroll 영역이 시각적으로 우세하고, ContextHeader/InfoTabs/ActionBar가 적절한 비율로 배치된다.
- [ ] InfoTabs 영역이 비대해지지 않는다(`CustomMinimumSize.Y=180`이 하한 역할만 한다는 확인).
- [ ] 멸문 화면 안내 라벨이 한 줄로 표시되며 약간 흐리게(알파 60%) 보인다.
- [ ] 모든 화면에서 가장자리 여백(12px)이 일관되게 적용돼 보인다.

## 11. 남은 제약 또는 미구현 범위

- **수동 UI/UX 검증은 미수행.** 시각적 회귀(잘림/겹침/탭 헤더 폭/Modulate 가시성)는 사람 검증자가 §10 체크리스트로 확인해야 한다.
- **`project.godot` 미변경.** 사용자 승인 기본안. 사용자가 창을 직접 조정해 검증한다.
- **자동 검증 도구는 기능 회귀만 검증.** `--smoke`는 UI 노드를 만들지 않으므로 UI 회귀 무력. 사람 검증과 분리해 보고했다.
- **`Scenes/Main.tscn` 미변경.** 모든 UI 노드는 `Main.cs`가 런타임에 동적 생성. 정적 에디터 미리보기는 빈 `Control`만 보인다.
- **사건 선택지 수 ≤ 2 전제.** 현재 모든 사건이 2지선다라 ActionBar 세로 목록으로 충분하다. 선택지가 3개를 초과하는 미래 사건은 본 범위에서 처리하지 않았다.
- **상태 라벨/자리표시자 문구 미변경.** 결정 010의 보류 영역(라벨 문구 — 시대/세계관 톤)을 침범하지 않았다.
- **후계 후보 1명 시드 유지.** 시드 변경은 결정 008 보류 영역으로 본 범위 밖.

## 12. 다음 작업 후보

지시서 §"제외 범위" 및 §"멈춤 지점"에 부딪히지 않는 후속 후보를 우선순위 가늠 없이 나열한다.

1. **`docs/manual-ui-ux-validation-guide.md` 갱신** — 본 보고서 §10 체크리스트를 가이드 본문에 흡수해 단일 진입점으로 만들 가치가 있다(별도 결정 필요).
2. **`project.godot` `[display]` 추가 여부 재논의** — 사용자 검증 후 "기본 viewport가 너무 큼/작음"이 보이면 540x960, stretch `canvas_items`로 박는 결정을 한 라운드 더 받는다.
3. **선택지 4개 이상 사건 도입 시 ActionBar fallback 규칙** — 합류 계약에 "ActionBar 자식 수 상한 N, 초과 시 MainContent fallback"을 추가하는 후속 결정.
4. **`--ui-smoke` 도입 여부 재검토** — 본 범위에서는 도입하지 않았으나, UI shell이 `null` 노드 없이 빌드되는지만 확인하는 최소 헤드리스 UI 스모크는 가치 평가가 가능하다.
5. **세대 요약 화면에서 `가문사` 탭의 "이번 세대는 아직 포함되지 않음" 안내 가시화** — 사용자 혼동 방지용 자리표시자 한 줄 추가 후보(결정 필요).
6. **탭 사용자 선택 유지가 화면 흐름과 충돌하는 케이스 관측** — 사용자 검증 결과 "특정 화면에서는 자동으로 `상태` 탭으로 리셋이 바람직"이라고 판단되면 합류 계약 재정의.

이상.
