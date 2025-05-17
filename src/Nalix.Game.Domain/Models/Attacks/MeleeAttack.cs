using Nalix.Game.Domain.Models.Combat;

namespace Nalix.Game.Domain.Models.Attacks;

public sealed class MeleeAttack : IAttackBehavior
{
    public AttackType Type => AttackType.Melee;

    public void Execute(ICombatant attacker, ICombatant target)
    {
        long damage = attacker.CalculateDamage(target);
        target.TakeDamage(damage);
    }
}