public interface IDamagable
{
    void Damage(int damageTaken, DamageTypes[] damageTypes = null);
    void Heal(int healthReplenished, HealingTypes healingType);
    void Killed();
}
