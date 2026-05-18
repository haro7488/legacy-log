// Multigen MVP 진입점.
// 코드 기반 동적 UI 생성으로 시작/사건/결과/세대 요약/후계 선택/멸문 흐름을
// 단일 Control 위에 구성한다. Scenes/Main.tscn은 루트 Control + 본 스크립트만 어태치한다.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름):
// - MultigenFlow의 public 진입점만 호출한다(흐름 계산).
// - MultigenContent의 자리표시자 시드 함수만 호출한다(콘텐츠 문장).
// - 기본 상태 표시는 4단계 질적 라벨, 토글을 통해 상세 숫자/방향을 본다.
// - --smoke는 한 실행에서 후계 정상 경로(1세대 → 2세대) + 멸문 경로(2세대 종료)를 모두 검증한다.

using Godot;
using System.Collections.Generic;

public partial class Main : Control
{
    private FamilyRunState _family = null!;
    private VBoxContainer _content = null!;
    private VBoxContainer _statsPanel = null!;
    private Label _headerLabel = null!;
    private Button _detailToggleButton = null!;
    private VBoxContainer _detailPanel = null!;
    private bool _detailVisible;

    public override void _Ready()
    {
        _family = MultigenFlow.CreateNewFamily();

        if (IsSmokeMode())
        {
            RunSmoke();
            return;
        }

        BuildShell();
        ShowGenerationStart();
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

        _headerLabel = new Label();
        root.AddChild(_headerLabel);
        RefreshHeader();

        _content = new VBoxContainer();
        root.AddChild(_content);

        root.AddChild(new HSeparator());

        var statsHeader = new Label { Text = "현재 상태 (질적 표시)" };
        root.AddChild(statsHeader);

        _statsPanel = new VBoxContainer();
        root.AddChild(_statsPanel);

        _detailToggleButton = new Button { Text = "상세 정보 보기" };
        _detailToggleButton.Pressed += ToggleDetailPanel;
        root.AddChild(_detailToggleButton);

        _detailPanel = new VBoxContainer { Visible = false };
        root.AddChild(_detailPanel);

        RenderStats();
    }

    private void RefreshHeader()
    {
        _headerLabel.Text =
            $"{_family.FamilyName} — {MultigenContent.FormatFamilyEra(_family.CurrentGeneration)} — {_family.CurrentCharacterName}";
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
            var value = _family.FamilyStats.TryGetValue(definition.Key, out var v) ? v : 0;
            var band = MultigenFlow.BuildStateBandLabel(definition.Key, value);
            _statsPanel.AddChild(new Label
            {
                Text = $"{definition.Label}: {band}",
            });
        }

