using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WomenQuestion
{
    public int id;
    public WomenQuestionType type;
    public string description;
    public string cardsId;
    public int[] cardId;
}

public enum WomenQuestionType
{
    Start,Query,Idle,Normal    
}
