using Nalix.Domain.Models.Combat;

namespace Nalix.Domain.Models.Attacks;

public sealed class MeleeAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Melee;

    public void Execute(ICombatant attacker, ICombatant target)
    {
        System.Int64 damage = attacker.CalculateDamage(target);
        target.TakeDamage(damage);
    }
}