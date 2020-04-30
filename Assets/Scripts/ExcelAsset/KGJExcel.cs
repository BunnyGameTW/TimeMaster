using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset]
public class KGJExcel : ScriptableObject
{
    public List<MatchInfo> matchTable; // Replace 'EntityType' to an actual type that is serializable.
    public List<WomenQuestion> questionTable; // Replace 'EntityType' to an actual type that is serializable.
    public List<Card> cardTable; // Replace 'EntityType' to an actual type that is serializable.
    public List<WomenResponse> responseTable; // Replace 'EntityType' to an actual type that is serializable.
    public List<WomenInfo> womenTable; // Replace 'EntityType' to an actual type that is serializable.
}
