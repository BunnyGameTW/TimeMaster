using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public struct NavData
{
    public string fileName;
    public string title;

    public NavData(string path, string titleString)
    {
        fileName = path;
        title = titleString;
    }
}

public class GameManager : MonoBehaviour
{
	[SerializeField] KGJExcel excelData;
    public GameObject cardPrefab, friendCellPrefab, chatCellPrefab, roomGirlPrefab, roomMeTextPrefab, roomMeImagePrefab;
    public GameObject navCellPrefab;
    public Transform cardObjectTransform;
    public Transform selectCardTransform;
    public GameObject titleObject, navBarObject;
    public GameObject freindScrollViewObject, chatScrollViewObject, roomScrollViewObject;
    public GameObject meObject, roomObject;
    public GameObject womenPrefab;

    const int WOMEN_NUMBER = 2;
    const char FORMAT_ACCEPT_CARD_CHARACTER = ',';
    const int START_CARD_NUMBER = 3;
    const float CARD_START_POSITION_Y = -800.0f;
    const float CARD_PADDING = 0.0f;
    const float MAX_SCORE = 100.0f;
    const string FIREND_TITLE = "朋友";
    const string CHAT_TITLE = "聊天";
    const string ROOM_TITLE = "名字";
    const string SETTING_TITLE = "設定";
    const float NAV_ICON_SCALE_RATIO = 1.2f;
    const string NAV_FRIEND_ICON_FILE_NAME = "bunny";
    const string NAV_CHAT_ICON_FILE_NAME = "bunny";
    const string NAV_SETTING_ICON_FILE_NAME = "bunny";
    const string PLAYER_NAME = "小豬";
    const string PLAYER_STATE = "I wonna know, 你行不行";
    const string PLAYER_PHOTO_FILE_NAME = "player_photo";

    float CARD_TOTAL_WIDTH;

    List<WomenQuestion>[] womenQuestionDatas;
    List<WomenResponse>[] womenResponseDatas;
    List<Card> cardData;
    enum State
    {
        FRIEND, CHAT, ROOM, SETTING
    }
    State gameState;

    int womenIndex;
    List<Card> randomCardList;
    List<GameObject> playerCards;
    Dictionary<State, GameObject> scrollViewDictionary;
    Dictionary<State, string> titleTextDictionary;
    Dictionary<State, NavData> navDictionary;
    Dictionary<State, GameObject> navCellDictionary;
    List<GameObject> friendList;
    List<WomenBehavior> womenList;

    void Start()
    {
        InitWomenQuestionDatas();
        InitWomenResponseDatas();
        InitFormatCardId();
        gameState = State.FRIEND;
        cardData = excelData.cardTable;
        randomCardList = new List<Card>();
        InitPlayerCards();
        InitScrollViewDictionary();
        InitTitleDictionary();
        InitNavBar();
        InitMeInfo();
        InitFriendList();
        //InitChatList();
        InitWomenList();
        CARD_TOTAL_WIDTH = Screen.width -
            playerCards[0].gameObject.GetComponentInChildren<RectTransform>().sizeDelta.x - CARD_PADDING * 2;

       
        SwitchState();
    }

    void InitWomenList()
    {
        womenList = new List<WomenBehavior>();
        for (int i = 0; i < excelData.womenTable.Count; i++)
        {
            WomenBehavior women = Instantiate(womenPrefab).GetComponent<WomenBehavior>();
            women.SetData(excelData.womenTable[i], womenQuestionDatas[i], womenResponseDatas[i]);
            women.showMessageEvent += OnShowMessageEvent;
            womenList.Add(women);
        }
    }

    void InitMeInfo()
    {
        meObject.GetComponentsInChildren<Text>()[0].text = PLAYER_NAME;
        meObject.GetComponentsInChildren<Text>()[1].text = PLAYER_STATE;
        meObject.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>(PLAYER_PHOTO_FILE_NAME);
    }

    void InitFriendList()
    {
        friendList = new List<GameObject>();

        for (int i = 0; i < excelData.womenTable.Count; i++)
        {
            friendList.Add(CreateFriendCell(excelData.womenTable[i]));
        }
    }

