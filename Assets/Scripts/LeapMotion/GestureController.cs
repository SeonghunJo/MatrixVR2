using UnityEngine;
using System;
using Leap;

/*
    2015-11-21 Modify
*/

public class GestureController : MonoBehaviour
{
    Controller controller;       //립모션 컨트롤러
    GameObject cursorPointer;
    GameObject rigidHand;

    public GameObject target;   // RayCast 충돌위치로  옮겨 표시하는 물체 
    public GameObject leftCamera;
    public GameObject rightCamera;
    public GameObject clickParticle = null; //클릭 파티클 이펙트

    public bool enableScreenTap = true;
    public bool enableKeyTap = true;
    public bool enableSwipe = true;
    public bool enableCircle = true;
    public float swipeSpeed = 100.0f;

    public bool enableMouseControl = false;

    public float maxFov = 130.0f;
    public float minFov = 80.0f;
    public float zoomScale = 0.7f;

    Ray r;
    RaycastHit hit;
    GameObject hitObject;

    public delegate void MouseOutAction(GameObject g);
    public static event MouseOutAction OnCursorOut;

    public delegate void MouseOverAction(GameObject g);
    public static event MouseOverAction OnCursorOver;

    public delegate void ClickAction(GameObject g);
    public static event ClickAction OnClicked;

    public delegate void SwipeLeft();
    public static event SwipeLeft OnSwipeLeft;

    public delegate void SwipeRight();
    public static event SwipeRight OnSwipeRight;

    void Start()
    {
        target.GetComponent<Renderer>().material.color = Color.white;
        controller = new Controller();  //립모션 컨트롤러 할당 

        SetGesture(controller);
    }

    void SetGesture(Controller controller)
    {
        if (enableScreenTap)
        { // https://developer.leapmotion.com/documentation/unity/api/Leap.ScreenTapGesture.html
            controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            controller.Config.SetFloat("Gesture.ScreenTap.MinForwardVelocity", 30.0f); //ScreenTap 조건 30초로 바꿈
            controller.Config.SetFloat("Gesture.ScreenTap.HistorySeconds", 0.4f); // SHJO 판정시간 0.5초로 바꿈
            controller.Config.SetFloat("Gesture.ScreenTap.MinDistance", 1.0f); // SHJO 최소거리 5에서 3로 바꿈
            controller.Config.Save();
        }
        if (enableKeyTap)
        { // https://developer.leapmotion.com/documentation/unity/api/Leap.KeyTapGesture.html?proglang=unity
            controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            controller.Config.SetFloat("Gesture.KeyTap.MinDownVelocity", 20.0f); // SHJO 최소 속도 50에서 30으로 바꿈
            controller.Config.SetFloat("Gesture.KeyTap.HistorySeconds", 0.4f); // SHJO 판정시간  0.1에서 0.3초로 바꿈
            controller.Config.SetFloat("Gesture.KeyTap.MinDistance", 1.0f); // SHJO 최소거리 3에서 1로 바꿈
            controller.Config.Save();
        }
        if (enableSwipe)
        { // https://developer.leapmotion.com/documentation/unity/api/Leap.SwipeGesture.html
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
            controller.Config.SetFloat("Gesture.Swipe.MinLength", 60.0f); // SHJO Swipe 조건 바꿈 
            controller.Config.SetFloat("Gesture.Swipe.MinVelocity", 100.0f);
            controller.Config.Save();
        }
        if (enableCircle)
        { // https://developer.leapmotion.com/documentation/unity/api/Leap.CircleGesture.html#id3
            controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            controller.Config.SetFloat("Gesture.Circle.MinRadius", 10.0f);
            controller.Config.SetFloat("Gesture.Circle.MinArc", 1.0f);
            controller.Config.Save();
        }
    }
    
