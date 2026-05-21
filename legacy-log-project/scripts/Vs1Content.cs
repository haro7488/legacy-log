// Vertical Slice 1 콘텐츠 — public API + 태그 정의 + 라벨 + 후계 preset + 빌더 헬퍼.
//
// 사건 정의는 분류별 partial 파일(Vs1Content.Estate/Honor/Court/Family/Promotion/Crisis/GenerationEnd.cs)에 둔다.
//
// 합류 계약:
// - 사건 본문, 선택지 행동명, 영향 요약, 대표 연대기 문장은 Root 기획 문서에서 그대로 옮긴다.
// - 태그 변화는 vertical-slice-1-event-tag-change-table.md 기준.
// - 새 사건 추가 또는 본문 재작성 금지.
// - 분기 사건(X 또는 Y)은 Vs1ChoiceOutcomeVariant에 두고, 조건 없는 분기는 seed 기반 fallback.

using System.Collections.Generic;

public static partial class Vs1Content
{
    // -----------------------------------------------------------------------
    // 태그 키 상수 (오타 방지)
    // -----------------------------------------------------------------------

    public const string TagWangChongAe = "왕의 총애";
    public const string TagGungJeongUiSim = "궁정의 의심";
    public const string TagJeongJeokUiPyoJeok = "정적의 표적";
    public const string TagWangSilUiChaeMu = "왕실의 채무";

    public const string TagJeonJaengGongHun = "전쟁 공훈";
    public const string TagGiSaUiMyeongYe = "기사의 명예";
    public const string TagBulMyeongYe = "불명예";
    public const string TagGeopJaengIUiSoMun = "겁쟁이의 소문";

    public const string TagPungYoRoUnYeongJi = "풍요로운 영지";
    public const string TagPiPyeHanYeongJi = "피폐한 영지";
    public const string TagSangInHuWon = "상인 후원";
    public const string TagGaMunUiBuChae = "가문의 부채";

    public const string TagAnJeongDoenHuGye = "안정된 후계";
    public const string TagBulAnHanHuGye = "불안한 후계";
    public const string TagNaeBuGyunYeol = "내부 균열";
    public const string TagChungSeongRoUnChinJok = "충성스러운 친족";

    public const string TagSeungGyeokMyeongBun = "승격 명분";
    public const string TagBulAnHanSeungGyeok = "불안한 승격";
    public const string TagBakTalWiGi = "박탈 위기";
    public const string TagByeonGyeongUiChaekMu = "변경의 책무";

    public const string TagPiUiWonHan = "피의 원한";
    public const string TagItHiJiAnNeunPaeBae = "잊히지 않는 패배";
    public const string TagGaMunUiMaengSe = "가문의 맹세";
    public const string TagMolLakUiJingJo = "몰락의 징조";

    // -----------------------------------------------------------------------
    // 후계 preset 키
    // -----------------------------------------------------------------------

    public const string HeirPresetDefault = "default";
    public const string HeirPresetCourtEducated = "court-educated";
    public const string HeirPresetEstateEducated = "estate-educated";
    public const string HeirPresetKnightEducated = "knight-educated";
    public const string HeirPresetIllegitimate = "illegitimate-acknowledged";
    public const string HeirPresetUnsettled = "unsettled";
    public const string HeirPresetLegitimateEldest = "legitimate-eldest";
    public const string HeirPresetCompetent = "competent";

    // -----------------------------------------------------------------------
    // 라벨 (UI 표시 전용)
    // -----------------------------------------------------------------------

    public static string GetStateLabel(Vs1StateAxis axis) => axis switch
    {
        Vs1StateAxis.Wealth => "재산",
        Vs1StateAxis.Honor => "명예",
        Vs1StateAxis.CourtInfluence => "궁정 영향력",
        Vs1StateAxis.HouseUnity => "가문 결속",
        _ => axis.ToString(),
    };

    public static string GetStateBandLabel(Vs1StateBand band) => band switch
    {
        Vs1StateBand.Low => "낮음",
        Vs1StateBand.Strained => "흔들림",
        Vs1StateBand.Stable => "안정",
        Vs1StateBand.Strong => "굳건",
        _ => band.ToString(),
    };

    public static string GetTitleLabel(NobleTitleRank rank) => rank switch
    {
        NobleTitleRank.Baron => "남작",
        NobleTitleRank.Viscount => "자작",
        NobleTitleRank.Count => "백작",
        NobleTitleRank.Marquess => "후작",
        NobleTitleRank.Duke => "공작",
        _ => rank.ToString(),
    };

