using System.Text.Json;
using WizardIslandRestApi.Game.Spells;

namespace WizardIslandRestApi.Helpers
{
    public class SpellSpecificConfiguration
    {
        public double MaxCooldownSeconds { get; set; }

        public int MaxCooldownTicks()
            => (int)(MaxCooldownSeconds * Game.Game._updatesPerSecond);
    }

    public class SpellConfiguration
    {
        private const string _configDirectory = "DefaultConfig";
        private const string _configFilePath = $"{_configDirectory}/SpellConfiguration.json";
        public Dictionary<string, SpellSpecificConfiguration> Spells { get; set; } = [];

        public static SpellConfiguration? LoadDefaultSpellConfiguration()
        {
            if (!File.Exists(_configFilePath))
                return null;
            SpellConfiguration? config = null;
            using (var file = new StreamReader(File.OpenRead(_configFilePath)))
            {
                string json = file.ReadToEnd();
                config = JsonSerializer.Deserialize<SpellConfiguration>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            }
            return config;
        }
#if DEBUG
        public static void ExportDefaultSpellConfiguration()
        {
            Spell[] spells = Spell.GetSpells();
            SpellConfiguration abc = new SpellConfiguration();
            foreach (var spell in spells)
            {
                abc.Spells.Add(spell.GetType().Name, new SpellSpecificConfiguration() 
                { 
                    MaxCooldownSeconds = (float)spell.CooldownMax / Game.Game._updatesPerSecond,
                });
            }
            string json = JsonSerializer.Serialize(abc, new JsonSerializerOptions() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (!Directory.Exists(_configDirectory))
                Directory.CreateDirectory(_configDirectory);
            using (var file = new StreamWriter(File.Create(_configFilePath)))
            {
                file.Write(json);
            }
            return;
        }
#endif
    }
}