    void Update()
    {
        Frame frame = controller.Frame();           //frame 
        GestureList gestures = frame.Gestures();    //frame 안의 gesture 인식
        HandList hands = frame.Hands;               //frame 안의 hands 인식

        if (hands.Count < 1)
            return;
        
        if (FindAndDisableRigidHand())
        {
            //손바닥을 UnityScale로 좌표 변환 , Handcontroller TransformPoint로 Transform 형식에 맞게 변환, 이후 왼쪽 카메라 기준으로 월드 스크린으로 변환 
            if (RayFromCursor())
            {
                RayPoint(); // able  Pointed Object
            }
            else
            {
                RayPointedOut(); // Disable Pointed Object
            }
        }
        //립모션 제스쳐 감지 
        for (int i = 0; i < gestures.Count; i++)
        {
            Gesture gesture = gestures[i];
            HandList handsForGesture = gesture.Hands;
            switch (hands.Count)
            {
                case 1:
                    // Key Tap
                    if (gesture.Type == Gesture.GestureType.TYPE_KEY_TAP)
                        KeyTap(gesture);
                    // Screen Tap
                    else if (gesture.Type == Gesture.GestureType.TYPE_SCREEN_TAP)
                        ScreenTap(gesture);
                    // Swipe
                    else if (gesture.Type == Gesture.GestureType.TYPE_SWIPE)
                        Swipe(gesture);
                    // Circle
                    else if (gesture.Type == Gesture.GestureType.TYPE_CIRCLE)
                        Circle(gesture);
                    break;
                case 2:
                    ZoomInOut(gesture, handsForGesture);
                    break;

            }
            // ZOOM IN OUT Motion

        } // END OF GESTURE RECOGNITION LOOP
    }

    //Get Tipping postion
    Vector3 GetTippingPos() // 현재 포인터 끝이 되는 오브젝트의 
    {
        cursorPointer = GameObject.Find("RigidHand(Clone)/index/bone3");       //생선돈 손 모양 객체의 손바닥 오브젝트를 찾는다
        if (cursorPointer != null)
        {
            return cursorPointer.transform.position;  //객체를 찾았다면 객체 위치를 반환
        }
        else
        {
            return Vector3.zero;
        }

    }

    //Gesture Event for Keytap 
    void KeyTap(Gesture gesture)
    {
        KeyTapGesture keyTap = new KeyTapGesture(gesture);
        Debug.Log("TYPE_KEY_TAP - Duration : " + keyTap.DurationSeconds.ToString());

        GameObject particleObj = Instantiate(clickParticle, GetTippingPos(), Quaternion.identity) as GameObject;

        Destroy(particleObj, 2f);

        if(hitObject != null)
        {
            OnClicked(hitObject);
        }
    }

    //Gesture Event for ScreenTap
    void ScreenTap(Gesture gesture)
    {
        ScreenTapGesture screenTap = new ScreenTapGesture(gesture);
        print("Screen Tap " + screenTap.Duration.ToString());

        GameObject particleObj = Instantiate(clickParticle, GetTippingPos(), Quaternion.identity) as GameObject;

        Destroy(particleObj, 2f);

        if (hitObject != null)
        {
            OnClicked(hitObject);
        }
    }

    //Gesture Event for Swipe
    void Swipe(Gesture gesture)
    {
        SwipeGesture swipe = new SwipeGesture(gesture);
        print("Swipe Speed : " + swipe.Speed.ToString() + " Swipe Start : " + swipe.StartPosition.ToString() + " Swipe End : " + swipe.Position.ToString());

        // TODO : Swipe (SHJO)
        if (swipe.StartPosition.x > swipe.Position.x) // swipe.direction을 써도됨
        {
            if (OnSwipeLeft == null)
            {
                Debug.LogWarning("SwipeLeft Event Receiver is null");
                return;
            }

            OnSwipeLeft();
        }
        else
        {
            if (OnSwipeRight == null)
            {
                Debug.LogWarning("SwipeRight Event Receiver is null");
                return;
            }
            
            OnSwipeRight();
        }
    }

