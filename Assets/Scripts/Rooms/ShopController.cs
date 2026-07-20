using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ShopController : MonoBehaviour, IRoomFeature
{
    [SerializeField] private ShopProduct productPrefab;
    [SerializeField] private Transform[] slotPoints;

    public void Configure(ShopProduct shopProductPrefab, Transform[] shopSlotPoints)
    {
        productPrefab = shopProductPrefab;
        slotPoints = shopSlotPoints;
    }

    public void Bind(Player player, RoomNode node, int dungeonSeed)
    {
        if (node.Type != RoomType.Shop) return;

        List<ShopProductType> products = new List<ShopProductType>(
            (ShopProductType[])Enum.GetValues(typeof(ShopProductType)));
        int seed = unchecked(dungeonSeed * 16777619 ^ node.Coordinate.GetHashCode() ^ 0x5A17);
        System.Random random = new System.Random(seed);
        for (int index = products.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            ShopProductType temporary = products[index];
            products[index] = products[swapIndex];
            products[swapIndex] = temporary;
        }

        int slotCount = Mathf.Min(3, slotPoints.Length);
        for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
        {
            if (node.IsShopSlotPurchased(slotIndex)) continue;
            ShopProduct product = Instantiate(productPrefab, slotPoints[slotIndex].position, Quaternion.identity, transform);
            product.Initialize(player, node.Shop, slotIndex, products[slotIndex]);
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
