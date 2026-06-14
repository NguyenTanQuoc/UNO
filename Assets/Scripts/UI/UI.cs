using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject Grid;
    [SerializeField] private GameObject ui;
    [SerializeField] private Image sprite;

    public static UI Main { get; private set; }
    public Player Player { get; private set; }
    public static readonly List<Vector3> list = new()
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
            var card = Instantiate(ui, Grid.transform.position, Quaternion.identity, Grid.transform);
            card.GetComponentInChildren<TextMeshProUGUI>().text = Players.Main.PlayersList[i].name;
            if (Players.Main.PlayersList.Count == 1)
            {
                card.GetComponent<RectTransform>().localPosition = list[2];
            }
            else if (Players.Main.PlayersList.Count == 2)
            {
                if (i == 0)
                {
                    card.GetComponent<RectTransform>().localPosition = list[1];
                }
                else
                {
                    card.GetComponent<RectTransform>().localPosition = list[3];
                }
            }
            else if (Players.Main.PlayersList.Count == 3)
            {
                card.GetComponent<RectTransform>().localPosition = list[i + 1];
            }
            else
            {
                card.GetComponent<RectTransform>().localPosition = list[i];
            }
        }
    }

    public void ClearUpdateUI()
    {
        for (int i = 2; i < Grid.transform.childCount; i++)
        {
            Destroy(Grid.transform.GetChild(i).gameObject);
        }
    }

    public void UpdateUICard()
    {
        ClearUpdateUICard();
        foreach (Player p in Players.Main.PlayersList)
        {
            for (int i = 2; i < Grid.transform.childCount; i++)
            {
                for (int j = 0; j < p.CardList.Count; j++)
                {
                    if (Grid.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text == p.name)
                    {
                        var temp = Instantiate(sprite, Grid.transform.GetChild(i).GetChild(0).transform);
                        if (UNO.Main.Check)
                        {
                            temp.gameObject.SetActive(true);
                        }
                        else
                        {
                            temp.gameObject.SetActive(false);
                        }
                        if (UNO.PlayerList[Game.CurrentPlayer].name == p.name)
                        {
                            temp.GetComponent<Image>().color = Color.white;
                        }
                        else
                        {
                            temp.GetComponent<Image>().color = Color.gray;
                        }
                    }
                }
            }
        }
    }

    private void ClearUpdateUICard()
    {
        for (int i = 2; i < Grid.transform.childCount; i++)
        {
            for (int j = 0; j < Grid.transform.GetChild(i).GetChild(0).childCount; j++)
            {
                Destroy(Grid.transform.GetChild(i).GetChild(0).GetChild(j).gameObject);
            }
        }
    }
}