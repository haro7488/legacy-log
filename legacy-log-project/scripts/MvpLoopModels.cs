// MVP First Loop 모델 정의.
// 본 타입들은 docs/mvp-first-loop-final-work-instruction.md의 합류 계약에 따른다.
// 콘텐츠와 분리되어 있다(시드 데이터는 MvpLoopContent.cs).

using System.Collections.Generic;

public sealed class StateDefinition
{
    public string Key { get; }
    public string Label { get; }

    public StateDefinition(string key, string label)
    {
        Key = key;
        Label = label;
    }
}

public sealed class StateDelta
{
    public string Key { get; }
    public int Amount { get; }

    public StateDelta(string key, int amount)
    {
        Key = key;
        Amount = amount;
    }
}

public sealed class EventChoice
{
    public string Label { get; }
    public string ResultText { get; }
    public string ChronicleText { get; }
    public IReadOnlyList<StateDelta> Deltas { get; }

    public EventChoice(string label, string resultText, string chronicleText, IReadOnlyList<StateDelta> deltas)
    {
        Label = label;
        ResultText = resultText;
        ChronicleText = chronicleText;
        Deltas = deltas;
    }
}

public sealed class LoopEvent
{
    public string Title { get; }
    public string Body { get; }
    public IReadOnlyList<EventChoice> Choices { get; }

    public LoopEvent(string title, string body, IReadOnlyList<EventChoice> choices)
    {
        Title = title;
        Body = body;
        Choices = choices;
    }
}

public sealed class ChronicleEntry
{
    public string EventTitle { get; }
    public string ChoiceLabel { get; }
    public string RecordedLine { get; }

    public ChronicleEntry(string eventTitle, string choiceLabel, string recordedLine)
    {
        EventTitle = eventTitle;
        ChoiceLabel = choiceLabel;
        RecordedLine = recordedLine;
    }
}

public sealed class RunState
{
    public string FamilyName { get; }
    public string FounderName { get; }
    public string IntroLine { get; }
    public int CurrentEventIndex { get; private set; }
    public Dictionary<string, int> Stats { get; }
    public List<ChronicleEntry> Chronicle { get; }

    public RunState(
        string familyName,
        string founderName,
        string introLine,
        Dictionary<string, int> stats)
    {
        FamilyName = familyName;
        FounderName = founderName;
        IntroLine = introLine;
        CurrentEventIndex = 0;
        Stats = stats;
        Chronicle = new List<ChronicleEntry>();
    }

    public void ApplyChoice(LoopEvent currentEvent, EventChoice choice)
    {
        foreach (var delta in choice.Deltas)
        {
            if (Stats.ContainsKey(delta.Key))
            {
                Stats[delta.Key] += delta.Amount;
            }
            else
            {
                Stats[delta.Key] = delta.Amount;
            }
        }

        Chronicle.Add(new ChronicleEntry(currentEvent.Title, choice.Label, choice.ChronicleText));
        CurrentEventIndex += 1;
    }
}
