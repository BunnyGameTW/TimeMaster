using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum MessageType
{
    Player, WomenQuestion, WomenResponse
}

public struct Message
{
    public MessageType type;
    public int id;

    public Message(int index, MessageType t)
    {
        id = index;
        type = t;
    }
}

public class MessageEvenArgs : EventArgs
{
    public Message message;
    public MessageEvenArgs(Message msg)
    {
        message = msg;
    }
}

public class AudioEventArgs : EventArgs
{
    public WomenResponseType responseType;
    public AudioEventArgs(WomenResponseType type)
    {
        responseType = type;
    }
}

public class PitchEventArgs: EventArgs
{
    public int i;
    public PitchEventArgs(int level)
    {
        i = level;
    }
}

public class WomenBehavior : MonoBehaviour
{
    public const float ASK_QUESTION_TIME = 6.0f;
    public const float IDLE_TIME = 6.0f;
    const int IDLE_SCORE = 15;
    const int WRONG_ANSWER_SCORE = 10;
    const float MIN_START_TIME = 0.0f;
    const float MAX_START_TIME = 5.0f;
    const int UNREAD_INDEX = -1;
    const int MIN_SCORE = 0;
    const int MAX_SCORE = 100;
    readonly int [] WARNING_SCORE = { 40, 60, 80 };
    const float AFTER_IDLE_DELAY_TIME = 1.0f;
    const float RESPONSE_DELAY_TIME = 1.0f;


    WomenInfo data;
    List<WomenQuestion> questionData;
    List<WomenResponse> responseData;
    List<MatchInfo> matchData;

    int score;
    float timer, idleTimer;
    float RANDOM_START_TIME;
    bool hasAskQuestion;
    int questionCounter;
    int unreadCounter;
    int questionId;
    int unreadIndex;
    int formatNameIndex;
    bool isGameOver;
    List<Message> historyMessageList = new List<Message>();
    List<int> randomQuestionList = new List<int>();
    List<int> startQuestionIdList = new List<int>();
    List<int> wrongResponseIdList = new List<int>();
    List<int> idleResponseIdList = new List<int>();
    List<string> formatNameList = new List<string>();
    Card playerCard;


    public event EventHandler<MessageEvenArgs> showMessageEvent;
    public event EventHandler<AudioEventArgs> audioEvent;
    public event EventHandler<EventArgs> gameOverEvent;
    public event EventHandler<PitchEventArgs> changePitchEvent;


