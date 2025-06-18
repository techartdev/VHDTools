using System.IO;
using System.Text.Json;

namespace VHDTools.App.Services
{
    public class AppSettings
    {
        public string? LastVhdPath { get; set; }
    }

    public class SettingsService : ISettingsService
    {
        private const string FileName = "appsettings.json";

        public AppSettings Settings { get; private set; } = new AppSettings();

        public SettingsService()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    var json = File.ReadAllText(FileName);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null)
                        Settings = loaded;
                }
            }
            catch
            {
                Settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FileName, json);
            }
            catch
            {
            }
        }
    }
}