    public static string GetTitleRiskLabel(TitleRiskStage stage) => stage switch
    {
        TitleRiskStage.Stable => "작위 안정",
        TitleRiskStage.Watched => "작위 견제",
        TitleRiskStage.Crisis => "작위 위기",
        TitleRiskStage.RevocationThreat => "박탈 위협",
        _ => stage.ToString(),
    };

    public static string GetCategoryLabel(Vs1EventCategory category) => category switch
    {
        Vs1EventCategory.Estate => "영지/재산",
        Vs1EventCategory.Honor => "명예/전쟁",
        Vs1EventCategory.Court => "궁정/정치",
        Vs1EventCategory.Family => "가문 내부/후계",
        Vs1EventCategory.Promotion => "승격 기회",
        Vs1EventCategory.Crisis => "작위 위기",
        Vs1EventCategory.GenerationEnd => "세대 종결",
        _ => category.ToString(),
    };

    public static string GetEndTypeLabel(Vs1GenerationEndType endType) => endType switch
    {
        Vs1GenerationEndType.NaturalDeath => "자연사",
        Vs1GenerationEndType.BattleDeath => "전사",
        Vs1GenerationEndType.IllnessDeath => "병사",
        Vs1GenerationEndType.ForcedAbdication => "강제 폐위",
        Vs1GenerationEndType.Extinction => "멸문",
        Vs1GenerationEndType.Other => "기타",
        _ => endType.ToString(),
    };

    public static string GetLegacyTagLabel(string key)
    {
        // 라벨이 키와 동일한 한국어 문자열이라 그대로 반환한다.
        // 별도 표시명이 필요한 태그(예: "왕실의 채무" -> "왕실의 빚")는 매핑.
        return key switch
        {
            TagWangSilUiChaeMu => "왕실의 빚",
            _ => key,
        };
    }

    // -----------------------------------------------------------------------
    // 태그 정의
    // -----------------------------------------------------------------------

    public static IReadOnlyList<Vs1LegacyTagDefinition> LegacyTags { get; } = BuildLegacyTags();

