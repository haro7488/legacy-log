// Vertical Slice 1 모델 정의.
//
// 기준 문서:
// - docs/handoff/vertical-slice-1-playtest-build/03-final-work-instruction.md §6.1
// - docs/decisions/014-vertical-slice-1-playtest-scope.md
// - docs/decisions/015-vertical-slice-1-gameplay-content-and-validation.md
// - docs/product/vertical-slice-1-* (이식 원본)
//
// 합류 계약(2026-05-22, vertical-slice-1-playtest-build):
// - VS1 진실 출처는 Vs1FamilyState. 기존 FamilyRunState를 확장하지 않는다.
// - 상태는 4축 내부 점수. UI 기본 표시는 Vs1StateBand 질적 구간.
// - 사건 데이터는 정적 C# 콘텐츠. 외부 JSON/CSV 미도입.
// - 선택지 결과의 X 또는 Y 분기는 Vs1ChoiceOutcomeVariant.
// - 실제 세대 종결은 END-* 사건에서만 발화한다(EndTypeHint).
// - 후계 후보 풀 동적 추가는 VS1 범위 밖. FAM 사건은 태그와 다음 세대 프로필 preset으로 처리.

using System.Collections.Generic;

// ---------------------------------------------------------------------------
// enum
// ---------------------------------------------------------------------------

public enum Vs1StateAxis
{
    Wealth,
    Honor,
    CourtInfluence,
    HouseUnity,
}

public enum Vs1StateBand
{
    Low,
    Strained,
    Stable,
    Strong,
}

public enum Vs1ImpactDirection
{
    Down,
    Up,
}

public enum Vs1ImpactMagnitude
{
    Minor,
    Moderate,
    Major,
}

public enum NobleTitleRank
{
    Baron,
    Viscount,
    Count,
    Marquess,
    Duke,
}

public enum TitleRiskStage
{
    Stable,
    Watched,
    Crisis,
    RevocationThreat,
}

public enum Vs1EventCategory
{
    Estate,
    Honor,
    Court,
    Family,
    Promotion,
    Crisis,
    GenerationEnd,
}

public enum Vs1EventRole
{
    Foundation,
    Pressure,
    TurningPoint,
    GenerationEnd,
}

public enum Vs1EventImportance
{
    Minor,
    Major,
}

public enum Vs1SummaryPriority
{
    Low,
    Medium,
    High,
}

public enum LegacyTagDuration
{
    ShortTerm,
    Generation,
    HouseHistory,
}

public enum Vs1LegacyTagChangeKind
{
    Add,
    Resolve,
    Replace,
    Weaken,
    Strengthen,
}

public enum Vs1ConditionKind
{
    Title,
    StateBand,
    ActiveTag,
    MissingTag,
    Generation,
    HeirTrait,
}

public enum Vs1GenerationEndType
{
    NaturalDeath,
    BattleDeath,
    IllnessDeath,
    ForcedAbdication,
    Extinction,
    Other,
}

// ---------------------------------------------------------------------------
// 사건/선택지/조건 데이터 모델
// ---------------------------------------------------------------------------

public sealed class Vs1EventCondition
{
    public Vs1ConditionKind Kind { get; init; }
    public NobleTitleRank? MinTitle { get; init; }
    public NobleTitleRank? MaxTitle { get; init; }
    public Vs1StateAxis? StateAxis { get; init; }
    public Vs1StateBand? StateBandAtLeast { get; init; }
    public Vs1StateBand? StateBandAtMost { get; init; }
    public string? RequiredTagKey { get; init; }
    public string? ForbiddenTagKey { get; init; }
    public int? MinGeneration { get; init; }
    public string? HeirTraitKey { get; init; }
    public string DisplayReason { get; init; } = string.Empty;
}

public sealed class Vs1WeightModifier
{
    public Vs1ConditionKind TriggerKind { get; init; }
    public string TriggerKey { get; init; } = string.Empty;
    public NobleTitleRank? TriggerTitleAtLeast { get; init; }
    public int Delta { get; init; }
}

public sealed class Vs1StateImpact
{
    public Vs1StateAxis Axis { get; init; }
    public Vs1ImpactDirection Direction { get; init; }
    public Vs1ImpactMagnitude Magnitude { get; init; }
}

public sealed class Vs1LegacyTagChange
{
    public Vs1LegacyTagChangeKind Kind { get; init; }
    public string TagKey { get; init; } = string.Empty;
    public string? ReplacedTagKey { get; init; }
    public LegacyTagDuration? DurationOverride { get; init; }
    public string DisplayText { get; init; } = string.Empty;
}

