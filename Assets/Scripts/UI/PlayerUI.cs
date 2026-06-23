using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private GameObject Hand;

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
        if (!IsOwner) 
            return;
        ClearCards();
        for (int i = 0; i < player.CardList.Count; i++)
        {
            var cards = player.CardList[i];
            var card = Instantiate(UI, Hand.transform.position, Quaternion.identity, Hand.transform);
            UpdateSpacing();
            if (UNO.Main.IsMatchInProgress)
                card.gameObject.SetActive(true);
            else
                card.gameObject.SetActive(false);
            card.GetComponent<Image>().color = Color.gray;
            card.UpdateUI(cards, player);
        }
    }

    private void UpdateSpacing()
    {
        if (Hand.transform.childCount > 5)
        {
            Hand.GetComponent<HorizontalLayoutGroup>().spacing = -1520;
            if (Hand.transform.childCount > 10)
                Hand.GetComponent<HorizontalLayoutGroup>().spacing = -1320;
            if (Hand.transform.childCount > 20)
                Hand.GetComponent<HorizontalLayoutGroup>().spacing = -820;
        }
    }

    private void ClearCards()
    {
        if (Hand.transform.childCount < 1)
            return;
        for (int i = 0; i < Hand.transform.childCount; i++)
        {
            Destroy(Hand.transform.GetChild(i).gameObject);
        }
    }
}