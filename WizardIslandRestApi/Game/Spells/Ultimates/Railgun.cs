using WizardIslandRestApi.Game.Physics;

namespace WizardIslandRestApi.Game.Spells.Ultimates
{
    public class Railgun : Spell
    {
        private float _range = 30;
        private float _size = .5f;
        private int _delay = Game._updatesPerSecond / 4;

        private float _damage = 15;
        private float _knockback = 4.0f;
        public Railgun(Player player) : base(player)
        {
            Type = SpellType.Ultimate;
        }

        public override int CooldownMax { get; protected set; } = (int)(25 * Game._updatesPerSecond);

        public override void OnCast(Vector2 startPos, Vector2 mousePos)
        {
            Vector2 spellDir = mousePos - startPos;
            float spellLen = _range;
            //float spellLen = spellDir.Length();
            spellDir.Normalize();
            float forwardAngle = HomingBoltEntity.GetAngleFromDirection(spellDir);
            ShadowEntity[] spells = new ShadowEntity[(int)(spellLen + 1)];
            for (int i = 0; i < spells.Length; i++)
                GetCurrentGame().Entities.Add(spells[i] = new ShadowEntity() { Color = "255,255,0", Size = .5f, EntityId = "RailgunPartical", Pos = startPos + spellDir * i, ForwardAngle = forwardAngle, TicksUntilDeletion = _delay + 3 });
            
            GetCurrentGame().Entities.Add(new WaitToDoSomethingEntity(_delay, () =>
            {
                for (int i = 0; i < spells.Length; i++)
                {
                    spells[i].Color = "255,255,255";
                    spells[i].EntityId = "RailgunParticalExplosion";
                }
                    var collider = new Collider(startPos);
                collider.Pos = startPos + spellDir * spellLen;
                // check player hits
                foreach (Player player in GetCurrentGame().Players.Values)
                {
                    if (player == MyPlayer)
                        continue;
                    if (collider.CheckCollision(player.MyCollider))
                    {
                        player.TakeDamage(_damage, MyPlayer);
                        player.ApplyKnockback(spellDir, _knockback);
                    }
                }
            }));
            GoOnCooldown();
        }
    }
}
