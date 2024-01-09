using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaveScammer
{

    public class SettingsCarrier
    {
        public string SettingsPath { get; set; }
        public string StoragePath { get; set; }

        public int DefaultInterval { get; set; }
        public int MaxAutoSaveCount { get; set; }
        public int MaxAutoSaveTogetherSize { get; set; }


        public SettingsCarrier()
        {
            SettingsPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "savescammer");
            StoragePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "savescammer", "default_storage");
        }
    }
    public class Settings
    {
        public static string SettingsStoragePath
        {
            get => instance.SettingsPath; 
            set
            {
                instance.SettingsPath = value;
                UpdateStoragePaths();
            }
        }

        public static string StoragePath
        {
            get => instance.StoragePath; 
            set
            {
                instance.StoragePath = value;
                UpdateStoragePaths();
            }
        }

        public static int DefaultInterval
        {
            get => instance.DefaultInterval; 
            set
            {
                instance.DefaultInterval = value;
            }
        }

        public static string ProfilesStoragePath { get; private set; }


        private static SettingsCarrier instance;

        static Settings()
        {
            Load();
            UpdateStoragePaths();
        }

        static void UpdateStoragePaths()
        {
            ProfilesStoragePath = Path.Combine(SettingsStoragePath, "profiles");
        }

        public static void Save()
        {
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = Path.Join(appdataPath, "savescammer");
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
            }
            settingsPath = Path.Join(settingsPath, "options.json");
            using (StreamWriter sw = new StreamWriter(settingsPath, false, encoding: Encoding.UTF8))
            {
                sw.Write(ToJson());
            }
        }
        public static SettingsCarrier Load()
        {
            var appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = Path.Join(appdataPath, "savescammer", "options.json");
            if (!Path.Exists(settingsPath))
            {
                instance = new SettingsCarrier();
            }
            else
            {
                instance = FromJson(settingsPath);
            }
            return instance;
        }

        public static string ToJson()
        {
            return JsonSerializer.Serialize(instance, Misc.JsonOptions);
        }

        public static SettingsCarrier FromJson(string settingsPath)
        {
            string json = "";
            using (StreamReader sr = new StreamReader(settingsPath, encoding: Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            return JsonSerializer.Deserialize<SettingsCarrier>(json) ?? new SettingsCarrier();
        }
    }
}
