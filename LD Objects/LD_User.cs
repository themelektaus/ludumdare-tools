namespace LudumDareTools;

public class LD_User : LD_Node
{
    public class Settings
    {
        public class Options
        {
            public bool preferGifs = false;
            public bool filterOnlyFavorites = false;
            public bool filterJam = true;
            public bool filterCompo = true;
            public bool filterRated = true;
            public bool filterUnrated = true;
            public string orderCategory = "smart";
        }

        public List<long> favoriteGameIds = new();
        public Options options = new();
    }

    [JsonIgnore]
    public Settings settings;

    public void LoadSettings()
    {
        settings = new();
        var jsonFile = Path.Combine("data", "usersettings", $"{id}.json");
        if (File.Exists(jsonFile))
        {
            var json = File.ReadAllText(jsonFile);
            var jsonSettings = Utils.GetJsonSettings();
            try { settings = Json.DeserializeObject<Settings>(json, jsonSettings); } catch { }
        }
    }

    public void SaveAndUnloadSettings()
    {
        var folder = Path.Combine("data", "usersettings");
        Directory.CreateDirectory(folder);
        var jsonFile = Path.Combine(folder, $"{id}.json");
        var jsonSettings = Utils.GetJsonSettings();
        var json = Json.SerializeObject(settings, jsonSettings);
        File.WriteAllText(jsonFile, json);
        settings = null;
    }
}