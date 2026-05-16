namespace Rounds2.Combat
{
    public static class WeaponFireRules
    {
        public static bool CanFire(bool roundCombatOpen, bool shooterDead)
        {
            return roundCombatOpen && !shooterDead;
        }
    }
}
