using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoomSwipe : MonoBehaviour
{
    public GameObject roomScrollView;
    const float SWIPE_DISTANCE = 300.0f;
    const float OFFSET = 50.0f;
    const float MOVE_SPEED = 5000.0f;
    const float SCREEN_WIDTH = 1080.0f;
    const float MIN_DRAG_OFFSET = 50.0f;
    const float ORIGIN_POSITION_X = 9999.0f;
    float originMousePositionX, originMousePositionY;
    float targetX;
    bool isMove, hasPointerDown, canSwipe, needInvokeEvent;

    public event EventHandler<EventArgs> moveEndEvent;
    public event EventHandler<EventArgs> hideCardEvent;

    

    // Start is called before the first frame update
    void Start()
    {
        isMove = hasPointerDown = false;
        originMousePositionX = ORIGIN_POSITION_X;
        canSwipe = false;
        needInvokeEvent = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMove)
        {
            Move();
        }
        else
        {
            if(canSwipe) DetectInput();
        }
      
    }

    public void SetCanSwipe(bool can)
    {
        canSwipe = can;
    }

    public void SetMoveIn()
    {

        targetX = 0;
        isMove = true;
    }

    public void SetMoveOut()
    {
        targetX = SCREEN_WIDTH;
        isMove = true;
    }

    void Move()
    {
        if (Mathf.Abs(transform.localPosition.x - targetX) > OFFSET)
        {

            float direction = targetX > transform.localPosition.x ? 1 : -1;
            transform.localPosition = transform.localPosition + new Vector3(direction * MOVE_SPEED * Time.deltaTime, 0, 0);
            roomScrollView.transform.localPosition = roomScrollView.transform.localPosition + new Vector3(direction * MOVE_SPEED * Time.deltaTime, 0, 0);
        }
        else
        {
            transform.localPosition = new Vector3(targetX, 0, 0);
            roomScrollView.transform.localPosition = new Vector3(targetX, roomScrollView.transform.localPosition.y, 0);

            isMove = false;
            if(targetX == 0)
            {
                canSwipe = true;
            }

            if (!needInvokeEvent)
            {
                needInvokeEvent = true;
            }
            else
            {
                moveEndEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    void DetectInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float min = -SCREEN_WIDTH / 2;
            float max = SCREEN_WIDTH / 2;
            transform.localPosition = new Vector3(min, 0, 0);
            min = transform.position.x;
            transform.localPosition = new Vector3(max, 0, 0);
            max = transform.position.x;
            transform.localPosition = Vector3.zero;
          
            if (Input.mousePosition.x < min || Input.mousePosition.x > max)
            {
                Debug.Log("is out of range");
                return;
            }
            originMousePositionX = Input.mousePosition.x;
            originMousePositionY = Input.mousePosition.y;
            hasPointerDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float offsetX = Mathf.Abs(Input.mousePosition.x - originMousePositionX);
            float offsetY = Mathf.Abs(Input.mousePosition.y - originMousePositionY);
            if (offsetY > offsetX || (offsetX == 0 && offsetY == 0))
                return;
            if (Input.mousePosition.x - originMousePositionX >= SWIPE_DISTANCE)
            {
                SetMoveOut();
                originMousePositionX = ORIGIN_POSITION_X;
                hideCardEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                SetMoveIn();
                needInvokeEvent = false;
            }
            hasPointerDown = false;
        }
        else if (Input.GetMouseButton(0) && hasPointerDown)
        {
            float offset = Mathf.Abs(Input.mousePosition.x - originMousePositionX);
            float offsetY = Mathf.Abs(Input.mousePosition.y - originMousePositionY);
            if (offset >= MIN_DRAG_OFFSET && offset > offsetY && Input.mousePosition.x - Screen.width / 2 >= 0)
            {
                transform.localPosition = new Vector3(Input.mousePosition.x - Screen.width / 2, 0, 0);
                roomScrollView.transform.localPosition = new Vector3(
                    Input.mousePosition.x - Screen.width / 2,
                    roomScrollView.transform.localPosition.y,
                    0
                );
            }
        }
    }
}
