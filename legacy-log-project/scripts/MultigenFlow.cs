// Multigen MVP 흐름 서비스.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름) §5, §6, §7:
// - 이 파일은 UI 노드를 만들지 않는다. Main.cs가 UI를 그린다.
// - 이 파일은 콘텐츠 문장을 직접 만들지 않는다. MultigenContent가 자리표시자 문장을 제공한다.
// - 상태 거동 매핑(StateCarryKind)은 이 파일에 두며 MVP 검증용 임시 매핑이다.
//   상태 키 셋과 거동 분류는 결정 008의 결정 보류 영역.
// - 질적 구간 라벨은 4단계 자리표시자이며 임계값도 자리표시자다.
// - 후계 판정은 Placeholder* 명명과 코드 주석으로 검증용임을 명확히 한다.
//   후계 가능성 계산식은 결정 008의 결정 보류 영역.
// - 자동 흐름이 발화시키는 종료 유형은 NaturalDeath(1세대) / Extinction(2세대) 2종이다.

using System.Collections.Generic;

public static class MultigenFlow
{
    // MVP 검증용 임시 매핑. 상태 키 셋과 거동 분류는 결정 008의 결정 보류 영역.
    // 매핑이 사실상 규칙으로 굳지 않도록 키와 거동을 자리표시자 셋으로만 둔다.
    private static readonly Dictionary<string, StateCarryKind> StateCarryMap = new()
    {
        { MvpLoopContent.StateKeyReputation, StateCarryKind.Reputation },
        { MvpLoopContent.StateKeyStores, StateCarryKind.Resource },
        { MvpLoopContent.StateKeyCohesion, StateCarryKind.Relation },
    };

    public static FamilyRunState CreateNewFamily()
    {
        return new FamilyRunState(
            familyName: MultigenContent.PlaceholderFamilyName,
            firstCharacterName: MultigenContent.GetFirstCharacterName(),
            familyStats: MvpLoopContent.CreateInitialFamilyStats(),
            firstCharacterTraits: MultigenContent.GetFirstCharacterTraits());
    }

    // 현재 세대 사건 진행이 끝났을 때 호출된다.
    // 후계 판정 + 종료 유형 결정 + 세대 요약 단락 + GenerationRecord 생성을 한 번에 처리한다.
    // 다음 세대 RunState는 BuildNextGenerationRun에서 별도로 만든다.
    public static GenerationEndResult FinishCurrentGeneration(FamilyRunState family)
    {
        // 이번 세대의 큰 사건 결과 태그를 누적에 합친다.
        // OutcomeTag는 EventChoice 단위라 한 세대 안에 여러 개가 쌓일 수 있다.
        foreach (var entry in family.CurrentRun.Chronicle)
        {
            // ChronicleEntry는 OutcomeTag를 직접 들고 있지 않다.
            // OutcomeTag 누적은 ApplyChoice 시점이 아니라 세대 종료 시점에 한 번에 처리한다.
            _ = entry;
        }

        var candidates = PlaceholderEvaluateSuccession(family);

        // 자동 흐름 발화 유형:
        //  - 1세대 종료: NaturalDeath. 후계 후보 1명 이상이면 다음 세대로.
        //  - 2세대 이상 종료: Extinction. 검증용으로 후계 후보를 강제 0명 처리해 멸문 경로 발화.
        // 결정 008: 후계가 있으면 NaturalDeath여도 가문은 이어진다. 후계가 없고 사망/퇴장이면 멸문.
        bool isCurrentCharacterGone = true;
        GenerationEndType endType;
        bool isExtinct;
        IReadOnlyList<SuccessionCandidate> effectiveCandidates;

        if (family.CurrentGeneration == 1)
        {
            endType = GenerationEndType.NaturalDeath;
            effectiveCandidates = candidates;
            isExtinct = false;
        }
        else
        {
            // MVP 검증용 자리표시자 분기: 2세대 종료에서 후계 부재로 멸문 경로를 발화시킨다.
            // 후계 가능성 계산식 확정 아님.
            endType = GenerationEndType.Extinction;
            effectiveCandidates = new List<SuccessionCandidate>();
            isExtinct = true;
        }

        var stateNarrative = MultigenContent.BuildStateNarrative(BuildBandPairsForNarrative(family));
        var hasSuccessor = effectiveCandidates.Count > 0 && !isExtinct;
        var summary = MultigenContent.BuildSummaryParagraph(
            generationNumber: family.CurrentGeneration,
            characterName: family.CurrentCharacterName,
            endType: endType,
            stateNarrativeLine: stateNarrative,
            hasSuccessor: hasSuccessor);

        return new GenerationEndResult(
            endType: endType,
            isCurrentCharacterGone: isCurrentCharacterGone,
            candidates: effectiveCandidates,
            isExtinct: isExtinct,
            summaryParagraph: summary);
    }

    public static GenerationRecord BuildGenerationRecord(FamilyRunState family, GenerationEndResult endResult)
    {
        return new GenerationRecord(
            generationNumber: family.CurrentGeneration,
            characterName: family.CurrentCharacterName,
            endType: endResult.EndType,
            entries: new List<ChronicleEntry>(family.CurrentRun.Chronicle),
            summaryParagraph: endResult.SummaryParagraph);
    }

