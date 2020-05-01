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
    public GameObject newMessagePrefab;

    const int WOMEN_NUMBER = 2;
    const int NO_WOMEN_INDEX = -1;
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
        gameState = State.FRIEND;
        cardData = excelData.cardTable;
        randomCardList = new List<Card>();
        womenIndex = NO_WOMEN_INDEX;

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

    //public 
    public void OnBackButtonClicked()
    {
        gameState = State.CHAT;
        womenIndex = NO_WOMEN_INDEX;
        SwitchState();
    }

    //event
    void OnPlayCardEvent(object sender, CardEventArgs param)
    {
        Card cardData = param.cardData;
        Debug.Log(cardData.description);
        Debug.Log(cardData.id);
        SetCanUseCard(false);

        GetTalkWomen().AddMessage(cardData.id, MessageType.Player);
        GetTalkWomen().OnPlayCardEvent(cardData);

        //get new card
        DeleteCard((CardBehavior)sender);
        playerCards.Add(GetCard());
        UpdatePlayerCards();
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
            if (pair.Value == navCell.gameObject)
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

    void OnShowMessageEvent(object sender, MessageEvenArgs param)
    {
        WomenBehavior women = (WomenBehavior)sender;
        if (women.GetData().id == womenIndex)
        {
            UpdateScore();
            ShowInRoomMessage(param.message.type, param.message.id);
        }
        else
        {
            string[] messages = param.message.type == MessageType.WomenQuestion ?
              GetQuestionMessageString(param.message.id) : GetResponseMessageString(param.message.id);
            for (int i = 0; i < messages.Length; i++)
            {
                Debug.Log("show alert:" + women.GetData().name + "-> " + messages[i]);
                NewMessageBehavior messageObject = Instantiate(newMessagePrefab, titleObject.transform.parent).GetComponent<NewMessageBehavior>();
                messageObject.SetData(women.GetData(), messages[i]);
                messageObject.clickNewMessageEvent += OnNewMessageClickEvent;
            }
            women.AddUnreadMessage(param.message);
        }

    }

    void OnNewMessageClickEvent(object sender, NewMessageEventArgs param)
    {
        womenIndex = param.womenId;
        gameState = State.ROOM;
        SwitchState();
    }

    //private
    void InitWomenList()
    {
        womenList = new List<WomenBehavior>();
        for (int i = 0; i < excelData.womenTable.Count; i++)//TODO excelData.womenTable.Count
        {
            if (i == 0)//TODO removed
            {
                WomenBehavior women = Instantiate(womenPrefab).GetComponent<WomenBehavior>();
                women.SetData(
                    excelData.womenTable[i],
                    excelData.questionTable,
                    excelData.responseTable,
                    GetWomenMatchData(excelData.womenTable[i].id));
                women.showMessageEvent += OnShowMessageEvent;
                womenList.Add(women);
            }
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

    void InitTitleDictionary()
    {
        titleTextDictionary = new Dictionary<State, string>();
        titleTextDictionary.Add(State.CHAT, CHAT_TITLE);
        titleTextDictionary.Add(State.ROOM, ROOM_TITLE);
        titleTextDictionary.Add(State.FRIEND, FIREND_TITLE);
        titleTextDictionary.Add(State.SETTING, SETTING_TITLE);
    }

    void InitScrollViewDictionary()
    {
        scrollViewDictionary = new Dictionary<State, GameObject>();
        scrollViewDictionary.Add(State.CHAT, chatScrollViewObject);
        scrollViewDictionary.Add(State.ROOM, roomScrollViewObject);
        scrollViewDictionary.Add(State.FRIEND, freindScrollViewObject);
    }

    void InitPlayerCards()
    {
        playerCards = new List<GameObject>();
        for (int i = 0; i < START_CARD_NUMBER; i++)
        {
            playerCards.Add(GetCard());
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
                UpdateScore();
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

    void UpdateTitle()
    {
        string title = titleTextDictionary[gameState];
        if (gameState == State.ROOM)
        {
            title = GetTalkWomen().GetData().name;
        }
        titleObject.GetComponentInChildren<Text>().text = title;
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

    void UpdateNavBar()
    {
        if (gameState == State.ROOM)
        {
            return;
        }

        foreach (KeyValuePair<State, GameObject> pair in navCellDictionary)
        {
            pair.Value.transform.localScale = new Vector3(1, 1, 1);
            pair.Value.GetComponentInChildren<Image>().color = Color.gray;
            pair.Value.GetComponentInChildren<Text>().color = Color.gray;
        }

        GameObject gameObject = navCellDictionary[gameState];
        gameObject.transform.localScale = new Vector3(NAV_ICON_SCALE_RATIO, NAV_ICON_SCALE_RATIO, 1);
        gameObject.GetComponentInChildren<Image>().color = Color.white;
        gameObject.GetComponentInChildren<Text>().color = Color.white;
    }

    void UpdatePlayerCards()
    {
        //position
        float mid = (playerCards.Count - 1) / 2;
        for (int i = 0; i < playerCards.Count; i++)
        {
            float offset = CARD_TOTAL_WIDTH / playerCards.Count;
            playerCards[i].GetComponent<CardBehavior>().UpdatePosition(new Vector3((i - mid) * offset, CARD_START_POSITION_Y, 0));
        }

        //z order
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].transform.SetParent(selectCardTransform);
            playerCards[i].transform.SetParent(cardObjectTransform);
        }
    }

    void UpdateScore()
    {
        float score = GetTalkWomen().GetScore();
        roomObject.GetComponentInChildren<Text>().text = score.ToString();
        roomObject.GetComponentsInChildren<Image>()[1].fillAmount = score / MAX_SCORE;
    }

    List<WomenInfo> GetNotIncludedWomenInfo(int id)
    {
        List<WomenInfo> list = new List<WomenInfo>();
        foreach (WomenInfo item in excelData.womenTable)
        {
            if (item.id != id)
            {
                list.Add(item);
            }
        }
        return list;
    }

    WomenQuestion GetQuestionData(int id)
    {
        foreach (WomenQuestion item in excelData.questionTable)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    string[] GetQuestionMessageString(int id)
    {
        WomenQuestion q = GetQuestionData(id);
        string str = q.description;

        //List<WomenInfo> list = GetNotIncludedWomenInfo(GetTalkWomen().GetData().id);
        //for (int i = 0; i < q.formatNumber; i++)
        //{
        //    int index = UnityEngine.Random.Range(0, list.Count);
        //    str = string.Format(str, list[index].name);
        //    //TODO 格式化人名要記下來

        //    list.RemoveAt(index);
        //Debug.Log("after format->" + str);
        //}

        if (q.hasMuitipleLine)
        {
            return str.Split('\n');
        }
        string[] strs = new string[1];
        strs[0] = str;
        return strs;
    }

    WomenResponse GetResponseData(int id)
    {
        foreach (WomenResponse item in excelData.responseTable)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    string[] GetResponseMessageString(int id)
    {
        WomenResponse r = GetResponseData(id);
        string str = r.description;
        if (r.hasMuitipleLine)
        {
            return str.Split('\n');
        }
        string[] strs = new string[1];
        strs[0] = str;
        return strs;
    }

    WomenBehavior GetTalkWomen()
    {
        return womenList[womenIndex - 1];
    }

    //TODO 機率
    Card GetRandomCardData()
    {
        if (randomCardList.Count == 0)
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

    Card GetCardData(int id)
    {
        foreach (Card item in cardData)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    GameObject GetCard()
    {
        GameObject gameObject = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, cardObjectTransform);
        gameObject.GetComponent<CardBehavior>().SetData(GetRandomCardData());
        gameObject.GetComponent<CardBehavior>().playCardEvent += OnPlayCardEvent;
        gameObject.GetComponent<CardBehavior>().pointerEnterEvent += OnCardEnterEvent;
        gameObject.GetComponent<CardBehavior>().pointerExitEvent += OnCardExitEvent;
        return gameObject;
    }

    List<MatchInfo> GetWomenMatchData(int id)
    {
        List<MatchInfo> matchData = new List<MatchInfo>();
        for (int i = 0; i < excelData.matchTable.Count; i++)
        {
            if (excelData.matchTable[i].womenId == id)
            {
                matchData.Add(excelData.matchTable[i]);
            }
        }
        return matchData;
    }

    GameObject CreateFriendCell(WomenInfo data)
    {
        Transform parent = scrollViewDictionary[State.FRIEND].GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
        GameObject gameObject = Instantiate(friendCellPrefab, parent);
        gameObject.GetComponent<FriendCellBehavior>().SetData(data);
        gameObject.GetComponent<FriendCellBehavior>().clickEvent += OnFriendCellClickEvent;
        return gameObject;
    }


    GameObject CreateNavCell(State state, Transform parent)
    {
        GameObject cell = Instantiate(navCellPrefab, parent);
        cell.GetComponent<NavCellBehavior>().SetData(navDictionary[state]);
        cell.GetComponent<NavCellBehavior>().clickEvent += OnNavCellClickEvent;
        return cell;
    }

    //format women accept card id
    //void InitFormatCardId()
    //{
    //    for (int i = 0; i < womenQuestionDatas.Length; i++)
    //    {
    //        foreach (var item in womenQuestionDatas[i])
    //        {
    //            string[] strs = item.cardsId.Split(FORMAT_ACCEPT_CARD_CHARACTER);
    //            item.cardId = new int [strs.Length];
    //            for (int j = 0; j < strs.Length; j++)
    //            {
    //                item.cardId[j] = Convert.ToInt32(strs[j]);
    //            }
    //        }
    //    }
    //}

   

    void DeleteCard(CardBehavior card)
    {
        //card.playCardEvent -= OnPlayCardEvent;
        //card.pointerEnterEvent -= OnCardEnterEvent;
        //card.pointerExitEvent -= OnCardExitEvent;
        Destroy(card.gameObject);
        playerCards.Remove(card.gameObject);
    }


    void ShowInRoomMessage(MessageType type, int id)
    {
        if (type == MessageType.Player)
        {
            Card card = GetCardData(id);
            Transform parent = scrollViewDictionary[State.ROOM].GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
            if(card.type == CardType.Text)
            {
                GameObject gameObject = Instantiate(roomMeTextPrefab, parent);
                gameObject.GetComponentInChildren<Text>().text = card.description;
            }
            else
            {
                GameObject gameObject = Instantiate(roomMeImagePrefab, parent);
                gameObject.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>(card.fileName);
            }
            StartCoroutine(AutoScroll());
        }
        else
        {
            string[] messages = type == MessageType.WomenQuestion ? 
                GetQuestionMessageString(id) : GetResponseMessageString(id);
            for (int i = 0; i < messages.Length; i++)
            {
                AddWomenMessage(messages[i]);
                if(i == messages.Length - 1 && type == MessageType.WomenQuestion)
                {
                    SetCanUseCard(true);
                }
            }
        }
    }

    void AddWomenMessage(string text)
    {
        Transform parent = scrollViewDictionary[State.ROOM].GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
        GameObject gameObject = Instantiate(roomGirlPrefab, parent);
        gameObject.GetComponentInChildren<Text>().text = text;
        gameObject.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>(GetTalkWomen().GetData().fileName);
        StartCoroutine(AutoScroll());
    }
 
    IEnumerator AutoScroll()
    {
        RectTransform parent = scrollViewDictionary[State.ROOM].GetComponentInChildren<ContentSizeFitter>().gameObject.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollViewDictionary[State.ROOM].GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;
    }

   
    void SetCanUseCard(bool boolean)
    {
        Debug.Log("playerCards.Count" + playerCards.Count);
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].GetComponent<CardBehavior>().SetCanUse(boolean);
        }
    }

}

