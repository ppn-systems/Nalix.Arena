using Nalix.Domain.Models.Combat;

namespace Nalix.Domain.Models.Attacks;

public interface IAttackBehavior
{
    AttackType Type { get; }

    void Execute(ICombatant attacker, ICombatant target);
}