public sealed class Vs1TitleEffect
{
    public NobleTitleRank? PromoteTo { get; init; }
    public bool LoseRepresentativeTitle { get; init; }
    public TitleRiskStage? RiskStageChange { get; init; }
    public string? ResultSummary { get; init; }
}

// 선택지 결과의 X 또는 Y 분기 표현. 조건이 비면 fallback 후보로 seed 기반 추첨.
public sealed class Vs1ChoiceOutcomeVariant
{
    public IReadOnlyList<Vs1EventCondition> Conditions { get; init; } = System.Array.Empty<Vs1EventCondition>();
    public IReadOnlyList<Vs1StateImpact> ExtraStateImpacts { get; init; } = System.Array.Empty<Vs1StateImpact>();
    public IReadOnlyList<Vs1LegacyTagChange> ExtraTagChanges { get; init; } = System.Array.Empty<Vs1LegacyTagChange>();
    public Vs1TitleEffect? ExtraTitleEffect { get; init; }
    public string? VariantResultText { get; init; }
    public string? VariantChronicleLine { get; init; }
    public int Weight { get; init; } = 1;
}

public sealed class Vs1EventChoice
{
    public string Label { get; init; } = string.Empty;
    public string ImpactPreview { get; init; } = string.Empty;
    public string ResultText { get; init; } = string.Empty;
    public IReadOnlyList<Vs1StateImpact> StateImpacts { get; init; } = System.Array.Empty<Vs1StateImpact>();
    public IReadOnlyList<Vs1LegacyTagChange> TagChanges { get; init; } = System.Array.Empty<Vs1LegacyTagChange>();
    public Vs1TitleEffect? TitleEffect { get; init; }
    public string? NextHeirPresetKey { get; init; }
    public string? ChronicleLineOverride { get; init; }
    public IReadOnlyList<Vs1ChoiceOutcomeVariant> OutcomeVariants { get; init; } = System.Array.Empty<Vs1ChoiceOutcomeVariant>();
    public Vs1GenerationEndType? EndTypeHint { get; init; }
}

public sealed class Vs1EventDefinition
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public Vs1EventCategory Category { get; init; }
    public Vs1EventRole Role { get; init; }
    public Vs1EventImportance Importance { get; init; }
    public Vs1SummaryPriority SummaryPriority { get; init; }
    public string Body { get; init; } = string.Empty;
    public IReadOnlyList<Vs1EventCondition> Conditions { get; init; } = System.Array.Empty<Vs1EventCondition>();
    public IReadOnlyList<Vs1EventChoice> Choices { get; init; } = System.Array.Empty<Vs1EventChoice>();
    public string ChronicleLine { get; init; } = string.Empty;
    public int BaseWeight { get; init; } = 1;
    public IReadOnlyList<Vs1WeightModifier> WeightModifiers { get; init; } = System.Array.Empty<Vs1WeightModifier>();
}

// ---------------------------------------------------------------------------
// 태그/후계자/세대 정보 모델
// ---------------------------------------------------------------------------

public sealed class Vs1LegacyTagDefinition
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string SeriesKey { get; init; } = string.Empty;
    public LegacyTagDuration DefaultDuration { get; init; }
    public IReadOnlyList<string> OpposedTags { get; init; } = System.Array.Empty<string>();
    public string GenerationStartLine { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
}

public sealed class Vs1ActiveLegacyTag
{
    public string Key { get; }
    public LegacyTagDuration Duration { get; set; }
    public int CreatedGeneration { get; }
    public string? OriginEventId { get; set; }

    public Vs1ActiveLegacyTag(string key, LegacyTagDuration duration, int createdGeneration, string? originEventId = null)
    {
        Key = key;
        Duration = duration;
        CreatedGeneration = createdGeneration;
        OriginEventId = originEventId;
    }
}

public sealed class Vs1HeirProfile
{
    public string Name { get; init; } = string.Empty;
    public string DispositionKey { get; init; } = string.Empty;
    public string DispositionLabel { get; init; } = string.Empty;
    public string StrengthKey { get; init; } = string.Empty;
    public string StrengthLabel { get; init; } = string.Empty;
    public string WeaknessKey { get; init; } = string.Empty;
    public string WeaknessLabel { get; init; } = string.Empty;
    public string SuccessionReason { get; init; } = string.Empty;
}

public sealed class Vs1ChronicleEntry
{
    public string EventId { get; }
    public string EventTitle { get; }
    public string ChoiceLabel { get; }
    public string RecordedLine { get; }
    public bool IsMajor { get; }

    public Vs1ChronicleEntry(string eventId, string eventTitle, string choiceLabel, string recordedLine, bool isMajor)
    {
        EventId = eventId;
        EventTitle = eventTitle;
        ChoiceLabel = choiceLabel;
        RecordedLine = recordedLine;
        IsMajor = isMajor;
    }
}

