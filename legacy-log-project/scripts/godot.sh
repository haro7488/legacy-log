#!/usr/bin/env bash
# Godot 헤드리스 호출 래퍼.
#
# winget으로 설치한 Godot Mono를 PATH의 심볼릭 링크(WinGet/Links/godot.exe)로
# 실행하면 Godot이 GodotSharp 디렉터리를 링크 위치 기준으로 찾는 알려진 문제로
# .NET 어셈블리 로드에 실패한다. 따라서 항상 실제 exe 절대경로로 호출해야 한다.
#
# 우선순위:
#   1. GODOT_BIN 환경 변수가 설정돼 있으면 그것을 사용 (CI 등에서 명시 지정)
#   2. winget Mono 표준 설치 경로를 자동 탐색 (로컬 Windows + Git Bash)

set -euo pipefail

if [[ -n "${GODOT_BIN:-}" ]]; then
    exec "$GODOT_BIN" "$@"
fi

candidate="$(ls -1 "$HOME"/AppData/Local/Microsoft/WinGet/Packages/GodotEngine.GodotEngine.Mono_*/Godot_*_mono_win64/Godot_*_mono_win64.exe 2>/dev/null \
    | grep -v '_console\.exe$' \
    | sort -V \
    | tail -1)"

if [[ -z "${candidate:-}" ]]; then
    echo "Godot Mono 빌드를 찾지 못했다. 다음 중 하나를 수행하라:" >&2
    echo "  1) winget install --id GodotEngine.GodotEngine.Mono -e" >&2
    echo "  2) GODOT_BIN 환경 변수에 Godot exe 절대경로 지정" >&2
    exit 1
fi

exec "$candidate" "$@"
