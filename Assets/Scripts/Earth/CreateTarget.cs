using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class CreateTarget : MonoBehaviour
{
    private StreetViewManager SVManager;
    private EarthManager earthManager;

    public GameObject greatSpotObject;
    public GameObject spotObject;
    public GameObject earth; 

    //입력된 값을 float로 저장할 변수
    // Use this for initialization
    void Start()
    {
        earthManager = EarthManager.Instance;
        SVManager = StreetViewManager.Instance;

        for (int i = 0; i < EarthManager.panoramas.Count; i++)
        {           
			Information info = EarthManager.panoramas[i];
            
			//추천지역은 타겟의 모양을 다르게 			
			GameObject pin = Instantiate( info.isFamous ? greatSpotObject : spotObject , transform.position, Quaternion.identity) as GameObject;
            pin.transform.parent = earth.transform;

            StreetViewPoint point = pin.GetComponent<StreetViewPoint>();
            point.SetPosition(info); // 파라미터 2개 추가
            point.info_area = info.area;
            point.info_contents = info.contents;
            point.info_country = info.country;
            point.info_flag_path = info.info_flag_path;
            point.info_title = info.title;

            if (  SVManager.enableAutoImageCache
                && !Utility.FindCachedImageFromID(info.panoid)
			    && info.isFamous ) {

                SVManager.IsHotPlace = info.isFamous;
                SVManager.PlaceID = info.panoid;
				Application.LoadLevel("StreetViewer");
			}

        }
    }
}
