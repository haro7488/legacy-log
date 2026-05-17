# LegacyLog (legacy-log-project)

Godot 4.6.2 + C# (.NET 8) 게임 프로젝트.

## 세션 역할

이 디렉토리에서 열린 Claude Code 세션은 **Project Claude**다.

- 작업 영역: 이 디렉토리 전체 (`legacy-log-project/`)
- 주 책임: Project Codex의 구현 계획을 검토하고 개선점을 제안한 뒤, 최종 작업 지시서에 따라 구현한다.
- 상위 `../CLAUDE.md`(메타 에이전트용)는 `.claude/settings.json`의 `claudeMdExcludes`로 자동 로드에서 제외돼 있다. 필요하면 `../docs/current-focus.md`와 `../docs/decisions/002-agent-workflow.md`를 직접 참조한다.

## 기본 흐름

기본 작업 흐름은 상위 결정 문서 `../docs/decisions/002-agent-workflow.md`를 따른다.

1. Project Codex가 아키텍처 및 구현 계획을 작성한다.
2. Project Claude가 구현 계획을 검토하고 개선점을 제안한다.
3. Project Codex가 개선점을 검토한 뒤 최종 작업 지시서를 작성한다.
4. Project Claude가 최종 작업 지시서에 따라 구현한다.

## 모드 구분

Project Claude는 현재 지시가 **리뷰 요청**인지 **구현 요청**인지 먼저 구분한다.

### 리뷰 요청일 때

- 구현 계획의 누락, 위험, 범위 초과, 검증 부족을 우선 검토한다.
- 바로 구현하지 않는다.
- 개선점은 Project Codex가 판단해 반영할 수 있도록 구체적으로 제안한다.
- PRD나 현재 초점에 없는 게임 규칙을 새로 만들지 않는다.
- 리뷰 요청 문서가 답변 문서 경로를 지정하면, 리뷰 결과를 해당 파일에 작성한다.
- 답변 문서에는 Project Codex가 이어받을 수 있도록 주요 검토 의견, 범위 초과 여부, 병렬 작업 분해 개선안, 합류 계약, 검증 보완안을 포함한다.

### 구현 요청일 때

- Project Codex가 작성한 최종 작업 지시서를 기준으로 구현한다.
- 지시서의 범위를 벗어난 개선이나 리팩터링을 하지 않는다.
- 사전 가정이 실제 코드와 맞지 않으면 추측으로 진행하지 않고 멈춘다.
- 구현 후 지정된 검증 명령을 실행하고 결과를 보고한다.
- 최종 작업 지시서가 구현 결과 보고 경로를 지정하면, 변경 파일, 구현된 사용자 흐름, 실행한 확인 명령과 결과, 남은 제약을 해당 파일에 작성한다.

## 의존성

로컬 헤드리스 작업에 필요한 도구:

| 도구 | 버전 | 설치 |
|---|---|---|
| Godot Engine (Mono) | 4.6.2 | `winget install --id GodotEngine.GodotEngine.Mono -e` |
| .NET SDK | 8.0.x | `winget install --id Microsoft.DotNet.SDK.8 -e` |

## 헤드리스 명령

**임포트 (에셋 스캔 / `.godot/` 갱신)**
```bash
./scripts/godot.sh --headless --import      # Git Bash
.\scripts\godot.cmd --headless --import     # cmd / PowerShell
```

**C# 빌드** (셸 무관)
```
dotnet build
```

새 스크립트/씬 추가 후 위 두 단계를 한 번씩 돌리면 IDE 없이도 컴파일 가능 상태가 유지된다.

## Godot 에디터 실행

```bash
./scripts/godot.sh -e        # Git Bash
.\scripts\godot.cmd -e       # cmd / PowerShell
```

## Godot 호출 규칙

- Godot은 반드시 `scripts/godot.sh`(POSIX 셸) 또는 `scripts/godot.cmd`(Windows 셸) 래퍼를 통해 실행한다. `winget` 설치 시 PATH에 등록되는 `WinGet/Links/godot.exe` 심볼릭 링크로 직접 실행하면 Godot이 `GodotSharp/` 디렉터리를 링크 위치 기준으로 찾아 .NET 어셈블리 로드에 실패한다(헤드리스에서도 GUI 다이얼로그가 뜸).
- 두 래퍼 모두 `GODOT_BIN` 환경 변수가 있으면 그것을 우선 사용하고, 없으면 표준 winget Mono 설치 경로를 자동 탐색한다. CI 등에서는 `GODOT_BIN`을 명시적으로 지정한다.
- `*.cmd`/`*.bat`은 CRLF가 강제(`.gitattributes`)된다. LF로 저장되면 cmd가 라인을 잘못 파싱한다.

## Godot + C# 함정 (반복 방지)

### `.tscn`의 `ext_resource path`는 디스크 케이스와 정확히 일치해야 한다

