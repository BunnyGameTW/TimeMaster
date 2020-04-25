using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class CardBehavior : MonoBehaviour
{
    public Text descriptionText;
    public Image image;

    const float MOVE_OFFSET_Y = 20.0f;

    Card data;

    // Start is called before the first frame update
    void Start()
    {
        
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
            image.sprite = (Sprite)Resources.Load(data.fileName);//TODO
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
        Debug.Log("OnPointerEnter");
        gameObject.transform.localScale *= 1.5f;
        gameObject.transform.localPosition += new Vector3(0, MOVE_OFFSET_Y, 0);
    }

    public void OnPointerExit()
    {
        Debug.Log("OnPointerExit");
        gameObject.transform.localScale /= 1.5f;
        gameObject.transform.localPosition -= new Vector3(0, MOVE_OFFSET_Y, 0);

    }

    public void OnPointerDown()
    {
        Debug.Log("OnPointerDown");
    }

    public void OnPointerDrag()
    {
        Debug.Log("OnPointerDrag");
    }

    public void OnPointerUp()
    {
        Debug.Log("OnPointerUp");
        //check position Y and hasDown and hasEnter
        //use card event
    }
}
