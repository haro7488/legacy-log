# LegacyLog (legacy-log-project)

Godot 4.6.2 + C# (.NET 8) 게임 프로젝트.

## 세션 역할

이 디렉토리에서 열린 Claude Code 세션은 **구현 에이전트**다.

- 작업 영역: 이 디렉토리 전체 (`legacy-log-project/`)
- 상위 `../CLAUDE.md`(메타 에이전트용)는 `.claude/settings.json`의 `claudeMdExcludes`로 자동 로드에서 제외돼 있다. 참조하지 않는다.

## 의존성

로컬 헤드리스 작업에 필요한 도구:

| 도구 | 버전 | 설치 |
|---|---|---|
| Godot Engine (Mono) | 4.6.2 | `winget install --id GodotEngine.GodotEngine.Mono -e` |
| .NET SDK | 8.0.x | `winget install --id Microsoft.DotNet.SDK.8 -e` |

## 헤드리스 명령

**임포트 (에셋 스캔 / `.godot/` 갱신)**
```bash
./scripts/godot.sh --headless --import
```

**C# 빌드**
```bash
dotnet build
```

**둘 다** 새 코드/씬 추가 후 한 번씩 실행하면 IDE 없이도 컴파일 가능 상태가 유지된다.

## Godot 호출 규칙

- Godot은 반드시 `scripts/godot.sh` 래퍼를 통해 실행한다. `winget` 설치 시 PATH에 등록되는 `WinGet/Links/godot.exe` 심볼릭 링크로 직접 실행하면 Godot이 `GodotSharp/` 디렉터리를 링크 위치 기준으로 찾아 .NET 어셈블리 로드에 실패한다(헤드리스에서도 GUI 다이얼로그가 뜸).
- 래퍼는 `GODOT_BIN` 환경 변수가 있으면 그것을 우선 사용하고, 없으면 표준 winget Mono 설치 경로를 자동 탐색한다. CI 등에서는 `GODOT_BIN`을 명시적으로 지정한다.

## 컨벤션

(추가 결정 사항이 도출되는 대로 이 섹션에 누적한다.)
