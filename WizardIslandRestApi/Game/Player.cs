using System.Drawing;
using WizardIslandRestApi.Game.Physics;
using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.Debuffs;

namespace WizardIslandRestApi.Game
{
    public class PlayerStatObservers
    {
        /// <summary>
        /// Health before, then health after
        /// </summary>
        public Action<int, int> OnHealthChanged;
    }
    public class PlayerStats
    {
        public PlayerStatObservers Observers {  get; } = new PlayerStatObservers();
        public const float DefaultSpeed = 1.5f / Game._updatesPerSecond;
        private float _speed = DefaultSpeed;
        private float _speedMultiplier = 1f;
        private int _health = 0;
        public float Speed { get { return _speed * SpeedMultiplier; } set { _speed = value; } }
        public float SpeedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }
        public float MaxSpeed { get { return Speed * Game._updatesPerSecond; } }
        public float SlowDownSpeed { get { return Speed / 3; } }
        public int Health { get { return _health; } set { if (value > MaxHealth) value = MaxHealth; Observers.OnHealthChanged?.Invoke(_health, value); _health = value; } }
        public int MaxHealth { get; set; } = 100;

        public float CooldownMultiplier { get; set; } = 1;
        public float KnockbackMultiplier { get; set; } = .75f;
        public float DamageMultiplier { get; set; } = 1;

    }
    public class PlayerScoreStats
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }

    }
    public class PlayerOverrideAndObservers
    {
        public Action<int> OnSpellCast;
        public Action OnRespawnPreReset;
        public Action OnRespawnPostReset;
        public Action OnHealthChanged;
    }
    public class Player
    {
        private Vector2 _pos;
        private float _size; // don't set here
        public int Id { get; set; }
        public Game _game { get; set; }
        public string Name { get; set; }
        public string Password { get; }
        public float CanStopSpeed { get { return Stats.Speed * 1.1f; } }
        public Vector2 TargetPos { get; set; }
        public Vector2 Pos { get { return _pos; } set { _pos = value; MyCollider.Pos = value; } }
        public Vector2 Vel {  get; set; }
        public float Size { get { return _size; } set { _size = value; MyCollider.Size = value; } }
        public string UserName { get; set; } = "Nameless";
        public string Color { get; set; } = "255, 0, 0";
        public PlayerStats Stats { get; set; } = new PlayerStats();
        public PlayerScoreStats ScoreStats { get; set; } = new PlayerScoreStats();
        public PlayerOverrideAndObservers OverridesAndObservers { get; set; } = new PlayerOverrideAndObservers();
        public Player? LastHitByPlayer { get; set; } = null; // the player that last hit this player
        public Collider MyCollider { get; }
        private Spell[] MySpells { get; set; }
        public int TicksTillAlive { get; private set; }
        public bool IsDead { get { return TicksTillAlive > 0; } }
        
        List<DebuffBase> Debuffs = new List<DebuffBase>();
        public Player(int id, Game game, int[] spells)
        {
            MyCollider = new Collider(new Vector2());
            Size = 1.0f;
            Id = id;
            _game = game;
            MyCollider.Owner = this;
            // spells
            MySpells = new Spell[spells.Length];
            for (int i = 0; i < spells.Length; i++)
            {
                MySpells[i] = Spell.GetSpell(this, spells[i]);
                MySpells[i].SetSpellIndex(spells[i]);
            }

            Password = "";
            Random random = new Random();
            for (int i = 0; i < 10; i++)
                Password += random.Next(10);
            Reset();
        }
        public void CastSpell(int spellIndex, Vector2 mousePos)
        {
            if (IsDead || spellIndex < 0 || spellIndex >= MySpells.Length || !MySpells[spellIndex].CanCast)
                return;
            OverridesAndObservers.OnSpellCast?.Invoke(spellIndex);
            lock(MySpells[spellIndex])
                MySpells[spellIndex].OnCast(Pos, mousePos);
        }

        public void Reset()
        {
            TicksTillAlive = -1;
            Stats.Health = Stats.MaxHealth;
            Vel = new Vector2();
            // remove debuffs
            ClearDebuffs();
            // add a little invulnerability
            ApplyDebuff(new Invulnerability(this) { TicksTillRemoval = 3 * Game._updatesPerSecond });
            // ready spells
            for (int i = 0; i < MySpells.Length; i++)
                MySpells[i].OnPlayerReset();
            //get random spawn on the map
            Random r = new Random();
            float angle = (float)(r.NextDouble() * Math.PI * 2);
            var map = GetMap();
            float distance = (float)(r.NextDouble() * (map.CircleRadius - map.CircleInnerRadius)) + map.CircleInnerRadius;
            TeleportTo(map.GroundMiddle + new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance));
            TargetPos = Pos;
        }