    GameObject CreateFriendCell(WomenInfo data)
    {
        Transform parent = scrollViewDictionary[State.FRIEND].GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
        GameObject gameObject = Instantiate(friendCellPrefab, parent);
        gameObject.GetComponent<FriendCellBehavior>().SetData(data);
        gameObject.GetComponent<FriendCellBehavior>().clickEvent += OnFriendCellClickEvent;
        return gameObject;
    }

    void InitNavBar()
    {
        navDictionary = new Dictionary<State, NavData>();//TODO
        navDictionary.Add(State.FRIEND, new NavData(NAV_FRIEND_ICON_FILE_NAME, FIREND_TITLE));
        navDictionary.Add(State.CHAT, new NavData(NAV_CHAT_ICON_FILE_NAME, CHAT_TITLE));
        navDictionary.Add(State.SETTING, new NavData(NAV_SETTING_ICON_FILE_NAME, SETTING_TITLE));

        navCellDictionary = new Dictionary<State, GameObject>();
        Transform transform = navBarObject.GetComponentInChildren<HorizontalLayoutGroup>().gameObject.transform;
        navCellDictionary.Add(State.FRIEND, CreateNavCell(State.FRIEND, transform));//TODO
        navCellDictionary.Add(State.CHAT, CreateNavCell(State.CHAT, transform));
        navCellDictionary.Add(State.SETTING, CreateNavCell(State.SETTING, transform));
    }

    GameObject CreateNavCell(State state, Transform parent)
    {
        GameObject cell = Instantiate(navCellPrefab, parent);
        cell.GetComponent<NavCellBehavior>().SetData(navDictionary[state]);
        cell.GetComponent<NavCellBehavior>().clickEvent += OnNavCellClickEvent;
        return cell;
    }
   
    void InitTitleDictionary()
    {
        titleTextDictionary = new Dictionary<State, string>();
        titleTextDictionary.Add(State.CHAT, CHAT_TITLE);
        titleTextDictionary.Add(State.ROOM, ROOM_TITLE);//TODO
        titleTextDictionary.Add(State.FRIEND, FIREND_TITLE);
        titleTextDictionary.Add(State.SETTING, SETTING_TITLE);
    }

    void UpdateTitle()
    {
        string title = titleTextDictionary[gameState];
        if (gameState == State.ROOM)
        {
            title = friendList[womenIndex - 1].GetComponent<FriendCellBehavior>().GetData().name;
        }
        titleObject.GetComponentInChildren<Text>().text = title;
    }

    void InitScrollViewDictionary()
    {
        scrollViewDictionary = new Dictionary<State, GameObject>();
        scrollViewDictionary.Add(State.CHAT, chatScrollViewObject);
        scrollViewDictionary.Add(State.ROOM, roomScrollViewObject);
        scrollViewDictionary.Add(State.FRIEND, freindScrollViewObject);
    }

    void UpdateScrollView()
    {
        foreach (KeyValuePair<State, GameObject> pair in scrollViewDictionary)
        {
            pair.Value.SetActive(false);
        }

        if (gameState != State.SETTING)
        {
            scrollViewDictionary[gameState].SetActive(true);
        }
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
                //float score = womenList[womenIndex - 1].GetData().score;
                //roomObject.GetComponentInChildren<Text>().text = score.ToString();
                //roomObject.GetComponentInChildren<Image>().fillAmount = score / MAX_SCORE;
                break;
            default:
                break;
        }

        UpdatePlayerCards();
        UpdateScrollView();
        UpdateTitle();
        UpdateNavBar();

