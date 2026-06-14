using System.Collections.Generic;
using Unity.Netcode;

public class Table : NetworkBehaviour
{
    #region Attributes
    public List<int> CardList;
    public static Table Main { get; private set; }
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        Main = this;
        CardList = new();
    }
    #endregion

    #region Change Table Card
    public void UpdateCard(int id)
    {
        CardList.Add(id);
        TableUI.Main.UpdateUI(id);
    }
    #endregion
}