using System.Collections.Generic;
using Unity.Netcode;

public class Players : NetworkBehaviour
{
    #region Attributes
    public List<Player> PlayersList { get; set; }
    public static Players Main { get; private set; }
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        PlayersList = new List<Player>();
        Main = this;
    }
    #endregion
}