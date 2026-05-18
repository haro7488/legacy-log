// Multigen MVP 모델 정의.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름):
// - 가문 단위 진실 출처는 FamilyRunState다. 가문명, 가문 단위 상태(FamilyStats),
//   현재 세대 인물명, 끝난 세대 이력(History), 큰 사건 결과 누적(CarriedOutcomeTags),
//   현재 인물 특성 스텁, 멸문 여부를 보관한다.
// - 한 세대 사건 진행 상태는 RunState(MvpLoopModels.cs)가 보관한다.
//   RunState는 CurrentEventIndex와 Chronicle(현재 세대 항목식 기록)만 가진다.
// - TraitStub은 표시 전용이다. MultigenFlow의 후계/상태/사건/멸문 어떤 로직 입력에도 쓰이지 않는다.
// - GenerationEndType은 5종을 정의하지만, 자동 흐름이 실제 발화시키는 유형은
//   NaturalDeath(후계 있음 1세대)와 Extinction(후계 없음 2세대) 2종이다.
//   BattleDeath/IllnessDeath/Deposed는 자리만 확보한다. 트리거 시스템은 만들지 않는다.

using System.Collections.Generic;

public enum GenerationEndType
{
    Extinction,
    NaturalDeath,
    BattleDeath,
    IllnessDeath,
    Deposed,
}

public enum StateCarryKind
{
    Resource,   // 누적
    Reputation, // 누적되지만 시간이 지나며 감쇠
    Relation,   // 인물 교체 시 부분 재구성
}

public sealed class TraitStub
{
    public string Key { get; }
    public string Label { get; }

    public TraitStub(string key, string label)
    {
        Key = key;
        Label = label;
    }
}

public sealed class GenerationRecord
{
    public int GenerationNumber { get; }
    public string CharacterName { get; }
    public GenerationEndType EndType { get; }
    public IReadOnlyList<ChronicleEntry> Entries { get; }
    public string SummaryParagraph { get; }

    public GenerationRecord(
        int generationNumber,
        string characterName,
        GenerationEndType endType,
        IReadOnlyList<ChronicleEntry> entries,
        string summaryParagraph)
    {
        GenerationNumber = generationNumber;
        CharacterName = characterName;
        EndType = endType;
        Entries = entries;
        SummaryParagraph = summaryParagraph;
    }
}

public sealed class SuccessionCandidate
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<TraitStub> Traits { get; }

    public SuccessionCandidate(string name, string description, IReadOnlyList<TraitStub> traits)
    {
        Name = name;
        Description = description;
        Traits = traits;
    }
}

public sealed class GenerationEndResult
{
    public GenerationEndType EndType { get; }
    public bool IsCurrentCharacterGone { get; }
    public IReadOnlyList<SuccessionCandidate> Candidates { get; }
    public bool IsExtinct { get; }
    public string SummaryParagraph { get; }

    public GenerationEndResult(
        GenerationEndType endType,
        bool isCurrentCharacterGone,
        IReadOnlyList<SuccessionCandidate> candidates,
        bool isExtinct,
        string summaryParagraph)
    {
        EndType = endType;
        IsCurrentCharacterGone = isCurrentCharacterGone;
        Candidates = candidates;
        IsExtinct = isExtinct;
        SummaryParagraph = summaryParagraph;
    }
}

public sealed class FamilyRunState
{
    public string FamilyName { get; }
    public int CurrentGeneration { get; private set; }
    public string CurrentCharacterName { get; private set; }
    public RunState CurrentRun { get; private set; }
    public Dictionary<string, int> FamilyStats { get; }
    public List<GenerationRecord> History { get; }
    public List<string> CarriedOutcomeTags { get; }
    public List<TraitStub> CurrentCharacterTraits { get; }
    public bool IsExtinct { get; private set; }

    public FamilyRunState(
        string familyName,
        string firstCharacterName,
        Dictionary<string, int> familyStats,
        IReadOnlyList<TraitStub> firstCharacterTraits)
    {
        FamilyName = familyName;
        CurrentGeneration = 1;
        CurrentCharacterName = firstCharacterName;
        CurrentRun = new RunState();
        FamilyStats = familyStats;
        History = new List<GenerationRecord>();
        CarriedOutcomeTags = new List<string>();
        CurrentCharacterTraits = new List<TraitStub>(firstCharacterTraits);
        IsExtinct = false;
    }

    public void RecordCurrentGenerationEnd(GenerationRecord record)
    {
        History.Add(record);
    }

    public void AdvanceToNextGeneration(SuccessionCandidate chosen, RunState nextRun)
    {
        CurrentGeneration += 1;
        CurrentCharacterName = chosen.Name;
        CurrentCharacterTraits.Clear();
        foreach (var trait in chosen.Traits)
        {
            CurrentCharacterTraits.Add(trait);
        }
        CurrentRun = nextRun;
    }

    public void MarkExtinct()
    {
        IsExtinct = true;
    }

    public void AddCarriedOutcomeTag(string tag)
    {
        CarriedOutcomeTags.Add(tag);
    }
}
