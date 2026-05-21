// Vertical Slice 1 흐름 서비스.
//
// 책임:
// - VS1 가문 생성, 세대 시작 정보 구성, 사건 후보 선정, 선택 결과 적용, 세대 종결 판정,
//   다음 세대 생성, 유산 태그 노화, 상태 구간 계산, 태그 변화 표시 정렬.
//
// 합류 계약:
// - UI 노드를 만들지 않는다.
// - 콘텐츠 문장은 Vs1Content에서 가져온다.
// - 상태 변화량 수치(Minor=1, Moderate=2, Major=3)와 구간 임계값은 모두 임시값.
//   최종 밸런스는 Root 후속 라운드에서 정한다.
// - 실제 세대 종결은 END-* 사건의 EndTypeHint에서만 발화한다.
//   Extinction은 END-* 결과 + 후계 부재(또는 강제 분기)에서만 발화한다.

using System;
using System.Collections.Generic;
using System.Linq;

public static class Vs1Flow
{
    // -----------------------------------------------------------------------
    // 상태 점수 ↔ 구간 매핑 (임시값)
    // -----------------------------------------------------------------------

    private const int BandThresholdLow = -3;
    private const int BandThresholdStrained = 0;
    private const int BandThresholdStable = 4;

    public static Vs1StateBand ComputeStateBand(int score)
    {
        if (score <= BandThresholdLow) return Vs1StateBand.Low;
        if (score <= BandThresholdStrained) return Vs1StateBand.Strained;
        if (score <= BandThresholdStable) return Vs1StateBand.Stable;
        return Vs1StateBand.Strong;
    }

    public static int ImpactToDelta(Vs1ImpactDirection direction, Vs1ImpactMagnitude magnitude)
    {
        int mag = magnitude switch
        {
            Vs1ImpactMagnitude.Minor => 1,
            Vs1ImpactMagnitude.Moderate => 2,
            Vs1ImpactMagnitude.Major => 3,
            _ => 1,
        };
        return direction == Vs1ImpactDirection.Up ? mag : -mag;
    }

    // -----------------------------------------------------------------------
    // 가문 생성
    // -----------------------------------------------------------------------

    public static Vs1FamilyState CreateNewFamily(int? seed = null)
    {
        int effectiveSeed = seed ?? Environment.TickCount;
        var family = Vs1Content.BuildInitialFamily(effectiveSeed);
        family.CurrentRun = new Vs1GenerationRun(BuildGenerationPlan(family), family.RepresentativeTitle);
        return family;
    }

    // -----------------------------------------------------------------------
    // 사건 후보 선정
    // -----------------------------------------------------------------------

    public static List<string> BuildGenerationPlan(Vs1FamilyState family)
    {
        // 세대 슬롯: 기반 1~2 + 압력 2~3 + 전환 0~2 + 종결 1 = 5~7
        // 결정 단순화: 6개 슬롯 = 기반 1 + 압력 3 + 전환 1 + 종결 1
        var rng = MakeRng(family, "plan");
        var plan = new List<string>(6);

        var allEvents = Vs1Content.AllEvents;
        var usedIds = new HashSet<string>();
        var usedCategories = new List<Vs1EventCategory>();

        // 슬롯별 선택
        var slots = new (Vs1EventRole Role, int Count)[]
        {
            (Vs1EventRole.Foundation, 1),
            (Vs1EventRole.Pressure, 3),
            (Vs1EventRole.TurningPoint, 1),
        };

        foreach (var (role, count) in slots)
        {
            for (int i = 0; i < count; i++)
            {
                var pick = PickEventForSlot(family, allEvents, role, usedIds, usedCategories, rng);
                if (pick != null)
                {
                    plan.Add(pick.Id);
                    usedIds.Add(pick.Id);
                    usedCategories.Add(pick.Category);
                }
            }
        }

        // 종결 슬롯 = END-*
        var endEvent = PickEventForSlot(family, allEvents, Vs1EventRole.GenerationEnd, usedIds, usedCategories, rng);
        if (endEvent != null)
        {
            plan.Add(endEvent.Id);
            usedIds.Add(endEvent.Id);
        }
        else
        {
            // fallback: END-01
            plan.Add("END-01");
        }

        return plan;
    }

