using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class UNO : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject grid;
    [SerializeField] private GameObject cardDeal;
    public bool Check { get; set; }
    public static List<GameObject> PlayerList { get; set; }
    public static UNO Main { get; private set; }
    private readonly List<GameObject> Temp = new();
    public Coroutine GameCoroutine { get; set; }

    #region Call When Game Start
    private void Awake()
    {
        PlayerList = new List<GameObject>();
        Main = this;
        Check = false;
        GameCoroutine = null;
    }
    #endregion

    public void StopGame()
    {
        if (GameCoroutine != null)
        {
            StopCoroutine(GameCoroutine);
            GameCoroutine = null;
        }
    }

    public void StartGame()
    {
        GameCoroutine ??= StartCoroutine(Delay());
    }

    public IEnumerator Delay()
    {
        for (int i = 0; i < 7; i++)
        {
            foreach (GameObject p in PlayerList)
            {
                CheckAI(p);
                CardDeal(p);
                Cards.RandomCards.Remove(Cards.RandomCards[^1]);
                yield return new WaitForSeconds(0.5f);
            }
        }
        for (int i = Cards.RandomCards.Count - 1; i >= 0; i--)
        {
            if (Cards.GetCardById(Cards.RandomCards[i]).cardType == Card.CardType.Number)
            {
                Table.Main.UpdateCard(Cards.RandomCards[i]);
                Cards.RandomCards.Remove(Cards.RandomCards[i]);
                break;
            }
        }
        GetCurrentPlayer().Turn = true;
        Player.TimeInTurn = 20f;
        Check = true;
        DeleteCardDeal();
        UI.Main.UpdateUICard();
        foreach (GameObject p in PlayerList)
        {
            if (p.activeSelf)
            {
                p.GetComponent<PlayerUI>().UpdateUI();
            }
        }
        Desk.Main.EnableDraw();
        GameCoroutine = null;
    }

    public void DeleteCardDeal()
    {
        foreach (GameObject g in Temp)
        {
            Destroy(g);
        }
    }

    private void CardDeal(GameObject p)
    {
        var card = Instantiate(cardDeal, grid.transform.position, Quaternion.identity, grid.transform.parent.transform);
        Temp.Add(card);
        if (Players.Main.PlayersList.Contains(p.GetComponent<Player>()))
        {
            int index = Players.Main.PlayersList.IndexOf(p.GetComponent<Player>());
            Vector3 vector = grid.transform.GetChild(index + 2).GetComponent<RectTransform>().position;
            ITween.MoveTo(card, new Vector3(vector.x, vector.y - 8, 0), 0.25f);
            ITween.ScaleTo(card, new Vector3(0.5f, 0.5f, 0.5f), 0.25f);
        }
        if (p.name == UI.Main.Player.name)
        {
            Vector3 vector = p.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().position;
            ITween.MoveTo(card, new Vector3(vector.x, vector.y + 30, 0), 0.25f);
        }
    }

    private void CheckAI(GameObject p)
    {
        if (p.GetComponent<Player>() != null)
        {
            p.GetComponent<Player>().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
            UI.Main.UpdateUICard();
            if (p.activeSelf)
            {
                p.GetComponent<PlayerUI>().UpdateUI();
            }
        }
        else
        {
            /*p.GetComponent<AI>().cards.Add(Cards.Main.RandomCards[^1]);*/
        }
    }

    public static Player GetCurrentPlayer()
    {
        return PlayerList[Game.CurrentPlayer].GetComponent<Player>();
    }
}