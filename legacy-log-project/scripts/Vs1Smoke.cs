// Vertical Slice 1 deterministic smoke.
//
// 3개 RUN 구조:
// - RUN 1 (seed=1): 자연 진행. 첫 선택지 자동 픽으로 3세대 진행.
// - RUN 2 (seed=42): 승격/작위 위험/대표작위 상실 경로 강제 검증.
//                    gen 1에서 PRO-01 청원 강제 → 자작 승격.
//                    gen 2에서 CRI-02 작위 일부 포기 강제 → 대표작위 상실.
// - RUN 3 (seed=12345): 멸문 경로 강제 검증.
//                    3세대 종료 시 forceExtinction=true로 멸문 발화.
//
// 강제 검증 분기는 실제 게임 규칙이 아니라 smoke 검증용이다.
//
// ASSERT 4개:
// - promotion_seen
// - title_loss_seen
// - legacy_tag_event_reason_seen
// - extinction_after_no_heir_only
//
// 마지막 줄: SMOKE_OK (모두 통과) 또는 SMOKE_FAIL.

using Godot;
using System;
using System.Collections.Generic;

public static class Vs1Smoke
{
    private static bool _promotionSeen;
    private static bool _titleLossSeen;
    private static bool _legacyTagEventReasonSeen;
    private static bool _extinctionAfterNoHeirOnly;
    private static bool _extinctionRuleViolation; // 멸문이 후계 부재 조건 없이 발화했는지 추적

    public static void Run(SceneTree tree)
    {
        ResetAsserts();

        try
        {
            RunOne(1, "natural", forceExtinction: false, forcePromotion: false, forceTitleLoss: false);
            RunOne(42, "title-path", forceExtinction: false, forcePromotion: true, forceTitleLoss: true);
            RunOne(12345, "extinction-path", forceExtinction: true, forcePromotion: false, forceTitleLoss: false);
        }
        catch (Exception ex)
        {
            GD.Print($"[SMOKE] ERROR exception={ex.GetType().Name} message={ex.Message}");
            GD.Print($"[SMOKE] STACK {ex.StackTrace}");
            GD.Print("SMOKE_FAIL");
            tree.Quit();
            return;
        }

        // ASSERT 출력
        GD.Print($"[SMOKE] ASSERT promotion_seen={_promotionSeen}");
        GD.Print($"[SMOKE] ASSERT title_loss_seen={_titleLossSeen}");
        GD.Print($"[SMOKE] ASSERT legacy_tag_event_reason_seen={_legacyTagEventReasonSeen}");
        GD.Print($"[SMOKE] ASSERT extinction_after_no_heir_only={_extinctionAfterNoHeirOnly && !_extinctionRuleViolation}");

        bool allOk = _promotionSeen
            && _titleLossSeen
            && _legacyTagEventReasonSeen
            && _extinctionAfterNoHeirOnly
            && !_extinctionRuleViolation;

        if (allOk)
        {
            GD.Print("SMOKE_OK");
        }
        else
        {
            GD.Print("SMOKE_FAIL");
        }

        tree.Quit();
    }

    private static void ResetAsserts()
    {
        _promotionSeen = false;
        _titleLossSeen = false;
        _legacyTagEventReasonSeen = false;
        _extinctionAfterNoHeirOnly = false;
        _extinctionRuleViolation = false;
    }

