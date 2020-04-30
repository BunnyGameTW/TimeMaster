using System;

[Serializable]
public class WomenQuestion
{
    public int id;
    public WomenQuestionType type;
    public string description;
    public int formatNumber;
    public bool hasMuitipleLine;
}

public enum WomenQuestionType
{
    Start,Query,Normal    
}
