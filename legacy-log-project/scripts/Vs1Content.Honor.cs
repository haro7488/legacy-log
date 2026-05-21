// Vertical Slice 1 사건 — 명예/전쟁/기사도 (HON-01~08).

using System.Collections.Generic;

public static partial class Vs1Content
{
    private static void AddHonorEvents(List<Vs1EventDefinition> list)
    {
        // HON-01 국경의 소규모 충돌 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-01",
            Title = "국경의 소규모 충돌",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "국경 초소에서 적대 가문의 기병과 충돌이 일어났다. 큰 전쟁은 아니지만, 젊은 기사들은 가문의 깃발을 앞세울 기회라고 여긴다.",
            ChronicleLine = "국경의 충돌 속에서 가문의 깃발은 전공 또는 상처로 기록되었다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Baron, "남작 이상의 군사 사건") },
            WeightModifiers = new[] { WeightForTitleAtLeast(NobleTitleRank.Marquess, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "직접 출전한다",
                    ImpactPreview = "명예↑ · 전쟁 공훈 또는 잊히지 않는 패배",
                    ResultText = "가주가 직접 말 위에 올랐다. 깃발은 흔들렸고, 그 결과는 광장에서 노래로 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagGeopJaengIUiSoMun) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeonJaengGongHun) },
                            VariantResultText = "기병의 깃발이 무너졌다. 가문은 전공을 얻었다.",
                            VariantChronicleLine = "국경의 충돌에서 가문은 전공을 얻었다.",
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraStateImpacts = new[] { Down(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                            ExtraTagChanges = new[] { TagAdd(TagItHiJiAnNeunPaeBae) },
                            VariantResultText = "기병의 창끝이 더 길었다. 가문은 상처를 안고 돌아왔다.",
                            VariantChronicleLine = "국경의 충돌에서 가문은 잊히지 않는 패배를 안았다.",
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "병력만 보낸다",
                    ImpactPreview = "재산↓ · 명예 소폭↑",
                    ResultText = "가주는 성에 남았다. 병력은 충돌을 잠재웠지만 그 이름은 광장에 오래 머물지 않았다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.Honor) },
                },
            },
        });

        // HON-02 기사단의 결투 청원 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-02",
            Title = "기사단의 결투 청원",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "가문의 기사 둘이 명예를 걸고 결투를 청했다. 허락하면 기사도는 서겠지만 피가 흐르고, 금지하면 원한은 속으로 쌓일 것이다.",
            ChronicleLine = "기사단의 결투 청원은 가문의 명예와 피의 값을 시험했다.",
            WeightModifiers = new[] { WeightForTag(TagGiSaUiMyeongYe, 1), WeightForStateLow(Vs1StateAxis.Honor, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "결투를 허락한다",
                    ImpactPreview = "명예↑ 가능 · 기사의 명예 또는 피의 원한",
                    ResultText = "두 기사가 마당 한가운데에서 마주섰다. 한 사람은 일어났고, 한 사람은 다시 일어나지 못했다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                    TagChanges = new[] { TagResolve(TagGeopJaengIUiSoMun) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGiSaUiMyeongYe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagPiUiWonHan) },
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "금지하고 배상한다",
                    ImpactPreview = "재산↓ · 결속↑ · 피의 원한 해소",
                    ResultText = "결투는 막혔다. 두 기사는 배상금을 받고 같은 식탁에 앉지 않은 채 흩어졌다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.HouseUnity) },
                    TagChanges = new[] { TagResolve(TagPiUiWonHan) },
                },
            },
        });

        // HON-03 패잔병 수용 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-03",
            Title = "패잔병 수용",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Low,
            Body = "패배한 전투에서 살아남은 병사들이 영지로 흘러들어 왔다. 그들을 받아들이면 가문은 관대해 보이겠지만, 겨울 식량은 더 빠르게 줄 것이다.",
            ChronicleLine = "패잔병들이 영지에 들어오며 가문의 관대함과 부담이 함께 늘었다.",
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "받아들인다",
                    ImpactPreview = "명예↑ 재산↓ · 충성스러운 친족 또는 전쟁 공훈",
                    ResultText = "병사들은 가문의 깃발 아래로 들어왔다. 곡식은 줄었지만 새로운 손이 늘었다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor), Down(Vs1StateAxis.Wealth) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagChungSeongRoUnChinJok) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeonJaengGongHun) },
                            Weight = 1,
                        },
                    },
                },
                new Vs1EventChoice
                {
                    Label = "돌려보낸다",
                    ImpactPreview = "재산 유지 · 겁쟁이의 소문 위험",
                    ResultText = "성문은 닫혔다. 병사들은 다른 길로 흩어졌고, 그들의 입에서 가문의 이름은 차갑게 남았다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                    TagChanges = new[] { TagAdd(TagGeopJaengIUiSoMun) },
                },
            },
        });

        // HON-04 전공을 가로챈 귀족 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-04",
            Title = "전공을 가로챈 귀족",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "궁정 연회에서 다른 귀족이 가문의 전공을 자기 이름으로 말했다. 지금 항의하면 사람들은 기억하겠지만, 새로운 적도 생긴다.",
            ChronicleLine = "빼앗긴 전공을 둘러싸고 가문은 명예 회복과 정적의 위험 사이에 섰다.",
            Conditions = new[] { CondHasTag(TagJeonJaengGongHun, "전쟁 공훈을 가진 자만 빼앗긴 사실을 안다") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "공개 항의한다",
                    ImpactPreview = "명예↑ 가능 · 정적의 표적",
                    ResultText = "연회장이 잠시 조용해졌다. 가문의 전공은 다시 가문의 이름으로 불렸지만, 어떤 시선은 더 차가워졌다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagJeongJeokUiPyoJeok) },
                },
                new Vs1EventChoice
                {
                    Label = "조용히 증거를 모은다",
                    ImpactPreview = "궁정↑ · 즉시 명예는 안 오름",
                    ResultText = "가문은 입을 다물고 종이를 모았다. 시간이 들겠지만, 어떤 패는 천천히 펼쳐야 더 멀리 간다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagSeungGyeokMyeongBun) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 2 },
                    },
                },
            },
        });

        // HON-05 성인의 유해 호송 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-05",
            Title = "성인의 유해 호송",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.Foundation,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Low,
            Body = "성인의 유해가 가문의 영지를 지나간다. 성대히 호송하면 명예가 높아지겠지만, 행렬을 치르는 비용은 만만치 않다.",
            ChronicleLine = "성인의 유해가 영지를 지나며 가문은 경건함과 비용을 저울질했다.",
            WeightModifiers = new[] { WeightForStateLow(Vs1StateAxis.Honor, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "성대히 호송한다",
                    ImpactPreview = "재산↓ 명예↑ · 기사의 명예 · 불명예 해소",
                    ResultText = "수십 개의 등불이 유해를 따라 영지를 가로질렀다. 가문의 이름은 그날 밤의 빛과 함께 기억되었다.",
                    StateImpacts = new[] { Down(Vs1StateAxis.Wealth), Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagGiSaUiMyeongYe), TagResolve(TagBulMyeongYe) },
                },
                new Vs1EventChoice
                {
                    Label = "비용을 줄인다",
                    ImpactPreview = "재산 유지 · 명예 소폭↑",
                    ResultText = "행렬은 조용히 지나갔다. 가문의 이름은 함께 지나갔지만 깊이 새기지는 못했다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor) },
                },
            },
        });

        // HON-06 패배한 적장의 몸값 (보조)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-06",
            Title = "패배한 적장의 몸값",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.Pressure,
            Importance = Vs1EventImportance.Minor,
            SummaryPriority = Vs1SummaryPriority.Low,
            Body = "전장에서 사로잡힌 적장이 값비싼 몸값을 약속했다. 기사들은 명예로운 석방을 말하고, 회계관은 금고가 비었다고 말한다.",
            ChronicleLine = "포로가 된 적장의 몸값은 기사도와 금고 사이의 선택으로 남았다.",
            Conditions = new[] { CondHasTag(TagJeonJaengGongHun, "전쟁 사건 이후에 등장한다") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "명예롭게 석방한다",
                    ImpactPreview = "명예↑ · 기사의 명예 · 재산 기회 상실",
                    ResultText = "적장은 가문의 칼날 대신 가문의 말씀을 받아 들고 돌아갔다. 금고는 비어 있지만 그 이름은 광장에 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagGiSaUiMyeongYe), TagResolve(TagPiUiWonHan) },
                },
                new Vs1EventChoice
                {
                    Label = "높은 몸값을 요구한다",
                    ImpactPreview = "재산↑ · 명예↓ 가능 · 불명예",
                    ResultText = "은화 자루가 도착했다. 금고는 가벼움을 면했지만 광장에는 가문의 이름이 동전 소리와 함께 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Wealth, Vs1ImpactMagnitude.Moderate) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraStateImpacts = new[] { Down(Vs1StateAxis.Honor) },
                            ExtraTagChanges = new[] { TagAdd(TagBulMyeongYe) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant { Weight = 1 },
                    },
                },
            },
        });

        // HON-07 겁쟁이라는 소문 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-07",
            Title = "겁쟁이라는 소문",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.Medium,
            Body = "술집과 병영에서 가문이 싸움을 피한다는 노래가 퍼졌다. 소문을 잠재우려면 위험한 출전이 필요하지만, 무모함은 또 다른 비극을 부른다.",
            ChronicleLine = "겁쟁이라는 소문은 가문을 다시 위험한 전장으로 밀어 넣었다.",
            Conditions = new[] { CondHasTag(TagGeopJaengIUiSoMun, "겁쟁이의 소문이 활성일 때") },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "위험한 출전을 감수한다",
                    ImpactPreview = "명예↑ · 전쟁 공훈 · 큰 위험",
                    ResultText = "가주는 가장 위험한 전장으로 향했다. 그 모습은 노래를 잠재웠다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagGeopJaengIUiSoMun), TagAdd(TagJeonJaengGongHun) },
                },
                new Vs1EventChoice
                {
                    Label = "소문을 무시한다",
                    ImpactPreview = "안전 · 겁쟁이의 소문 지속",
                    ResultText = "가주는 노래를 듣지 않은 척했다. 노래는 시들지 않았다.",
                    StateImpacts = System.Array.Empty<Vs1StateImpact>(),
                },
            },
        });

        // HON-08 전쟁 포로의 학살 명령 (큰 사건)
        list.Add(new Vs1EventDefinition
        {
            Id = "HON-08",
            Title = "전쟁 포로의 학살 명령",
            Category = Vs1EventCategory.Honor,
            Role = Vs1EventRole.TurningPoint,
            Importance = Vs1EventImportance.Major,
            SummaryPriority = Vs1SummaryPriority.High,
            Body = "왕의 장군이 포로 처형 명령을 내렸다. 따르지 않으면 왕명에 맞서는 것이고, 따르면 가문의 이름은 피로 기록된다.",
            ChronicleLine = "전쟁 포로의 운명 앞에서 가문은 왕명과 기사도 중 하나를 택했다.",
            Conditions = new[] { CondTitleAtLeast(NobleTitleRank.Count, "백작 이상에서 왕명을 받는다") },
            WeightModifiers = new[] { WeightForTag(TagWangChongAe, 1) },
            Choices = new[]
            {
                new Vs1EventChoice
                {
                    Label = "명령을 따른다",
                    ImpactPreview = "궁정↑ · 명예↓ · 불명예 · 왕의 총애 가능",
                    ResultText = "명령은 그날 밤 안에 집행되었다. 가문의 깃발은 왕의 곁에 가까이 섰지만, 그 천에는 보이지 않는 얼룩이 남았다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.CourtInfluence, Vs1ImpactMagnitude.Moderate), Down(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagAdd(TagBulMyeongYe), TagAdd(TagWangChongAe) },
                },
                new Vs1EventChoice
                {
                    Label = "기사도를 내세워 거부한다",
                    ImpactPreview = "명예↑ · 궁정의 의심 또는 정적의 표적 · 불명예 해소",
                    ResultText = "가주는 칼을 거두었다. 포로는 살았고, 왕의 사자는 입을 굳게 닫은 채 떠났다.",
                    StateImpacts = new[] { Up(Vs1StateAxis.Honor, Vs1ImpactMagnitude.Moderate) },
                    TagChanges = new[] { TagResolve(TagBulMyeongYe) },
                    OutcomeVariants = new[]
                    {
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagGungJeongUiSim) },
                            Weight = 1,
                        },
                        new Vs1ChoiceOutcomeVariant
                        {
                            ExtraTagChanges = new[] { TagAdd(TagJeongJeokUiPyoJeok) },
                            Weight = 1,
                        },
                    },
                },
            },
        });
    }
}
