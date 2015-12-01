using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]

//Mouse test
public class EarthScript : MonoBehaviour
{
	public int Speed;		//rotate speed
	
	public static bool leftRotate;
	public static bool rightRotate;
	public static bool guideTrigger = true;
    
    public void Awake()
    {
        GestureController.OnSwipeLeft += GestureController_OnSwipeLeft;
        GestureController.OnSwipeRight += GestureController_OnSwipeRight;
    }

	public void Start()
	{
		leftRotate = false;
		rightRotate = false;
		Speed = 10;
	}

    private void GestureController_OnSwipeLeft()
    {
        transform.Rotate(new Vector3(0, -Time.deltaTime * 50, 0));
    }

    private void GestureController_OnSwipeRight()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * 50, 0));
    }


    void Update()
	{
		if (leftRotate)
		{
			transform.Rotate(Vector3.up, Speed * Time.deltaTime * 5, Space.Self);
		}
		if (rightRotate)
		{
			transform.Rotate(Vector3.down, Speed * Time.deltaTime * 5, Space.Self);
		}
		
		/*
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(Vector3.left, Speed * Time.deltaTime * 5, Space.Self);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(Vector3.right, Speed * Time.deltaTime * 5, Space.Self);
        }
        */
		
	}
	
    void OnDestroy()
    {
        GestureController.OnSwipeLeft -= GestureController_OnSwipeLeft;
        GestureController.OnSwipeRight -= GestureController_OnSwipeRight;
    }
	
}


