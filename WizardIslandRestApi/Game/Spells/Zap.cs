using WizardIslandRestApi.Game.Physics;

namespace WizardIslandRestApi.Game.Spells
{
    public class Zap : Spell
    {
        private float _range = 15;
        private float _knockback = 1.5f;
        public Zap(Player player) : base(player)
        {
        }

        public override int CooldownMax { get; protected set; } = 10 * Game._updatesPerSecond;

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            if ((mousePos - startPos).LengthSqr() > _range * _range)
                mousePos = startPos + (mousePos - startPos).Normalized() * _range;

            Vector2 spellDir = mousePos - startPos;
            float spellLen = spellDir.Length();
            if (spellLen != 0)
                spellDir /= spellLen;

            var collider = new Collider(startPos);
            collider.Pos = mousePos;
            // check player hits
            foreach (Player player in GetCurrentGame().Players.Values)
            {
                if (player == MyPlayer)
                    continue;
                if (collider.CheckCollision(player.MyCollider))
                {
                    player.Vel *= -1;
                    var dir = player.Vel;
                    if (dir.LengthSqr() == 0)
                        dir = startPos - mousePos;
                    dir.Normalize();
                    player.ApplyKnockback(dir, _knockback);
                }
            }
            // visual
            for (int i = 0; i < spellLen; i++)
                GetCurrentGame().Entities.Add(new ShadowEntity() { Color = "255,255,0", Size = .5f, EntityId = "Electricity", Pos = startPos + spellDir * i });
            GetCurrentGame().Entities.Add(new ShadowEntity() { Color = "255,255,0", Size = .75f, EntityId = "Electricity", Pos = startPos + spellDir * spellLen });
            GoOnCooldown();
        }
    }
}