    private static Vs1EventDefinition? PickEventForSlot(
        Vs1FamilyState family,
        IReadOnlyList<Vs1EventDefinition> allEvents,
        Vs1EventRole desiredRole,
        HashSet<string> usedIds,
        List<Vs1EventCategory> usedCategories,
        Random rng)
    {
        var fullyMatched = new List<(Vs1EventDefinition Event, int Weight)>();
        var partiallyMatched = new List<(Vs1EventDefinition Event, int Weight)>();
        var roleMatched = new List<(Vs1EventDefinition Event, int Weight)>();

        foreach (var ev in allEvents)
        {
            if (usedIds.Contains(ev.Id)) continue;
            if (ev.Importance == Vs1EventImportance.Major && family.ExperiencedMajorEventIds.Contains(ev.Id))
            {
                // 한 실행에서 같은 큰 사건 반복은 가능하면 피한다 — partiallyMatched에만 후순위로 둔다.
                if (ev.Role != desiredRole) continue;
            }

            bool roleOk = ev.Role == desiredRole
                || (desiredRole == Vs1EventRole.Pressure && (ev.Role == Vs1EventRole.Pressure || ev.Role == Vs1EventRole.Foundation))
                || (desiredRole == Vs1EventRole.Foundation && ev.Role == Vs1EventRole.Foundation)
                || (desiredRole == Vs1EventRole.TurningPoint && ev.Role == Vs1EventRole.TurningPoint)
                || (desiredRole == Vs1EventRole.GenerationEnd && ev.Role == Vs1EventRole.GenerationEnd);

            if (!roleOk) continue;

            // 같은 분류 3회 연속 금지
            if (IsCategoryThreeInRow(usedCategories, ev.Category)) continue;

            // 종결 슬롯이 아닌데 END-*는 배제
            if (desiredRole != Vs1EventRole.GenerationEnd && ev.Category == Vs1EventCategory.GenerationEnd) continue;
            if (desiredRole == Vs1EventRole.GenerationEnd && ev.Category != Vs1EventCategory.GenerationEnd) continue;

            int weight = ComputeEventWeight(family, ev);
            bool allConditionsMet = AllConditionsMet(family, ev.Conditions);

            if (allConditionsMet)
            {
                fullyMatched.Add((ev, weight + 2));
            }
            else if (AnyConditionMet(family, ev.Conditions))
            {
                partiallyMatched.Add((ev, weight));
            }
            else if (ev.Conditions.Count == 0)
            {
                fullyMatched.Add((ev, weight));
            }
            else
            {
                roleMatched.Add((ev, Math.Max(1, weight - 1)));
            }
        }

        var bucket = fullyMatched.Count > 0 ? fullyMatched
            : (partiallyMatched.Count > 0 ? partiallyMatched : roleMatched);

        return WeightedPick(bucket, rng);
    }

    private static bool IsCategoryThreeInRow(List<Vs1EventCategory> usedCategories, Vs1EventCategory next)
    {
        if (usedCategories.Count < 2) return false;
        var last = usedCategories[usedCategories.Count - 1];
        var prev = usedCategories[usedCategories.Count - 2];
        return last == next && prev == next;
    }

    private static int ComputeEventWeight(Vs1FamilyState family, Vs1EventDefinition ev)
    {
        int weight = ev.BaseWeight;
        foreach (var mod in ev.WeightModifiers)
        {
            if (TriggerMet(family, mod))
            {
                weight += mod.Delta;
            }
        }
        return Math.Max(1, weight);
    }

    private static bool TriggerMet(Vs1FamilyState family, Vs1WeightModifier mod)
    {
        switch (mod.TriggerKind)
        {
            case Vs1ConditionKind.ActiveTag:
                return family.HasActiveTag(mod.TriggerKey);
            case Vs1ConditionKind.Title:
                return mod.TriggerTitleAtLeast.HasValue
                    && (int)family.RepresentativeTitle >= (int)mod.TriggerTitleAtLeast.Value;
            case Vs1ConditionKind.StateBand:
                // mod.TriggerKey 형식: "axis:low"
                var parts = mod.TriggerKey.Split(':');
                if (parts.Length == 2 && Enum.TryParse<Vs1StateAxis>(parts[0], out var axis))
                {
                    int score = family.StateScores[axis];
                    if (parts[1] == "low") return ComputeStateBand(score) <= Vs1StateBand.Strained;
                    if (parts[1] == "high") return ComputeStateBand(score) >= Vs1StateBand.Stable;
                }
                return false;
            default:
                return false;
        }
    }

    private static bool AllConditionsMet(Vs1FamilyState family, IReadOnlyList<Vs1EventCondition> conditions)
    {
        foreach (var c in conditions)
        {
            if (!ConditionMet(family, c)) return false;
        }
        return true;
    }

    private static bool AnyConditionMet(Vs1FamilyState family, IReadOnlyList<Vs1EventCondition> conditions)
    {
        foreach (var c in conditions)
        {
            if (ConditionMet(family, c)) return true;
        }
        return false;
    }

