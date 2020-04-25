using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
public class GameManager : MonoBehaviour
{
	[SerializeField] KGJExcel excelData;
    public GameObject Card;
    public Transform CardObjectTransform;

    const int WOMEN_NUMBER = 2;
    const char FORMAT_ACCEPT_CARD_CHARACTER = ',';
    const int START_CARD_NUMBER = 3;

    List<WomenQuestion>[] womenQuestionDatas;
    List<WomenResponse>[] womenResponseDatas;
    List<Card> cardData;

    enum State
    {
        FRIEND, CHAT, ROOM
    }
    State gameState;
    int womenIndex;
    List<Card> playerCards, randomCardList;

    void Start()
    {
        InitWomenQuestionDatas();
        InitWomenResponseDatas();
        InitFormatCardId();
        gameState = State.ROOM;
        cardData = excelData.cardTable;
        randomCardList = new List<Card>();
        InitPlayerCards();
        SwitchState();

    }


    Card RandomGetCard()
    {
        if(randomCardList.Count == 0)
        {
            for (int i = 0; i < cardData.Count; i++)
            {
                randomCardList.Add(cardData[i]);
            }
        }
        int index = UnityEngine.Random.Range(0, randomCardList.Count);
        Card randomCard = randomCardList[index];
        randomCardList.RemoveAt(index);
        return randomCard;
    }

    void InitWomenQuestionDatas()
    {
        womenQuestionDatas = new List<WomenQuestion>[WOMEN_NUMBER] {
            excelData.womenQuestionTable1,
            excelData.womenQuestionTable2
        };
    }

    void InitWomenResponseDatas()
    {
        womenResponseDatas = new List<WomenResponse>[WOMEN_NUMBER] {
            excelData.womenResponseTable1,
            excelData.womenResponseTable2
        };
    }

    //format women accept card id
    void InitFormatCardId()
    {
        for (int i = 0; i < womenQuestionDatas.Length; i++)
        {
            foreach (var item in womenQuestionDatas[i])
            {
                string[] strs = item.cardsId.Split(FORMAT_ACCEPT_CARD_CHARACTER);
                item.cardId = new int [strs.Length];
                for (int j = 0; j < strs.Length; j++)
                {
                    item.cardId[j] = Convert.ToInt32(strs[j]);
                }
            }
        }
    }

    void SwitchState()
    {
        switch (gameState)
        {
            case State.FRIEND:

                break;
            case State.CHAT:
                break;
            case State.ROOM:

                break;
            default:
                break;
        }
    }

    void InitPlayerCards()
    {
        for (int i = 0; i < START_CARD_NUMBER; i++)
        {
            //playerCards.Add(RandomGetCard());
            Card cardData = RandomGetCard();
            GameObject gameObjectCard = Instantiate(Card, Vector3.zero, Quaternion.identity, CardObjectTransform);
            SetCard(cardData, gameObjectCard);
            //TODO
        }
    }

    void SetCard(Card cardData, GameObject gameObject)
    {
        gameObject.GetComponent<CardBehavior>().SetData(cardData);
    }

    
}

