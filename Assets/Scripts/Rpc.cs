using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Rpc : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject end;
    public GameObject gameScenes;

    public static Rpc Main { get; private set; }
    private bool check;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        check = false;
    }
    #endregion

    #region Call When Someone Disconnect
    [ClientRpc]
    public void UpdateGameStateClientRpc(ulong clientId)
    {
        if (!UNO.Main.Check && Lobby.Main.IsGameStarted.Value)
        {
            var client = UNO.PlayerList.FirstOrDefault(p => p.GetComponent<Player>().ID.Value == clientId);
            if (client != null)
            {
                UNO.Main.StopGame();
                Player player = client.GetComponent<Player>();
                UNO.PlayerList.Remove(player.gameObject);
                Players.Main.PlayersList.Remove(player);
                UI.Main.UpdateUI();
                UI.Main.UpdateUICard();
                UNO.Main.DeleteCardDeal();
                gameScenes.SetActive(false);
                if (IsServer)
                {
                    Lobby.Main.IsGameStarted.Value = false;
                }
                Lobby.Main.Restart();
            }
        }
        if (!UNO.Main.Check && !Lobby.Main.IsGameStarted.Value)
        {
            Lobby.Main.PlayersList.Remove(Lobby.Main.PlayersList.FirstOrDefault(p => p.ID.Value == clientId));
            Main.ResetDisplayServerRpc();
        }
        else if (UNO.Main.Check)
        {
            var client = UNO.PlayerList.FirstOrDefault(p => p.GetComponent<Player>().ID.Value == clientId);
            if (client != null)
            {
                Player player = client.GetComponent<Player>();
                UNO.PlayerList.Remove(player.gameObject);
                Players.Main.PlayersList.Remove(player);
                UI.Main.UpdateUI();
                UI.Main.UpdateUICard();
                if (UNO.PlayerList.Count == 1)
                {
                    gameScenes.SetActive(false);
                    if (IsServer)
                    {
                        Lobby.Main.IsGameStarted.Value = false;
                    }
                    Lobby.Main.Restart();
                    UNO.Main.Check = false;
                }
            }
        }

    }
    #endregion

    #region Delete Card On Player Hand


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeleteCardServerRpc(int cardId, int playerIndex)
    {

        DeleteCardClientRpc(cardId, playerIndex);
    }


    [ClientRpc]
    public void DeleteCardClientRpc(int cardId, int playerIndex)
    {
  
        var actionPlayer = UNO.PlayerList[playerIndex].GetComponent<Player>(); 

        // 2. XÓA BÀI & CẬP NHẬT UI
        actionPlayer.CardList.Remove(Cards.GetCardById(cardId));
        actionPlayer.InSteak.Clear(); // Xóa lịch sử/trạng thái bài nếu cần

        // Cập nhật lại giao diện của người chơi đó và giao diện bài trên bàn
        UNO.PlayerList[playerIndex].GetComponent<PlayerUI>().UpdateUI();
        UI.Main.UpdateUICard();


        if (actionPlayer.CardList.Count < 1)
        {
            // Nếu người này hết bài, gọi hàm kết thúc game và truyền tên người thắng vào
            TriggerEndGame(UNO.PlayerList[playerIndex].name);
        }
    }


    private void TriggerEndGame(string winnerName)
    {
        // Ẩn UI của tất cả người chơi
        foreach (GameObject g in UNO.PlayerList)
        {
            g.transform.GetChild(0).gameObject.SetActive(false);
        }

        // Ẩn màn hình chơi game và hiện màn hình kết quả (End)
        gameScenes.SetActive(false);
        end.SetActive(true);

        // Ghi tên người chiến thắng lên Message Box
        end.GetComponentInChildren<TextMeshProUGUI>().text = winnerName + " Win Game";

        // Bắt đầu đếm ngược để khởi động lại ván
        StartCoroutine(Delay());
    }

    // Hàm đếm ngược delay (ĐÃ SỬA LẠI ĐỂ SỬA LỖI KẸT LOBBY)
    private IEnumerator Delay()
    {
        // 1. CHỈ DUY NHẤT SERVER mới được quyền đếm giờ và điều phối chuyển cảnh
        if (IsServer)
        {
            Lobby.Main.IsGameStarted.Value = false;

            // 2. Tạm dừng 3 giây trên Server
            yield return new WaitForSeconds(3);


            end.SetActive(false);

            Lobby.Main.TriggerRestartClientRpc();
        }
    }
    #endregion
    #region Reset Lobby
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ResetDisplayServerRpc()
    {
        ResetDisplayClientRpc();
    }

    [ClientRpc]
    private void ResetDisplayClientRpc()
    {
        Lobby.Main.DisplayPlayerUI();
    }
    #endregion

    #region Change Card On Table
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ChangeTableCardServerRpc(int id)
    {
        ChangeTableCardClientRpc(id);
    }

    [ClientRpc]
    public void ChangeTableCardClientRpc(int id)
    {
        Table.Main.UpdateCard(id);
    }
    #endregion

    #region Call When Player Draw 1 Card
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddCardServerRpc()
    {
        AddCardClientRpc();
        Cards.RandomCards.Remove(Cards.RandomCards[^1]);
        check = false;
        if (UNO.GetCurrentPlayer().Turn)
        {
            if (Desk.Main.draw.interactable == false)
            {
                foreach (var card in UNO.GetCurrentPlayer().CardList)
                {
                    if (Rules.Main.Rule(card))
                    {
                        check = true;
                    }
                }
                if (!check && Player.TimeInTurn > 0)
                {
                    UNO.GetCurrentPlayer().Skip();
                    check = false;
                }
            }
        }
    }

    [ClientRpc]
    public void AddCardClientRpc()
    {
        UNO.GetCurrentPlayer().CardList.Add(Cards.GetCardById(Cards.RandomCards[^1]));
        UNO.PlayerList[Game.CurrentPlayer].GetComponent<PlayerUI>().UpdateUI();
    }
    #endregion

    #region Reset Time Clock
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ResetTimeClockServerRpc()
    {
        ResetTimeClockClientRpc();
    }

    [ClientRpc]
    public void ResetTimeClockClientRpc()
    {
        Player.TimeInTurn = 21.5f;
    }
    #endregion
}