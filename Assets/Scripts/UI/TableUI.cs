using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TableUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Image cardOnTable;

    public static TableUI Main { get; private set; }

    private void Awake()
    {
        Main = this;
    }

    public void UpdateUI(int id)
    {
        cardOnTable.sprite = GameSprites.GetCardImage(Cards.GetCardById(id));
        cardOnTable.gameObject.SetActive(true);
    }
}