// Multigen MVP 진입점.
// 코드 기반 동적 UI 생성으로 시작/사건/결과/세대 요약/후계 선택/멸문 흐름을
// 단일 Control 위에 구성한다. Scenes/Main.tscn은 루트 Control + 본 스크립트만 어태치한다.
//
// 합류 계약(2026-05-19, multigen-ui-ux-improvement):
// - UI 영역은 ContextHeader / MainScroll(MainContent) / InfoTabs / ActionBar 4영역.
//   루트 Control만 FullRect를 잡고, 내부 컨테이너는 부모 컨테이너 레이아웃을 따른다.
// - InfoTabs는 3개 탭(상태/연대기/가문사). 유산 정보는 가문사 탭에 흡수한다.
// - 탭 갱신은 RenderInfoTabs() 단일 진입점, ActionBar 갱신은 SetActionBar() 단일 진입점.
// - 화면 전환은 RefreshContextHeader → ClearMainContent → 본문 구성 → SetActionBar
//   → RenderInfoTabs → ResetMainScroll 흐름을 명확히 호출한다.
// - 상태 상세 숫자/방향은 상태 탭 안 토글로 유지하며, 토글 상태는 화면 전환과 무관하게 보존한다.
// - 사건 화면 선택지는 ActionBar에 둔다(현재 MVP는 2개).
// - 멸문 화면 ActionBar에는 진행 버튼을 두지 않고 비활성 종료 안내 라벨만 둔다.
// - 탭 사용자 선택은 화면 전환 시 유지한다(InfoTabs.CurrentTab을 강제하지 않는다).
// - History.Add는 AfterSummary 시점에서만 일어나며, 세대 요약 화면 시점의
//   가문사 탭은 아직 현재 세대를 History에 포함하지 않는다.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름):
// - MultigenFlow의 public 진입점만 호출한다(흐름 계산).
// - MultigenContent의 자리표시자 시드 함수만 호출한다(콘텐츠 문장).
// - 기본 상태 표시는 4단계 질적 라벨, 토글을 통해 상세 숫자/방향을 본다.
// - --smoke는 한 실행에서 후계 정상 경로(1세대 → 2세대) + 멸문 경로(2세대 종료)를 모두 검증한다.
// - --smoke는 UI 노드를 만들지 않는 기능 검증용 경로다. UI 회귀는 수동 GUI 검증으로 확인한다.

using Godot;
using System.Collections.Generic;

public partial class Main : Control
{
    private FamilyRunState _family = null!;

    // 4영역 노드.
    private VBoxContainer _contextHeader = null!;
    private Label _contextHeaderLine1 = null!;
    private Label _contextHeaderLine2 = null!;
    private ScrollContainer _mainScroll = null!;
    private VBoxContainer _mainContent = null!;
    private TabContainer _infoTabs = null!;
    private VBoxContainer _stateTabContent = null!;
    private VBoxContainer _chronicleTabContent = null!;
    private VBoxContainer _familyHistoryTabContent = null!;
    private VBoxContainer _actionBar = null!;

    // 상태 탭 상세 토글 상태(화면 전환 후에도 유지).
    private bool _stateDetailVisible;

    // 세대 요약 → AfterSummary 사이에 잠시 보관.
    private GenerationEndResult _pendingEnd = null!;

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

    // -------------------------------------------------------------------------
    // UI shell 빌더 (영역별 분리)
    // -------------------------------------------------------------------------

    private void BuildShell()
    {
        // 루트 Control만 FullRect.
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        // 작은 화면 안전 여백.
        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        AddChild(margin);

        var appRoot = new VBoxContainer
        {
            Name = "AppRoot",
        };
        appRoot.AddThemeConstantOverride("separation", 8);
        margin.AddChild(appRoot);

        BuildContextHeader(appRoot);
        BuildMainScroll(appRoot);
        BuildInfoTabs(appRoot);
        BuildActionBar(appRoot);
    }

    private void BuildContextHeader(Container parent)
    {
        _contextHeader = new VBoxContainer
        {
            Name = "ContextHeader",
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
        };
        _contextHeader.AddThemeConstantOverride("separation", 2);

        _contextHeaderLine1 = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        _contextHeader.AddChild(_contextHeaderLine1);

        _contextHeaderLine2 = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        _contextHeader.AddChild(_contextHeaderLine2);

        parent.AddChild(_contextHeader);
        parent.AddChild(new HSeparator());
    }

