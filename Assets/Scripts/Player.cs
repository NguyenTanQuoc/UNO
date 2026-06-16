using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    public Button skip;

    public NetworkVariable<ulong> ID { get; private set; }
    public List<Card> CardList { get; set; }
    public bool Turn { get; set; }
    public Dictionary<int, bool> InSteak { get; set; }
    public static float TimeInTurn { get; set; }
    public NetworkVariable<bool> IsUNO;
    public NetworkVariable<FixedString128Bytes> PlayerName;
    #endregion

    #region Run When Game Start
    private void Awake()
    {
        CardList = new List<Card>();
        Turn = false;
        InSteak = new Dictionary<int, bool>();
        skip.interactable = false;
        TimeInTurn = 0;
        IsUNO = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        ID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        PlayerName = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        RegisterEvent();
    }

    private void RegisterEvent()
    {
        IsUNO.OnValueChanged += (oldBool, newBool) =>
        {
            oldBool = newBool;
        };
        ID.OnValueChanged += (oldID, newID) =>
        {
            oldID = newID;
        };
        PlayerName.OnValueChanged += (oldName, newName) =>
        {
            name = newName.Value;
        };
    }
    #endregion

    #region Run When NetworkObject Spawn
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            ID.Value = OwnerClientId;
            PlayerName.Value = PlayerPrefs.GetString("Player_Name");
            UI.Main.MainPlayer(GetComponent<Player>());
            Lobby.Main.PlayersList.Add(GetComponent<Player>());
            return;
        }
        name = PlayerName.Value.ToString();
        Lobby.Main.PlayersList.Add(GetComponent<Player>());
    }
    #endregion

    #region Skip Turn
    public void Skip()
    {
        Game.Main.NextTurnServerRpc();
        Desk.Main.DisableDraw();
    }
    #endregion

    #region Clock
    private void UpdateTime()
    {
        TimeInTurn -= Time.deltaTime;
        if (TimeInTurn <= 0 && Turn)
        {
            if (Game.Main.chooseColor.activeSelf)
            {
                Game.Main.chooseColor.SetActive(false);
                Game.Main.ChooseColorServerRpc(Random.Range(0, 4));
            }
            else
            {
                if (Desk.Main.draw.interactable && Cards.RandomCards.Count >= 1 && !InSteak.ContainsKey(2) && !InSteak.ContainsKey(4))
                {
                    Rpc.Main.AddCardServerRpc();
                }
                if (Cards.RandomCards.Count == 1)
                {
                    Cards.Main.Shuffle();
                }
                else
                {
                    Skip();
                }
            }
        }
        Game.Main.clock.fillAmount = TimeInTurn / 20f;
        if (TimeInTurn >= 0 && TimeInTurn <= 20)
        {
            Game.Main.clock.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = Mathf.RoundToInt(TimeInTurn).ToString();
        }
    }

    private void Update()
    {
        if (UNO.Main.Check && Lobby.Main.IsGameStarted.Value)
        {
            UpdateTime();
            if (CardList.Count == 1)
            {
                IsUNO.Value = true;
            }
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            foreach(var id in Cards.RandomCards)
            {
                Debug.Log(id);
            }
        }
    }
    #endregion
}