// MVP First Loop 진입점.
// 코드 기반 동적 UI 생성으로 시작/사건/결과/세대 요약 흐름을 단일 Control 위에 구성한다.
// 자세한 합류 계약은 docs/mvp-first-loop-final-work-instruction.md를 참고.

using Godot;
using System.Collections.Generic;

public partial class Main : Control
{
    private RunState _state = null!;
    private VBoxContainer _content = null!;
    private VBoxContainer _statsPanel = null!;

    public override void _Ready()
    {
        _state = MvpLoopContent.CreateInitialRunState();

        if (IsSmokeMode())
        {
            RunSmoke();
            return;
        }

        BuildShell();
        ShowStart();
    }

    private static bool IsSmokeMode()
    {
        foreach (var arg in OS.GetCmdlineArgs())
        {
            if (arg == "--smoke")
            {
                return true;
            }
        }

        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            if (arg == "--smoke")
            {
                return true;
            }
        }

        return false;
    }

    private void BuildShell()
    {
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var root = new VBoxContainer();
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);

        var header = new Label
        {
            Text = $"{_state.FamilyName} — {_state.FounderName}",
        };
        root.AddChild(header);

        _content = new VBoxContainer();
        root.AddChild(_content);

        root.AddChild(new HSeparator());

        var statsHeader = new Label { Text = "현재 상태" };
        root.AddChild(statsHeader);

        _statsPanel = new VBoxContainer();
        root.AddChild(_statsPanel);

        RenderStats();
    }

    private void ClearContent()
    {
        foreach (Node child in _content.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void RenderStats()
    {
        foreach (Node child in _statsPanel.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = _state.Stats.TryGetValue(definition.Key, out var v) ? v : 0;
            _statsPanel.AddChild(new Label
            {
                Text = $"{definition.Label}: {value}",
            });
        }
    }

    private void ShowStart()
    {
        ClearContent();

        _content.AddChild(new Label
        {
            Text = _state.IntroLine,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var startButton = new Button { Text = "첫 사건으로 들어간다" };
        startButton.Pressed += ShowEvent;
        _content.AddChild(startButton);
    }

    private void ShowEvent()
    {
        ClearContent();

        var ev = MvpLoopContent.Events[_state.CurrentEventIndex];

        _content.AddChild(new Label
        {
            Text = $"[{_state.CurrentEventIndex + 1} / {MvpLoopContent.Events.Count}] {ev.Title}",
        });

        _content.AddChild(new Label
        {
            Text = ev.Body,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        foreach (var choice in ev.Choices)
        {
            var captured = choice;
            var button = new Button { Text = captured.Label };
            button.Pressed += () => ApplyChoice(captured);
            _content.AddChild(button);
        }
    }

    private void ApplyChoice(EventChoice choice)
    {
        var ev = MvpLoopContent.Events[_state.CurrentEventIndex];
        _state.ApplyChoice(ev, choice);
        RenderStats();
        ShowResult(ev, choice);
    }

    private void ShowResult(LoopEvent ev, EventChoice choice)
    {
        ClearContent();

        _content.AddChild(new Label
        {
            Text = $"결과: {choice.ResultText}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        foreach (var delta in choice.Deltas)
        {
            var label = MvpLoopContent.GetStateLabel(delta.Key);
            var sign = delta.Amount >= 0 ? "+" : string.Empty;
            _content.AddChild(new Label
            {
                Text = $"{label} {sign}{delta.Amount}",
            });
        }

        var latest = _state.Chronicle[^1];
        _content.AddChild(new Label
        {
            Text = $"연대기: {latest.RecordedLine}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        bool hasMore = _state.CurrentEventIndex < MvpLoopContent.Events.Count;
        var nextButton = new Button
        {
            Text = hasMore ? "다음 사건으로" : "세대 요약 보기",
        };
        nextButton.Pressed += () =>
        {
            if (_state.CurrentEventIndex < MvpLoopContent.Events.Count)
            {
                ShowEvent();
            }
            else
            {
                ShowSummary();
            }
        };
        _content.AddChild(nextButton);
    }

    private void ShowSummary()
    {
        ClearContent();

        _content.AddChild(new Label
        {
            Text = $"{_state.FamilyName}의 첫 세대 요약",
        });

        _content.AddChild(new Label { Text = "누적 연대기" });
        foreach (var entry in _state.Chronicle)
        {
            _content.AddChild(new Label
            {
                Text = $"- ({entry.EventTitle}) {entry.RecordedLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _content.AddChild(new Label { Text = "최종 상태" });
        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = _state.Stats.TryGetValue(definition.Key, out var v) ? v : 0;
            _content.AddChild(new Label
            {
                Text = $"- {definition.Label}: {value}",
            });
        }

        _content.AddChild(new Label
        {
            Text = $"{_state.FounderName}의 첫 장은 이렇게 닫힌다. 다음 세대가 이 기록 위에 새 줄을 쓸 것이다.",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });
    }

    // MVP 검증용 임시 자동 스모크. 통합 테스트 체계 도입 시 제거 대상.
    // 각 사건의 첫 번째 선택지를 자동 선택해 stdout에 핵심 정보를 출력하고 SMOKE_OK로 종료.
    private void RunSmoke()
    {
        GD.Print($"[SMOKE] START family={_state.FamilyName} founder={_state.FounderName}");
        GD.Print($"[SMOKE] INTRO {_state.IntroLine}");

        for (int i = 0; i < MvpLoopContent.Events.Count; i++)
        {
            var ev = MvpLoopContent.Events[i];
            var choice = ev.Choices[0];
            GD.Print($"[SMOKE] EVENT {i + 1} title={ev.Title} choice={choice.Label}");

            _state.ApplyChoice(ev, choice);
            GD.Print($"[SMOKE] RESULT {choice.ResultText}");

            foreach (var delta in choice.Deltas)
            {
                var label = MvpLoopContent.GetStateLabel(delta.Key);
                var sign = delta.Amount >= 0 ? "+" : string.Empty;
                GD.Print($"[SMOKE] DELTA {label} {sign}{delta.Amount}");
            }

            var latest = _state.Chronicle[^1];
            GD.Print($"[SMOKE] CHRONICLE {latest.RecordedLine}");
        }

        GD.Print($"[SMOKE] SUMMARY family={_state.FamilyName}");
        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = _state.Stats.TryGetValue(definition.Key, out var v) ? v : 0;
            GD.Print($"[SMOKE] FINAL {definition.Label}={value}");
        }

        GD.Print("SMOKE_OK");
        GetTree().Quit();
    }
}