#pragma warning disable
        private Map GetMap()
        {
            return GetGame().GameMap;
        }
        public Game GetGame()
        {
            return _game;
        }
#pragma warning enable

        public void Update()
        {
            if (IsDead)
            {
                TicksTillAlive--;
                if (!IsDead)
                    Reset();
                return;
            }
            UpdateDebuffs();
            Move();
            // update collider
            //MyCollider.Pos = Pos;
            //MyCollider.Size = Size;
            // take damage from lava
            Map map = GetMap();
            float distanceToMapCenterSqr = (map.GroundMiddle - Pos).LengthSqr(); 
            if (distanceToMapCenterSqr < map.CircleInnerRadius * map.CircleInnerRadius || distanceToMapCenterSqr > map.CircleRadius * map.CircleRadius)
            {
                TakeDamage(Game.LavaDamage);
            }
        }

        private void Move()
        {
            // update velocity
            // slow down a little
            var velLen = Vel.Length();
            Vector2 velNormalized = Vel / velLen;
            if (Vel.LengthSqr() > Stats.SlowDownSpeed)
            {
                Vel -= velNormalized * MathF.Sqrt(velLen) * Stats.SlowDownSpeed;
            }

            // add more velocity
            //Vector2 dir = TargetPos - Pos;
            //dir.Normalize();
            // if we move along our current velocity, we might need to move a bit to the right or left, to correctly hit the target position
            Vector2 posPlusVelocityDir = TargetPos - (Pos + Vel * (10 * PlayerStats.DefaultSpeed / Stats.Speed));
            posPlusVelocityDir.Normalize();
            //dir = dir + (posPlusVelocityDir * 10);
            //dir.Normalize();
            Vector2 dir = posPlusVelocityDir;
            Vector2 velToAdd = new Vector2();
            if (velNormalized.Dot(dir) < .7f || velLen < Stats.MaxSpeed)
                velToAdd = dir * Stats.Speed;
            // check for overshooting
            if (Stats.Speed * Stats.Speed > (TargetPos - Pos).LengthSqr() && Vel.LengthSqr() < Stats.Speed * Stats.Speed)
            {
                velToAdd = Vel * -1 + (TargetPos - Pos);
            }

            Vel += velToAdd;
            // update position
            Pos += Vel;
            //Vector2 dirAfter = TargetPos - Pos;
            //if ((TargetPos - Pos).LengthSqr() < Size && Vel.LengthSqr() < CanStopSpeed * CanStopSpeed)
            //{
            //    Vel = new Vector2(0, 0);
            //    Pos = TargetPos;
            //}
        }

        public void ApplyDebuff(DebuffBase debuff)
        {
            // remove old instance of debuff, if not stackable
            if (!debuff.Stackable)
                for (int i = 0; i < Debuffs.Count; i++) 
                {
                    if (debuff.GetType().Name == Debuffs[i].GetType().Name)
                    {
                        Debuffs[i].OnRemove();
                        Debuffs.RemoveAt(i);
                        break;
                    }
                }
            debuff.OnApply();
            Debuffs.Add(debuff);
        }
        public void ClearDebuffs()
        {
            while (Debuffs.Count > 0)
            {
                Debuffs[Debuffs.Count - 1].OnRemove();
                Debuffs.RemoveAt(Debuffs.Count - 1);
            }
            Stats.Speed = PlayerStats.DefaultSpeed;
            Stats.SpeedMultiplier = 1.0f;
        }
        private void UpdateDebuffs()
        {
            for (int i = 0; i < Debuffs.Count; i++)
            {
                if (Debuffs[i].Update())
                {
                    Debuffs[i].OnRemove();
                    Debuffs.RemoveAt(i);
                    i--;
                }
            }
        }
        public void RemoveDebuff(string name)
        {
            for (int i = 0; i < Debuffs.Count; i++)
            {
                if (Debuffs[i].ToString() == name)
                {
                    Debuffs[i].OnRemove();
                    Debuffs.RemoveAt(i);
                    return;
                }
            }
        }
        public void TeleportTo(Vector2 pos)
        {
            Pos = pos;
            Pos = pos; // also set the previous pos
            TargetPos = pos;
        }

        public void ApplyKnockback(Vector2 dir, float amount)
        {
            Vel += dir * amount * GetGame().GameModifiers.KnockbackMultiplier * Stats.KnockbackMultiplier;
        }
        public void TakeDamage(float dmg, Player player = null)
        {
            if (IsDead)
                return;
            if (player != null && player != this)
                LastHitByPlayer = player;
            Stats.Health -= (int)(dmg * GetGame().GameModifiers.DamageMultiplier * Stats.DamageMultiplier);
            if (Stats.Health <= 0)
                Die();
        }
        public void Heal(float amount)
        {
            var scaledAmount = (int)(amount);
            if (scaledAmount < 0) 
                scaledAmount = 0;
            Stats.Health += scaledAmount;
        }
        private void Die()
        {
            ScoreStats.Deaths++;
            if (LastHitByPlayer != null)
            {
                LastHitByPlayer.ScoreStats.Kills++;
                LastHitByPlayer = null;
            }
            TicksTillAlive = 5 * Game._updatesPerSecond;
        }
        public SpellCooldown[] GetSpellCooldowns()
        {
            SpellCooldown[] spells = new SpellCooldown[MySpells.Length];
            for (int i = 0; i < spells.Length; i++)
                spells[i] = new SpellCooldown()
                {
                    SpellName = MySpells[i].ToString(),
                    CooldownRemaining = MySpells[i].CurrentCooldown - _game.GameTick,
                    CooldownMax = MySpells[i].CooldownMax
                };
            return spells;
        }

        public Spell[] GetSpells() { return MySpells; }
        public void SetSpells(Spell[] spells) 
        {
            var oldSpells = MySpells;
            MySpells = spells;
            for (int i = 0; i < oldSpells.Length && i < MySpells.Length; i++)
                if (!oldSpells[i].CanBeReplaced)
                    MySpells[i] = oldSpells[i];
        }

        public IEnumerable<DebuffBase> GetDebuffs() { return Debuffs; }
    }

    public class SpellCooldown
    {
        public string SpellName { get; set; }
        public int CooldownRemaining { get; set; }
        public int CooldownMax { get; set; }
    }

    public class PlayerMinimum
    {
        public int Id { get; set; }
        public Vector2 Pos { get; set; }
        public float Size { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public bool IsDead { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public IEnumerable<string> Debuffs { get; set; }
        public PlayerMinimum(Player player) 
        {
            Id = player.Id;
            Size = player.Size;
            Pos = player.Pos;
            Health = player.Stats.Health;
            MaxHealth = player.Stats.MaxHealth;
            Kills = player.ScoreStats.Kills;
            Deaths = player.ScoreStats.Deaths;
            IsDead = player.IsDead;
            Name = player.UserName;
            Color = player.Color;
            Debuffs = player.GetDebuffs().Select(d => d.ToString());
        }

        public static IEnumerable<PlayerMinimum> Copy(IEnumerable<Player> players)
        {
            List<PlayerMinimum> list = new List<PlayerMinimum>(players.Count());
            foreach (var player in players)
            {
                list.Add(new PlayerMinimum(player));
            }
            return list;
        }
    }
}
