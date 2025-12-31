using WizardIslandRestApi.Game.Spells;
using WizardIslandRestApi.Game.Spells.BasicSpells;
namespace WizardIslandRestApi.Game.Events
{
    public class BackToBasicsEvent : EventBase
    {
        private Dictionary<int, Spell[]> _spells = new Dictionary<int, Spell[]>();
        public BackToBasicsEvent(Game game) : base(game)
        {
            Name = "Back to basics";
        }

        public override void Start()
        {
            foreach (var player in _game.Players)
            {
                var spells = player.Value.GetSpells();
                _spells.Add(player.Key, spells);
                Spell[] fireballs = new Spell[_game.AllowedSpellCount];
                for (int i = 0; i < fireballs.Length; i++)
                    fireballs[i] = new FireBall(player.Value);
                player.Value.SetSpells(fireballs);
            }
        }

        public override void End()
        {
            foreach (var playerSpells in _spells)
            {
                _game.Players[playerSpells.Key].SetSpells(playerSpells.Value);
            }
        }

        public override void EarlyUpdate()
        {
        }

        public override void LateUpdate()
        {
        }
    }
}
