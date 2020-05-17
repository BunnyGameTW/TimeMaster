using System;

[Serializable]
public class Card
{
    public int id;
    public CardType type;
    public string description;
    public string fileName;
    public int probability;
}

public enum CardType
{
    Text, Image
}
