using UnityEngine;

public class SunLookAt : MonoBehaviour
{
    public GameObject Earth;
    // Use this for initialization
    void Start()
    {
        if (Earth == null)
            Debug.LogWarning("SunLookAt.cs : No Earth Attached");
    }

    // Update is called once per frame
    void Update()
    {
        if (Earth == null)
            return;
        if (Earth.transform.position == Vector3.zero)
            return;
        transform.rotation.SetLookRotation(Earth.transform.position);
    }
}
