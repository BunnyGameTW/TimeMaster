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
    const int MAX_TEXT_LENGTH = 24;

    Text testText, testText2, testText3;//TPD
    WomenInfo womenInfo;
    WomenBehavior womenData;
    float height;
    Vector3 originMousePosition;
    bool hasDrag, isGameOver;
    const float DEFAULT_SCREEN_HEIGHT = 1920.0f;
    const float MIN_DRAG_DISTANCE = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        hasDrag = isGameOver = false;

        //Debug.Log(Screen.height / DEFAULT_SCREEN_HEIGHT);
        height = GetComponent<RectTransform>().sizeDelta.y * (Screen.height / DEFAULT_SCREEN_HEIGHT);
        //Debug.Log("height1-> " + GetComponent<RectTransform>().sizeDelta.y);
        //Debug.Log("height->" + height);

        //testText = GameObject.Find("testText").GetComponent<Text>();
        //testText2 = GameObject.Find("testText2").GetComponent<Text>();
        //testText3 = GameObject.Find("testText3").GetComponent<Text>();

    }

    void Update()
    {


        //testText.text = hasDrag ? "true" : "false";
        //testText2.text = Input.touchCount == 1 ? Input.GetTouch(0).phase.ToString() : "None";
        //testText3.text = Vector3.Distance(Input.mousePosition, originMousePosition).ToString();
        if (isGameOver)
            return;
# if UNITY_EDITOR	
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
            if (Input.mousePosition.y < (transform.position.y + height / 2) && Input.mousePosition.y > (transform.position.y - height / 2))
            {
                if (!hasDrag)
                {
                    clickEvent?.Invoke(this, EventArgs.Empty);
                }
            }
            hasDrag = false;
        }
# if UNITY_EDITOR	
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
        timeBarImage.fillAmount = 1.0f - womenData.GetAskTimeRatio();
        Color barColor = womenData.GetHasAskQuestion() ? waitResponseColor : idleColor;
        if (timeBarImage.color != barColor)
        {
            timeBarImage.color = barColor;
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

    public void SetGameOver()
    {
        isGameOver = true;
    }
    //update unread number and text
    public void UpdateUI(string text, int unreadNumber)
    {
        if (text.Length > MAX_TEXT_LENGTH)
            chatText.text = text.Substring(0, MAX_TEXT_LENGTH) + "...";
        else
            chatText.text = text;
        unreadObject.SetActive(unreadNumber != 0);
        unreadNumberText.text = unreadNumber.ToString();
    }
  

}
