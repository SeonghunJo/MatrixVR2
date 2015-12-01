using UnityEngine;
using System.Collections;
using LitJson;

using System;
using System.Text.RegularExpressions;

// 각 포인트에 대한 위치 정보 및 썸네일 정보를 가지고 있는 클래스


public class StreetViewPoint : MonoBehaviour
{
    //해당 point의 위도, 경도, 파노라마 ID를 저장
    public float Lat;
    public float Lng;
    public string panoID;
	public bool isFamous;

    //정보창 여행지 정보 
    public string info_title;
    public string info_country;
    public string info_area;
    public string info_contents;
    public string info_flag_path;

    //Thumbnail image URL & Streetview name URL
    public string metaURL = "http://maps.google.com/cbk?output=json&panoid=";
    public string thumbnailURL = "http://maps.google.com/cbk?output=thumbnail&panoid=";
    
	public Texture2D thumbTexture;
	public string thumbTitle;

    // For Wikipedia
    public static string wikiURL = "http://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&exsentences=1&exlimit=1&exintro=1&explaintext=1&exsectionformat=plain&titles=";
    public static string wikiSearchURL = "http://en.wikipedia.org/w/api.php?action=query&list=search&format=json&srprop=snippet&srsearch=";

    public string searchKeyword = null;
    public string wikiText = null;
    public bool retrieveMetaData = false;

    public string region = null;
    public string country = null;
    public string description = null;

    private StreetViewManager SVManager;
    private Thumbnail thumbnail;

	// Use this for initialization
    IEnumerator Start() {

        SVManager = StreetViewManager.Instance;
        thumbnail = Thumbnail.Instance();

        GestureController.OnCursorOver += GestureController_OnCursorOver;
        GestureController.OnCursorOut += GestureController_OnCursorOut;
        GestureController.OnClicked += GestureController_OnClicked;

        yield return StartCoroutine(GetThumbnailImage(thumbnailURL));

        while (!retrieveMetaData)
        {
            yield return StartCoroutine(GetLocationText(metaURL + panoID));
        }
        
        //StartCoroutine(GetWikiKeyword(wikiSearchURL + searchKeyword));
        //StartCoroutine(GetWikiData(wikiURL + searchKeyword));
    }

    void OnDestroy()
    {
        GestureController.OnCursorOver -= GestureController_OnCursorOver;
        GestureController.OnCursorOut -= GestureController_OnCursorOut;
        GestureController.OnClicked -= GestureController_OnClicked;
    }

    private void GestureController_OnClicked(GameObject g)
    {
        if(g == gameObject)
        {
            SVManager.PlaceID = panoID;
            SVManager.PlaceImage = thumbTexture;

            SVManager.title = info_title;
            SVManager.country = info_country;
            SVManager.area = info_area;
            SVManager.contents = info_contents;
            SVManager.info_flag_path = info_flag_path;

            Application.LoadLevel("StreetViewer");
        }
    }

    private void GestureController_OnCursorOut(GameObject g)
    {
        thumbnail.Hide();
    }

    private void GestureController_OnCursorOver(GameObject g)
    {
        if(g == gameObject)
        {
            Debug.Log("Cursor Over : " + country);

            thumbnail.Title = thumbTitle;
            thumbnail.RawImage = thumbTexture;
            
            print(wikiText);

            if (wikiText != null)
                thumbnail.WikiText = wikiText;
            else
                thumbnail.WikiText = "NULL";

            thumbnail.Show();
        }
    }
    
	public void SetPosition(Information panoInfo)
    {
		Vector3 _rotation = new Vector3(panoInfo.lat, -panoInfo.lng, 0.0f);
		Vector3 _translate = new Vector3(0, 0, -37.5f);

        Lat = _rotation.x;
        Lng = -(_rotation.y);
		panoID = panoInfo.panoid;
		isFamous = panoInfo.isFamous;

		thumbnailURL = thumbnailURL + panoID;

		Debug.Log(Lat + " " + Lng + " " + panoID );

        transform.Rotate(Vector3.up, 5);
        transform.Rotate(Vector3.back, 15);
        transform.Rotate(_rotation);
        transform.Translate(_translate);
    }