    public static bool ConditionMet(Vs1FamilyState family, Vs1EventCondition c)
    {
        switch (c.Kind)
        {
            case Vs1ConditionKind.Title:
                var rank = family.RepresentativeTitle;
                if (c.MinTitle.HasValue && (int)rank < (int)c.MinTitle.Value) return false;
                if (c.MaxTitle.HasValue && (int)rank > (int)c.MaxTitle.Value) return false;
                return true;
            case Vs1ConditionKind.StateBand:
                if (!c.StateAxis.HasValue) return false;
                int score = family.StateScores[c.StateAxis.Value];
                var band = ComputeStateBand(score);
                if (c.StateBandAtLeast.HasValue && (int)band < (int)c.StateBandAtLeast.Value) return false;
                if (c.StateBandAtMost.HasValue && (int)band > (int)c.StateBandAtMost.Value) return false;
                return true;
            case Vs1ConditionKind.ActiveTag:
                return !string.IsNullOrEmpty(c.RequiredTagKey) && family.HasActiveTag(c.RequiredTagKey);
            case Vs1ConditionKind.MissingTag:
                return !string.IsNullOrEmpty(c.ForbiddenTagKey) && !family.HasActiveTag(c.ForbiddenTagKey);
            case Vs1ConditionKind.Generation:
                return c.MinGeneration.HasValue && family.CurrentGeneration >= c.MinGeneration.Value;
            case Vs1ConditionKind.HeirTrait:
                if (string.IsNullOrEmpty(c.HeirTraitKey)) return false;
                var p = family.CurrentProfile;
                return p.DispositionKey == c.HeirTraitKey
                    || p.StrengthKey == c.HeirTraitKey
                    || p.WeaknessKey == c.HeirTraitKey;
            default:
                return false;
        }
    }

    private static Vs1EventDefinition? WeightedPick(List<(Vs1EventDefinition Event, int Weight)> candidates, Random rng)
    {
        if (candidates.Count == 0) return null;
        int total = 0;
        foreach (var (_, w) in candidates) total += Math.Max(1, w);
        int pick = rng.Next(total);
        int acc = 0;
        foreach (var (ev, w) in candidates)
        {
            acc += Math.Max(1, w);
            if (pick < acc) return ev;
        }
        return candidates[candidates.Count - 1].Event;
    }

    // -----------------------------------------------------------------------
    // RNG
    // -----------------------------------------------------------------------

    private static Random MakeRng(Vs1FamilyState family, string salt)
    {
        // 같은 가문/세대/salt에서는 항상 같은 시퀀스를 만든다(deterministic).
        unchecked
        {
            int hash = family.Seed * 31 + family.CurrentGeneration;
            foreach (char ch in salt) hash = hash * 31 + ch;
            return new Random(hash);
        }
    }

    // -----------------------------------------------------------------------
    // 현재 사건
    // -----------------------------------------------------------------------

    public static Vs1EventDefinition? GetCurrentEvent(Vs1FamilyState family)
    {
        var run = family.CurrentRun;
        if (run.CurrentEventIndex >= run.PlannedEventIds.Count) return null;
        return Vs1Content.GetEventById(run.PlannedEventIds[run.CurrentEventIndex]);
    }

    // -----------------------------------------------------------------------
    // 선택 적용
    // -----------------------------------------------------------------------

    public sealed class ChoiceApplyResult
    {
        public IReadOnlyList<Vs1StateImpact> AppliedImpacts { get; init; } = Array.Empty<Vs1StateImpact>();
        public IReadOnlyList<Vs1LegacyTagChange> AppliedTagChanges { get; init; } = Array.Empty<Vs1LegacyTagChange>();
        public Vs1TitleEffect? AppliedTitleEffect { get; init; }
        public string ResolvedResultText { get; init; } = string.Empty;
        public string ResolvedChronicleLine { get; init; } = string.Empty;
    }

    public static ChoiceApplyResult ApplyChoice(Vs1FamilyState family, Vs1EventDefinition ev, Vs1EventChoice choice)
    {
        var rng = MakeRng(family, "apply:" + ev.Id + ":" + choice.Label);

        // 1. 변종 결정
        Vs1ChoiceOutcomeVariant? variant = SelectVariant(family, choice, rng);

        // 2. 영향 통합
        var impacts = new List<Vs1StateImpact>(choice.StateImpacts);
        var tagChanges = new List<Vs1LegacyTagChange>(choice.TagChanges);
        Vs1TitleEffect? titleEffect = choice.TitleEffect;
        string resultText = choice.ResultText;
        string chronicleLine = choice.ChronicleLineOverride ?? ev.ChronicleLine;

        if (variant != null)
        {
            impacts.AddRange(variant.ExtraStateImpacts);
            tagChanges.AddRange(variant.ExtraTagChanges);
            if (variant.ExtraTitleEffect != null) titleEffect = variant.ExtraTitleEffect;
            if (!string.IsNullOrEmpty(variant.VariantResultText)) resultText = variant.VariantResultText!;
            if (!string.IsNullOrEmpty(variant.VariantChronicleLine)) chronicleLine = variant.VariantChronicleLine!;
        }

        // 3. 상태 적용
        foreach (var imp in impacts)
        {
            int delta = ImpactToDelta(imp.Direction, imp.Magnitude);
            family.StateScores[imp.Axis] += delta;
        }

        // 4. 작위 효과 적용
        if (titleEffect != null)
        {
            ApplyTitleEffect(family, titleEffect);
        }

        // 5. 태그 변화 적용 (실제 효과 있는 변화만 남긴다)
        var appliedTagChanges = new List<Vs1LegacyTagChange>();
        foreach (var change in tagChanges)
        {
            if (TryApplyTagChange(family, change, ev.Id, out var actual))
            {
                appliedTagChanges.Add(actual);
            }
        }

        // 6. 후계 프로필 preset 적용
        if (!string.IsNullOrEmpty(choice.NextHeirPresetKey))
        {
            family.PendingHeirPresetKey = choice.NextHeirPresetKey;
        }

        // 7. 연대기 기록
        var entry = new Vs1ChronicleEntry(ev.Id, ev.Title, choice.Label, chronicleLine, ev.Importance == Vs1EventImportance.Major);
        family.CurrentRun.Chronicle.Add(entry);
        family.CurrentRun.TagChangesThisGeneration.AddRange(appliedTagChanges);

        if (ev.Importance == Vs1EventImportance.Major)
        {
            family.CurrentRun.MajorEventIds.Add(ev.Id);
            family.ExperiencedMajorEventIds.Add(ev.Id);
        }

        // 8. 작위 변화 추적
        if (titleEffect?.PromoteTo != null)
        {
            family.CurrentRun.PromotionsThisGeneration += 1;
        }
        if (titleEffect?.LoseRepresentativeTitle == true)
        {
            family.CurrentRun.TitleLostThisGeneration = true;
        }

        // 9. 종결 힌트 기록
        if (choice.EndTypeHint.HasValue)
        {
            family.CurrentRun.PendingEndType = choice.EndTypeHint;
        }

        // 10. 인덱스 진행
        family.CurrentRun.CurrentEventIndex += 1;

        return new ChoiceApplyResult
        {
            AppliedImpacts = impacts,
            AppliedTagChanges = appliedTagChanges,
            AppliedTitleEffect = titleEffect,
            ResolvedResultText = resultText,
            ResolvedChronicleLine = chronicleLine,
        };
    }

