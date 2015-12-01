using UnityEngine;
using System.Collections;

public class InformationCanvas : MonoBehaviour {
	public GameObject InfoCanvas;

    void Awake()
    {
        GestureController.OnCursorOver += GestureController_OnCursorOver;
        GestureController.OnCursorOut += GestureController_OnCursorOut;
        GestureController.OnClicked += GestureController_OnClicked;
    }

    private void GestureController_OnClicked(GameObject g)
    {
        if (g != gameObject)
            return;
        InfoCanvas.SetActive(false);
    }

    private void GestureController_OnCursorOut(GameObject g)
    {
    }

    private void GestureController_OnCursorOver(GameObject g)
    {

    }

    void OnDestroy()
    {
        GestureController.OnCursorOver -= GestureController_OnCursorOver;
        GestureController.OnCursorOut -= GestureController_OnCursorOut;
        GestureController.OnClicked -= GestureController_OnClicked;
    }


}
