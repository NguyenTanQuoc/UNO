using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System.Collections;

public class Cards : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private Card[] cardList;

    public static Cards Main { get; private set; }
    private static Card[] cards;
    public static NetworkList<int> RandomCards = new();
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        RandomCards = new();
        cards = cardList;
    }
    #endregion

    #region Shuffle Cards When Start
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ShuffleServerRpc()
    {
        if (!IsServer) return;
        RandomCards.Clear();
        var cardsList = new List<Card>();
        for (int i = 0; i < 107; i++)
        {
            cardsList.Add(cards[i]);
        }
        int length = cardsList.Count;
        for (int i = 0; i < length; i++)
        {
            int random = Random.Range(0, cardsList.Count);
            RandomCards.Add(cardsList[random].Id);
            cardsList.RemoveAt(random);
        }
    }
    #endregion

    #region GetCardById Card Id
    public static Card GetCardById(int id)
    {
        return cards.FirstOrDefault(card => card.Id == id);
    }
    #endregion

    #region Shuffle Cards When Desk Empty
    public void Shuffle()
    {
        ShuffleDeskServerRpc();
        DelayGameServerRpc();
        StartCoroutine(DelayTurn());
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ShuffleDeskServerRpc()
    {
        if (!IsServer) return;
        RandomCards.Clear();
        var cardsList = new List<Card>();
        for (int i = Table.Main.CardList.Count - 2; i >= 0; i--)
        {
            if (Table.Main.CardList[i] > 107)
            {
                RemoveTableCardClientRpc(i);
            }
            cardsList.Add(GetCardById(Table.Main.CardList[i]));
            RemoveTableCardClientRpc(i);
        }
        for (int i = RandomCards.Count - 1; i >= 0; i--)
        {
            cardsList.Add(GetCardById(RandomCards[i]));
            RandomCards.RemoveAt(i);
        }
        int cardLength = cardsList.Count;
        for (int i = 0; i < cardLength; i++)
        {
            int random = Random.Range(0, cardsList.Count);
            RandomCards.Add(cardsList[random].Id);
            cardsList.RemoveAt(random);
        }
    }

    [ClientRpc]
    public void RemoveTableCardClientRpc(int i)
    {
        Table.Main.CardList.RemoveAt(i);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DelayGameServerRpc()
    {
        DelayGameClientRpc();
    }

    [ClientRpc]
    private void DelayGameClientRpc()
    {
        UNO.GetCurrentPlayer().Turn = false;
        Desk.Main.DisableDraw();
        Player.TimeInTurn = 0;
    }

    private IEnumerator DelayTurn()
    {
        yield return new WaitForSeconds(1.5f);
        Game.Main.ContinueServerRpc(Table.Main.CardList[^1]);
    }
    #endregion

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void CardRemoveServerRpc()
    {
        RandomCards.Remove(RandomCards[^1]);
    }
}