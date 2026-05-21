// Vertical Slice 1 진입점 - 3단 UI 구조.
//
// 합류 계약(2026-05-22, vertical-slice-1-ui-layout-improvement):
// - UI 영역은 ContextHeader / GameScroll(GameContent) / BottomTabs 3영역.
// - BottomTabs는 4개 주 화면 전환 메뉴: 사건 / 상태 / 연대기 / 가문사.
// - 별도 InfoTabs 보조 패널과 ActionBar는 없다.
// - 사건 선택지와 진행 버튼은 GameContent 내부 마지막 행동 영역에 둔다.
//
// 렌더링 / mutation 분리 계약 (가장 중요):
// - RefreshAll(), RefreshContextHeader(), RefreshBottomTabs(), RenderSelectedTab()
//   그리고 모든 Render*Screen() 함수는 _family 상태를 변경하지 않는다(읽기 전용 렌더링).
// - Vs1Flow.ApplyChoice, Vs1Flow.FinishCurrentGeneration, Vs1Flow.AdvanceToNextGeneration
//   같은 mutation은 진행 버튼 핸들러(ApplyChoiceAndShowResult, GoToNextEvent,
//   FinishGenerationAndShowSummary, ContinueAfterSummary)에서만 호출한다.
// - 탭 전환은 SelectTab(MainTab)을 통해서만 일어나며 _family를 변경하지 않는다.
// - 이로써 사용자가 탭을 떠났다 돌아와도 같은 화면이 그대로 보이고, 세대 종결이
//   중복 실행되지 않는다.
//
// 헤더 2행 정책:
// - 사건 탭: [단계: <event-screen-label>] <purpose>
// - 그 외 탭: [화면: <탭 이름>] <탭별 설명>
// - 작위 위험 Crisis/RevocationThreat는 모든 탭에서 헤더 끝에 경고로 표시.
//
// --smoke는 UI 노드를 만들지 않는 기능 검증 경로다. Vs1Smoke.Run() 호출.

