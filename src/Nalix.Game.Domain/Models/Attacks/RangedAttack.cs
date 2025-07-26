using Nalix.Domain.Models.Combat;

namespace Nalix.Domain.Models.Attacks;
public sealed class RangedAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Ranged;

    public void Execute(ICombatant attacker, ICombatant target)
    {
        System.Int64 damage = attacker.CalculateDamage(target) - 2; // Tầm xa có thể yếu hơn một chút
        target.TakeDamage(damage);
    }
}