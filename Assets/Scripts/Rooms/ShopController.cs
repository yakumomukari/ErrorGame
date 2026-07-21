using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopController : MonoBehaviour, IRoomFeature
{
    [SerializeField] private ShopProduct productPrefab;
    [SerializeField] private Transform[] slotPoints;
    [SerializeField] private ShopProductSet productSet;

    public ShopProductSet ProductSet => productSet;

    public void Configure(ShopProduct shopProductPrefab, Transform[] shopSlotPoints)
    {
        productPrefab = shopProductPrefab;
        slotPoints = shopSlotPoints;
    }

    public void SetProductSet(ShopProductSet products)
    {
        productSet = products;
    }

    public void Bind(Player player, RoomNode node, int dungeonSeed)
    {
        if (node.Type != RoomType.Shop) return;

        int seed = unchecked(dungeonSeed * 16777619 ^ node.Coordinate.GetHashCode() ^ 0x5A17);
        System.Random random = new System.Random(seed);
        if (productSet != null && productSet.Count > 0)
        {
            SpawnConfiguredProducts(player, node, random);
            return;
        }

        SpawnLegacyProducts(player, node, random);
    }

    private void SpawnConfiguredProducts(Player player, RoomNode node, System.Random random)
    {
        List<ShopProductDefinition> products = new List<ShopProductDefinition>(productSet.Products);
        for (int index = products.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            ShopProductDefinition temporary = products[index];
            products[index] = products[swapIndex];
            products[swapIndex] = temporary;
        }

        int slotCount = Mathf.Min(3, Mathf.Min(slotPoints.Length, products.Count));
        for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
        {
            if (node.IsShopSlotPurchased(slotIndex)) continue;
            ShopProduct product = Instantiate(productPrefab, slotPoints[slotIndex].position, Quaternion.identity, transform);
            product.Initialize(player, node, slotIndex, products[slotIndex]);
        }
    }

    private void SpawnLegacyProducts(Player player, RoomNode node, System.Random random)
    {
        List<ShopProductType> products = new List<ShopProductType>(
            (ShopProductType[])Enum.GetValues(typeof(ShopProductType)));
        for (int index = products.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            ShopProductType temporary = products[index];
            products[index] = products[swapIndex];
            products[swapIndex] = temporary;
        }

        int slotCount = Mathf.Min(3, Mathf.Min(slotPoints.Length, products.Count));
        for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
        {
            if (node.IsShopSlotPurchased(slotIndex)) continue;
            ShopProduct product = Instantiate(productPrefab, slotPoints[slotIndex].position, Quaternion.identity, transform);
            product.Initialize(player, node, slotIndex, products[slotIndex]);
        }
    }

    public void Initialize(IRoomRuntimeContext runtimeContext, RoomNode node)
    {
        Bind(runtimeContext.Player, node, runtimeContext.ActiveSeed);
    }

    public void OnRoomEntered()
    {
    }
}
