using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {

    private StreetViewManager SVManager;

    void Awake()
    {
        SVManager = StreetViewManager.Instance;
        GestureController.OnCursorOver += GestureController_OnCursorOver;
        GestureController.OnCursorOut += GestureController_OnCursorOut;
        GestureController.OnClicked += GestureController_OnClicked;
    }

    private void GestureController_OnClicked(GameObject g)
    {
        if (g != gameObject)
            return;

        SVManager.PlaceID = panoramaID;
        StreetViewRenderer.Instance().StartRenderStreetView();
    }

    private void GestureController_OnCursorOut(GameObject g)
    {
        if (g != gameObject)
            return;

        Color color = GetComponent<MeshRenderer>().material.color;
        color.a = 0.3f;
        GetComponent<Renderer>().material.color = color;
    }

    private void GestureController_OnCursorOver(GameObject g)
    {
        if (g != gameObject)
            return;

        Color color = GetComponent<MeshRenderer>().material.color;
        color.a = 1.0f;
        GetComponent<Renderer>().material.color = color;
    }

    void OnDestroy()
    {
        GestureController.OnCursorOver -= GestureController_OnCursorOver;
        GestureController.OnCursorOut -= GestureController_OnCursorOut;
        GestureController.OnClicked -= GestureController_OnClicked;
    }

    public string panoramaID;
    public float yawDegree;

	public void SetDegree(float degree)
	{
        // Mesh Heart
        /*
		transform.Rotate(new Vector3(280.0f, 180.0f, degree));
		transform.Translate(new Vector3(0.0f, 0.0f, -2.5f));
		transform.Translate(new Vector3(0.0f, -6.5f, 0.0f));
        */
        yawDegree = degree;

        transform.Rotate(Vector3.right, 90.0f); // 세워진 화살표를 눕히고
        transform.Rotate(Vector3.forward, degree - 90.0f); // 화살표의 전방방향으로 Yaw만큼 회전한 후 90도의 오차만큼 회전한다.
        transform.Translate(0, 6, 3, Space.Self);
	}

	public void SetPanoramaID(string id)
	{
		panoramaID = id;
	}
}