        RenderDetailPanel();
        RenderTraits();
    }

    private void RenderDetailPanel()
    {
        foreach (Node child in _detailPanel.GetChildren())
        {
            child.QueueFree();
        }

        _detailPanel.AddChild(new Label { Text = "[자리표시자: 추가 정보창]" });
        foreach (var line in MultigenFlow.BuildDetailedStateLines(_family))
        {
            _detailPanel.AddChild(new Label { Text = line });
        }
    }

    private void RenderTraits()
    {
        if (_family.CurrentCharacterTraits.Count == 0)
        {
            return;
        }

        var traitLine = new System.Text.StringBuilder();
        traitLine.Append("특성: ");
        for (int i = 0; i < _family.CurrentCharacterTraits.Count; i++)
        {
            if (i > 0) traitLine.Append(", ");
            traitLine.Append(_family.CurrentCharacterTraits[i].Label);
        }
        _statsPanel.AddChild(new Label { Text = traitLine.ToString() });
    }

    private void ToggleDetailPanel()
    {
        _detailVisible = !_detailVisible;
        _detailPanel.Visible = _detailVisible;
        _detailToggleButton.Text = _detailVisible ? "상세 정보 숨기기" : "상세 정보 보기";
    }

    private void ShowGenerationStart()
    {
        RefreshHeader();
        RenderStats();
        ClearContent();

        var intro = MultigenContent.GetCharacterIntroLine(_family.CurrentCharacterName, _family.CurrentGeneration);
        _content.AddChild(new Label
        {
            Text = intro,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var startButton = new Button { Text = "첫 사건으로 들어간다" };
        startButton.Pressed += ShowEvent;
        _content.AddChild(startButton);
    }

    private void ShowEvent()
    {
        ClearContent();

        var run = _family.CurrentRun;
        var ev = MvpLoopContent.Events[run.CurrentEventIndex];

        _content.AddChild(new Label
        {
            Text = $"[{run.CurrentEventIndex + 1} / {MvpLoopContent.Events.Count}] {ev.Title}",
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
        var run = _family.CurrentRun;
        var ev = MvpLoopContent.Events[run.CurrentEventIndex];
        run.ApplyChoice(ev, choice, _family.FamilyStats);
        MultigenFlow.RecordOutcomeTag(_family, choice.OutcomeTag);
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

        var latest = _family.CurrentRun.Chronicle[^1];
        _content.AddChild(new Label
        {
            Text = $"연대기: {latest.RecordedLine}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        bool hasMore = _family.CurrentRun.CurrentEventIndex < MvpLoopContent.Events.Count;
        var nextButton = new Button
        {
            Text = hasMore ? "다음 사건으로" : "세대 요약 보기",
        };
        nextButton.Pressed += () =>
        {
            if (_family.CurrentRun.CurrentEventIndex < MvpLoopContent.Events.Count)
            {
                ShowEvent();
            }
            else
            {
                ShowGenerationSummary();
            }
        };
        _content.AddChild(nextButton);
    }

    private GenerationEndResult _pendingEnd = null!;

    private void ShowGenerationSummary()
    {
        ClearContent();

        var endResult = MultigenFlow.FinishCurrentGeneration(_family);
        _pendingEnd = endResult;

        _content.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — {MultigenContent.FormatFamilyEra(_family.CurrentGeneration)} 세대 요약",
        });

        _content.AddChild(new Label { Text = "누적 연대기 (이번 세대)" });
        foreach (var entry in _family.CurrentRun.Chronicle)
        {
            _content.AddChild(new Label
            {
                Text = $"- ({entry.EventTitle}) {entry.RecordedLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _content.AddChild(new Label { Text = "후대 기록자의 요약" });
        _content.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        if (_family.History.Count > 0)
        {
            _content.AddChild(new Label { Text = "지난 세대 이력" });
            foreach (var rec in _family.History)
            {
                _content.AddChild(new Label
                {
                    Text = $"- {MultigenContent.FormatFamilyEra(rec.GenerationNumber)} {rec.CharacterName} ({rec.EndType})",
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }
        }

        if (_family.CarriedOutcomeTags.Count > 0)
        {
            var joined = string.Join(", ", _family.CarriedOutcomeTags);
            _content.AddChild(new Label { Text = $"누적 큰 사건 결과 태그: {joined}" });
        }

        var nextButton = new Button
        {
            Text = endResult.IsExtinct ? "가문의 끝을 본다" : "후계를 본다",
        };
        nextButton.Pressed += AfterSummary;
        _content.AddChild(nextButton);
    }

    private void AfterSummary()
    {
        if (_pendingEnd is null)
        {
            return;
        }

        var record = MultigenFlow.BuildGenerationRecord(_family, _pendingEnd);
        _family.RecordCurrentGenerationEnd(record);

        if (_pendingEnd.IsExtinct)
        {
            ShowExtinction(_pendingEnd);
        }
        else
        {
            ShowSuccession(_pendingEnd);
        }
    }

    private void ShowSuccession(GenerationEndResult endResult)
    {
        ClearContent();

        _content.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — 후계 후보",
        });

        foreach (var candidate in endResult.Candidates)
        {
            var captured = candidate;
            _content.AddChild(new Label
            {
                Text = $"- {captured.Name}: {captured.Description}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });

            if (captured.Traits.Count > 0)
            {
                var traitText = new System.Text.StringBuilder();
                traitText.Append("  특성: ");
                for (int i = 0; i < captured.Traits.Count; i++)
                {
                    if (i > 0) traitText.Append(", ");
                    traitText.Append(captured.Traits[i].Label);
                }
                _content.AddChild(new Label { Text = traitText.ToString() });
            }

            var pickButton = new Button
            {
                Text = endResult.Candidates.Count == 1
                    ? $"{captured.Name}로 다음 세대로 진입"
                    : $"{captured.Name}을(를) 후계로 삼는다",
            };
            pickButton.Pressed += () => OnSuccessionPicked(captured);
            _content.AddChild(pickButton);
        }
    }

    private void OnSuccessionPicked(SuccessionCandidate chosen)
    {
        var nextRun = MultigenFlow.BuildNextGenerationRun(_family, chosen);
        MultigenFlow.AdvanceToNextGeneration(_family, chosen, nextRun);
        _pendingEnd = null;
        ShowGenerationStart();
    }

    private void ShowExtinction(GenerationEndResult endResult)
    {
        ClearContent();
        _family.MarkExtinct();

        _content.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — 가문 멸문",
        });

        _content.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _content.AddChild(new Label { Text = "가문 전체 이력" });
        foreach (var rec in _family.History)
        {
            _content.AddChild(new Label
            {
                Text = $"- {MultigenContent.FormatFamilyEra(rec.GenerationNumber)} {rec.CharacterName} ({rec.EndType})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
    }

    // MVP 검증용 임시 자동 스모크. 통합 테스트 체계 도입 시 제거 대상.
    // 경로 A: 1세대(NaturalDeath, 후계 있음) → 2세대 진입 → 2세대(Extinction) → SMOKE_OK.
    // 각 사건의 첫 번째 선택지를 자동 선택해 stdout에 핵심 정보를 출력한다.
    private void RunSmoke()
    {
        GD.Print($"[SMOKE] FAMILY name={_family.FamilyName} first_character={_family.CurrentCharacterName}");

        // 1세대
        RunSmokeGeneration();

        var firstEnd = MultigenFlow.FinishCurrentGeneration(_family);
        PrintSmokeGenerationEnd(firstEnd);

        if (firstEnd.IsExtinct || firstEnd.Candidates.Count == 0)
        {
            GD.Print("[SMOKE] ERROR 1세대에서 후계 없음 — 경로 A 검증 실패");
            GD.Print("SMOKE_OK");
            GetTree().Quit();
            return;
        }

        var firstRecord = MultigenFlow.BuildGenerationRecord(_family, firstEnd);
        _family.RecordCurrentGenerationEnd(firstRecord);

        var chosen = firstEnd.Candidates[0];
        GD.Print($"[SMOKE] SUCCESSION_PICK name={chosen.Name}");

        var carryStatsBefore = SerializeStats(_family.FamilyStats);
        var nextRun = MultigenFlow.BuildNextGenerationRun(_family, chosen);
        MultigenFlow.AdvanceToNextGeneration(_family, chosen, nextRun);
        var carryStatsAfter = SerializeStats(_family.FamilyStats);
        var carriedTags = _family.CarriedOutcomeTags.Count == 0
            ? "(none)"
            : string.Join(",", _family.CarriedOutcomeTags);

        GD.Print($"[SMOKE] GEN 2 character={_family.CurrentCharacterName}");
        GD.Print($"[SMOKE] CARRIED stats_before={carryStatsBefore} stats_after_carry={carryStatsAfter} outcome_tags={carriedTags} chronicle_history_count={_family.History.Count}");

        // 2세대
        RunSmokeGeneration();

        var secondEnd = MultigenFlow.FinishCurrentGeneration(_family);
        PrintSmokeGenerationEnd(secondEnd);

        var secondRecord = MultigenFlow.BuildGenerationRecord(_family, secondEnd);
        _family.RecordCurrentGenerationEnd(secondRecord);

        if (secondEnd.IsExtinct)
        {
            _family.MarkExtinct();
            GD.Print($"[SMOKE] EXTINCT family={_family.FamilyName} total_generations={_family.History.Count}");
        }
        else
        {
            GD.Print("[SMOKE] WARN 2세대에서 멸문이 발화하지 않음 — 경로 A 멸문 단계 검증 실패");
        }

        GD.Print("SMOKE_OK");
        GetTree().Quit();
    }

    private void RunSmokeGeneration()
    {
        GD.Print($"[SMOKE] GEN {_family.CurrentGeneration} character={_family.CurrentCharacterName} era={MultigenContent.FormatFamilyEra(_family.CurrentGeneration)}");
        GD.Print($"[SMOKE] INTRO {MultigenContent.GetCharacterIntroLine(_family.CurrentCharacterName, _family.CurrentGeneration)}");

        for (int i = 0; i < MvpLoopContent.Events.Count; i++)
        {
            var ev = MvpLoopContent.Events[i];
            var choice = ev.Choices[0];
            GD.Print($"[SMOKE] EVENT gen={_family.CurrentGeneration} idx={i + 1} title={ev.Title} choice={choice.Label} tag={choice.OutcomeTag}");

            _family.CurrentRun.ApplyChoice(ev, choice, _family.FamilyStats);
            MultigenFlow.RecordOutcomeTag(_family, choice.OutcomeTag);

            GD.Print($"[SMOKE] RESULT {choice.ResultText}");

            foreach (var delta in choice.Deltas)
            {
                var label = MvpLoopContent.GetStateLabel(delta.Key);
                var sign = delta.Amount >= 0 ? "+" : string.Empty;
                GD.Print($"[SMOKE] DELTA {label} {sign}{delta.Amount}");
            }

            var latest = _family.CurrentRun.Chronicle[^1];
            GD.Print($"[SMOKE] CHRONICLE {latest.RecordedLine}");
        }

        GD.Print($"[SMOKE] STATE_END gen={_family.CurrentGeneration} stats={SerializeStats(_family.FamilyStats)}");
        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = _family.FamilyStats.TryGetValue(definition.Key, out var v) ? v : 0;
            var band = MultigenFlow.BuildStateBandLabel(definition.Key, value);
            GD.Print($"[SMOKE] BAND {definition.Label}={band} value={value}");
        }
    }

    private static void PrintSmokeGenerationEnd(GenerationEndResult endResult)
    {
        GD.Print($"[SMOKE] GEN_END type={endResult.EndType} is_current_character_gone={endResult.IsCurrentCharacterGone} is_extinct={endResult.IsExtinct}");
        GD.Print($"[SMOKE] SUMMARY {endResult.SummaryParagraph}");
        GD.Print($"[SMOKE] SUCCESSION candidates={endResult.Candidates.Count}");
        for (int i = 0; i < endResult.Candidates.Count; i++)
        {
            var c = endResult.Candidates[i];
            GD.Print($"[SMOKE] CANDIDATE idx={i} name={c.Name} traits={c.Traits.Count}");
        }
    }

    private static string SerializeStats(Dictionary<string, int> stats)
    {
        var parts = new List<string>();
        foreach (var pair in stats)
        {
            parts.Add($"{pair.Key}={pair.Value}");
        }
        return "[" + string.Join(",", parts) + "]";
    }
}
