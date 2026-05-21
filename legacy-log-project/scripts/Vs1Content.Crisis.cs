// Vertical Slice 1 사건 — 작위 상실/위기/반역 의심 (CRI-01~05).

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddCrisisEvents(List<Vs1EventDefinition> list)
    {
        // CRI-01 반역 혐의 소환 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRI-01",
            Title = "반역 혐의 소환",
            Category = Vs1EventCategory.Crisis,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕의 인장이 찍힌 소환장이 도착했다. 궁정은 가문이 반역을 꾀했다는 소문을 묻고 있으며, 출두하지 않으면 의심은 곧 죄가 된다.",
            ChronicleLine = "반역 혐의 소환은 가문의 작위와 충성을 궁정의 심판대에 올렸다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Count, "백작 이상의 정치 위기") },
            WeightModifiers = new[] { WeightForTag(TagGungJeongUiSim, 2), WeightForTag(TagJeongJeokUiPyoJeok, 2) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "궁정으로 간다",
                    ImpactPreview = "궁정↑로 방어 · 박탈 위기 또는 궁정의 의심 해소",
                    ResultText = "가문은 왕도의 회의장에 섰다. 답변은 길었고, 결과는 그 자리에서 결정되지 않았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagResolve(TagGungJeongUiSim) },
                            VariantResultText = "긴 답변이 의심을 풀었다. 가문의 이름은 회의장에서 다시 깨끗해졌다.",
                            VariantChronicleLine = "가문은 궁정 앞에서 의심을 풀어냈다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraStateImpacts = new[] { Down(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate) },
                            ExtraTitleEffect = new Vs1TitleEffect { RiskStageChange = TitleRiskStage.Crisis, ResultSummary = "작위 위기" },
                            ExtraTagChanges = new[] { TagAdd(TagBakTalWiGi) },
                            VariantResultText = "회의장의 답은 차가웠다. 작위는 흔들렸고, 박탈의 그림자가 가까워졌다.",
                            VariantChronicleLine = "가문은 궁정 앞에서 작위 위기를 안고 돌아왔다.",
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "영지에서 버틴다",
                    ImpactPreview = "결속/군사↑ 가능 · 박탈 위기 또는 몰락의 징조",
                    ResultText = "가문은 성문을 닫고 영지에 머물렀다. 왕의 사자는 빈손으로 돌아갔지만, 그 침묵의 답은 곧 다른 형태로 돌아왔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity), Down(Vs1StateAxis.CourtInfluence) },
                    TitleEffect = new Vs1TitleEffect { RiskStageChange = TitleRiskStage.RevocationThreat, ResultSummary = "박탈 위협" },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBakTalWiGi) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagMolLakUiJingJo) },
                            Weight = 1,
                        },
                    },
                    ChronicleLineOverride = "가문은 영지에 버티며 박탈의 위협을 자초했다.",
                },
            },
        });

        // CRI-02 작위 유지 비용 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRI-02",
            Title = "작위 유지 비용",
            Category = Vs1EventCategory.Crisis,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "높은 작위를 유지하는 데 드는 선물, 병력, 의전 비용이 금고를 갉아먹고 있다. 체면을 지키려면 빚을 져야 하고, 포기하면 가문은 낮아진다.",
            ChronicleLine = "작위 유지 비용은 높은 이름이 얼마나 무거운 짐인지 드러냈다.",
            Conditions = new[]
            {
                CondTitleAtLeast(NobleTitleRank.Viscount, "자작 이상의 유지 압박"),
                CondStateLow(Vs1StateAxis.Wealth, "재산이 낮을 때 등장"),
            },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "빚을 낸다",
                    ImpactPreview = "작위 유지 · 가문의 부채",
                    ResultText = "장부에 새 줄이 그어졌다. 작위는 지켜졌지만, 그 무게는 다음 세대까지 따라올 것이다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth) },
                    TagChanges = new[] { TagAdd(TagGaMunUiBuChae) },
                    ChronicleLineOverride = "가문은 빚을 내어 작위를 유지했다.",
                },
                new Vs1EventChoice
                {
                    Label = "작위 일부를 포기한다",
                    ImpactPreview = "대표작위↓ · 재산 안정 · 몰락의 징조",
                    ResultText = "가문은 가장 높은 자리에서 한 걸음 내려왔다. 호칭은 낮아졌지만 장부는 다시 평평해졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth), Down(Vs1StateAxis.Honor) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        LoseRepresentativeTitle = true,
                        ResultSummary = "대표작위 하락",
                    },
                    TagChanges = new[] { TagAdd(TagMolLakUiJingJo), TagResolve(TagGaMunUiBuChae) },
                    ChronicleLineOverride = "가문은 작위 일부를 포기하며 한 단계 낮은 자리로 내려왔다.",
                },
            },
        });

        // CRI-03 정적의 탄핵 (큰 사건, 3선택지)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRI-03",
            Title = "정적의 탄핵",
            Category = Vs1EventCategory.Crisis,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "정적들이 회의장에서 가문의 실정과 오래된 흠을 들추었다. 말로 맞서면 명예에 걸어야 하고, 돈으로 막으면 오명이 남는다.",
            ChronicleLine = "정적의 탄핵은 가문의 오래된 흠을 궁정 한복판으로 끌어냈다.",
            Conditions = new[] { CondHasTag(TagJeongJeokUiPyoJeok, "정적의 표적이 활성일 때 또는 궁정 영향력이 낮을 때") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "반박 연설을 한다",
                    ImpactPreview = "명예↑ · 정적의 표적 해소 가능",
                    ResultText = "가주는 회의장에서 길게 말했다. 어떤 자리는 끄덕였고, 어떤 자리는 그대로였다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagJeongJeokUiPyoJeok) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGiSaUiMyeongYe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 1 },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "뇌물을 쓴다",
                    ImpactPreview = "재산↓ 위기 회피 · 불명예",
                    ResultText = "은화가 회의장 너머로 옮겨졌다. 다음 회의에서는 가문의 이름이 다른 줄로 옮겨졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagBulMyeongYe), TagResolve(TagJeongJeokUiPyoJeok) },
                },
                new Vs1EventChoice
                {
                    Label = "침묵한다",
                    ImpactPreview = "즉시 손실↓ · 박탈 위기",
                    ResultText = "가주는 답하지 않았다. 회의장의 무게는 가문 쪽으로 기울었다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TitleEffect = new Vs1TitleEffect { RiskStageChange = TitleRiskStage.Crisis, ResultSummary = "작위 위기" },
                    TagChanges = new[] { TagAdd(TagBakTalWiGi) },
                    ChronicleLineOverride = "가문의 침묵은 작위 위기로 되돌아왔다.",
                },
            },
        });

        // CRI-04 영지 반란 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRI-04",
            Title = "영지 반란",
            Category = Vs1EventCategory.Crisis,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "세금과 굶주림에 지친 영지민들이 무장을 들었다. 그들은 가문의 보호를 잊었다고 외치고, 기사들은 단호한 진압을 요구한다.",
            ChronicleLine = "영지 반란은 가문이 지켜야 할 백성과 맞서게 했다.",
            Conditions = new[] { CondHasTag(TagPiPyeHanYeongJi, "피폐한 영지 또는 결속 낮음") },
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.HouseUnity, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "협상한다",
                    ImpactPreview = "재산↓ 결속↑ · 피폐한 영지 해소 가능",
                    ResultText = "가주는 농민들의 대표와 마주 앉았다. 곡식과 인사가 교환됐고, 무기가 내려졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagPiPyeHanYeongJi), TagResolve(TagNaeBuGyunYeol) },
                },
                new Vs1EventChoice
                {
                    Label = "무력 진압한다",
                    ImpactPreview = "권위↑ · 피의 원한 또는 불명예",
                    ResultText = "기사들이 마을로 출격했다. 다음 날 아침 영지는 다시 조용해졌지만, 그 조용함은 비어 있었다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity), Down(Vs1StateAxis.Honor) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagPiUiWonHan) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBulMyeongYe) },
                            Weight = 1,
                        },
                    },
                },
            },
        });

        // CRI-05 후계권 무효 청원 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRI-05",
            Title = "후계권 무효 청원",
            Category = Vs1EventCategory.Crisis,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "궁정에 후계권 무효 청원이 올라갔다. 피의 정당성을 의심하는 문서가 돌고, 가문 안에서도 침묵하던 불만이 고개를 든다.",
            ChronicleLine = "후계권 무효 청원은 가문의 피와 계승을 의심받게 했다.",
            Conditions = new[] { CondHasTag(TagBulAnHanHuGye, "불안한 후계가 활성일 때") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "왕실 인가를 구한다",
                    ImpactPreview = "궁정 의존 · 왕실의 빚 또는 안정된 후계",
                    ResultText = "가문은 왕도의 인장에 손을 뻗었다. 왕은 짧게 끄덕였고, 그 끄덕임의 값은 다음에 청구될 것이다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagWangSilUiChaeMu), TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye) },
                    ChronicleLineOverride = "가문은 왕실 인가로 후계를 지켰다.",
                },
                new Vs1EventChoice
                {
                    Label = "친족 회의를 연다",
                    ImpactPreview = "결속 판정 · 안정된 후계 또는 내부 균열",
                    ResultText = "가문의 친족이 같은 자리에 모였다. 의자의 거리는 의견의 거리만큼이나 멀었다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye) },
                            VariantResultText = "친족의 손은 한 자리에 모였다. 후계자의 자리는 다시 단단해졌다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagNaeBuGyunYeol) },
                            VariantResultText = "회의는 갈라진 채 끝났다. 후계의 자리는 더 위태로워졌다.",
                            Weight = 1,
                        },
                    },
                },
            },
        });
    }
}