public sealed class Vs1GenerationRun
{
    public List<string> PlannedEventIds { get; }
    public int CurrentEventIndex { get; set; }
    public List<Vs1ChronicleEntry> Chronicle { get; }
    public List<Vs1LegacyTagChange> TagChangesThisGeneration { get; }
    public List<string> MajorEventIds { get; }
    public NobleTitleRank StartTitle { get; }
    public int PromotionsThisGeneration { get; set; }
    public bool TitleLostThisGeneration { get; set; }
    public Vs1GenerationEndType? PendingEndType { get; set; }

    public Vs1GenerationRun(IEnumerable<string> plannedEventIds, NobleTitleRank startTitle)
    {
        PlannedEventIds = new List<string>(plannedEventIds);
        CurrentEventIndex = 0;
        Chronicle = new List<Vs1ChronicleEntry>();
        TagChangesThisGeneration = new List<Vs1LegacyTagChange>();
        MajorEventIds = new List<string>();
        StartTitle = startTitle;
        PromotionsThisGeneration = 0;
        TitleLostThisGeneration = false;
        PendingEndType = null;
    }
}

public sealed class Vs1GenerationRecord
{
    public int GenerationNumber { get; }
    public string CharacterName { get; }
    public NobleTitleRank StartTitle { get; }
    public NobleTitleRank EndTitle { get; }
    public Vs1GenerationEndType EndType { get; }
    public IReadOnlyList<string> MajorEventIds { get; }
    public IReadOnlyList<string> FeaturedLegacyTagKeys { get; }
    public string SummaryParagraph { get; }
    public IReadOnlyList<Vs1ChronicleEntry> Entries { get; }

    public Vs1GenerationRecord(
        int generationNumber,
        string characterName,
        NobleTitleRank startTitle,
        NobleTitleRank endTitle,
        Vs1GenerationEndType endType,
        IReadOnlyList<string> majorEventIds,
        IReadOnlyList<string> featuredLegacyTagKeys,
        string summaryParagraph,
        IReadOnlyList<Vs1ChronicleEntry> entries)
    {
        GenerationNumber = generationNumber;
        CharacterName = characterName;
        StartTitle = startTitle;
        EndTitle = endTitle;
        EndType = endType;
        MajorEventIds = majorEventIds;
        FeaturedLegacyTagKeys = featuredLegacyTagKeys;
        SummaryParagraph = summaryParagraph;
        Entries = entries;
    }
}

public sealed class Vs1GenerationEndResult
{
    public Vs1GenerationEndType EndType { get; }
    public bool IsExtinct { get; }
    public Vs1HeirProfile? NextHeir { get; }
    public string SummaryParagraph { get; }
    public IReadOnlyList<string> FeaturedLegacyTagKeys { get; }
    public NobleTitleRank EndTitle { get; }

    public Vs1GenerationEndResult(
        Vs1GenerationEndType endType,
        bool isExtinct,
        Vs1HeirProfile? nextHeir,
        string summaryParagraph,
        IReadOnlyList<string> featuredLegacyTagKeys,
        NobleTitleRank endTitle)
    {
        EndType = endType;
        IsExtinct = isExtinct;
        NextHeir = nextHeir;
        SummaryParagraph = summaryParagraph;
        FeaturedLegacyTagKeys = featuredLegacyTagKeys;
        EndTitle = endTitle;
    }
}

public sealed class Vs1StateBandLine
{
    public Vs1StateAxis Axis { get; }
    public Vs1StateBand Band { get; }

    public Vs1StateBandLine(Vs1StateAxis axis, Vs1StateBand band)
    {
        Axis = axis;
        Band = band;
    }
}

public sealed class Vs1GenerationStartInfo
{
    public int GenerationNumber { get; }
    public NobleTitleRank Title { get; }
    public TitleRiskStage TitleRisk { get; }
    public string CharacterName { get; }
    public Vs1HeirProfile Profile { get; }
    public string PreviousGenerationLine { get; }
    public IReadOnlyList<Vs1ActiveLegacyTag> FeaturedTags { get; }
    public IReadOnlyList<Vs1StateBandLine> StateBands { get; }
    public string PressureLine { get; }

    public Vs1GenerationStartInfo(
        int generationNumber,
        NobleTitleRank title,
        TitleRiskStage titleRisk,
        string characterName,
        Vs1HeirProfile profile,
        string previousGenerationLine,
        IReadOnlyList<Vs1ActiveLegacyTag> featuredTags,
        IReadOnlyList<Vs1StateBandLine> stateBands,
        string pressureLine)
    {
        GenerationNumber = generationNumber;
        Title = title;
        TitleRisk = titleRisk;
        CharacterName = characterName;
        Profile = profile;
        PreviousGenerationLine = previousGenerationLine;
        FeaturedTags = featuredTags;
        StateBands = stateBands;
        PressureLine = pressureLine;
    }
}

