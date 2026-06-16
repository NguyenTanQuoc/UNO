using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class NetworkUI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button btn;
    [SerializeField] private GameObject failScene;
    [SerializeField] private GameObject lobbyScene;
    [SerializeField] private GameObject playerName;
    [SerializeField] private TextMeshProUGUI text;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        if (PlayerPrefs.HasKey("Player_Name"))
        {
            playerName.GetComponentInChildren<TMP_InputField>().text = PlayerPrefs.GetString("Player_Name");
            playerName.GetComponentInChildren<Button>().interactable = false;
        }
    }
    #endregion

    private void GetLocalIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if(ip.ToString().StartsWith("172.") || ip.ToString().StartsWith("10."))
                {
                    text.text = ip.ToString();
                }
            }
        }
    }

    #region Register Event
    private void Start()
    {
        NetworkManager.Singleton.OnServerStopped += HandleStopped;
        NetworkManager.Singleton.OnClientStopped += HandleStopped;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
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
        Debug.Log(NetworkManager.Singleton.DisconnectReason);
    }

    public void DisconnectGame()
    {
        NetworkManager.Singleton.Shutdown();
        UnRegisterEvent();
        SceneManager.LoadScene(0);
    }
    #endregion

    #region Set IP To Connect To Server
    public void SetIpAddress(string input)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = input;
    }
    #endregion

    #region Start Server
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        GetLocalIP();
        text.gameObject.SetActive(true);
    }
    #endregion

    #region Client Join Game
    private IEnumerator TryConnect()
    {
        float waitTime = 0;
        while (!NetworkManager.Singleton.IsConnectedClient && waitTime < 5)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            failScene.SetActive(true);
            failScene.transform.GetChild(0).gameObject.SetActive(true);
            NetworkManager.Singleton.Shutdown();
        }
    }

    public void StartClient()
    {
        SetIpAddress(input.text);
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 6)
        {
            NetworkManager.Singleton.StartClient();
            StartCoroutine(TryConnect());
        }
        else
        {
            failScene.SetActive(true);
            failScene.transform.GetChild(0).gameObject.SetActive(true);
            failScene.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Server Is Full";
            NetworkManager.Singleton.Shutdown();
        }
    }
    #endregion

    #region Enable Button When Input IP
    public void InputIp()
    {
        if (string.IsNullOrEmpty(input.text))
        {
            btn.interactable = false;
        }
        else
        {
            btn.interactable = true;
        }
    }
    #endregion

    #region Change Player Name
    public void ChangeInputName()
    {
        var button = playerName.GetComponentInChildren<Button>();
        if (string.IsNullOrEmpty(playerName.GetComponentInChildren<TMP_InputField>().text) 
            || playerName.GetComponentInChildren<TMP_InputField>().text.Length > 15)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
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

    #region Unregister Event
    private void UnRegisterEvent()
    {
        NetworkManager.Singleton.OnServerStopped -= HandleStopped;
        NetworkManager.Singleton.OnClientStopped -= HandleStopped;
        NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }
    #endregion
}