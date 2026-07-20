public interface ICombatEnemy
{
    EnemyHealth Health { get; }
    void Initialize(Player playerTarget);
}
