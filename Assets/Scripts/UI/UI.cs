using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject Table;
    [SerializeField] private GameObject PlayerSlot;
    [SerializeField] private Image CardBackImage;

    public static UI Main { get; private set; }
    public Player Player { get; private set; }
    public static readonly List<Vector3> posPlayerList = new()
    {
        new Vector3(600, 0, 0),
        new Vector3(400, 300, 0),
        new Vector3(0, 400, 0),
        new Vector3(-400, 300, 0),
        new Vector3(-600, 0, 0)
    };
    #endregion

    private void Awake()
    {
        Main = this;
    }

    public void MainPlayer(Player player)
    {
        Player = player;
    }

    public void Remove()
    {
        int index = Players.Main.PlayersList.IndexOf(Player);
        Players.Main.PlayersList.RemoveAt(index);
        List<Player> temp = new();
        for (int i = Players.Main.PlayersList.Count - 1; i > -1; i--)
        {
            if (i < index)
            {
                temp.Add(Players.Main.PlayersList[i]);
                Players.Main.PlayersList.RemoveAt(i);
            }
        }
        for (int i = temp.Count - 1; i > -1; i--)
        {
            Players.Main.PlayersList.Add(temp[i]);
        }
        temp.Clear();
    }

    public void UpdateUI()
    {
        ClearUpdateUI();
        for (int i = 0; i < Players.Main.PlayersList.Count; i++)
        {
            var card = Instantiate(PlayerSlot, Table.transform.position, Quaternion.identity, Table.transform);
            card.GetComponentInChildren<TextMeshProUGUI>().text = Players.Main.PlayersList[i].name;
            if (Players.Main.PlayersList.Count == 1)
            {
                card.GetComponent<RectTransform>().localPosition = posPlayerList[2];
            }
            else if (Players.Main.PlayersList.Count == 2)
            {
                if (i == 0)
                    card.GetComponent<RectTransform>().localPosition = posPlayerList[1];
                else
                    card.GetComponent<RectTransform>().localPosition = posPlayerList[3];
            }
            else if (Players.Main.PlayersList.Count == 3)
            {
                card.GetComponent<RectTransform>().localPosition = posPlayerList[i + 1];
            }
            else
            {
                card.GetComponent<RectTransform>().localPosition = posPlayerList[i];
            }
        }
    }

    public void ClearUpdateUI()
    {
        for (int i = 2; i < Table.transform.childCount; i++)
        {
            Destroy(Table.transform.GetChild(i).gameObject);
        }
    }

    public void UpdateUICard()
    {
        ClearUpdateUICard();
        foreach (Player p in Players.Main.PlayersList)
        {
            for (int i = 2; i < Table.transform.childCount; i++)
            {
                for (int j = 0; j < p.CardList.Count; j++)
                {
                    if (Table.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text == p.name)
                    {
                        var temp = Instantiate(CardBackImage, Table.transform.GetChild(i).GetChild(0).transform);
                        if (UNO.Main.IsMatchInProgress)
                            temp.gameObject.SetActive(true);
                        else
                            temp.gameObject.SetActive(false);
                        if (UNO.PlayerList[Game.CurrentPlayer].name == p.name)
                            temp.GetComponent<Image>().color = Color.white;
                        else
                            temp.GetComponent<Image>().color = Color.gray;
                    }
                }
            }
        }
    }

    private void ClearUpdateUICard()
    {
        for (int i = 2; i < Table.transform.childCount; i++)
        {
            for (int j = 0; j < Table.transform.GetChild(i).GetChild(0).childCount; j++)
            {
                Destroy(Table.transform.GetChild(i).GetChild(0).GetChild(j).gameObject);
            }
        }
    }
}