using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.IO; // For File Class

// SINGLETON MANAGER CLASS FOR ALL SCENES AND SCRIPTS
// Street Viewer Scene Manager
public class StreetViewManager {
	
	private static StreetViewManager _instance;
	public static StreetViewManager Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = new StreetViewManager();
			}
			return _instance;
		}
	}

    // INFORMATION
    public string title;
    public string country;
    public string area;
    public string contents;
    public string info_flag_path;


    // FOR STREETVIEWER.SCENE VARIABLES AND FUNCTIONS
    private Texture2D placeImage;

	public string[] nextIDs;
	public string[] nextDegrees;

    public bool enableAutoImageCache = false; // if true, auto image caching

    private string placeID;
    private bool isHotPlace;

    public bool IsHotPlace
    {
        get
        {
            return isHotPlace;
        }

        set
        {
            isHotPlace = value;
        }
    }

    public string PlaceID
    {
        get
        {
            return placeID;
        }

        set
        {
            placeID = value;
        }
    }

    public Texture2D PlaceImage
    {
        get
        {
            return placeImage;
        }

        set
        {
            placeImage = value;
        }
    }
}