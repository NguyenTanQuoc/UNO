using System.Collections;
using System.Linq;
using TMPro;
using Unity.Collections;
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
        // Lấy đúng người chơi vừa đánh bài
        var actionPlayer = UNO.PlayerList[playerIndex].GetComponent<Player>();

        // Xóa bài & cập nhật giao diện
        actionPlayer.CardList.Remove(Cards.GetCardById(cardId));
        actionPlayer.InSteak.Clear();
        UNO.PlayerList[playerIndex].GetComponent<PlayerUI>().UpdateUI();
        UI.Main.UpdateUICard();

        // ---- CHỖ ÔNG TÌM NẰM Ở ĐÂY NÈ ----
        // Kiểm tra tay bài của người vừa đánh
        if (actionPlayer.CardList.Count < 1)
        {
            // CHỈ SERVER mới có quyền tuyên bố kết thúc game để tránh loạn
            if (IsServer)
            {
                // 1. Lấy tên người chiến thắng
                string winnerName = UNO.PlayerList[playerIndex].name;

                // 2. Kêu tất cả các máy hiện Message Box lên
                ShowWinnerClientRpc(winnerName);

                // 3. Server bắt đầu bấm giờ 3 giây
                StartCoroutine(Delay());
            }
        }
    }

    private IEnumerator Delay()
    {
        // Đánh dấu game đã kết thúc
        Lobby.Main.IsGameStarted.Value = false;

        // Chờ 3 giây
        yield return new WaitForSeconds(3);

        // Ẩn Message Box đi
        end.SetActive(false);

        // Gọi lệnh ClientRpc ở file Lobby để lôi TẤT CẢ cùng về Lobby
        Lobby.Main.TriggerRestartClientRpc();
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

    #region ShowWinner Client
    [ClientRpc]
    public void ShowWinnerClientRpc(FixedString32Bytes winnerName)
    {
        // Ẩn UI vòng sáng/thông tin của các người chơi
        foreach (GameObject g in UNO.PlayerList)
        {
            g.transform.GetChild(0).gameObject.SetActive(false);
        }

        // Ẩn màn chơi, bật màn hình Message Box
        gameScenes.SetActive(false);
        end.SetActive(true);

        end.GetComponentInChildren<TextMeshProUGUI>().text = winnerName.ToString() + " Win Game";
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