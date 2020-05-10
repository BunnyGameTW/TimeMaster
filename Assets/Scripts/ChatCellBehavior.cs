using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class ChatCellBehavior : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI chatText;
    public Image headImage;
    public GameObject unreadObject;
    public TextMeshProUGUI unreadNumberText;
    public event EventHandler<EventArgs> clickEvent;
    public Image timeBarImage;
    public Color idleColor;
    public Color waitResponseColor;
    const int MAX_WORD_NUMBER = 24;

    WomenInfo womenInfo;
    WomenBehavior womenData;
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

        if (womenData.GetHasAskQuestion())
        {
            timeBarImage.color = waitResponseColor;
            timeBarImage.fillAmount = 1.0f - womenData.GetIdleTimer() / WomenBehavior.IDLE_TIME;
        }
        else
        {
            timeBarImage.color = idleColor;
            timeBarImage.fillAmount = 1.0f - womenData.GetTimer() / WomenBehavior.ASK_QUESTION_TIME;

        }
    }


    public void SetData(WomenInfo info, WomenBehavior women)
    {
        womenInfo = info;
        headImage.sprite = Resources.Load<Sprite>(info.fileName);
        nameText.text = info.name;
        womenData = women;
    }

    public WomenInfo GetData()
    {
        return womenInfo;
    }

    //update unread number and text
    public void UpdateUI(string text, int unreadNumber)
    {
        if (text.Length > MAX_WORD_NUMBER)
            chatText.text = text.Substring(0, MAX_WORD_NUMBER) + "...";
        else
            chatText.text = text;//TODO 檢查超過長度
        unreadObject.SetActive(unreadNumber != 0);
        unreadNumberText.text = unreadNumber.ToString();
    }
  

}
