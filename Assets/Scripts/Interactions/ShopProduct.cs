using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public sealed class ShopProduct : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private TextMesh label;

    private Player player;
    private IPlayerInput input;
    private RoomNode roomNode;
    private ShopProductDefinition productDefinition;
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

    public void Initialize(Player playerReference, RoomNode shopRoom, int index, ShopProductType type)
    {
        player = playerReference;
        input = player.Input;
        roomNode = shopRoom;
        slotIndex = index;
        productDefinition = null;
        productType = type;
        price = ShopProductCatalog.GetPrice(type);
        if (visual != null) visual.color = ShopProductCatalog.GetColor(type);
        if (label != null) label.text = $"{ShopProductCatalog.GetDisplayName(type)}\n{price} COINS\nF BUY";
    }

    public void Initialize(
        Player playerReference,
        RoomNode shopRoom,
        int index,
        ShopProductDefinition definition)
    {
        player = playerReference;
        input = player.Input;
        roomNode = shopRoom;
        slotIndex = index;
        productDefinition = definition;
        price = definition != null ? definition.Price : 0;
        if (visual != null && definition != null) visual.color = definition.DisplayColor;
        if (label != null)
        {
            label.text = definition != null
                ? $"{definition.DisplayName}\n{price} COINS\nF BUY"
                : "INVALID PRODUCT";
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || !playerInRange || purchased) return;
        if (input.InteractPressedThisFrame) TryPurchase();
    }

    public bool TryPurchase()
    {
        if (purchased || player == null || roomNode == null) return false;
        bool canApply = productDefinition != null
            ? productDefinition.CanApply(player)
            : ShopProductCatalog.CanApply(player, productType);
        if (!canApply) return false;
        if (!player.Inventory.SpendCoins(price)) return false;

        purchased = true;
        if (productDefinition != null) productDefinition.Apply(player);
        else ShopProductCatalog.Apply(player, productType);
        roomNode.MarkShopSlotPurchased(slotIndex);
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
