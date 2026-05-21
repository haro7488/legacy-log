// Vertical Slice 1 사건 — 세대 종결 변형 (END-01~04).
// 실제 세대 종결은 END-* 사건에서만 발화한다. EndTypeHint로 종결 유형을 결정.

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddGenerationEndEvents(List<Vs1EventDefinition> list)
    {
        // END-01 노쇠한 가주의 마지막 겨울
        list.Add(new Vs1EventDefinition
        {
            Id = "END-01",
            Title = "노쇠한 가주의 마지막 겨울",
            Category = Vs1EventCategory.GenerationEnd,
            Role = Vs1EventRole.GenerationEnd,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "겨울이 길어지고 가주의 손은 더 이상 인장을 단단히 쥐지 못한다. 죽음은 가까이 있고, 남은 일은 권한을 어떻게 넘기느냐다.",
            ChronicleLine = "노쇠한 가주의 마지막 겨울에 가문은 권한을 넘길 방식을 정했다.",
            BaseWeight = 2,
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "후계자에게 권한을 넘긴다",
                    ImpactPreview = "안정된 후계 · 상태 안정",
                    ResultText = "가주는 인장을 후계자의 손에 놓았다. 다음 아침의 빛은 다른 손 위에서 시작될 것이다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye) },
                    EndTypeHint = Vs1GenerationEndType.NaturalDeath,
                    ChronicleLineOverride = "가주는 권한을 넘기며 안정된 후계를 남겼다.",
                },
                new Vs1EventChoice
                {
                    Label = "끝까지 직접 통치한다",
                    ImpactPreview = "명예↑ · 후계 불안",
                    ResultText = "가주는 마지막까지 인장을 쥐고 있었다. 권위는 끝까지 갔지만, 후계자의 자리는 그 손이 풀린 뒤에 흔들렸다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagAdd(TagBulAnHanHuGye) },
                    EndTypeHint = Vs1GenerationEndType.NaturalDeath,
                    ChronicleLineOverride = "가주는 끝까지 직접 통치하며 후계 불안을 남겼다.",
                },
            },
        });

        // END-02 전장의 마지막 돌격
        list.Add(new Vs1EventDefinition
        {
            Id = "END-02",
            Title = "전장의 마지막 돌격",
            Category = Vs1EventCategory.GenerationEnd,
            Role = Vs1EventRole.GenerationEnd,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "전투의 깃발이 쓰러지고 적의 창끝이 가까워졌다. 마지막 돌격은 노래가 될 수 있지만, 가문의 다음 아침을 보지 못할 수도 있다.",
            ChronicleLine = "전장의 마지막 돌격은 가주의 이름을 영광 또는 패배로 끝맺었다.",
            Conditions = new[] { CondHasTag(TagJeonJaengGongHun, "전쟁 흐름의 종결") },
            WeightModifiers = new[] { WeightForTag(TagByeonGyeongUiChaekMu, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "돌격한다",
                    ImpactPreview = "전사 가능성 · 전쟁 공훈 또는 잊히지 않는 패배 · 겁쟁이의 소문 해소",
                    ResultText = "가주는 깃발 앞으로 말을 몰았다. 다음 노래는 그 등을 따라갔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagGeopJaengIUiSoMun) },
                    EndTypeHint = Vs1GenerationEndType.BattleDeath,
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeonJaengGongHun) },
                            VariantResultText = "돌격은 적의 깃발을 쓰러뜨렸다. 가주의 이름은 그날의 노래로 남았다.",
                            VariantChronicleLine = "가주의 마지막 돌격은 전쟁 공훈으로 끝맺었다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagItHiJiAnNeunPaeBae) },
                            VariantResultText = "돌격은 적의 창끝에 막혔다. 가주의 이름은 패배의 기록으로 남았다.",
                            VariantChronicleLine = "가주의 마지막 돌격은 잊히지 않는 패배로 끝맺었다.",
                            Weight = 1,
                        },
                    },
                    ChronicleLineOverride = "가주는 마지막 돌격으로 세대를 끝맺었다.",
                },
                new Vs1EventChoice
                {
                    Label = "후방 지휘를 택한다",
                    ImpactPreview = "생존 · 겁쟁이의 소문 가능",
                    ResultText = "가주는 깃발 뒤에 섰다. 부대는 다음 산을 넘었지만, 부하들의 입에는 다른 노래가 흘렀다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    EndTypeHint = Vs1GenerationEndType.NaturalDeath,
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGeopJaengIUiSoMun) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 1 },
                    },
                },
            },
        });

        // END-03 병상 위의 서약
        list.Add(new Vs1EventDefinition
        {
            Id = "END-03",
            Title = "병상 위의 서약",
            Category = Vs1EventCategory.GenerationEnd,
            Role = Vs1EventRole.GenerationEnd,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "병상 곁에 후계자와 가신들이 모였다. 마지막 말은 법보다 오래 남을 수 있고, 침묵은 다음 세대의 분쟁이 된다.",
            ChronicleLine = "병상 위의 서약은 다음 세대가 무엇을 이어받을지 정했다.",
            BaseWeight = 1,
            WeightModifiers = new[] { WeightForTag(TagBulAnHanHuGye, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "후계자를 공개 지명한다",
                    ImpactPreview = "안정된 후계 (교체) · 반발 가능",
                    ResultText = "병상에서 가주의 손가락이 후계자를 가리켰다. 그 손짓은 법보다 단단했다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye) },
                    EndTypeHint = Vs1GenerationEndType.IllnessDeath,
                    ChronicleLineOverride = "가주는 병상에서 후계자를 공개 지명하며 안정된 후계를 남겼다.",
                },
                new Vs1EventChoice
                {
                    Label = "가신들에게 맹세를 받는다",
                    ImpactPreview = "가문의 맹세 · 내부 균열 해소",
                    ResultText = "병상 둘레에서 가신들이 무릎을 꿇었다. 그날의 약속은 가문의 맹세로 새겨졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagGaMunUiMaengSe), TagResolve(TagNaeBuGyunYeol) },
                    EndTypeHint = Vs1GenerationEndType.IllnessDeath,
                },
            },
        });

        // END-04 강제 폐위의 밤
        list.Add(new Vs1EventDefinition
        {
            Id = "END-04",
            Title = "강제 폐위의 밤",
            Category = Vs1EventCategory.GenerationEnd,
            Role = Vs1EventRole.GenerationEnd,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕명이 밤에 도착했다. 인장은 회수되고, 가주의 방 밖에는 이미 낯선 병사들이 서 있다. 가문은 끝나지 않을 수 있지만, 같은 모습으로 남지는 못한다.",
            ChronicleLine = "강제 폐위의 밤에 가문은 작위를 잃고도 이어질 길을 찾았다.",
            Conditions = new[] { CondHasTag(TagBakTalWiGi, "박탈 위기가 활성일 때") },
            WeightModifiers = new[] { WeightForTag(TagMolLakUiJingJo, 2), WeightForStateLow(Vs1StateAxis.HouseUnity, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "왕명을 받아들인다",
                    ImpactPreview = "대표작위 상실 · 후계 가능 · 박탈 위기 해소",
                    ResultText = "가주는 인장을 내려놓았다. 가문은 한 단계 낮은 자리로 내려왔지만, 그 문은 다음 아침까지 열려 있었다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        LoseRepresentativeTitle = true,
                        RiskStageChange = TitleRiskStage.Watched,
                        ResultSummary = "대표작위 상실",
                    },
                    TagChanges = new[] { TagAdd(TagMolLakUiJingJo), TagResolve(TagBakTalWiGi) },
                    EndTypeHint = Vs1GenerationEndType.ForcedAbdication,
                    ChronicleLineOverride = "가문은 왕명을 받아들이며 대표작위를 잃었다.",
                },
                new Vs1EventChoice
                {
                    Label = "도주해 방계에 맡긴다",
                    ImpactPreview = "몰락의 징조 + 피의 원한 · 멸문 위험↑",
                    ResultText = "가주는 밤의 길로 사라졌다. 방계의 손에 가문의 인장이 옮겨졌지만, 그 손은 떨리고 있었다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.HouseUnity, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        LoseRepresentativeTitle = true,
                        RiskStageChange = TitleRiskStage.RevocationThreat,
                        ResultSummary = "도주",
                    },
                    TagChanges = new[] { TagAdd(TagMolLakUiJingJo), TagAdd(TagPiUiWonHan) },
                    EndTypeHint = Vs1GenerationEndType.ForcedAbdication,
                    ChronicleLineOverride = "가주는 도주하며 가문에 몰락의 징조와 피의 원한을 남겼다.",
                },
            },
        });
    }
}
