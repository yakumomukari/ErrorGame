public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(Player player);
    void Interact(Player player);
}
