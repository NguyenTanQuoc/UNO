using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class Game : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    public Image clock;
    public GameObject chooseColor;

    public static Game Main { get; private set; }
    public static int CurrentPlayer;
    private int drawSteak2;
    private int drawSteak4;
    private bool isReverse;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        drawSteak2 = 0;
        drawSteak4 = 0;
        CurrentPlayer = 0;
        isReverse = false;
    }
    #endregion

    #region Next Player Turn
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NextTurnServerRpc(int id)
    {
        TurnDisableClientRpc();
        if (Cards.GetCardById(id).cardType == Card.CardType.Draw_2)
        {
            Steak2ClientRpc();
            Rpc.Main.ChangeTableCardClientRpc(id);
            if (Cards.RandomCards.Count <= drawSteak2 * 2)
            {
                Cards.Main.Shuffle();
                return;
            }
        }
        if (Cards.GetCardById(id).cardType == Card.CardType.Draw_4)
        {
            Steak4ClientRpc();
            Rpc.Main.ChangeTableCardClientRpc(id);
            if (Cards.RandomCards.Count <= drawSteak4 * 4)
            {
                Cards.Main.Shuffle();
                return;
            }
        }
        ReverseClientRpc(id);
        /*Wild(card);*/
        CheckReverseClientRpc();
        Draw4ClientRpc(id);
        Draw2ClientRpc(id);
        SkipClientRpc(id);
        StartCoroutine(Delay());
    }

    [ClientRpc]
    private void Steak2ClientRpc()
    {
        drawSteak2++;
    }

    [ClientRpc]
    private void Steak4ClientRpc()
    {
        drawSteak4++;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void NextTurnServerRpc()
    {
        TurnDisableClientRpc();
        CheckReverseClientRpc();
        StartCoroutine(Delay());
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ContinueServerRpc(int id)
    {
        ReverseClientRpc(id);
        CheckReverseClientRpc();
        Draw4ClientRpc(id);
        Draw2ClientRpc(id);
        SkipClientRpc(id);
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        Rpc.Main.ResetTimeClockServerRpc();
        yield return new WaitForSeconds(1.5f);
        TurnEnableClientRpc();
    }
    #endregion

    [ClientRpc]
    private void CheckReverseClientRpc()
    {
        CheckReverse();
    }

    private void CheckReverse()
    {
        CurrentPlayer = isReverse == true ? CurrentPlayer - 1 : CurrentPlayer + 1;
        CurrentPlayer = CurrentPlayer < 0 ? UNO.PlayerList.Count - 1 : CurrentPlayer;
        CurrentPlayer = CurrentPlayer >= UNO.PlayerList.Count ? 0 : CurrentPlayer;
    }

    [ClientRpc]
    private void ReverseClientRpc(int id)
    {
        if (Cards.GetCardById(id).cardType == Card.CardType.Reverse)
        {
            isReverse = !isReverse;
            Table.Main.UpdateCard(id);
        }
    }

    [ClientRpc]
    private void SkipClientRpc(int id)
    {
        if (Cards.GetCardById(id).cardType == Card.CardType.Skip)
        {
            CheckReverse();
            Table.Main.UpdateCard(id);
        }
    }

    public void Wild(Card card)
    {
        if (card.cardType == Card.CardType.Wild || card.cardType == Card.CardType.Draw_4)
        {
            if (CheckAI())
            {
                /*int random = Random.Range(0, 4);
                switch (random)
                {
                    case 0:
                        Table.Main.card.Value.cardColor = Card.CardColor.Red;
                        break;
                    case 1:
                        Table.Main.card.Value.cardColor = Card.CardColor.Blue;
                        break;
                    case 2:
                        Table.Main.card.Value.cardColor = Card.CardColor.Green;
                        break;
                    case 3:
                        Table.Main.card.Value.cardColor = Card.CardColor.Yellow;
                        break;
                    default:
                        break;
                }
                if (Table.Main.card.Value.cardType == Card.CardType.Draw_4)
                {
                    CheckReverseClientRpc();
                    Draw4();
                }*/
            }
            else
            {
                chooseColor.SetActive(true);
                Rpc.Main.ChangeTableCardServerRpc(card.Id);
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChooseColorServerRpc(int color)
    {
        int id = Table.Main.CardList[^1];
        if (IsServer)
        {
            if (Cards.GetCardById(Table.Main.CardList[^1]).cardType == Card.CardType.Draw_4)
            {
                switch (color)
                {
                    case 0:
                        id = 112;
                        break;
                    case 1:
                        id = 114;
                        break;
                    case 2:
                        id = 113;
                        break;
                    case 3:
                        id = 115;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (color)
                {
                    case 0:
                        id = 108;
                        break;
                    case 1:
                        id = 110;
                        break;
                    case 2:
                        id = 109;
                        break;
                    case 3:
                        id = 111;
                        break;
                    default:
                        break;
                }
            }
        }
        TurnDisableClientRpc();
        Rpc.Main.ChangeTableCardClientRpc(id);
        CloseChooseColorClientRpc();
        if (Cards.GetCardById(Table.Main.CardList[^1]).cardType == Card.CardType.Draw_4)
        {
            if (Cards.GetCardById(Table.Main.CardList[^1]).cardType == Card.CardType.Draw_4)
            {
                Steak4ClientRpc();
                Debug.Log(drawSteak4 * 4);
                if (Cards.RandomCards.Count <= drawSteak4 * 4)
                {
                    Cards.Main.Shuffle();
                    return;
                }
            }
            CheckReverseClientRpc();
            Draw4ClientRpc(id);
            StartCoroutine(Delay());
        }
        else
        {
            NextTurnServerRpc();
        }
    }

    [ClientRpc]
    private void CloseChooseColorClientRpc()
    {
        chooseColor.SetActive(false);
    }

    [ClientRpc]
    private void Draw4ClientRpc(int id)
    {
        Card card = Cards.GetCardById(id);
        bool ck = false;
        if (card.cardType == Card.CardType.Draw_4 || card.cardType == Card.CardType.Reverse && drawSteak4 > 0)
        {
            if (CheckAI())
            {
                /*for(int i = 0; i < UNO.Main.player[currentPlayer].GetComponent<AI>().cards.Count; i++)
                {
                    if(UNO.Main.player[currentPlayer].GetComponent<AI>().cards[i].cardType == Card.CardType.Draw_4)
                    {
                        ck = true;
                        UNO.Main.player[currentPlayer].GetComponent<AI>().InSteak(ck);
                        break;
                    }
                }
                if(!ck)
                {
                    for(int i = 0; i < drawSteak * 4; i++)
                    {
                        UNO.Main.player[currentPlayer].GetComponent<AI>().cards.Add(Cards.Main.RandomCards[^1]);
                        Cards.Main.RandomCards.RemoveAt(Cards.Main.RandomCards.Count - 1);
                    }
                    UNO.Main.player[currentPlayer].GetComponent<AI>().InSteak(ck);
                    CheckReverseClientRpc();
                    drawSteak = 0;
                }*/
            }
            else
            {
                for (int i = 0; i < UNO.GetCurrentPlayer().CardList.Count; i++)
                {
                    if (UNO.GetCurrentPlayer().CardList[i].cardType == Card.CardType.Draw_4)
                    {
                        ck = true;
                        UNO.GetCurrentPlayer().InSteak.Add(4, ck);
                        break;
                    }
                    if (UNO.GetCurrentPlayer().CardList[i].cardType == Card.CardType.Reverse)
                    {
                        if (UNO.GetCurrentPlayer().CardList[i].cardColor == card.cardColor && card.cardType == Card.CardType.Draw_4)
                        {
                            ck = true;
                            UNO.GetCurrentPlayer().InSteak.Add(4, ck);
                            break;
                        }
                        else if (card.cardType == Card.CardType.Reverse)
                        {
                            ck = true;
                            UNO.GetCurrentPlayer().InSteak.Add(4, ck);
                            break;
                        }
                    }
                }
                if (!ck)
                {
                    for (int i = 0; i < drawSteak4 * 4; i++)
                    {
                        UNO.GetCurrentPlayer().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
                        Cards.Main.CardRemoveServerRpc();
                    }
                    UNO.GetCurrentPlayer().InSteak.Clear();
                    UNO.PlayerList[CurrentPlayer].GetComponent<PlayerUI>().UpdateUI();
                    drawSteak4 = 0;
                    UI.Main.UpdateUICard();
                    CheckReverse();
                }
            }
        }
    }

    [ClientRpc]
    private void Draw2ClientRpc(int id)
    {
        Card card = Cards.GetCardById(id);
        bool ck = false;
        if (card.cardType == Card.CardType.Draw_2 || card.cardType == Card.CardType.Reverse && drawSteak2 > 0)
        {
            if (CheckAI())
            {
                /*for (int i = 0; i < UNO.Main.player[currentPlayer].GetComponent<AI>().cards.Count; i++)
                {
                    if(UNO.Main.player[currentPlayer].GetComponent<AI>().cards[i].cardType == Card.CardType.Draw_2)
                    {
                        ck = true;
                        UNO.Main.player[currentPlayer].GetComponent<AI>().InSteak(ck);
                        break;
                    }
                }
                if(!ck)
                {
                    for(int i = 0; i < drawSteak * 2; i++)
                    {
                        UNO.Main.player[currentPlayer].GetComponent<AI>().cards.Add(Cards.Main.RandomCards[^1]);
                        Cards.Main.RandomCards.RemoveAt(Cards.Main.RandomCards.Count - 1);
                    }
                    UNO.Main.player[currentPlayer].GetComponent<AI>().InSteak(ck);
                    CheckReverseClientRpc();
                    drawSteak = 0;
                }*/
            }
            else
            {
                for (int i = 0; i < UNO.GetCurrentPlayer().CardList.Count; i++)
                {
                    if (UNO.GetCurrentPlayer().CardList[i].cardType == Card.CardType.Draw_2)
                    {
                        ck = true;
                        UNO.GetCurrentPlayer().InSteak.Add(2, ck);
                        break;
                    }
                    if (UNO.GetCurrentPlayer().CardList[i].cardType == Card.CardType.Reverse)
                    {
                        if (UNO.GetCurrentPlayer().CardList[i].cardColor == card.cardColor && card.cardType == Card.CardType.Draw_2)
                        {
                            ck = true;
                            UNO.GetCurrentPlayer().InSteak.Add(2, ck);
                            break;
                        }
                        else if (card.cardType == Card.CardType.Reverse)
                        {
                            ck = true;
                            UNO.GetCurrentPlayer().InSteak.Add(2, ck);
                            break;
                        }
                    }
                }
                if (!ck)
                {
                    for (int i = 0; i < drawSteak2 * 2; i++)
                    {
                        UNO.GetCurrentPlayer().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
                        Cards.Main.CardRemoveServerRpc();
                    }
                    UNO.GetCurrentPlayer().InSteak.Clear();
                    UNO.PlayerList[CurrentPlayer].GetComponent<PlayerUI>().UpdateUI();
                    drawSteak2 = 0;
                    UI.Main.UpdateUICard();
                    CheckReverse();
                }
            }
        }
    }

    [ClientRpc]
    private void TurnDisableClientRpc()
    {
        if (CheckAI())
        {
            /*UNO.Main.player[currentPlayer].GetComponent<AI>().turn = false;*/
        }
        else
        {
            if (UNO.GetCurrentPlayer().InSteak.ContainsKey(2))
            {
                for (int i = 0; i < drawSteak2 * 2; i++)
                {
                    UNO.GetCurrentPlayer().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
                    Cards.RandomCards.Remove(Cards.RandomCards[^1]);
                }
                UNO.GetCurrentPlayer().InSteak.Clear();
                UNO.PlayerList[CurrentPlayer].GetComponent<PlayerUI>().UpdateUI();
                drawSteak2 = 0;
                UI.Main.UpdateUICard();
            }
            else if (UNO.GetCurrentPlayer().InSteak.ContainsKey(4))
            {
                for (int i = 0; i < drawSteak4 * 4; i++)
                {
                    UNO.GetCurrentPlayer().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
                    Cards.RandomCards.Remove(Cards.RandomCards[^1]);
                }
                UNO.GetCurrentPlayer().InSteak.Clear();
                UNO.PlayerList[CurrentPlayer].GetComponent<PlayerUI>().UpdateUI();
                drawSteak4 = 0;
                UI.Main.UpdateUICard();
            }
            UNO.GetCurrentPlayer().Turn = false;
            UNO.GetCurrentPlayer().skip.interactable = false;
        }
    }

    [ClientRpc]
    private void TurnEnableClientRpc()
    {
        if (CheckAI())
        {
            /*UNO.Main.player[currentPlayer].GetComponent<AI>().turn = true;*/
        }
        else
        {
            UNO.GetCurrentPlayer().Turn = true;
            Desk.Main.EnableDraw();
            UI.Main.UpdateUICard();
        }
    }

    private bool CheckAI()
    {
        return UNO.GetCurrentPlayer() == null;
    }

    private void Update()
    {
        if (!Lobby.Main.IsGameStarted.Value)
        {
            CurrentPlayer = 0;
            isReverse = false;
        }
    }
}