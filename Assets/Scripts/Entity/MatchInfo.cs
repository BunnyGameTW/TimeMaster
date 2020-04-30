using System;

[Serializable]
public class MatchInfo
{
    public int womenId;
    public int questionId;
    public int cardId;
    public int responseId;
    public float cdTime;
    public int addScore;
    public bool hasMultipleQuestion;
    public int nextQuestionId;
}
