using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ShopProductSet",
    menuName = "Error Game/Shop/Product Set")]
public sealed class ShopProductSet : ScriptableObject
{
    [SerializeField] private List<ShopProductDefinition> products = new List<ShopProductDefinition>();

    public IReadOnlyList<ShopProductDefinition> Products => products;
    public int Count => products != null ? products.Count : 0;

    public void Configure(IEnumerable<ShopProductDefinition> definitions)
    {
        products = definitions != null
            ? new List<ShopProductDefinition>(definitions)
            : new List<ShopProductDefinition>();
    }
}
