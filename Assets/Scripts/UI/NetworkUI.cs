using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button btn;
    [SerializeField] private GameObject failScene;
    [SerializeField] private GameObject lobbyScene;
    [SerializeField] private GameObject joinScene;
    [SerializeField] private GameObject playerName;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject roomListGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject roomDisplay;

    private ISession session;
    #endregion

    #region Call When Game Start
    private async void Awake()
    {
        if (PlayerPrefs.HasKey("Player_Name"))
        {
            playerName.GetComponentInChildren<TMP_InputField>().text = PlayerPrefs.GetString("Player_Name");
            playerName.GetComponentInChildren<Button>().interactable = false;
        }
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error Create UGS: {e.Message}");
        }
    }
    #endregion

    #region Event Register & UnRegister
    private void Start()
    {
        NetworkManager.Singleton.OnServerStopped += HandleStopped;
        NetworkManager.Singleton.OnClientStopped += HandleStopped;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void UnRegisterEvent()
    {
        NetworkManager.Singleton.OnServerStopped -= HandleStopped;
        NetworkManager.Singleton.OnClientStopped -= HandleStopped;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
    #endregion

    #region Call When Client Disconnected
    private void HandleClientDisconnected(ulong id)
    {
        Rpc.Main.UpdateGameStateClientRpc(id);
    }
    #endregion

    #region Call When Client Connected
    private void HandleClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            lobbyScene.SetActive(true);
            StartCoroutine(Delay());
        }
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        Rpc.Main.ResetDisplayServerRpc();
    }
    #endregion

    #region Approve Client Connect Request
    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = !Lobby.Main.IsGameStarted.Value;
        response.CreatePlayerObject = response.Approved;
    }
    #endregion

    #region Call When Server Or Client Stopped
    private void HandleStopped(bool ck)
    {
        if (IsHost)
        {
            DisconnectGame();
            return;
        }
        failScene.SetActive(true);
        failScene.transform.GetChild(1).gameObject.SetActive(true);
    }

    public async void DisconnectGame()
    {
        try
        {
            if (session != null)
            {
                await session.LeaveAsync();
                UnRegisterEvent();
                session = null;
                SceneManager.LoadScene(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    #endregion

    #region Create or Join Game
    public async void StartHost()
    {
        try
        {
            session = await MultiplayerService.Instance.CreateSessionAsync(
                new SessionOptions()
                {
                    MaxPlayers = 8,
                    Name = PlayerPrefs.GetString("Player_Name") + "'s Room",
                }.WithRelayNetwork()
            );
            text.text = $"Code: {session.Code}";
            text.gameObject.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void StartClient()
    {
        string code = input.text.Trim();
        if (string.IsNullOrEmpty(code))
            return;
        try
        {
            session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region Enable Button When Room Code
    public void InputCode()
    {
        if (string.IsNullOrEmpty(input.text))
            btn.interactable = false;
        else
            btn.interactable = true;
    }
    #endregion

    #region Change Player Name
    public void ChangeInputName()
    {
        var button = playerName.GetComponentInChildren<Button>();
        var inputField = playerName.GetComponentInChildren<TMP_InputField>();
        if (string.IsNullOrEmpty(inputField.text) || inputField.text.Length > 15)
            button.interactable = false;
        else
            button.interactable = true;
    }

    public void SavePlayerName()
    {
        string name = playerName.GetComponentInChildren<TMP_InputField>().text;
        PlayerPrefs.SetString("Player_Name", name);
        playerName.GetComponentInChildren<Button>().interactable = false;
    }
    #endregion

    #region Exit Game
    public void ExitGame()
    {
        Application.Quit();
    }
    #endregion

    public async void RefreshSessionList()
    {
        try
        {
            QuerySessionsOptions queryOptions = new()
            {
                Count = 20
            };
            QuerySessionsResults queryResults = await MultiplayerService.Instance.QuerySessionsAsync(queryOptions);
            Clear();
            foreach (ISessionInfo session in queryResults.Sessions)
            {
                var d = Instantiate(roomDisplay, transform.position, Quaternion.identity, roomListGrid.transform);
                d.GetComponent<Button>().onClick.AddListener(() => OnJoinSession(session.Id));
                d.GetComponentInChildren<TextMeshProUGUI>().text = session.Name;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private void Clear()
    {
        if (roomListGrid.transform.childCount < 1)
            return;
        for (int i = 0; i < roomListGrid.transform.childCount; i++)
        {
            Destroy(roomListGrid.transform.GetChild(i).gameObject);
        }
    }

    public async void OnJoinSession(string sessionId)
    {
        joinScene.SetActive(false);
        try
        {
            session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            joinScene.SetActive(true);
        }
    }
}