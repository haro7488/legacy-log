// MVP First Loop 시드 콘텐츠.
//
// 이 파일에 등장하는 상태 키, 상태 표시명, 가문명, 인물명, 도입 문장,
// 사건 제목/본문, 선택지 문구, 결과 문구, 연대기 문장, 수치 변화는
// 모두 MVP 검증용 임시 값이다. 최종 콘텐츠나 밸런스로 확정하지 않는다.
//
// 참고:
// - docs/decisions/004-mvp-starting-premise.md (이름과 문안 의도적 미확정)
// - docs/decisions/005-mvp-event-result-feedback.md (서사 + 연대기 + 최소 상태 변화)
// - docs/decisions/006-mvp-first-loop-product-criteria.md (사건 3개, 선택지 2개 이상)

using System.Collections.Generic;

public static class MvpLoopContent
{
    // MVP 검증용 임시 상태 키. 최종 상태 모델 아님.
    public const string StateKeyReputation = "Reputation";
    public const string StateKeyStores = "Stores";
    public const string StateKeyCohesion = "Cohesion";

    public static IReadOnlyList<StateDefinition> StateDefinitions { get; } = new[]
    {
        new StateDefinition(StateKeyReputation, "명망"),
        new StateDefinition(StateKeyStores, "비축"),
        new StateDefinition(StateKeyCohesion, "결속"),
    };

    public static IReadOnlyList<LoopEvent> Events { get; } = new[]
    {
        new LoopEvent(
            title: "낯선 사절의 방문",
            body: "이웃 영지에서 사절이 찾아와 우호의 표시로 곡식을 청한다. 가문이 자리 잡은 첫 해, 첫 외교 자리다.",
            choices: new[]
            {
                new EventChoice(
                    label: "곡식 일부를 내어 우호를 표시한다.",
                    resultText: "사절은 만족하여 돌아갔다. 비축이 줄었지만 가문의 이름이 이웃에 알려졌다.",
                    chronicleText: "첫 해, 가문은 곡식을 나누어 이웃의 이름에 우리 이름을 새겼다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyReputation, +2),
                        new StateDelta(StateKeyStores, -1),
                    }),
                new EventChoice(
                    label: "겨울이 길다며 청을 거절한다.",
                    resultText: "사절은 굳은 얼굴로 돌아갔다. 곳간은 지켰으나 이웃의 시선이 차가워졌다.",
                    chronicleText: "첫 해, 가문은 곳간을 지키기로 했다. 이웃의 인사는 짧아졌다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyReputation, -1),
                        new StateDelta(StateKeyStores, +1),
                    }),
            }),
        new LoopEvent(
            title: "마을의 다툼",
            body: "두 가신 가문이 경계의 우물을 두고 다툰다. 양쪽 모두 가문에 충성을 약속해 왔다.",
            choices: new[]
            {
                new EventChoice(
                    label: "직접 중재해 양쪽에 양보를 요구한다.",
                    resultText: "두 가신은 마지못해 손을 잡았다. 결속은 단단해졌지만 양쪽 모두 약간의 불만을 남겼다.",
                    chronicleText: "가문주는 우물 곁에서 두 가신의 손을 맞붙였다. 결속은 흔들리지 않았다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyCohesion, +2),
                        new StateDelta(StateKeyReputation, +1),
                    }),
                new EventChoice(
                    label: "한쪽 편을 들어 빠르게 매듭짓는다.",
                    resultText: "다툼은 빨리 끝났지만, 패한 가신의 사람들은 등을 돌렸다.",
                    chronicleText: "가문주는 한쪽 손을 들었다. 우물은 잠잠해졌지만 결속은 갈라졌다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyCohesion, -2),
                        new StateDelta(StateKeyStores, +1),
                    }),
            }),
        new LoopEvent(
            title: "첫 추수의 밤",
            body: "첫 해의 추수가 끝났다. 가신들은 잔치를 청하고, 곳간지기는 다음 겨울을 걱정한다.",
            choices: new[]
            {
                new EventChoice(
                    label: "잔치를 열어 가신들과 어울린다.",
                    resultText: "잔치의 불빛은 길게 이어졌다. 가신들의 얼굴은 밝았으나 곳간은 가벼워졌다.",
                    chronicleText: "첫 추수의 밤, 가문은 불빛 아래 함께 웃었다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyCohesion, +2),
                        new StateDelta(StateKeyStores, -2),
                    }),
                new EventChoice(
                    label: "곳간을 채우고 잔치는 미룬다.",
                    resultText: "곳간은 든든해졌지만, 가신들의 인사는 짧고 무거웠다.",
                    chronicleText: "첫 추수의 밤, 가문은 곳간 문 앞에 등을 켰다.",
                    deltas: new[]
                    {
                        new StateDelta(StateKeyCohesion, -1),
                        new StateDelta(StateKeyStores, +2),
                    }),
            }),
    };

    public static RunState CreateInitialRunState()
    {
        var stats = new Dictionary<string, int>
        {
            { StateKeyReputation, 0 },
            { StateKeyStores, 0 },
            { StateKeyCohesion, 0 },
        };

        return new RunState(
            familyName: "하서가",
            founderName: "하서 단",
            introLine: "하서가의 단이 처음으로 가문의 첫 장을 연다.",
            stats: stats);
    }

    public static string GetStateLabel(string key)
    {
        foreach (var definition in StateDefinitions)
        {
            if (definition.Key == key)
            {
                return definition.Label;
            }
        }

        return key;
    }
}