    IEnumerator GetThumbnailImage(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Thumbnail Download Failed. " + www.error);
            yield break;
        }

        // 다운로드 받은 썸네일 이미지 
        thumbTexture = www.texture;
    }


    IEnumerator GetLocationText(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("PanoID : " + panoID + " WWW Error [Meta Data] : " + www.error);
            retrieveMetaData = false;
            yield break;
        }
        else
        {
            retrieveMetaData = true;
        }
		
		string locationText = "";
		JsonData json = JsonMapper.ToObject(www.text);
		
		JsonData location = json["Location"];

        if (location.Keys.Contains("description") && !string.IsNullOrEmpty(location["description"].ToString().Trim()))
		{
			description = location["description"].ToString();
			locationText += description;

            searchKeyword = locationText;
		}
        if (location.Keys.Contains("country") && !string.IsNullOrEmpty(location["country"].ToString().Trim()))
		{
			country = location["country"].ToString();
            if (!string.IsNullOrEmpty(locationText))
                locationText += ", ";
			locationText += country;
		}
        if (location.Keys.Contains("region") && !string.IsNullOrEmpty(location["region"].ToString().Trim()))
		{
			region = location["region"].ToString();
            if (!string.IsNullOrEmpty(locationText))
                locationText += ", ";
            locationText += region;
		}
        
        if(!string.IsNullOrEmpty(description)) // 메타데이터에 세부 위치 정보가 포함되어있으면
        {
            searchKeyword = description; // 해당 장소이름을 키워드로 설정
        }
        else
        {
            searchKeyword = country; // 해당 국가를 키워드로 설정
        }

        searchKeyword = searchKeyword.Replace(" ", "%20"); // 위키피디아 검색을 위해 공백을  '_' 로 치환
		thumbTitle = locationText;
    }

    string WikiDataNormalizeBySplit(string wikiString) // Normalize by Split
    {
        Regex regex = new Regex("\\([^)]+\\)");
        string[] words = regex.Split(wikiString);

        string result = "";
        for(int i=0; i<words.Length; i++)
        {
            result += words[i];
        }
        return result;
    }

    string WikiDataNormalizeByMatches(string wikiString) // Normalize by Matching
    {
        Regex regex = new Regex("([^()])+(?=\\(|$)");
        MatchCollection matches = regex.Matches(wikiString);

        string result = null;
        if (matches.Count > 0)
        {
            foreach (Match match in matches)
                result += match.Value;
        }
        return result;
    }

    IEnumerator GetWikiKeyword(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("WWW Error [GetWikiData] : " + www.error);
            yield break;
        }

        JsonData json = JsonMapper.ToObject(www.text);

        if(json.Keys.Contains("query"))
        {
            JsonData data = json["query"];

            int hits = Convert.ToInt32(data["searchinfo"]["totalhits"].ToString());
            Debug.Log("Hits " + searchKeyword + " : " + hits.ToString());

            if (hits > 0)
            {
                Debug.Log("Origin Search Keyword : " + searchKeyword);
                JsonData search = data["search"];
                searchKeyword = search[0]["title"].ToString();

                searchKeyword = searchKeyword.Replace(' ', '_');
                Debug.Log("New Search Keyword : " + searchKeyword);
            }  
        }
    }

    IEnumerator GetWikiData(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("WWW Error [GetWikiData] : " + www.error);
            yield break;
        }

        JsonData json = JsonMapper.ToObject(www.text);
        if(json.Keys.Contains("query"))
        {
            JsonData data = json["query"];

            JsonData pages = data["pages"];

            if (pages.Count > 0)
            {
                JsonData page = pages[0];

                if (page.Keys.Contains("extract"))
                {
                    wikiText = WikiDataNormalizeBySplit(page["extract"].ToString());
                }
            }
        }

    }
}