        meObject.SetActive(gameState == State.FRIEND);
        navBarObject.SetActive(gameState != State.ROOM);
        cardObjectTransform.gameObject.SetActive(gameState == State.ROOM);
        selectCardTransform.gameObject.SetActive(gameState == State.ROOM);
        roomObject.SetActive(gameState == State.ROOM);
    }

    GameObject GetCard()
    {
        Card cardData = RandomGetCard();
        GameObject gameObjectCard = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, cardObjectTransform);
        SetCard(cardData, gameObjectCard);
        return gameObjectCard;
    }

    void AddCard()
    {
        GameObject gameObjectCard = GetCard();
        playerCards.Add(gameObjectCard);
        gameObjectCard.GetComponent<CardBehavior>().playCardEvent += OnPlayCardEvent;
        gameObjectCard.GetComponent<CardBehavior>().pointerEnterEvent += OnCardEnterEvent;
        gameObjectCard.GetComponent<CardBehavior>().pointerExitEvent += OnCardExitEvent;
    }

    void InitPlayerCards()
    {
        playerCards = new List<GameObject>();
        for (int i = 0; i < START_CARD_NUMBER; i++)
        {
            AddCard();
        }
    }

    //event
    void OnPlayCardEvent(object sender, CardEventArgs param)
    {
        Card cardData = param.cardData;
        Debug.Log(cardData.description);
        Debug.Log(cardData.id);
        //TODO show message

        womenList[womenIndex - 1].AddCard(cardData);

        CardBehavior cardModel = (CardBehavior)sender;
        for (int i = 0; i < playerCards.Count; i++)
        {
            CardBehavior cardBehavior = playerCards[i].GetComponent<CardBehavior>();
            if (cardBehavior == cardModel)
            {
                cardBehavior.playCardEvent -= OnPlayCardEvent;
                cardBehavior.pointerEnterEvent += OnCardEnterEvent;
                cardBehavior.pointerExitEvent += OnCardExitEvent;
                Destroy(playerCards[i]);
                playerCards.RemoveAt(i);
            }
        }
        AddCard();
        UpdatePlayerCards();
        //TODO girl behavior
    }

    void OnCardEnterEvent(object sender, CardEventArgs param)
    {
        CardBehavior cardModel = (CardBehavior)sender;
        cardModel.transform.SetParent(selectCardTransform);
    }

    void OnCardExitEvent(object sender, CardEventArgs param)
    {
        UpdatePlayerCards();
    }

    void OnNavCellClickEvent(object sender, EventArgs param)
    {
        NavCellBehavior navCell = (NavCellBehavior)sender;
        foreach (KeyValuePair<State, GameObject> pair in navCellDictionary)
        {
            if(pair.Value == navCell.gameObject)
            {
                gameState = pair.Key;
            }
        }

        SwitchState();
    }

    void OnFriendCellClickEvent(object sender, EventArgs param)
    {
        FriendCellBehavior friendCell = (FriendCellBehavior)sender;
        gameState = State.ROOM;
        womenIndex = friendCell.GetData().id;
        SwitchState();
    }


    void OnShowMessageEvent(object sender, EventArgs param)
    {
        //TODO type women or player
    }

    public void OnBackButtonClicked()
    {
        gameState = State.CHAT;
        SwitchState();
    }

  

    void UpdateNavBar()
    {
        foreach (KeyValuePair<State, GameObject> pair in navCellDictionary)
        {
            pair.Value.transform.localScale = new Vector3(1, 1, 1);
            pair.Value.GetComponentInChildren<Image>().color = Color.gray;
            pair.Value.GetComponentInChildren<Text>().color = Color.gray;
        }
        if (gameState == State.ROOM)
            return;
        GameObject gameObject = navCellDictionary[gameState];
        gameObject.transform.localScale = new Vector3(NAV_ICON_SCALE_RATIO, NAV_ICON_SCALE_RATIO, 1);
        gameObject.GetComponentInChildren<Image>().color = Color.white;
        gameObject.GetComponentInChildren<Text>().color = Color.white;
    }

    void SetCard(Card cardData, GameObject gameObject)
    {
        gameObject.GetComponent<CardBehavior>().SetData(cardData);
    }

    void UpdatePlayerCards()
    {
        //position
        float mid = (playerCards.Count - 1) / 2;
        for (int i = 0; i < playerCards.Count; i++)
        {
            float offset = CARD_TOTAL_WIDTH / playerCards.Count;
            playerCards[i].transform.localPosition = new Vector3((i - mid) * offset, CARD_START_POSITION_Y, 0);
        }
        //z order
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].transform.SetParent(selectCardTransform);
            playerCards[i].transform.SetParent(cardObjectTransform);
        }
    }



}

