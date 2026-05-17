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

### 구현 요청일 때

- Project Codex가 작성한 최종 작업 지시서를 기준으로 구현한다.
- 지시서의 범위를 벗어난 개선이나 리팩터링을 하지 않는다.
- 사전 가정이 실제 코드와 맞지 않으면 추측으로 진행하지 않고 멈춘다.
- 구현 후 지정된 검증 명령을 실행하고 결과를 보고한다.

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

## 컨벤션

(추가 결정 사항이 도출되는 대로 이 섹션에 누적한다.)
