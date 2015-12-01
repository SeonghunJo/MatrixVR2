
public class Information
{
    public float lat;       //위도
    public float lng;       //경도
    public string panoid;   //파노라마 ID
    public bool isFamous;   //추천 지역(T/F)

    public string title;
    public string country;
    public string area;
    public string contents;
    public string info_flag_path;

    public Information(float lat, float lng, string panoid, bool isFamous = true)
    {
        this.lat = lat;
        this.lng = lng;
        this.panoid = panoid;
        this.isFamous = isFamous; //  isFamous;
    }

    public Information(float lat, float lng, string panoid, string title,
                        string country, string area, string contents, string info_flag_path, bool impo = true)
    {
        this.lat = lat;
        this.lng = lng;
        this.panoid = panoid;
        this.isFamous = impo; //  impo;

        this.title = title;
        this.country = country;
        this.area = area;
        this.contents = contents;
        this.info_flag_path = info_flag_path;
    }

}
