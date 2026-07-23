namespace WizardIslandRestApi.Game.Spells.ExtraEntities
{
    public class SimpleSpellEntityWithListOfPlayersToIgnore : SimpleSpellEntity
    {
        public int MaxHitCountPerPlayer { get; set; } = 1;
        private Dictionary<Player, int> _listOgPlayersAndHitCount = [];
        public SimpleSpellEntityWithListOfPlayersToIgnore(Player owner, Vector2 startPos, Dictionary<Player, int> listOgPlayersAndHitCount) : base(owner, startPos)
        {
            _listOgPlayersAndHitCount = listOgPlayersAndHitCount;
        }

        public override bool OnCollision(Player other)
        {
            if (IgnoreHitOnOwnerOnSpawn && other == MyCollider.Owner)
            {
                _hitOwnerLastUpdate = true;
                return false;
            }

            if (!_listOgPlayersAndHitCount.ContainsKey(other))
                _listOgPlayersAndHitCount.Add(other, 0);
            if (++_listOgPlayersAndHitCount[other] > MaxHitCountPerPlayer)
                return false;
            other.TakeDamage(Damage, MyCollider.Owner);
            other.ApplyKnockback((other.MyCollider.Pos - (Pos - Dir * Speed * 5)).Normalized(), Knockback);

            return true;
        }
    }
}