    private static IReadOnlyList<Vs1LegacyTagDefinition> BuildLegacyTags()
    {
        return new[]
        {
            // 왕권/궁정
            new Vs1LegacyTagDefinition
            {
                Key = TagWangChongAe,
                Label = "왕의 총애",
                SeriesKey = "왕권/궁정",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagGungJeongUiSim },
                GenerationStartLine = "이전 세대의 충성은 왕의 기억에 남았다. 이번 세대는 궁정의 문이 조금 더 열린 상태에서 시작한다.",
                ShortDescription = "왕실 보상과 청원 기회가 열려 있다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagGungJeongUiSim,
                Label = "궁정의 의심",
                SeriesKey = "왕권/궁정",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagWangChongAe },
                GenerationStartLine = "궁정은 아직 이 가문을 완전히 믿지 않는다. 이번 세대의 청원과 방어는 더 조심스러워야 한다.",
                ShortDescription = "궁정 사건에서 방어가 필요하다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagJeongJeokUiPyoJeok,
                Label = "정적의 표적",
                SeriesKey = "왕권/궁정",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "누군가는 이 가문의 상승을 기억하고 있다. 이번 세대는 궁정의 시선 아래에서 시작한다.",
                ShortDescription = "정치적 공격과 탄핵 위험이 높다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagWangSilUiChaeMu,
                Label = "왕실의 빚",
                SeriesKey = "왕권/궁정",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "왕실과 맺은 빚은 아직 끝나지 않았다. 이번 세대는 은혜와 부담을 함께 물려받았다.",
                ShortDescription = "왕실 의존이 다음 선택의 부담이 된다.",
            },
            // 명예/불명예
            new Vs1LegacyTagDefinition
            {
                Key = TagJeonJaengGongHun,
                Label = "전쟁 공훈",
                SeriesKey = "명예/불명예",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = new[] { TagItHiJiAnNeunPaeBae },
                GenerationStartLine = "전장에서 얻은 이름은 아직 사라지지 않았다. 이번 세대는 그 공훈을 명분으로 삼을 수 있다.",
                ShortDescription = "명예와 승격 명분으로 쓰일 수 있다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagGiSaUiMyeongYe,
                Label = "기사의 명예",
                SeriesKey = "명예/불명예",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagBulMyeongYe },
                GenerationStartLine = "선대의 명예로운 선택은 기사들의 입에 남았다. 이번 세대는 그 신뢰를 등에 업고 시작한다.",
                ShortDescription = "명예 사건에서 신뢰를 얻기 쉽다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagBulMyeongYe,
                Label = "불명예",
                SeriesKey = "명예/불명예",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = new[] { TagGiSaUiMyeongYe },
                GenerationStartLine = "선대의 오명은 쉽게 씻기지 않았다. 이번 세대는 먼저 평판을 회복해야 한다.",
                ShortDescription = "평판 회복 사건이 필요하다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagGeopJaengIUiSoMun,
                Label = "겁쟁이의 소문",
                SeriesKey = "명예/불명예",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagJeonJaengGongHun },
                GenerationStartLine = "물러섰다는 소문은 가문보다 오래 남았다. 이번 세대는 전장과 궁정에서 그 말을 견뎌야 한다.",
                ShortDescription = "전쟁과 명예 사건에서 약점으로 작용한다.",
            },
            // 영지/재산
            new Vs1LegacyTagDefinition
            {
                Key = TagPungYoRoUnYeongJi,
                Label = "풍요로운 영지",
                SeriesKey = "영지/재산",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagPiPyeHanYeongJi },
                GenerationStartLine = "선대가 다진 영지는 아직 힘을 낸다. 이번 세대는 비교적 안정된 재산 기반에서 출발한다.",
                ShortDescription = "재산 기반이 안정적이다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagPiPyeHanYeongJi,
                Label = "피폐한 영지",
                SeriesKey = "영지/재산",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagPungYoRoUnYeongJi },
                GenerationStartLine = "영지는 아직 선대의 부담을 회복하지 못했다. 이번 세대는 백성의 불만과 부족한 재산을 함께 안고 시작한다.",
                ShortDescription = "영지 불안과 탄원 사건이 늘어난다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagSangInHuWon,
                Label = "상인 후원",
                SeriesKey = "영지/재산",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "상인들은 이 가문의 가능성에 돈을 걸었다. 이번 세대는 그 후원과 이해관계 속에서 시작한다.",
                ShortDescription = "재산 기회와 외부 의존이 함께 온다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagGaMunUiBuChae,
                Label = "가문의 부채",
                SeriesKey = "영지/재산",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "빚은 장부에만 남지 않았다. 이번 세대의 선택은 먼저 채권자와 유지 비용을 의식해야 한다.",
                ShortDescription = "작위 유지와 재산 사건의 압박이 커진다.",
            },
            // 가문 내부/후계
            new Vs1LegacyTagDefinition
            {
                Key = TagAnJeongDoenHuGye,
                Label = "안정된 후계",
                SeriesKey = "가문 내부/후계",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagBulAnHanHuGye },
                GenerationStartLine = "후계는 공개적으로 인정받았다. 이번 세대는 권리 다툼보다 운영에 집중할 수 있다.",
                ShortDescription = "세대 전환이 안정적이다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagBulAnHanHuGye,
                Label = "불안한 후계",
                SeriesKey = "가문 내부/후계",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagAnJeongDoenHuGye },
                GenerationStartLine = "후계의 이름은 정해졌지만 모두가 받아들인 것은 아니다. 이번 세대는 정통성을 증명해야 한다.",
                ShortDescription = "후계권 공격과 멸문 위험이 커진다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagNaeBuGyunYeol,
                Label = "내부 균열",
                SeriesKey = "가문 내부/후계",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = new[] { TagChungSeongRoUnChinJok },
                GenerationStartLine = "가문 안의 금은 아직 닫히지 않았다. 이번 세대는 친족과 가신의 불신을 안고 시작한다.",
                ShortDescription = "친족과 가신단 갈등이 커진다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagChungSeongRoUnChinJok,
                Label = "충성스러운 친족",
                SeriesKey = "가문 내부/후계",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = new[] { TagNaeBuGyunYeol },
                GenerationStartLine = "선대가 남긴 신뢰는 친족의 충성으로 돌아왔다. 이번 세대는 내부의 손을 빌릴 수 있다.",
                ShortDescription = "내부 협력과 섭정 기회가 열린다.",
            },
            // 작위/정치적 입지
            new Vs1LegacyTagDefinition
            {
                Key = TagSeungGyeokMyeongBun,
                Label = "승격 명분",
                SeriesKey = "작위/정치적 입지",
                DefaultDuration = LegacyTagDuration.ShortTerm,
                OpposedTags = new[] { TagBakTalWiGi },
                GenerationStartLine = "선대의 공적은 더 높은 작위를 요구할 근거가 되었다. 이번 세대에는 청원의 기회가 보인다.",
                ShortDescription = "상위 작위 청원 기회가 가까워진다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagBulAnHanSeungGyeok,
                Label = "불안한 승격",
                SeriesKey = "작위/정치적 입지",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "작위는 올랐지만 기반은 아직 따라오지 못했다. 이번 세대는 새 지위를 지켜내야 한다.",
                ShortDescription = "새 작위가 아직 안정되지 않았다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagBakTalWiGi,
                Label = "박탈 위기",
                SeriesKey = "작위/정치적 입지",
                DefaultDuration = LegacyTagDuration.ShortTerm,
                OpposedTags = new[] { TagSeungGyeokMyeongBun },
                GenerationStartLine = "대표작위는 아직 남아 있지만, 그 아래의 땅은 흔들린다. 이번 세대는 상실을 막아야 한다.",
                ShortDescription = "대표작위 상실 위험이 크다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagByeonGyeongUiChaekMu,
                Label = "변경의 책무",
                SeriesKey = "작위/정치적 입지",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "변경을 맡은 이름은 명예이자 짐이다. 이번 세대는 전쟁과 방위의 압력을 피하기 어렵다.",
                ShortDescription = "전쟁과 방위 부담이 늘어난다.",
            },
            // 비극/몰락
            new Vs1LegacyTagDefinition
            {
                Key = TagPiUiWonHan,
                Label = "피의 원한",
                SeriesKey = "비극/몰락",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "피로 끝난 선택은 살아남은 자들의 기억에 남았다. 이번 세대는 복수와 불신의 씨앗을 안고 시작한다.",
                ShortDescription = "복수와 내부 폭력 사건의 불씨가 남았다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagItHiJiAnNeunPaeBae,
                Label = "잊히지 않는 패배",
                SeriesKey = "비극/몰락",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = new[] { TagJeonJaengGongHun },
                GenerationStartLine = "패배는 전장에서 끝나지 않았다. 이번 세대는 그 기억을 딛고 다시 이름을 세워야 한다.",
                ShortDescription = "전쟁 실패가 평판과 선택을 압박한다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagGaMunUiMaengSe,
                Label = "가문의 맹세",
                SeriesKey = "비극/몰락",
                DefaultDuration = LegacyTagDuration.HouseHistory,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "선대가 남긴 맹세는 아직 가문을 묶고 있다. 이번 세대는 그 약속을 지키거나 배신해야 한다.",
                ShortDescription = "결속과 장기 목표의 근거가 된다.",
            },
            new Vs1LegacyTagDefinition
            {
                Key = TagMolLakUiJingJo,
                Label = "몰락의 징조",
                SeriesKey = "비극/몰락",
                DefaultDuration = LegacyTagDuration.Generation,
                OpposedTags = System.Array.Empty<string>(),
                GenerationStartLine = "지난 세대의 끝은 쇠락의 예고처럼 남았다. 이번 세대는 멸문을 피할 길을 찾아야 한다.",
                ShortDescription = "멸문과 쇠락 위험이 가까워진다.",
            },
        };
    }

    private static Dictionary<string, Vs1LegacyTagDefinition>? _legacyTagMap;
    public static Vs1LegacyTagDefinition? GetLegacyTagDefinition(string key)
    {
        if (_legacyTagMap == null)
        {
            _legacyTagMap = new Dictionary<string, Vs1LegacyTagDefinition>();
            foreach (var t in LegacyTags) _legacyTagMap[t.Key] = t;
        }
        return _legacyTagMap.TryGetValue(key, out var def) ? def : null;
    }

    // -----------------------------------------------------------------------
    // 후계 preset
    // -----------------------------------------------------------------------

    public static Vs1HeirProfile BuildHeirProfile(string presetKey, string name)
    {
        return presetKey switch
        {
            HeirPresetCourtEducated => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "court-minded",
                DispositionLabel = "궁정 지향",
                StrengthKey = "court-savvy",
                StrengthLabel = "궁정 처세",
                WeaknessKey = "field-inexperienced",
                WeaknessLabel = "현장 미숙",
                SuccessionReason = "궁정 교육을 받은 후계자다.",
            },
            HeirPresetEstateEducated => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "pragmatic",
                DispositionLabel = "실리적",
                StrengthKey = "estate-management",
                StrengthLabel = "영지 경영",
                WeaknessKey = "court-cold",
                WeaknessLabel = "궁정 미숙",
                SuccessionReason = "영지 경영을 배운 후계자다.",
            },
            HeirPresetKnightEducated => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "honor-bound",
                DispositionLabel = "명예 중시",
                StrengthKey = "battlefield-command",
                StrengthLabel = "전장 지휘",
                WeaknessKey = "politics-naive",
                WeaknessLabel = "정치 미숙",
                SuccessionReason = "기사 훈련을 거친 후계자다.",
            },
            HeirPresetIllegitimate => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "ambitious",
                DispositionLabel = "야심",
                StrengthKey = "outsider-resilience",
                StrengthLabel = "비주류의 강인함",
                WeaknessKey = "legitimacy-doubted",
                WeaknessLabel = "정통성 의심",
                SuccessionReason = "사생아로 인정받아 후계가 되었다.",
            },
            HeirPresetUnsettled => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "uncertain",
                DispositionLabel = "흔들리는 의지",
                StrengthKey = "cautious",
                StrengthLabel = "조심성",
                WeaknessKey = "indecisive",
                WeaknessLabel = "결단 부족",
                SuccessionReason = "후계 다툼 끝에 결정이 미뤄진 후계자다.",
            },
            HeirPresetLegitimateEldest => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "traditional",
                DispositionLabel = "전통 존중",
                StrengthKey = "ceremonial-weight",
                StrengthLabel = "정통의 무게",
                WeaknessKey = "untested",
                WeaknessLabel = "검증 부족",
                SuccessionReason = "장자 상속으로 후계가 되었다.",
            },
            HeirPresetCompetent => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "ambitious",
                DispositionLabel = "야심",
                StrengthKey = "all-rounder",
                StrengthLabel = "균형 잡힌 능력",
                WeaknessKey = "factional-strain",
                WeaknessLabel = "파벌 마찰",
                SuccessionReason = "능력 본위로 후계가 결정된 후계자다.",
            },
            _ => new Vs1HeirProfile
            {
                Name = name,
                DispositionKey = "balanced",
                DispositionLabel = "균형",
                StrengthKey = "even-hand",
                StrengthLabel = "고른 손",
                WeaknessKey = "no-standout",
                WeaknessLabel = "두드러진 강점 없음",
                SuccessionReason = "전통적인 방식으로 후계가 정해졌다.",
            },
        };
    }

    // -----------------------------------------------------------------------
    // 초기 가문 생성
    // -----------------------------------------------------------------------

    public const string PlaceholderFamilyName = "가문 A";

    public static Vs1FamilyState BuildInitialFamily(int seed)
    {
        var firstProfile = BuildHeirProfile(HeirPresetDefault, "인물 1");
        return new Vs1FamilyState(
            seed: seed,
            familyName: PlaceholderFamilyName,
            firstCharacterName: "인물 1",
            firstProfile: firstProfile,
            startTitle: NobleTitleRank.Baron);
    }

    public static string GetCharacterNameForGeneration(int generationNumber)
    {
        return $"인물 {generationNumber}";
    }

    // -----------------------------------------------------------------------
    // 사건 목록 — 분류별 partial에서 채운다.
    // -----------------------------------------------------------------------

    private static IReadOnlyList<Vs1EventDefinition>? _allEvents;

    public static IReadOnlyList<Vs1EventDefinition> AllEvents
    {
        get
        {
            if (_allEvents == null)
            {
                var list = new List<Vs1EventDefinition>(46);
                AddEstateEvents(list);
                AddHonorEvents(list);
                AddCourtEvents(list);
                AddFamilyEvents(list);
                AddPromotionEvents(list);
                AddCrisisEvents(list);
                AddGenerationEndEvents(list);
                _allEvents = list;
            }
            return _allEvents;
        }
    }

    private static Dictionary<string, Vs1EventDefinition>? _eventMap;
    public static Vs1EventDefinition? GetEventById(string id)
    {
        if (_eventMap == null)
        {
            _eventMap = new Dictionary<string, Vs1EventDefinition>();
            foreach (var e in AllEvents) _eventMap[e.Id] = e;
        }
        return _eventMap.TryGetValue(id, out var def) ? def : null;
    }

    // -----------------------------------------------------------------------
    // 데이터 빌더 헬퍼 (사건 정의를 짧게 쓰기 위한 단축 함수)
    // -----------------------------------------------------------------------

    internal static Vs1StateImpact Impact(Vs1StateAxis axis, Vs1ImpactDirection direction, Vs1ImpactMagnitude magnitude)
    {
        return new Vs1StateImpact { Axis = axis, Direction = direction, Magnitude = magnitude };
    }

    internal static Vs1StateImpact Up(Vs1StateAxis axis, Vs1ImpactMagnitude magnitude = Vs1ImpactMagnitude.Minor)
    {
        return Impact(axis, Vs1ImpactDirection.Up, magnitude);
    }

    internal static Vs1StateImpact Down(Vs1StateAxis axis, Vs1ImpactMagnitude magnitude = Vs1ImpactMagnitude.Minor)
    {
        return Impact(axis, Vs1ImpactDirection.Down, magnitude);
    }

    internal static Vs1LegacyTagChange TagAdd(string tagKey, string displayText = "")
    {
        return new Vs1LegacyTagChange
        {
            Kind = Vs1LegacyTagChangeKind.Add,
            TagKey = tagKey,
            DisplayText = string.IsNullOrEmpty(displayText) ? $"새 유산: {GetLegacyTagLabel(tagKey)}" : displayText,
        };
    }

    internal static Vs1LegacyTagChange TagResolve(string tagKey)
    {
        return new Vs1LegacyTagChange
        {
            Kind = Vs1LegacyTagChangeKind.Resolve,
            TagKey = tagKey,
            DisplayText = $"해소된 유산: {GetLegacyTagLabel(tagKey)}",
        };
    }

    internal static Vs1LegacyTagChange TagReplace(string newKey, string replacedKey)
    {
        return new Vs1LegacyTagChange
        {
            Kind = Vs1LegacyTagChangeKind.Replace,
            TagKey = newKey,
            ReplacedTagKey = replacedKey,
            DisplayText = $"교체: {GetLegacyTagLabel(replacedKey)} -> {GetLegacyTagLabel(newKey)}",
        };
    }

    internal static Vs1LegacyTagChange TagWeaken(string tagKey)
    {
        return new Vs1LegacyTagChange
        {
            Kind = Vs1LegacyTagChangeKind.Weaken,
            TagKey = tagKey,
            DisplayText = $"약화된 유산: {GetLegacyTagLabel(tagKey)}",
        };
    }

    internal static Vs1EventCondition CondTitleAtLeast(NobleTitleRank min, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.Title,
            MinTitle = min,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondTitleRange(NobleTitleRank min, NobleTitleRank max, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.Title,
            MinTitle = min,
            MaxTitle = max,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondStateLow(Vs1StateAxis axis, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.StateBand,
            StateAxis = axis,
            StateBandAtMost = Vs1StateBand.Strained,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondStateHigh(Vs1StateAxis axis, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.StateBand,
            StateAxis = axis,
            StateBandAtLeast = Vs1StateBand.Stable,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondHasTag(string tagKey, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.ActiveTag,
            RequiredTagKey = tagKey,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondMissingTag(string tagKey, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.MissingTag,
            ForbiddenTagKey = tagKey,
            DisplayReason = reason,
        };
    }

    internal static Vs1EventCondition CondMinGeneration(int minGeneration, string reason)
    {
        return new Vs1EventCondition
        {
            Kind = Vs1ConditionKind.Generation,
            MinGeneration = minGeneration,
            DisplayReason = reason,
        };
    }

    internal static Vs1WeightModifier WeightForTag(string tagKey, int delta)
    {
        return new Vs1WeightModifier
        {
            TriggerKind = Vs1ConditionKind.ActiveTag,
            TriggerKey = tagKey,
            Delta = delta,
        };
    }

    internal static Vs1WeightModifier WeightForTitleAtLeast(NobleTitleRank min, int delta)
    {
        return new Vs1WeightModifier
        {
            TriggerKind = Vs1ConditionKind.Title,
            TriggerTitleAtLeast = min,
            Delta = delta,
        };
    }

    internal static Vs1WeightModifier WeightForStateLow(Vs1StateAxis axis, int delta)
    {
        return new Vs1WeightModifier
        {
            TriggerKind = Vs1ConditionKind.StateBand,
            TriggerKey = axis.ToString() + ":low",
            Delta = delta,
        };
    }
}
