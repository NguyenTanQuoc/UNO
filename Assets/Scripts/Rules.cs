using UnityEngine;

public class Rules : MonoBehaviour
{
    #region Attributes
    public static Rules Main { get; private set; }

    private Card currentCard;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
    }
    #endregion

    #region UNO Game Rules
    public bool Rule(Card card)
    {
        currentCard = Cards.GetCardById(Table.Main.CardList[^1]);
        if (card.cardType == Card.CardType.Wild || card.cardType == Card.CardType.Draw_4)
            return true;
        if (currentCard.cardType == Card.CardType.Draw_2)
        {
            if (card.cardType == currentCard.cardType)
                return true;
        }
        if (currentCard.cardColor == card.cardColor)
        {
            return true;
        }
        if (currentCard.cardType == Card.CardType.Skip)
        {
            if (card.cardType == currentCard.cardType)
                return true;
        }
        if (currentCard.cardType == Card.CardType.Reverse)
        {
            if (card.cardType == currentCard.cardType)
                return true;
        }
        if (currentCard.cardNumber == card.cardNumber && currentCard.cardNumber != -1)
            return true;
        return false;
    }
    #endregion
}