    private static Vs1ChoiceOutcomeVariant? SelectVariant(Vs1FamilyState family, Vs1EventChoice choice, Random rng)
    {
        if (choice.OutcomeVariants.Count == 0) return null;

        // 조건이 있는 변종 중 모든 조건 충족 우선
        var conditional = new List<Vs1ChoiceOutcomeVariant>();
        var fallback = new List<Vs1ChoiceOutcomeVariant>();
        foreach (var v in choice.OutcomeVariants)
        {
            if (v.Conditions.Count == 0) fallback.Add(v);
            else if (AllConditionsMet(family, v.Conditions)) conditional.Add(v);
        }

        var bucket = conditional.Count > 0 ? conditional : fallback;
        if (bucket.Count == 0) bucket = new List<Vs1ChoiceOutcomeVariant>(choice.OutcomeVariants);

        int total = 0;
        foreach (var v in bucket) total += Math.Max(1, v.Weight);
        int pick = rng.Next(total);
        int acc = 0;
        foreach (var v in bucket)
        {
            acc += Math.Max(1, v.Weight);
            if (pick < acc) return v;
        }
        return bucket[bucket.Count - 1];
    }

    private static void ApplyTitleEffect(Vs1FamilyState family, Vs1TitleEffect effect)
    {
        if (effect.PromoteTo.HasValue)
        {
            var promoteTo = effect.PromoteTo.Value;
            if (!family.HeldTitles.Contains(promoteTo))
            {
                family.HeldTitles.Add(promoteTo);
            }
        }
        if (effect.LoseRepresentativeTitle)
        {
            var current = family.RepresentativeTitle;
            family.HeldTitles.Remove(current);
            if (family.HeldTitles.Count == 0)
            {
                // 최소 1개는 남긴다 — 한 단계 아래 (Baron 미만이면 그대로 Baron 유지)
                int below = Math.Max(0, (int)current - 1);
                family.HeldTitles.Add((NobleTitleRank)below);
            }
        }
        if (effect.RiskStageChange.HasValue)
        {
            family.TitleRisk = effect.RiskStageChange.Value;
        }
    }