    void Start()
    {
        RANDOM_START_TIME = UnityEngine.Random.Range(MIN_START_TIME, MAX_START_TIME);
        idleTimer = timer = 0.0f;
        questionCounter = 0;
        unreadCounter = 0;
        unreadIndex = UNREAD_INDEX;
        hasAskQuestion = false;
        formatNameIndex = 0;
        score = 0;
        isGameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            if (hasAskQuestion)
            {
                idleTimer += Time.deltaTime;
                CheckIdle();
            }
            else
            {
                timer += Time.deltaTime;
                CheckAskQuestion();
            }
        }
    }

    //public
    public void SetData(WomenInfo info, List<WomenQuestion> questions, List<WomenResponse> responses, List<MatchInfo> matchs)
    {
        data = info;
        questionData = questions;
        responseData = responses;
        matchData = matchs;
        InitQuestionList();
        InitResponseList();
    }

    public WomenInfo GetData()
    {
        return data;
    }

    public void AddMessage(int id, MessageType type)
    {
        Message message = new Message(id, type);
        historyMessageList.Add(message);
        showMessageEvent?.Invoke(this, new MessageEvenArgs(message));
    }


    public void AddUnreadNumber()
    {
        if (unreadIndex != UNREAD_INDEX)
        {
            unreadIndex = historyMessageList.Count;
        }
        unreadCounter++;
    }

    public void ResetUnreadNumber()
    {
        unreadCounter = 0;
        unreadIndex = UNREAD_INDEX;
    }

    public int GetUnreadNumber()
    {
        return unreadCounter;
    }

    public int GetScore()
    {
        return score;
    }

    public List<Message> GetHistoryMessageList()
    {
        return historyMessageList;
    }

    public void AddFormatName(string name)
    {
        formatNameList.Add(name);
    }

    public string GetFormatName()
    {
    
        return formatNameList[formatNameIndex];
    }

    public void SetFormatNameIndex(int i)
    {
        formatNameIndex = i;
    }

    public int GetFormatNameIndex()
    {
        return formatNameIndex;
    }

    public void SetGameOver()
    {
        isGameOver = true;
    }

    public float GetTimer()
    {
        return timer;
    }

    public float GetIdleTimer()
    {
        return idleTimer;
    }

    public bool GetHasAskQuestion()
    {
        return hasAskQuestion;
    }
    //event
    public void OnPlayCardEvent(Card card)
    {
        hasAskQuestion = false;
        timer = 0;
        playerCard = card;
        StartCoroutine(Response(RESPONSE_DELAY_TIME));
    }
    
    //private
    void InitQuestionList()
    {
        foreach (MatchInfo item in matchData)
        {
            WomenQuestion q = GetQuestionData(item.questionId);
            if (q.type == WomenQuestionType.Start && !startQuestionIdList.Contains(item.questionId))
            {
                startQuestionIdList.Add(item.questionId);
            }
            else if (q.type != WomenQuestionType.Start && !randomQuestionList.Contains(item.questionId))
            {
                randomQuestionList.Add(item.questionId);
            }
        }
    }

    void InitResponseList()
    {
        foreach (WomenResponse item in responseData)
        {
            if(item.type == WomenResponseType.Idle)
            {
                idleResponseIdList.Add(item.id);
            }
            else if(item.type == WomenResponseType.Wrong)
            {
                wrongResponseIdList.Add(item.id);
            }
        }
    }

    void AddScore(int number)
    {
        score += number;
        if(score >= MAX_SCORE)
        {
            gameOverEvent?.Invoke(this, EventArgs.Empty);
        }
        else if(score < 0)
        {
            score = 0;
        }

        for (int i = WARNING_SCORE.Length - 1; i >= 0; i--)
        {
            if (score >= WARNING_SCORE[i])
            {
                changePitchEvent?.Invoke(this, new PitchEventArgs(i));
                break;
            }
        }
 
    }

    void AddCoolDownTime(float time)//TODO?
    {
        timer -= time;
    }

    MatchInfo GetMatchResponse(Card card)
    {
        foreach (MatchInfo item in matchData)
        {
            if (item.questionId == questionId && item.cardId == card.id)
            {
                return item;
            }
        }
        return null;
    }

    WomenQuestion GetQuestionData(int id)
    {
        foreach (WomenQuestion item in questionData)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    WomenResponse GetResponseData(int id)
    {
        foreach (WomenResponse item in responseData)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }


    void AskQuestion(int id)
    {
        questionId = id;
        AddMessage(id, MessageType.WomenQuestion);
        questionCounter++;
    }

    void AskStartQuestion()
    {
        int index = startQuestionIdList[UnityEngine.Random.Range(0, startQuestionIdList.Count)];
        questionId = index;
        AddMessage(index, MessageType.WomenQuestion);
        questionCounter++;
    }

    void CheckIdle()
    {
        if (idleTimer > IDLE_TIME)
        {
            hasAskQuestion = false;
            idleTimer = 0;
            timer = ASK_QUESTION_TIME - AFTER_IDLE_DELAY_TIME;
            int responseId = idleResponseIdList[UnityEngine.Random.Range(0, idleResponseIdList.Count)];
            AddMessage(responseId, MessageType.WomenResponse);
            AddScore(IDLE_SCORE);
        }
    }

    void CheckAskQuestion()
    {
        if (timer > RANDOM_START_TIME && questionCounter == 0)
        {
            hasAskQuestion = true;
            AskStartQuestion();
        }
        else if (timer > ASK_QUESTION_TIME)
        {
            int index = UnityEngine.Random.Range(0, randomQuestionList.Count);
            hasAskQuestion = true;
            idleTimer = 0;
            AskQuestion(randomQuestionList[index]);
        }
    }


    IEnumerator AskNextQuestion(int id)
    {
        yield return new WaitForSeconds(RESPONSE_DELAY_TIME);
        hasAskQuestion = true;
        idleTimer = 0;
        AskQuestion(id);
    }

    IEnumerator Response(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        MatchInfo info = GetMatchResponse(playerCard);
        if (info != null)
        {
            AddScore(info.addScore);
            AddCoolDownTime(info.cdTime);
            AddMessage(info.responseId, MessageType.WomenResponse);
            audioEvent?.Invoke(this, new AudioEventArgs(WomenResponseType.Normal));
            if (info.hasMultipleQuestion)
            {
                StartCoroutine(AskNextQuestion(info.nextQuestionId));
            }
        }
        else
        {
            int responseId = wrongResponseIdList[UnityEngine.Random.Range(0, wrongResponseIdList.Count)];
            audioEvent?.Invoke(this, new AudioEventArgs(WomenResponseType.Wrong));
            AddScore(WRONG_ANSWER_SCORE);
            AddMessage(responseId, MessageType.WomenResponse);
        }
    }
}
