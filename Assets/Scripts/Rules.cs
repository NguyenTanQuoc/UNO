using UnityEngine;

public class Rules : MonoBehaviour
{
    #region Attributes
    public static Rules Main { get; private set; }

    private Card currentCart;
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
        currentCart = Cards.GetCardById(Table.Main.CardList[^1]);
        if (card.cardType == Card.CardType.Wild || card.cardType == Card.CardType.Draw_4)
        {
            return true;
        }
        if (currentCart.cardType == Card.CardType.Draw_2)
        {
            if (card.cardType == currentCart.cardType)
            {
                return true;
            }
        }
        if (currentCart.cardColor == card.cardColor)
        {
            return true;
        }
        if (currentCart.cardType == Card.CardType.Skip)
        {
            if (card.cardType == currentCart.cardType)
            {
                return true;
            }
        }
        if (currentCart.cardType == Card.CardType.Reverse)
        {
            if (card.cardType == currentCart.cardType)
            {
                return true;
            }
        }
        if (currentCart.cardNumber == card.cardNumber && currentCart.cardNumber != -1)
        {
            return true;
        }
        return false;
    }
    #endregion
}