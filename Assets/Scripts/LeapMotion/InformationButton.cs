using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InformationButton : MonoBehaviour {

	public GameObject InformationCanvas;

    public GameObject title;
    public GameObject country;
    public GameObject area;
    public GameObject contents;
    public GameObject image;
    public Sprite flag;

    StreetViewManager SVManager;

    void Awake()
    {
        GestureController.OnCursorOver += GestureController_OnCursorOver;
        GestureController.OnCursorOut += GestureController_OnCursorOut;
        GestureController.OnClicked += GestureController_OnClicked;

        SVManager = StreetViewManager.Instance;
    }

    private void GestureController_OnClicked(GameObject g)
    {
        if (g != gameObject)
            return;
        InformationCanvas.SetActive(true);
    }

    private void GestureController_OnCursorOut(GameObject g)
    {
    }

    private void GestureController_OnCursorOver(GameObject g)
    {
        if (g != gameObject)
            return;

        flag = Resources.Load<Sprite>("Flags/" + SVManager.info_flag_path);
        Image tempImage = image.GetComponent<Image>();
        tempImage.sprite = flag;
        
        Text temp = title.GetComponent<Text>();
        temp.text = SVManager.title;

        temp = country.GetComponent<Text>();
        temp.text = SVManager.country;

        temp = area.GetComponent<Text>();
        temp.text = SVManager.area;

        temp = contents.GetComponent<Text>();
        temp.text = SVManager.contents;

        InformationCanvas.SetActive(true);
    }

    void OnDestroy()
    {
        GestureController.OnCursorOver -= GestureController_OnCursorOver;
        GestureController.OnCursorOut -= GestureController_OnCursorOut;
        GestureController.OnClicked -= GestureController_OnClicked;
    }


}