    private static bool TryApplyTagChange(Vs1FamilyState family, Vs1LegacyTagChange change, string originEventId, out Vs1LegacyTagChange applied)
    {
        applied = change;
        switch (change.Kind)
        {
            case Vs1LegacyTagChangeKind.Add:
                if (family.HasActiveTag(change.TagKey))
                {
                    // 이미 있는 태그는 다시 추가하지 않는다
                    return false;
                }
                // 상반 태그 검사: 상반이 활성이면 Replace로 승격
                var def = Vs1Content.GetLegacyTagDefinition(change.TagKey);
                if (def != null)
                {
                    foreach (var opposed in def.OpposedTags)
                    {
                        if (family.HasActiveTag(opposed))
                        {
                            // 자동 Replace
                            var oldTag = family.FindActiveTag(opposed)!;
                            family.ActiveTags.Remove(oldTag);
                            var newTag = new Vs1ActiveLegacyTag(change.TagKey,
                                change.DurationOverride ?? def.DefaultDuration,
                                family.CurrentGeneration,
                                originEventId);
                            family.ActiveTags.Add(newTag);
                            applied = new Vs1LegacyTagChange
                            {
                                Kind = Vs1LegacyTagChangeKind.Replace,
                                TagKey = change.TagKey,
                                ReplacedTagKey = opposed,
                                DurationOverride = change.DurationOverride,
                                DisplayText = $"교체: {Vs1Content.GetLegacyTagLabel(opposed)} -> {Vs1Content.GetLegacyTagLabel(change.TagKey)}",
                            };
                            return true;
                        }
                    }
                }
                {
                    var duration = change.DurationOverride ?? (def?.DefaultDuration ?? LegacyTagDuration.Generation);
                    family.ActiveTags.Add(new Vs1ActiveLegacyTag(change.TagKey, duration, family.CurrentGeneration, originEventId));
                }
                return true;

            case Vs1LegacyTagChangeKind.Resolve:
                {
                    var existing = family.FindActiveTag(change.TagKey);
                    if (existing == null) return false;
                    family.ActiveTags.Remove(existing);
                    return true;
                }

            case Vs1LegacyTagChangeKind.Replace:
                {
                    var oldTag = !string.IsNullOrEmpty(change.ReplacedTagKey) ? family.FindActiveTag(change.ReplacedTagKey!) : null;
                    if (oldTag != null) family.ActiveTags.Remove(oldTag);
                    if (!family.HasActiveTag(change.TagKey))
                    {
                        var newDef = Vs1Content.GetLegacyTagDefinition(change.TagKey);
                        var duration = change.DurationOverride ?? (newDef?.DefaultDuration ?? LegacyTagDuration.Generation);
                        family.ActiveTags.Add(new Vs1ActiveLegacyTag(change.TagKey, duration, family.CurrentGeneration, originEventId));
                    }
                    return true;
                }

            case Vs1LegacyTagChangeKind.Weaken:
                {
                    var existing = family.FindActiveTag(change.TagKey);
                    if (existing == null) return false;
                    existing.Duration = existing.Duration switch
                    {
                        LegacyTagDuration.HouseHistory => LegacyTagDuration.Generation,
                        LegacyTagDuration.Generation => LegacyTagDuration.ShortTerm,
                        LegacyTagDuration.ShortTerm => LegacyTagDuration.ShortTerm,
                        _ => existing.Duration,
                    };
                    return true;
                }

            case Vs1LegacyTagChangeKind.Strengthen:
                {
                    var existing = family.FindActiveTag(change.TagKey);
                    if (existing == null) return false;
                    existing.Duration = existing.Duration switch
                    {
                        LegacyTagDuration.ShortTerm => LegacyTagDuration.Generation,
                        LegacyTagDuration.Generation => LegacyTagDuration.HouseHistory,
                        LegacyTagDuration.HouseHistory => LegacyTagDuration.HouseHistory,
                        _ => existing.Duration,
                    };
                    return true;
                }

            default:
                return false;
        }
    }

    // -----------------------------------------------------------------------
    // 세대 종결
    // -----------------------------------------------------------------------

    public static Vs1GenerationEndResult FinishCurrentGeneration(Vs1FamilyState family, bool forceExtinction = false)
    {
        var run = family.CurrentRun;

        // 종결 유형: PendingEndType > NaturalDeath fallback
        var endType = run.PendingEndType ?? Vs1GenerationEndType.NaturalDeath;
        var endTitle = family.RepresentativeTitle;

        // 멸문 판정:
        // - forceExtinction 분기(smoke 검증용) 우선
        // - 또는 EndTypeHint가 사망/퇴장 + 후계 부재 결합(현재 VS1에서는 PendingHeirPresetKey가 없는데
        //   ActiveTag에 강한 부정 태그가 결합되면 후계 부재로 본다 — 단순화)
        bool deathOrAbdication = endType == Vs1GenerationEndType.NaturalDeath
            || endType == Vs1GenerationEndType.BattleDeath
            || endType == Vs1GenerationEndType.IllnessDeath
            || endType == Vs1GenerationEndType.ForcedAbdication;

        bool noHeir = forceExtinction
            || (family.HasActiveTag(Vs1Content.TagMolLakUiJingJo)
                && family.HasActiveTag(Vs1Content.TagBakTalWiGi)
                && family.HasActiveTag(Vs1Content.TagBulAnHanHuGye));

        bool isExtinct = deathOrAbdication && noHeir;
        if (isExtinct)
        {
            endType = Vs1GenerationEndType.Extinction;
        }

        var featuredTagKeys = SelectFeaturedTagsForSummary(family);
        var summary = BuildSummaryParagraph(family, run, endType, featuredTagKeys);

        Vs1HeirProfile? nextHeir = isExtinct ? null : BuildNextHeirProfile(family);

        return new Vs1GenerationEndResult(
            endType: endType,
            isExtinct: isExtinct,
            nextHeir: nextHeir,
            summaryParagraph: summary,
            featuredLegacyTagKeys: featuredTagKeys,
            endTitle: endTitle);
    }