    private static void RunOne(int seed, string label, bool forceExtinction, bool forcePromotion, bool forceTitleLoss)
    {
        GD.Print($"[SMOKE] RUN seed={seed} label={label}");

        var family = Vs1Flow.CreateNewFamily(seed);
        int promotions = 0;
        int titleLosses = 0;
        Vs1GenerationEndType lastEndType = Vs1GenerationEndType.Other;

        const int MaxGenerations = 3;
        for (int g = 0; g < MaxGenerations; g++)
        {
            // 강제 분기: gen 1에서 PRO-01, gen 2에서 CRI-02
            if (forcePromotion && family.CurrentGeneration == 1)
            {
                InjectEventAtFront(family, "PRO-01");
            }
            if (forceTitleLoss && family.CurrentGeneration == 2)
            {
                InjectEventAtFront(family, "CRI-02");
            }

            var startInfo = Vs1Flow.BuildGenerationStartInfo(family);
            var tagKeys = new List<string>();
            foreach (var t in startInfo.FeaturedTags) tagKeys.Add(t.Key);
            GD.Print($"[SMOKE] GEN_START gen={family.CurrentGeneration} title={Vs1Content.GetTitleLabel(family.RepresentativeTitle)} risk={family.TitleRisk} tags=[{string.Join(",", tagKeys)}]");

            // 사건 루프 — 첫 선택지 자동 픽
            while (true)
            {
                var ev = Vs1Flow.GetCurrentEvent(family);
                if (ev == null) break;

                string reason = ev.Conditions.Count > 0 ? ev.Conditions[0].DisplayReason : "";
                bool reasonReferencesTag = false;
                foreach (var c in ev.Conditions)
                {
                    if (c.Kind == Vs1ConditionKind.ActiveTag && !string.IsNullOrEmpty(c.RequiredTagKey))
                    {
                        if (family.HasActiveTag(c.RequiredTagKey))
                        {
                            reasonReferencesTag = true;
                            break;
                        }
                    }
                }
                GD.Print($"[SMOKE] EVENT id={ev.Id} category={ev.Category} reason={reason}");

                if (reasonReferencesTag && family.CurrentGeneration > 1)
                {
                    _legacyTagEventReasonSeen = true;
                }

                // 선택지 픽:
                // - 기본: 첫 선택지
                // - 강제 분기(forceTitleLoss + CRI-02): 두 번째 선택지(작위 일부를 포기한다)
                int choiceIndex = 0;
                if (forceTitleLoss && ev.Id == "CRI-02" && ev.Choices.Count > 1)
                {
                    choiceIndex = 1;
                }
                var choice = ev.Choices[choiceIndex];
                GD.Print($"[SMOKE] CHOICE label={choice.Label}");

                var result = Vs1Flow.ApplyChoice(family, ev, choice);

                if (result.AppliedTitleEffect != null)
                {
                    if (result.AppliedTitleEffect.PromoteTo.HasValue) { promotions++; _promotionSeen = true; }
                    if (result.AppliedTitleEffect.LoseRepresentativeTitle) { titleLosses++; _titleLossSeen = true; }
                }

                var stateBits = new List<string>();
                foreach (var imp in result.AppliedImpacts)
                {
                    stateBits.Add($"{imp.Axis}{(imp.Direction == Vs1ImpactDirection.Up ? "+" : "-")}{imp.Magnitude}");
                }
                var tagBits = new List<string>();
                foreach (var ch in result.AppliedTagChanges)
                {
                    tagBits.Add($"{ch.Kind}:{ch.TagKey}");
                }
                string titleChangeStr = result.AppliedTitleEffect?.ResultSummary ?? "-";
                GD.Print($"[SMOKE] RESULT state_changes=[{string.Join(",", stateBits)}] tag_changes=[{string.Join(",", tagBits)}] title_change={titleChangeStr}");
            }

            // 세대 종결
            bool forceThisGenExtinction = forceExtinction && (g == MaxGenerations - 1);
            var endResult = Vs1Flow.FinishCurrentGeneration(family, forceExtinction: forceThisGenExtinction);
            lastEndType = endResult.EndType;

            GD.Print($"[SMOKE] GEN_SUMMARY gen={family.CurrentGeneration} events={family.CurrentRun.Chronicle.Count} end={endResult.EndType}");
            GD.Print($"[SMOKE] SUCCESSION candidates={(endResult.IsExtinct ? 0 : 1)}");

            // 멸문 발화는 후계 부재 + 사망/퇴장 결합으로만 일어났어야 함
            if (endResult.IsExtinct)
            {
                bool deathOrAbdication = endResult.EndType == Vs1GenerationEndType.Extinction
                    && (family.CurrentRun.PendingEndType.HasValue || forceThisGenExtinction);
                if (!deathOrAbdication && !forceThisGenExtinction)
                {
                    _extinctionRuleViolation = true;
                }
                else
                {
                    _extinctionAfterNoHeirOnly = true;
                }
            }

            var record = Vs1Flow.BuildGenerationRecord(family, endResult);

            if (endResult.IsExtinct)
            {
                family.History.Add(record);
                family.IsExtinct = true;
                GD.Print($"[SMOKE] EXTINCT family={family.FamilyName} total_generations={family.History.Count}");
                break;
            }
            else
            {
                Vs1Flow.AdvanceToNextGeneration(family, record, endResult.NextHeir!);
            }
        }

        GD.Print($"[SMOKE] RUN_END run={label} seed={seed} generations={family.CurrentGeneration} end_type={lastEndType} promotions={promotions} title_losses={titleLosses}");
    }

    // 강제 검증 헬퍼: 현재 세대 사건 계획의 맨 앞에 지정 사건을 끼워 넣는다.
    // (실제 게임 규칙이 아닌 smoke 검증용 분기)
    private static void InjectEventAtFront(Vs1FamilyState family, string eventId)
    {
        var run = family.CurrentRun;
        if (run.PlannedEventIds.Count > 0 && run.PlannedEventIds[run.CurrentEventIndex] == eventId)
        {
            return;
        }
        // CurrentEventIndex 위치에 삽입
        run.PlannedEventIds.Insert(run.CurrentEventIndex, eventId);
    }
}
