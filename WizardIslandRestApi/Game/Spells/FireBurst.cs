namespace WizardIslandRestApi.Game.Spells
{
    public class FireBurst : Spell
    {
        public override string Name { get { return "Fire burst"; } }
        private float _damage = 1;
        private float _knockback = 1.1f;
        public override int CooldownMax { get; protected set; } = 5 * Game._updatesPerSecond;
        public FireBurst(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            float distanceBetweenFireballs = 8;
            int fireballs = 8;
            Vector2 fwd = (mousePos - MyPlayer.Pos).Normalized();
            Vector2 normal = (mousePos - MyPlayer.Pos).Normal().Normalized();
            float size = .5f;

            for (int i = 0; i < fireballs; i++)
            {
                float half = i - fireballs / 2 + .5f;
                Vector2 fireballStartPos = normal * distanceBetweenFireballs * (half) + MyPlayer.Pos;
                // move it back a little
                fireballStartPos -= fwd * (MathF.Abs(half) * distanceBetweenFireballs / 4);
                fireballStartPos += fwd * (MyPlayer.Size + size + .6f);
                var dir = (mousePos - fireballStartPos).Normalized();
                GetCurrentGame().Entities.Add(new SimpleSpellEntity(MyPlayer)
                {
                    Pos = fireballStartPos,
                    Dir = dir,
                    Speed = 2f,
                    Color = "255, 0, 0",
                    Size = size,
                    TicksUntilDeletion = 90,
                    Damage = _damage,
                    Knockback = _knockback,
                });
            }

            GoOnCooldown();
        }
    }
}