    private static List<string> SelectFeaturedTagsForSummary(Vs1FamilyState family)
    {
        var sorted = new List<Vs1ActiveLegacyTag>(family.ActiveTags);
        sorted.Sort((a, b) =>
        {
            int sa = TagSummaryPriority(a.Key);
            int sb = TagSummaryPriority(b.Key);
            return sb.CompareTo(sa);
        });
        var result = new List<string>();
        for (int i = 0; i < sorted.Count && result.Count < 3; i++) result.Add(sorted[i].Key);
        return result;
    }

    private static int TagSummaryPriority(string key) => key switch
    {
        Vs1Content.TagMolLakUiJingJo => 100,
        Vs1Content.TagBakTalWiGi => 95,
        Vs1Content.TagBulAnHanHuGye => 90,
        Vs1Content.TagBulAnHanSeungGyeok => 85,
        Vs1Content.TagByeonGyeongUiChaekMu => 80,
        Vs1Content.TagJeongJeokUiPyoJeok => 75,
        Vs1Content.TagWangSilUiChaeMu => 70,
        Vs1Content.TagPiUiWonHan => 65,
        Vs1Content.TagItHiJiAnNeunPaeBae => 60,
        Vs1Content.TagBulMyeongYe => 55,
        Vs1Content.TagGaMunUiBuChae => 50,
        Vs1Content.TagPiPyeHanYeongJi => 45,
        Vs1Content.TagWangChongAe => 40,
        Vs1Content.TagJeonJaengGongHun => 35,
        Vs1Content.TagAnJeongDoenHuGye => 30,
        Vs1Content.TagPungYoRoUnYeongJi => 25,
        _ => 10,
    };

    private static string BuildSummaryParagraph(Vs1FamilyState family, Vs1GenerationRun run, Vs1GenerationEndType endType, IReadOnlyList<string> featuredTagKeys)
    {
        var startTitle = Vs1Content.GetTitleLabel(run.StartTitle);
        var endTitle = Vs1Content.GetTitleLabel(family.RepresentativeTitle);
        var endLabel = Vs1Content.GetEndTypeLabel(endType);
        var charName = family.CurrentCharacterName;
        var gen = family.CurrentGeneration;

        var titleChange = run.StartTitle == family.RepresentativeTitle
            ? $"{startTitle}의 자리는 그대로 이어졌다"
            : ((int)family.RepresentativeTitle > (int)run.StartTitle
                ? $"가문은 {startTitle}에서 {endTitle}으로 올라섰다"
                : $"가문은 {startTitle}에서 {endTitle}으로 내려왔다");

        var majorBit = run.MajorEventIds.Count > 0
            ? $"이 세대의 가장 큰 기록은 {run.MajorEventIds[0]}였다"
            : "이 세대의 굵직한 기록은 작은 사건들의 누적이었다";

        var legacyBit = featuredTagKeys.Count > 0
            ? "남긴 유산: " + string.Join(", ", featuredTagKeys.Select(Vs1Content.GetLegacyTagLabel))
            : "남긴 유산은 크게 두드러지지 않았다";

        string nextHint;
        if (endType == Vs1GenerationEndType.Extinction)
        {
            nextHint = "후계는 남지 않았고, 가문의 기록은 여기서 끝났다.";
        }
        else
        {
            nextHint = "다음 세대는 이 유산을 안고 다시 시작한다.";
        }

        return $"{gen}세대 {charName}은(는) {startTitle}으로 세대를 열었다. {majorBit}. {titleChange}. {legacyBit}. {endLabel}으로 그의 세대는 끝났고, {nextHint}";
    }

    public static Vs1HeirProfile BuildNextHeirProfile(Vs1FamilyState family)
    {
        var nextGen = family.CurrentGeneration + 1;
        var name = Vs1Content.GetCharacterNameForGeneration(nextGen);
        var presetKey = family.PendingHeirPresetKey ?? Vs1Content.HeirPresetDefault;
        return Vs1Content.BuildHeirProfile(presetKey, name);
    }

    // -----------------------------------------------------------------------
    // 세대 기록 + 다음 세대 진행
    // -----------------------------------------------------------------------

    public static Vs1GenerationRecord BuildGenerationRecord(Vs1FamilyState family, Vs1GenerationEndResult endResult)
    {
        var run = family.CurrentRun;
        return new Vs1GenerationRecord(
            generationNumber: family.CurrentGeneration,
            characterName: family.CurrentCharacterName,
            startTitle: run.StartTitle,
            endTitle: endResult.EndTitle,
            endType: endResult.EndType,
            majorEventIds: new List<string>(run.MajorEventIds),
            featuredLegacyTagKeys: new List<string>(endResult.FeaturedLegacyTagKeys),
            summaryParagraph: endResult.SummaryParagraph,
            entries: new List<Vs1ChronicleEntry>(run.Chronicle));
    }

