using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Text;

public class LoadingScreen : MonoBehaviour {

	public GameObject uiBackground;
	public GameObject uiPlaceText;
	public GameObject uiPlaceImage;
    public GameObject uiProgress;
    public GameObject uiComment;
    
    static private LoadingScreen instance;
    public static LoadingScreen Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(LoadingScreen)) as LoadingScreen;
            if (!instance)
                Debug.LogError("There needs to be one active LoadingScreen script on a GameObject in your scene.");

            instance.gameObject.SetActive(false);
        }

        return instance;
    }

    private string placeText;
    public string PlaceText
    {
        get { return placeText; }
        set
        {
            if(uiPlaceText)
            {
                placeText = value;
                uiPlaceText.GetComponent<Text>().text = placeText;
            }
        }
    }

    private Texture2D placeImage;
    public Texture2D PlaceImage
    {
        get { return placeImage; }
        set
        {
            if(uiPlaceImage)
            {
                placeImage = value;
                uiPlaceImage.GetComponent<RawImage>().texture = placeImage;
            }
        }
    }


    private int progress;
    public int Progress
    {
        get { return progress; }
        set
        {
            progress = value;
            uiProgress.GetComponent<Text>().text = progress + "%";
        }
    }

    void Awake()
	{
        Hide();
	}
    
    public void Load(int index)
	{
		if(NoInstance()) return;

		Show();
		Application.LoadLevel(index);
		Hide();
	}

	public void Load(string name)
	{
		if(NoInstance()) return;
		Show();
		Application.LoadLevel(name);
		Hide();
	}

	public void Show()
	{
		if(NoInstance()) return;

        PlaceImage = StreetViewManager.Instance.PlaceImage;
        gameObject.SetActive(true);
    }

	public void Hide()
	{
		if(NoInstance()) return;

        gameObject.SetActive(false);
    }

	static bool NoInstance()
	{
		if(!instance)
		{
			Debug.LogError("Loading Screen is not existing in scence.");
			return true;
		}
		return false;
	}

    public void IncreaseProgress(int val = 1)
    {
        progress += val;
        uiProgress.GetComponent<Text>().text = progress + "%";
    }

    private string comment;
    public string Comment
    {
        get { return comment; }
        set
        {
            if(uiComment)
            {
                comment = value;
                uiComment.GetComponent<Text>().text = comment;
            }
        }
    }
}


/*
public class LoadingScreen : MonoBehaviour {

	public string levelToLoad;

	public GameObject uiBackground;
	public GameObject text;
	public GameObject progressBar;

	private int loadProgress = 0;

	// Use this for initialization
	void Start () {
		uiBackground.SetActive(false);
		text.SetActive(false);
		progressBar.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("space")) {
			StartCoroutine(DisplayLoadingScreen(levelToLoad));
		}
	}

	IEnumerator DisplayLoadingScreen(string level) {
		uiBackground.SetActive(true);
		text.SetActive(true);
		progressBar.SetActive(true);

		progressBar.transform.localScale = new Vector3(loadProgress, progressBar.transform.localScale.y, progressBar.transform.localScale.z);

		text.guiText.text = "Loading Progress " + loadProgress + "%";

		AsyncOperation async = Application.LoadLevelAsync(levelToLoad);

		while (!async.isDone) {
			loadProgress = (int)(async.progress * 100);
			text.guiText.text = "Loading Progress " + loadProgress + "%";
			progressBar.transform.localScale = new Vector3(loadProgress, progressBar.transform.localScale.y, progressBar.transform.localScale.z);
			
			yield return null;
		}
	}
}
*/
