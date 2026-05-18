// Multigen MVP 자리표시자 콘텐츠.
//
// 이 파일의 모든 문장, 인물명, 후계 후보 시드, 종료 유형 표시 문장, 후대 기록자 요약
// 템플릿, 특성 스텁 시드는 MVP 검증용 자리표시자다. 최종 콘텐츠나 세계관 톤이 아니다.
//
// 합류 계약(2026-05-18, multigen-mvp 흐름):
// - 후대 기록자 요약 자리표시자는 영광/오욕 같은 평가 어휘를 확정하지 않는다.
// - 시간 표시는 자체 연호 `가문력 N년` 예시를 그대로 쓴다. 최종 명칭은 결정 보류.
// - 특성 스텁은 표시 전용이며 키/라벨 모두 자리표시자다.
//
// 참고:
// - docs/decisions/008-multigen-mvp-scope.md
// - docs/decisions/009-chronicle-style.md
// - docs/decisions/010-state-display-layering.md

using System.Collections.Generic;

public static class MultigenContent
{
    public const string PlaceholderFamilyName = "가문 A";

    public static string GetFirstCharacterName() => "인물 1";

    public static string GetCharacterIntroLine(string characterName, int generationNumber)
    {
        return $"[자리표시자: 도입 문장 — {characterName} ({generationNumber}세대)]";
    }

    // 자리표시자 특성 스텁 시드. 표시 전용.
    public static IReadOnlyList<TraitStub> GetFirstCharacterTraits()
    {
        return new[]
        {
            new TraitStub("trait-1", "특성 1"),
        };
    }

    // 자리표시자 후계 후보 시드. 인물 표시명은 generation 번호로 결정.
    // 첫 후보는 1개로 두지만 합류 계약상 2개 이상이면 선택 화면을 띄울 수 있어야 한다.
    public static IReadOnlyList<SuccessionCandidate> BuildSuccessionCandidatesForGeneration(int nextGenerationNumber)
    {
        // MVP 검증용 임시 시드. 후계 후보 수, 이름, 설명, 특성은 결정 보류.
        var candidate = new SuccessionCandidate(
            name: $"인물 {nextGenerationNumber}",
            description: $"[자리표시자: 후계 후보 설명 — 인물 {nextGenerationNumber}]",
            traits: new[]
            {
                new TraitStub("trait-1", "특성 1"),
            });

        return new[] { candidate };
    }

    // 종료 유형별 자리표시자 표시 문구. 평가 어휘는 두지 않는다.
    public static string GetEndTypeDisplay(GenerationEndType endType)
    {
        return endType switch
        {
            GenerationEndType.NaturalDeath => "[자리표시자: 자연사 요약]",
            GenerationEndType.BattleDeath => "[자리표시자: 전사 요약]",
            GenerationEndType.IllnessDeath => "[자리표시자: 병사 요약]",
            GenerationEndType.Deposed => "[자리표시자: 강제 폐위 요약]",
            GenerationEndType.Extinction => "[자리표시자: 멸문 요약]",
            _ => "[자리표시자: 종료 요약]",
        };
    }

    // 후대 기록자 시점의 한 단락 요약 템플릿.
    // 합류 계약 §8: (a) 이번 세대의 의미, (b) 유산, (c) 다음 세대 상황 암시, (d) 세대 말 상태 서술
    // 을 한 단락에 통합한다. 톤은 서정·중립 자리표시자.
    public static string BuildSummaryParagraph(
        int generationNumber,
        string characterName,
        GenerationEndType endType,
        string stateNarrativeLine,
        bool hasSuccessor)
    {
        var meaningLine = $"[자리표시자: {generationNumber}세대 {characterName}의 시대 의미]";
        var legacyLine = $"[자리표시자: {generationNumber}세대가 남긴 유산]";
        var endLine = GetEndTypeDisplay(endType);
        var nextHintLine = hasSuccessor
            ? "[자리표시자: 다음 세대가 직면할 상황 암시]"
            : "[자리표시자: 가문이 더는 이어지지 않는다]";

        return string.Join(" ", new[]
        {
            meaningLine,
            legacyLine,
            endLine,
            stateNarrativeLine,
            nextHintLine,
        });
    }

    // 자체 연호 자리표시자. 결정 009의 예시 `가문력 N년`을 그대로 사용한다.
    public static string FormatFamilyEra(int generationNumber)
    {
        return $"가문력 {generationNumber}년";
    }

    // 세대 말 상태의 짧은 서술 자리표시자. 결정 010의 "세대 끝 서술 요약"을
    // 후대 기록자 요약 단락(BuildSummaryParagraph)에 통합하기 위한 자리표시자.
    public static string BuildStateNarrative(IEnumerable<string> bandLines)
    {
        var joined = string.Join(", ", bandLines);
        return $"[자리표시자: 세대 말 상태 서술 — {joined}]";
    }
}
