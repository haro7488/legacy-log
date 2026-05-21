// Vertical Slice 1 진입점.
//
// 코드 기반 동적 UI 생성으로 세대 시작/사건/결과/세대 요약/멸문 흐름을 단일 Control 위에 구성한다.
// Scenes/Main.tscn은 루트 Control + 본 스크립트만 어태치한다.
//
// 합류 계약(2026-05-22, vertical-slice-1-playtest-build):
// - UI 영역은 ContextHeader / MainScroll(MainContent) / InfoTabs / ActionBar 4영역.
// - InfoTabs는 3개 탭(상태/연대기/가문사).
// - 화면 전환 순서: RefreshContextHeader → ClearMainContent → 본문 구성 → SetActionBar
//   → RenderInfoTabs → ResetMainScroll
// - 작위 위험은 Crisis 이상일 때만 헤더에 경고로 표시. Stable/Watched는 상태 탭에 표시.
// - seed는 상태 탭 하단에 표시(헤더에는 넣지 않는다).
// - 멸문 화면 ActionBar에는 진행 버튼을 두지 않고 비활성 안내 라벨만 둔다.
// - --smoke는 UI 노드를 만들지 않는 기능 검증 경로다. Vs1Smoke.Run() 호출.

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Control
{
    private Vs1FamilyState _family = null!;

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

    private bool _stateDetailVisible;

    // 결과 화면 직전 적용 결과(요약 화면 직전까지 보관).
    private Vs1Flow.ChoiceApplyResult? _lastApplyResult;
    private Vs1EventDefinition? _lastEvent;
    private Vs1EventChoice? _lastChoice;

    // 세대 요약 → AfterSummary 사이에 잠시 보관.
    private Vs1GenerationEndResult? _pendingEnd;

    public override void _Ready()
    {
        if (IsSmokeMode())
        {
            Vs1Smoke.Run(GetTree());
            return;
        }

        _family = Vs1Flow.CreateNewFamily();

        BuildShell();
        ShowGenerationStart();
    }

    private static bool IsSmokeMode()
    {
        foreach (var arg in OS.GetCmdlineArgs())
        {
            if (arg == "--smoke") return true;
        }
        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            if (arg == "--smoke") return true;
        }
        return false;
    }

    // -------------------------------------------------------------------------
    // UI shell
    // -------------------------------------------------------------------------

    private void BuildShell()
    {
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 12);
        margin.AddThemeConstantOverride("margin_bottom", 12);
        AddChild(margin);

        var appRoot = new VBoxContainer { Name = "AppRoot" };
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

        _contextHeaderLine1 = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
        _contextHeader.AddChild(_contextHeaderLine1);

        _contextHeaderLine2 = new Label { AutowrapMode = TextServer.AutowrapMode.Word };
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

    private static VBoxContainer AddTabPage(TabContainer tabs, string nodeName, string title)
    {
        var page = new ScrollContainer
        {
            Name = nodeName,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        var inner = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
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
    // 4영역 갱신 단일 진입점
    // -------------------------------------------------------------------------

    private void RefreshContextHeader(string stepLabel, string purposeLine)
    {
        var title = Vs1Content.GetTitleLabel(_family.RepresentativeTitle);
        _contextHeaderLine1.Text = $"{_family.FamilyName} · {title} · {_family.CurrentGeneration}세대 · {_family.CurrentCharacterName}";

        string warning = "";
        if (_family.TitleRisk == TitleRiskStage.Crisis || _family.TitleRisk == TitleRiskStage.RevocationThreat)
        {
            warning = $" · [경고: {Vs1Content.GetTitleRiskLabel(_family.TitleRisk)}]";
        }
        _contextHeaderLine2.Text = $"[단계: {stepLabel}] {purposeLine}{warning}";
    }

    private void ClearMainContent()
    {
        foreach (Node child in _mainContent.GetChildren()) child.QueueFree();
    }

    private void ResetMainScroll()
    {
        _mainScroll.ScrollVertical = 0;
        _mainScroll.ScrollHorizontal = 0;
    }

    private void SetActionBar(IEnumerable<Control> children)
    {
        foreach (Node child in _actionBar.GetChildren()) child.QueueFree();
        foreach (var c in children) _actionBar.AddChild(c);
    }

    // -------------------------------------------------------------------------
    // 탭 렌더링
    // -------------------------------------------------------------------------

    private void RenderInfoTabs()
    {
        RenderStateTab();
        RenderChronicleTab();
        RenderFamilyHistoryTab();
    }

    private void RenderStateTab()
    {
        foreach (Node child in _stateTabContent.GetChildren()) child.QueueFree();

        _stateTabContent.AddChild(new Label { Text = "핵심 상태 (질적 구간)" });
        foreach (var axis in System.Enum.GetValues(typeof(Vs1StateAxis)))
        {
            var a = (Vs1StateAxis)axis;
            var band = Vs1Flow.ComputeStateBand(_family.StateScores[a]);
            _stateTabContent.AddChild(new Label
            {
                Text = $"{Vs1Content.GetStateLabel(a)}: {Vs1Content.GetStateBandLabel(band)}",
            });
        }

        _stateTabContent.AddChild(new HSeparator());
        _stateTabContent.AddChild(new Label
        {
            Text = $"작위 위험: {Vs1Content.GetTitleRiskLabel(_family.TitleRisk)}",
        });

        var toggle = new Button
        {
            Text = _stateDetailVisible ? "상세 정보 숨기기" : "상세 정보 보기",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
        };
        toggle.Pressed += ToggleStateDetail;
        _stateTabContent.AddChild(toggle);

        if (_stateDetailVisible)
        {
            _stateTabContent.AddChild(new Label { Text = "[임시 내부 점수]" });
            foreach (var line in Vs1Flow.BuildDetailedStateLines(_family))
            {
                _stateTabContent.AddChild(new Label { Text = line });
            }
        }

        _stateTabContent.AddChild(new HSeparator());
        _stateTabContent.AddChild(new Label
        {
            Text = $"현재 인물 특성: {_family.CurrentProfile.DispositionLabel} · 강점 {_family.CurrentProfile.StrengthLabel} · 약점 {_family.CurrentProfile.WeaknessLabel}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _stateTabContent.AddChild(new HSeparator());
        _stateTabContent.AddChild(new Label { Text = $"seed: {_family.Seed}" });
    }

    private void ToggleStateDetail()
    {
        _stateDetailVisible = !_stateDetailVisible;
        RenderStateTab();
    }

    private void RenderChronicleTab()
    {
        foreach (Node child in _chronicleTabContent.GetChildren()) child.QueueFree();

        _chronicleTabContent.AddChild(new Label { Text = $"{_family.CurrentGeneration}세대 연대기" });

        var chronicle = _family.CurrentRun.Chronicle;
        if (chronicle.Count == 0)
        {
            _chronicleTabContent.AddChild(new Label
            {
                Text = "아직 기록된 사건이 없습니다.",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
            return;
        }

        for (int i = 0; i < chronicle.Count; i++)
        {
            var entry = chronicle[i];
            var prefix = (i == chronicle.Count - 1) ? "▶" : (entry.IsMajor ? "●" : "·");
            _chronicleTabContent.AddChild(new Label
            {
                Text = $"{prefix} [{entry.EventId}] {entry.RecordedLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
    }

    private void RenderFamilyHistoryTab()
    {
        foreach (Node child in _familyHistoryTabContent.GetChildren()) child.QueueFree();

        _familyHistoryTabContent.AddChild(new Label { Text = "활성 유산 태그" });
        if (_family.ActiveTags.Count == 0)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = "아직 이어진 유산이 없습니다.",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
        else
        {
            foreach (var tag in _family.ActiveTags)
            {
                var def = Vs1Content.GetLegacyTagDefinition(tag.Key);
                string desc = def?.ShortDescription ?? "";
                _familyHistoryTabContent.AddChild(new Label
                {
                    Text = $"- {Vs1Content.GetLegacyTagLabel(tag.Key)} [{tag.Duration}] {desc}",
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }
        }

        _familyHistoryTabContent.AddChild(new HSeparator());
        _familyHistoryTabContent.AddChild(new Label { Text = "작위 이력" });
        var titleLine = string.Join(", ", _family.HeldTitles.Select(t => Vs1Content.GetTitleLabel(t)));
        _familyHistoryTabContent.AddChild(new Label
        {
            Text = $"보유 작위: {titleLine}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _familyHistoryTabContent.AddChild(new HSeparator());
        var historyHeading = new Label
        {
            Text = _family.IsExtinct ? "가문 전체 이력" : "지난 세대 이력",
        };
        _familyHistoryTabContent.AddChild(historyHeading);

        if (_family.History.Count == 0)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = "아직 끝난 세대가 없습니다.",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
            return;
        }

        foreach (var rec in _family.History)
        {
            _familyHistoryTabContent.AddChild(new Label
            {
                Text = $"- {rec.GenerationNumber}세대 {rec.CharacterName}: {Vs1Content.GetTitleLabel(rec.StartTitle)} → {Vs1Content.GetTitleLabel(rec.EndTitle)} ({Vs1Content.GetEndTypeLabel(rec.EndType)})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }
    }

    // -------------------------------------------------------------------------
    // 화면: 세대 시작
    // -------------------------------------------------------------------------

    private void ShowGenerationStart()
    {
        RefreshContextHeader("세대 시작", "이번 세대의 출발 조건을 확인합니다");
        ClearMainContent();

        var info = Vs1Flow.BuildGenerationStartInfo(_family);

        if (!string.IsNullOrEmpty(info.PreviousGenerationLine))
        {
            _mainContent.AddChild(new Label
            {
                Text = $"이전 세대: {info.PreviousGenerationLine}",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
            _mainContent.AddChild(new HSeparator());
        }

        if (info.FeaturedTags.Count > 0)
        {
            _mainContent.AddChild(new Label { Text = "핵심 유산" });
            foreach (var tag in info.FeaturedTags)
            {
                var def = Vs1Content.GetLegacyTagDefinition(tag.Key);
                _mainContent.AddChild(new Label
                {
                    Text = $"- {Vs1Content.GetLegacyTagLabel(tag.Key)}: {def?.GenerationStartLine ?? ""}",
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }
            _mainContent.AddChild(new HSeparator());
        }

        _mainContent.AddChild(new Label { Text = "출발 상태" });
        foreach (var bandLine in info.StateBands)
        {
            _mainContent.AddChild(new Label
            {
                Text = $"- {Vs1Content.GetStateLabel(bandLine.Axis)}: {Vs1Content.GetStateBandLabel(bandLine.Band)}",
            });
        }

        _mainContent.AddChild(new HSeparator());
        _mainContent.AddChild(new Label
        {
            Text = $"이번 인물: {info.CharacterName} ({info.Profile.DispositionLabel} · 강점 {info.Profile.StrengthLabel} · 약점 {info.Profile.WeaknessLabel})",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });
        _mainContent.AddChild(new Label
        {
            Text = info.Profile.SuccessionReason,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _mainContent.AddChild(new HSeparator());
        _mainContent.AddChild(new Label
        {
            Text = info.PressureLine,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var startButton = new Button { Text = "첫 사건으로 들어간다" };
        startButton.Pressed += ShowEvent;
        SetActionBar(new Control[] { startButton });

        RenderInfoTabs();
        ResetMainScroll();
    }

    // -------------------------------------------------------------------------
    // 화면: 사건
    // -------------------------------------------------------------------------

    private void ShowEvent()
    {
        RefreshContextHeader("사건 진행", "사건의 선택을 합니다");
        ClearMainContent();

        var ev = Vs1Flow.GetCurrentEvent(_family);
        if (ev == null)
        {
            ShowGenerationSummary();
            return;
        }

        _mainContent.AddChild(new Label
        {
            Text = $"[{_family.CurrentRun.CurrentEventIndex + 1} / {_family.CurrentRun.PlannedEventIds.Count}] {Vs1Content.GetCategoryLabel(ev.Category)} · {ev.Title}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });
        _mainContent.AddChild(new Label
        {
            Text = ev.Body,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        // 단서: 관련 조건 (최대 3개)
        if (ev.Conditions.Count > 0)
        {
            var cluesLine = new System.Text.StringBuilder("단서: ");
            int shown = 0;
            foreach (var c in ev.Conditions)
            {
                if (shown >= 3) break;
                if (!string.IsNullOrEmpty(c.DisplayReason))
                {
                    if (shown > 0) cluesLine.Append(" · ");
                    cluesLine.Append(c.DisplayReason);
                    shown++;
                }
            }
            if (shown > 0)
            {
                _mainContent.AddChild(new Label
                {
                    Text = cluesLine.ToString(),
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }
        }

        // 선택지
        var choiceButtons = new List<Control>();
        foreach (var choice in ev.Choices)
        {
            var captured = choice;
            var box = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            box.AddThemeConstantOverride("separation", 2);
            var btn = new Button { Text = captured.Label };
            btn.Pressed += () => ApplyChoice(ev, captured);
            box.AddChild(btn);
            if (!string.IsNullOrEmpty(captured.ImpactPreview))
            {
                var preview = new Label
                {
                    Text = captured.ImpactPreview,
                    AutowrapMode = TextServer.AutowrapMode.Word,
                };
                preview.Modulate = new Color(1f, 1f, 1f, 0.75f);
                box.AddChild(preview);
            }
            choiceButtons.Add(box);
        }
        SetActionBar(choiceButtons);

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void ApplyChoice(Vs1EventDefinition ev, Vs1EventChoice choice)
    {
        var result = Vs1Flow.ApplyChoice(_family, ev, choice);
        _lastApplyResult = result;
        _lastEvent = ev;
        _lastChoice = choice;
        ShowResult();
    }

    // -------------------------------------------------------------------------
    // 화면: 결과
    // -------------------------------------------------------------------------

    private void ShowResult()
    {
        RefreshContextHeader("결과 확인", "선택의 결과를 확인합니다");
        ClearMainContent();

        var result = _lastApplyResult!;

        _mainContent.AddChild(new Label
        {
            Text = $"결과: {result.ResolvedResultText}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        if (result.AppliedImpacts.Count > 0)
        {
            _mainContent.AddChild(new HSeparator());
            _mainContent.AddChild(new Label { Text = "상태 변화" });
            foreach (var imp in result.AppliedImpacts)
            {
                var arrow = imp.Direction == Vs1ImpactDirection.Up ? "↑" : "↓";
                _mainContent.AddChild(new Label
                {
                    Text = $"- {Vs1Content.GetStateLabel(imp.Axis)} {arrow} ({imp.Magnitude})",
                });
            }
        }

        if (result.AppliedTagChanges.Count > 0)
        {
            _mainContent.AddChild(new HSeparator());
            _mainContent.AddChild(new Label { Text = "유산 변화" });
            foreach (var ch in Vs1Flow.SortTagChangesForDisplay(result.AppliedTagChanges))
            {
                _mainContent.AddChild(new Label
                {
                    Text = $"- {ch.DisplayText}",
                    AutowrapMode = TextServer.AutowrapMode.Word,
                });
            }
        }

        if (result.AppliedTitleEffect != null)
        {
            _mainContent.AddChild(new HSeparator());
            _mainContent.AddChild(new Label
            {
                Text = $"작위 변화: {result.AppliedTitleEffect.ResultSummary ?? ""} (현재 대표작위 {Vs1Content.GetTitleLabel(_family.RepresentativeTitle)}, {Vs1Content.GetTitleRiskLabel(_family.TitleRisk)})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _mainContent.AddChild(new HSeparator());
        _mainContent.AddChild(new Label
        {
            Text = $"연대기: {result.ResolvedChronicleLine}",
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        bool hasMore = _family.CurrentRun.CurrentEventIndex < _family.CurrentRun.PlannedEventIds.Count;
        var nextButton = new Button { Text = hasMore ? "다음 사건으로" : "세대 요약 보기" };
        nextButton.Pressed += () =>
        {
            if (_family.CurrentRun.CurrentEventIndex < _family.CurrentRun.PlannedEventIds.Count)
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

    // -------------------------------------------------------------------------
    // 화면: 세대 요약
    // -------------------------------------------------------------------------

    private void ShowGenerationSummary()
    {
        RefreshContextHeader("세대 요약", "이번 세대를 마감합니다");
        ClearMainContent();

        var endResult = Vs1Flow.FinishCurrentGeneration(_family);
        _pendingEnd = endResult;

        _mainContent.AddChild(new Label
        {
            Text = $"{_family.FamilyName} · {_family.CurrentGeneration}세대 {_family.CurrentCharacterName}",
        });
        _mainContent.AddChild(new Label
        {
            Text = $"시작 대표작위: {Vs1Content.GetTitleLabel(_family.CurrentRun.StartTitle)} → 종료: {Vs1Content.GetTitleLabel(endResult.EndTitle)}",
        });
        _mainContent.AddChild(new Label
        {
            Text = $"종결 유형: {Vs1Content.GetEndTypeLabel(endResult.EndType)}",
        });

        if (endResult.FeaturedLegacyTagKeys.Count > 0)
        {
            _mainContent.AddChild(new Label
            {
                Text = "대표 유산: " + string.Join(", ", endResult.FeaturedLegacyTagKeys.Select(Vs1Content.GetLegacyTagLabel)),
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        _mainContent.AddChild(new HSeparator());
        _mainContent.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        var nextButton = new Button
        {
            Text = endResult.IsExtinct ? "가문의 끝을 본다" : "다음 세대로 진입",
        };
        nextButton.Pressed += AfterSummary;
        SetActionBar(new Control[] { nextButton });

        RenderInfoTabs();
        ResetMainScroll();
    }

    private void AfterSummary()
    {
        if (_pendingEnd == null) return;

        var record = Vs1Flow.BuildGenerationRecord(_family, _pendingEnd);

        if (_pendingEnd.IsExtinct)
        {
            _family.History.Add(record);
            _family.IsExtinct = true;
            ShowExtinction(_pendingEnd);
        }
        else
        {
            Vs1Flow.AdvanceToNextGeneration(_family, record, _pendingEnd.NextHeir!);
            _pendingEnd = null;
            ShowGenerationStart();
        }
    }

    // -------------------------------------------------------------------------
    // 화면: 멸문
    // -------------------------------------------------------------------------

    private void ShowExtinction(Vs1GenerationEndResult endResult)
    {
        RefreshContextHeader("멸문", "가문이 끝났습니다");
        ClearMainContent();

        _mainContent.AddChild(new Label { Text = $"{_family.FamilyName} — 가문 멸문" });
        _mainContent.AddChild(new Label
        {
            Text = endResult.SummaryParagraph,
            AutowrapMode = TextServer.AutowrapMode.Word,
        });

        _mainContent.AddChild(new HSeparator());
        _mainContent.AddChild(new Label { Text = "가문 전체 이력" });
        foreach (var rec in _family.History)
        {
            _mainContent.AddChild(new Label
            {
                Text = $"- {rec.GenerationNumber}세대 {rec.CharacterName}: {Vs1Content.GetTitleLabel(rec.StartTitle)} → {Vs1Content.GetTitleLabel(rec.EndTitle)} ({Vs1Content.GetEndTypeLabel(rec.EndType)})",
                AutowrapMode = TextServer.AutowrapMode.Word,
            });
        }

        var endNotice = new Label
        {
            Text = "가문이 멸문하여 더 이상 진행할 수 없습니다.",
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
        endNotice.Modulate = new Color(1f, 1f, 1f, 0.6f);
        SetActionBar(new Control[] { endNotice });

        RenderInfoTabs();
        ResetMainScroll();
    }
}
