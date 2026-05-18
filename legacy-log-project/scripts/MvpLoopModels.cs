// MVP First Loop 모델 정의.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름):
// - RunState는 한 세대 사건 진행 상태로 책임을 좁힌다.
//   가문명/현재 인물명/도입 문장/가문 단위 상태(Stats)는
//   FamilyRunState(MultigenModels.cs)가 진실 출처로 보관한다.
// - EventChoice에는 OutcomeTag(string)을 둔다. 결정 008의 "큰 사건 결과"를
//   후계/세대 전환 입력으로 넘기는 최소 표식이며 값은 자리표시자다.
// - RunState.ApplyChoice는 가문 단위 상태 Dictionary를 인자로 받아 갱신한다.
//   이렇게 두면 RunState 자체는 가문 단위 상태를 보관하지 않는다.

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
    public string OutcomeTag { get; }

    public EventChoice(
        string label,
        string resultText,
        string chronicleText,
        IReadOnlyList<StateDelta> deltas,
        string outcomeTag)
    {
        Label = label;
        ResultText = resultText;
        ChronicleText = chronicleText;
        Deltas = deltas;
        OutcomeTag = outcomeTag;
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
    public int CurrentEventIndex { get; private set; }
    public List<ChronicleEntry> Chronicle { get; }

    public RunState()
    {
        CurrentEventIndex = 0;
        Chronicle = new List<ChronicleEntry>();
    }

    public void ApplyChoice(LoopEvent currentEvent, EventChoice choice, Dictionary<string, int> familyStats)
    {
        foreach (var delta in choice.Deltas)
        {
            if (familyStats.ContainsKey(delta.Key))
            {
                familyStats[delta.Key] += delta.Amount;
            }
            else
            {
                familyStats[delta.Key] = delta.Amount;
            }
        }

        Chronicle.Add(new ChronicleEntry(currentEvent.Title, choice.Label, choice.ChronicleText));
        CurrentEventIndex += 1;
    }
}
