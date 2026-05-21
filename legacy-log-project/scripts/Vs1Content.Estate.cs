// Vertical Slice 1 사건 — 영지/재산 (EST-01~08).
// 기준: docs/product/vertical-slice-1-event-list.md, event-copy-draft.md, event-chronicle-draft.md, event-tag-change-table.md
// 사건 본문/선택지 행동명은 Root 기획 문서에서 그대로 이식.

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddEstateEvents(List<Vs1EventDefinition> list)
    {
        // EST-01 흉작의 해 (큰 사건, 요약 높음)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-01",
            Title = "흉작의 해",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "긴 비가 수확철을 망쳤다. 영지의 곡창 앞에는 굶주린 농민들이 모였고, 가문의 회계관은 겨울을 버티려면 창고를 지켜야 한다고 말한다.",
            ChronicleLine = "흉작의 해에 가문은 곡창과 세금 사이에서 영지의 겨울을 결정했다.",
            BaseWeight = 1,
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.Wealth, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "곡창을 연다",
                    ImpactPreview = "재산↓ 명예↑ 결속↑ · 피폐한 영지 위험 낮춤",
                    ResultText = "곡창을 열어 굶주린 농민을 먹였다. 재산은 줄었지만 영지의 사람들은 가문의 이름을 더 가까이 불렀다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.Honor), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagPiPyeHanYeongJi) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGaMunUiMaengSe) },
                            VariantResultText = "굶주린 자들 앞에서의 약속은 가문의 맹세로 새겨졌다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "세금을 유지한다",
                    ImpactPreview = "재산 유지 · 명예↓ 결속↓ · 피폐한 영지 위험",
                    ResultText = "세금은 그대로 거뒀다. 곳간은 지켰지만 굶주린 자들의 시선이 가문을 떠나지 않았다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Honor), Down(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagAdd(TagPiPyeHanYeongJi) },
                },
            },
        });

        // EST-02 상단의 투자 제안 (보조 사건, 요약 중간)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-02",
            Title = "상단의 투자 제안",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "먼 도시의 상단이 영지 시장에 은화를 대겠다고 제안했다. 그들은 빠른 부흥을 약속하지만, 계약서에는 다음 세대까지 이어질 권리 조항이 붙어 있다.",
            ChronicleLine = "먼 도시의 상단이 영지에 들어오며 가문의 금고와 장래의 빚을 함께 흔들었다.",
            Conditions = new[]
            {
                CondTitleRange(NobleTitleRank.Baron, NobleTitleRank.Count, "남작~백작 단계의 상업 기회"),
                CondMissingTag(TagSangInHuWon, "이미 상인 후원이 있으면 제안이 오지 않는다"),
            },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "투자를 받는다",
                    ImpactPreview = "재산↑↑ · 상인 후원 + 가문의 부채 위험",
                    ResultText = "상단의 은화가 영지에 흘러들었다. 시장은 빠르게 살아났지만 장부의 한 줄에는 계약 조항이 길게 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagSangInHuWon), TagAdd(TagGaMunUiBuChae) },
                },
                new Vs1EventChoice
                {
                    Label = "거절하고 자립한다",
                    ImpactPreview = "결속↑ · 재산 정체",
                    ResultText = "가문은 외부의 손을 빌리지 않기로 했다. 회복은 느리지만, 가신들은 그 결정에 묵묵히 따랐다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagSangInHuWon) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGaMunUiMaengSe) },
                            VariantResultText = "자립의 결정은 가문의 맹세로 새겨졌다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
            },
        });

        // EST-03 영지 도로 정비 (보조, 요약 낮음)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-03",
            Title = "영지 도로 정비",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Low,
            Body = "오래된 도로가 무너져 장터와 성채 사이의 마차가 자주 멈춘다. 지금 고치면 큰 비용이 들지만, 방치하면 영지의 숨통이 서서히 막힌다.",
            ChronicleLine = "가문은 낡은 도로를 손보며 영지의 다음 계절을 준비했다.",
            Conditions = new[] { CondStateHigh(Vs1StateAxis.Wealth, "재산이 보통 이상이어야 한다") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "대대적으로 정비한다",
                    ImpactPreview = "재산↓ · 풍요로운 영지",
                    ResultText = "도로의 돌이 새로 놓이며 가문의 영지는 한 단계 더 단단해졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagReplace(TagPungYoRoUnYeongJi, TagPiPyeHanYeongJi) },
                },
                new Vs1EventChoice
                {
                    Label = "임시 보수만 한다",
                    ImpactPreview = "재산 유지 · 다음 영지 사건 위험",
                    ResultText = "급한 곳만 손보고 다음 계절로 미뤘다. 마차는 다시 굴렀지만, 다음 비에는 같은 자리에서 멈출 것이다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth) },
                },
            },
        });

        // EST-04 세금 징수관의 부패 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-04",
            Title = "세금 징수관의 부패",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "징수관이 가문의 이름으로 더 많은 세금을 거두고 일부를 감췄다는 고발이 올라왔다. 그를 처벌하면 돈은 잃지만, 눈감으면 곧장 금고가 찬다.",
            ChronicleLine = "부패한 징수관의 문제는 금고와 명예 중 무엇을 지킬지 묻게 했다.",
            Conditions = new[]
            {
                CondStateLow(Vs1StateAxis.Wealth, "재산 낮음일 때 등장도가 오른다"),
            },
            WeightModifiers = new[] { WeightForTag(TagGaMunUiBuChae, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "눈감고 걷게 둔다",
                    ImpactPreview = "재산↑ · 명예↓ · 불명예",
                    ResultText = "장부의 빈 줄은 채워졌지만 영지의 누군가는 그 일을 잊지 않을 것이다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth), Down(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagAdd(TagBulMyeongYe) },
                },
                new Vs1EventChoice
                {
                    Label = "공개 처벌한다",
                    ImpactPreview = "재산↓ · 명예↑ 결속↑ · 불명예 해소",
                    ResultText = "징수관은 광장에서 끌려나갔다. 금고의 회복은 늦어졌지만 가문의 이름은 흔들리지 않았다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.Honor), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagBulMyeongYe) },
                },
            },
        });

        // EST-05 변경 시장 개방 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-05",
            Title = "변경 시장 개방",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "변경의 상인들이 성문 밖에 자유 시장을 열게 해 달라고 청했다. 허가하면 금화가 흐르겠지만, 오래된 귀족 특권은 흔들릴 것이다.",
            ChronicleLine = "변경 시장의 문이 열리거나 닫히며 영지의 질서와 이익이 갈라졌다.",
            Conditions = new[]
            {
                CondTitleAtLeast(NobleTitleRank.Viscount, "자작 이상 또는 상인 후원에서 등장"),
            },
            WeightModifiers = new[] { WeightForTag(TagSangInHuWon, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "시장권을 허가한다",
                    ImpactPreview = "재산↑ · 궁정 영향력↓ · 상인 후원",
                    ResultText = "성문 밖에 새로운 시장이 열렸다. 금화는 흐르지만 궁정의 보수파는 차가운 얼굴로 그 광경을 보았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagSangInHuWon) },
                },
                new Vs1EventChoice
                {
                    Label = "귀족 특권을 지킨다",
                    ImpactPreview = "명예↑ 궁정↑ · 재산 기회 상실",
                    ResultText = "오래된 특권은 흔들리지 않았다. 다만 변경의 상인들은 다른 영지의 문을 두드리러 갔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor), Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagResolve(TagSangInHuWon) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGiSaUiMyeongYe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
            },
        });

        // EST-06 영지민의 탄원 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-06",
            Title = "영지민의 탄원",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "마을 대표들이 무릎을 꿇고 세금 유예를 청했다. 그들은 가문의 자비를 말하고, 회계관은 영주의 권위가 한 번 꺾이면 다시 세우기 어렵다고 말한다.",
            ChronicleLine = "영지민의 탄원 앞에서 가문은 자비와 권위 중 하나를 앞세웠다.",
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.HouseUnity, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "요구를 받아들인다",
                    ImpactPreview = "재산↓ · 명예↑ 결속↑",
                    ResultText = "마을 대표들은 고개를 숙이고 돌아갔다. 곳간은 가벼워졌지만 영지의 분위기는 따뜻해졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.Honor), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagPiPyeHanYeongJi) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGaMunUiMaengSe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "영주의 권위를 세운다",
                    ImpactPreview = "재산 유지 · 결속↓ · 피폐한 영지",
                    ResultText = "탄원은 받아들여지지 않았다. 권위는 섰지만 그 그늘에서 더 많은 사람이 무릎을 꿇고 일어나지 않았다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.HouseUnity, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagPiPyeHanYeongJi) },
                },
            },
        });

        // EST-07 오래된 광산 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-07",
            Title = "오래된 광산",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "산속의 폐광에서 다시 은맥이 보인다는 보고가 왔다. 노련한 광부들은 기회라고 말하지만, 오래전 붕괴로 죽은 이들의 이름도 아직 잊히지 않았다.",
            ChronicleLine = "오래된 광산은 가문에 부의 약속과 묻힌 원한을 동시에 드러냈다.",
            Conditions = new[] { CondTitleRange(NobleTitleRank.Baron, NobleTitleRank.Marquess, "남작~후작 단계의 영지 사건") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "광산을 재개한다",
                    ImpactPreview = "재산↑ 가능 · 사고/원한 위험",
                    ResultText = "광부들의 등불이 다시 산속에 켜졌다. 어떤 빛이 나올지는 산이 결정할 것이다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagPungYoRoUnYeongJi) },
                            VariantResultText = "은맥은 풍성했다. 영지는 곧 그 광산을 자랑으로 삼았다.",
                            VariantChronicleLine = "오래된 광산은 가문에 새로운 부의 길을 열어주었다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraStateImpacts = new[] { Down(Vs1StateAxis.Honor), Down(Vs1StateAxis.HouseUnity) },
                            ExtraTagChanges = new[] { TagAdd(TagPiUiWonHan) },
                            VariantResultText = "갱이 무너졌다. 묻혔던 이름들이 다시 광부들의 입에 올랐다.",
                            VariantChronicleLine = "오래된 광산은 가문에 묻힌 원한을 다시 일깨웠다.",
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "봉인된 광산으로 둔다",
                    ImpactPreview = "위험 회피 · 재산 정체",
                    ResultText = "산의 입구는 다시 돌로 막혔다. 위험과 함께 기회도 묻었다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                },
            },
        });

        // EST-08 왕실 징발 명령 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "EST-08",
            Title = "왕실 징발 명령",
            Category = Vs1EventCategory.Estate,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕실 사자가 도착해 곡물과 마차를 징발하겠다고 통보했다. 명령을 따르면 궁정은 가문을 기억하겠지만, 영지는 비어 갈 것이다.",
            ChronicleLine = "왕실의 징발 명령은 가문의 충성과 영지의 생계를 맞부딪치게 했다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Count, "백작 이상 또는 왕의 총애 보유") },
            WeightModifiers = new[] { WeightForTag(TagWangChongAe, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "징발에 협조한다",
                    ImpactPreview = "궁정↑ 재산↓ · 왕의 총애",
                    ResultText = "곡물과 마차가 왕도로 떠났다. 왕실 사자는 가문의 이름을 기억하겠다고 말했다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagReplace(TagWangChongAe, TagGungJeongUiSim) },
                },
                new Vs1EventChoice
                {
                    Label = "영지 사정을 호소한다",
                    ImpactPreview = "재산 유지 · 궁정의 의심",
                    ResultText = "사자는 한참을 듣더니 짧게 답했다. 곳간은 지켜졌지만 왕도의 귀에 가문의 이름이 다르게 들어갔다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                },
            },
        });
    }
}
