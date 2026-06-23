using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Netcode;

public class CardUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Attributes
    private Card currentCard;
    private Player player;
    private Image image;
    private bool isPointer;
    #endregion

    #region Call When Game Start
    private void Awake() => image = GetComponent<Image>();
    #endregion

    #region Update Card On Hand
    public void UpdateUI(Card card, Player p)
    {
        player = p;
        currentCard = card;
        image.sprite = GameSprites.GetCardImage(card);
    }
    #endregion

    #region Check If Card Can Play
    private void Update()
    {
        if (!CanPlay())
        {
            image.color = Color.gray;
            return;
        }
        bool isValidCard = false;
        if (player.InSteak.ContainsKey(2) || player.InSteak.ContainsKey(4))
        {
            var card = Cards.GetCardById(Table.Main.CardList[^1]);
            if (card.cardType == currentCard.cardType)
            {
                isValidCard = true;
            }
            if (card.cardColor == currentCard.cardColor && currentCard.cardType == Card.CardType.Reverse)
            {
                isValidCard = true;
            }
            if (player.InSteak.ContainsKey(2) && currentCard.cardType == Card.CardType.Draw_2 || player.InSteak.ContainsKey(4) && currentCard.cardType == Card.CardType.Draw_4)
            {
                isValidCard = true;
            }
        }
        else
        {
            isValidCard = Rules.Main.Rule(currentCard);
        }
        image.color = isValidCard ? Color.white : Color.gray;
    }

    private bool CanPlay()
    {
        return player.Turn && !Game.Main.chooseColor.activeSelf && IsNotLastInvalidCard();
    }

    private bool IsNotLastInvalidCard()
    {
        if (player.CardList.Count != 1) 
            return true;
        return currentCard.cardType != Card.CardType.Draw_4 && currentCard.cardType != Card.CardType.Wild;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image.color == Color.white)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + 10);
            isPointer = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPointer)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - 10);
            isPointer = false;
        }
    }
    #endregion

    #region Card Click Event
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPointer)
        {
            if (currentCard.cardType != Card.CardType.Number)
            {
                if (currentCard.cardType == Card.CardType.Wild || currentCard.cardType == Card.CardType.Draw_4)
                {
                    Desk.Main.DisableDraw();
                    Rpc.Main.DeleteCardServerRpc(currentCard.Id);
                    Game.Main.Wild(currentCard);
                }
                else
                {
                    Desk.Main.DisableDraw();
                    Rpc.Main.DeleteCardServerRpc(currentCard.Id);
                    Game.Main.NextTurnServerRpc(currentCard.Id);
                }
            }
            else
            {
                Desk.Main.DisableDraw();
                Rpc.Main.ChangeTableCardServerRpc(currentCard.Id);
                Rpc.Main.DeleteCardServerRpc(currentCard.Id);
                Game.Main.NextTurnServerRpc();
            }
        }
    }
    #endregion

}