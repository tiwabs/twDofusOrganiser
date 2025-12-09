using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace twDofusOrganiser
{
    // Model for persisted configuration
    public class AppConfig
    {
        public List<string> WindowOrder { get; set; } = new List<string>();
        public Dictionary<string, bool> Enabled { get; set; } = new Dictionary<string, bool>();
        public string? PreviousHotkey { get; set; }
        public string? NextHotkey { get; set; }
    }

    // Static helper for load/save
    public static class ConfigManager
    {
        private static string ConfigPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                    return new AppConfig();

                string json = File.ReadAllText(ConfigPath);
                var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                return cfg ?? new AppConfig();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading config: " + ex.Message);
                return new AppConfig();
            }
        }

        public static void Save(AppConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving config: " + ex.Message);
            }
        }
    }
}
