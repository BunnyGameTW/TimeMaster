using System;

[Serializable]
public class WomenResponse
{
    public int id;
    public WomenResponseType type;
    public string description;
    public bool hasMuitipleLine;
    public int womenId;
}

public enum WomenResponseType
{
    Idle, Normal, Wrong
}
