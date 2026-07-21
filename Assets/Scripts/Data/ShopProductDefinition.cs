using UnityEngine;

[CreateAssetMenu(
    fileName = "ShopProduct",
    menuName = "Error Game/Shop/Product")]
public sealed class ShopProductDefinition : ScriptableObject
{
    [SerializeField] private string stableId;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int price;
    [SerializeField] private Color displayColor = Color.white;
    [SerializeField] private PlayerEffectDefinition effect;

    public string StableId => stableId;
    public string DisplayName => displayName;
    public int Price => price;
    public Color DisplayColor => displayColor;
    public PlayerEffectDefinition Effect => effect;

    public bool CanApply(Player player)
    {
        return effect != null && effect.CanApply(player);
    }

    public void Apply(Player player)
    {
        effect?.Apply(player);
    }

    public void Configure(
        string id,
        string productDisplayName,
        int productPrice,
        Color color,
        PlayerEffectDefinition playerEffect)
    {
        stableId = id;
        displayName = productDisplayName;
        price = Mathf.Max(0, productPrice);
        displayColor = color;
        effect = playerEffect;
    }
}
