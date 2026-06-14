using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject Grid;

    [Header("Prefabs")]
    [SerializeField] private CardUI UI;

    private Player player;
    #endregion

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(IsOwner);
    }

    public void UpdateUI()
    {
        if (!IsOwner) return;
        ClearCards();
        for (int i = 0; i < player.CardList.Count; i++)
        {
            var cards = player.CardList[i];
            var card = Instantiate(UI, Grid.transform.position, Quaternion.identity, Grid.transform);
            UpdateSpacing();
            if (UNO.Main.Check)
            {
                card.gameObject.SetActive(true);
            }
            else
            {
                card.gameObject.SetActive(false);
            }
            card.GetComponent<Image>().color = Color.gray;
            card.UpdateUI(cards, player);
        }
    }

    private void UpdateSpacing()
    {
        if (Grid.transform.childCount > 5)
        {
            Grid.GetComponent<HorizontalLayoutGroup>().spacing = -1520;
            if (Grid.transform.childCount > 10)
            {
                Grid.GetComponent<HorizontalLayoutGroup>().spacing = -1320;
            }
            if (Grid.transform.childCount > 20)
            {
                Grid.GetComponent<HorizontalLayoutGroup>().spacing = -820;
            }
        }
    }

    private void ClearCards()
    {
        if (Grid.transform.childCount < 1)
        {
            return;
        }
        for (int i = 0; i < Grid.transform.childCount; i++)
        {
            Destroy(Grid.transform.GetChild(i).gameObject);
        }
    }
}