using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Thumbnail : MonoBehaviour {

    private static Thumbnail instance;
    public static Thumbnail Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(Thumbnail)) as Thumbnail;
            if (!instance)
                Debug.LogError("There needs to be one active Thumbnail script on a GameObject in your scene.");

            instance.gameObject.SetActive(false);
        }

        return instance;
    }

    public GameObject uiRawImage;
    public GameObject uiText;

    private RawImage rawImage;
    private Text location;

    [HideInInspector]
    private string title;
    public string Title
    {
        get { return title; }
        set {
            title = value;
            location.text = value;
        }
    }

    [HideInInspector]
    public Texture2D image;
    public Texture2D RawImage
    {
        get { return image; }
        set {
            image = value;
            rawImage.texture = value;
        }
    }
    [HideInInspector]
    private string wikiText;
    public string WikiText
    {
        get { return wikiText; }
        set
        {
            wikiText = value;
            // Some other code
        }
    }

    void Start () {
        rawImage = uiRawImage.GetComponent<RawImage>();
        location = uiText.GetComponent<Text>();
    }

    public void Show ()
    {
        gameObject.SetActive(true);
    }

    public void Hide ()
    {
        gameObject.SetActive(false);
    }

}