    public static RunState BuildNextGenerationRun(FamilyRunState family, SuccessionCandidate chosen)
    {
        // 상태 거동 적용: 자리표시자 규칙으로 누적/감쇠/재구성을 표시 수준으로만 처리한다.
        // 감쇠율/재구성 강도는 결정 008의 결정 보류 영역.
        ApplyPlaceholderStateCarry(family.FamilyStats);

        // 다음 세대를 위한 큰 사건 결과 태그 캐리오버.
        // 현재 세대의 OutcomeTag는 family.CurrentRun.Chronicle에 없으므로
        // 세대 종료 시점에 EventChoice 태그를 누적할 필요가 있다면 호출자가 추가한다.
        _ = chosen;

        return new RunState();
    }

    public static void AdvanceToNextGeneration(FamilyRunState family, SuccessionCandidate chosen, RunState nextRun)
    {
        family.AdvanceToNextGeneration(chosen, nextRun);
    }

    // 결정 010: 기본 표시는 4단계 질적 라벨. 라벨 문구와 임계값은 자리표시자.
    // MVP 검증용 임시 임계값. 단계 수/임계값은 결정 보류 영역.
    public static string BuildStateBandLabel(string key, int value)
    {
        _ = key;
        if (value <= -5) return "구간 1";
        if (value <= 0) return "구간 2";
        if (value <= 4) return "구간 3";
        return "구간 4";
    }

    // 결정 010: 추가 정보창은 숫자와 변화 방향을 보여준다.
    // MVP 검증용: 변화 방향은 직전 갱신 비교 없이 부호로만 표시(자리표시자).
    public static IReadOnlyList<string> BuildDetailedStateLines(FamilyRunState family)
    {
        var lines = new List<string>();
        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = family.FamilyStats.TryGetValue(definition.Key, out var v) ? v : 0;
            var direction = value > 0 ? "+" : (value < 0 ? "-" : "0");
            lines.Add($"{definition.Label}: {value} ({direction})");
        }
        return lines;
    }

    // 외부 노출 헬퍼: 한 세대 종료 시 OutcomeTag 누적.
    // Main.cs가 한 사건 적용 직후 호출하면 OutcomeTag가 가문 단위 누적에 반영된다.
    public static void RecordOutcomeTag(FamilyRunState family, string outcomeTag)
    {
        if (!string.IsNullOrEmpty(outcomeTag))
        {
            family.AddCarriedOutcomeTag(outcomeTag);
        }
    }

    // MVP 검증용 임시 로직. 후계 가능성 계산식은 결정 008의 결정 보류 영역.
    // 본 함수는 다세대 흐름이 후계/멸문 양쪽 경로에 모두 도달 가능함을 확인하기 위한 자리표시자다.
    // 입력(가문 상태, 누적 OutcomeTag, 랜덤)을 받을 수 있는 자리만 두고, 실제 분기는 단순화한다.
    private static IReadOnlyList<SuccessionCandidate> PlaceholderEvaluateSuccession(FamilyRunState family)
    {
        // 입력 자리만 확보. 실제 분기 규칙은 두지 않는다.
        _ = family.FamilyStats;
        _ = family.CarriedOutcomeTags;

        // 다음 세대 후보 시드는 MultigenContent에서 제공.
        var nextGen = family.CurrentGeneration + 1;
        return MultigenContent.BuildSuccessionCandidatesForGeneration(nextGen);
    }

    // MVP 검증용 임시 상태 거동. 감쇠율/재구성 강도는 결정 보류 영역.
    private static void ApplyPlaceholderStateCarry(Dictionary<string, int> familyStats)
    {
        // 자리표시자 거동: 매핑된 거동 종류에 따라 표시 수준 변화만 적용한다.
        // 실제 감쇠/재구성 수치는 두지 않는다.
        var keys = new List<string>(familyStats.Keys);
        foreach (var key in keys)
        {
            if (!StateCarryMap.TryGetValue(key, out var kind))
            {
                continue;
            }

            switch (kind)
            {
                case StateCarryKind.Resource:
                    // 누적: 변경 없음.
                    break;
                case StateCarryKind.Reputation:
                    // 자리표시자 감쇠: 절댓값을 1만큼 0 방향으로 끌어당김.
                    if (familyStats[key] > 0) familyStats[key] -= 1;
                    else if (familyStats[key] < 0) familyStats[key] += 1;
                    break;
                case StateCarryKind.Relation:
                    // 자리표시자 재구성: 0 방향으로 절반화(정수 나눗셈).
                    familyStats[key] /= 2;
                    break;
            }
        }
    }

    private static IEnumerable<string> BuildBandPairsForNarrative(FamilyRunState family)
    {
        var pairs = new List<string>();
        foreach (var definition in MvpLoopContent.StateDefinitions)
        {
            var value = family.FamilyStats.TryGetValue(definition.Key, out var v) ? v : 0;
            var band = BuildStateBandLabel(definition.Key, value);
            pairs.Add($"{definition.Label} {band}");
        }
        return pairs;
    }
}
