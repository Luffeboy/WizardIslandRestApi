namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Luna : Spell
    {
        private float _rangeMax = 35;
        private float _damage = 5;
        private float _knockback = 1.75f;
        private float _moonFallKnockback = 2.5f;
        public Luna(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = 20 * Game._updatesPerSecond;

        public override void OnCast(Vector2 mousePos)
        {
            float size = 1.0f;
            Vector2 endPos = mousePos;
            Vector2 dir = mousePos - MyPlayer.Pos;
            if (dir.LengthSqr() > _rangeMax * _rangeMax)
            {
                dir = dir.Normalized() * _rangeMax;
                endPos = MyPlayer.Pos + dir;
            }
            var ticksUntilDeletion = (int)(dir.Length() * .75f);
            dir.Normalize();
            Vector2 dirNormal = dir.Normal();
            Vector2 startPos = MyPlayer.Pos +
                               dir * (MyPlayer.Size + size + .1f) +
                               dirNormal * (MyPlayer.Size + size + .1f);
            float amountToSide = 1.0f;
            GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos, amountToSide)
            {
                Color = "100, 100, 255",
                Size = size,
                TicksUntilDeletionMax = ticksUntilDeletion,
                Damage = _damage,
                Knockback = _knockback,
                AmountToSideMultiplier = amountToSide,
            });
            GetCurrentGame().Entities.Add(new CrescentMoonEntity(MyPlayer, startPos, endPos, -amountToSide)
            {
                Color = "100, 100, 255",
                Size = size,
                TicksUntilDeletionMax = ticksUntilDeletion,
                Damage = _damage,
                Knockback = _knockback,
            });
            GetCurrentGame().Entities.Add(new WaitToDoSomethingEntity(ticksUntilDeletion, () => 
            {
                // when the two cresent moons hit each other
                GetCurrentGame().Entities.Add(new MeteorEntity(MyPlayer, endPos)
                {
                    Color = "50, 50, 200",
                    FallTime = 5,
                    KnockbackMin = _moonFallKnockback,
                    KnockbackMax = _moonFallKnockback,
                    Damage = _damage,
                    EntityId = "Moon",
                    Size = 5.0f,
                });
            }));
            GoOnCooldown();
        }
    }
}
