// Vertical Slice 1 사건 — 가문 내부/후계 (FAM-01~08).

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddFamilyEvents(List<Vs1EventDefinition> list)
    {
        // FAM-01 후계자의 교육 (큰 사건, 3선택지)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-01",
            Title = "후계자의 교육",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "후계자의 교육 방향을 정할 때가 왔다. 궁정 예법, 영지 경영, 기사 훈련 중 무엇을 먼저 가르치느냐가 다음 세대의 성향을 바꿀 것이다.",
            ChronicleLine = "후계자의 교육 방향은 다음 세대가 어떤 가주로 자랄지 예고했다.",
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "궁정 교육을 시킨다",
                    ImpactPreview = "다음 세대 → 궁정 지향",
                    ResultText = "후계자는 궁정 예법을 익히기 시작했다. 영지의 흙은 그의 손에서 멀어졌다.",
                    NextHeirPresetKey = HeirPresetCourtEducated,
                    ChronicleLineOverride = "후계자는 궁정 예법을 익히는 길로 들어섰다.",
                },
                new Vs1EventChoice
                {
                    Label = "영지 교육을 시킨다",
                    ImpactPreview = "다음 세대 → 실리적",
                    ResultText = "후계자는 영지의 회계와 농지를 배우기 시작했다. 그의 손에는 곧 장부가 익숙해질 것이다.",
                    NextHeirPresetKey = HeirPresetEstateEducated,
                    ChronicleLineOverride = "후계자는 영지 경영을 배우는 길로 들어섰다.",
                },
                new Vs1EventChoice
                {
                    Label = "기사 교육을 시킨다",
                    ImpactPreview = "다음 세대 → 명예 중시",
                    ResultText = "후계자는 검과 말 위에서 자라기 시작했다. 그의 등은 곧 갑옷의 무게를 알 것이다.",
                    NextHeirPresetKey = HeirPresetKnightEducated,
                    ChronicleLineOverride = "후계자는 기사 훈련의 길로 들어섰다.",
                },
            },
        });

        // FAM-02 방계 친족의 청원 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-02",
            Title = "방계 친족의 청원",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "방계 친족이 오래된 공을 들며 작은 영지를 요구했다. 나누어 주면 친족은 고마워하겠지만, 가문의 중심은 약해질 수 있다.",
            ChronicleLine = "방계 친족의 청원은 가문의 결속과 재산을 나누어 시험했다.",
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.HouseUnity, 1), WeightForTag(TagNaeBuGyunYeol, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "영지를 나눠준다",
                    ImpactPreview = "재산↓ 결속↑ · 충성스러운 친족",
                    ResultText = "방계 친족은 작은 영지의 인장을 받았다. 그들의 답례는 짧지만 진했다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagChungSeongRoUnChinJok, TagNaeBuGyunYeol) },
                },
                new Vs1EventChoice
                {
                    Label = "청원을 거절한다",
                    ImpactPreview = "재산 유지 · 내부 균열",
                    ResultText = "청원은 정중한 말로 돌려보내졌다. 친족의 인사는 짧아졌다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagNaeBuGyunYeol) },
                },
            },
        });

        // FAM-03 사생아의 등장 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-03",
            Title = "사생아의 등장",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "한 젊은이가 가주의 피를 이었다며 성문 앞에 섰다. 인정하면 후계 구도가 흔들리고, 묻어 버리면 원한은 어둠 속에서 자란다.",
            ChronicleLine = "사생아의 등장은 피의 정당성과 후계 질서를 흔들었다.",
            Conditions = new[] { CondMinGeneration(2, "중기 세대 이상에서 등장") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "인정한다",
                    ImpactPreview = "불안한 후계 (교체)",
                    ResultText = "젊은이는 가문의 이름을 받았다. 식탁의 분위기는 그날 밤부터 달라졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagBulAnHanHuGye, TagAnJeongDoenHuGye) },
                    NextHeirPresetKey = HeirPresetIllegitimate,
                    ChronicleLineOverride = "사생아가 인정받으며 후계 구도가 흔들렸다.",
                },
                new Vs1EventChoice
                {
                    Label = "침묵시킨다",
                    ImpactPreview = "명예 보존 · 피의 원한 또는 불명예",
                    ResultText = "젊은이는 다시 어둠 속으로 사라졌다. 그 이름은 가문의 장부에 적히지 않았다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
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

        // FAM-04 가신단의 분열 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-04",
            Title = "가신단의 분열",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "오래 섬긴 가신들이 두 편으로 갈라졌다. 모두를 설득하려면 시간이 들고, 한쪽을 꺾으면 빠르게 조용해질 것이다.",
            ChronicleLine = "갈라진 가신단은 가문의 내부 균열을 드러냈다.",
            Conditions = new[] { CondStateLow(Vs1StateAxis.HouseUnity, "결속이 낮을 때 등장") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "중재한다",
                    ImpactPreview = "결속↑ · 충성스러운 친족 가능",
                    ResultText = "가주는 두 편의 사이에 앉았다. 시간은 들었지만 식탁은 다시 둥글게 돌아왔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagNaeBuGyunYeol) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagChungSeongRoUnChinJok) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "한쪽을 숙청한다",
                    ImpactPreview = "단기 안정 · 피의 원한 또는 내부 균열",
                    ResultText = "한 편의 자리는 그날 밤 안에 비었다. 다른 편은 침묵했지만, 그 침묵의 무게는 가벼워지지 않았다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.HouseUnity), Up(Vs1StateAxis.Wealth) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagPiUiWonHan) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagNaeBuGyunYeol) },
                            Weight = 1,
                        },
                    },
                },
            },
        });

        // FAM-05 후계자의 병약함 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-05",
            Title = "후계자의 병약함",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "후계자가 오래 앓고 있다. 명의를 부르면 금고가 비고, 다른 후계를 찾으면 가문 안의 시선이 갈라진다.",
            ChronicleLine = "병약한 후계자는 가문의 다음 세대가 얼마나 불안한지 보여주었다.",
            Conditions = new[] { CondMinGeneration(2, "중기 세대 이상의 후계 사건") },
            WeightModifiers = new[] { WeightForTag(TagBulAnHanHuGye, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "명의를 부른다",
                    ImpactPreview = "재산↓ · 불안한 후계 해소 가능",
                    ResultText = "명의는 먼 길을 와 처방을 남겼다. 후계자의 호흡은 가벼워졌고, 금고는 무거움을 잃었다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagBulAnHanHuGye) },
                    ChronicleLineOverride = "명의를 부르며 가문은 후계자의 호흡을 지켰다.",
                },
                new Vs1EventChoice
                {
                    Label = "대체 후계를 찾는다",
                    ImpactPreview = "불안한 후계 (교체) · 결속↓",
                    ResultText = "가문은 다른 자리에서 후계 후보를 찾기 시작했다. 시선은 갈라졌고, 후계자의 자리는 위태로워졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagBulAnHanHuGye, TagAnJeongDoenHuGye) },
                    NextHeirPresetKey = HeirPresetUnsettled,
                    ChronicleLineOverride = "가문은 대체 후계를 찾으며 후계자의 자리를 흔들었다.",
                },
            },
        });

        // FAM-06 충성스러운 숙부 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-06",
            Title = "충성스러운 숙부",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "숙부가 가문을 위해 권한을 맡겠다고 나섰다. 그는 믿을 만하지만, 권한을 준 친족은 언젠가 또 다른 중심이 될 수도 있다.",
            ChronicleLine = "충성스러운 숙부는 가문을 지탱할 손이자 새로운 권한의 중심이 되었다.",
            WeightModifiers = new[] { WeightForTag(TagChungSeongRoUnChinJok, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "섭정 권한을 준다",
                    ImpactPreview = "결속↑ 궁정 안정",
                    ResultText = "숙부의 자리는 옆에 마련됐다. 식탁의 자리는 둘로 늘었지만 어조는 한쪽으로 모였다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagChungSeongRoUnChinJok), TagResolve(TagNaeBuGyunYeol) },
                },
                new Vs1EventChoice
                {
                    Label = "군무를 맡긴다",
                    ImpactPreview = "명예/전쟁 보정 · 전쟁 공훈 또는 충성스러운 친족",
                    ResultText = "숙부는 부대의 깃발을 받아 들고 떠났다. 그의 등 뒤로 가문의 이름이 새 자리로 옮겨졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeonJaengGongHun) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagChungSeongRoUnChinJok) },
                            Weight = 1,
                        },
                    },
                },
            },
        });

        // FAM-07 가문 회계의 빈 장부 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-07",
            Title = "가문 회계의 빈 장부",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "장부의 빈 줄이 너무 많다. 누군가 훔쳤거나, 모두가 조금씩 빼돌렸거나, 가문 자체가 이미 빚 위에 서 있을지도 모른다.",
            ChronicleLine = "빈 장부는 가문의 금고뿐 아니라 내부 신뢰도 비어 있음을 드러냈다.",
            Conditions = new[] { CondStateLow(Vs1StateAxis.Wealth, "재산이 낮을 때 등장") },
            WeightModifiers = new[] { WeightForTag(TagGaMunUiBuChae, 2) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "내부 감사를 한다",
                    ImpactPreview = "결속↓ 가능 · 재산↑ · 가문의 부채 해소",
                    ResultText = "장부의 행마다 새 손이 닿았다. 누군가는 자리를 잃었고, 누군가는 다시 신뢰를 얻었다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth), Down(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagGaMunUiBuChae) },
                },
                new Vs1EventChoice
                {
                    Label = "외부 차입으로 덮는다",
                    ImpactPreview = "즉시 안정 · 가문의 부채",
                    ResultText = "외부의 손이 빈 자리를 채웠다. 장부는 다시 정렬됐고, 새 줄도 함께 그어졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth) },
                    TagChanges = new[] { TagAdd(TagGaMunUiBuChae) },
                },
            },
        });

        // FAM-08 후계자 간 경쟁 (큰 사건, 3선택지)
        list.Add(new Vs1EventDefinition
        {
            Id = "FAM-08",
            Title = "후계자 간 경쟁",
            Category = Vs1EventCategory.Family,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "두 후계자가 서로 다른 지지자를 모으고 있다. 장자를 세우면 질서는 지켜지고, 유능한 이를 고르면 가문은 갈라질 수 있다.",
            ChronicleLine = "후계자 간 경쟁은 다음 세대의 질서와 균열을 동시에 낳았다.",
            Conditions = new[] { CondMinGeneration(2, "후계 후보가 둘 이상인 단계에서 등장") },
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.HouseUnity, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "장자를 지지한다",
                    ImpactPreview = "안정된 후계 (교체) · 능력 불확실",
                    ResultText = "가문은 오래된 자리의 손을 들었다. 식탁의 한쪽은 묵묵해졌다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagReplace(TagAnJeongDoenHuGye, TagBulAnHanHuGye) },
                    NextHeirPresetKey = HeirPresetLegitimateEldest,
                    ChronicleLineOverride = "가문은 장자를 후계로 세웠다.",
                },
                new Vs1EventChoice
                {
                    Label = "유능한 후보를 지지한다",
                    ImpactPreview = "상태 보정 · 내부 균열",
                    ResultText = "가문은 검증된 손의 편에 섰다. 다른 자리에서는 식탁이 조용히 흔들렸다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagNaeBuGyunYeol) },
                    NextHeirPresetKey = HeirPresetCompetent,
                    ChronicleLineOverride = "가문은 능력 본위로 후계를 결정했다.",
                },
                new Vs1EventChoice
                {
                    Label = "결정을 미룬다",
                    ImpactPreview = "불안한 후계",
                    ResultText = "선택은 다음 계절로 미뤄졌다. 양쪽 모두 그날 밤 잠들지 못했다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagBulAnHanHuGye) },
                    NextHeirPresetKey = HeirPresetUnsettled,
                    ChronicleLineOverride = "가문은 후계 결정을 미루며 불안을 남겼다.",
                },
            },
        });
    }
}
