using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChatCellBehavior : MonoBehaviour
{
    public Text nameText;
    public Text chatText;
    public Image headImage;
    public GameObject unreadObject;
    public Text unreadNumberText;
    public event EventHandler<EventArgs> clickEvent;

    WomenInfo womenInfo;

    float height;
    Vector3 originMousePosition;
    bool hasDrag;

    // Start is called before the first frame update
    void Start()
    {
        hasDrag = false;
        height = GetComponent<RectTransform>().sizeDelta.y;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))//TODO 寫個base
        {
            originMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (Input.mousePosition.y < (transform.position.y + height / 2) && Input.mousePosition.y > (transform.position.y - height / 2))
            {
                if (!hasDrag)
                {
                    clickEvent?.Invoke(this, EventArgs.Empty);
                }
            }
            hasDrag = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition != originMousePosition && !hasDrag)
            {
                hasDrag = true;
            }
        }
    }


    public void SetData(WomenInfo info)
    {
        womenInfo = info;
        headImage.sprite = Resources.Load<Sprite>(info.fileName);
        nameText.text = info.name;
    }

    public WomenInfo GetData()
    {
        return womenInfo;
    }

    //update unread number and text
    public void UpdateUI(string text, int unreadNumber)
    {
        chatText.text = text;//TODO 檢查超過長度
        unreadObject.SetActive(unreadNumber != 0);
        unreadNumberText.text = unreadNumber.ToString();
    }
  

}
