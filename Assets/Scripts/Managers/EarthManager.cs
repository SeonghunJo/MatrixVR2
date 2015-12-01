using UnityEngine;
using System.Collections.Generic;

//Earth Scene Manager
public class EarthManager
{
    // FOR CAMERA RIG
    public Vector3 CameraRotation;
    public Quaternion CameraOrientation;

    public bool showGuide;
    public static List<Information> panoramas = new List<Information>();

    private static EarthManager _instance;
	public static EarthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new EarthManager();
                Initialize();
            }
            return _instance;
        }
    }

    // 추후 DB로 교체
    private static void Initialize()
    {
        // 네팔
        panoramas.Add(new Information(27.718398f, 86.716000f, "kFuSB9hD_6yJy8oxmjr1RQ", "Mudslide Bridge", "네팔", "사가르마타", "사가르마타에는 세계 최고봉(8,848m)인 에베레스트 산이 높이 솟아 있으며 그 외에도 여러 개의 높은 봉우리와 빙하, 깊은 계곡이 있다. 이곳에서는 눈 표범, 레서 판다와 같은 희귀종이 발견되었다. 독특한 문화를 갖고 있는 셰르파는 이 지역에 대한 흥미를 더해 준다."
                                       , "Nepal", false)); //Mudslide Bridge
        // 대만
        panoramas.Add(new Information(25.036556f, 121.517962f, "uM-jW1fT93Y_eJ7CPTG6jA", "장개석 기념관", "대만", "타이베이", "타이베이 시 중정구에 위치해 있는 장개석 기념관(중정기념당)은 중화민국의 초대 총통이었던 장개석을 기념해 1980년에 건설한 기념관이다."
                                       , "Taiwan", false)); //장개석 기념관

        // 대한민국
        panoramas.Add(new Information(37.571199f, 126.968461f, "zMrHSTO0GCYAAAQINlCkXg", "경희궁", "대한민국", "서울특별시 종로구", "경희궁은 서울시에 있는 조선 시대 궁궐로 광해군 10년(1623년)에 건립한 이후, 10대에 걸쳐 임금이 정사를 보았던 궁궐이다. 경복궁, 창경궁과 함께 조선왕조의 3대궁으로 꼽힐 만큼 큰 궁궐이었으나 일제강점기에 심하게 훼손되어 현재 남아있는 건물은 정문이었던 흥화문과 정전이었던 숭정전, 그리고 후원의 정자였던 황학정까지 세 채에 불과하다. 그나마 초석과 기단이 남아 있고, 뒤쪽에는 울창한 수림이 잘 보전돼 있어 궁궐의 자취를 잘 간직하고 있는 편이다."
                                       , "SouthKorea", false)); //Gyeonghui Palace
        panoramas.Add(new Information(33.459519f, 126.939750f, "IUmXlW6pRu1w9QnUdQk4vw", "성산일출봉", "대한민국", "제주 서귀포시", "성산 일출봉은 서귀포시 성산읍에 있는 산이다. 분화구 높이는 182m 이며, 성산 일출봉에서의 일출은 영주십경 중 하나이다. 일출봉 분화구와 주변 1km 해역은 성산 일출봉 천연보호구역으로 대한민국의 천연기념물로 지정되어 있다."
                                       , "SouthKorea", false)); //성산일출봉

        // 미국
        panoramas.Add(new Information(37.172210f, -93.322539f, "ayOK2SvpqYzlk4rm1Q4KPQ", "Mizumoto Japanese Stroll Garden", "미국", "캘리포니아", "일본식 정원을 재현해 놓은 재패니즈 가든. 황제와 귀족의 정원은 레크레이션 및 미적 쾌락을 위해 설계되었으나 종교적인 정원은 사색과 명상을 위해 설계되었다."
                                       , "America", true)); //Mizumoto Japanese Stroll Garden
        panoramas.Add(new Information(-3.825792f, -32.396424f, "rfgb-QoM16gAAAQY_-2qjg", "Catlin Seaview Survey", "미국", "미국", "해경 조사 지역으로 과학적 탐구를 위한 수많은 산호초 연구가 이루어지고 있고, 많은 종류의 물고기와 돌고래를 관측할 수 있다."
                                       , "America", true)); //돌고래 Catlin Seaview Survey
        panoramas.Add(new Information(40.688619f, -74.044125f, "cthAMoR7m9cmP-wUC8AIWA", "자유의 여신상", "미국", "뉴욕", "미국과 프랑스 국민들 간의 친목을 기념하고, 미국의 독립 100주년을 기념하기 위해 프랑스인들의 모금운동으로 증정되었으며, 1886년에 완공되었다. 미국의 자유와 민주주의의 상징이고, 1984년에는 유네스코 세계문화유산에 등록되었다."
                                       , "America", true)); //Statue of Liberty National Monument
        panoramas.Add(new Information(-14.865306f, 145.680527f, "nNw59vW3owAAAAQfCaaiYQ", "Minke Whales", "미국", "미국", "일반적으로 대형 긴수염고래류와는 형태적으로 쉽게 구별된다. 머리는 옆이나 위에서 보아도 뾰족하게 나있다. 등지느러미는 높고, 뒤로 굽혀있으며 몸길이의 2/3 정도에 위치해 있다. 몸 색은 특징적으로 등은 검은색이 나는 회색이고 배는 흰색이다. 가슴지느러미 중앙을 가로지르는 흰 문양이 특징적이나 서식해역에 따라 차이가 있다."
                                       , "America", true)); //Minke Whales

        // 브라질
        panoramas.Add(new Information(-3.137761f, -60.493355f, "1ci-8iBT_UuG1dlrUy1vzg", "Rio Negro", "브라질", "브라질 북서부", "네그루 강은 아마존 강 북부의 지류 중 가장 크고, 검은색을 띤 강으로는 세계 최대의 길이와 유량을 가지고 있다. 하천의 이름 네그루란 ‘검다’는 뜻으로, 식물의 잔해가 완전히 분해되지 못한 상태에서 발생하는 유기산 때문이다. 검은색 강과 흰색 강이 만나 섞이지 않고 흐르는 현상, 물속에서 자라는 숲 이가포 등 네그루 강의 독특한 생태계를 경험하기 위해 많은 사람들이 찾고 있다."
                                       , "Brazil", true)); //Rio Negro 아마존

        // 아랍에미리트 연합
        panoramas.Add(new Information(25.195232f, 55.276428f, "eznax7JvTOXrAztKUfqj-A", "Burj Khalifa", "아랍에미리트 연합", "두바이", "부르즈 할리파는 아랍에미리트 두바이의 신도심 지역에 있는 높이 829.8미터의 초고층 건물이다. 완공 이전 이름은 부르즈 두바이로, 아랍에미리트의 대통령인 할리파 빈 자이드 알나하얀의 이름을 본따 부르즈 두바이에서 부르즈 할리파로 개명되었다. 현재까지 완성된 초고층 건물 중에서 가장 높아 지상층에서 최고층까지 초고속 엘리베이터로 약 1분이 걸리며, 세계에서 가장 높은 인공 구조물이다."
                                       , "Arab", true)); //Burj Khalifa

        // 영국
        panoramas.Add(new Information(51.178946f, -1.826564f, "VxzhBNNu-VGQC8HtVIaY3A", "스톤헨지", "영국", "윌트셔주 솔즈베리 평원", "스톤헨지는 선사 시대의 거석기념물에 있는 원형으로 배치된 유적이다. 높이 8미터, 무게 50톤인 거대 석상 80여 개가 세워져 있다. 수수께끼의 선사시대 유적으로 누가, 어떻게, 왜 만들었는가에 대한 의문이 풀려지지 않고 있다."
                                       , "England", true)); //스톤헨지

        // 이탈리아
        panoramas.Add(new Information(46.162368f, 10.917272f, "5_AWZhbCGuqMd0AGfOlOUw", "동오미티 벨루네시 국립공원", "이탈리아", "베네토주 벨루노", "수천 년 전부터 사람이 살아온 흔적이 발견되었는데, 그중 발레 임페리나의 탄광 중심지에 있는 몇 곳의 선사시대 고고학 유적이 중요하다. 그밖에 카르투지오 수도회의 수도원, 산록지대의 작은 교회들, 중세시대의 여행자 숙박소, 제1차 세계대전 당시의 군사도로, 양치기 헛간 등 중세를 거쳐 현대에 이르는 다양한 유적이 남아 있다. 현재 유네스코의 세계문화유산으로 지정되어있다."
                                       , "Italy", true)); // Dolomiti UNESCO2
        panoramas.Add(new Information(41.890072f, 12.492534f, "07gbqMWIg_HId5m7W94qHg", "콜로세움", "이탈리아", "로마", "콜로세움은 고대 로마 시대의 건축물 가운데 하나로 로마 제국 시대에 만들어진 원형 경기장이다. 현재는 로마를 대표하는 유명한 관광지로 탈바꿈하였다. 콜로세움이라는 이름은 근처에 있었던 네로 황제의 동상에서 유래한다. 원래 이름은 플라비우스 원형 극장이었다."
                                       , "Italy", false)); //로마 콜로세움

        // 인도
        panoramas.Add(new Information(15.949466f, 75.815568f, "oG_6nL1zn2L5SiHB7fEqBQ", "Pattadakal", "인도", "남부 카르나타카주", "파타다칼은 인도 카르나타카 주 북부에 위치한 마을로, 8세기에 건설된 힌두교 사원 유적으로 유명한 곳이다. 1987년 유네스코가 지정한 세계유산으로 선정되었으며 북부의 드라비다인 건축 양식과 남부의 아리아인 건축 양식이 혼합된 것이 특징이다."
                                       , "India", false)); //Group of Mounments at Pattadakal
        panoramas.Add(new Information(19.900922f, 75.320179f, "PxmiVxloPR5XpPOC3sDb3A", "Bibi Ka Maqbara", "인도", "아우랑가바드", "비비 까 마끄바라(여자의 무덤)은 무굴황제 아우랑제브의 첫 번째 부인인 라비아 웃 다우라니의 무덤으로 큰 아들인 아잠 샤에 의해 건설되었다. 아잠 샤의 친할머니 뭄 따즈마할의 묘, 타지마할을 본 따 만들어 졌으며 작은 타지마할로 불린다."
                                       , "India", true)); //Bibi Ka Maqbara

        // 인도네시아
        panoramas.Add(new Information(-8.737039f, 119.412259f, "Ri71LYeNvGsAAAQW4U3bQQ", "코모도섬", "인도네시아", "누사텡가라티무르 주", "발리에서 동쪽으로 483킬로미터 떨어진 플로레스 섬과 숨바와 섬 사이에 있는 코모도 섬은 코모도왕도마뱀 2,500여 마리의 서식지이다. 코모도 섬은 자바, 발리, 수마트라 같은 큰 섬과 파드라와 린카 같은 작은 섬들을 만든 거대한 화산작용으로 생성되었다. 이 섬은 1980년에는 국립공원으로, 1992년에는 세계유산으로 지정되었다."
                                       , "Indonesia", true)); //Komodo Island

        // 중국
        panoramas.Add(new Information(34.557692f, 117.742510f, "rY5ZW6S3sFUAAAAGOzOdpg", "타이얼좡 고성", "중국", "산둥성", "중국의 남방과 북방이 만나는 접점지대의 옛 마을인 조장의 타이얼좡. 중국의 남방문화와 북방문화 등 다양한 지역 문화를 볼 수 있다. 폐허가 된 고성을 2008년에 중건하기로 선포하고 공사기간을 거쳐 2010년 5월부터 정식으로 여행객을 받게 되었다."
                                       , "China", true)); //Datamen Street

        // 프랑스
        panoramas.Add(new Information(48.861780f, 2.288130f, "psUz8wNiVzzOSuElXeSG0g", "에펠탑", "프랑스", "파리", "에펠탑은 1889년 파리 마르스 광장에 지어진 탑이다. 프랑스의 대표 건축물인 이 탑은 격자 구조로 이루어져 파리에서 가장 높은 건축물이며, 매년 수백만 명이 방문할 만큼 세계적인 유료 관람지이다. 이를 디자인한 귀스타브 에펠의 이름에서 명칭을 얻었으며, 1889년 프랑스 혁명 100주년 기념 세계 박람회의 출입 관문으로 건축되었다."
                                       , "France", true)); //에펠탑

        // 핀란드
        panoramas.Add(new Information(68.509006f, 27.481845f, "GtnAyp0MCbYAAAQZLDcQIA", "북극광", "핀란드", "라피주", "오로라라고도 불리는 북극광은 남반구의 오스트레일리아 오로라와 함께 자연이 우리에게 보내는 가장 아름다운 선물 중 하나이다. 매년 북극 라플란드의 하늘에는 신비로운 오로라가 펼쳐지는데 그 횟수는 200회에 달한다. 이 현상은 1초에 1,000킬로미터의 속도로 확산되는 플라스마의 흐름인 태양풍과 지구 자기장 사이에 일어나는 상호작용의 결과이다."
                                       , "Finland", true)); //Northern Lights

        // 호주
        panoramas.Add(new Information(21.479702f, -86.632599f, "xq5H2tvkw_AAAAQJLmg-7A", "고래 상어", "호주", "호주", "고래상어는 가장 큰 어류로 길이가 15m, 무게 40톤에 달한다. 넓이 1.5m에 달하는 큰 입과 작은 눈, 흰색의 줄무늬와 점무늬가 특징이다. 어류인데도 포유류인 고래 이름을 붙여 고래상어로 불리는 것은 덩치가 고래만큼 크고 먹이 사냥 방식 또한 수염고래를 닮았기 때문이다."
                                       , "Australia", true)); //Whale Sharks
    }
}
