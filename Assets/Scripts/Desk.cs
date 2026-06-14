using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class Desk : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    public Button draw;

    public static Desk Main { get; private set; }
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        draw.interactable = false;
    }
    #endregion

    #region Draw Card
    public void Draw()
    {
        Rpc.Main.AddCardServerRpc();
        UNO.GetCurrentPlayer().skip.interactable = true;
    }
    
    public void EnableDraw()
    {
        if (!IsCurrentPlayerLocal() || Cards.RandomCards.Count < 1)
        {
            DisableDraw();
            return;
        }

        if (UNO.GetCurrentPlayer().InSteak.ContainsKey(2) || UNO.GetCurrentPlayer().InSteak.ContainsKey(4))
        {
            UNO.GetCurrentPlayer().skip.interactable = true;
        }
        else if(Cards.RandomCards.Count == 1)
        {
            Cards.Main.Shuffle();
        }
        else
        {
            draw.interactable = true;
        }
    }

    private bool IsCurrentPlayerLocal()
    {
        return UNO.GetCurrentPlayer().PlayerName.Value == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().PlayerName.Value;
    }

    public void DisableDraw() => draw.interactable = false;
    #endregion
}