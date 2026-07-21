using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerEffectSet",
    menuName = "Error Game/Effects/Player Effect Set")]
public sealed class PlayerEffectSet : ScriptableObject
{
    [SerializeField] private List<PlayerEffectDefinition> effects = new List<PlayerEffectDefinition>();

    public IReadOnlyList<PlayerEffectDefinition> Effects => effects;
    public int Count => effects != null ? effects.Count : 0;

    public PlayerEffectDefinition GetAt(int index)
    {
        return effects != null && index >= 0 && index < effects.Count ? effects[index] : null;
    }

    public void Configure(IEnumerable<PlayerEffectDefinition> definitions)
    {
        effects = definitions != null
            ? new List<PlayerEffectDefinition>(definitions)
            : new List<PlayerEffectDefinition>();
    }
}
