using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class WomenBehavior : MonoBehaviour
{
    const float ASK_QUESTION_TIME = 10.0f;
    const float IDLE_TIME = 5.0f;
    const float IDLE_SCORE = 10.0f;

    WomenInfo data;
    List<WomenQuestion> questionData;
    List<WomenResponse> responseData;
    float score;
    float timer, idleTimer;
    int[] historyMessage;
    int askQuestionNumber;
    bool idleFlag;
    List<int> randomQuestionList;
    public event EventHandler<EventArgs> showMessageEvent;
    Dictionary<WomenQuestionType, List<int>> questionDictionary;
    List<int> idleQuestionList, startQuestionList;


    void Start()
    {
        idleTimer = timer = 0.0f;
        askQuestionNumber = 0;
        idleFlag = false;
        randomQuestionList = new List<int>();
        questionDictionary = new Dictionary<WomenQuestionType, List<int>>();
        questionDictionary.Add(WomenQuestionType.Idle, idleQuestionList);
        questionDictionary.Add(WomenQuestionType.Start, startQuestionList);
    }

    // Update is called once per frame
    void Update()
    {
        //if (idleFlag)
        //{
        //    idleTimer += Time.deltaTime;
        //    if(idleTimer > IDLE_TIME)
        //    {
        //        idleTimer = 0;
        //        int id = GetQuestion(WomenQuestionType.Idle);
        //        AskQuestion(id);
        //        AddScore(id);
        //        idleFlag = false;
        //        timer = ASK_QUESTION_TIME;
        //    }
        //}
        //else
        //{
        //    timer += Time.deltaTime;
        //    if (timer > ASK_QUESTION_TIME)
        //    {
        //        idleFlag = true;
        //        idleTimer = 0;
        //        AskQuestion(GetRandomQuestion());
        //    }
        //}
    }
    
    public void SetData(WomenInfo info, List<WomenQuestion> questions, List<WomenResponse> responses)
    {
        data = info;
        questionData = questions;
        responseData = responses;
    }

    public void AddCard(Card card)
    {
        //response
        idleFlag = false;
        timer = 0;

        //AddMessageId();//TODO
        showMessageEvent?.Invoke(this, EventArgs.Empty);
    }

    void AddScore(int questionId)
    {
        //TODO讀表加懷疑
        score += IDLE_SCORE;
    }


    void AskQuestion(int id)//TODO 初始問題
    {
       

    
    }

    int GetQuestion(WomenQuestionType type)
    {
        if (questionDictionary[type].Count == 0)
        {
            foreach (WomenQuestion question in questionData)
            {
                if (question.type == type)
                {
                    questionDictionary[type].Add(question.id);
                }
            }
        }

        int index = UnityEngine.Random.Range(0, questionDictionary[type].Count);
        int questionId = questionDictionary[type][index];
        questionDictionary[type].RemoveAt(index);
        return questionId;
    }

    int GetRandomQuestion()//TODO 連續問答 是否指定下個問題 多行文字
    {
        if(randomQuestionList.Count == 0)
        {
            foreach (WomenQuestion question in questionData)
            {
                if(question.type == WomenQuestionType.Normal || question.type == WomenQuestionType.Query)
                {
                    randomQuestionList.Add(question.id);
                }
            } 
        }

        int index = UnityEngine.Random.Range(0, randomQuestionList.Count);
        int questionId = randomQuestionList[index];
        randomQuestionList.RemoveAt(index);
        return questionId;
    }
}
