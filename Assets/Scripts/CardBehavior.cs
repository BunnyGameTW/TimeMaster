using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
public class CardEventArgs : EventArgs
{
    public Card cardData;
}

public class CardBehavior : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;
    public Image image;
    public Color activeColor;
    const float MOVE_OFFSET_Y = 100.0f;
    const float PLAY_CARD_POSITION_Y = 0;
    const float SCALE_RATIO = 1.5f;

    Card data;
    bool hasPointerDown, hasPointerEnter;
    bool canUse;
    Image backgroundImage ;
    Vector3 originalPosition;
    AudioSource audioSource;

    public event EventHandler<CardEventArgs> playCardEvent;
    public event EventHandler<CardEventArgs> pointerEnterEvent;
    public event EventHandler<CardEventArgs> pointerExitEvent;
    public event EventHandler<CardEventArgs> pointerUpEvent;

    

    // Start is called before the first frame update
    void Start()
    {
       
    }

    void Init()
    {
        hasPointerDown = hasPointerEnter = false;
        backgroundImage = GetComponentsInChildren<Image>()[0];
        audioSource = GetComponent<AudioSource>();
        originalPosition = transform.localPosition;
        SetCanUse(false);
    }

    public void SetCanUse(bool boolean)
    {
        canUse = boolean;
        backgroundImage.color = boolean ? activeColor : Color.gray;
    }

    public void UpdatePosition(Vector3 position)
    {
        transform.localPosition = position;
        originalPosition = transform.localPosition;
    }


    public void SetData(Card cardData)
    {
        data = cardData;
        Init();
        UpdateUI();
    }

    public void OnPointerEnter()
    {
        if (canUse)
        {
            transform.localScale = new Vector3(SCALE_RATIO, SCALE_RATIO);
            transform.localPosition = originalPosition + new Vector3(0, MOVE_OFFSET_Y, 0);
            hasPointerEnter = true;
            pointerEnterEvent?.Invoke(this, new CardEventArgs());
            audioSource.Play();
        }
    }

    public void OnPointerExit()
    {
        if (canUse)
        {
            gameObject.transform.localScale = new Vector3(1.0f, 1.0f);
            gameObject.transform.localPosition = originalPosition;
            hasPointerEnter = false;
            pointerExitEvent?.Invoke(this, new CardEventArgs());
        }
    }

    public void OnPointerDown()
    {
        if (canUse)
        {
            hasPointerDown = true;
            //TODO invoke click card event
        }
    }

    public void OnPointerDrag()
    {
        if (canUse)
        {
            if (hasPointerDown)
            {
                transform.position = Input.mousePosition;
            }
        }
    }

    public void OnPointerUp()
    {
        if (canUse)
        {
            if (transform.localPosition.y > PLAY_CARD_POSITION_Y && hasPointerDown && hasPointerEnter)
            {
                CardEventArgs param = new CardEventArgs();
                param.cardData = data;
                playCardEvent?.Invoke(this, param);
            }
            else
            {
                pointerUpEvent?.Invoke(this, new CardEventArgs());
            }
            hasPointerDown = false;
        }
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
}
