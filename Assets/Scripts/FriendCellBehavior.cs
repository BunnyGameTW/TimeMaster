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

    // Start is called before the first frame update
    void Start()
    {
        hasDrag = false;
        height = GetComponent<RectTransform>().sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
        else if (Input.GetMouseButton(0))
        {
            if(Input.mousePosition != originMousePosition && !hasDrag)
            {
                hasDrag = true;
            }
        }
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