using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Control
{
    // -------------------------------------------------------------------------
    // 탭/사건 상태 enum
    // -------------------------------------------------------------------------

    private enum MainTab
    {
        Event,
        State,
        Chronicle,
        FamilyHistory,
    }

    private enum EventScreen
    {
        GenerationStart,
        Event,
        Result,
        GenerationSummary,
        Extinction,
    }

    // -------------------------------------------------------------------------
    // 게임 상태
    // -------------------------------------------------------------------------

    private Vs1FamilyState _family = null!;

    // -------------------------------------------------------------------------
    // 3영역 UI 노드
    // -------------------------------------------------------------------------

    private VBoxContainer _contextHeader = null!;
    private Label _contextHeaderLine1 = null!;
    private Label _contextHeaderLine2 = null!;

    private ScrollContainer _gameScroll = null!;
    private VBoxContainer _gameContent = null!;

    private HBoxContainer _bottomTabsContainer = null!;
    private Button _eventTabButton = null!;
    private Button _stateTabButton = null!;
    private Button _chronicleTabButton = null!;
    private Button _familyHistoryTabButton = null!;

    // -------------------------------------------------------------------------
    // 탭/사건 흐름 상태
    // -------------------------------------------------------------------------

    private MainTab _selectedTab = MainTab.Event;
    private EventScreen _eventScreen = EventScreen.GenerationStart;
    private bool _stateDetailVisible;

    // 결과 화면용으로 보존되는 가장 최근 선택 결과.
    private Vs1Flow.ChoiceApplyResult? _lastApplyResult;
    private Vs1EventDefinition? _lastAppliedEvent;
    private Vs1EventChoice? _lastAppliedChoice;

    // 세대 요약 + 멸문 화면용으로 보존되는 세대 종결 결과.
    // FinishGenerationAndShowSummary()에서 한 번만 채워지고,
    // ContinueAfterSummary()가 호출되어 다음 세대로 진입할 때까지 유지된다.
    private Vs1GenerationEndResult? _pendingEnd;

    // -------------------------------------------------------------------------
    // 진입점
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        if (IsSmokeMode())
        {
            Vs1Smoke.Run(GetTree());
            return;
        }

        _family = Vs1Flow.CreateNewFamily();

        BuildShell();

        _selectedTab = MainTab.Event;
        _eventScreen = EventScreen.GenerationStart;
        RefreshAll();
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
    // UI shell — 3영역
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
        BuildGameScroll(appRoot);
        BuildBottomTabs(appRoot);
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

    private void BuildGameScroll(Container parent)
    {
        _gameScroll = new ScrollContainer
        {
            Name = "GameScroll",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        parent.AddChild(_gameScroll);

        _gameContent = new VBoxContainer
        {
            Name = "GameContent",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        _gameContent.AddThemeConstantOverride("separation", 6);
        _gameScroll.AddChild(_gameContent);
    }

    private void BuildBottomTabs(Container parent)
    {
        parent.AddChild(new HSeparator());

        _bottomTabsContainer = new HBoxContainer
        {
            Name = "BottomTabs",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkEnd,
        };
        _bottomTabsContainer.AddThemeConstantOverride("separation", 4);

        _eventTabButton = BuildTabButton("사건", () => SelectTab(MainTab.Event));
        _stateTabButton = BuildTabButton("상태", () => SelectTab(MainTab.State));
        _chronicleTabButton = BuildTabButton("연대기", () => SelectTab(MainTab.Chronicle));
        _familyHistoryTabButton = BuildTabButton("가문사", () => SelectTab(MainTab.FamilyHistory));

        _bottomTabsContainer.AddChild(_eventTabButton);
        _bottomTabsContainer.AddChild(_stateTabButton);
        _bottomTabsContainer.AddChild(_chronicleTabButton);
        _bottomTabsContainer.AddChild(_familyHistoryTabButton);

        parent.AddChild(_bottomTabsContainer);
    }

    private static Button BuildTabButton(string label, System.Action onPressed)
    {
        var btn = new Button
        {
            Text = label,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            ClipText = false,
        };
        btn.Pressed += () => onPressed();
        return btn;
    }

    // -------------------------------------------------------------------------
    // 탭 전환 (mutation 없음)
    // -------------------------------------------------------------------------

    private void SelectTab(MainTab tab)
    {
        _selectedTab = tab;
        RefreshAll();
    }

    // -------------------------------------------------------------------------
    // 단일 갱신 진입점
    // -------------------------------------------------------------------------

    private void RefreshAll()
    {
        RefreshContextHeader();
        RefreshBottomTabs();
        RenderSelectedTab();
        ResetScroll();
    }

    private void ResetScroll()
    {
        _gameScroll.ScrollVertical = 0;
        _gameScroll.ScrollHorizontal = 0;
    }

    private void ClearGameContent()
    {
        foreach (Node child in _gameContent.GetChildren()) child.QueueFree();
    }

    // -------------------------------------------------------------------------
    // 헤더 (읽기 전용 렌더링)
    // -------------------------------------------------------------------------

    private void RefreshContextHeader()
    {
        var title = Vs1Content.GetTitleLabel(_family.RepresentativeTitle);
        _contextHeaderLine1.Text =
            $"{_family.FamilyName} · {title} · {_family.CurrentGeneration}세대 · {_family.CurrentCharacterName}";

        string warning = "";
        if (_family.TitleRisk == TitleRiskStage.Crisis || _family.TitleRisk == TitleRiskStage.RevocationThreat)
        {
            warning = $" · [경고: {Vs1Content.GetTitleRiskLabel(_family.TitleRisk)}]";
        }

        string line2 = _selectedTab switch
        {
            MainTab.Event => $"[단계: {GetEventScreenLabel(_eventScreen)}] {GetEventScreenPurpose(_eventScreen)}",
            MainTab.State => "[화면: 상태] 핵심 상태와 현재 인물을 확인합니다",
            MainTab.Chronicle => "[화면: 연대기] 이번 세대의 기록을 확인합니다",
            MainTab.FamilyHistory => "[화면: 가문사] 유산과 지난 세대 이력을 확인합니다",
            _ => "",
        };
        _contextHeaderLine2.Text = line2 + warning;
    }

    private static string GetEventScreenLabel(EventScreen es) => es switch
    {
        EventScreen.GenerationStart => "세대 시작",
        EventScreen.Event => "사건 진행",
        EventScreen.Result => "결과 확인",
        EventScreen.GenerationSummary => "세대 요약",
        EventScreen.Extinction => "멸문",
        _ => "사건",
    };

    private static string GetEventScreenPurpose(EventScreen es) => es switch
    {
        EventScreen.GenerationStart => "이번 세대의 출발 조건을 확인합니다",
        EventScreen.Event => "사건의 선택을 합니다",
        EventScreen.Result => "선택의 결과를 확인합니다",
        EventScreen.GenerationSummary => "이번 세대를 마감합니다",
        EventScreen.Extinction => "가문이 끝났습니다",
        _ => "",
    };

    // -------------------------------------------------------------------------
    // 하단 탭 버튼 (읽기 전용 렌더링)
    // -------------------------------------------------------------------------

    private void RefreshBottomTabs()
    {
        ConfigureTab(_eventTabButton, "사건", _selectedTab == MainTab.Event);
        ConfigureTab(_stateTabButton, "상태", _selectedTab == MainTab.State);
        ConfigureTab(_chronicleTabButton, "연대기", _selectedTab == MainTab.Chronicle);
        ConfigureTab(_familyHistoryTabButton, "가문사", _selectedTab == MainTab.FamilyHistory);
    }

    private static void ConfigureTab(Button button, string label, bool isActive)
    {
        button.Text = isActive ? $"> {label}" : label;
        button.Disabled = isActive;
    }

    // -------------------------------------------------------------------------
    // 화면 렌더링 dispatch (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderSelectedTab()
    {
        ClearGameContent();
        switch (_selectedTab)
        {
            case MainTab.Event: RenderEventScreen(); break;
            case MainTab.State: RenderStateScreen(); break;
            case MainTab.Chronicle: RenderChronicleScreen(); break;
            case MainTab.FamilyHistory: RenderFamilyHistoryScreen(); break;
        }
    }

    private void RenderEventScreen()
    {
        switch (_eventScreen)
        {
            case EventScreen.GenerationStart: RenderGenerationStart(); break;
            case EventScreen.Event: RenderEventPlay(); break;
            case EventScreen.Result: RenderResult(); break;
            case EventScreen.GenerationSummary: RenderGenerationSummary(); break;
            case EventScreen.Extinction: RenderExtinction(); break;
        }
    }

    // -------------------------------------------------------------------------
    // 사건 탭: 세대 시작 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderGenerationStart()
    {
        var info = Vs1Flow.BuildGenerationStartInfo(_family);

        if (!string.IsNullOrEmpty(info.PreviousGenerationLine))
        {
            _gameContent.AddChild(WrappedLabel($"이전 세대: {info.PreviousGenerationLine}"));
            _gameContent.AddChild(new HSeparator());
        }

        if (info.FeaturedTags.Count > 0)
        {
            _gameContent.AddChild(new Label { Text = "핵심 유산" });
            foreach (var tag in info.FeaturedTags)
            {
                var def = Vs1Content.GetLegacyTagDefinition(tag.Key);
                _gameContent.AddChild(WrappedLabel($"- {Vs1Content.GetLegacyTagLabel(tag.Key)}: {def?.GenerationStartLine ?? ""}"));
            }
            _gameContent.AddChild(new HSeparator());
        }

        _gameContent.AddChild(new Label { Text = "출발 상태" });
        foreach (var bandLine in info.StateBands)
        {
            _gameContent.AddChild(new Label
            {
                Text = $"- {Vs1Content.GetStateLabel(bandLine.Axis)}: {Vs1Content.GetStateBandLabel(bandLine.Band)}",
            });
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(WrappedLabel(
            $"이번 인물: {info.CharacterName} ({info.Profile.DispositionLabel} · 강점 {info.Profile.StrengthLabel} · 약점 {info.Profile.WeaknessLabel})"));
        _gameContent.AddChild(WrappedLabel(info.Profile.SuccessionReason));

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(WrappedLabel(info.PressureLine));

        // 행동 영역
        AddActionSection("행동");
        AddActionButton("첫 사건으로 들어간다", GoToFirstEvent);
    }

    // -------------------------------------------------------------------------
    // 사건 탭: 사건 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderEventPlay()
    {
        var ev = Vs1Flow.GetCurrentEvent(_family);
        if (ev == null)
        {
            // 정상 흐름에서는 도달하지 않는다. 안전 보호 코드.
            _gameContent.AddChild(WrappedLabel("현재 진행할 사건이 없습니다."));
            AddActionSection("행동");
            AddActionButton("세대 요약 보기", FinishGenerationAndShowSummary);
            return;
        }

        _gameContent.AddChild(WrappedLabel(
            $"[{_family.CurrentRun.CurrentEventIndex + 1} / {_family.CurrentRun.PlannedEventIds.Count}] " +
            $"{Vs1Content.GetCategoryLabel(ev.Category)} · {ev.Title}"));
        _gameContent.AddChild(WrappedLabel(ev.Body));

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
                _gameContent.AddChild(WrappedLabel(cluesLine.ToString()));
            }
        }

        // 행동 영역: 선택지 블록 (버튼 + 영향 요약)
        AddActionSection("선택지");
        foreach (var choice in ev.Choices)
        {
            var captured = choice;
            var capturedEv = ev;
            AddChoiceBlock(captured, () => ApplyChoiceAndShowResult(capturedEv, captured));
        }
    }

    private void AddChoiceBlock(Vs1EventChoice choice, System.Action onPressed)
    {
        var box = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        box.AddThemeConstantOverride("separation", 2);

        var btn = new Button { Text = choice.Label };
        btn.Pressed += () => onPressed();
        box.AddChild(btn);

        if (!string.IsNullOrEmpty(choice.ImpactPreview))
        {
            var preview = WrappedLabel(choice.ImpactPreview);
            preview.Modulate = new Color(1f, 1f, 1f, 0.75f);
            box.AddChild(preview);
        }

        _gameContent.AddChild(box);
    }

    // -------------------------------------------------------------------------
    // 사건 탭: 결과 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderResult()
    {
        if (_lastApplyResult == null)
        {
            _gameContent.AddChild(WrappedLabel("표시할 결과가 없습니다."));
            return;
        }

        var result = _lastApplyResult;

        _gameContent.AddChild(WrappedLabel($"결과: {result.ResolvedResultText}"));

        if (result.AppliedImpacts.Count > 0)
        {
            _gameContent.AddChild(new HSeparator());
            _gameContent.AddChild(new Label { Text = "상태 변화" });
            foreach (var imp in result.AppliedImpacts)
            {
                var arrow = imp.Direction == Vs1ImpactDirection.Up ? "↑" : "↓";
                _gameContent.AddChild(new Label
                {
                    Text = $"- {Vs1Content.GetStateLabel(imp.Axis)} {arrow} ({imp.Magnitude})",
                });
            }
        }

        if (result.AppliedTagChanges.Count > 0)
        {
            _gameContent.AddChild(new HSeparator());
            _gameContent.AddChild(new Label { Text = "유산 변화" });
            foreach (var ch in Vs1Flow.SortTagChangesForDisplay(result.AppliedTagChanges))
            {
                _gameContent.AddChild(WrappedLabel($"- {ch.DisplayText}"));
            }
        }

        if (result.AppliedTitleEffect != null)
        {
            _gameContent.AddChild(new HSeparator());
            _gameContent.AddChild(WrappedLabel(
                $"작위 변화: {result.AppliedTitleEffect.ResultSummary ?? ""} " +
                $"(현재 대표작위 {Vs1Content.GetTitleLabel(_family.RepresentativeTitle)}, " +
                $"{Vs1Content.GetTitleRiskLabel(_family.TitleRisk)})"));
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(WrappedLabel($"연대기: {result.ResolvedChronicleLine}"));

        // 행동 영역
        bool hasMore = _family.CurrentRun.CurrentEventIndex < _family.CurrentRun.PlannedEventIds.Count;
        AddActionSection("행동");
        if (hasMore)
        {
            AddActionButton("다음 사건으로", GoToNextEvent);
        }
        else
        {
            AddActionButton("세대 요약 보기", FinishGenerationAndShowSummary);
        }
    }

    // -------------------------------------------------------------------------
    // 사건 탭: 세대 요약 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderGenerationSummary()
    {
        if (_pendingEnd == null)
        {
            // 보호 코드. 정상 흐름에서는 발생하지 않는다.
            _gameContent.AddChild(WrappedLabel("세대 요약 데이터가 없습니다."));
            return;
        }

        var endResult = _pendingEnd;

        _gameContent.AddChild(WrappedLabel(
            $"{_family.FamilyName} · {_family.CurrentGeneration}세대 {_family.CurrentCharacterName}"));
        _gameContent.AddChild(WrappedLabel(
            $"시작 대표작위: {Vs1Content.GetTitleLabel(_family.CurrentRun.StartTitle)} → " +
            $"종료: {Vs1Content.GetTitleLabel(endResult.EndTitle)}"));
        _gameContent.AddChild(new Label
        {
            Text = $"종결 유형: {Vs1Content.GetEndTypeLabel(endResult.EndType)}",
        });

        if (endResult.FeaturedLegacyTagKeys.Count > 0)
        {
            _gameContent.AddChild(WrappedLabel(
                "대표 유산: " + string.Join(", ", endResult.FeaturedLegacyTagKeys.Select(Vs1Content.GetLegacyTagLabel))));
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(WrappedLabel(endResult.SummaryParagraph));

        AddActionSection("행동");
        AddActionButton(
            endResult.IsExtinct ? "가문의 끝을 본다" : "다음 세대로 진입",
            ContinueAfterSummary);
    }

    // -------------------------------------------------------------------------
    // 사건 탭: 멸문 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderExtinction()
    {
        _gameContent.AddChild(new Label { Text = $"{_family.FamilyName} — 가문 멸문" });

        if (_pendingEnd != null)
        {
            _gameContent.AddChild(WrappedLabel(_pendingEnd.SummaryParagraph));
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label { Text = "가문 전체 이력" });
        foreach (var rec in _family.History)
        {
            _gameContent.AddChild(WrappedLabel(
                $"- {rec.GenerationNumber}세대 {rec.CharacterName}: " +
                $"{Vs1Content.GetTitleLabel(rec.StartTitle)} → {Vs1Content.GetTitleLabel(rec.EndTitle)} " +
                $"({Vs1Content.GetEndTypeLabel(rec.EndType)})"));
        }

        // 진행 버튼 없음. 종료 안내만.
        _gameContent.AddChild(new HSeparator());
        var endNotice = WrappedLabel("가문이 멸문하여 더 이상 진행할 수 없습니다.");
        endNotice.Modulate = new Color(1f, 1f, 1f, 0.6f);
        _gameContent.AddChild(endNotice);
    }

    // -------------------------------------------------------------------------
    // 상태 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderStateScreen()
    {
        _gameContent.AddChild(new Label { Text = "핵심 상태 (질적 구간)" });
        foreach (var axisObj in System.Enum.GetValues(typeof(Vs1StateAxis)))
        {
            var a = (Vs1StateAxis)axisObj;
            var band = Vs1Flow.ComputeStateBand(_family.StateScores[a]);
            _gameContent.AddChild(new Label
            {
                Text = $"{Vs1Content.GetStateLabel(a)}: {Vs1Content.GetStateBandLabel(band)}",
            });
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label
        {
            Text = $"작위 위험: {Vs1Content.GetTitleRiskLabel(_family.TitleRisk)}",
        });

        var toggle = new Button
        {
            Text = _stateDetailVisible ? "상세 정보 숨기기" : "상세 정보 보기",
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
        };
        toggle.Pressed += ToggleStateDetail;
        _gameContent.AddChild(toggle);

        if (_stateDetailVisible)
        {
            _gameContent.AddChild(new Label { Text = "[임시 내부 점수]" });
            foreach (var line in Vs1Flow.BuildDetailedStateLines(_family))
            {
                _gameContent.AddChild(new Label { Text = line });
            }
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(WrappedLabel(
            $"현재 인물 특성: {_family.CurrentProfile.DispositionLabel} · " +
            $"강점 {_family.CurrentProfile.StrengthLabel} · " +
            $"약점 {_family.CurrentProfile.WeaknessLabel}"));

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label { Text = $"seed: {_family.Seed}" });
    }

    private void ToggleStateDetail()
    {
        _stateDetailVisible = !_stateDetailVisible;
        // 토글은 상태 탭에 있을 때만 활성이지만 안전하게 전체 갱신.
        RefreshAll();
    }

    // -------------------------------------------------------------------------
    // 연대기 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderChronicleScreen()
    {
        _gameContent.AddChild(new Label { Text = $"{_family.CurrentGeneration}세대 연대기" });

        var chronicle = _family.CurrentRun.Chronicle;
        if (chronicle.Count == 0)
        {
            _gameContent.AddChild(WrappedLabel("아직 기록된 사건이 없습니다."));
            return;
        }

        // 최신 기록 1건
        var latest = chronicle[chronicle.Count - 1];
        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label { Text = "최신 기록" });
        _gameContent.AddChild(WrappedLabel($"▶ [{latest.EventId}] {latest.RecordedLine}"));

        // 큰 사건 / 보조 사건 분리 (최신 제외)
        var majors = new List<Vs1ChronicleEntry>();
        var minors = new List<Vs1ChronicleEntry>();
        for (int i = 0; i < chronicle.Count - 1; i++)
        {
            if (chronicle[i].IsMajor) majors.Add(chronicle[i]);
            else minors.Add(chronicle[i]);
        }

        if (majors.Count > 0)
        {
            _gameContent.AddChild(new HSeparator());
            _gameContent.AddChild(new Label { Text = "큰 사건" });
            foreach (var entry in majors)
            {
                _gameContent.AddChild(WrappedLabel($"● [{entry.EventId}] {entry.RecordedLine}"));
            }
        }

        if (minors.Count > 0)
        {
            _gameContent.AddChild(new HSeparator());
            _gameContent.AddChild(new Label { Text = "보조 사건" });
            foreach (var entry in minors)
            {
                _gameContent.AddChild(WrappedLabel($"· [{entry.EventId}] {entry.RecordedLine}"));
            }
        }
    }

    // -------------------------------------------------------------------------
    // 가문사 화면 (읽기 전용)
    // -------------------------------------------------------------------------

    private void RenderFamilyHistoryScreen()
    {
        _gameContent.AddChild(new Label { Text = "활성 유산 태그" });
        if (_family.ActiveTags.Count == 0)
        {
            _gameContent.AddChild(WrappedLabel("아직 이어진 유산이 없습니다."));
        }
        else
        {
            foreach (var tag in _family.ActiveTags)
            {
                var def = Vs1Content.GetLegacyTagDefinition(tag.Key);
                string desc = def?.ShortDescription ?? "";
                _gameContent.AddChild(WrappedLabel(
                    $"- {Vs1Content.GetLegacyTagLabel(tag.Key)} [{tag.Duration}] {desc}"));
            }
        }

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label { Text = "보유 작위" });
        var titleLine = string.Join(", ", _family.HeldTitles.Select(t => Vs1Content.GetTitleLabel(t)));
        _gameContent.AddChild(WrappedLabel(titleLine));

        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label
        {
            Text = _family.IsExtinct ? "가문 전체 이력" : "지난 세대 이력",
        });

        if (_family.History.Count == 0)
        {
            _gameContent.AddChild(WrappedLabel("아직 끝난 세대가 없습니다."));
            return;
        }

        foreach (var rec in _family.History)
        {
            _gameContent.AddChild(WrappedLabel(
                $"- {rec.GenerationNumber}세대 {rec.CharacterName}: " +
                $"{Vs1Content.GetTitleLabel(rec.StartTitle)} → {Vs1Content.GetTitleLabel(rec.EndTitle)} " +
                $"({Vs1Content.GetEndTypeLabel(rec.EndType)})"));
        }
    }

    // -------------------------------------------------------------------------
    // 사건 흐름 진행 (mutation. 버튼 핸들러에서만 호출)
    // -------------------------------------------------------------------------

    private void GoToFirstEvent()
    {
        _eventScreen = EventScreen.Event;
        _selectedTab = MainTab.Event;
        RefreshAll();
    }

    private void GoToNextEvent()
    {
        _eventScreen = EventScreen.Event;
        _selectedTab = MainTab.Event;
        RefreshAll();
    }

    private void ApplyChoiceAndShowResult(Vs1EventDefinition ev, Vs1EventChoice choice)
    {
        // mutation: Vs1Flow.ApplyChoice는 여기서만 호출된다.
        var result = Vs1Flow.ApplyChoice(_family, ev, choice);
        _lastApplyResult = result;
        _lastAppliedEvent = ev;
        _lastAppliedChoice = choice;

        _eventScreen = EventScreen.Result;
        _selectedTab = MainTab.Event;
        RefreshAll();
    }

    private void FinishGenerationAndShowSummary()
    {
        // mutation: _pendingEnd가 비어 있을 때만 FinishCurrentGeneration을 호출한다.
        // 탭을 떠났다 돌아와도 중복 호출되지 않게 보호한다.
        if (_pendingEnd == null)
        {
            _pendingEnd = Vs1Flow.FinishCurrentGeneration(_family);
        }

        _eventScreen = EventScreen.GenerationSummary;
        _selectedTab = MainTab.Event;
        RefreshAll();
    }

    private void ContinueAfterSummary()
    {
        if (_pendingEnd == null) return;

        var record = Vs1Flow.BuildGenerationRecord(_family, _pendingEnd);

        if (_pendingEnd.IsExtinct)
        {
            _family.History.Add(record);
            _family.IsExtinct = true;
            _eventScreen = EventScreen.Extinction;
            // _pendingEnd는 멸문 화면 렌더링에 필요하므로 유지.
        }
        else
        {
            Vs1Flow.AdvanceToNextGeneration(_family, record, _pendingEnd.NextHeir!);
            _pendingEnd = null;
            _lastApplyResult = null;
            _lastAppliedEvent = null;
            _lastAppliedChoice = null;
            _eventScreen = EventScreen.GenerationStart;
        }

        _selectedTab = MainTab.Event;
        RefreshAll();
    }

    // -------------------------------------------------------------------------
    // 행동 영역 헬퍼
    // -------------------------------------------------------------------------

    private void AddActionSection(string title)
    {
        _gameContent.AddChild(new HSeparator());
        _gameContent.AddChild(new Label { Text = title });
    }

    private void AddActionButton(string text, System.Action onPressed)
    {
        var btn = new Button
        {
            Text = text,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        btn.Pressed += () => onPressed();
        _gameContent.AddChild(btn);
    }

    private static Label WrappedLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.Word,
        };
    }
}
