using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CardEventArgs : EventArgs
{
    public Card cardData;
}

public class CardBehavior : MonoBehaviour
{
    public Text descriptionText;
    public Image image;

    const float MOVE_OFFSET_Y = 100.0f;
    const float PLAY_CARD_POSITION_Y = 0;
    const float SCALE_RATIO = 1.5f;
    Card data;
    bool hasPointerDown, hasPointerEnter;

    public event EventHandler<CardEventArgs> playCardEvent;
    public event EventHandler<CardEventArgs> pointerEnterEvent;
    public event EventHandler<CardEventArgs> pointerExitEvent;

    // Start is called before the first frame update
    void Start()
    {
        hasPointerDown = hasPointerEnter = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateUI()
    {
        descriptionText.text = data.description;
        if (data.type == CardType.Image)
        {
            image.sprite = Resources.Load<Sprite>(data.fileName);
            image.gameObject.SetActive(true);
            descriptionText.gameObject.SetActive(false);
        }
    }

    public void SetData(Card cardData)
    {
        data = cardData;
        UpdateUI();
      
    }

    public void OnPointerEnter()
    {
        transform.localScale *= SCALE_RATIO;
        transform.localPosition += new Vector3(0, MOVE_OFFSET_Y, 0);
        hasPointerEnter = true;
        pointerEnterEvent?.Invoke(this, new CardEventArgs());
    }

    public void OnPointerExit()
    {
        gameObject.transform.localScale /= SCALE_RATIO;
        gameObject.transform.localPosition -= new Vector3(0, MOVE_OFFSET_Y, 0);
        hasPointerEnter = false;
        pointerExitEvent?.Invoke(this, new CardEventArgs());
    }

    public void OnPointerDown()
    {
        hasPointerDown = true;
    }

    public void OnPointerDrag()
    {
        if (hasPointerDown)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void OnPointerUp()
    {
        if(transform.localPosition.y > PLAY_CARD_POSITION_Y && hasPointerDown && hasPointerEnter)
        {
            CardEventArgs param = new CardEventArgs();
            param.cardData = data;
            playCardEvent?.Invoke(this, param);
        }
        else
        {
            //TODO reset position
            pointerExitEvent?.Invoke(this, new CardEventArgs());
        }
        hasPointerDown = false;
    }
}
