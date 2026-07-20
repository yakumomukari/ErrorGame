using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInteractor : MonoBehaviour
{
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    [SerializeField, Min(0.1f)] private float interactionRadius = 1.2f;
    private Player player;

    public string CurrentPrompt { get; private set; } = string.Empty;
    public event Action<string> PromptChanged;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        IInteractable nearest = FindNearestInteractable();
        string prompt = nearest != null ? nearest.InteractionPrompt : string.Empty;
        if (prompt != CurrentPrompt)
        {
            CurrentPrompt = prompt;
            PromptChanged?.Invoke(CurrentPrompt);
        }

        // Direct interaction currently has no input binding. E is reserved for
        // bomb placement; resource drops are collected automatically.
    }

    private IInteractable FindNearestInteractable()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, interactionRadius, overlapResults);
        IInteractable nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapResults[i];
            IInteractable candidate = hit.GetComponentInParent<IInteractable>();
            if (candidate == null || !candidate.CanInteract(player)) continue;

            float distance = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance >= nearestDistance) continue;
            nearestDistance = distance;
            nearest = candidate;
        }

        return nearest;
    }
}
