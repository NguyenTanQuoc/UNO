using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Cards/Card")]
[Serializable]
public class Card : ScriptableObject
{
    #region Attributes
    public int Id;
    public CardType cardType;
    public CardColor cardColor;
    public int cardNumber;
    #endregion

    #region Card Color
    public enum CardColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow
    }
    #endregion

    #region Card Type
    public enum CardType
    {
        Number,
        Reverse,
        Skip,
        Draw_2,
        Draw_4,
        Wild,
    }
    #endregion
}