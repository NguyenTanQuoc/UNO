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
        if (player.CardList.Count != 1) return true;
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
            // Lấy vị trí của người chơi hiện tại đang thực hiện hành động click này
            // (Nếu game ông dùng biến khác để lưu index của Local Player thì thay vào đây nhé)
            int currentPlayerIndex = Game.CurrentPlayer;

            // Nếu lá bài KHÔNG phải là bài số (tức là bài chức năng: Cấm, Đảo chiều, +2, +4, Đổi màu...)
            if (currentCard.cardType != Card.CardType.Number)
            {
                // Xử lý riêng cho bài Đổi màu (Wild) hoặc Thêm 4 (Draw_4)
                if (currentCard.cardType == Card.CardType.Wild || currentCard.cardType == Card.CardType.Draw_4)
                {
                    Desk.Main.DisableDraw();
                    // SỬA LỖI: Truyền thêm currentPlayerIndex vào hàm
                    Rpc.Main.DeleteCardServerRpc(currentCard.Id, currentPlayerIndex);
                    Game.Main.Wild(currentCard);
                }
                // Xử lý cho các bài chức năng còn lại (Cấm, Đảo chiều, +2)
                else
                {
                    Desk.Main.DisableDraw();
                    // SỬA LỖI: Truyền thêm currentPlayerIndex vào hàm
                    Rpc.Main.DeleteCardServerRpc(currentCard.Id, currentPlayerIndex);
                    Game.Main.NextTurnServerRpc(currentCard.Id);
                }
            }
            // Nếu lá bài LÀ bài số bình thường
            else
            {
                Desk.Main.DisableDraw();
                Rpc.Main.ChangeTableCardServerRpc(currentCard.Id);
                // SỬA LỖI: Truyền thêm currentPlayerIndex vào hàm
                Rpc.Main.DeleteCardServerRpc(currentCard.Id, currentPlayerIndex);
                Game.Main.NextTurnServerRpc();
            }
        }
    }
    #endregion

}