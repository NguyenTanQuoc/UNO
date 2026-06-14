using UnityEngine;

public class AI : MonoBehaviour
{
    /*public List<Card> cards;
    public bool turn = false;
    private bool inSteak = false;
    private List<Card> ramdom;

    private void Update()
    {
        if(turn)
        {
            RandomCard();
        }
    }

    public void InSteak(bool ck)
    {
        inSteak = ck;
    }

    public void RandomCard()
    {
        if(inSteak)
        {
            ramdom = new List<Card>();
            for(int i = 0; i < cards.Count; i++)
            {
                if(Rules.Main.Rule(cards[i]))
                {
                    if(cards[i].cardType == Card.CardType.Draw_2 || cards[i].cardType == Card.CardType.Draw_4)
                    {
                        ramdom.Add(cards[i]);
                    }
                }
            }
            int ngaunhien = Random.Range(0, ramdom.Count);
            for(int i = 0; i < cards.Count; i++)
            {
                if(cards[i] == ramdom[ngaunhien])
                {
                    Table.Main.card.Value = cards[i];
                    turn = false;
                    Game.Main.NextTurn(cards[i].cardType);
                    inSteak = false;
                    cards.Remove(cards[i]);
                    TableUI.Main.UpdateUI(cards[i]);
                    break;
                }
            }
        }
        else
        {
            ramdom = new List<Card>();
            for(int i = 0; i < cards.Count; i++)
            {
                if(Rules.Main.Rule(cards[i]))
                {
                    ramdom.Add(cards[i]);
                }
            }
            if(ramdom.Count > 0)
            {
                int ngaunhien = Random.Range(0, ramdom.Count);
                for(int i = 0; i < cards.Count; i++)
                {
                    if(cards[i] == ramdom[ngaunhien])
                    {
                        Table.Main.card.Value = cards[i];
                        if(cards[i].cardType != Card.CardType.Number)
                        {
                            turn = false;
                            Game.Main.NextTurn(cards[i].cardType);
                        }
                        else
                        {
                            turn = false;
                            Game.Main.NextTurnServerRpc(cards[i]);
                        }
                        cards.Remove(cards[i]);
                        TableUI.Main.UpdateUI(cards[i]);
                        break;
                    }
                }
            }
            else
            {
                if(Cards.Main.RandomCards.Count > 0)
                {
                    cards.Add(Cards.Main.RandomCards[^1]);
                    Cards.Main.RandomCards.RemoveAt(Cards.Main.RandomCards.Count - 1);
                    if(Rules.Main.Rule(cards[^1]))
                    {
                        Table.Main.card.Value = cards[^1];
                        if(cards[^1].cardType != Card.CardType.Number)
                        {
                            turn = false;
                            Game.Main.NextTurn(cards[^1].cardType);
                        }
                        else
                        {
                            turn = false;
                            Game.Main.NextTurnServerRpc(cards[^1]);
                        }
                        cards.Remove(cards[^1]);
                        TableUI.Main.UpdateUI(cards[^1]);
                    }
                    else
                    {
                        turn = false;
                        Game.Main.NextTurnServerRpc(card[]);
                    }
                }
                else
                {
                    turn = false;
                    Game.Main.NextTurnServerRpc();
                }
            }
        }
    }*/
}