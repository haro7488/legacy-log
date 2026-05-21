// Vertical Slice 1 사건 — 궁정/정치 (CRT-01~08).

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddCourtEvents(List<Vs1EventDefinition> list)
    {
        // CRT-01 왕의 사냥 초대 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-01",
            Title = "왕의 사냥 초대",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "왕이 가문을 사냥에 초대했다. 숲에서 나누는 한마디가 궁정의 문을 열 수 있지만, 그동안 영지는 가주의 눈을 잃는다.",
            ChronicleLine = "왕의 사냥 초대는 가문을 궁정 가까이로 부르며 영지를 잠시 비웠다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Viscount, "자작 이상의 궁정 접근") },
            WeightModifiers = new[] { WeightForTag(TagWangChongAe, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "참석한다",
                    ImpactPreview = "궁정↑ · 왕의 총애 · 결속↓ 가능",
                    ResultText = "숲의 한낮에서 가주는 왕의 곁을 따라다녔다. 몇 마디는 짧았지만 그 무게는 무거웠다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagReplace(TagWangChongAe, TagGungJeongUiSim) },
                },
                new Vs1EventChoice
                {
                    Label = "영지에 남는다",
                    ImpactPreview = "결속↑ · 궁정 기회 상실",
                    ResultText = "가주는 영지를 떠나지 않았다. 사신은 짧은 인사를 남기고 돌아갔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity) },
                },
            },
        });

        // CRT-02 왕비의 후원 요청 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-02",
            Title = "왕비의 후원 요청",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "왕비의 사절이 조용히 찾아와 후원을 청했다. 후원은 곧 영향력이 되겠지만, 왕실의 부탁은 한 번으로 끝나지 않는다.",
            ChronicleLine = "왕비의 후원 요청은 은화로 궁정의 호의를 사는 길을 열었다.",
            Conditions = new[] { CondStateHigh(Vs1StateAxis.CourtInfluence, "궁정 영향력이 보통 이상") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "후원한다",
                    ImpactPreview = "재산↓ 궁정↑ · 왕실의 빚",
                    ResultText = "은화는 왕비의 손에 도착했다. 답례의 말은 가벼웠지만 그 무게는 다음 청에서 다시 돌아왔다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagWangSilUiChaeMu), TagResolve(TagGungJeongUiSim) },
                },
                new Vs1EventChoice
                {
                    Label = "정중히 거절한다",
                    ImpactPreview = "재산 유지 · 궁정의 의심",
                    ResultText = "정중한 사양이 돌아왔다. 왕비의 답신은 짧았다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                },
            },
        });

        // CRT-03 파벌의 비밀 서약 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-03",
            Title = "파벌의 비밀 서약",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "궁정 파벌의 대표가 밤중에 서약서를 내밀었다. 서명하면 더 높은 자리로 갈 길이 보이지만, 반대편 귀족들은 가문을 적으로 볼 것이다.",
            ChronicleLine = "비밀 서약은 가문을 궁정 파벌의 그림자 속으로 끌어들였다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Count, "백작 이상의 파벌 사건") },
            WeightModifiers = new[] { WeightForTag(TagJeongJeokUiPyoJeok, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "서약한다",
                    ImpactPreview = "궁정↑ · 정적의 표적 또는 왕의 총애",
                    ResultText = "촛불 아래에서 서약서에 이름이 적혔다. 약속은 두 방향으로 동시에 작동했다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeongJeokUiPyoJeok) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagWangChongAe) },
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "거리를 둔다",
                    ImpactPreview = "위험↓ 기회↓ · 정적의 표적 해소 가능",
                    ResultText = "서약서는 다시 접혔다. 그날 밤의 약속은 가문의 이름 위에 남지 않았다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagResolve(TagJeongJeokUiPyoJeok) },
                },
            },
        });

        // CRT-04 왕실 혼례 초청 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-04",
            Title = "왕실 혼례 초청",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Low,
            Body = "왕실 혼례 초청장이 도착했다. 예물의 크기는 곧 가문의 위신으로 읽힐 것이고, 검소함은 신중함이 아니라 초라함으로 보일 수 있다.",
            ChronicleLine = "왕실 혼례의 예물은 가문의 위신과 금고를 함께 드러냈다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Viscount, "자작 이상의 궁정 의례") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "화려한 예물을 보낸다",
                    ImpactPreview = "재산↓ 궁정↑",
                    ResultText = "예물 행렬은 왕도까지 길게 늘어섰다. 가문의 이름은 그날 광장의 사람들 입에 올랐다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate), Up(Vs1StateAxis.CourtInfluence) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagWangChongAe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "검소하게 참석한다",
                    ImpactPreview = "재산 유지 · 궁정의 의심",
                    ResultText = "가문은 조용히 자리만 채웠다. 회랑의 시선이 어깨에 잠시 머물렀다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                },
            },
        });

        // CRT-05 밀서의 발견 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-05",
            Title = "밀서의 발견",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "하인이 정적의 밀서를 발견했다. 왕에게 바치면 충성을 증명할 수 있고, 쥐고 있으면 거래의 칼날이 된다.",
            ChronicleLine = "발견된 밀서는 충성의 증거이자 거래의 칼날이 되었다.",
            Conditions = new[] { CondHasTag(TagGungJeongUiSim, "궁정의 의심이 있을 때 등장") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "왕에게 바친다",
                    ImpactPreview = "왕의 총애 · 정적의 표적 · 궁정의 의심 해소",
                    ResultText = "밀서는 왕의 손에 도착했다. 왕은 짧게 끄덕였고, 다른 자리에서는 가문의 이름이 길게 적혔다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagWangChongAe), TagAdd(TagJeongJeokUiPyoJeok), TagResolve(TagGungJeongUiSim) },
                },
                new Vs1EventChoice
                {
                    Label = "협상 카드로 쓴다",
                    ImpactPreview = "재산↑ 또는 영향력↑ · 불명예 또는 궁정의 의심",
                    ResultText = "가문은 밀서를 손에 쥐고 정적과 마주 앉았다. 거래의 무게는 양쪽 모두에게 무거웠다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagBulMyeongYe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                            Weight = 1,
                        },
                    },
                },
            },
        });

        // CRT-06 재판관 매수 제안 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-06",
            Title = "재판관 매수 제안",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "오래된 소송이 가문을 괴롭히는 가운데, 재판관의 조카가 은밀한 값을 불렀다. 돈으로 길을 열 수도, 공개 재판으로 명예를 걸 수도 있다.",
            ChronicleLine = "재판관 매수 제안은 가문이 법과 은화 중 무엇을 믿는지 드러냈다.",
            Conditions = new[]
            {
                CondStateHigh(Vs1StateAxis.Wealth, "재산이 보통 이상이어야 한다"),
                CondStateLow(Vs1StateAxis.CourtInfluence, "궁정 영향력이 낮을 때 제안이 온다"),
            },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "매수한다",
                    ImpactPreview = "궁정↑ 가능 · 불명예",
                    ResultText = "은화가 조용히 옮겨졌다. 다음 재판은 빨리 끝났지만, 그 빠름은 또 다른 자국을 남겼다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.CourtInfluence) },
                    TagChanges = new[] { TagAdd(TagBulMyeongYe) },
                },
                new Vs1EventChoice
                {
                    Label = "공개 재판을 요구한다",
                    ImpactPreview = "명예↑ · 결과 불확실",
                    ResultText = "가문은 광장에 자리를 잡았다. 결과는 누구의 손에도 미리 들어있지 않았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagResolve(TagBulMyeongYe) },
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
            },
        });

        // CRT-07 왕의 빚 보증 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-07",
            Title = "왕의 빚 보증",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕실 금고가 비었다는 소문이 도는 가운데, 가문에 보증 요청이 내려왔다. 왕은 잊지 않겠지만, 빚도 쉽게 잊히지 않는다.",
            ChronicleLine = "왕의 빚에 이름을 올리며 가문은 총애와 위험한 채무를 함께 얻었다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Count, "백작 이상 또는 왕의 총애 보유") },
            WeightModifiers = new[] { WeightForTag(TagWangChongAe, 2) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "보증한다",
                    ImpactPreview = "궁정↑ · 왕실의 빚 · 왕의 총애 · 재산 위험",
                    ResultText = "가문의 인장이 보증서에 찍혔다. 왕의 사자는 따뜻한 말로 떠났고, 장부에는 차가운 줄이 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.Wealth) },
                    TagChanges = new[] { TagAdd(TagWangSilUiChaeMu), TagAdd(TagWangChongAe) },
                },
                new Vs1EventChoice
                {
                    Label = "조건을 붙인다",
                    ImpactPreview = "재산 보호 · 궁정의 의심",
                    ResultText = "가문은 신중한 말로 답했다. 보증은 늦춰졌고, 왕도의 한 자리에서는 가문의 이름이 다르게 불렸다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                },
            },
        });

        // CRT-08 궁정 시인의 풍자 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "CRT-08",
            Title = "궁정 시인의 풍자",
            Category = Vs1EventCategory.Court,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "궁정 시인이 가문을 비웃는 노래를 불렀다. 웃어넘기면 여유로 보일 수도 있고, 처벌하면 권위로 보일 수도 있다.",
            ChronicleLine = "궁정 시인의 풍자는 가문의 평판을 노래와 처벌 사이에 세웠다.",
            Conditions = new[] { CondStateLow(Vs1StateAxis.Honor, "명예가 낮거나 불명예가 있을 때") },
            WeightModifiers = new[] { WeightForTag(TagBulMyeongYe, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "시인을 후원한다",
                    ImpactPreview = "재산↓ · 불명예 또는 겁쟁이의 소문 해소 가능",
                    ResultText = "시인은 가문의 식탁에 앉았다. 다음 노래에는 가문의 이름이 다른 어조로 실렸다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagResolve(TagBulMyeongYe), TagResolve(TagGeopJaengIUiSoMun) },
                },
                new Vs1EventChoice
                {
                    Label = "처벌한다",
                    ImpactPreview = "권위↑ · 겁쟁이의 소문 또는 불명예",
                    ResultText = "시인은 광장 밖으로 끌려나갔다. 노래는 잠시 멈췄지만, 다른 입들로 옮겨졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.HouseUnity) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGeopJaengIUiSoMun) },
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
    }
}
