﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
public class NewMessageEventArgs : EventArgs
{
    public int womenId;
    public NewMessageEventArgs(int id)
    {
        womenId = id;
    }
}
public class NewMessageBehavior : MonoBehaviour
{
    public event EventHandler<NewMessageEventArgs> clickNewMessageEvent;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;
    public Image headImage;

    const float WAITING_TIME = 3.0f;
    const int MAX_TEXT_LENGTH = 13;

    float timer;
    Animation ani;
    bool isDisappear;
    int womenId;

    // Start is called before the first frame update
    void Start()
    {
        isDisappear = false;
        timer = 0;
        ani = GetComponent<Animation>();
        ani.Play();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > WAITING_TIME && !isDisappear)
        {
            isDisappear = true;
            ani.Play("newMessageDisappear");
        }
    }

    public void SetData(WomenInfo info, string message)
    {
        nameText.text = info.name;
        if (message.Contains("\n"))
            messageText.text = message.Substring(0, message.IndexOf('\n')) + "...";
        else if (message.Length >= MAX_TEXT_LENGTH)
            messageText.text = message.Substring(0, MAX_TEXT_LENGTH) + "...";
        else
            messageText.text = message;
        headImage.sprite = Resources.Load<Sprite>(info.fileName);
        womenId = info.id;
    }

    public void OnButtonClicked()
    {
        clickNewMessageEvent?.Invoke(this, new NewMessageEventArgs(womenId));
    }

    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
 
    public int GetWomenId()
    {
        return womenId;
    }
}
