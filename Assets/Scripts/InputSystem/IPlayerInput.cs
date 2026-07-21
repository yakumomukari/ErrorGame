using UnityEngine;

/// <summary>
/// Read-only gameplay input contract. Runtime systems depend on this interface
/// so keyboard/mouse input can later be replaced by gamepad, AI, replay, or tests.
/// </summary>
public interface IPlayerInput
{
    Vector2 Move { get; }
    Vector2 PointerScreenPosition { get; }
    bool FireHeld { get; }
    bool DashPressedThisFrame { get; }
    bool BombPressedThisFrame { get; }
    bool InteractPressedThisFrame { get; }
    bool PausePressedThisFrame { get; }
    bool RestartPressedThisFrame { get; }
    bool ItemsTabPressedThisFrame { get; }
    bool NotebookTabPressedThisFrame { get; }
    bool LogTabPressedThisFrame { get; }
}

public static class PlayerInputResolver
{
    public static IPlayerInput Require(Component owner)
    {
        IPlayerInput input = owner != null ? owner.GetComponent<IPlayerInput>() : null;
        if (input == null && owner != null)
        {
            Debug.LogError($"{owner.GetType().Name} requires an IPlayerInput component.", owner);
        }
        return input;
    }
}