`Godot.SourceGenerators`는 컴파일러가 받은 입력 파일 경로(= 디스크 케이스)를 기준으로 각 `partial class : Godot.GodotObject`에 `[ScriptPath("res://...")]`를 자동 생성한다. Godot의 `CSharpScript`는 이 어트리뷰트와 씬의 `ext_resource path=` 문자열을 **대소문자 정확히 일치**로 매칭한다. Windows의 파일시스템은 대소문자를 무시하므로 파일 읽기는 성공하지만, 매칭에서 어긋나면 다음 에러가 뜬다:

```
ERROR: Cannot instantiate C# script because the associated class could not be found.
Script: 'res://Xxx/Foo.cs'. Make sure the script exists and contains a class
definition with a name that matches the filename of the script exactly...
```

이 메시지는 "클래스가 없다"고 말하지만 실제 원인은 거의 항상 **경로 케이스 mismatch**다. 점검 순서:

1. 실제 디스크 케이스를 확인한다: `find . -iname "<file>.cs"` (Git Bash) 또는 `Get-ChildItem -Recurse -Filter "<file>.cs"` (PowerShell)
2. `.tscn`의 `ext_resource path` 문자열을 디스크 케이스와 글자 단위로 맞춘다
3. 디렉터리가 사전 존재하면(예: `scripts/` 래퍼 디렉터리) 새 디렉터리를 같은 이름의 대문자 변형으로 만들지 말 것. Windows에서는 합쳐져 사전 케이스가 유지된다

부수 사실: 클래스를 네임스페이스에 두든 톱레벨에 두든, 또는 `[GlobalClass]` 어트리뷰트 유무도 이 에러의 원인이 아니다. 네임스페이스를 지우는 시도는 거의 항상 시간 낭비다.

### Windows PowerShell 5.1로 Godot용 .NET 8 어셈블리를 검사하지 말 것

`powershell.exe`(Windows PowerShell 5.1)는 .NET Framework 위에서 돈다. `LegacyLog.dll`은 .NET 8을 타깃하므로 `GodotSharp`, `System.Runtime 8.0.0.0` 같은 의존성이 PS 5.1에서 로드되지 않는다. 결과:

- `Assembly.GetTypes()` → `ReflectionTypeLoadException`
- `asm.GetType("Foo")` → 클래스가 실제로 dll에 있어도 `null` 반환 (false negative)

dll 안의 타입을 진짜로 확인해야 하면 `dotnet` CLI, `pwsh`(PowerShell 7+), 또는 작은 `dotnet run` 스크립트를 쓴다. 그 전에 `dotnet build`가 0 오류로 끝났는지 먼저 본다 — 보통 그것으로 충분하다.

### 이 에이전트 컨텍스트에서 Godot GUI 실행(`godot.cmd` 단독)을 신뢰 도구로 쓰지 말 것

- 디스플레이 인터랙션 루프가 없어 사람이 클릭으로 흐름을 진행할 수 없다
- `timeout`이나 외부 신호로 강제 종료하면 teardown 중 `Fatal error. Internal CLR error. (0x80131506)`가 stdout으로 새는데, 이건 코드 문제가 아니라 비정상 종료 산물이다. 이걸 단서로 코드를 의심하면 시간 낭비
- 9단계 사용자 흐름 같은 기능 검증은 `--smoke` 류의 헤드리스 자동 분기로 처리한다. GUI는 사람 검증자에게 남긴다

### 검증 반복 비용 — 헤드리스 명령 사이클은 비싸다

이 머신에서 한 사이클은 대략:

- `dotnet build` ≈ 1–3초
- `.\scripts\godot.cmd --headless --import` ≈ 5–10초
- `.\scripts\godot.cmd --headless -- --smoke` ≈ 5–10초

작은 수정 한 건마다 세 단계를 다 돌리면 분 단위로 누적된다. 권장:

- C# 변경만 한 경우: `dotnet build`로 먼저 빠르게 컴파일 오류를 거른 뒤, 통과하면 그때 `--smoke`만 돌린다. `--import`는 리소스(`.tscn`, `.import`, 새 에셋 파일)가 변하지 않았으면 다시 돌릴 필요가 없다
- 씬/리소스 변경이 포함된 경우에만 `--import`를 추가
- 같은 단계를 여러 번 돌릴 때는 백그라운드 실행(`run_in_background`)으로 묶지 말고, 한 번에 끝나면 결과만 확인한다 (출력 파일 폴링은 또 시간을 먹는다)

### `--quit-after N`은 프레임 단위(초 아님)

Godot 4의 `--quit-after`는 프레임 수다. `--quit-after 2`는 약 2/60초 안에 종료되므로 사용자 흐름 검증에 부적합하다. 종료 제어가 필요하면 코드에서 `GetTree().Quit()`을 명시적으로 부른다(`--smoke` 분기 패턴 참고).

## 컨벤션

(추가 결정 사항이 도출되는 대로 이 섹션에 누적한다.)
