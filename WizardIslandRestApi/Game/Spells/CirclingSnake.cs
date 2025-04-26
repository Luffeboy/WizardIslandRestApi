using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace WizardIslandRestApi.Game.Spells
{
    public class CirclingSnake : Spell
    {
        public override string Name { get { return "Circling snake"; } }
        private float _damage = 5;
        private float _knockback = 1.75f;
        public override int CooldownMax { get; protected set; } = (int)(12.0f * Game._updatesPerSecond);
        public CirclingSnake(Player player) : base(player)
        {
        }
        public override void OnCast(Vector2 pos, Vector2 mousePos)
        {
            var dir = (mousePos - pos).Normalized();
            float size = .5f;
            int snakeParts = 10;
            GetCurrentGame().Entities.Add(new CirclingSnakePart(MyPlayer, 5 * Game._updatesPerSecond, GetCurrentGame(), pos, snakeParts)
            {
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
        public CirclingSnakePart? _parent; // body part in front
        public CirclingSnakePart? _child; // body part in front
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
        private List<Player> _hitPlayers;
        public CirclingSnakePart(Player owner, int ticksUntilDeletion, Game game, Vector2 startPos, int snakePartsToCreate = 5, CirclingSnakePart? parent = null, List<Player> hitPlayers = null) : base(owner, ticksUntilDeletion, startPos)
        {
            _parent = parent;
            _snakePartsToCreate = snakePartsToCreate;
            _ticksUntilDeletionMax = ticksUntilDeletion;
            _game = game;
            _hitPlayers = (hitPlayers == null) ? new List<Player>() : hitPlayers;
            GetAndSetEntityId();
            ForwardAngle = MathF.Atan2(-_dir.y, -_dir.x);
        }

        public void GetAndSetEntityId()
        {
            if (_parent == null)
                EntityId = "SnakeHead";
            else if (_child == null)
                EntityId = "SnakeTail";
            else EntityId = "SnakeBody";
        }


        public override bool OnCollision(Entity other)
        {
            if (other.GetType().Name == GetType().Name)
                return false;
            if (base.OnCollision(other))
            {
                Died();
                return true;
            }
            return false;
        }

        public override void ReTarget(Vector2 pos)
        {
            _ticksUntilDeletion = _ticksUntilDeletionMax;
            if (_parent != null)
                _parent.ReTarget(pos);
            else
            {
                _target = pos;
                _dir = (Target - Pos).Normalized();
                _isCircling = false;
                Pos += _dir * Speed;
                ForwardAngle = MathF.Atan2(-_dir.y, -_dir.x);
            }
        }

        public override bool Update()
        {
            // create snake body
            if (_snakePartsToCreate > 0 && TickTillSnakePartCreation > _ticksUntilDeletionMax - _ticksUntilDeletion)
            {
                _child = new CirclingSnakePart(MyCollider.Owner, _ticksUntilDeletion, _game, Pos, _snakePartsToCreate - 1, this, _hitPlayers)
                {
                    Pos = Pos,
                    Speed = Speed,
                    Size = Size,
                    Color = Color,
                    Damage = Damage,
                    Knockback = Knockback,
                    //Target = Target,
                    CirclingDistance = CirclingDistance,
                };
                _game.Entities.Add(_child);
                _snakePartsToCreate = 0;
                GetAndSetEntityId();
            }
            // move snake
            if (IsHead)
            {
                if (_isCircling)
                {
                    // circling
                    _angle -= CirclingSpeed;
                    ForwardAngle = _angle - MathF.PI * .5f;
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
                ForwardAngle = MathF.Atan2(-_dir.y, -_dir.x); // starting angle
            }
            if (--_ticksUntilDeletion < 0)
            {
                Died();
                return true;
            }
            return false;
        }

        protected override bool HitPlayer(Player other)
        {
            if (!_hitPlayers.Contains(other))
            {
                other.TakeDamage(Damage, MyCollider.Owner);
                other.ApplyKnockback((other.MyCollider.PreviousPos - MyCollider.PreviousPos).Normalized(), Knockback);
                _hitPlayers.Add(other);
                Died();
                return true;
            }
            //Died();
            return false;
        }

        private void Died()
        {
            if (_child != null)
            {
                _child._parent = _parent;
                _child.GetAndSetEntityId();
                if (IsHead)
                    _child.Target = Target;
            }
            if (_parent != null)
            {
                _parent._child = _child;
                _parent.GetAndSetEntityId();
            }
        }
    }
}
