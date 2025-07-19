using Nalix.Game.Domain.Models.Combat;

namespace Nalix.Game.Domain.Models.Attacks;

public interface IAttackBehavior
{
    AttackType Type { get; }

    void Execute(ICombatant attacker, ICombatant target);
}