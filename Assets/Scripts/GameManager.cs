using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;

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
    public Transform selectCardTransform, newMessageTransform;
    public GameObject titleObject, navBarObject;
    public GameObject freindScrollViewObject, chatScrollViewObject, roomScrollViewObject;
    public GameObject meObject, roomObject;
    public GameObject womenPrefab;
    public GameObject newMessagePrefab;
    public AudioClip wrong, correct, click;
    public GameObject endObject;
    public TextMeshProUGUI endText, scoreText;
    public Image endImage;
    public TextMeshProUGUI timeText;
    public Slider changeBackgroundSlider;
    public Sprite normalBackground, nakedBackground;
    public Image backgroundImage;
    public GameObject settingObject;

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
    const string NAV_FRIEND_ICON_FILE_NAME = "icon_friend";
    const string NAV_CHAT_ICON_FILE_NAME = "icon_chat";
    const string NAV_SETTING_ICON_FILE_NAME = "icon_setting";
    const string PLAYER_NAME = "小豬";
    const string PLAYER_STATE = "I wonna know, 你行不行";
    const string PLAYER_PHOTO_FILE_NAME = "player_photo";
    const float GAME_TIME = 60.0f;
    const string HAPPY_END_STRING = "你精通了時間管理術\n持續徜徉於多人聊天運動的快樂中";
    readonly float [] PITCH_LEVEL = { 1.2f, 1.5f, 2.0f };
    const float CHANGE_PITCH_RATIO = 0.2f;
    const float MULTIPLE_LINE_DELAY_TIME = 1.0f;
    float CARD_TOTAL_WIDTH;

    List<Card> cardData;

    enum State
    {
        FRIEND, CHAT, ROOM, SETTING
    }
    State gameState;
    float timer;
    int womenIndex;
    bool isGameOver;
    float secondTime;
    List<Card> randomCardList;
    List<GameObject> playerCards;
    Dictionary<State, GameObject> scrollViewDictionary;
    Dictionary<State, string> titleTextDictionary;
    Dictionary<State, NavData> navDictionary;
    Dictionary<State, GameObject> navCellDictionary;
    List<GameObject> friendList;
    List<WomenBehavior> womenList;
    List<ChatCellBehavior> chatList;
    AudioSource audioSource, bgmAudioSource;
    float nowPitch;
    int pitchLevel;
    bool isChangePitch;
    void Start()
    {
        gameState = State.FRIEND;
        timer = 0.0f;
        secondTime = Mathf.Infinity;
        isChangePitch = isGameOver = false;
        cardData = excelData.cardTable;
        womenIndex = NO_WOMEN_INDEX;
        pitchLevel = -1;
        randomCardList = new List<Card>();
        chatList = new List<ChatCellBehavior>();
        audioSource = GetComponent<AudioSource>();
        bgmAudioSource = Camera.main.GetComponent<AudioSource>();
        nowPitch = audioSource.pitch;
        changeBackgroundSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
        InitPlayerCards();
        InitScrollViewDictionary();
        InitTitleDictionary();
        InitNavBar();
        InitMeInfo();
        InitFriendList();
        InitWomenList();

        CARD_TOTAL_WIDTH = playerCards[0].gameObject.GetComponentInChildren<RectTransform>().sizeDelta.x;

        SwitchState();
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer += Time.deltaTime;
            if(Mathf.FloorToInt(timer) != secondTime)
            {
                secondTime = Mathf.FloorToInt(timer);
                timeText.text = (GAME_TIME - Mathf.FloorToInt(timer)).ToString();
            }
            if (timer >= GAME_TIME)
            {
                isGameOver = true;
                GameOver(NO_WOMEN_INDEX, GetScore());
            }
            if (isChangePitch)
            {
                nowPitch += Time.deltaTime * CHANGE_PITCH_RATIO;
                if(nowPitch >= PITCH_LEVEL[pitchLevel])
                {
                    isChangePitch = false;
                }
                ChangePitch();
            }
        }
        else
        {
            nowPitch -= Time.deltaTime * CHANGE_PITCH_RATIO;
            if (nowPitch <= 1)
            {
                nowPitch = 1;
            }
            ChangePitch();
        }
    }

    int GetScore()
    {
        int score = 0;
        foreach (WomenBehavior item in womenList)
        {
            score += item.GetScore();
        }
        return score;
    }

    //public 
    public void OnBackButtonClicked()
    {
        audioSource.PlayOneShot(click);
        gameState = State.CHAT;
        womenIndex = NO_WOMEN_INDEX;
        SwitchState();
    }

    void OnSliderValueChanged()
    {
        if(changeBackgroundSlider.value == 0.0f)
        {
            backgroundImage.sprite = normalBackground;
            changeBackgroundSlider.GetComponentsInChildren<Image>()[0].color = Color.gray;
        }
        else
        {
            backgroundImage.sprite = nakedBackground;
            changeBackgroundSlider.GetComponentsInChildren<Image>()[0].color = new Color(0.65f, 1.0f, 0.7f);
        }
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

    //TODO
    void OnCardEnterEvent(object sender, CardEventArgs param)
    {
        CardBehavior cardModel = (CardBehavior)sender;
        //cardModel.transform.SetAsFirstSibling();
        cardModel.transform.SetParent(selectCardTransform);
    }

    void OnCardExitEvent(object sender, CardEventArgs param)
    {
        UpdatePlayerCards();
    }

    void OnNavCellClickEvent(object sender, EventArgs param)
    {
        audioSource.PlayOneShot(click);
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
        audioSource.PlayOneShot(click);
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
            ShowInRoomMessages(param.message.type, param.message.id, false);
        }
        else
        {
            ShowNewMessages(param.message, women);
        }
    }

    void OnChatCellClickEvent(object sender, EventArgs param)
    {
        audioSource.PlayOneShot(click);
        ChatCellBehavior chatCell = (ChatCellBehavior)sender;
        gameState = State.ROOM;
        womenIndex = chatCell.GetData().id;
        SwitchState();
    }

    void OnNewMessageClickEvent(object sender, NewMessageEventArgs param)
    {
        CheckNewMessageObject(param.womenId);
        audioSource.PlayOneShot(click);
        womenIndex = param.womenId;
        gameState = State.ROOM;
        SwitchState();
    }

    void OnAudioEvent(object sender, AudioEventArgs param)
    {
        audioSource.PlayOneShot(param.responseType == WomenResponseType.Wrong ? wrong : correct);
    }

    void OnGameOverEvent(object sender, EventArgs param)
    {
        isGameOver = true;
        
        WomenBehavior women = (WomenBehavior)sender;
        GameOver(women.GetData().id, GetScore());
    }

    void OnChangePitchEvent(object sender, PitchEventArgs param)
    {
        if(param.i > pitchLevel)
        {
            pitchLevel = param.i;
            isChangePitch = true;

        }
    }


    //private
    void InitWomenList()
    {
        womenList = new List<WomenBehavior>();
        for (int i = 0; i < excelData.womenTable.Count; i++)
        {
            //if (i < 1)//TODO removed
            //{
                WomenBehavior women = Instantiate(womenPrefab).GetComponent<WomenBehavior>();
                women.SetData(
                    excelData.womenTable[i],
                    excelData.questionTable,
                    excelData.responseTable,
                    GetWomenMatchData(excelData.womenTable[i].id));
                women.showMessageEvent += OnShowMessageEvent;
                women.audioEvent += OnAudioEvent;
                women.gameOverEvent += OnGameOverEvent;
                women.changePitchEvent += OnChangePitchEvent;
                womenList.Add(women);
            //}
        }
    }

   

    void InitMeInfo()
    {
        meObject.GetComponentsInChildren<TextMeshProUGUI>()[0].text = PLAYER_NAME;
        meObject.GetComponentsInChildren<TextMeshProUGUI>()[1].text = PLAYER_STATE;
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
        UpdatePlayerCards();
        UpdateScrollView();
        UpdateTitle();
        UpdateNavBar();

        meObject.SetActive(gameState == State.FRIEND);
        navBarObject.SetActive(gameState != State.ROOM);
        cardObjectTransform.gameObject.SetActive(gameState == State.ROOM);
        selectCardTransform.gameObject.SetActive(gameState == State.ROOM);
        roomObject.SetActive(gameState == State.ROOM);
        settingObject.SetActive(gameState == State.SETTING);

        if(gameState == State.ROOM)
        {
            foreach (Transform child in roomScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.transform)
            {
                Destroy(child.gameObject);
            }

            UpdateScore();
            GetTalkWomen().ResetUnreadNumber();

            GetTalkWomen().SetFormatNameIndex(0);
            List<Message> list = GetTalkWomen().GetHistoryMessageList();
            foreach (Message item in list)
            {
                SetCanUseCard(false);
                ShowInRoomMessages(item.type, item.id, true);
            }
        }
    }

    void UpdateTitle()
    {
        string title = titleTextDictionary[gameState];
        if (gameState == State.ROOM)
        {
            title = GetTalkWomen().GetData().name;
        }
        titleObject.GetComponentInChildren<TextMeshProUGUI>().text = title;
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
            pair.Value.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
        }

        GameObject gameObject = navCellDictionary[gameState];
        gameObject.transform.localScale = new Vector3(NAV_ICON_SCALE_RATIO, NAV_ICON_SCALE_RATIO, 1);
        gameObject.GetComponentInChildren<Image>().color = Color.white;
        gameObject.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
    }

    void UpdatePlayerCards()
    {
        //position
        float mid = (playerCards.Count - 1) / 2;
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].GetComponent<CardBehavior>().UpdatePosition(new Vector3((i - mid) * CARD_TOTAL_WIDTH, CARD_START_POSITION_Y, 0));
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
        roomObject.GetComponentInChildren<TextMeshProUGUI>().text = score.ToString();
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

    string[] GetQuestionMessageString(int id, WomenBehavior women, bool isHistory)
    {
        WomenQuestion q = GetQuestionData(id);
        string str = q.description;
        
        //format name
        if (!isHistory)
        {
            List<WomenInfo> list = GetNotIncludedWomenInfo(women.GetData().id);
            for (int i = 0; i < q.formatNumber; i++)
            {
                int index = UnityEngine.Random.Range(0, list.Count);
                women.AddFormatName(list[index].name);
                list.RemoveAt(index);
            }
        }

        if(q.formatNumber == 1)
        {
            str = string.Format(str, women.GetFormatName());
            women.SetFormatNameIndex(women.GetFormatNameIndex() + 1);
        }
        else if(q.formatNumber == 2)
        {
            string name1 = women.GetFormatName();
            women.SetFormatNameIndex(women.GetFormatNameIndex() + 1);
            string name2 = women.GetFormatName();
            women.SetFormatNameIndex(women.GetFormatNameIndex() + 1);
            str = string.Format(str, name1, name2);
        }

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

    ChatCellBehavior GetChatCell(int id)
    {
        foreach (ChatCellBehavior item in chatList)
        {
            if (item.GetData().id == id)
            {
                return item;
            }
        }
        return null;
    }

    GameObject CreateFriendCell(WomenInfo data)
    {
        Transform parent = freindScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
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
        Destroy(card.gameObject);
        playerCards.Remove(card.gameObject);
    }


    void ShowInRoomMessages(MessageType type, int id, bool isHistory)
    {
        if (type == MessageType.Player)
        {
            //player message
            Card card = GetCardData(id);
            Transform parent = roomScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
            if(card.type == CardType.Text)
            {
                GameObject gameObject = Instantiate(roomMeTextPrefab, parent);
                gameObject.GetComponentInChildren<TextMeshProUGUI>().text = card.description;
            }
            else
            {
                GameObject gameObject = Instantiate(roomMeImagePrefab, parent);
                gameObject.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>(card.fileName);
            }


            string text = card.type == CardType.Text ? card.description : "傳送了圖片";//TODO
            UpdateChatList(GetTalkWomen(), text, true);

            StartCoroutine(AutoScroll());
        }
        else
        {
            string[] messages = type == MessageType.WomenQuestion ? 
                GetQuestionMessageString(id, GetTalkWomen(), isHistory) : GetResponseMessageString(id);
            if (!isHistory)
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    StartCoroutine(ShowInRoomMessage(MULTIPLE_LINE_DELAY_TIME * i, messages, i, type));
                }
            }
            else
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    AddWomenMessage(messages[i]);
                    UpdateChatList(GetTalkWomen(), messages[i], true);

                    if (i == messages.Length - 1 && type == MessageType.WomenQuestion)
                    {
                        SetCanUseCard(true);
                    }
                }
            }
          
        }
    }

    IEnumerator ShowInRoomMessage(float waitTime, string [] messages, int i, MessageType type)
    {
        yield return new WaitForSeconds(waitTime);
        AddWomenMessage(messages[i]);
        UpdateChatList(GetTalkWomen(), messages[i], true);

        if (i == messages.Length - 1 && type == MessageType.WomenQuestion)
        {
            SetCanUseCard(true);
        }
    }

    void ShowNewMessages(Message message, WomenBehavior women)
    {
        string[] messages = message.type == MessageType.WomenQuestion ?
             GetQuestionMessageString(message.id, women, false) : GetResponseMessageString(message.id);
        for (int i = 0; i < messages.Length; i++)
        {
            StartCoroutine(ShowNewMessage(MULTIPLE_LINE_DELAY_TIME * i, women, messages, i));
        }
    }

    IEnumerator ShowNewMessage(float waitTime, WomenBehavior women, string [] messages, int i)
    {
        yield return new WaitForSeconds(waitTime);
        NewMessageBehavior messageObject = Instantiate(newMessagePrefab, newMessageTransform).GetComponent<NewMessageBehavior>();
        messageObject.SetData(women.GetData(), messages[i]);
        messageObject.clickNewMessageEvent += OnNewMessageClickEvent;
        messageObject.GetComponent<AudioSource>().pitch = nowPitch;
        women.AddUnreadNumber();
        UpdateChatList(women, messages[i], false);

    }

    void UpdateChatList(WomenBehavior women, string text, bool isTalkWomen)
    {
        //update chat cell
        ChatCellBehavior chatCell = GetChatCell(women.GetData().id);
        if (chatCell == null)
        {
            Transform parent = chatScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
            chatCell = Instantiate(chatCellPrefab, parent).GetComponent<ChatCellBehavior>();
            chatCell.SetData(women.GetData(), women);
            chatCell.clickEvent += OnChatCellClickEvent;
            chatList.Add(chatCell);
        }
        chatCell.UpdateUI(text, isTalkWomen ? 0 : women.GetUnreadNumber());

        //reorder list
        chatCell.gameObject.transform.SetAsFirstSibling();
    }

    void AddWomenMessage(string text)
    {
        Transform parent = roomScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.transform;
        GameObject gameObject = Instantiate(roomGirlPrefab, parent);
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = text;
        gameObject.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>(GetTalkWomen().GetData().fileName);
        StartCoroutine(AutoScroll());
    }
 
    IEnumerator AutoScroll()
    {
        RectTransform parent = roomScrollViewObject.GetComponentInChildren<ContentSizeFitter>().gameObject.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        roomScrollViewObject.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 0;
    }
   
    void SetCanUseCard(bool boolean)
    {
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].GetComponent<CardBehavior>().SetCanUse(boolean);
        }
    }


    void CheckNewMessageObject(int id)
    {
        foreach (Transform item in newMessageTransform)
        {
            if (item.GetComponent<NewMessageBehavior>().GetWomenId() == id)
            {
                Destroy(item.gameObject);
            }
        }
    }

    void ChangePitch()
    {
        audioSource.pitch = nowPitch;
        bgmAudioSource.pitch = nowPitch;
    }

    void GameOver(int id, int score)
    {

        foreach (WomenBehavior item in womenList)
        {
            item.SetGameOver();
        }
        Destroy(newMessageTransform.gameObject);
        //TODO 有圖時要換
        string fileName;
        if (id == NO_WOMEN_INDEX)
        {
            fileName = "end_0";
        }
        else
        {
            fileName = id > 2 ? "to_be_continue" : string.Format("end_{0}", id);
        }

        string charName = id == NO_WOMEN_INDEX ? "大師" : womenList[id - 1].GetData().name;

        endImage.sprite = Resources.Load<Sprite>(fileName);
        endText.text = id == NO_WOMEN_INDEX ? HAPPY_END_STRING : excelData.womenTable[id - 1].end;
        scoreText.text = charName + "結局：\n" + string.Format(scoreText.text, score);
        endObject.SetActive(true);
    }

}

