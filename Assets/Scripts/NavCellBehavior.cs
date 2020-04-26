using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class NavCellBehavior : MonoBehaviour
{
    NavData data;
    public Text text;
    public Image image;
    public event EventHandler<EventArgs> clickEvent;


    public void SetData(NavData navData)
    {
        data = navData;
        image.sprite = Resources.Load<Sprite>(navData.fileName);
        text.text = navData.title;
    }

    public void OnPointerUp()
    {
        clickEvent?.Invoke(this, EventArgs.Empty);
    }
}
