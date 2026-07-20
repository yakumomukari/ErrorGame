using UnityEngine;

public sealed class HealingStation : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "E  恢复半颗心";

    public bool CanInteract(Player player)
    {
        return player != null && !player.Health.IsDead &&
               player.Health.CurrentHealthUnits < player.Health.MaxHealthUnits;
    }

    public void Interact(Player player)
    {
        player.Health.Heal(1);
    }
}
