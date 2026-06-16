using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject gameScene;
    [SerializeField] private Button start;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject playListGrid;
    [SerializeField] private GameObject lobbyScene;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerDisplay;

    public NetworkVariable<bool> IsGameStarted { get; private set; }
    public static Lobby Main { get; private set; }
    public List<Player> PlayersList;
    private float count = 3;
    private bool check;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        PlayersList = new List<Player>();
        IsGameStarted = new(false);
        check = false;
    }
    #endregion

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            start.gameObject.SetActive(true);
            return;
        }
        text.gameObject.SetActive(true);
    }

    public void Restart()
    {
        GameObject temp;
        if (UNO.Main.Check)
        {
            for (int i = 0; i < UNO.PlayerList.Count - 1; i++)
            {
                for (int j = i + 1; j < UNO.PlayerList.Count; j++)
                {
                    if (UNO.PlayerList[i].GetComponent<Player>().CardList.Count > UNO.PlayerList[j].GetComponent<Player>().CardList.Count)
                    {
                        temp = UNO.PlayerList[i];
                        UNO.PlayerList[i] = UNO.PlayerList[j];
                        UNO.PlayerList[j] = temp;
                    }
                }
            }
            UNO.Main.Check = false;
        }
        foreach (GameObject player in UNO.PlayerList)
        {
            PlayersList.Add(player.GetComponent<Player>());
            player.GetComponent<Player>().CardList.Clear();
            player.GetComponent<Player>().Turn = false;
            player.GetComponent<PlayerUI>().UpdateUI();
            if (player.name == NetworkManager.Singleton.LocalClient.PlayerObject.name)
            {
                player.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
        Desk.Main.draw.interactable = false;
        Table.Main.CardList.Clear();
        UNO.PlayerList.Clear();
        Players.Main.PlayersList.Clear();
        lobbyScene.SetActive(true);
        lobbyScene.transform.GetChild(0).gameObject.SetActive(true);
        start.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        DisplayPlayerUI();
        UI.Main.ClearUpdateUI();
        if (PlayersList[0].name == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().PlayerName.Value.ToString())
        {
            start.gameObject.SetActive(true);
            return;
        }
        text.gameObject.SetActive(true);
    }

    public void DisplayPlayerUI()
    {
        Clear();
        if (PlayersList.Count <= 1)
        {
            start.interactable = false;
        }
        foreach (Player p in PlayersList)
        {
            var d = Instantiate(playerDisplay, transform.position, Quaternion.identity, playListGrid.transform);
            d.GetComponentInChildren<TextMeshProUGUI>().text = p.name;
        }
        if (NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
        {
            start.interactable = true;
        }
    }

    private void Clear()
    {
        if (playListGrid.transform.childCount < 1)
        {
            return;
        }
        for (int i = 0; i < playListGrid.transform.childCount; i++)
        {
            Destroy(playListGrid.transform.GetChild(i).gameObject);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void StartGameServerRpc()
    {
        Cards.Main.ShuffleServerRpc();
        if (IsServer)
        {
            IsGameStarted.Value = true;
        }
        IsStartClientRpc();
        StartCoroutine(Delay());
    }

    [ClientRpc]
    private void IsStartClientRpc()
    {
        lobbyScene.transform.GetChild(0).gameObject.SetActive(false);
        lobbyScene.transform.GetChild(1).gameObject.SetActive(true);
        check = true;
    }

    private void Update()
    {
        if (check)
        {
            count -= Time.deltaTime;
            if (count <= 0)
            {
                count = 3;
                lobbyScene.transform.GetChild(1).gameObject.SetActive(false);
                check = false;
            }
            lobbyScene.transform.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(count).ToString();
        }
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(3);
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        foreach (Player p in PlayersList)
        {
            Players.Main.PlayersList.Add(p);
            UNO.PlayerList.Add(p.gameObject);
            p.transform.GetChild(0).gameObject.SetActive(true);
        }
        lobbyScene.SetActive(false);
        gameScene.SetActive(true);
        PlayersList.Clear();
        UI.Main.Remove();
        UI.Main.UpdateUI();
        UNO.Main.StartGame();
    }
}