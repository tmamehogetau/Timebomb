namespace Rounds2.Cards
{
    public readonly struct CardDefinition
    {
        public CardDefinition(
            CardId id,
            string name,
            string effect,
            CardCategory category,
            CardStacking stacking,
            int burstCountDelta = 0,
            int projectileCountDelta = 0,
            float spreadAngleDegreesDelta = 0f,
            float reloadMultiplier = 1f,
            float shieldActiveSecondsDelta = 0f,
            float shieldCooldownMultiplier = 1f,
            float projectileSpeedMultiplier = 1f,
            float projectileDamageMultiplier = 1f,
            int projectileBounces = 0)
        {
            Id = id;
            Name = name;
            Effect = effect;
            Category = category;
            Stacking = stacking;
            BurstCountDelta = burstCountDelta;
            ProjectileCountDelta = projectileCountDelta;
            SpreadAngleDegreesDelta = spreadAngleDegreesDelta;
            ReloadMultiplier = reloadMultiplier;
            ShieldActiveSecondsDelta = shieldActiveSecondsDelta;
            ShieldCooldownMultiplier = shieldCooldownMultiplier;
            ProjectileSpeedMultiplier = projectileSpeedMultiplier;
            ProjectileDamageMultiplier = projectileDamageMultiplier;
            ProjectileBounces = projectileBounces;
        }

        public CardId Id { get; }
        public string Name { get; }
        public string Effect { get; }
        public CardCategory Category { get; }
        public CardStacking Stacking { get; }
        public int BurstCountDelta { get; }
        public int ProjectileCountDelta { get; }
        public float SpreadAngleDegreesDelta { get; }
        public float ReloadMultiplier { get; }
        public float ShieldActiveSecondsDelta { get; }
        public float ShieldCooldownMultiplier { get; }
        public float ProjectileSpeedMultiplier { get; }
        public float ProjectileDamageMultiplier { get; }
        public int ProjectileBounces { get; }
    }
}