    private void BuildMainScroll(Container parent)
    {
        _mainScroll = new ScrollContainer
        {
            Name = "MainScroll",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        parent.AddChild(_mainScroll);

        _mainContent = new VBoxContainer
        {
            Name = "MainContent",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        _mainContent.AddThemeConstantOverride("separation", 6);
        _mainScroll.AddChild(_mainContent);
    }

    private void BuildInfoTabs(Container parent)
    {
        _infoTabs = new TabContainer
        {
            Name = "InfoTabs",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
        };
        _infoTabs.CustomMinimumSize = new Vector2(0, 180);

        _stateTabContent = AddTabPage(_infoTabs, "StateTab", "상태");
        _chronicleTabContent = AddTabPage(_infoTabs, "ChronicleTab", "연대기");
        _familyHistoryTabContent = AddTabPage(_infoTabs, "FamilyHistoryTab", "가문사");

        parent.AddChild(_infoTabs);
    }

    // 탭 노드 이름(영문)과 표시 라벨(한글)을 분리. 탭 내부에 ScrollContainer를 두어
    // 작은 화면에서도 ActionBar를 밀어내지 않고 탭 내용을 스크롤할 수 있게 한다.
    private static VBoxContainer AddTabPage(TabContainer tabs, string nodeName, string title)
    {
        var page = new ScrollContainer
        {
            Name = nodeName,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };

        var inner = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        inner.AddThemeConstantOverride("separation", 4);
        page.AddChild(inner);

        tabs.AddChild(page);
        tabs.SetTabTitle(tabs.GetTabCount() - 1, title);
        return inner;
    }

    private void BuildActionBar(Container parent)
    {
        parent.AddChild(new HSeparator());

        _actionBar = new VBoxContainer
        {
            Name = "ActionBar",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkEnd,
        };
        _actionBar.AddThemeConstantOverride("separation", 6);
        parent.AddChild(_actionBar);
    }

    // -------------------------------------------------------------------------
    // 영역 갱신 단일 진입점
    // -------------------------------------------------------------------------

    private void RefreshContextHeader(string stepLabel, string purposeLine)
    {
        _contextHeaderLine1.Text =
            $"{_family.FamilyName} · {MultigenContent.FormatFamilyEra(_family.CurrentGeneration)} · {_family.CurrentCharacterName}";
        _contextHeaderLine2.Text = $"[단계: {stepLabel}] {purposeLine}";
    }

    private void ClearMainContent()
    {
        foreach (Node child in _mainContent.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void ResetMainScroll()
    {
        _mainScroll.ScrollVertical = 0;
        _mainScroll.ScrollHorizontal = 0;
    }

    private void SetActionBar(IEnumerable<Control> children)
    {
        foreach (Node child in _actionBar.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var c in children)
        {
            _actionBar.AddChild(c);
        }
    }

    // -------------------------------------------------------------------------
    // 탭 렌더링 (RenderInfoTabs 단일 진입점)
    // -------------------------------------------------------------------------

    private void RenderInfoTabs()
    {
        RenderStateTab();
        RenderChronicleTab();
        RenderFamilyHistoryTab();
    }

    private void RenderStateTab()
    {
        foreach (Node child in _stateTabContent.GetChildren())
        {
            child.QueueFree();
        }

        var heading = new Label { Text = "현재 상태 (질적 표시)" };
        _stateTabContent.AddChild(heading);

        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = _family.FamilyStats.TryGetValue(definition.Key, out var v) ? v : 0;
            var band = MultigenFlow.BuildStateBandLabel(definition.Key, value);
            _stateTabContent.AddChild(new Label
            {
                Text = $"{definition.Label}: {band}",
            });
        }

        var toggleButton = new Button
        {
            Text = _stateDetailVisible ? "상세 정보 숨기기" : "상세 정보 보기",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
        };
        toggleButton.Pressed += ToggleStateDetail;
        _stateTabContent.AddChild(toggleButton);

        if (_stateDetailVisible)
        {
            var detailHeader = new Label { Text = "[자리표시자: 추가 정보창]" };
            _stateTabContent.AddChild(detailHeader);
            foreach (var line in MultigenFlow.BuildDetailedStateLines(_family))
            {
                _stateTabContent.AddChild(new Label { Text = line });
            }
        }

        _stateTabContent.AddChild(new HSeparator());

        var traitsHeader = new Label { Text = "현재 인물 특성" };
        _stateTabContent.AddChild(traitsHeader);

        if (_family.CurrentCharacterTraits.Count == 0)
        {
            _stateTabContent.AddChild(new Label
            {
                Text = "[자리표시자: 특성 없음]",
            });
        }
        else
        {
            var traitLine = new System.Text.StringBuilder();
            for (int i = 0; i < _family.CurrentCharacterTraits.Count; i++)
            {
                if (i > 0) traitLine.Append(", ");
                traitLine.Append(_family.CurrentCharacterTraits[i].Label);
            }
            _stateTabContent.AddChild(new Label { Text = traitLine.ToString() });
        }
    }

    private void ToggleStateDetail()
    {
        _stateDetailVisible = !_stateDetailVisible;
        RenderStateTab();
    }

    private void RenderChronicleTab()
    {
        foreach (Node child in _chronicleTabContent.GetChildren())
        {
            child.QueueFree();
        }

        var heading = new Label
        {
            Text = $"{MultigenContent.FormatFamilyEra(_family.CurrentGeneration)} 연대기",
        };
        _chronicleTabContent.AddChild(heading);

        var chronicle = _family.CurrentRun.Chronicle;
        if (chronicle.Count == 0)
        {
            _chronicleTabContent.AddChild(new Label
            {
                Text = "[자리표시자: 아직 기록된 사건이 없습니다]",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
            return;
        }

        for (int i = 0; i < chronicle.Count; i++)
        {
            var entry = chronicle[i];
            var prefix = (i == chronicle.Count - 1) ? "▶" : "·";
            _chronicleTabContent.AddChild(new Label
            {
                Text = $"{prefix} ({entry.EventTitle}) {entry.RecordedLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
    }

    private void RenderFamilyHistoryTab()
    {
        foreach (Node child in _familyHistoryTabContent.GetChildren())
        {
            child.QueueFree();
        }

        var heading = new Label { Text = "가문사" };
        _familyHistoryTabContent.AddChild(heading);

        // 유산 요약 (가문사 탭 상단으로 흡수).
        _familyHistoryTabContent.AddChild(new Label { Text = "유산 요약" });
        if (_family.CarriedOutcomeTags.Count == 0)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = "[자리표시자: 아직 이어진 유산이 없습니다]",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
        else
        {
            var joined = string.Join(", ", _family.CarriedOutcomeTags);
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = $"누적 큰 사건 결과 태그: {joined}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _familyHistoryTabContent.AddChild(new HSeparator());

        // 끝난 세대 이력. History.Add는 AfterSummary에서만 일어나므로
        // 세대 요약 화면 시점에는 현재 세대가 아직 포함되지 않는다(의도된 동작).
        var historyHeading = new Label
        {
            Text = _family.IsExtinct ? "가문 전체 이력" : "지난 세대 이력",
        };
        _familyHistoryTabContent.AddChild(historyHeading);

        if (_family.History.Count == 0)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = "[자리표시자: 아직 끝난 세대가 없습니다]",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
            return;
        }

        foreach (var rec in _family.History)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = $"- {MultigenContent.FormatFamilyEra(rec.GenerationNumber)} {rec.CharacterName} ({rec.EndType})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
    }

    // -------------------------------------------------------------------------
    // 화면 메서드 (각 단계)
    // -------------------------------------------------------------------------

    private void ShowGenerationStart()
    {
        RefreshContextHeader("세대 시작", "[자리표시자: 화면 목적 — 새 세대를 시작합니다]");
        ClearMainContent();

        var intro = MultigenContent.GetCharacterIntroLine(_family.CurrentCharacterName, _family.CurrentGeneration);
        _mainContent.AddChild(new Label
        {
            Text = intro,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var startButton = new Button { Text = "첫 사건으로 들어간다" };
        startButton.Pressed += ShowEvent;
        SetActionBar(new Control[] { startButton });

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void ShowEvent()
    {
        RefreshContextHeader("사건 진행", "[자리표시자: 화면 목적 — 사건의 선택을 합니다]");
        ClearMainContent();

        var run = _family.CurrentRun;
        var ev = MvpLoopContent.Events[run.CurrentEventIndex];

        _mainContent.AddChild(new Label
        {
            Text = $"[{run.CurrentEventIndex + 1} / {MvpLoopContent.Events.Count}] {ev.Title}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _mainContent.AddChild(new Label
        {
            Text = ev.Body,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var choiceButtons = new List<Control>();
        foreach (var choice in ev.Choices)
        {
            var captured = choice;
            var button = new Button { Text = captured.Label };
            button.Pressed += () => ApplyChoice(captured);
            choiceButtons.Add(button);
        }
        SetActionBar(choiceButtons);

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void ApplyChoice(EventChoice choice)
    {
        var run = _family.CurrentRun;
        var ev = MvpLoopContent.Events[run.CurrentEventIndex];
        run.ApplyChoice(ev, choice, _family.FamilyStats);
        MultigenFlow.RecordOutcomeTag(_family, choice.OutcomeTag);
        ShowResult(ev, choice);
    }

    private void ShowResult(LoopEvent ev, EventChoice choice)
    {
        _ = ev;
        RefreshContextHeader("결과 확인", "[자리표시자: 화면 목적 — 선택의 결과를 확인합니다]");
        ClearMainContent();

        _mainContent.AddChild(new Label
        {
            Text = $"결과: {choice.ResultText}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        foreach (var delta in choice.Deltas)
        {
            var label = MvpLoopContent.GetStateLabel(delta.Key);
            var sign = delta.Amount >= 0 ? "+" : string.Empty;
            _mainContent.AddChild(new Label
            {
                Text = $"{label} {sign}{delta.Amount}",
            });
        }

        var latest = _family.CurrentRun.Chronicle[^1];
        _mainContent.AddChild(new Label
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
        SetActionBar(new Control[] { nextButton });

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void ShowGenerationSummary()
    {
        RefreshContextHeader("세대 요약", "[자리표시자: 화면 목적 — 세대를 마감합니다]");
        ClearMainContent();

        var endResult = MultigenFlow.FinishCurrentGeneration(_family);
        _pendingEnd = endResult;

        _mainContent.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — {MultigenContent.FormatFamilyEra(_family.CurrentGeneration)} 세대 요약",
        });

        _mainContent.AddChild(new Label { Text = "이번 세대 누적 연대기" });
        foreach (var entry in _family.CurrentRun.Chronicle)
        {
            _mainContent.AddChild(new Label
            {
                Text = $"- ({entry.EventTitle}) {entry.RecordedLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _mainContent.AddChild(new Label { Text = "후대 기록자의 요약" });
        _mainContent.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var nextButton = new Button
        {
            Text = endResult.IsExtinct ? "가문의 끝을 본다" : "후계를 본다",
        };
        nextButton.Pressed += AfterSummary;
        SetActionBar(new Control[] { nextButton });

        // 가문사 탭은 아직 현재 세대를 History에 포함하지 않는다(AfterSummary에서 추가).
        RenderInfoTabs();
        ResetMainScroll();
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
        RefreshContextHeader("후계 선택", "[자리표시자: 화면 목적 — 다음 세대로 이어갈 후계를 정합니다]");
        ClearMainContent();

        _mainContent.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — 후계 후보",
        });

        var actionButtons = new List<Control>();
        foreach (var candidate in endResult.Candidates)
        {
            var captured = candidate;
            _mainContent.AddChild(new Label
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
                _mainContent.AddChild(new Label
                {
                    Text = traitText.ToString(),
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }

            var pickButton = new Button
            {
                Text = endResult.Candidates.Count == 1
                    ? $"{captured.Name}(으)로 다음 세대 진입"
                    : $"{captured.Name}을(를) 후계로 삼는다",
            };
            pickButton.Pressed += () => OnSuccessionPicked(captured);
            actionButtons.Add(pickButton);
        }
        SetActionBar(actionButtons);

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void OnSuccessionPicked(SuccessionCandidate chosen)
    {
        var nextRun = MultigenFlow.BuildNextGenerationRun(_family, chosen);
        MultigenFlow.AdvanceToNextGeneration(_family, chosen, nextRun);
        _pendingEnd = null!;
        ShowGenerationStart();
    }

    private void ShowExtinction(GenerationEndResult endResult)
    {
        _family.MarkExtinct();

        RefreshContextHeader("멸문", "[자리표시자: 화면 목적 — 가문이 끝났습니다]");
        ClearMainContent();

        _mainContent.AddChild(new Label
        {
            Text = $"{_family.FamilyName} — 가문 멸문",
        });

        _mainContent.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _mainContent.AddChild(new Label { Text = "가문 전체 이력" });
        foreach (var rec in _family.History)
        {
            _mainContent.AddChild(new Label
            {
                Text = $"- {MultigenContent.FormatFamilyEra(rec.GenerationNumber)} {rec.CharacterName} ({rec.EndType})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        // 멸문 화면 ActionBar에는 진행 버튼을 두지 않고 비활성 안내만 둔다.
        var endNotice = new Label
        {
            Text = "[자리표시자: 가문이 멸문하여 더 이상 진행할 수 없습니다]",
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        endNotice.Modulate = new Color(1f, 1f, 1f, 0.6f);
        SetActionBar(new Control[] { endNotice });

        RenderInfoTabs();
        ResetMainScroll();
    }

    // -------------------------------------------------------------------------
    // --smoke 경로 (UI 노드를 만들지 않음 — 합류 계약 유지)
    // -------------------------------------------------------------------------

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
