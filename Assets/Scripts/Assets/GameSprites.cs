using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameSprites : NetworkBehaviour
{
    #region Attributes
    [Header("References")]
    [SerializeField] private List<Sprite> sprites;

    private static List<Sprite> spritesList;
    #endregion

    #region Call When Game Start
    private void Awake()
    {
        spritesList = sprites;
    }
    #endregion

    #region GetCardById Sprite
    public static Sprite GetCardImage(Card card)
    {
        return spritesList.FirstOrDefault(sprite =>
            sprite.name.Equals(card.cardColor + " " + card.cardNumber) ||
            sprite.name.Equals(card.cardColor + "_" + card.cardType) ||
            sprite.name.Equals(card.cardType.ToString())
        );
    }
    #endregion
}