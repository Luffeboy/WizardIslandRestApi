using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace WizardIslandRestApi.Game.Spells
{
    public class CirclingSnake : Spell
    {
        public override string Name { get { return "Circling snake"; } }
        private float _damage = 5;
        private float _knockback = 1.5f;
        public override int CooldownMax { get; protected set; } = (int)(8.0f * Game._updatesPerSecond);
        public CirclingSnake(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 mousePos)
        {
            var dir = (mousePos - MyPlayer.Pos).Normalized();
            float size = .5f;
            int snakeParts = 10;
            GetCurrentGame().Entities.Add(new CirclingSnakePart(MyPlayer, 5 * Game._updatesPerSecond, GetCurrentGame(), snakeParts)
            {
                Pos = MyPlayer.Pos,
                Target = mousePos,
                Speed = 2,
                CirclingDistance = 10,
                Color = "100, 255, 100",
                Size = size,
                Damage = 5,
                Knockback = 1.5f,
            });
            GoOnCooldown();
        }
    }
    class CirclingSnakePart : CantHitOwnerAtStartSpellEntity
    {
        private const int TickTillSnakePartCreation = 3;
        private Vector2 _dir;
        private Vector2 _target;
        private float _angle; // circling angle
        private int _snakePartsToCreate;
        private int _ticksUntilDeletionMax;
        private CirclingSnakePart? _parent; // body part in front
        private CirclingSnakePart? _child; // body part in front
        private Game _game;
        private bool _isCircling = false;

        private bool IsHead { get { return _parent == null;  } }
        public Vector2 Target { get { return _target; } set { ReTarget(value); } }
        public float Speed { get; set; }
        public float CirclingDistance { get; set; }
        public float CirclingSpeed { get { return Speed / CirclingDistance; } }
        public override int TicksUntillCanHitOwner { get; set; } = 20;
        public float Damage { get; set; }
        public float Knockback { get; set; }
        public CirclingSnakePart(Player owner, int ticksUntilDeletion, Game game, int snakePartsToCreate = 5, CirclingSnakePart? parent = null) : base(owner, ticksUntilDeletion)
        {
            _parent = parent;
            _snakePartsToCreate = snakePartsToCreate;
            _ticksUntilDeletionMax = ticksUntilDeletion;
            _game = game;
        }


        public override bool OnCollision(Entity other)
        {
            if (other.GetType().Name == GetType().Name)
                return false;
            if (base.OnCollision(other))
            {
                if (_child != null)
                    _child._parent = _parent;
                return true;
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            _target = pos;
            _dir = (Target - Pos).Normalized();
            _isCircling = false;
        }

        public override bool Update()
        {
            // create snake body
            if (_snakePartsToCreate > 0 && TickTillSnakePartCreation > _ticksUntilDeletionMax - _ticksUntilDeletion)
            {
                _child = new CirclingSnakePart(MyCollider.Owner, _ticksUntilDeletion, _game, _snakePartsToCreate - 1, this)
                {
                    Pos = Pos,
                    Speed = Speed,
                    Size = Size,
                    Color = Color,
                    Damage = Damage,
                    Knockback = Knockback,
                    Target = Target,
                    CirclingDistance = CirclingDistance,
                };
                _game.Entities.Add(_child);
                _snakePartsToCreate = 0;
            }
            // move snake
            if (IsHead)
            {
                if (_isCircling)
                {
                    // circling
                    _angle -= CirclingSpeed;
                    Pos = Target + new Vector2(MathF.Cos(_angle), MathF.Sin(_angle)) * CirclingDistance;
                }
                else
                {
                    // not circling
                    Pos += _dir * Speed;
                    // check if we are in circling distance
                    var diffToTarget = _target - Pos;
                    if (diffToTarget.x * diffToTarget.x + 
                        diffToTarget.y * diffToTarget.y < 
                        CirclingDistance * CirclingDistance)
                    {
                        _isCircling = true;
                        _angle = MathF.Atan2(-_dir.y, -_dir.x); // starting angle
                    }
                }
            }
            else
            {
                // not head
                var dir = (Pos - _parent.Pos).Normalized();
                Pos = _parent.Pos + dir * (Size + _parent.Size);
            }
            return --_ticksUntilDeletion < 0;
        }

        protected override bool HitPlayer(Player other)
        {
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);
            if (_child != null)
                _child._parent = _parent;
            return true;
        }
    }
}