    //Gesture Event for Circle
    void Circle(Gesture gesture)
    {
        CircleGesture circleGesture = new CircleGesture(gesture);
        print("Circle");

        // TODO : Circle (SHJO)
        if (circleGesture.Pointable.Direction.AngleTo(circleGesture.Normal) <= Math.PI / 2) // Clockwise
        {
        }
        else
        {
        }

    }

    //Gesture Even ZoomInOut
    void ZoomInOut(Gesture gesture, HandList handsForGesture)
    {
        Debug.Log("left zoom");
        SwipeGesture Swipe = new SwipeGesture(gesture);
        Vector swipeDirection = Swipe.Direction;
        float temp = 0;
        if (swipeDirection.x < 0 && handsForGesture[0].IsLeft
            || swipeDirection.x > 0 && handsForGesture[0].IsRight)
        {

            if (leftCamera.GetComponent<Camera>().fieldOfView < maxFov)
            {
                temp = zoomScale;
            }

        }
        else if (swipeDirection.x > 0 && handsForGesture[0].IsLeft
                 || swipeDirection.x < 0 && handsForGesture[0].IsRight)
        {
            if (leftCamera.GetComponent<Camera>().fieldOfView > minFov)
            {
                temp = zoomScale * -1; ;
            }
        }

        leftCamera.GetComponent<Camera>().fieldOfView += temp;
        rightCamera.GetComponent<Camera>().fieldOfView += temp;

    }
    

    // RigidHand의 Collision을 해제하는 함수
    bool FindAndDisableRigidHand()
    {
        rigidHand = GameObject.Find("RigidHand(Clone)");
        if (rigidHand == null)
        {
            Debug.LogWarning("RigidHand is null");
            return false;
        }

        if (GetCursorPointer() == false)
        {
            return false;
        }

        Collider[] cols = rigidHand.GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }
        return true;
    }

    bool GetCursorPointer()
    {
        cursorPointer = GameObject.Find("RigidHand(Clone)/index/bone3");
        if (cursorPointer == null)
        {
            Debug.LogWarning("CursorPointer is null");
            return false;
        }
        return true;
    }

    bool RayFromCursor()
    {
        //손바닥을 UnityScale로 좌표 변환 , Handcontroller TransformPoint로 Transform 형식에 맞게 변환, 이후 왼쪽 카메라 기준으로 월드 스크린으로 변환 
        Vector2 screenPoint = leftCamera.GetComponent<Camera>().WorldToScreenPoint(cursorPointer.transform.position);
        if (enableMouseControl)
            screenPoint = Input.mousePosition;

        r = leftCamera.GetComponent<Camera>().ScreenPointToRay(screenPoint);      // ScreentPoint로부터 Ray를 쏜다
        Debug.DrawRay(r.origin, r.direction * 10000, Color.red);

        //rayCast에서 부딪힌 객체 관리
        if (Physics.Raycast(r, out hit, Mathf.Infinity) == true)
            return true;

        return false;
    }

    void RayPoint()
    {
        if (hit.collider == null)
            return;
        
        if(hit.collider.tag == "RaycastTarget") // 올바른 타겟
        {
            if (hitObject == hit.transform.gameObject) // 이전에 맞은 타겟과 같은 오브젝트이면
                return;
            else
            {
                if (hitObject != null)
                    OnCursorOut(hitObject);

                target.transform.position = hit.transform.position;
                hitObject = hit.transform.gameObject;
                OnCursorOver(hit.transform.gameObject);
            }
        }
        else // 올바르지 않은 타겟
        {
            if (hitObject != null)
                OnCursorOut(hitObject);

            hitObject = null;
        }
    }

    void RayPointedOut() // 허공
    {
        if (hitObject == null)
            return;

        OnCursorOut(hitObject);
        hitObject = null;
    }
}




