using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float perspectiveZoomSpeed = 0.05f;
    public float orthoZoomSpeed = 0.05f;

    [SerializeField]
    Camera cam;
    [SerializeField]
    GameObject player;

    private float Speed = 0.5f;
    private Vector2 nowPos, prePos;
    private Vector3 movePos;

    void Update()
    {
        if(Input.touchCount == 1) // drag
        {
            Touch touch = Input.GetTouch (0);
            if(touch.phase == TouchPhase.Began)
            {
                prePos = touch.position - touch.deltaPosition;
            }
            else if(touch.phase == TouchPhase.Moved)
            {
                nowPos = touch.position - touch.deltaPosition;
                movePos = (Vector3)(prePos - nowPos) * Time.deltaTime * Speed;
                cam.transform.Translate(movePos); 
                prePos = touch.position - touch.deltaPosition;
            }
        }
        if (Input.touchCount == 2) //zoom in zoom out
        {

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            if (cam.orthographic)
            {
                cam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
                cam.orthographicSize = Mathf.Max(cam.orthographicSize, 0.1f);
            }
            else
            {
                cam.fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
                cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, 0.1f, 179.9f);
            }
        }
    }
}
