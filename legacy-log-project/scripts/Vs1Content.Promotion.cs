// Vertical Slice 1 사건 — 작위 상승/청원/포상 (PRO-01~05).
// 승격은 PRO 사건 선택 결과에서만 발생한다.

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddPromotionEvents(List<Vs1EventDefinition> list)
    {
        // PRO-01 자작위 청원 (남작 -> 자작)
        list.Add(new Vs1EventDefinition
        {
            Id = "PRO-01",
            Title = "자작위 청원",
            Category = Vs1EventCategory.Promotion,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "영지와 명예가 쌓이자 가문 안팎에서 자작위 청원을 말하기 시작했다. 지금 손을 뻗으면 더 높은 문이 열리지만, 궁정의 시선도 함께 따라온다.",
            ChronicleLine = "자작위 청원은 가문이 더 높은 문을 두드린 첫 기록이 되었다.",
            Conditions = new[]
            {
                CondTitleRange(NobleTitleRank.Baron, NobleTitleRank.Baron, "남작 단계의 첫 승격 기회"),
            },
            BaseWeight = 1,
            WeightModifiers = new[]
            {
                WeightForTag(TagSeungGyeokMyeongBun, 2),
                WeightForTag(TagJeonJaengGongHun, 1),
            },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "청원한다",
                    ImpactPreview = "재산↓ · 자작 승격 가능 · 승격 명분 또는 불안한 승격",
                    ResultText = "왕도의 인장이 가문의 청원서에 찍혔다. 가문의 이름은 다음 세대에 자작의 호칭으로 불릴 것이다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.CourtInfluence) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        PromoteTo = NobleTitleRank.Viscount,
                        ResultSummary = "자작 승격",
                    },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagSeungGyeokMyeongBun) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBulAnHanSeungGyeok) },
                            Weight = 1,
                        },
                    },
                    ChronicleLineOverride = "가문은 자작의 이름을 얻었다.",
                },
                new Vs1EventChoice
                {
                    Label = "기반을 더 다진다",
                    ImpactPreview = "결속↑ 재산 안정 · 불안한 승격 회피",
                    ResultText = "청원은 다음 해로 미뤄졌다. 영지의 발은 한 걸음 더 굳어졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity), Up(Vs1StateAxis.Wealth) },
                    ChronicleLineOverride = "가문은 자작위 청원을 보류하고 기반을 다졌다.",
                },
            },
        });

        // PRO-02 백작의 빈 영지 (자작 -> 백작)
        list.Add(new Vs1EventDefinition
        {
            Id = "PRO-02",
            Title = "백작의 빈 영지",
            Category = Vs1EventCategory.Promotion,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "인접 백작령의 주인이 후계 없이 죽었다. 왕은 충성스러운 이를 찾고 있고, 정적들은 이미 빈 자리를 노리고 있다.",
            ChronicleLine = "빈 백작령을 둘러싼 요구는 가문의 야망을 궁정의 표적으로 만들었다.",
            Conditions = new[]
            {
                CondTitleRange(NobleTitleRank.Viscount, NobleTitleRank.Viscount, "자작 단계의 승격 기회"),
            },
            BaseWeight = 1,
            WeightModifiers = new[]
            {
                WeightForTag(TagWangChongAe, 2),
                WeightForTag(TagSeungGyeokMyeongBun, 1),
            },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "영지를 요구한다",
                    ImpactPreview = "백작 승격 가능 · 정적의 표적 또는 불안한 승격",
                    ResultText = "가문은 왕 앞에 손을 뻗었다. 왕의 답은 짧았지만 그 무게는 다른 어떤 답보다 무거웠다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        PromoteTo = NobleTitleRank.Count,
                        ResultSummary = "백작 승격",
                    },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeongJeokUiPyoJeok) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBulAnHanSeungGyeok) },
                            Weight = 1,
                        },
                    },
                    ChronicleLineOverride = "가문은 백작의 이름을 얻었다.",
                },
                new Vs1EventChoice
                {
                    Label = "대리 통치를 맡는다",
                    ImpactPreview = "궁정↑ · 승격 명분 · 승격 지연",
                    ResultText = "가문은 빈 영지의 임시 인장을 받았다. 백작의 호칭은 아직 멀지만, 그 길은 분명해졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagSeungGyeokMyeongBun) },
                },
            },
        });

        // PRO-03 변경 방위 공적 (백작 -> 후작)
        list.Add(new Vs1EventDefinition
        {
            Id = "PRO-03",
            Title = "변경 방위 공적",
            Category = Vs1EventCategory.Promotion,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "변경의 성채를 지켜낸 공으로 가문의 이름이 왕의 회의에서 거론됐다. 후작위는 영광이지만, 변경을 지키는 책무도 함께 온다.",
            ChronicleLine = "변경 방위의 공적은 후작위의 영광과 책무를 함께 불러왔다.",
            Conditions = new[]
            {
                CondTitleRange(NobleTitleRank.Count, NobleTitleRank.Count, "백작 단계의 승격 기회"),
            },
            WeightModifiers = new[] { WeightForTag(TagJeonJaengGongHun, 2) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "변경을 맡는다",
                    ImpactPreview = "후작 승격 가능 · 변경의 책무",
                    ResultText = "가문은 변경의 깃발을 함께 받아 들었다. 후작의 호칭은 그 무게의 다른 이름이었다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        PromoteTo = NobleTitleRank.Marquess,
                        ResultSummary = "후작 승격",
                    },
                    TagChanges = new[] { TagAdd(TagByeonGyeongUiChaekMu) },
                    ChronicleLineOverride = "가문은 변경을 떠맡고 후작의 이름을 얻었다.",
                },
                new Vs1EventChoice
                {
                    Label = "공적만 인정받는다",
                    ImpactPreview = "명예↑ · 위험 낮음 · 승격 보류",
                    ResultText = "가문은 변경의 깃발 대신 회의장의 박수를 받았다. 후작의 호칭은 더 신중한 때를 기다린다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagAdd(TagSeungGyeokMyeongBun) },
                },
            },
        });

        // PRO-04 공작의 후계 단절 (후작 -> 공작)
        list.Add(new Vs1EventDefinition
        {
            Id = "PRO-04",
            Title = "공작의 후계 단절",
            Category = Vs1EventCategory.Promotion,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "공작가의 본가가 끊어졌고, 먼 혈연과 오래된 서약이 가문에도 계승권을 준다. 주장하면 거대한 판에 들어서게 된다.",
            ChronicleLine = "공작가의 단절은 가문을 거대한 계승 분쟁의 한복판에 세웠다.",
            Conditions = new[]
            {
                CondTitleRange(NobleTitleRank.Marquess, NobleTitleRank.Marquess, "후작 단계의 승격 기회"),
                CondMinGeneration(3, "장기 진행 후의 승격 사건"),
            },
            WeightModifiers = new[] { WeightForTag(TagWangChongAe, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "계승권을 주장한다",
                    ImpactPreview = "공작 승격 가능 · 정적의 표적 · 불안한 승격",
                    ResultText = "가문은 공작가의 계승권을 입에 올렸다. 그 한 마디가 왕도의 모든 식탁을 흔들었다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.CourtInfluence) },
                    TitleEffect = new Vs1TitleEffect
                    {
                        PromoteTo = NobleTitleRank.Duke,
                        ResultSummary = "공작 승격",
                    },
                    TagChanges = new[] { TagAdd(TagJeongJeokUiPyoJeok), TagAdd(TagBulAnHanSeungGyeok) },
                    ChronicleLineOverride = "가문은 공작의 이름을 얻었다.",
                },
                new Vs1EventChoice
                {
                    Label = "왕실 중재를 요청한다",
                    ImpactPreview = "안정적 영향력 · 직접 승격 가능성 낮음",
                    ResultText = "가문은 왕실의 손에 결정을 맡겼다. 영향력은 안전했지만, 공작의 호칭은 가까이 오지 않았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagWangSilUiChaeMu) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagWangChongAe) },
                            Weight = 1,
                        },
                    },
                },
            },
        });

        // PRO-05 왕의 특별 포상 (3선택지)
        list.Add(new Vs1EventDefinition
        {
            Id = "PRO-05",
            Title = "왕의 특별 포상",
            Category = Vs1EventCategory.Promotion,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕은 가문의 공을 인정하며 원하는 보상을 말하라 했다. 작위, 세금, 후계 보장 중 무엇을 청하느냐가 다음 세대를 바꿀 것이다.",
            ChronicleLine = "왕의 특별 포상 앞에서 가문은 작위, 세금, 후계 중 하나를 청했다.",
            Conditions = new[] { CondHasTag(TagWangChongAe, "왕의 총애가 있을 때만 받는 보상") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "작위를 청한다",
                    ImpactPreview = "승격 명분 또는 불안한 승격 · 정치 위험",
                    ResultText = "가문은 더 높은 자리를 청했다. 왕의 답은 무거웠다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBulAnHanSeungGyeok) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagSeungGyeokMyeongBun) },
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "세금 면제를 청한다",
                    ImpactPreview = "재산↑↑ · 가문의 부채 해소 가능",
                    ResultText = "왕도의 인장이 가문의 영지에 면세의 자국을 남겼다. 장부는 가벼워졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Major) },
                    TagChanges = new[] { TagResolve(TagGaMunUiBuChae) },
                },
                new Vs1EventChoice
                {
                    Label = "후계 보장을 청한다",
                    ImpactPreview = "안정된 후계 (교체) · 왕실의 빚",
                    ResultText = "왕의 인장이 후계자의 이름 옆에 찍혔다. 후계의 자리는 단단해졌지만, 그 자리는 왕실에 빚을 졌다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye), TagAdd(TagWangSilUiChaeMu) },
                    ChronicleLineOverride = "가문은 왕에게 후계 보장을 청했다.",
                },
            },
        });
    }
}
