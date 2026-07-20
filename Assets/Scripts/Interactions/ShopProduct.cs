using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class ShopProduct : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMesh label;

    private Player player;
    private GameInputReader input;
    private RoomShopState shopState;
    private ShopProductType productType;
    private int slotIndex;
    private int price;
    private bool playerInRange;
    private bool purchased;

    public void Configure(SpriteRenderer productVisual, TextMesh productLabel)
    {
        visual = productVisual;
        label = productLabel;
    }

    public void Initialize(Player playerReference, RoomShopState roomShop, int index, ShopProductType type)
    {
        player = playerReference;
        input = player.Input;
        shopState = roomShop;
        slotIndex = index;
        productType = type;
        price = ShopProductCatalog.GetPrice(type);
        if (visual != null) visual.color = ShopProductCatalog.GetColor(type);
        if (label != null) label.text = $"{ShopProductCatalog.GetDisplayName(type)}\n{price} COINS\nF BUY";
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || !playerInRange || purchased) return;
        if (input.InteractPressedThisFrame) TryPurchase();
    }

    public bool TryPurchase()
    {
        if (purchased || player == null || !ShopProductCatalog.CanApply(player, productType)) return false;
        if (!player.Inventory.SpendCoins(price)) return false;

        purchased = true;
        ShopProductCatalog.Apply(player, productType);
        shopState.MarkSlotPurchased(slotIndex);
        Destroy(gameObject);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == player) playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == player) playerInRange = false;
    }
}
