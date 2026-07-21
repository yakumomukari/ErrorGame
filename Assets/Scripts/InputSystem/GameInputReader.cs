using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
public sealed class GameInputReader : MonoBehaviour, IPlayerInput
{
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap gameplay;
    private InputAction move;
    private InputAction pointer;
    private InputAction fire;
    private InputAction dash;
    private InputAction bomb;
    private InputAction interact;
    private InputAction pause;
    private InputAction restart;
    private InputAction itemsTab;
    private InputAction notebookTab;
    private InputAction logTab;

    public InputActionAsset InputActions => inputActions;
    public InputActionMap GameplayMap => gameplay;
    public InputAction MoveAction => move;
    public InputAction PointerAction => pointer;
    public InputAction FireAction => fire;
    public InputAction DashAction => dash;
    public InputAction BombAction => bomb;
    public InputAction InteractAction => interact;
    public InputAction PauseAction => pause;
    public InputAction RestartAction => restart;
    public InputAction ItemsTabAction => itemsTab;
    public InputAction NotebookTabAction => notebookTab;
    public InputAction LogTabAction => logTab;
    public bool IsReady => gameplay != null && gameplay.enabled;
    public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 PointerScreenPosition => pointer != null ? pointer.ReadValue<Vector2>() : Vector2.zero;
    public bool FireHeld => fire != null && fire.IsPressed();
    public bool DashPressedThisFrame => dash != null && dash.WasPressedThisFrame();
    public bool BombPressedThisFrame => bomb != null && bomb.WasPressedThisFrame();
    public bool InteractPressedThisFrame => interact != null && interact.WasPressedThisFrame();
    public bool PausePressedThisFrame => pause != null && pause.WasPressedThisFrame();
    public bool RestartPressedThisFrame => restart != null && restart.WasPressedThisFrame();
    public bool ItemsTabPressedThisFrame => itemsTab != null && itemsTab.WasPressedThisFrame();
    public bool NotebookTabPressedThisFrame => notebookTab != null && notebookTab.WasPressedThisFrame();
    public bool LogTabPressedThisFrame => logTab != null && logTab.WasPressedThisFrame();

    public void Configure(InputActionAsset actions)
    {
        inputActions = actions;
    }

    private void Awake()
    {
        ResolveActions();
    }

    private void OnEnable()
    {
        if (gameplay == null) ResolveActions();
        gameplay?.Enable();
    }

    private void OnDisable()
    {
        gameplay?.Disable();
    }

    private void ResolveActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("GameInputReader requires an InputActionAsset.", this);
            return;
        }

        gameplay = inputActions.FindActionMap("Gameplay", true);
        move = gameplay.FindAction("Move", true);
        pointer = gameplay.FindAction("Pointer", true);
        fire = gameplay.FindAction("Fire", true);
        dash = gameplay.FindAction("Dash", true);
        bomb = gameplay.FindAction("Bomb", true);
        interact = gameplay.FindAction("Interact", true);
        pause = gameplay.FindAction("Pause", true);
        restart = gameplay.FindAction("Restart", true);
        itemsTab = gameplay.FindAction("ItemsTab", true);
        notebookTab = gameplay.FindAction("NotebookTab", true);
        logTab = gameplay.FindAction("LogTab", true);
    }
}
