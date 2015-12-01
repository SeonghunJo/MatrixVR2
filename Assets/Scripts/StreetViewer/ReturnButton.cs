using UnityEngine;

public class ReturnButton : MonoBehaviour
{
    void Awake()
    {
        GestureController.OnCursorOver += GestureController_OnCursorOver;
        GestureController.OnCursorOut += GestureController_OnCursorOut;
        GestureController.OnClicked += GestureController_OnClicked;
    }

    private void GestureController_OnClicked(GameObject g)
    {
        if (g == null || g != gameObject)
            return;

        Application.LoadLevel("SceneEarth");
       
       //this.renderer.material.color.a = 1f;	
    }

    private void GestureController_OnCursorOut(GameObject g)
    {
        if (g == null || g != gameObject)
            return;

        Color temp = GetComponent<Renderer>().material.color;
        temp.a = 0.3f;
        GetComponent<Renderer>().material.color = temp;
        transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
    }

    private void GestureController_OnCursorOver(GameObject g)
    {
        if (g == null || g != gameObject)
            return;

        Color temp = GetComponent<Renderer>().material.color;
        temp.a = 1f;
        GetComponent<Renderer>().material.color = temp;
        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    void OnDestroy()
    {
        GestureController.OnCursorOver -= GestureController_OnCursorOver;
        GestureController.OnCursorOut -= GestureController_OnCursorOut;
        GestureController.OnClicked -= GestureController_OnClicked;
    }
}