public sealed class Vs1GenerationSummaryInfo
{
    public int GenerationNumber { get; }
    public string CharacterName { get; }
    public NobleTitleRank StartTitle { get; }
    public NobleTitleRank EndTitle { get; }
    public Vs1GenerationEndType EndType { get; }
    public IReadOnlyList<string> FeaturedLegacyTagKeys { get; }
    public IReadOnlyList<string> MajorEventIds { get; }
    public string SummaryParagraph { get; }

    public Vs1GenerationSummaryInfo(
        int generationNumber,
        string characterName,
        NobleTitleRank startTitle,
        NobleTitleRank endTitle,
        Vs1GenerationEndType endType,
        IReadOnlyList<string> featuredLegacyTagKeys,
        IReadOnlyList<string> majorEventIds,
        string summaryParagraph)
    {
        GenerationNumber = generationNumber;
        CharacterName = characterName;
        StartTitle = startTitle;
        EndTitle = endTitle;
        EndType = endType;
        FeaturedLegacyTagKeys = featuredLegacyTagKeys;
        MajorEventIds = majorEventIds;
        SummaryParagraph = summaryParagraph;
    }
}

// ---------------------------------------------------------------------------
// 가문 상태 (mutable)
// ---------------------------------------------------------------------------

public sealed class Vs1FamilyState
{
    public int Seed { get; }
    public string FamilyName { get; }
    public int CurrentGeneration { get; set; }
    public string CurrentCharacterName { get; set; }
    public Vs1HeirProfile CurrentProfile { get; set; }

    // 핵심 상태 4축 내부 점수. 임시값(밸런스 최종 아님).
    public Dictionary<Vs1StateAxis, int> StateScores { get; }

    // 보유 작위 목록. 대표작위 = HeldTitles 최대값.
    public List<NobleTitleRank> HeldTitles { get; }

    public TitleRiskStage TitleRisk { get; set; }

    public List<Vs1ActiveLegacyTag> ActiveTags { get; }

    public Vs1GenerationRun CurrentRun { get; set; }

    public List<Vs1GenerationRecord> History { get; }

    public bool IsExtinct { get; set; }

    // 가장 최근 끝난 세대의 한 줄 요약(다음 세대 시작 화면용).
    public string LastGenerationLine { get; set; }

    // 한 실행에서 같은 큰 사건 반복 방지 추적.
    public HashSet<string> ExperiencedMajorEventIds { get; }

    // FAM-01 등으로 다음 세대 가주 프로필을 미리 정한 경우의 preset key. null이면 기본 풀에서 추첨.
    public string? PendingHeirPresetKey { get; set; }

    public Vs1FamilyState(
        int seed,
        string familyName,
        string firstCharacterName,
        Vs1HeirProfile firstProfile,
        NobleTitleRank startTitle)
    {
        Seed = seed;
        FamilyName = familyName;
        CurrentGeneration = 1;
        CurrentCharacterName = firstCharacterName;
        CurrentProfile = firstProfile;
        StateScores = new Dictionary<Vs1StateAxis, int>
        {
            { Vs1StateAxis.Wealth, 0 },
            { Vs1StateAxis.Honor, 0 },
            { Vs1StateAxis.CourtInfluence, 0 },
            { Vs1StateAxis.HouseUnity, 0 },
        };
        HeldTitles = new List<NobleTitleRank> { startTitle };
        TitleRisk = TitleRiskStage.Stable;
        ActiveTags = new List<Vs1ActiveLegacyTag>();
        CurrentRun = new Vs1GenerationRun(new List<string>(), startTitle);
        History = new List<Vs1GenerationRecord>();
        IsExtinct = false;
        LastGenerationLine = string.Empty;
        ExperiencedMajorEventIds = new HashSet<string>();
        PendingHeirPresetKey = null;
    }

    public NobleTitleRank RepresentativeTitle
    {
        get
        {
            if (HeldTitles.Count == 0)
            {
                return NobleTitleRank.Baron;
            }
            var max = HeldTitles[0];
            for (int i = 1; i < HeldTitles.Count; i++)
            {
                if ((int)HeldTitles[i] > (int)max)
                {
                    max = HeldTitles[i];
                }
            }
            return max;
        }
    }

    public bool HasActiveTag(string key)
    {
        foreach (var t in ActiveTags)
        {
            if (t.Key == key) return true;
        }
        return false;
    }

    public Vs1ActiveLegacyTag? FindActiveTag(string key)
    {
        foreach (var t in ActiveTags)
        {
            if (t.Key == key) return t;
        }
        return null;
    }
}