    public static void AdvanceToNextGeneration(Vs1FamilyState family, Vs1GenerationRecord lastRecord, Vs1HeirProfile nextHeir)
    {
        family.History.Add(lastRecord);
        family.LastGenerationLine = BuildPreviousGenerationLine(lastRecord);

        // 상태 4축 이월: 임시 매핑
        ApplyStateCarry(family);

        // 유산 태그 노화
        AgeLegacyTags(family);

        // 세대/인물 진행
        family.CurrentGeneration += 1;
        family.CurrentCharacterName = nextHeir.Name;
        family.CurrentProfile = nextHeir;
        family.PendingHeirPresetKey = null;

        // 작위 위험 단계는 단계 약화
        family.TitleRisk = family.TitleRisk switch
        {
            TitleRiskStage.RevocationThreat => TitleRiskStage.Crisis,
            TitleRiskStage.Crisis => TitleRiskStage.Watched,
            TitleRiskStage.Watched => TitleRiskStage.Stable,
            _ => TitleRiskStage.Stable,
        };

        // 새 세대 계획
        family.CurrentRun = new Vs1GenerationRun(BuildGenerationPlan(family), family.RepresentativeTitle);
    }

    private static string BuildPreviousGenerationLine(Vs1GenerationRecord record)
    {
        var startTitle = Vs1Content.GetTitleLabel(record.StartTitle);
        var endTitle = Vs1Content.GetTitleLabel(record.EndTitle);
        var endLabel = Vs1Content.GetEndTypeLabel(record.EndType);
        var change = record.StartTitle == record.EndTitle
            ? $"{startTitle}으로 유지"
            : $"{startTitle} → {endTitle}";
        return $"{record.GenerationNumber}세대 {record.CharacterName}: {change}, {endLabel}";
    }

    // -----------------------------------------------------------------------
    // 상태 이월 (임시값)
    // -----------------------------------------------------------------------

    public static void ApplyStateCarry(Vs1FamilyState family)
    {
        // 임시 매핑:
        // - Wealth: 누적 (변경 없음)
        // - Honor: 0 방향으로 1 감쇠
        // - CourtInfluence: 0 방향으로 1 감쇠
        // - HouseUnity: 0 방향으로 절반화
        DecayTowardZero(family, Vs1StateAxis.Honor, 1);
        DecayTowardZero(family, Vs1StateAxis.CourtInfluence, 1);
        family.StateScores[Vs1StateAxis.HouseUnity] /= 2;
    }

    private static void DecayTowardZero(Vs1FamilyState family, Vs1StateAxis axis, int amount)
    {
        int v = family.StateScores[axis];
        if (v > 0) family.StateScores[axis] = Math.Max(0, v - amount);
        else if (v < 0) family.StateScores[axis] = Math.Min(0, v + amount);
    }

    // -----------------------------------------------------------------------
    // 유산 태그 노화
    // -----------------------------------------------------------------------

    public static void AgeLegacyTags(Vs1FamilyState family)
    {
        var survivors = new List<Vs1ActiveLegacyTag>(family.ActiveTags.Count);
        foreach (var tag in family.ActiveTags)
        {
            switch (tag.Duration)
            {
                case LegacyTagDuration.ShortTerm:
                    // 세대 종료 시 소멸
                    break;
                case LegacyTagDuration.Generation:
                    // 등급 한 단계 약화 (다음 세대까지는 ShortTerm으로 유지)
                    tag.Duration = LegacyTagDuration.ShortTerm;
                    survivors.Add(tag);
                    break;
                case LegacyTagDuration.HouseHistory:
                    // 유지
                    survivors.Add(tag);
                    break;
            }
        }
        family.ActiveTags.Clear();
        family.ActiveTags.AddRange(survivors);
    }

    // -----------------------------------------------------------------------
    // 세대 시작 정보
    // -----------------------------------------------------------------------

    public static Vs1GenerationStartInfo BuildGenerationStartInfo(Vs1FamilyState family)
    {
        var stateBands = new List<Vs1StateBandLine>(4);
        foreach (Vs1StateAxis axis in Enum.GetValues(typeof(Vs1StateAxis)))
        {
            stateBands.Add(new Vs1StateBandLine(axis, ComputeStateBand(family.StateScores[axis])));
        }

        var featuredTags = SelectFeaturedTagsForStart(family);
        var pressure = BuildPressureLine(family);

        return new Vs1GenerationStartInfo(
            generationNumber: family.CurrentGeneration,
            title: family.RepresentativeTitle,
            titleRisk: family.TitleRisk,
            characterName: family.CurrentCharacterName,
            profile: family.CurrentProfile,
            previousGenerationLine: family.LastGenerationLine,
            featuredTags: featuredTags,
            stateBands: stateBands,
            pressureLine: pressure);
    }

    private static List<Vs1ActiveLegacyTag> SelectFeaturedTagsForStart(Vs1FamilyState family)
    {
        // 부정/위험 태그 먼저, 나머지는 가문사 영향 큰 순서.
        var sorted = new List<Vs1ActiveLegacyTag>(family.ActiveTags);
        sorted.Sort((a, b) =>
        {
            int sa = TagStartPriority(a.Key);
            int sb = TagStartPriority(b.Key);
            return sb.CompareTo(sa);
        });
        var result = new List<Vs1ActiveLegacyTag>();
        for (int i = 0; i < sorted.Count && result.Count < 4; i++) result.Add(sorted[i]);
        return result;
    }

