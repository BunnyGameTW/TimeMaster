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

public class WomenBehavior : MonoBehaviour
{
    const float ASK_QUESTION_TIME = 10.0f;
    const float IDLE_TIME = 10.0f;
    const int IDLE_SCORE = 10;
    const int WRONG_ANSWER_SCORE = 10;
    const float MIN_START_TIME = 0.0f;
    const float MAX_START_TIME = 5.0f;
    const int UNREAD_INDEX = -1;



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
    List<Message> historyMessageList = new List<Message>();
    List<int> randomQuestionList = new List<int>();
    List<int> startQuestionIdList = new List<int>();
    List<int> wrongResponseIdList = new List<int>();
    List<int> idleResponseIdList = new List<int>();


    public event EventHandler<MessageEvenArgs> showMessageEvent;

    void Start()
    {
        RANDOM_START_TIME = UnityEngine.Random.Range(MIN_START_TIME, MAX_START_TIME);
        idleTimer = timer = 0.0f;
        questionCounter = 0;
        unreadCounter = 0;
        unreadIndex = -1;
        hasAskQuestion = false;
    }

    // Update is called once per frame
    void Update()
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

    public void AddUnreadMessage(Message message)
    {
        if(unreadIndex != UNREAD_INDEX)//TODO show history message and unread line
        {
            unreadIndex = historyMessageList.Count;
        }
        unreadCounter++;
    }

    public int GetScore()
    {
        return score;
    }

    //event
    public void OnPlayCardEvent(Card card)
    {
        hasAskQuestion = false;
        timer = 0;

        //response
        MatchInfo info = GetMatchResponse(card);
        if (info != null)
        {
            AddScore(info.addScore);
            AddCoolDownTime(info.cdTime);
            AddMessage(info.responseId, MessageType.WomenResponse);
            if (info.hasMultipleQuestion)
            {
                AskQuestion(info.nextQuestionId);
            }
        }
        else
        {
            int responseId = wrongResponseIdList[UnityEngine.Random.Range(0, wrongResponseIdList.Count)];
            AddScore(WRONG_ANSWER_SCORE);
            AddMessage(responseId, MessageType.WomenResponse);
        }

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
        score = score < 0 ? 0 : score;
    }

    void AddCoolDownTime(float time)
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
            timer = ASK_QUESTION_TIME;
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
            hasAskQuestion = true;
            idleTimer = 0;
            int index = UnityEngine.Random.Range(0, randomQuestionList.Count);
            AskQuestion(randomQuestionList[index]);
        }
    }
}
