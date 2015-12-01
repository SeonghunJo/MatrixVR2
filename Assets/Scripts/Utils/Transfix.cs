using UnityEngine;
using UnityEngine.VR;
using System.Collections;

public class Transfix : MonoBehaviour {

    public GameObject target;
	// Use this for initialization
	void Start () {
    }

    void FixedUpdate()
    {
        transform.LookAt(target.transform);
    }
    
    // Update is called once per frame
    void Update () {
        //InputTracking.Recenter();
        //OVRManager.display.RecenterPose();
    }
}