    private static int TagStartPriority(string key) => key switch
    {
        Vs1Content.TagMolLakUiJingJo => 100,
        Vs1Content.TagBakTalWiGi => 95,
        Vs1Content.TagBulAnHanHuGye => 92,
        Vs1Content.TagBulAnHanSeungGyeok => 88,
        Vs1Content.TagJeongJeokUiPyoJeok => 85,
        Vs1Content.TagGungJeongUiSim => 80,
        Vs1Content.TagPiPyeHanYeongJi => 75,
        Vs1Content.TagGaMunUiBuChae => 70,
        Vs1Content.TagByeonGyeongUiChaekMu => 65,
        Vs1Content.TagPiUiWonHan => 60,
        Vs1Content.TagItHiJiAnNeunPaeBae => 58,
        Vs1Content.TagBulMyeongYe => 55,
        Vs1Content.TagGeopJaengIUiSoMun => 50,
        Vs1Content.TagWangSilUiChaeMu => 48,
        Vs1Content.TagSeungGyeokMyeongBun => 45,
        Vs1Content.TagAnJeongDoenHuGye => 35,
        Vs1Content.TagWangChongAe => 30,
        Vs1Content.TagPungYoRoUnYeongJi => 25,
        Vs1Content.TagJeonJaengGongHun => 22,
        Vs1Content.TagGiSaUiMyeongYe => 20,
        Vs1Content.TagChungSeongRoUnChinJok => 18,
        Vs1Content.TagSangInHuWon => 15,
        Vs1Content.TagGaMunUiMaengSe => 12,
        Vs1Content.TagNaeBuGyunYeol => 50,
        _ => 5,
    };

    private static string BuildPressureLine(Vs1FamilyState family)
    {
        if (family.HasActiveTag(Vs1Content.TagMolLakUiJingJo))
        {
            return "이번 세대는 몰락의 그림자 속에서 다시 일어설 길을 찾아야 한다.";
        }
        if (family.HasActiveTag(Vs1Content.TagBakTalWiGi))
        {
            return "이번 세대의 첫 과제는 대표작위를 지키는 일이다.";
        }
        if (family.HasActiveTag(Vs1Content.TagBulAnHanHuGye))
        {
            return "이번 세대는 후계의 정통성을 증명해야 한다.";
        }
        if (family.HasActiveTag(Vs1Content.TagSeungGyeokMyeongBun))
        {
            return "이번 세대에는 더 높은 자리로 가는 청원의 기회가 보인다.";
        }
        if (family.HasActiveTag(Vs1Content.TagWangChongAe))
        {
            return "이번 세대는 왕실의 호의가 가까이 있는 자리에서 시작한다.";
        }
        return "이번 세대는 평이한 출발선에서 시작한다.";
    }

    // -----------------------------------------------------------------------
    // 헤더/상태/태그 표시 헬퍼
    // -----------------------------------------------------------------------

    public static IReadOnlyList<string> BuildDetailedStateLines(Vs1FamilyState family)
    {
        var lines = new List<string>(4);
        foreach (Vs1StateAxis axis in Enum.GetValues(typeof(Vs1StateAxis)))
        {
            int score = family.StateScores[axis];
            string direction = score > 0 ? "+" : (score < 0 ? "" : "");
            var bandLabel = Vs1Content.GetStateBandLabel(ComputeStateBand(score));
            lines.Add($"{Vs1Content.GetStateLabel(axis)}: {bandLabel} (내부 점수 {direction}{score})");
        }
        return lines;
    }

    public static List<Vs1LegacyTagChange> SortTagChangesForDisplay(IEnumerable<Vs1LegacyTagChange> changes)
    {
        // 우선순위:
        // 1) 새 가문사 태그 추가, 2) 새 세대 태그 추가, 3) 상반 교체, 4) 부정 태그 해소, 5) 단기 추가
        var list = new List<Vs1LegacyTagChange>(changes);
        list.Sort((a, b) =>
        {
            int sa = TagChangeDisplayPriority(a);
            int sb = TagChangeDisplayPriority(b);
            return sa.CompareTo(sb);
        });
        return list;
    }

    private static int TagChangeDisplayPriority(Vs1LegacyTagChange change)
    {
        var def = Vs1Content.GetLegacyTagDefinition(change.TagKey);
        bool isHouseHistory = def?.DefaultDuration == LegacyTagDuration.HouseHistory;
        switch (change.Kind)
        {
            case Vs1LegacyTagChangeKind.Add:
                return isHouseHistory ? 1 : 2;
            case Vs1LegacyTagChangeKind.Replace:
                return 3;
            case Vs1LegacyTagChangeKind.Resolve:
                return 4;
            case Vs1LegacyTagChangeKind.Weaken:
                return 5;
            case Vs1LegacyTagChangeKind.Strengthen:
                return 1;
            default:
                return 9;
        }
    }
}
