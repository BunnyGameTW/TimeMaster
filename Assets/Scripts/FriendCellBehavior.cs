using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class FriendCellBehavior : MonoBehaviour
{
    WomenInfo data;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI stateText;
    public Image image;
    public event EventHandler<EventArgs> clickEvent;

    bool hasDrag;
    float height;
    Vector3 originMousePosition;
    const float DEFAULT_SCREEN_HEIGHT = 1920.0f;
    const float MIN_DRAG_DISTANCE = 50.0f;

    // Start is called before the first frame update
    void Start()
    {
        hasDrag = false;
        height = GetComponent<RectTransform>().sizeDelta.y * (Screen.height / DEFAULT_SCREEN_HEIGHT);

    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(0))//TODO 寫個base
        {
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
#endif
            originMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.mousePosition.y < transform.position.y + height / 2 && Input.mousePosition.y > transform.position.y - height / 2)
            {
                if (!hasDrag)
                {
                    clickEvent?.Invoke(this, EventArgs.Empty);
                }
            }
             hasDrag = false;
        }
# if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        else if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition != originMousePosition && !hasDrag)
            {
                hasDrag = true;
            }

        }
#else
        else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            if (Mathf.Abs(Vector3.Distance(Input.mousePosition, originMousePosition)) > MIN_DRAG_DISTANCE && !hasDrag)
            {
                hasDrag = true;
            }
        }
#endif
    }

    public void SetData(WomenInfo womenInfo)
    {
        data = womenInfo;

        nameText.text = data.name;
        stateText.text = data.state;
        image.sprite = Resources.Load<Sprite>(data.fileName);
    }

    public WomenInfo GetData()
    {
        return data;
    }
}
